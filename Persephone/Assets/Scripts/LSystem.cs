// Assets/Scripts/LSystem.cs
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;

/// <summary>
/// Represents an L-System with its axiom, production rules, and iteration count.
/// Provides functionality to generate the resulting string based on these parameters.
/// </summary>
public class LSystem
{
    #region Fields

    /// <summary>
    /// The initial axiom (starting string) of the L-System.
    /// </summary>
    public string Axiom { get; set; }

    /// <summary>
    /// The list of production rules that define how each symbol is replaced.
    /// </summary>
    public List<Rule> Rules { get; set; }

    /// <summary>
    /// The number of iterations to apply the production rules.
    /// </summary>
    public int Iterations { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="LSystem"/> class with specified axiom, rules, and iterations.
    /// </summary>
    /// <param name="axiom">The initial axiom of the L-System.</param>
    /// <param name="rules">The production rules for the L-System.</param>
    /// <param name="iterations">The number of iterations to apply the production rules.</param>
    public LSystem(string axiom, List<Rule> rules, int iterations)
    {
        Axiom = axiom;
        Rules = rules;
        Iterations = iterations;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Generates the L-System string by applying production rules for the specified number of iterations.
    /// This method uses traditional string manipulation and is suitable for smaller iteration counts.
    /// </summary>
    /// <returns>The generated L-System string.</returns>
    public string Generate()
    {
        StringBuilder currentString = new StringBuilder(Axiom);

        for (int i = 0; i < Iterations; i++)
        {
            StringBuilder nextString = new StringBuilder();

            foreach (char c in currentString.ToString())
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

            currentString = nextString;
        }

        return currentString.ToString();
    }

    /// <summary>
    /// Generates the L-System string using Unity's Job System and Burst Compiler for optimized performance.
    /// This method is suitable for larger iteration counts and complex rule sets.
    /// </summary>
    /// <returns>The generated L-System string.</returns>
    public string GenerateOptimized()
    {
        // Convert rules to NativeArray
        NativeArray<Rule> nativeRules = new NativeArray<Rule>(Rules.Count, Allocator.TempJob);
        for (int i = 0; i < Rules.Count; i++)
        {
            nativeRules[i] = new Rule(
                Rules[i].Predecessor,
                new FixedString128Bytes(Rules[i].Successor)
            );
        }

        // Prepare the result NativeList
        NativeList<char> result = new NativeList<char>(Allocator.TempJob);

        // Set up the job
        LSystemGenerationJob job = new LSystemGenerationJob
        {
            axiom = new FixedString512Bytes(Axiom),
            iterations = Iterations,
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

        return finalString;
    }

    #endregion
}