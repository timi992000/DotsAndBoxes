using System.Drawing;

namespace DotsAndBoxes.Core.Entities
{
    public struct Boxes
    {
        public Boxes(Point upperLeft, Point upperRight, Point lowerLeft, Point lowerRight)
        {
            Lines = new Line[4];
            Lines[0] = new Line(upperLeft, lowerLeft);
            Lines[1] = new Line(lowerLeft, lowerRight);
            Lines[2] = new Line(upperRight, lowerRight);
            Lines[3] = new Line(upperLeft, upperRight);
            Player = -1;
        }
        public Line[] Lines { get; set; }
        public int Player { get; set; }
        public bool AffectByPointPair(Point point1, Point point2)
        {
            return Lines.Any(x => x.IsPointPairIsLine(point1, point2));
        }
        public void SetPlayerByLine(Point point1, Point point2, int player)
        {
            var lines = Lines.Where(x => x.IsPointPairIsLine(point1, point2) && x.Player == -1);
            for(int i = 0; i < Lines.Length; i++)
            {
                if (lines.Count() == 0)
                    return;
                if (Lines[i].Equals(lines.First()))
                    Lines[i].Player = player;
            }
        }
        public bool IsCompleted()
        {
            return Lines.All(x => x.Player != -1);
        }
    }
}
