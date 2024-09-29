// Assets/Scripts/LSystemGenerator.cs
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// Generates an L-System string using a Burst-compiled job and delegates rendering to the LSystemRenderer.
/// </summary>
public class LSystemGenerator : MonoBehaviour
{
    #region Fields

    /// <summary>
    /// The list of production rules for the L-System.
    /// </summary>
    public List<Rule> rules;

    /// <summary>
    /// The initial axiom of the L-System.
    /// </summary>
    public string axiom;

    /// <summary>
    /// The number of iterations to apply the production rules.
    /// </summary>
    public int iterations;

    /// <summary>
    /// Reference to the LSystemRenderer component responsible for rendering the generated L-System.
    /// </summary>
    public LSystemRenderer renderer;

    #endregion

    #region Properties

    /// <summary>
    /// The angle (in degrees) used for branching and turning in the renderer.
    /// </summary>
    public float Angle { get; set; }

    /// <summary>
    /// The length of each branch segment in the renderer.
    /// </summary>
    public float Length { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Generates the L-System string using the provided axiom, rules, and iterations, then renders it.
    /// </summary>
    public void GenerateLSystem()
    {
        if (renderer == null)
        {
            Debug.LogError("LSystemRenderer reference is not set.");
            return;
        }

        if (rules == null || rules.Count == 0)
        {
            Debug.LogError("No rules defined for the L-System.");
            return;
        }

        // Convert rules to NativeArray
        NativeArray<Rule> nativeRules = new NativeArray<Rule>(rules.Count, Allocator.TempJob);
        for (int i = 0; i < rules.Count; i++)
        {
            nativeRules[i] = new Rule(
                rules[i].Predecessor,
                new FixedString128Bytes(rules[i].Successor)
            );
        }

        // Prepare the result NativeList
        NativeList<char> result = new NativeList<char>(Allocator.TempJob);

        // Set up the job
        LSystemGenerationJob job = new LSystemGenerationJob
        {
            axiom = new FixedString512Bytes(axiom),
            iterations = iterations,
            rules = nativeRules,
            result = result
        };

        // Schedule and complete the job
        JobHandle handle = job.Schedule();
        handle.Complete();

        // Convert the result to a string
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < result.Length; i++)
        {
            sb.Append(result[i]);
        }
        string finalString = sb.ToString();

        // Dispose of native collections
        nativeRules.Dispose();
        result.Dispose();

        // Update renderer properties
        renderer.Angle = Angle;
        renderer.Length = Length;

        // Render the L-System
        renderer.Render(finalString);
    }

    #endregion
}