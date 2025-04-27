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
        public WriteableBitmap Overlay { get; }
        private int _width = 800;
        private int _height = 600;
        public List<ShapeBase> Shapes { get; set; } = new();
        private Tool _selectedTool = Tool.None;
        public Tool SelectedTool
        {
            get => _selectedTool;
            set
            {
                _selectedTool = value;
                OnPropertyChanged(nameof(SelectedTool));
            }
        }
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
        private bool _isAntialiased = false;
        public bool IsAntialiased
        {
            get => _isAntialiased;
            set
            {
                _isAntialiased = value;
                OnPropertyChanged(nameof(IsAntialiased));
            }
        }
        public ShapeBase? SelectedShape { get; set; }
        public int? SelectedControlPointIndex { get; set; }
        private (int x, int y)? _dragStartPoint;
        public MainWindowViewModel()
        {
            Bitmap = new(new PixelSize(_width, _height), new Vector(96, 96), PixelFormat.Bgra8888);
            Overlay = new(new PixelSize(_width, _height), new Vector(96, 96), PixelFormat.Bgra8888);
            ClearBitmap();
        }
        public void ClearOverlay()
        {
            using (var fb = Overlay.Lock())
            {
                unsafe
                {
                    fb.GetPixels().Clear();
                }
            }
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
        public void SelectShapeAt(int x, int y)
        {
            if (SelectedShape != null)
            {
                SelectedShape.IsSelected = false;
                SelectedShape = null;
            }
            SelectedControlPointIndex = null;

            var reversedShapes = new List<ShapeBase>(Shapes);
            reversedShapes.Reverse();

            foreach (var shape in reversedShapes)
            {
                if (shape.ContainsPoint(x, y))
                {
                    SelectedShape = shape;
                    shape.IsSelected = true;

                    var controlPoints = shape.GetControlPoints();
                    for (int i = 0; i < controlPoints.Count; i++)
                    {
                        var pt = controlPoints[i];
                        if (Math.Abs(pt.x - x) < 5 && Math.Abs(pt.y - y) < 5)
                        {
                            SelectedControlPointIndex = i;
                            break;
                        }
                    }
                    RedrawAll();
                    shape.DrawHitpoints(Overlay, SelectedControlPointIndex == null ? null : controlPoints[SelectedControlPointIndex.Value]);
                    return;
                }
            }
            RedrawAll();
        }
        public void StartDrag(int x, int y)
        {
            _dragStartPoint = (x, y);
        }
        public void HandleDrag(int x, int y)
        {
            if (_dragStartPoint == null || SelectedShape == null) return;

            var (startX, startY) = _dragStartPoint.Value;
            int deltaX = x - startX;
            int deltaY = y - startY;

            if (SelectedControlPointIndex.HasValue)
                SelectedShape.MovePoint(SelectedControlPointIndex.Value, x, y);
            else
                SelectedShape.Move(deltaX, deltaY);

            _dragStartPoint = (x, y);
            RedrawAll();
        }
        public void EndDrag()
        {
            _dragStartPoint = null;
        }
        public void DeleteSelectedShape()
        {
            if (SelectedShape != null)
            {
                Shapes.Remove(SelectedShape);
                SelectedShape = null;
                RedrawAll();
            }
        }
    }
}
