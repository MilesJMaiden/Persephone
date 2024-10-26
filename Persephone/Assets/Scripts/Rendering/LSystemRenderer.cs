using UnityEngine;
using System.Collections.Generic;
using ProceduralGraphics.LSystems.UI;
using System.Collections;
using ProceduralGraphics.LSystems.ScriptableObjects;

namespace ProceduralGraphics.LSystems.Rendering
{
    [RequireComponent(typeof(LineRenderer))]
    public class LSystemRenderer : RendererBase
    {
        [SerializeField]
        private GameObject lineRendererPrefab;

        [SerializeField]
        private GameObject nodePrefab;

        [SerializeField]
        public GameObject[] leafVariants; // Array to hold leaf variants

        [SerializeField]
        private Transform lineRenderParent;

        private List<GameObject> lineRenderers = new List<GameObject>();
        private List<GameObject> pruningNodes = new List<GameObject>();
        private List<GameObject> leaves = new List<GameObject>(); // List to track instantiated leaves

        private bool useMeshRenderer = false;
        private LSystemUIController uiController;

        private bool isStochastic = false;
        private int selectedLeafVariantIndex = 0; // Track the selected leaf variant
        private Queue<GameObject> branchPool = new Queue<GameObject>(); // Pool for reusing branches

        private void Start()
        {
            uiController = FindObjectOfType<LSystemUIController>();

            if (uiController != null)
            {
                uiController.OnUseMeshToggleChanged += HandleMeshRendererToggleChanged;
                uiController.OnLeafVariantSelected += OnLeafVariantSelected; // Subscribe to leaf variant selection event
            }
        }

        private void HandleMeshRendererToggleChanged(bool isOn)
        {
            useMeshRenderer = isOn;
            Debug.Log($"Mesh Renderer Mode Enabled: {useMeshRenderer}");

            ClearAllObjects();
            RegeneratePlant();
        }

        private void OnLeafVariantSelected(int index) // Handle leaf variant selection
        {
            selectedLeafVariantIndex = index;
            Debug.Log($"Leaf variant {index} selected.");
        }

        public override void Render(LSystemConfig config)
        {
            if (lineRendererPrefab == null || nodePrefab == null || leafVariants.Length == 0 || lineRenderParent == null)
            {
                Debug.LogError("LSystemRenderer: Prefab or Parent not assigned.");
                return;
            }

            StartCoroutine(RenderLSystemCoroutine(config));
        }

