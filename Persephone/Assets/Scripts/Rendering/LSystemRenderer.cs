using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProceduralGraphics.LSystems.Generation;
using ProceduralGraphics.LSystems.UI;

namespace ProceduralGraphics.LSystems.Rendering
{
    public class LSystemRenderer : RendererBase
    {
        [SerializeField]
        private GameObject lineRendererPrefab;

        [SerializeField]
        private GameObject pruningNodePrefab;

        [SerializeField]
        private Transform lineRenderParent;

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
            if (lineRendererPrefab == null || pruningNodePrefab == null || lineRenderParent == null)
            {
                Debug.LogError("LSystemRenderer: Prefab or Parent not assigned.");
                return;
            }

            StartCoroutine(RenderLSystemCoroutine(lSystemString, length, angle));
        }

        private IEnumerator RenderLSystemCoroutine(string lSystemString, float length, float angle)
        {
            ClearAllObjects();  // Clear previous objects before rendering

            Stack<TransformState> stack = new Stack<TransformState>();
            Vector3 currentPosition = Vector3.zero;
            Quaternion currentRotation = Quaternion.identity;

            GameObject currentParent = lineRenderParent.gameObject;
            GameObject currentLineRendererObject = Instantiate(lineRendererPrefab, currentParent.transform);
            LineRenderer currentLineRenderer = currentLineRendererObject.GetComponent<LineRenderer>();
            lineRenderers.Add(currentLineRendererObject);

            List<Vector3> positions = new List<Vector3> { currentPosition };

            bool branchingHappened = false;
            int counter = 0;
            foreach (char command in lSystemString)
            {
                switch (command)
                {
                    case 'F':  // Move forward
                        Vector3 direction = is3D ? RandomDirection3D() : Vector3.up;
                        Vector3 nextPosition = currentPosition + currentRotation * direction * length;
                        positions.Add(nextPosition);

                        if (positions.Count >= 2)
                        {
                            currentLineRenderer.positionCount = positions.Count;
                            currentLineRenderer.SetPositions(positions.ToArray());

                            // Create a new line renderer for the next branch
                            GameObject newLineRendererObject = Instantiate(lineRendererPrefab, currentParent.transform);
                            LineRenderer newLineRenderer = newLineRendererObject.GetComponent<LineRenderer>();
                            lineRenderers.Add(newLineRendererObject);

                            currentLineRendererObject = newLineRendererObject;
                            currentLineRenderer = newLineRenderer;
                            positions = new List<Vector3> { nextPosition };

                            currentParent = currentLineRendererObject;
                        }

                        currentPosition = nextPosition;
                        break;

                    case '+':  // Turn right
                        currentRotation *= Quaternion.Euler(is3D ? Random3DAngle(angle) : new Vector3(0, 0, -angle));
                        break;

                    case '-':  // Turn left
                        currentRotation *= Quaternion.Euler(is3D ? Random3DAngle(-angle) : new Vector3(0, 0, angle));
                        break;

                    case '[':  // Save state
                        stack.Push(new TransformState(currentPosition, currentRotation, currentParent));
                        branchingHappened = true;  // Set this to true to signal a node should be placed here
                        break;

                    case ']':  // Restore state
                        if (stack.Count > 0)
                        {
                            TransformState state = stack.Pop();
                            currentPosition = state.position;
                            currentRotation = state.rotation;
                            currentParent = state.parent;

                            positions.Add(currentPosition);
                        }
                        break;

                    default:
                        Debug.LogWarning($"Unknown L-System Command: {command}");
                        break;
                }

                counter++;
                if (counter % 100 == 0)  // Batch updates for performance
                {
                    yield return null;  // Yield to prevent editor freezing
                }
            }

            // Set final positions for the last line renderer
            if (positions.Count > 0)
            {
                currentLineRenderer.positionCount = positions.Count;
                currentLineRenderer.SetPositions(positions.ToArray());
            }

            Debug.Log($"Rendered {lineRenderers.Count} line segments.");

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
            lineRenderers.Clear();

            foreach (var pruningNode in pruningNodes)
            {
                if (pruningNode != null)
                {
                    Destroy(pruningNode);
                }
            }
            pruningNodes.Clear();

            Debug.Log("Cleared all line renderers and nodes.");
        }

        private void MarkForDestruction(Transform parent)
        {
            foreach (Transform child in parent)
            {
                MarkForDestruction(child);  // Recursively destroy children
            }

            Debug.Log($"Destroyed object: {parent.name}");
            Destroy(parent.gameObject);  // Destroy the parent object itself
        }

        private void RegeneratePlantFromNode(Vector3 startPosition)
        {
            Debug.Log($"Regenerating plant from position {startPosition}");

            // Call the LSystemGenerator to regenerate the plant from the clicked node
            FindObjectOfType<LSystemGenerator>().GenerateFromNode(startPosition);

            Debug.Log($"Generating from new node at {startPosition}");
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

            mainCamera.transform.position = plantCenter - mainCamera.transform.forward * plantBounds.size.magnitude * 1.5f;
            mainCamera.transform.LookAt(plantCenter);

            Debug.Log($"Camera focused on plant at center: {plantCenter}, bounds size: {plantBounds.size}");
        }

        private Bounds CalculateBounds()
        {
            Bounds bounds = new Bounds(lineRenderParent.position, Vector3.zero);

            foreach (var lineRenderer in lineRenderers)
            {
                if (lineRenderer != null)
                {
                    Bounds lineBounds = lineRenderer.GetComponent<LineRenderer>().bounds;
                    bounds.Encapsulate(lineBounds);
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

        public void TriggerGrowthFromNode(Vector3 position)
        {
            Debug.Log($"Regenerating from node at position {position}");
            ClearAllObjects();
            FindObjectOfType<LSystemGenerator>().GenerateFromNode(position);
        }

        private struct TransformState
        {
            public Vector3 position;
            public Quaternion rotation;
            public GameObject parent;

            public TransformState(Vector3 pos, Quaternion rot, GameObject parentObj)
            {
                position = pos;
                rotation = rot;
                parent = parentObj;
            }
        }
    }
}
