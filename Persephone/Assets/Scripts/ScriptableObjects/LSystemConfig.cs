using System.Collections.Generic;
using UnityEngine;
using ProceduralGraphics.LSystems.Generation;

namespace ProceduralGraphics.LSystems.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewLSystemConfig", menuName = "L-System/Config", order = 1)]
    public class LSystemConfig : ScriptableObject
    {
        public string Name;
        [TextArea] public string Axiom;
        public List<Rule> Rules;
        public float Angle;
        public float Length;
        public float Thickness = 0.1f;

        [Range(0.5f, 1.5f)] public float LengthVariationFactor = 1.0f;
        [Range(0.5f, 1.5f)] public float ThicknessVariationFactor = 1.0f;

        [Range(0f, 45f)] public float CurvatureAngleMin = 5f;
        [Range(0f, 45f)] public float CurvatureAngleMax = 15f;
        public float CurvatureAngle = 10f;

        // Leaf Settings
        [Range(0.5f, 2.0f)] public float LeafScaleMin = 0.8f;
        public float LeafScaleMax = 1.2f;
        [Range(0f, 0.1f)] public float LeafOffset = 0.05f;
        [Range(0f, 1f)] public float LeafPlacementProbability = 1.0f;
        [Range(0.1f, 2.0f)] public float LeafDensity = 1.0f;
        public Color LeafColor = Color.green;
        public Material LeafMaterial;

        // Flower Settings
        [Range(0.5f, 2.0f)] public float FlowerScaleMin = 2f;
        public float FlowerScaleMax = 3.5f;
        [Range(0f, 1f)] public float FlowerPlacementProbability = 0.5f;
        [Range(0f, 0.1f)] public float FlowerOffset = 0.02f;
        public Color FlowerColor = Color.red;
        public Material FlowerMaterial;

        public int SelectedFlowerVariantIndex = 0;

        public int DefaultIterations;
        public bool IsStochastic;

        public void ResetToDefaults()
        {
            LengthVariationFactor = 1.0f;
            ThicknessVariationFactor = 1.0f;
            CurvatureAngleMin = 5f;
            CurvatureAngleMax = 15f;
            CurvatureAngle = 10f;

            LeafScaleMin = 0.8f;
            LeafScaleMax = 1.2f;
            LeafOffset = 0.05f;
            LeafPlacementProbability = 1.0f;
            LeafDensity = 1.0f;


            FlowerScaleMin = 2f;
            FlowerScaleMax = 3.5f;
            FlowerPlacementProbability = 0.5f;
            FlowerOffset = 0.02f;

            DefaultIterations = 5;
            IsStochastic = false;
        }
    }
}
