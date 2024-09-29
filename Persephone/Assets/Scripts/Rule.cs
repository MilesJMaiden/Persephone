using Unity.Collections;
using UnityEngine;

/// <summary>
/// Represents a production rule in an L-System.
/// </summary>
[System.Serializable]
public struct Rule
{
    #region Fields

    /// <summary>
    /// The character that is replaced by the rule.
    /// </summary>
    public char Predecessor;

    /// <summary>
    /// The replacement string for the Predecessor character, stored as a fixed-size string for use in jobs.
    /// </summary>
    public FixedString128Bytes SuccessorFixed;

    #endregion

    #region Properties

    /// <summary>
    /// The replacement string for the Predecessor character.
    /// This property is used for Unity's serialization in the Inspector.
    /// </summary>
    [HideInInspector]
    public string Successor
    {
        get => SuccessorFixed.ToString();
        set => SuccessorFixed = new FixedString128Bytes(value);
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Rule"/> struct.
    /// </summary>
    /// <param name="predecessor">The character to be replaced.</param>
    /// <param name="successor">The replacement string.</param>
    public Rule(char predecessor, string successor)
    {
        Predecessor = predecessor;
        SuccessorFixed = new FixedString128Bytes(successor);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rule"/> struct for use in jobs.
    /// </summary>
    /// <param name="predecessor">The character to be replaced.</param>
    /// <param name="successorFixed">The replacement string as a FixedString128Bytes.</param>
    public Rule(char predecessor, FixedString128Bytes successorFixed)
    {
        Predecessor = predecessor;
        SuccessorFixed = successorFixed;
    }

    #endregion
}