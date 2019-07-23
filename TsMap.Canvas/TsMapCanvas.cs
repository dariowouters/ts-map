using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace TsMap.Canvas
{
    public partial class TsMapCanvas : Form
    {
        private readonly TsMapper _mapper;
        private readonly TsMapRenderer _renderer;

        private ImageExportOptionForm _imageExportForm;
        private ItemVisibilityForm _itemVisibilityForm;
        private PaletteEditorForm _paletteEditorForm;
        private LocalizationSettingsForm _localizationSettingsForm;

        private RenderFlags _renderFlags = RenderFlags.All;

        private MapPalette _palette;

        private bool _dragging;
        private PointF _lastPoint;
        private PointF _startPoint;
        private float _scale = 0.2f;

        public TsMapCanvas(Form f, string path, List<Mod> mods)
        {
            InitializeComponent();

            _mapper = new TsMapper(path, mods);
            _palette = new SimpleMapPalette();

            if (path.Contains("American Truck Simulator"))
            {
                _startPoint = new PointF(-103000, -54444);
                _mapper.IsEts2 = false;
            }
            else
            {
                _startPoint = new PointF(-1000, -4000);
                _mapper.IsEts2 = true;
            }

            _mapper.Parse();

            _renderer = new TsMapRenderer(_mapper);

            Timer t = new Timer
            {
                Interval = 1000 / 30
            };
            t.Tick += (s, a) => MapPanel.Invalidate();
            t.Start();

            // Panning around
            MapPanel.MouseDown += (s, e) =>
            {
                _dragging = true;
                _lastPoint = new PointF(e.X, e.Y);
            };
            MapPanel.MouseUp += (s, e) => _dragging = false;
            MapPanel.MouseMove += (s, e) =>
            {
                if (_dragging)
                {
                    _startPoint.X -= (e.X - _lastPoint.X) / _scale;
                    _startPoint.Y -= (e.Y - _lastPoint.Y) / _scale;
                }
                _lastPoint = new PointF(e.X, e.Y);
            };

            MapPanel.MouseWheel += (s, e) =>
            {
                _scale += (e.Delta > 0 ? 1 : -1) * 0.05f * _scale;
                _scale = Math.Max(_scale, 0.0005f);
            };

            MapPanel.Resize += TsMapCanvas_Resize;

            Closed += (s, e) =>
            {
                f.Close();
                _imageExportForm?.Close();
            };

        }

        private void TsMapCanvas_Resize(object sender, System.EventArgs e)
        {
            MapPanel.Invalidate();
        }

        private void ExportImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_imageExportForm == null || _imageExportForm.IsDisposed) _imageExportForm = new ImageExportOptionForm();
            _imageExportForm.Show();
            _imageExportForm.BringToFront();

            _imageExportForm.ExportImage += (width, height) => // Called when export button is pressed in ImageExportOptionForm
            {
                if (width == 0 || height == 0) return;
                var bitmap = new Bitmap(width, height);

                _renderer.Render(Graphics.FromImage(bitmap), new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    _scale, _startPoint, _palette, _renderFlags);

                var result = exportFileDialog.ShowDialog();
                if (result != DialogResult.OK) return;

                var fileStream = exportFileDialog.OpenFile();

                bitmap.Save(fileStream, ImageFormat.Png);
                fileStream.Close();
                _imageExportForm.Hide();
            };
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ItemVisibilityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _itemVisibilityForm = new ItemVisibilityForm(_renderFlags);
            _itemVisibilityForm.Show();
            _itemVisibilityForm.BringToFront();

            _itemVisibilityForm.UpdateItemVisibility += (renderFlags) =>
            {
                _renderFlags = renderFlags;
            };
        }

        private void paletteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _paletteEditorForm = new PaletteEditorForm(_palette);
            _paletteEditorForm.Show();
            _paletteEditorForm.BringToFront();

            _paletteEditorForm.UpdatePalette += (palette) =>
            {
                _palette = palette;
            };
        }

        private void MapPanel_Paint(object sender, PaintEventArgs e)
        {
            _renderer.Render(e.Graphics, e.ClipRectangle, _scale, _startPoint, _palette, _renderFlags);
        }

        private void localizationSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _localizationSettingsForm = new LocalizationSettingsForm(_mapper.LocalizationList, _mapper.SelectedLocalization);
            _localizationSettingsForm.Show();
            _localizationSettingsForm.BringToFront();

            _localizationSettingsForm.UpdateLocalization += (locIndex) =>
            {
                _mapper.UpdateLocalization(locIndex);
                _localizationSettingsForm.Close();
            };
        }
    }
}
