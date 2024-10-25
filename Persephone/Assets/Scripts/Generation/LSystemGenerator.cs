using UnityEngine;
using System.Collections.Generic;
using System.Text;
using ProceduralGraphics.LSystems.Rendering;
using ProceduralGraphics.LSystems.ScriptableObjects;

namespace ProceduralGraphics.LSystems.Generation
{
    public class LSystemGenerator : MonoBehaviour
    {
        public List<Rule> Rules { get; set; }
        public string Axiom { get; set; }
        public int Iterations { get; set; }
        public float Angle { get; set; }
        public float Length { get; set; }

        private RendererBase renderer;

        public void GenerateLSystem()
        {
            Debug.Log("LSystemGenerator: GenerateLSystem called.");

            if (renderer == null)
            {
                Debug.LogError("LSystemGenerator: Renderer not set.");
                return;
            }

            string currentString = Axiom;
            for (int i = 0; i < Iterations; i++)
            {
                currentString = ApplyRules(currentString);
            }

            Debug.Log($"Generated L-System String: {currentString}");

            renderer.Render(new LSystemConfig
            {
                Name = "GeneratedConfig",
                Axiom = currentString,
                Rules = Rules,
                Angle = Angle,
                Length = Length,
                Thickness = 0.1f,  // Default thickness; adjust as needed

                LengthVariationFactor = 1.0f,  // Default length variation factor; adjust as needed
                ThicknessVariationFactor = 1.0f,  // Default thickness variation factor; adjust as needed

                CurvatureAngleMin = 5f,  // Minimum curvature angle; adjust as needed
                CurvatureAngleMax = 15f,  // Maximum curvature angle; adjust as needed
                CurvatureAngle = 10f,  // General curvature angle; adjust as needed

                LeafScaleMin = 0.8f,  // Minimum leaf scale; adjust as needed
                LeafScaleMax = 1.2f,  // Maximum leaf scale; adjust as needed
                LeafOffset = 0.05f,  // Leaf offset; adjust as needed
                LeafPlacementProbability = 1.0f,  // Probability of placing a leaf; adjust as needed

                DefaultIterations = Iterations,
                IsStochastic = false  // Set to true if you want stochastic behavior
            });

        }

        private string ApplyRules(string input)
        {
            StringBuilder nextString = new StringBuilder();

            foreach (char c in input)
            {
                bool ruleApplied = false;
                foreach (var rule in Rules)
                {
                    if (rule.Predecessor == c)
                    {
                        nextString.Append(rule.Successor);
                        ruleApplied = true;
                        break;
                    }
                }
                if (!ruleApplied)
                {
                    nextString.Append(c);
                }
            }

            // Log the resulting string after applying rules
            Debug.Log($"Applied rules: {nextString.ToString()}");
            return nextString.ToString();
        }

        public void SetRenderer(RendererBase newRenderer)
        {
            renderer = newRenderer;
            Debug.Log($"LSystemGenerator: Renderer set to {newRenderer.GetType().Name}");
        }

        public void GenerateFromNode(Vector3 nodePosition)
        {
            Debug.Log($"Generating new branches from node at position: {nodePosition}");

            string currentString = Axiom;
            if (string.IsNullOrEmpty(Axiom))
            {
                Debug.LogError("LSystemGenerator: Axiom is not set.");
                return;
            }

            Debug.Log($"Initial Axiom: {currentString}");

            if (Rules == null || Rules.Count == 0)
            {
                Debug.LogError("LSystemGenerator: No rules defined.");
                return;
            }

            for (int i = 0; i < Iterations; i++)
            {
                currentString = ApplyRules(currentString);
            }

            Debug.Log($"Generated L-System string from node: {currentString}");

            LSystemRenderer renderer = FindObjectOfType<LSystemRenderer>();

            if (renderer != null)
            {
                renderer.Render(new LSystemConfig
                {
                    Name = "GeneratedConfig",
                    Axiom = currentString,
                    Rules = Rules,
                    Angle = Angle,
                    Length = Length,
                    Thickness = 0.1f,  // Default thickness; adjust as needed

                    LengthVariationFactor = 1.0f,  // Default length variation factor; adjust as needed
                    ThicknessVariationFactor = 1.0f,  // Default thickness variation factor; adjust as needed

                    CurvatureAngleMin = 5f,  // Minimum curvature angle; adjust as needed
                    CurvatureAngleMax = 15f,  // Maximum curvature angle; adjust as needed
                    CurvatureAngle = 10f,  // General curvature angle; adjust as needed

                    LeafScaleMin = 0.8f,  // Minimum leaf scale; adjust as needed
                    LeafScaleMax = 1.2f,  // Maximum leaf scale; adjust as needed
                    LeafOffset = 0.05f,  // Leaf offset; adjust as needed
                    LeafPlacementProbability = 1.0f,  // Probability of placing a leaf; adjust as needed

                    DefaultIterations = Iterations,
                    IsStochastic = false  // Set to true if you want stochastic behavior
                });

                Debug.Log("New branches generated successfully.");
            }
            else
            {
                Debug.LogError("Renderer is not assigned or found.");
            }
        }

    }
}
