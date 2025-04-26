using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;

namespace DrawingAppCG.Models
{
    public class Polygon : ShapeBase
    {
        public List<(int x, int y)> Points { get; set; } = new();
        public override void Draw(WriteableBitmap bitmap)
        {
            for(int i = 0; i < Points.Count; i++)
            {
                var p1 = Points[i];
                var p2 = Points[(i + 1) % Points.Count];
                var line = new Line
                {
                    Start = p1,
                    End = p2,
                    Color = Color,
                    Thickness = Thickness,
                    IsAntiAliased = IsAntiAliased
                };
                line.Draw(bitmap);
            }
        }
    }
}
