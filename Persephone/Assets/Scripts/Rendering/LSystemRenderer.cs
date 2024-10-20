using UnityEngine;
using System.Collections.Generic;
using ProceduralGraphics.LSystems.UI;
using System.Collections;

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
        private Transform lineRenderParent;

        private List<GameObject> lineRenderers = new List<GameObject>();
        private List<GameObject> pruningNodes = new List<GameObject>();

        private bool useMeshRenderer = false;
        private LSystemUIController uiController;

        private void Start()
        {
            uiController = FindObjectOfType<LSystemUIController>();

            if (uiController != null)
            {
                uiController.OnUseMeshToggleChanged += HandleMeshRendererToggleChanged;
            }
        }

        private void HandleMeshRendererToggleChanged(bool isOn)
        {
            useMeshRenderer = isOn;
            Debug.Log($"Mesh Renderer Mode Enabled: {useMeshRenderer}");

            ClearAllObjects();
            RegeneratePlant();
        }

        public override void Render(string lSystemString, float length, float angle)
        {
            if (lineRendererPrefab == null || nodePrefab == null || lineRenderParent == null)
            {
                Debug.LogError("LSystemRenderer: Prefab or Parent not assigned.");
                return;
            }

            StartCoroutine(RenderLSystemCoroutine(lSystemString, length, angle));
        }

        private IEnumerator RenderLSystemCoroutine(string lSystemString, float length, float angle)
        {
            ClearAllObjects();  // Clear previous objects before rendering

            Stack<BranchState> stack = new Stack<BranchState>();
            Vector3 currentPosition = Vector3.zero;
            Quaternion currentRotation = Quaternion.identity;

            Branch mainParentBranch = null;
            GameObject currentParent = lineRenderParent.gameObject;
            GameObject currentLineRendererObject = null;
            LineRenderer currentLineRenderer = null;

            // Initialize the positions list with the current position
            List<Vector3> positions = new List<Vector3> { currentPosition };

            Branch currentBranch = null;
            List<Branch> branches = new List<Branch>();

            foreach (char command in lSystemString)
            {
                switch (command)
                {
                    case 'F': 
                             
                        Vector3 direction = Vector3.up;

                        // Calculate the next position based on the current rotation and direction
                        Vector3 nextPosition = currentPosition + currentRotation * direction * length;

                        // Check if a node already exists at the next position
                        if (!NodeExistsAtPosition(nextPosition))
                        {
                            positions.Add(nextPosition); //add if no node exists
                        }

                        if (positions.Count == 2) 
                        {
                            currentLineRendererObject = Instantiate(lineRendererPrefab, currentParent.transform);
                            currentLineRenderer = currentLineRendererObject.GetComponent<LineRenderer>();

                            // Set the positions for this LineRenderer
                            currentLineRenderer.positionCount = 2;
                            currentLineRenderer.SetPosition(0, positions[0]);
                            currentLineRenderer.SetPosition(1, positions[1]);
                            lineRenderers.Add(currentLineRendererObject);

                            // Instantiate the node prefab at the start of the line and set it as a child of the current branch
                            GameObject nodeInstance = Instantiate(nodePrefab, positions[0], Quaternion.identity, currentLineRendererObject.transform);
                            pruningNodes.Add(nodeInstance);

                            NodeBehaviour nodeBehaviour = nodeInstance.GetComponent<NodeBehaviour>();
                            if (nodeBehaviour != null)
                            {
                                nodeBehaviour.Initialize(currentBranch);
                            }

                            Branch newBranch = new Branch(currentLineRendererObject, null);

                            // Connect the new branch to its parent if applicable
                            if (currentBranch != null && currentBranch.LineRendererObject != null)
                            {
                                LineRenderer previousLineRenderer = currentBranch.LineRendererObject.GetComponent<LineRenderer>();
                                if (previousLineRenderer != null && positions[0] == previousLineRenderer.GetPosition(1))
                                {
                                    newBranch.SetParent(currentBranch);
                                }
                            }

                            branches.Add(newBranch);

                            // Set main branch if it hasn't been set yet
                            if (mainParentBranch == null)
                            {
                                mainParentBranch = newBranch;
                            }

                            // Update currentBranch and currentPosition
                            currentBranch = newBranch;
                            currentPosition = nextPosition;
                            positions.Clear();
                            positions.Add(currentPosition);
                        }
                        break;

                    case '+':
                        currentRotation *= Quaternion.Euler(Random3DAngle(angle));
                        break;

                    case '-':
                        currentRotation *= Quaternion.Euler(Random3DAngle(-angle));
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
                        Debug.LogWarning($"Unknown L-System Command: {command}"); // Handle unknown commands
                        break;
                }

                yield return null;
            }

            Debug.Log($"Rendered {branches.Count} branches.");
            FocusCameraOnPlant();
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
            lineRenderers.Clear();
            pruningNodes.Clear();

            Debug.Log("Cleared all line renderers and nodes.");
        }

        private void RegeneratePlant()
        {
            if (uiController != null)
            {
                uiController.RegeneratePlant();
            }
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

        private Vector3 Random3DAngle(float angle)
        {
            return new Vector3(Random.Range(-angle, angle), Random.Range(-angle, angle), Random.Range(-angle, angle));
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