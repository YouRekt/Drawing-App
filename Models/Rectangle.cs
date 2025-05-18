using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DrawingAppCG.Models
{
    public class Rectangle : Polygon
    {
        public (int x, int y) TopLeft { get; set; }
        public (int x, int y) BottomRight { get; set; }
        [JsonIgnore]
        public (int x, int y) BottomLeft => (TopLeft.x, BottomRight.y);
        [JsonIgnore]
        public (int x, int y) TopRight => (BottomRight.x, TopLeft.y);
        [JsonIgnore]
        public override List<(int x, int y)> Points { get => [TopLeft, TopRight, BottomRight, BottomLeft]; }
        public override void Move(int deltaX, int deltaY)
        {
            TopLeft = (TopLeft.x + deltaX, TopLeft.y + deltaY);
            BottomRight = (BottomRight.x + deltaX, BottomRight.y + deltaY);
        }
        public override void MovePoint(int pointIndex, int newX, int newY)
        {
            switch (pointIndex)
            {
                case 0:
                    TopLeft = (newX, newY);
                    break;
                case 1:
                    TopLeft = (TopLeft.x, newY);
                    BottomRight = (newX, BottomRight.y);
                    break;
                case 2:
                    BottomRight = (newX, newY);
                    break;
                case 3:
                    TopLeft = (newX, TopLeft.y);
                    BottomRight = (BottomRight.x, newY);
                    break;
            }
        }
    }
}
