using System.Collections.Generic;
using UnityEngine;

namespace MathPirates.Presentation
{
    public static class LowPolyMesh
    {
        // Flat normals, shared meshes, no imported models, textures or water simulation.
        public static Mesh Create(PrimitiveType type)
        {
            if (type == PrimitiveType.Cube)
            {
                var vertices = new List<Vector3>();
                Vector3[] corners = { new(-.5f,-.5f,-.5f), new(.5f,-.5f,-.5f), new(.5f,.5f,-.5f), new(-.5f,.5f,-.5f), new(-.5f,-.5f,.5f), new(.5f,-.5f,.5f), new(.5f,.5f,.5f), new(-.5f,.5f,.5f) };
                int[] faces = { 0,3,2,0,2,1, 4,5,6,4,6,7, 0,4,7,0,7,3, 1,2,6,1,6,5, 3,7,6,3,6,2, 0,1,5,0,5,4 };
                foreach (int index in faces) vertices.Add(corners[index]);
                return Finish(vertices, "Low poly box");
            }
            int segments = type == PrimitiveType.Cylinder ? 24 : 8;
            Vector2[] rings = type == PrimitiveType.Cylinder
                ? new[] { new Vector2(0, -1), new Vector2(.5f, -1), new Vector2(.5f, 1), new Vector2(0, 1) }
                : new[] { new Vector2(0, -.5f), new Vector2(.5f, -.15f), new Vector2(.5f, .15f), new Vector2(0, .5f) };
            var points = new List<Vector3>();
            Vector3 Point(Vector2 ring, int segment)
            {
                float angle = segment * Mathf.PI * 2 / segments;
                return new Vector3(Mathf.Cos(angle) * ring.x, ring.y, Mathf.Sin(angle) * ring.x);
            }
            for (int r = 0; r < rings.Length - 1; r++)
                for (int s = 0; s < segments; s++)
                {
                    var a = Point(rings[r], s); var b = Point(rings[r + 1], s);
                    var c = Point(rings[r + 1], s + 1); var d = Point(rings[r], s + 1);
                    if (rings[r + 1].x > 0) points.AddRange(new[] { a, b, c });
                    if (rings[r].x > 0) points.AddRange(new[] { a, c, d });
                }
            return Finish(points, "Low poly " + type);
        }

        private static Mesh Finish(List<Vector3> points, string name)
        {
            var mesh = new Mesh { name = name };
            mesh.SetVertices(points);
            int[] triangles = new int[points.Count];
            for (int i = 0; i < triangles.Length; i++) triangles[i] = i;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
