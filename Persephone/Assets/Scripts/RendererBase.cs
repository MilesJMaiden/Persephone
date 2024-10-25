// Assets/Scripts/Rendering/RendererBase.cs
using ProceduralGraphics.LSystems.ScriptableObjects;
using UnityEngine;

namespace ProceduralGraphics.LSystems.Rendering
{
    /// <summary>
    /// Abstract base class for rendering L-System structures.
    /// </summary>
    public abstract class RendererBase : MonoBehaviour
    {
        /// <summary>
        /// Abstract method to render the L-System string.
        /// </summary>
        /// <param name="lSystemString">The generated L-System string.</param>
        /// <param name="length">The length of each branch segment.</param>
        /// <param name="angle">The angle (in degrees) used for turning.</param>
        public abstract void Render(LSystemConfig config);
    }
}
