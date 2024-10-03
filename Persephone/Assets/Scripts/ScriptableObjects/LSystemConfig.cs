// Assets/Scripts/ScriptableObjects/LSystemConfig.cs
using System.Collections.Generic;
using UnityEngine;
using ProceduralGraphics.LSystems.Generation; // Ensure correct namespace

namespace ProceduralGraphics.LSystems.ScriptableObjects
{
    /// <summary>
    /// Defines the configuration for an L-System, including the axiom, rules, angle, length, and pruning iteration.
    /// </summary>
    [CreateAssetMenu(fileName = "NewLSystemConfig", menuName = "L-System/Config", order = 1)]
    public class LSystemConfig : ScriptableObject
    {
        /// <summary>
        /// The name of the L-System configuration.
        /// </summary>
        public string Name;

        /// <summary>
        /// The initial axiom of the L-System.
        /// </summary>
        [TextArea]
        public string Axiom;

        /// <summary>
        /// The list of production rules for the L-System.
        /// </summary>
        public List<Rule> Rules;

        /// <summary>
        /// The turning angle (in degrees) used in the L-System.
        /// </summary>
        public float Angle;

        /// <summary>
        /// The length of each branch segment in the L-System.
        /// </summary>
        public float Length;

        /// <summary>
        /// The default number of iterations to apply the production rules.
        /// </summary>
        public int DefaultIterations;

        /// <summary>
        /// The iteration at which pruning occurs.
        /// </summary>
        public int PruneIteration;
    }
}
