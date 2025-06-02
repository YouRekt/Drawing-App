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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static DrawingAppCG.Models.FillRegion;

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
        public ClippingStage CurrentClippingStage { get; set; } = ClippingStage.None;
        public Polygon? ClipSubject { get; private set; } // Potentially AntiAliasedShape to discard Circle and Pill from clipping? Now polygon for sake of simplicity
        public Polygon? ClippingPolygon { get; set; }
        private FillMode _fillMode = FillMode.None;
        public FillMode FillMode
        {
            get => _fillMode;
            set
            {
                _fillMode = value;
                OnPropertyChanged(nameof(FillMode));
            }
        }
        private Color? _fillColor;
        public Color? FillColor { 
            get => _fillColor;
            set
            { 
                _fillColor = value;
                if (SelectedShape != null && SelectedShape is Polygon polygon)
                {
                    polygon.FillSource = FillSource;
                    polygon.Draw(Bitmap);
                }
                OnPropertyChanged(nameof(FillColor));
            } 
        } //= Colors.Magenta;
        private ImageFill? _imageFill;
        public ImageFill? FillImage { 
            get => _imageFill; 
            set
            {
                _imageFill = value;
                if (SelectedShape != null && SelectedShape is Polygon polygon)
                {
                    polygon.FillSource = FillSource;
                    polygon.Draw(Bitmap);
                }
                OnPropertyChanged(nameof(FillImage));
            }
        }
        public IFillSource? FillSource => FillMode == FillMode.None ? null : (FillMode == FillMode.Color ? FillColor?.AsFillSource() : FillImage);
        public (int x, int y)? BucketPoint { get; set; }
        private float _alpha = 0.0f;
        public float Alpha
        {
            get => _alpha;
            set
            {
                _alpha = value;
                if (SelectedShape != null && SelectedShape is Cube cube)
                {
                    cube.Alpha = value;
                    cube.Draw(Bitmap);
                }
                OnPropertyChanged(nameof(Alpha));
            }
        }
        private float _beta = 0.0f;
        public float Beta
        {
            get => _beta;
            set
            {
                _beta = value;
                if (SelectedShape != null && SelectedShape is Cube cube)
                {
                    cube.Beta = value;
                    cube.Draw(Bitmap);
                }
                OnPropertyChanged(nameof(Beta));
            }
        }
        private int _distance = 400;
        public int Distance
        {
            get => _distance;
            set
            {
                _distance = value;
                if (SelectedShape != null && SelectedShape is Cube cube)
                {
                    cube.Distance = value;
                    cube.Draw(Bitmap);
                }
                OnPropertyChanged(nameof(Distance));
            }
        }
        private int _size = 100;
        public int Size
        {
            get => _size;
            set
            {
                _size = value;
                if (SelectedShape != null && SelectedShape is Cube cube)
                {
                    cube.Size = value;
                    cube.Draw(Bitmap);
                }
                OnPropertyChanged(nameof(Size));
            }
        }
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
            if (BucketPoint.HasValue)
                FillRegion(BucketPoint.Value);

            OnPropertyChanged(nameof(Bitmap));
        }
        public void AddShape(ShapeBase shape)
        {
            Shapes.Add(shape);
            shape.Draw(Bitmap);
            OnPropertyChanged(nameof(Bitmap));
        }
        public void ClearShapes()
        {
            Shapes.Clear();
            BucketPoint = null;
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
                                    case Polygon when shape is not Rectangle:
                                        if (SelectedPointIndices.Contains(i))
                                            SelectedPointIndices.Remove(i);
                                        else
                                            SelectedPointIndices.Add(i);
                                        break;
                                    case Rectangle:
                                        if (SelectedPointIndices.Contains(i))
                                            SelectedPointIndices.Remove(i);
                                        else if (!SelectedPointIndices.Contains((i + 2) % 4))
                                        {
                                            if (SelectedPointIndices.Count == 2)
                                                SelectedPointIndices.Remove(_lastSelectedPointIndex!.Value);
                                            SelectedPointIndices.Add(i);
                                            _lastSelectedPointIndex = i;
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
        public void SelectClipSubjectAt(int x, int y)
        {
            foreach (var shape in Shapes)
            {
                if (shape is Polygon polygon)
                {
                    if (polygon.ContainsPoint(x, y))
                    {
                        ClipSubject = polygon;
                        CurrentClippingStage = ClippingStage.SelectedSubjectPolygon;
                        UpdateClippedHitpoints();
                        return;
                    }
                    else ClipSubject = null;
                }
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
                    //System.Diagnostics.Debug.WriteLine($"Moving point {index}: {controlPoints[index].x} + {deltaX}, {controlPoints[index].y} + {deltaY}");
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
        public void UpdateClippedHitpoints()
        {
            ClearOverlay();
            //System.Diagnostics.Debug.WriteLine($"Count: {ClipSubject!.GetControlPoints().Count}, Enumerable.Range: {String.Join(", ", Enumerable.Range(0, ClipSubject.GetControlPoints().Count))}");
            ClipSubject!.DrawHitpoints(Overlay, [.. Enumerable.Range(0, ClipSubject.GetControlPoints().Count)]);
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

            // Rebuild clip references
            var polygonMap = loadedShapes
                .OfType<Polygon>()
                .ToDictionary(p => p.Id);

            foreach (var polygon in polygonMap.Values)
            {
                if (polygon.ClipId is Guid id && polygonMap.TryGetValue(id, out var clip))
                {
                    polygon.Clip = clip;
                }
                if (!string.IsNullOrEmpty(polygon.FillImagePath) && File.Exists(polygon.FillImagePath))
                {
                    var bmp = new Bitmap(polygon.FillImagePath);
                    var wb = new WriteableBitmap(bmp.PixelSize, bmp.Dpi, bmp.Format);
                    using (var fb = wb.Lock())
                    {
                        bmp.CopyPixels(new PixelRect(fb.Size), fb.Address, fb.Size.Height * fb.RowBytes, fb.RowBytes);
                    }
                    polygon.FillSource = new ImageFill(wb, polygon.FillImagePath);
                }
                else if (polygon.FillColor.HasValue)
                {
                    polygon.FillSource = polygon.FillColor.Value.AsFillSource();
                }
            }

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
        public async Task LoadFillImage(Window window)
        {
            var files = await window.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Select Fill Image",
                    FileTypeFilter = [new FilePickerFileType("Images") { Patterns = ["*.png", "*.jpg", "*.jpeg", "*.bmp"] }]
                });

            if (files.Count == 0) return;

            var file = files[0];
            await using var stream = await file.OpenReadAsync();
            var bmp = new Bitmap(stream);
            var wb = new WriteableBitmap(bmp.PixelSize, bmp.Dpi, bmp.Format);
            using (var fb = wb.Lock())
            {
                bmp.CopyPixels(new PixelRect(fb.Size), fb.Address, fb.Size.Height * fb.RowBytes, fb.RowBytes);
            }
            FillImage = new ImageFill(wb, file.Path.LocalPath);
        }
        public void FillRegion((int x, int y) seed)
        {
            if(FillSource != null)
                SmithScanline(Bitmap, seed, FillSource);
        }
    }
}
