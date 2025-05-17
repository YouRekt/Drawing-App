using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DrawingAppCG.Models
{
    public class Rectangle : AntiAliasedShapeBase
    {
        public (int x, int y) TopLeft { get; set; }
        public (int x, int y) BottomRight { get; set; }
        [JsonIgnore]
        public (int x, int y) BottomLeft => (TopLeft.x, BottomRight.y);
        [JsonIgnore]
        public (int x, int y) TopRight => (BottomRight.x, TopLeft.y);
        public override bool ContainsPoint(int x, int y)
        {
            var edges = GetEdges();

            foreach (var edge in edges)
            {
                if (edge.ContainsPoint(x, y)) return true;
            }

            return false;
        }
        public override void Draw(WriteableBitmap bitmap)
        {
            var edges = GetEdges();

            foreach (var edge in edges)
                edge.Draw(bitmap);
        }
        public override List<(int x, int y)> GetControlPoints() => [TopLeft, BottomRight, BottomLeft, TopRight];
        public override void Move(int deltaX, int deltaY)
        {
            TopLeft = (TopLeft.x + deltaX, TopLeft.y + deltaY);
            BottomRight = (BottomRight.x + deltaX, BottomRight.y + deltaY);
        }
        public override void MovePoint(int pointIndex, int newX, int newY)
        {
            switch (pointIndex)
            {
                case 0:
                    TopLeft = (newX, newY);
                    break;
                case 1:
                    BottomRight = (newX, newY);
                    break;
                case 2:
                    TopLeft = (newX, TopLeft.y);
                    BottomRight = (BottomRight.x, newY);
                    break;
                case 3:
                    TopLeft = (TopLeft.x, newY);
                    BottomRight = (newX, BottomRight.y);
                    break;
            }
        }
        private List<Line> GetEdges()
        {
            return
            [
                new() { Start = TopLeft, End = TopRight, Color = Color, Thickness = Thickness, IsAntialiased = IsAntialiased },
                new() { Start = BottomLeft, End = BottomRight, Color = Color, Thickness = Thickness, IsAntialiased = IsAntialiased },
                new() { Start = TopLeft, End = BottomLeft, Color = Color, Thickness = Thickness, IsAntialiased = IsAntialiased },
                new() { Start = TopRight, End = BottomRight, Color = Color, Thickness = Thickness, IsAntialiased = IsAntialiased },
            ];
        }
    }
}
