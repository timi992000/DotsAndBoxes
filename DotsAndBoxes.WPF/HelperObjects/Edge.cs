using DotsAndBoxes.Core.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotsAndBoxes.WPF.HelperObjects
{
    public class Edge
    {
        public Point CanvasFrom;
        public Point CanvasTo;
        public Point SimulatedFrom;
        public Point SimulatedTo;
        public System.Windows.Shapes.Rectangle EdgeInstance;
        public ePlayer DrawedFrom;
    }
}
