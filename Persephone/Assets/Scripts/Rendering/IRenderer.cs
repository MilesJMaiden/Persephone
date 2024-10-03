// Assets/Scripts/Generation/IRenderer.cs
namespace ProceduralGraphics.LSystems.Generation
{
    /// <summary>
    /// Interface for renderer implementations.
    /// </summary>
    public interface IRenderer
    {
        /// <summary>
        /// Renders the provided L-System string.
        /// </summary>
        /// <param name="lSystemString">The generated L-System string.</param>
        void Render(string lSystemString);
    }
}
