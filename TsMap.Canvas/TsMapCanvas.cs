using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace TsMap.Canvas
{
    public partial class TsMapCanvas : Form
    {
        private readonly TsMapper _mapper;
        private readonly TsMapRenderer _renderer;

        private TileMapGeneratorForm _tileMapGeneratorForm;
        private ItemVisibilityForm _itemVisibilityForm;
        private PaletteEditorForm _paletteEditorForm;
        private LocalizationSettingsForm _localizationSettingsForm;

        private RenderFlags _renderFlags = RenderFlags.All;

        private MapPalette _palette;

        private bool _dragging;
        private PointF _lastPoint;
        private PointF _startPoint;
        private float _scale = 0.2f;
        private readonly int tileSize = 256;

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
                _tileMapGeneratorForm?.Close();
            };

        }

        private void SaveTileImage(int z, int x, int y, PointF pos, float zoom, string exportPath) // z = zoomLevel; x = row tile index; y = column tile index
        {
            var bitmap = new Bitmap(tileSize, tileSize);

            pos.X = (x == 0) ? pos.X : pos.X + (bitmap.Width / zoom) * x; // get tile start coords
            pos.Y = (y == 0) ? pos.Y : pos.Y + (bitmap.Height / zoom) * y;

            _renderer.Render(Graphics.FromImage(bitmap), new Rectangle(0, 0, bitmap.Width, bitmap.Height), zoom, pos, _palette, _renderFlags ^ RenderFlags.TextOverlay);

            Directory.CreateDirectory($"{exportPath}/{z}/{x}");
            bitmap.Save($"{exportPath}/{z}/{x}/{y}.png", ImageFormat.Png);
            bitmap.Dispose();
        }
        private void ZoomOutAndCenterMap(float targetWidth, float targetHeight, out PointF pos, out float zoom)
        {
            if (_mapper.maxX - _mapper.minX > _mapper.maxZ - _mapper.minZ) // get the scale to have the map edge to edge on the biggest axis
            {
                zoom = targetWidth / (_mapper.maxX - _mapper.minX);
                var z = _mapper.minZ + -(targetHeight / zoom) / 2f + (_mapper.maxZ - _mapper.minZ) / 2f;
                pos = new PointF(_mapper.minX, z);
            }
            else
            {
                zoom = targetHeight / (_mapper.maxZ - _mapper.minZ);
                var x = _mapper.minX + -(targetWidth / zoom) / 2f + (_mapper.maxX - _mapper.minX) / 2f;
                pos = new PointF(x, _mapper.minZ);
            }
        }

        private void GenerateTileMap(int zoomLevel, string exportPath)
        {
            ZoomOutAndCenterMap(tileSize, tileSize, out PointF pos, out float zoom); // get zoom and start coords for tile level 0
            SaveTileImage(0, 0, 0, pos, zoom, exportPath);

            for (int z = 1; z <= zoomLevel; z++) // https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
            {
                ZoomOutAndCenterMap((int)Math.Pow(2, z) * tileSize, (int)Math.Pow(2, z) * tileSize, out pos, out zoom); // get zoom and start coords for current tile level
                for (int x = 0; x < Math.Pow(2, z); x++)
                {
                    for (int y = 0; y < Math.Pow(2, z); y++)
                    {
                        SaveTileImage(z, x, y, pos, zoom, exportPath);
                    }
                }
            }
        }

        private void TsMapCanvas_Resize(object sender, System.EventArgs e)
        {
            MapPanel.Invalidate();
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

        private void GenerateTileMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_tileMapGeneratorForm == null || _tileMapGeneratorForm.IsDisposed) _tileMapGeneratorForm = new TileMapGeneratorForm();
            _tileMapGeneratorForm.Show();
            _tileMapGeneratorForm.BringToFront();

            _tileMapGeneratorForm.GenerateTileMap += (zoomLevel) => // Called when export button is pressed in TileMapGeneratorForm
            {
                if (zoomLevel < 0) return;
                folderBrowserDialog1.Description = "Select where you want the tile map files to be placed";
                _tileMapGeneratorForm.Hide();
                var res = folderBrowserDialog1.ShowDialog();
                if (res == DialogResult.OK)
                {
                    if (!Directory.Exists(folderBrowserDialog1.SelectedPath)) return;
                    GenerateTileMap(zoomLevel, folderBrowserDialog1.SelectedPath);
                    MessageBox.Show("Tile map has been generated!", "TsMap - Tile Map Generation Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                Focus();
            };
        }
    }
}
