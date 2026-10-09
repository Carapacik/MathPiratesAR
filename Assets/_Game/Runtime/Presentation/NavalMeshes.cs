using System.Collections.Generic;
using UnityEngine;

namespace MathPirates.Presentation
{
    public static class NavalMeshes
    {
        public static Mesh Hull()
        {
            float[] lengths = { -.62f, -.43f, .27f, .65f };
            float[] widths = { .18f, .29f, .25f, .015f };
            float[] heights = { .22f, .16f, .18f, .30f };
            var points = new List<Vector3>();
            Vector3 Cross(int ring, int corner)
            {
                float width = widths[ring];
                return corner switch
                {
                    0 => new Vector3(lengths[ring], heights[ring], -width),
                    1 => new Vector3(lengths[ring], -.06f, -width * .8f),
                    2 => new Vector3(lengths[ring], -.24f + (ring == 3 ? .15f : 0), 0),
                    3 => new Vector3(lengths[ring], -.06f, width * .8f),
                    _ => new Vector3(lengths[ring], heights[ring], width)
                };
            }
            for (int ring = 0; ring < 3; ring++)
                for (int edge = 0; edge < 4; edge++)
                    Quad(points, Cross(ring, edge), Cross(ring + 1, edge), Cross(ring + 1, edge + 1), Cross(ring, edge + 1));
            // Closed stern and stem; the deck is recessed below the raised gunwales.
            for (int edge = 1; edge < 4; edge++)
            {
                points.AddRange(new[] { Cross(0, 0), Cross(0, edge), Cross(0, edge + 1) });
                points.AddRange(new[] { Cross(3, 0), Cross(3, edge + 1), Cross(3, edge) });
            }
            return Finish(points, "Wooden ship hull");
        }

        public static Mesh Deck()
        {
            Vector3[] outline = { new(-.58f,.13f,-.16f), new(-.42f,.13f,-.26f), new(.26f,.13f,-.22f), new(.60f,.13f,0), new(.26f,.13f,.22f), new(-.42f,.13f,.26f), new(-.58f,.13f,.16f) };
            var points = new List<Vector3>();
            for (int i = 0; i < outline.Length; i++)
                points.AddRange(new[] { new Vector3(0,.13f,0), outline[(i + 1) % outline.Length], outline[i] });
            return Finish(points, "Recessed wooden deck");
        }

        public static Mesh Water()
        {
            const int rings = 10, segments = 48;
            var points = new List<Vector3>();
            Vector3 Point(int ring, int segment)
            {
                float angle = segment * Mathf.PI * 2 / segments;
                float radius = ring / (float)rings;
                return new Vector3(Mathf.Cos(angle) * radius * 3, 0, Mathf.Sin(angle) * radius * 2.5f);
            }
            for (int ring = 0; ring < rings; ring++)
                for (int segment = 0; segment < segments; segment++)
                {
                    var a = Point(ring, segment); var b = Point(ring + 1, segment);
                    var c = Point(ring + 1, segment + 1); var d = Point(ring, segment + 1);
                    points.AddRange(new[] { a, c, b });
                    if (ring > 0) points.AddRange(new[] { a, d, c });
                }
            var mesh = Finish(points, "Low poly wave surface");
            var colors = new Color[points.Count];
            for (int i = 0; i < colors.Length; i++)
            {
                float tint = .83f + ((i / 3 * 73) % 97) / 96f * .34f;
                colors[i] = new Color(tint, tint, tint, 1);
            }
            mesh.colors = colors;
            mesh.bounds = new Bounds(Vector3.zero, new Vector3(6.1f, .4f, 5.1f));
            return mesh;
        }

        public static float WaveHeight(float x, float z, float time) =>
            Mathf.Sin(time * 1.1f + x * 1.7f + z * .8f) * .08f + Mathf.Sin(time * .8f + z * 2.3f - x * .4f) * .035f;

        public static Mesh Sail()
        {
            Vector3[] outline = { new(-.46f,.98f,0), new(.46f,.98f,0), new(.36f,.36f,-.03f), new(-.36f,.36f,-.03f) };
            var points = new List<Vector3>();
            var center = new Vector3(0,.67f,-.13f);
            for (int i = 0; i < 4; i++)
            {
                var a = outline[i]; var b = outline[(i+1)%4];
                points.AddRange(new[] { center, a, b, center, b, a });
            }
            return Finish(points, "Broad cloth mainsail");
        }

        private static void Quad(List<Vector3> points, Vector3 a, Vector3 b, Vector3 c, Vector3 d) => points.AddRange(new[] { a, b, c, a, c, d });
        private static Mesh Finish(List<Vector3> points, string name)
        {
            var mesh = new Mesh { name = name };
            mesh.SetVertices(points);
            var indices = new int[points.Count];
            for (int i = 0; i < indices.Length; i++) indices[i] = i;
            mesh.triangles = indices;
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }
    }
}
