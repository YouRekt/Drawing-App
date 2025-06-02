using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DrawingAppCG.Models
{
    public class Cube : ShapeBase
    {
        public (int x, int y) Center;
        public int Distance = 400;
        public int Size = 100;
        public float Alpha = 30.0f;
        public float Beta = 30.0f;

        private readonly Vector4[] Vertices =
        [
            new Vector4(-1, -1, -1, 1),
            new Vector4( 1, -1, -1, 1),
            new Vector4( 1,  1, -1, 1),
            new Vector4(-1,  1, -1, 1),
            new Vector4(-1, -1,  1, 1),
            new Vector4( 1, -1,  1, 1),
            new Vector4( 1,  1,  1, 1),
            new Vector4(-1,  1,  1, 1)
        ];

        private readonly (int, int)[] Edges =
        [
            (0, 1), (1, 2), (2, 3), (3, 0), // back face
            (4, 5), (5, 6), (6, 7), (7, 4), // front face
            (0, 4), (1, 5), (2, 6), (3, 7)  // side edges
        ];

        public override void Draw(WriteableBitmap bitmap)
        {
            int width = bitmap.PixelSize.Width;
            int height = bitmap.PixelSize.Height;

            var rotationX = new Matrix4x4(
                1, 0, 0, 0,
                0, MathF.Cos(Alpha), -MathF.Sin(Alpha), 0,
                0, MathF.Sin(Alpha), MathF.Cos(Alpha), 0,
                0, 0, 0, 1);

            var rotationY = new Matrix4x4(
                MathF.Cos(Beta), 0, MathF.Sin(Beta), 0,
                0, 1, 0, 0,
                -MathF.Sin(Beta), 0, MathF.Cos(Beta), 0,
                0, 0, 0, 1);
           
            var scaleMatrix = Matrix4x4.CreateScale(Size);

            var translateMatrix = Matrix4x4.CreateTranslation(new Vector3(0, 0, Distance));

            var modelMatrix = scaleMatrix * rotationX * rotationY * translateMatrix;

            float d = Distance;
            Matrix4x4 projection = new(
                d, 0, 0, 0,
                0, d, 0, 0,
                0, 0, 0, 1,
                0, 0, 1, 0);

            var projected = new Vector2[Vertices.Length];
            for (int i = 0; i < Vertices.Length; i++)
            {
                var world = Vector4.Transform(Vertices[i], modelMatrix);
                var projected4D = Vector4.Transform(world, projection);

                float x2D = projected4D.X / projected4D.W + Center.x;
                float y2D = projected4D.Y / projected4D.W + Center.y;

                projected[i] = new Vector2(x2D, y2D);
            }

            foreach (var (i1, i2) in Edges)
            {
                var line = new Line
                {
                    Start = ((int)projected[i1].X, (int)projected[i1].Y),
                    End = ((int)projected[i2].X, (int)projected[i2].Y)
                };
                line.Draw(bitmap);
            }
        }
        public override bool ContainsPoint(int x, int y)
        {
            double distance = Math.Sqrt(Math.Pow(x - Center.x, 2) + Math.Pow(y - Center.y, 2));
            return distance <= 50;
        }
        public override List<(int x, int y)> GetControlPoints() => [Center];
        public override void Move(int deltaX, int deltaY)
        {
            Center = (Center.x + deltaX, Center.y + deltaY);
        }
        public override void MovePoint(int pointIndex, int newX, int newY)
        {
            if (pointIndex == 0)
                Center = (newX, newY);
        }
    }
}
