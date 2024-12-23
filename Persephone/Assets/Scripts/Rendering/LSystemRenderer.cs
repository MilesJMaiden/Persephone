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
        public event System.Action OnRenderComplete;
        private bool isGenerating = false;

        [SerializeField]
        private GameObject lineRendererPrefab;

        [SerializeField]
        private GameObject nodePrefab;

        [SerializeField]
        public GameObject[] leafVariants;

        [SerializeField]
        public GameObject[] flowerVariants;

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

        [SerializeField]
        private GameObject flowerPrefab; // Flower prefab to spawn

        private List<GameObject> flowers = new List<GameObject>();

        private WindManager windManager;

        public bool IsRenderingComplete { get; private set; }

        private void Start()
        {
            uiController = FindObjectOfType<LSystemUIController>();
            windManager = FindObjectOfType<WindManager>();

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
            if (isGenerating)
            {
                Debug.LogWarning("Render already in progress. Ignoring duplicate call.");
                return;
            }
            if (lineRendererPrefab == null || nodePrefab == null || leafVariants.Length == 0 || lineRenderParent == null)
            {
                Debug.LogError("LSystemRenderer: Prefab or Parent not assigned.");
                return;
            }

            Debug.Log("Starting L-System generation...");
            isGenerating = true;  // Set flag to true to indicate generation in progress
            StartCoroutine(RenderLSystemCoroutine(config));
        }

        private IEnumerator RenderLSystemCoroutine(LSystemConfig config)
        {
            ClearAllObjects();  // Clear previous objects before rendering
            IsRenderingComplete = false;
            isGenerating = true;
            Debug.Log("LSystem generation coroutine started.");  // Log at start

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
                            pruningNodes.Add(nodeInstance);

                            NodeBehaviour nodeBehaviour = nodeInstance.GetComponent<NodeBehaviour>();
                            if (nodeBehaviour != null)
                            {
                                nodeBehaviour.Initialize(currentBranch);
                            }

                            float branchLength = Vector3.Distance(positions[0], positions[1]); // Calculate branch length
                            Branch newBranch = new Branch(currentLineRendererObject, null, branchLength);
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

                            branchCount++;
                            if (branchCount % 10 == 0) FocusCameraOnPlant();

                            // Register branch to WindManager
                            if (windManager != null)
                            {
                                windManager.RegisterBranch(newBranch);
                            }
                        }
                        break;

                    case 'W': // Apply wind effect
                        if (currentBranch != null && windManager != null)
                        {
                            windManager.RegisterBranch(currentBranch);
                        }
                        break;

                    case 'V':
                        currentLength *= config.LengthVariationFactor;
                        break;

                    case 'L':
                        CreateLeaf(currentPosition, currentLineRendererObject, config);
                        break;

                    case 'X':
                        CreateFlower(currentPosition, currentLineRendererObject, config);
                        break;

                    case 'T':
                        currentThickness *= config.ThicknessVariationFactor;
                        break;

                    case 'B':
                        float curvatureAngle = isStochastic
                            ? Random.Range(config.CurvatureAngleMin, config.CurvatureAngleMax)
                            : config.CurvatureAngleMin; // Fixed curvature for deterministic mode
                        currentRotation *= Quaternion.Euler(0, 0, curvatureAngle);
                        break;

                    case '+':
                        currentRotation *= Quaternion.Euler(isStochastic
                            ? Stochastic3DAngle(config.Angle)
                            : new Vector3(0, 0, config.Angle));
                        break;

                    case '-':
                        currentRotation *= Quaternion.Euler(isStochastic
                            ? Stochastic3DAngle(-config.Angle)
                            : new Vector3(0, 0, -config.Angle));
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

                yield return null;  // Yield per iteration to avoid blocking
            }

            Debug.Log("All branches generated. Completing L-System generation.");
            FocusCameraOnPlant(); // Final focus adjustment

            // Ensure rendering complete is set after full coroutine finishes
            IsRenderingComplete = true;
            Debug.Log("LSystem generation has been completed.");  // Log completion

            // Signal that rendering is complete
            OnRenderComplete?.Invoke();

            // Reset the generation flag to allow new generations
            isGenerating = false;  // Reset the flag here to allow for future calls
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
                // Apply both LeafPlacementProbability and LeafDensity
                if (Random.value <= config.LeafPlacementProbability * config.LeafDensity)
                {
                    GameObject leafInstance = Instantiate(leafVariants[selectedLeafVariantIndex], currentLineRendererObject.transform);

                    leafInstance.transform.localPosition = currentLineRendererObject.transform.InverseTransformPoint(currentPosition);

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

                    // Set material color
                    if (config.LeafMaterial != null && leafInstance.TryGetComponent(out Renderer renderer))
                    {
                        renderer.material = config.LeafMaterial;
                        renderer.material.color = config.LeafColor; // Set the base color
                    }

                    leaves.Add(leafInstance);
                }
            }
        }

        private void CreateFlower(Vector3 currentPosition, GameObject currentLineRendererObject, LSystemConfig config)
        {
            if (currentLineRendererObject != null && config != null) // Ensure branch and config exist
            {
                // Ensure valid flower variant index
                int variantIndex = Mathf.Clamp(config.SelectedFlowerVariantIndex, 0, flowerVariants.Length - 1);
                GameObject selectedFlowerVariant = flowerVariants[variantIndex];

                if (selectedFlowerVariant == null)
                {
                    Debug.LogWarning($"Flower variant at index {variantIndex} is null. Skipping flower creation.");
                    return;
                }

                // Instantiate flower
                GameObject flowerInstance = Instantiate(selectedFlowerVariant, currentLineRendererObject.transform);
                flowerInstance.transform.localPosition = currentLineRendererObject.transform.InverseTransformPoint(currentPosition);

                // Randomize rotation
                flowerInstance.transform.localRotation = Quaternion.Euler(
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f)
                );

                // Apply random scaling
                float randomScaleFactor = Random.Range(config.FlowerScaleMin, config.FlowerScaleMax);
                flowerInstance.transform.localScale = new Vector3(randomScaleFactor, randomScaleFactor, randomScaleFactor);

                // Apply offset for more natural placement
                flowerInstance.transform.localPosition += new Vector3(
                    Random.Range(-config.FlowerOffset, config.FlowerOffset),
                    Random.Range(-config.FlowerOffset, config.FlowerOffset),
                    Random.Range(-config.FlowerOffset, config.FlowerOffset)
                );

                // Set material and color
                if (config.FlowerMaterial != null && flowerInstance.TryGetComponent(out Renderer renderer))
                {
                    renderer.material = config.FlowerMaterial; // Set the material
                    renderer.material.color = config.FlowerColor; // Set the base color
                }
                else
                {
                    Debug.LogWarning("No valid FlowerMaterial found or Renderer component missing on flower instance.");
                }

                flowers.Add(flowerInstance); // Track instantiated flowers
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
            foreach (var flower in flowers)
            {
                if (flower != null)
                {
                    Destroy(flower);
                }
            }
            lineRenderers.Clear();
            pruningNodes.Clear();
            leaves.Clear();
            flowers.Clear();

            Debug.Log("Cleared all line renderers, nodes, leaves, and flowers.");
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

        public void SetLeavesActive(bool isActive)
        {
            foreach (var leaf in leaves)
            {
                if (leaf != null)
                {
                    leaf.SetActive(isActive);
                }
            }
            Debug.Log($"All leaves set to active: {isActive}");
        }

        public void SetFlowersActive(bool isActive)
        {
            foreach (var flower in flowers)
            {
                if (flower != null)
                {
                    flower.SetActive(isActive);
                }
            }
            Debug.Log($"All flowers set to active: {isActive}");
        }

    }
}
