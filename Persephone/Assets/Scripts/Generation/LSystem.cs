// Assets/Scripts/Generation/LSystem.cs
using UnityEngine;
using Unity.Collections;
using ProceduralGraphics.LSystems.Generation;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;

namespace ProceduralGraphics.LSystems.Generation
{
    public class LSystem : MonoBehaviour
    {
        public List<Rule> Rules;
        public string Axiom;
        public int Iterations;
        public IRenderer Renderer;

        void Start()
        {
            Generate();
        }

        void Generate()
        {
            // Convert List<Rule> to NativeArray<JobRule>
            NativeArray<JobRule> nativeRules = new NativeArray<JobRule>(Rules.Count, Allocator.TempJob);
            for (int i = 0; i < Rules.Count; i++)
            {
                nativeRules[i] = new JobRule(Rules[i].Predecessor, Rules[i].Successor);
            }

            // Prepare the result NativeList
            NativeList<char> result = new NativeList<char>(Allocator.TempJob);

            // Set up the job
            LSystemGenerationJob job = new LSystemGenerationJob
            {
                axiom = new FixedString512Bytes(Axiom),
                iterations = Iterations,
                rules = nativeRules,
                result = result,
                PruneIteration = 2 // or any appropriate value
            };

            // Schedule and complete the job
            JobHandle handle = job.Schedule();
            handle.Complete();

            // Convert the result to a string
            string finalString = new string(result.ToArray());

            // Dispose of native collections
            nativeRules.Dispose();
            result.Dispose();

            // Render the L-System
            Renderer.Render(finalString);
        }
    }
}
