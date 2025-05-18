using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using static DrawingAppCG.Models.GeometryUtils;

namespace DrawingAppCG.Models
{
    [JsonDerivedType(typeof(Rectangle), "Rectangle")]
    public class Polygon : AntiAliasedShapeBase
    {
        public virtual List<(int x, int y)> Points { get; set; } = [];
        [JsonIgnore]
        public bool IsConvex => IsConvex(Points);
        public bool IsClipping { get; set; } = false;
        public Guid? ClipId { get; set; }
        [JsonIgnore]
        public Polygon? Clip { get; set; }
        private ((int x, int y) start, (int x, int y) end)? CyrusBeck((int x, int y) P0, (int x, int y) P1)
        {
            double tE = 0, tL = 1;
            (int x, int y) D = (P1.x - P0.x, P1.y - P0.y);
            var ClipPoints = Clip!.Points;
            for (int i = 0; i < ClipPoints.Count; i++)
            {
                var E0 = ClipPoints[i];
                var E1 = ClipPoints[(i + 1) % ClipPoints.Count];
                (double x, double y) PE = ((E0.x + E1.x) / 2.0, (E0.y + E1.y) / 2.0);
                var N = CalculateNormal(E0, E1);
                var numerator = DotProduct(N, (P0.x - PE.x, P0.y - PE.y));
                var denominator = DotProduct(N, D);
                if (denominator == 0)
                {
                    if (numerator > 0) return null;
                }
                else
                {
                    var t = numerator / -denominator;

                    if (denominator < 0) // PE
                    {
                        tE = Math.Max(t, tE);
                    }
                    else // PL
                    {
                        tL = Math.Min(t, tL);
                    }
                }
            }
            if (tE > tL) return null;

            (int x, int y) start = ((int)(P0.x + D.x * tE), (int)(P0.y + D.y * tE));
            (int x, int y) end = ((int)(P0.x + D.x * tL), (int)(P0.y + D.y * tL));

            return (start, end);

            static (double x, double y) CalculateNormal((int x, int y) p0, (int x, int y) p1)
            {
                return (-(p1.y - p0.y), p1.x - p0.x);
            }
        }
        public override void Draw(WriteableBitmap bitmap)
        {
            if (Clip != null)
            {
                for (int i = 0; i < Points.Count; i++)
                {
                    var P0 = Points[i];
                    var P1 = Points[(i + 1) % Points.Count];

                    var clipped = CyrusBeck(P0, P1);

                    if (clipped != null)
                    {
                        if (!(clipped.Value.start.x == P0.x && clipped.Value.start.y == P0.y))
                        {
                            var start = new Line
                            {
                                Start = P0,
                                End = clipped.Value.start,
                                Color = Colors.Red,
                                Thickness = Thickness,
                                IsAntialiased = IsAntialiased
                            };
                            start.Draw(bitmap);
                        }
                        if (!(clipped.Value.end.x == P1.x && clipped.Value.end.y == P1.y))
                        {
                            var end = new Line
                            {
                                Start = clipped.Value.end,
                                End = P1,
                                Color = Colors.Red,
                                Thickness = Thickness,
                                IsAntialiased = IsAntialiased
                            };
                            end.Draw(bitmap);
                        }
                        var line = new Line
                        {
                            Start = clipped.Value.start,
                            End = clipped.Value.end,
                            Color = Colors.LimeGreen,
                            Thickness = Thickness,
                            IsAntialiased = IsAntialiased
                        };
                        line.Draw(bitmap);
                    }
                    else
                    {
                        var line = new Line
                        {
                            Start = P0,
                            End = P1,
                            Color = Colors.Red,
                            Thickness = Thickness,
                            IsAntialiased = IsAntialiased
                        };
                        line.Draw(bitmap);
                    }
                }
            }
            else
            {
                for (int i = 0; i < Points.Count; i++)
                {
                    var P0 = Points[i];
                    var P1 = Points[(i + 1) % Points.Count];
                    var line = new Line
                    {
                        Start = P0,
                        End = P1,
                        Color = IsClipping ? IsConvex ? Color : Colors.Red : Color,
                        Thickness = Thickness,
                        IsAntialiased = IsAntialiased
                    };
                    line.Draw(bitmap);
                }
            }
        }
        public override bool ContainsPoint(int x, int y)
        {
            foreach (var point in Points)
            {
                if (Math.Abs(point.x - x) < 5 && Math.Abs(point.y - y) < 5)
                    return true;
            }

            for (int i = 0; i < Points.Count; i++)
            {
                var p1 = Points[i];
                var p2 = Points[(i + 1) % Points.Count];
                if (Line.DistanceToLine(x, y, p1.x, p1.y, p2.x, p2.y) <= 5)
                    return true;
            }
            return false;
        }
        public override void Move(int deltaX, int deltaY)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] = (Points[i].x + deltaX, Points[i].y + deltaY);
            }
        }
        public override void MovePoint(int pointIndex, int newX, int newY)
        {
            if (pointIndex >= 0 && pointIndex < Points.Count)
                Points[pointIndex] = (newX, newY);
        }
        public override List<(int x, int y)> GetControlPoints() => Points;
    }
}