        private IEnumerator RenderLSystemCoroutine(LSystemConfig config)
        {
            ClearAllObjects();  // Clear previous objects before rendering

            Stack<BranchState> stack = new Stack<BranchState>();
            Vector3 currentPosition = Vector3.zero;
            Quaternion currentRotation = Quaternion.identity;
            float currentThickness = config.Thickness;  // Initial thickness from config
            float currentLength = config.Length;  // Initial branch length from config

            Branch mainParentBranch = null;
            GameObject currentParent = lineRenderParent.gameObject;
            GameObject currentLineRendererObject = null;
            LineRenderer currentLineRenderer = null;

            List<Vector3> positions = new List<Vector3> { currentPosition };
            Branch currentBranch = null;
            List<Branch> branches = new List<Branch>();

            int branchCount = 0;

            foreach (char command in config.Axiom)
            {
                switch (command)
                {
                    case 'F':
                        Vector3 direction = Vector3.up;
                        Vector3 nextPosition = currentPosition + currentRotation * direction * currentLength;

                        if (!NodeExistsAtPosition(nextPosition))
                        {
                            positions.Add(nextPosition);
                        }

                        if (positions.Count == 2)
                        {
                            currentLineRendererObject = GetOrCreateLineRenderer();
                            currentLineRendererObject.transform.SetParent(currentParent.transform);
                            currentLineRenderer = currentLineRendererObject.GetComponent<LineRenderer>();

                            currentLineRenderer.useWorldSpace = false;
                            currentLineRenderer.positionCount = 2;
                            currentLineRenderer.SetPosition(0, positions[0]);
                            currentLineRenderer.SetPosition(1, positions[1]);

                            currentLineRenderer.startWidth = currentThickness;
                            currentLineRenderer.endWidth = currentThickness;

                            lineRenderers.Add(currentLineRendererObject);

                            GameObject nodeInstance = Instantiate(nodePrefab, currentLineRendererObject.transform);
                            nodeInstance.transform.localPosition = currentLineRendererObject.transform.InverseTransformPoint(positions[0]);
                            //nodeInstance.transform.localPosition += new Vector3(0, 0.22f, 0);
                            pruningNodes.Add(nodeInstance);

                            NodeBehaviour nodeBehaviour = nodeInstance.GetComponent<NodeBehaviour>();
                            if (nodeBehaviour != null)
                            {
                                nodeBehaviour.Initialize(currentBranch);
                            }

                            Branch newBranch = new Branch(currentLineRendererObject, null);
                            if (currentBranch != null && currentBranch.LineRendererObject != null)
                            {
                                LineRenderer previousLineRenderer = currentBranch.LineRendererObject.GetComponent<LineRenderer>();
                                if (previousLineRenderer != null && positions[0] == previousLineRenderer.GetPosition(1))
                                {
                                    newBranch.SetParent(currentBranch);
                                }
                            }

                            branches.Add(newBranch);
                            if (mainParentBranch == null) mainParentBranch = newBranch;
                            currentBranch = newBranch;
                            currentPosition = nextPosition;
                            positions.Clear();
                            positions.Add(currentPosition);

                            // Adjust camera position periodically (every 10 branches or end of iteration)
                            branchCount++;
                            if (branchCount % 10 == 0) FocusCameraOnPlant();
                        }
                        break;

                    case 'V':
                        currentLength *= config.LengthVariationFactor;
                        break;

                    case 'L':
                        CreateLeaf(currentPosition, currentLineRendererObject, config);
                        break;

                    case 'T':
                        currentThickness *= config.ThicknessVariationFactor;
                        break;

                    case 'B':
                        float curvatureAngle = Random.Range(config.CurvatureAngleMin, config.CurvatureAngleMax);
                        currentRotation *= Quaternion.Euler(0, 0, curvatureAngle);
                        break;

                    case '+':
                        currentRotation *= Quaternion.Euler(isStochastic ? Stochastic3DAngle(config.Angle) : Deterministic3DAngle(config.Angle));
                        break;

                    case '-':
                        currentRotation *= Quaternion.Euler(isStochastic ? Stochastic3DAngle(-config.Angle) : Deterministic3DAngle(-config.Angle));
                        break;

                    case '[':
                        stack.Push(new BranchState(currentPosition, currentRotation, currentBranch));
                        break;

                    case ']':
                        if (stack.Count > 0)
                        {
                            BranchState state = stack.Pop();
                            currentPosition = state.position;
                            currentRotation = state.rotation;
                            currentBranch = state.branch;
                            positions.Clear();
                            positions.Add(currentPosition);
                        }
                        break;

                    default:
                        Debug.LogWarning($"Unknown L-System Command: {command}");
                        break;
                }

                yield return null;
            }

            Debug.Log($"Rendered {branches.Count} branches.");
            FocusCameraOnPlant(); // Final focus adjustment
        }

        private GameObject GetOrCreateLineRenderer()
        {
            // Check the pool first for available branches
            if (branchPool.Count > 0)
            {
                var pooledLineRenderer = branchPool.Dequeue();
                pooledLineRenderer.SetActive(true);
                return pooledLineRenderer;
            }
            // If no pooled object is available, instantiate a new one
            return Instantiate(lineRendererPrefab);
        }

        private void CreateLeaf(Vector3 currentPosition, GameObject currentLineRendererObject, LSystemConfig config)
        {
            if (currentLineRendererObject != null) // Ensure currentLineRendererObject still exists
            {
                GameObject leafInstance = Instantiate(leafVariants[selectedLeafVariantIndex], currentLineRendererObject.transform);

                leafInstance.transform.localPosition = currentLineRendererObject.transform.InverseTransformPoint(currentPosition);
                //leafInstance.transform.localPosition += new Vector3(0, 0.22f, 0);

                leafInstance.transform.localRotation = Quaternion.Euler(
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f)
                );

                float randomScaleFactor = Random.Range(config.LeafScaleMin, config.LeafScaleMax);
                leafInstance.transform.localScale = new Vector3(randomScaleFactor, randomScaleFactor, randomScaleFactor);

                leafInstance.transform.localPosition += new Vector3(
                    Random.Range(-config.LeafOffset, config.LeafOffset),
                    Random.Range(-config.LeafOffset, config.LeafOffset),
                    Random.Range(-config.LeafOffset, config.LeafOffset)
                );

                leaves.Add(leafInstance);
            }
        }



