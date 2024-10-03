// Assets/Scripts/Controllers/LSystemController.cs
using UnityEngine;
using ProceduralGraphics.LSystems.Generation;
using ProceduralGraphics.LSystems.ScriptableObjects;
using ProceduralGraphics.LSystems.UI;
using ProceduralGraphics.LSystems.Rendering;

namespace ProceduralGraphics.LSystems.Controllers
{
    /// <summary>
    /// Controls the L-System generation and rendering based on UI interactions.
    /// </summary>
    public class LSystemController : MonoBehaviour
    {
        [SerializeField]
        private LSystemUIController uiController;

        [SerializeField]
        private LSystemGenerator generator;

        // References to both renderers
        [SerializeField]
        private RendererBase lineRenderer; // Assign LSystemRenderer
        [SerializeField]
        private RendererBase meshRenderer; // Assign LSystemMeshRenderer

        private void Start()
        {
            if (uiController != null)
            {
                uiController.OnGenerateRequested += HandleGenerateRequested;
                uiController.OnRenderToggle += HandleRenderToggle;
            }
            else
            {
                Debug.LogError("LSystemController: UIController reference is not set.");
            }

            if (generator == null)
            {
                Debug.LogError("LSystemController: LSystemGenerator reference is not set.");
            }

            // Assign default renderer (LineRenderer) and set active states
            if (lineRenderer != null && meshRenderer != null)
            {
                generator.SetRenderer(lineRenderer);
                lineRenderer.gameObject.SetActive(true);
                meshRenderer.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("LSystemController: Renderer references are not set.");
            }
        }

        private void HandleGenerateRequested(LSystemConfig config)
        {
            if (config == null)
            {
                Debug.LogError("LSystemController: Received null config.");
                return;
            }

            // Assign config properties to the generator
            generator.Axiom = config.Axiom;
            generator.Rules = config.Rules;
            generator.Angle = config.Angle;
            generator.Length = config.Length;
            generator.Iterations = config.DefaultIterations;

            // Log the selected config and ensure it matches the correct values
            Debug.Log($"Generating L-System with Axiom: {config.Axiom} and Rules Count: {config.Rules.Count}");
            foreach (var rule in config.Rules)
            {
                Debug.Log($"Rule: {rule.Predecessor} -> {rule.Successor}");
            }

            // Call GenerateLSystem
            generator.GenerateLSystem();
        }


        private void HandleRenderToggle(bool useMeshRenderer)
        {
            if (generator == null)
            {
                Debug.LogError("LSystemController: LSystemGenerator reference is not set.");
                return;
            }

            if (useMeshRenderer)
            {
                // Assign MeshRenderer as the renderer
                if (meshRenderer != null && lineRenderer != null)
                {
                    generator.SetRenderer(meshRenderer);
                    meshRenderer.gameObject.SetActive(true);
                    lineRenderer.gameObject.SetActive(false);
                }
                else
                {
                    Debug.LogError("LSystemController: MeshRenderer or LineRenderer reference is not set.");
                }
            }
            else
            {
                // Assign LineRenderer as the renderer
                if (lineRenderer != null && meshRenderer != null)
                {
                    generator.SetRenderer(lineRenderer);
                    lineRenderer.gameObject.SetActive(true);
                    meshRenderer.gameObject.SetActive(false);
                }
                else
                {
                    Debug.LogError("LSystemController: LineRenderer or MeshRenderer reference is not set.");
                }
            }
        }

        private void OnDestroy()
        {
            if (uiController != null)
            {
                uiController.OnGenerateRequested -= HandleGenerateRequested;
                uiController.OnRenderToggle -= HandleRenderToggle;
            }
        }
    }
}
