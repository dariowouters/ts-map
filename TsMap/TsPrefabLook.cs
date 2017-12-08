using System.Collections.Generic;
using System.Drawing;

namespace TsMap
{
    public enum TsPrefabLookType
    {
        Road,
        Poly
    }

    public class TsPrefabLook
    {
        public int ZIndex { get; set; }
        public TsPrefabLookType Type { get; set; }
        public Brush Color { get; set; }
        public float Width { get; set; }
        private readonly List<PointF> _points;

        public TsPrefabLook(List<PointF> points)
        {
            _points = points;
        }

        public TsPrefabLook() : this(new List<PointF>())
        {

        }

        public void AddPoint(PointF p)
        {
            _points.Add(p);
        }

        public void AddPoint(float x, float y)
        {
            AddPoint(new PointF(x, y));
        }

        public PointF[] GetPoints()
        {
            return _points.ToArray();
        }
    }
}
