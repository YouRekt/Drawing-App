using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using DrawingAppCG.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Schema;

namespace DrawingAppCG.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public WriteableBitmap Bitmap { get; }
        public WriteableBitmap Overlay { get; }
        private readonly int _width = 800;
        private readonly int _height = 600;
        public List<ShapeBase> Shapes { get; set; } = [];
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
                if (SelectedShape != null)
                {
                    SelectedShape.Thickness = value;
                    SelectedShape.Draw(Bitmap);
                }
                OnPropertyChanged(nameof(SelectedThickness));
            }
        }
        private Color _selectedColor = Colors.Black;
        public Color SelectedColor
        {
            get => _selectedColor;
            set
            {
                _selectedColor = value;
                if (SelectedShape != null)
                {
                    SelectedShape.Color = value;
                    SelectedShape.Draw(Bitmap);
                }
                OnPropertyChanged(nameof(SelectedColor));
            }
        }
        private bool _isAntialiased = false;
        public bool IsAntialiased
        {
            get => _isAntialiased;
            set
            {
                _isAntialiased = value;
                if (SelectedShape is AntiAliasedShapeBase notCircle)
                {
                    notCircle.IsAntialiased = value;
                    notCircle.Draw(Bitmap);
                }
                OnPropertyChanged(nameof(IsAntialiased));
            }
        }
        private ShapeBase? _selectedShape;
        public ShapeBase? SelectedShape
        {
            get => _selectedShape;
            set
            {
                _selectedShape = value;
                OnPropertyChanged(nameof(SelectedShape));
            }
        }
        public HashSet<int> SelectedPointIndices { get; } = [];
        private int? _lastSelectedPointIndex;
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
        public void SelectShapeAt(int x, int y, bool isShiftDown)
        {
            if (!isShiftDown)
            {
                if (SelectedShape != null) SelectedShape.IsSelected = false;
                SelectedShape = null;
                SelectedPointIndices.Clear();
            }

            foreach (var shape in Shapes.AsEnumerable().Reverse())
            {
                if (shape.ContainsPoint(x, y))
                {
                    SelectedShape = shape;
                    shape.IsSelected = true;

                    var controlPoints = shape.GetControlPoints();
                    for (int i = 0; i < controlPoints.Count; i++)
                    {
                        var (ptX, ptY) = controlPoints[i];
                        if (Math.Abs(ptX - x) < 5 && Math.Abs(ptY - y) < 5)
                        {
                            if (isShiftDown)
                            {
                                switch (shape)
                                {
                                    case Polygon:
                                        if (SelectedPointIndices.Contains(i))
                                            SelectedPointIndices.Remove(i);
                                        else
                                            SelectedPointIndices.Add(i);
                                        break;
                                    case Rectangle:
                                        if (SelectedPointIndices.Contains(i))
                                            SelectedPointIndices.Remove(i);
                                        else if (!(SelectedPointIndices.Contains(i + 1) || SelectedPointIndices.Contains(i - 1)))
                                        {
                                            if (SelectedPointIndices.Count == 2)
                                                SelectedPointIndices.Remove(_lastSelectedPointIndex!.Value);
                                            SelectedPointIndices.Add(i);
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                SelectedPointIndices.Clear();
                                SelectedPointIndices.Add(i);
                            }
                            break;
                        }
                    }

                    UpdateHitpoints();
                    RedrawAll();
                    return;
                }
            }

            if (!isShiftDown)
            {
                RedrawAll();
            }
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

            var controlPoints = SelectedShape.GetControlPoints();
            if (SelectedPointIndices.Count > 0 && SelectedPointIndices.Count < controlPoints.Count)
            {
                foreach (var index in SelectedPointIndices)
                {
                    SelectedShape.MovePoint(index,
                        controlPoints[index].x + deltaX,
                        controlPoints[index].y + deltaY);
                    System.Diagnostics.Debug.WriteLine($"Moving point {index}: {controlPoints[index].x} + {deltaX}, {controlPoints[index].y} + {deltaY}");
                }
            }
            else
            {
                // Normal movement for other shapes
                SelectedShape.Move(deltaX, deltaY);
            }

            _dragStartPoint = (x, y);
            UpdateHitpoints();
            RedrawAll();
        }
        public void UpdateHitpoints()
        {
            ClearOverlay();
            SelectedShape?.DrawHitpoints(Overlay, SelectedPointIndices);
        }
        public void EndDrag()
        {
            _dragStartPoint = null;
        }
        public bool CanDelete => SelectedShape != null;
        [RelayCommand(CanExecute = nameof(CanDelete))]
        public void DeleteSelectedShape()
        {
            if (SelectedShape != null)
            {
                Shapes.Remove(SelectedShape);
                SelectedShape = null;
                ClearOverlay();
                RedrawAll();
            }
        }
        public async Task SaveShapes(Window window)
        {
            var file = await window.StorageProvider.SaveFilePickerAsync(
                new FilePickerSaveOptions
                {
                    Title = "Save Drawing",
                    FileTypeChoices = [new FilePickerFileType("Drawing") { Patterns = ["*.draw"] }]
                });

            if (file is null) return;

            await using var stream = await file.OpenWriteAsync();
            await JsonSerializer.SerializeAsync(stream, Shapes, SerializerOptions);
        }
        public async Task LoadShapes(Window window)
        {
            var files = await window.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Open Drawing",
                    FileTypeFilter = [new FilePickerFileType("Drawing") { Patterns = ["*.draw"] }]
                });

            if (files.Count == 0) return;

            await using var stream = await files[0].OpenReadAsync();
            var loadedShapes = await JsonSerializer.DeserializeAsync<List<ShapeBase>>(
                stream, SerializerOptions);

            if (loadedShapes is null) return;

            ClearShapes();
            foreach (var shape in loadedShapes)
            {
                AddShape(shape);
            }
        }
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            IncludeFields = true,
            Converters =
            {
                new ColorConverter(),
                new PointConverter(),
            },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
    }
}