        public void ClearAllObjects()
        {
            foreach (var lineRenderer in lineRenderers)
            {
                if (lineRenderer != null)
                {
                    Destroy(lineRenderer);
                }
            }
            foreach (var node in pruningNodes)
            {
                if (node != null)
                {
                    Destroy(node);
                }
            }
            foreach (var leaf in leaves)
            {
                if (leaf != null)
                {
                    Destroy(leaf);
                }
            }
            lineRenderers.Clear();
            pruningNodes.Clear();
            leaves.Clear();

            Debug.Log("Cleared all line renderers, nodes, and leaves.");
        }

        private void RegeneratePlant()
        {
            if (uiController != null)
            {
                uiController.RegeneratePlant();
            }
        }

        public void SetStochasticMode(bool isStochastic)
        {
            this.isStochastic = isStochastic;
            Debug.Log($"L-System Renderer set to {(isStochastic ? "Stochastic" : "Deterministic")} mode.");
        }

        private void FocusCameraOnPlant()
        {
            Bounds plantBounds = CalculateBounds();
            Vector3 plantCenter = plantBounds.center;

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main Camera not found.");
                return;
            }

            float maxDimension = Mathf.Max(plantBounds.size.x, plantBounds.size.y, plantBounds.size.z);
            float cameraDistance = maxDimension * 1.5f;

            mainCamera.transform.position = plantCenter - mainCamera.transform.forward * cameraDistance;
            mainCamera.transform.LookAt(plantCenter);

            Debug.Log($"Camera focused on plant at center: {plantCenter}, bounds size: {plantBounds.size}");
        }

        private Bounds CalculateBounds()
        {
            if (lineRenderers.Count == 0)
            {
                return new Bounds(lineRenderParent.position, Vector3.zero);
            }

            Bounds bounds = new Bounds(lineRenderers[0].transform.position, Vector3.zero);

            foreach (var lineRenderer in lineRenderers)
            {
                if (lineRenderer != null)
                {
                    LineRenderer lr = lineRenderer.GetComponent<LineRenderer>();
                    if (lr != null && lr.positionCount > 0)
                    {
                        for (int i = 0; i < lr.positionCount; i++)
                        {
                            bounds.Encapsulate(lr.GetPosition(i));
                        }
                    }
                }
            }

            return bounds;
        }

        public void RotateRenderer(float rotationValue)
        {
            // Apply the rotation to the parent transform or another relevant object
            transform.rotation = Quaternion.Euler(0, rotationValue, 0);  // Rotating around the Y-axis
            Debug.Log($"L-System Renderer rotated to {rotationValue} degrees.");
        }

        private Vector3 Stochastic3DAngle(float angle)
        {
            return new Vector3(Random.Range(-angle, angle), Random.Range(-angle, angle), Random.Range(-angle, angle));
        }

        private Vector3 Deterministic3DAngle(float angle)
        {
            return new Vector3(angle, angle, angle);
        }

        public struct BranchState
        {
            public Vector3 position;
            public Quaternion rotation;
            public Branch branch;

            public BranchState(Vector3 pos, Quaternion rot, Branch currentBranch)
            {
                position = pos;
                rotation = rot;
                branch = currentBranch;
            }
        }

        private bool NodeExistsAtPosition(Vector3 position)
        {
            foreach (var node in pruningNodes)
            {
                if (node != null && Vector3.Distance(node.transform.position, position) < 0.01f)
                {
                    return true;
                }
            }
            return false;
        }

        public void SetAllNodesActive(bool isActive)
        {
            foreach (var node in pruningNodes)
            {
                if (node != null)
                {
                    node.SetActive(isActive);
                }
            }
            Debug.Log($"All nodes set to active: {isActive}");
        }

        public void TraverseBranches(GameObject parentBranch)
        {
            Debug.Log($"Traversing branch: {parentBranch.name}");

            foreach (Transform child in parentBranch.transform)
            {
                if (child.TryGetComponent(out LineRenderer childLineRenderer))
                {
                    TraverseBranches(child.gameObject);
                }
            }
        }
    }
}
