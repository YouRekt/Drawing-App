using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace DrawingAppCG.Models
{
    public abstract class ShapeBase
    {
        public Color Color { get; set; } = Colors.Black;
        public int Thickness { get; set; } = 1;
        public bool IsAntiAliased { get; set; } = true;

        public abstract void Draw(WriteableBitmap bitmap);
    }
}
