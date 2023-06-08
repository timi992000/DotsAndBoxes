using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotsAndBoxes.Core.Entities
{
    public struct Line
    {
        public Line(Point point1, Point point2)
        {
            Point1 = point1;
            Point2 = point2;
            Player = -1;
        }

        public Point Point1 { get; set; }
        public Point Point2 { get; set; }
        public int Player { get; set; }
        public bool IsPointPairIsLine(Point point1, Point point2)
        {
            return Point1 == point1 && Point2 == point2 || Point1 == point2 && Point2 == point1;
        }
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (!(obj is Line))
                return false;
            var casted = (Line)obj;
            return IsPointPairIsLine(casted.Point1, casted.Point2);
        }
    }
}
