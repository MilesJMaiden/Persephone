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
        private GameObject lineRendererPrefab;  // Prefab for the line renderer

        [SerializeField]
        private GameObject nodePrefab;  // Prefab for the node to be instantiated at branch ends

        [SerializeField]
        private Transform lineRenderParent; // Parent object for line renderers

        private List<GameObject> lineRenderers = new List<GameObject>();  // Tracks line renderers
        private List<GameObject> pruningNodes = new List<GameObject>();  // Tracks node prefabs
        private bool is3D = false;
        private bool useMeshRenderer = false;  // Handle mesh rendering
        private LSystemUIController uiController;

        private void Start()
        {
            uiController = FindObjectOfType<LSystemUIController>();

            if (uiController != null)
            {
                uiController.On3DToggleChanged += Handle3DToggleChanged;
                uiController.OnUseMeshToggleChanged += HandleMeshRendererToggleChanged;
            }
        }

        private void Handle3DToggleChanged(bool isOn)
        {
            is3D = isOn;
            Debug.Log($"3D Mode Enabled: {is3D}");

            ClearAllObjects();
            RegeneratePlant();
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

            List<Vector3> positions = new List<Vector3> { currentPosition };

            Branch currentBranch = null;
            List<Branch> branches = new List<Branch>();

            foreach (char command in lSystemString)
            {
                switch (command)
                {
                    case 'F':  // Move forward
                        Vector3 direction = is3D ? RandomDirection3D() : Vector3.up;
                        Vector3 nextPosition = currentPosition + currentRotation * direction * length;
                        positions.Add(nextPosition);

                        if (positions.Count == 2)
                        {
                            // Create the line renderer object only if we have two valid positions
                            currentLineRendererObject = Instantiate(lineRendererPrefab, currentParent.transform);
                            currentLineRenderer = currentLineRendererObject.GetComponent<LineRenderer>();

                            // Set the positions for this LineRenderer
                            currentLineRenderer.positionCount = 2;
                            currentLineRenderer.SetPosition(0, positions[0]);
                            currentLineRenderer.SetPosition(1, positions[1]);
                            lineRenderers.Add(currentLineRendererObject);

                            // Instantiate the node prefab at the start of the line and set it as a child of the current branch
                            GameObject nodeInstance = Instantiate(nodePrefab, positions[0], Quaternion.identity, currentLineRendererObject.transform);  // Set as a child of the line renderer
                            pruningNodes.Add(nodeInstance);  // Keep track of nodes for further processing

                            // Get the NodeBehaviour component and pass the current branch to it
                            NodeBehaviour nodeBehaviour = nodeInstance.GetComponent<NodeBehaviour>();
                            if (nodeBehaviour != null)
                            {
                                nodeBehaviour.Initialize(currentBranch); // Pass the current branch reference
                            }

                            Debug.Log($"New LineRenderer created with positions: Start {positions[0]}, End {positions[1]}");

                            // Create a new branch and check if it should be a child of the previous branch
                            Branch newBranch = new Branch(currentLineRendererObject, null);

                            // Check if the start of this line matches the end of the previous branch
                            if (currentBranch != null && currentBranch.LineRendererObject != null)
                            {
                                LineRenderer previousLineRenderer = currentBranch.LineRendererObject.GetComponent<LineRenderer>();
                                if (previousLineRenderer != null && positions[0] == previousLineRenderer.GetPosition(1))
                                {
                                    // The branches are connected, so set the new branch as a child of the current branch
                                    newBranch.SetParent(currentBranch);
                                    Debug.Log($"New branch created and parented to: {currentBranch.LineRendererObject.name}");
                                }
                            }

                            branches.Add(newBranch);

                            // If this is the first valid branch, mark it as the main parent
                            if (mainParentBranch == null)
                            {
                                mainParentBranch = newBranch;
                                Debug.Log("Main parent branch set.");
                            }

                            // Reset positions and update current branch
                            currentBranch = newBranch;
                            currentPosition = nextPosition;
                            positions.Clear();
                            positions.Add(currentPosition);
                        }
                        break;

                    case '+':  // Turn right
                        currentRotation *= Quaternion.Euler(is3D ? Random3DAngle(angle) : new Vector3(0, 0, -angle));
                        break;

                    case '-':  // Turn left
                        currentRotation *= Quaternion.Euler(is3D ? Random3DAngle(-angle) : new Vector3(0, 0, angle));
                        break;

                    case '[':  // Save current state for branching
                        stack.Push(new BranchState(currentPosition, currentRotation, currentBranch));
                        Debug.Log("State saved for branching.");
                        break;

                    case ']':  // Restore state
                        if (stack.Count > 0)
                        {
                            BranchState state = stack.Pop();
                            currentPosition = state.position;
                            currentRotation = state.rotation;
                            currentBranch = state.branch;

                            Debug.Log($"State restored: Parent branch is {currentBranch?.LineRendererObject.name ?? "None"}");

                            positions.Clear();
                            positions.Add(currentPosition);
                        }
                        break;

                    default:
                        Debug.LogWarning($"Unknown L-System Command: {command}");
                        break;
                }

                yield return null;  // Yield to prevent editor freezing
            }

            Debug.Log($"Rendered {branches.Count} branches.");

            // Move the camera to focus on the plant
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

            // Adjust camera position to fully frame the plant based on its bounds
            float maxDimension = Mathf.Max(plantBounds.size.x, plantBounds.size.y, plantBounds.size.z);
            float cameraDistance = maxDimension * 1.5f; // Adjust this factor for framing

            mainCamera.transform.position = plantCenter - mainCamera.transform.forward * cameraDistance;
            mainCamera.transform.LookAt(plantCenter);

            Debug.Log($"Camera focused on plant at center: {plantCenter}, bounds size: {plantBounds.size}");
        }

        private Bounds CalculateBounds()
        {
            if (lineRenderers.Count == 0)
            {
                return new Bounds(lineRenderParent.position, Vector3.zero); // If no line renderers, return empty bounds
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

        private Vector3 RandomDirection3D()
        {
            return new Vector3(Random.Range(-1f, 1f), Random.Range(0.5f, 1f), Random.Range(-1f, 1f)).normalized;
        }

        private Vector3 Random3DAngle(float angle)
        {
            return new Vector3(Random.Range(-angle, angle), Random.Range(-angle, angle), Random.Range(-angle, angle));
        }

        private struct BranchState
        {
            public Vector3 position;
            public Quaternion rotation;
            public Branch branch;  // The branch that corresponds to this state

            public BranchState(Vector3 pos, Quaternion rot, Branch currentBranch)
            {
                position = pos;
                rotation = rot;
                branch = currentBranch;
            }
        }

        // Recursively loop through the branches starting from the root
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
