using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DrawingAppCG.Models
{

    [JsonDerivedType(typeof(Circle), "Circle")]
    [JsonDerivedType(typeof(Line), "Line")]
    [JsonDerivedType(typeof(Polygon), "Polygon")]
    [JsonDerivedType(typeof(Pill), "Pill")]
    [JsonDerivedType(typeof(Rectangle), "Rectangle")]
    public abstract class ShapeBase
    {
        [JsonConverter(typeof(ColorConverter))]
        public Color Color { get; set; } = Colors.Black;
        public int Thickness { get; set; } = 1;
        [JsonIgnore]
        public bool IsSelected { get; set; } = false;
        public abstract bool ContainsPoint(int x, int y);
        public abstract void Move(int deltaX, int deltaY);
        public abstract void MovePoint(int pointIndex, int newX, int newY);
        public abstract List<(int x, int y)> GetControlPoints();
        public abstract void Draw(WriteableBitmap bitmap);
        public void DrawHitpoints(WriteableBitmap bitmap, HashSet<int> selectedIndices)
        {
            using (var fb = bitmap.Lock())
            {
                var controlPoints = GetControlPoints();
                for (int i = 0; i < controlPoints.Count; i++)
                {
                    var (x, y) = controlPoints[i];
                    Color color = selectedIndices.Count > 0 && selectedIndices.Contains(i) ? Colors.Blue : Colors.Red;
                    var hitPoint = new Circle
                    {
                        Center = (x, y),
                        Radius = 5,
                        Color = color,
                    };
                    hitPoint.Draw(bitmap);
                }
            }
        }
    }
    [JsonDerivedType(typeof(Line), "Line")]
    [JsonDerivedType(typeof(Polygon), "Polygon")]
    [JsonDerivedType(typeof(Rectangle), "Rectangle")]
    public abstract class AntiAliasedShapeBase : ShapeBase
    {
        public bool IsAntialiased { get; set; } = false;
    }
}
