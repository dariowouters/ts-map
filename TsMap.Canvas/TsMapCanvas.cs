using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TsMap.Canvas
{
    public partial class TsMapCanvas : Form
    {
        private readonly TsMapRenderer _renderer;

        private PointF _pos;
        private Point? _dragPoint;

        private float _mapScale = 4000;

        public TsMapCanvas(Form f, string path, List<Mod> mods)
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            var mapper = new TsMapper(path, mods);

            if (path.Contains("American Truck Simulator"))
            {
                _pos = new PointF(-103000, -54444);
                mapper.IsEts2 = false;
            }
            else
            {
                _pos = new PointF(850, -920);
                mapper.IsEts2 = true;
            }

            mapper.Parse();

            _renderer = new TsMapRenderer(mapper, new SimpleMapPalette());

            Timer t = new Timer
            {
                Interval = 1000 / 30
            };
            t.Tick += (s, a) => Invalidate();
            t.Start();

            // Panning around
            MouseDown += (s, e) => _dragPoint = e.Location;
            MouseUp += (s, e) => _dragPoint = null;
            MouseMove += (s, e) =>
            {
                if (_dragPoint == null) return;
                var spd = _mapScale / Math.Max(this.Width, this.Height);
                _pos.X = _pos.X - (e.X - _dragPoint.Value.X) * spd;
                _pos.Y = _pos.Y - (e.Y - _dragPoint.Value.Y) * spd;
                _dragPoint = e.Location;
            };

            MouseWheel += TsMapCanvas_MouseWheel;

            Resize += TsMapCanvas_Resize;

            Closed += (s, e) => { f.Close(); };

        }

        private void TsMapCanvas_MouseWheel(object sender, MouseEventArgs e)
        {
            _mapScale -= e.Delta;
            _mapScale = Math.Max(100, Math.Min(60000, _mapScale));
        }

        private void TsMapCanvas_Resize(object sender, System.EventArgs e)
        {
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            _renderer.Render(e.Graphics, e.ClipRectangle, _mapScale, _pos);
            base.OnPaint(e);
        }

    }
}
