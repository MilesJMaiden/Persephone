using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using System;

namespace ProceduralGraphics.LSystems.Generation
{
    /// <summary>
    /// Job responsible for generating the L-System string based on rules and iterations.
    /// </summary>
    [BurstCompile]
    public struct LSystemGenerationJob : IJob
    {
        public FixedString512Bytes axiom;
        public int iterations;

        [ReadOnly]
        public NativeArray<JobRule> rules;

        public NativeList<char> result;
        public int PruneIteration;

        public void Execute()
        {
            FixedString512Bytes currentString = axiom;
            FixedString512Bytes nextString = new FixedString512Bytes();

            for (int iter = 0; iter < iterations; iter++)
            {
                nextString.Clear();

                for (int i = 0; i < currentString.Length; i++)
                {
                    byte byteC = currentString[i];
                    char c = (char)byteC;

                    bool ruleApplied = false;

                    // Pruning condition
                    if (iter == PruneIteration && c == 'F')
                    {
                        continue; // Skip 'F' if it's the prune iteration
                    }

                    for (int j = 0; j < rules.Length; j++)
                    {
                        if (rules[j].Predecessor == c)
                        {
                            for (int k = 0; k < rules[j].SuccessorFixed.Length; k++)
                            {
                                nextString.Append(rules[j].SuccessorFixed[k]);
                            }
                            ruleApplied = true;
                            break;
                        }
                    }

                    if (!ruleApplied)
                    {
                        nextString.Append(byteC);
                    }
                }

                // Swap currentString and nextString
                FixedString512Bytes temp = currentString;
                currentString = nextString;
                nextString = temp;
            }

            // Convert FixedString512Bytes to the final result string
            for (int i = 0; i < currentString.Length; i++)
            {
                byte byteChar = currentString[i];
                char finalChar = (char)byteChar;
                result.Add(finalChar);
            }
        }
    }
}
