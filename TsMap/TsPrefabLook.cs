using System.Collections.Generic;
using System.Drawing;

namespace TsMap
{
    public abstract class TsPrefabLook
    {
        public int ZIndex { get; set; }
        public Brush Color { get; set; }
        protected readonly List<PointF> Points;

        protected TsPrefabLook(List<PointF> points)
        {
            Points = points;
        }

        protected TsPrefabLook() : this(new List<PointF>()) { }

        public void AddPoint(PointF p)
        {
            Points.Add(p);
        }

        public void AddPoint(float x, float y)
        {
            AddPoint(new PointF(x, y));
        }

        public abstract void Draw(Graphics g);
    }

    public class TsPrefabRoadLook : TsPrefabLook
    {
        public float Width { private get; set; }

        public TsPrefabRoadLook()
        {
            ZIndex = 1;
        }

        public override void Draw(Graphics g)
        {
            g.DrawLines(new Pen(Color, Width), Points.ToArray());
        }
    }

    public class TsPrefabPolyLook : TsPrefabLook
    {
        public TsPrefabPolyLook(List<PointF> points) : base(points) { }

        public override void Draw(Graphics g)
        {
            g.FillPolygon(Color, Points.ToArray());
        }
    }
}
