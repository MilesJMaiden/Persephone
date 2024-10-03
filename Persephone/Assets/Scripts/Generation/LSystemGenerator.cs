using UnityEngine;
using System.Collections.Generic;
using ProceduralGraphics.LSystems.Rendering;
using System.Text;

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

            renderer.Render(currentString, Length, Angle);
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

            // Apply the L-System rules starting from the pruned node.
            string currentString = Axiom;  // You may modify the axiom for node-specific rules if needed.

            for (int i = 0; i < Iterations; i++)
            {
                currentString = ApplyRules(currentString);  // Apply L-System rules to generate the string.
            }

            // Log the new generated L-System string for debugging.
            Debug.Log($"Generated L-System string from node: {currentString}");

            // Get the reference to the LSystemRenderer and pass the string, length, and angle.
            RendererBase renderer = FindObjectOfType<LSystemRenderer>();

            if (renderer != null)
            {
                // Render the L-System from the new node position
                renderer.Render(currentString, Length, Angle);
                Debug.Log("New branches generated successfully.");
            }
            else
            {
                Debug.LogError("Renderer is not assigned or found.");
            }
        }

    }
}
