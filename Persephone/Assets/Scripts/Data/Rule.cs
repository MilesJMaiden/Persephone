// Assets/Scripts/Generation/Rule.cs
using UnityEngine;

namespace ProceduralGraphics.LSystems.Generation
{
    /// <summary>
    /// Represents a single production rule in an L-System.
    /// </summary>
    [System.Serializable]
    public struct Rule
    {
        [Tooltip("The predecessor character to be replaced.")]
        public char Predecessor;

        [Tooltip("The successor string that replaces the predecessor.")]
        public string Successor;

        public Rule(char predecessor, string successor)
        {
            Predecessor = predecessor;
            Successor = successor;
        }
    }
}
