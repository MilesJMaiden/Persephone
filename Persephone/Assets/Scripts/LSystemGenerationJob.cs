using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

/// <summary>
/// A Burst-compiled job that generates an L-System string based on the provided axiom, rules, and iterations.
/// </summary>
[BurstCompile]
public struct LSystemGenerationJob : IJob
{
    #region Fields

    /// <summary>
    /// The initial axiom of the L-System.
    /// </summary>
    public FixedString512Bytes axiom;

    /// <summary>
    /// The array of production rules for the L-System.
    /// </summary>
    public NativeArray<Rule> rules;

    /// <summary>
    /// The number of iterations to apply the production rules.
    /// </summary>
    public int iterations;

    /// <summary>
    /// The resulting string after applying the production rules.
    /// </summary>
    public NativeList<char> result;

    #endregion

    #region Methods

    /// <summary>
    /// Executes the L-System generation logic.
    /// </summary>
    public void Execute()
    {
        var currentString = new NativeList<char>(Allocator.Temp);
        var nextString = new NativeList<char>(Allocator.Temp);

        // Initialize currentString with the axiom
        for (int i = 0; i < axiom.Length; i++)
        {
            currentString.Add((char)axiom[i]); // Cast byte to char
        }

        // Apply production rules for the specified number of iterations
        for (int iter = 0; iter < iterations; iter++)
        {
            nextString.Clear();

            for (int i = 0; i < currentString.Length; i++)
            {
                char c = currentString[i];
                bool ruleApplied = false;

                for (int j = 0; j < rules.Length; j++)
                {
                    if (rules[j].Predecessor == c)
                    {
                        FixedString128Bytes successor = rules[j].SuccessorFixed;
                        for (int k = 0; k < successor.Length; k++)
                        {
                            nextString.Add((char)successor[k]); // Cast byte to char
                        }
                        ruleApplied = true;
                        break;
                    }
                }

                if (!ruleApplied)
                {
                    nextString.Add(c);
                }
            }

            // Swap currentString and nextString
            var temp = currentString;
            currentString = nextString;
            nextString = temp;
        }

        // Copy the generated string to the result
        result.Clear();
        for (int i = 0; i < currentString.Length; i++)
        {
            result.Add(currentString[i]);
        }

        // Dispose of temporary NativeLists
        currentString.Dispose();
        nextString.Dispose();
    }

    #endregion
}