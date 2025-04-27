using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.Collections.Generic;

namespace DrawingAppCG.Models
{
    public abstract class ShapeBase
    {
        public Color Color { get; set; } = Colors.Black;
        public int Thickness { get; set; } = 1;
        public bool IsSelected { get; set; } = false;
        public abstract bool ContainsPoint(int x, int y);
        public abstract void Move(int deltaX, int deltaY);
        public abstract void MovePoint(int pointIndex, int newX, int newY);
        public abstract List<(int x, int y)> GetControlPoints();
        public abstract void Draw(WriteableBitmap bitmap);
        public void DrawHitpoints(WriteableBitmap bitmap, (int sx, int sy)? selectedHitpoint = null)
        {
            using (var fb = bitmap.Lock())
            {
                foreach (var (x, y) in GetControlPoints())
                {
                    var hitPoint = new Circle
                    {
                        CenterX = x,
                        CenterY = y,
                        Radius = 5,
                        Color = selectedHitpoint != null ? (selectedHitpoint == (x, y) ? Colors.Blue : Colors.Red) : Colors.Red,
                    };
                    hitPoint.Draw(bitmap);
                }
            }
        }
    }
    public abstract class AntiAliasedShapeBase : ShapeBase
    {
        public bool IsAntialiased { get; set; } = false;
    }
}
