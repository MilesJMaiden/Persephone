using UnityEngine;
using System.Collections.Generic;
using System.Text;
using ProceduralGraphics.LSystems.Rendering;

namespace ProceduralGraphics.LSystems.Generation
{
    public class LSystemGenerator : MonoBehaviour
    {
        public List<Rule> Rules { get; set; }
        public string Axiom { get; set; }
        public int Iterations { get; set; }
        public float Angle { get; set; }
        public float Length { get; set; }
        public float RandomOffset { get; set; } // Add this line to define the random offset
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

            renderer.Render(currentString, Length, Angle, RandomOffset); // Pass the random offset
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
                // Ensure you have a random offset value available
                float randomOffset = 0; // Set this from your UI or wherever you manage it
                renderer.Render(currentString, Length, Angle, randomOffset);
                Debug.Log("New branches generated successfully.");
            }
            else
            {
                Debug.LogError("Renderer is not assigned or found.");
            }
        }

    }
}
