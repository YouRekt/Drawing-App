using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DrawingAppCG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;

namespace DrawingAppCG.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public WriteableBitmap Bitmap { get; }
        private int _width = 800;
        private int _height = 600;
        public List<ShapeBase> Shapes { get; set; } = new();
        public Tool SelectedTool { get; set; } = Tool.None;
        public Polygon? TempPolygon { get; set; }
        private int _selectedThickness = 1;
        public int SelectedThickness
        {
            get => _selectedThickness;
            set
            {
                _selectedThickness = value;
                OnPropertyChanged(nameof(SelectedThickness));
            }
        }
        public MainWindowViewModel()
        {
            Bitmap = new(new PixelSize(_width, _height), new Vector(96, 96), PixelFormat.Bgra8888);
            ClearBitmap();
        }
        public void SetPixel(int x, int y, Color color)
        {
            using (var fb = Bitmap.Lock())
            {
                unsafe
                {
                    IntPtr buffer = fb.Address;
                    int stride = fb.RowBytes;
                    byte* ptr = (byte*)buffer + y * stride + x * 4;
                    ptr[0] = color.B;
                    ptr[1] = color.G;
                    ptr[2] = color.R;
                    ptr[3] = 255;
                }
            }

            OnPropertyChanged(nameof(Bitmap));
        }
        public void ClearBitmap()
        {
            using (var fb = Bitmap.Lock())
            {
                unsafe
                {
                    fb.GetPixels().Fill(255);
                }
            }
        }
        public void RedrawAll()
        {
            ClearBitmap();

            foreach (var shape in Shapes)
                shape.Draw(Bitmap);

            OnPropertyChanged(nameof(Bitmap));
        }
        public void AddShape(ShapeBase shape)
        {
            Shapes.Add(shape);
            shape.Draw(Bitmap);
        }
        public void ClearShapes()
        {
            Shapes.Clear();
            RedrawAll();
        }
    }
}
