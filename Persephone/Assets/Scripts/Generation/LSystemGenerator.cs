using UnityEngine;
using System.Collections.Generic;
using System.Text;
using ProceduralGraphics.LSystems.Rendering;
using ProceduralGraphics.LSystems.ScriptableObjects;
using System;

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

        public event Action OnRenderComplete;

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

            LSystemConfig config = ScriptableObject.CreateInstance<LSystemConfig>();
            config.Name = "GeneratedConfig";
            config.Axiom = currentString;
            config.Rules = Rules;
            config.Angle = Angle;
            config.Length = Length;
            config.Thickness = 0.1f;
            config.LengthVariationFactor = 1.0f;
            config.ThicknessVariationFactor = 1.0f;
            config.CurvatureAngleMin = 5f;
            config.CurvatureAngleMax = 15f;
            config.CurvatureAngle = 10f;
            config.LeafScaleMin = 0.8f;
            config.LeafScaleMax = 1.2f;
            config.LeafOffset = 0.05f;
            config.LeafPlacementProbability = 1.0f;
            config.DefaultIterations = Iterations;
            config.IsStochastic = false;

            renderer.Render(config);


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
                LSystemConfig config = ScriptableObject.CreateInstance<LSystemConfig>();
                config.Name = "GeneratedConfig";
                config.Axiom = currentString;
                config.Rules = Rules;
                config.Angle = Angle;
                config.Length = Length;
                config.Thickness = 0.1f;
                config.LengthVariationFactor = 1.0f;
                config.ThicknessVariationFactor = 1.0f;
                config.CurvatureAngleMin = 5f;
                config.CurvatureAngleMax = 15f;
                config.CurvatureAngle = 10f;
                config.LeafScaleMin = 0.8f;
                config.LeafScaleMax = 1.2f;
                config.LeafOffset = 0.05f;
                config.LeafPlacementProbability = 1.0f;
                config.DefaultIterations = Iterations;
                config.IsStochastic = false;

                renderer.Render(config);


                Debug.Log("New branches generated successfully.");
            }
            else
            {
                Debug.LogError("Renderer is not assigned or found.");
            }
        }

    }
}
