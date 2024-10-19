using UnityEngine;
using System.Collections.Generic;

namespace ProceduralGraphics.LSystems.Rendering
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class LSystemMeshRenderer : RendererBase
    {
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;

        public int segments = 8; // Number of segments for cylinders
        public float branchRadius = 0.05f; // Radius of each branch

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshFilter = GetComponent<MeshFilter>();

            if (meshRenderer == null || meshFilter == null)
            {
                Debug.LogError("LSystemMeshRenderer: MeshRenderer or MeshFilter component is missing.");
            }

            if (meshRenderer.material == null)
            {
                meshRenderer.material = new Material(Shader.Find("Standard"));
            }
        }

        public override void Render(string lSystemString, float length, float angle, float randomOffset)
        {
            Debug.Log("LSystemMeshRenderer: Render method called.");

            if (meshRenderer == null || meshFilter == null)
            {
                Debug.LogError("LSystemMeshRenderer: MeshRenderer or MeshFilter component is not assigned.");
                return;
            }

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            Stack<TransformState> stack = new Stack<TransformState>();
            Vector3 currentPosition = Vector3.zero;
            Quaternion currentRotation = Quaternion.identity;

            Mesh mesh = new Mesh();
            int vertexIndex = 0;

            foreach (char command in lSystemString)
            {
                switch (command)
                {
                    case 'F':
                        Vector3 nextPosition = currentPosition + currentRotation * Vector3.up * length;
                        CreateCylinder(vertices, triangles, currentPosition, nextPosition, branchRadius, segments, ref vertexIndex);
                        currentPosition = nextPosition;
                        break;

                    case '+':
                        currentRotation *= Quaternion.Euler(0, 0, -angle);
                        break;

                    case '-':
                        currentRotation *= Quaternion.Euler(0, 0, angle);
                        break;

                    case '[':
                        stack.Push(new TransformState(currentPosition, currentRotation));
                        break;

                    case ']':
                        if (stack.Count > 0)
                        {
                            TransformState state = stack.Pop();
                            currentPosition = state.position;
                            currentRotation = state.rotation;
                        }
                        break;

                    default:
                        break;
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            meshFilter.mesh = mesh;

            Debug.Log($"LSystemMeshRenderer: Rendered mesh with {vertices.Count} vertices.");
        }

        private void CreateCylinder(List<Vector3> vertices, List<int> triangles, Vector3 start, Vector3 end, float radius, int segments, ref int vertexIndex)
        {
            // Implementation of creating a cylinder mesh between start and end positions.
        }

        private struct TransformState
        {
            public Vector3 position;
            public Quaternion rotation;

            public TransformState(Vector3 pos, Quaternion rot)
            {
                position = pos;
                rotation = rot;
            }
        }
    }
}
