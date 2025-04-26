using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using DrawingAppCG.Models;
using DrawingAppCG.ViewModels;
using System;

namespace DrawingAppCG.Views;

public partial class MainWindow : Window
{
    private (int x, int y)? startPoint = null;

    public MainWindow()
    {
        InitializeComponent();
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
    private void SelectPolygonTool(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SelectedTool = Tool.Polygon;
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
        }
    }

    private void Image_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;

        var point = e.GetPosition((Visual?)sender!);
        int x = (int)point.X;
        int y = (int)point.Y;

        if (startPoint == null)
        {
            startPoint = (x, y);
            if (vm.SelectedTool == Tool.Polygon)
            {
                vm.TempPolygon = new Polygon
                {
                    Thickness = vm.SelectedThickness,
                };
                vm.TempPolygon.Points.Add((x, y));
                var hitPoint = new Circle()
                {
                    CenterX = x,
                    CenterY = y,
                    Radius = 10,
                    Color = Colors.Red,
                };
                hitPoint.Draw(vm.Bitmap);
            }
        }
        else
        {
            // Second click — create shape
            var (x1, y1) = startPoint.Value;

            switch (vm.SelectedTool)
            {
                case Tool.Line:
                    vm.AddShape(new Line
                    {
                        Thickness = vm.SelectedThickness,
                        Start = (x1, y1),
                        End = (x, y),
                        //X1 = x1,
                        //Y1 = y1,
                        //X2 = x,
                        //Y2 = y,
                    });
                    startPoint = null;
                    break;

                case Tool.Circle:
                    int radius = (int)Math.Sqrt((x - x1) * (x - x1) + (y - y1) * (y - y1));
                    vm.AddShape(new Circle
                    {
                        Thickness = vm.SelectedThickness,
                        CenterX = x1,
                        CenterY = y1,
                        Radius = radius,
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
                        vm.RedrawAll();
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
}