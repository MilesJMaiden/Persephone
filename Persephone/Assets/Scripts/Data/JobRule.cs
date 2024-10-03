// Assets/Scripts/Generation/JobRule.cs
using Unity.Collections;
using System;

namespace ProceduralGraphics.LSystems.Generation
{
    /// <summary>
    /// Represents a production rule optimized for use within Unity's Job System.
    /// </summary>
    [Serializable]
    public struct JobRule
    {
        public char Predecessor;
        public FixedString128Bytes SuccessorFixed;

        public JobRule(char predecessor, string successor)
        {
            Predecessor = predecessor;
            SuccessorFixed = new FixedString128Bytes(successor);
        }
    }
}
