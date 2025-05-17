using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DrawingAppCG.Models;
using DrawingAppCG.ViewModels;
using System;
using System.Linq;

namespace DrawingAppCG.Views;

public partial class MainWindow : Window
{
    private (int x, int y)? startPoint = null;
    private Point? _lastPointerPosition = null;
    private bool _isDragging = false;
    private ScaleTransform? _scaleTransform;
    private TranslateTransform? _translateTransform;
    public MainWindow()
    {
        InitializeComponent();
        var transformGroup = Resources["ImageTransform"] as TransformGroup;
        _scaleTransform = transformGroup?.Children.OfType<ScaleTransform>().FirstOrDefault();
        _translateTransform = transformGroup?.Children.OfType<TranslateTransform>().FirstOrDefault();
    }
    private void SelectLineTool(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SelectedTool = Tool.Line;
            startPoint = null;
        }
    }
    private void SelectCircleTool(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SelectedTool = Tool.Circle;
            startPoint = null;
        }
    }
    private void SelectRectangleTool(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SelectedTool = Tool.Rectangle;
            startPoint = null;
        }
    }
    private void SelectPolygonTool(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SelectedTool = Tool.Polygon;
            startPoint = null;
        }
    }
    private void SelectPillTool(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SelectedTool = Tool.Pill;
            startPoint = null;
        }
    }
    private void SelectMoveTool(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SelectedTool = Tool.Move;
            startPoint = null;
        }
    }
    private void ClearShapes(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.ClearShapes();
            startPoint = null;
            ImageCanvas.InvalidateVisual();
            ImageOverlay.InvalidateVisual();
        }
    }
    private void ToggleAA(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            foreach (AntiAliasedShapeBase antiAliasedShapeBase in vm.Shapes.OfType<AntiAliasedShapeBase>())
            {
                antiAliasedShapeBase.IsAntialiased = !antiAliasedShapeBase.IsAntialiased;
                antiAliasedShapeBase.Draw(vm.Bitmap);
            }
        }
    }
    private void Image_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.KeyModifiers != KeyModifiers.Control)
        {
            e.Handled = true;
            return;
        }

        if (_scaleTransform is null)
            return;

        const double zoomFactor = 1.1;

        if (e.Delta.Y > 0)
        {
            _scaleTransform.ScaleX *= zoomFactor;
            _scaleTransform.ScaleY *= zoomFactor;
        }
        else if (e.Delta.Y < 0)
        {
            _scaleTransform.ScaleX /= zoomFactor;
            _scaleTransform.ScaleY /= zoomFactor;
        }
    }
    private void Image_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;

        if (vm.SelectedTool == Tool.Move)
        {
            vm.EndDrag();
        }

        _isDragging = false;
        _lastPointerPosition = null;
    }
    private void Image_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;

        if (e.KeyModifiers == KeyModifiers.Control)
        {
            if (_isDragging && _lastPointerPosition != null && _translateTransform != null)
            {
                var currentPosition = e.GetPosition((Visual?)sender!);
                var delta = currentPosition - _lastPointerPosition.Value;
                _lastPointerPosition = currentPosition;

                _translateTransform.X += delta.X;
                _translateTransform.Y += delta.Y;
            }
            e.Handled = true;
            return;
        }

        var point = e.GetPosition((Visual?)sender!);
        int x = (int)point.X;
        int y = (int)point.Y;

        if (vm.SelectedTool == Tool.Move)
        {
            if (e.GetCurrentPoint((Visual?)sender!).Properties.IsLeftButtonPressed && vm.SelectedShape != null)
            {
                vm.HandleDrag(x, y);
                ImageOverlay.InvalidateVisual();
                e.Handled = true;
                return;
            }
        }
        else
        {
            vm.ClearOverlay();
        }

        if (startPoint != null)
        {
            switch (vm.SelectedTool)
            {
                case Tool.Line:
                    var line = new Line
                    {
                        IsAntialiased = vm.IsAntialiased,
                        Thickness = vm.SelectedThickness,
                        Color = vm.SelectedColor,
                        Start = startPoint.Value,
                        End = (x, y),
                    };
                    line.Draw(vm.Overlay);
                    break;
                case Tool.Circle:
                    var circle = new Circle
                    {
                        Thickness = vm.SelectedThickness,
                        Color = vm.SelectedColor,
                        Center = (startPoint.Value.x, startPoint.Value.y),
                        Radius = (int)Math.Sqrt((x - startPoint.Value.x) * (x - startPoint.Value.x) + (y - startPoint.Value.y) * (y - startPoint.Value.y)),
                    };
                    circle.Draw(vm.Overlay);
                    break;
                case Tool.Rectangle:
                    var rectangle = new Rectangle
                    {
                        TopLeft = startPoint.Value,
                        BottomRight = (x, y),
                        IsAntialiased = vm.IsAntialiased,
                        Thickness = vm.SelectedThickness,
                        Color = vm.SelectedColor,
                    };
                    rectangle.Draw(vm.Overlay);
                    break;
                case Tool.Pill:
                    var pill = new Pill
                    {
                        Color = vm.SelectedColor,
                        CenterA = (startPoint.Value.x, startPoint.Value.y),
                        CenterB = (x, y),
                        Radius = 10,
                    };
                    pill.Draw(vm.Overlay);
                    break;
                case Tool.Polygon:
                    var FirstPoint = new Circle()
                    {
                        Center = (startPoint.Value.x, startPoint.Value.y),
                        Radius = 10,
                        Color = Colors.Red,
                    };
                    FirstPoint.Draw(vm.Overlay);
                    if (vm.TempPolygon != null)
                    {
                        var poly = new Polygon
                        {
                            Color = vm.SelectedColor,
                            IsAntialiased = vm.IsAntialiased,
                            Thickness = vm.SelectedThickness,
                        };
                        poly.Points.AddRange(vm.TempPolygon.Points);
                        poly.Points.Add((x, y));
                        poly.Draw(vm.Overlay);
                    }
                    break;
            }
        }
        ImageOverlay.InvalidateVisual();
    }
    private void Image_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;

        if (e.KeyModifiers == KeyModifiers.Control)
        {
            _isDragging = true;
            _lastPointerPosition = e.GetPosition((Visual?)sender!);

            e.Handled = true;
            return;
        }

        var point = e.GetPosition((Visual?)sender!);
        int x = (int)point.X;
        int y = (int)point.Y;

        if (vm.SelectedTool == Tool.Move)
        {
            vm.ClearOverlay();
            if (e.GetCurrentPoint((Visual?)sender!).Properties.IsRightButtonPressed)
            {
                vm.SelectShapeAt(x, y, e.KeyModifiers == KeyModifiers.Shift);
                if (vm.SelectedShape != null)
                {
                    vm.UpdateHitpoints();
                }
            }
            else if (vm.SelectedShape != null)
            {
                vm.StartDrag(x, y);
            }
            ImageOverlay.InvalidateVisual();
            e.Handled = true;
            return;
        }

        if (startPoint == null)
        {
            startPoint = (x, y);
            if (vm.SelectedTool == Tool.Polygon)
            {
                vm.TempPolygon = new Polygon
                {
                    Color = vm.SelectedColor,
                    IsAntialiased = vm.IsAntialiased,
                    Thickness = vm.SelectedThickness,
                };
                vm.TempPolygon.Points.Add((x, y));
            }
        }
        else
        {
            var (x1, y1) = startPoint.Value;

            switch (vm.SelectedTool)
            {
                case Tool.Line:
                    vm.AddShape(new Line
                    {
                        Color = vm.SelectedColor,
                        IsAntialiased = vm.IsAntialiased,
                        Thickness = vm.SelectedThickness,
                        Start = (x1, y1),
                        End = (x, y),
                    });
                    startPoint = null;
                    break;

                case Tool.Circle:
                    int radius = (int)Math.Sqrt((x - x1) * (x - x1) + (y - y1) * (y - y1));
                    vm.AddShape(new Circle
                    {
                        Color = vm.SelectedColor,
                        Thickness = vm.SelectedThickness,
                        Center = (x1, y1),
                        Radius = radius,
                    });
                    startPoint = null;
                    break;
                case Tool.Rectangle:
                    vm.AddShape(new Rectangle
                    {
                        IsAntialiased = vm.IsAntialiased,
                        Thickness = vm.SelectedThickness,
                        Color = vm.SelectedColor,
                        TopLeft = (x1, y1),
                        BottomRight = (x, y),
                    });
                    startPoint = null;
                    break;
                case Tool.Pill:
                    vm.AddShape(new Pill
                    {
                        Color = vm.SelectedColor,
                        CenterA = (x1, y1),
                        CenterB = (x, y),
                        Radius = 10,
                    });
                    startPoint = null;
                    break;
                case Tool.Polygon:
                    double dist = Math.Sqrt((x - x1) * (x - x1) + (y - y1) * (y - y1));

                    if (dist < 10 && vm.TempPolygon!.Points.Count >= 3)
                    {
                        // Close Polygon
                        vm.AddShape(vm.TempPolygon);
                        startPoint = null;
                        vm.TempPolygon = null;
                    }
                    else
                    {
                        vm.TempPolygon!.Points.Add((x, y));
                    }

                    break;
                default:
                    startPoint = null;
                    break;
            }
        }
        ImageCanvas.InvalidateVisual();
    }
    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await vm.SaveShapes(this);
        }
    }
    private async void Load_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await vm.LoadShapes(this);
        }
    }
}