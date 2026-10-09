using UnityEngine;
using UnityEngine.UIElements;

namespace MathPirates.Presentation
{
    public sealed class PixelHeart : VisualElement
    {
        private static readonly string[] Pixels = { "0110110", "1111111", "1111111", "0111110", "0011100", "0001000" };

        public PixelHeart()
        {
            AddToClassList("pixel-heart");
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
        }

        private void Draw(MeshGenerationContext context)
        {
            var painter = context.painter2D;
            painter.fillColor = new Color32(230, 48, 62, 255);
            float cell = Mathf.Min(contentRect.width / 7, contentRect.height / 6);
            for (int y = 0; y < Pixels.Length; y++)
                for (int x = 0; x < Pixels[y].Length; x++)
                {
                    if (Pixels[y][x] != '1') continue;
                    painter.BeginPath();
                    painter.MoveTo(new Vector2(x * cell, y * cell));
                    painter.LineTo(new Vector2((x + 1) * cell, y * cell));
                    painter.LineTo(new Vector2((x + 1) * cell, (y + 1) * cell));
                    painter.LineTo(new Vector2(x * cell, (y + 1) * cell));
                    painter.ClosePath();
                    painter.Fill();
                }
        }
    }
}
