using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DrawingAppCG.Models
{
    public class Polygon : AntiAliasedShapeBase
    {
        public List<(int x, int y)> Points { get; set; } = [];
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
                    IsAntialiased = IsAntialiased
                };
                line.Draw(bitmap);
            }
        }
        public override bool ContainsPoint(int x, int y)
        {
            foreach (var point in Points)
            {
                if (Math.Abs(point.x - x) < 5 && Math.Abs(point.y - y) < 5)
                    return true;
            }

            for (int i = 0; i < Points.Count; i++)
            {
                var p1 = Points[i];
                var p2 = Points[(i + 1) % Points.Count];
                if (Line.DistanceToLine(x, y, p1.x, p1.y, p2.x, p2.y) <= 5)
                    return true;
            }
            return false;
        }
        public override void Move(int deltaX, int deltaY)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] = (Points[i].x + deltaX, Points[i].y + deltaY);
            }
        }
        public override void MovePoint(int pointIndex, int newX, int newY)
        {
            if (pointIndex >= 0 && pointIndex < Points.Count)
                Points[pointIndex] = (newX, newY);
        }
        public override List<(int x, int y)> GetControlPoints() => Points;
    }
}
