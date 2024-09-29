// Assets/Scripts/LSystemRenderer.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Renders the L-System string using a LineRenderer component.
/// </summary>
public class LSystemRenderer : MonoBehaviour
{
    #region Fields

    /// <summary>
    /// The angle (in degrees) used for turning in the L-System.
    /// </summary>
    public float Angle { get; set; }

    /// <summary>
    /// The length of each branch segment in the L-System.
    /// </summary>
    public float Length { get; set; }

    private LineRenderer lineRenderer;
    private List<Vector3> positions;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Initializes the LineRenderer component and its settings.
    /// </summary>
    void Awake()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        positions = new List<Vector3>();

        // Configure LineRenderer properties
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = 0.1f;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false;

        // Optionally, set color or other properties here
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Renders the L-System based on the provided instruction string.
    /// </summary>
    /// <param name="instructions">The L-System instruction string.</param>
    public void Render(string instructions)
    {
        if (string.IsNullOrEmpty(instructions))
        {
            Debug.LogWarning("No instructions provided for rendering.");
            return;
        }

        Stack<TransformInfo> transformStack = new Stack<TransformInfo>();
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;

        positions.Clear();
        positions.Add(position);

        foreach (char c in instructions)
        {
            switch (c)
            {
                case 'F':
                    Vector3 direction = rotation * Vector3.up;
                    Vector3 newPosition = position + direction * Length;
                    positions.Add(newPosition);
                    position = newPosition;
                    break;
                case '+':
                    rotation *= Quaternion.Euler(0, 0, -Angle);
                    break;
                case '-':
                    rotation *= Quaternion.Euler(0, 0, Angle);
                    break;
                case '[':
                    transformStack.Push(new TransformInfo(position, rotation));
                    break;
                case ']':
                    if (transformStack.Count > 0)
                    {
                        TransformInfo info = transformStack.Pop();
                        position = info.Position;
                        rotation = info.Rotation;
                        positions.Add(position);
                    }
                    else
                    {
                        Debug.LogWarning("Transform stack is empty. Cannot pop.");
                    }
                    break;
                // Handle additional symbols as needed
                default:
                    // Optional: Handle or ignore unrecognized symbols
                    break;
            }
        }

        // Update the LineRenderer with the new positions
        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
    }

    #endregion

    #region Private Structures

    /// <summary>
    /// Stores the position and rotation information for branching.
    /// </summary>
    private struct TransformInfo
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public TransformInfo(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }

    #endregion
}
