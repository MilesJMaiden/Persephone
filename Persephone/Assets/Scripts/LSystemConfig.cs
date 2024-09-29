// Assets/Scripts/LSystemConfig.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Configuration data for an L-System, defining its axiom, production rules, angle, and branch length.
/// </summary>
[CreateAssetMenu(fileName = "LSystemConfig", menuName = "L-System/Config")]
public class LSystemConfig : ScriptableObject
{
    #region Fields

    /// <summary>
    /// The name of the L-System configuration.
    /// </summary>
    public string Name;

    /// <summary>
    /// The initial axiom (starting string) of the L-System.
    /// </summary>
    public string Axiom;

    /// <summary>
    /// The list of production rules for the L-System.
    /// </summary>
    public List<Rule> Rules;

    /// <summary>
    /// The angle (in degrees) used for branching and turning.
    /// </summary>
    public float Angle;

    /// <summary>
    /// The length of each branch segment.
    /// </summary>
    public float Length;

    #endregion
}