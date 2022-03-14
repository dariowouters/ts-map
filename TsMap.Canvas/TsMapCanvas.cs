using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TsMap.TsItem;

namespace TsMap.Canvas
{
    public partial class TsMapCanvas : Form
    {
        private readonly TsMapper _mapper;
        private readonly TsMapRenderer _renderer;
        private Settings _appSettings;

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
        private int mapPadding = 500;

        public TsMapCanvas(SetupForm f, string path, List<Mod> mods)
        {
            InitializeComponent();

            _appSettings = f.AppSettings;

            JsonHelper.SaveSettings(_appSettings);

            _mapper = new TsMapper(path, mods);
            _palette = new SimpleMapPalette();

            _mapper.Parse();

            CityStripComboBox.Items.AddRange(_mapper.Cities.Where(x => !x.Hidden).ToArray());

            if (!_mapper.IsEts2) _startPoint = new PointF(-105000, 15000);
            else _startPoint = new PointF(-1000, -4000);
            _renderer = new TsMapRenderer(_mapper);

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
                    MapPanel.Invalidate();
                    _startPoint.X -= (e.X - _lastPoint.X) / _scale;
                    _startPoint.Y -= (e.Y - _lastPoint.Y) / _scale;
                }
                _lastPoint = new PointF(e.X, e.Y);
            };

            MapPanel.MouseWheel += (s, e) =>
            {
                _scale += (e.Delta > 0 ? 1 : -1) * 0.05f * _scale;
                _scale = Math.Max(_scale, 0.0005f);
                MapPanel.Invalidate();
            };

            MapPanel.Resize += TsMapCanvas_Resize;

            Closed += (s, e) =>
            {
                f.Close();
                _tileMapGeneratorForm?.Close();
            };

        }

        private void SaveTileImage(int z, int x, int y, PointF pos, float zoom, string exportPath, RenderFlags renderFlags) // z = zoomLevel; x = row tile index; y = column tile index
        {
            using (var bitmap = new Bitmap(tileSize, tileSize))
            using (var g = Graphics.FromImage(bitmap))
            {
                pos.X = (x == 0) ? pos.X : pos.X + (bitmap.Width / zoom) * x; // get tile start coords
                pos.Y = (y == 0) ? pos.Y : pos.Y + (bitmap.Height / zoom) * y;

                _renderer.Render(g, new Rectangle(0, 0, bitmap.Width, bitmap.Height), zoom, pos, _palette,
                    renderFlags & ~RenderFlags.TextOverlay);

                Directory.CreateDirectory($"{exportPath}/Tiles/{z}/{x}");
                bitmap.Save($"{exportPath}/Tiles/{z}/{x}/{y}.png", ImageFormat.Png);
            }
        }
        private void ZoomOutAndCenterMap(float targetWidth, float targetHeight, out PointF pos, out float zoom)
        {
            var mapWidth = _mapper.maxX - _mapper.minX + mapPadding * 2;
            var mapHeight = _mapper.maxZ - _mapper.minZ + mapPadding * 2;
            if (mapWidth > mapHeight) // get the scale to have the map edge to edge on the biggest axis (with padding)
            {
                zoom = targetWidth / mapWidth;
                var z = _mapper.minZ - mapPadding + -(targetHeight / zoom) / 2f + mapHeight / 2f;
                pos = new PointF(_mapper.minX - mapPadding, z);
            }
            else
            {
                zoom = targetHeight / mapHeight;
                var x = _mapper.minX - mapPadding + -(targetWidth / zoom) / 2f + mapWidth / 2f;
                pos = new PointF(x, _mapper.minZ - mapPadding);
            }
        }

        private void GenerateTileMap(int startZoomLevel, int endZoomLevel, string exportPath, bool createTiles, bool saveInfo, RenderFlags renderFlags)
        {
            _appSettings.LastTileMapPath = exportPath;
            JsonHelper.SaveSettings(_appSettings);

            if (saveInfo || startZoomLevel == 0)
            {
                ZoomOutAndCenterMap(tileSize, tileSize, out PointF pos, out float zoom); // get zoom and start coords for tile level 0
                if (saveInfo)
                {
                    JsonHelper.SaveTileMapInfo(exportPath, pos.X, pos.X + tileSize / zoom, pos.Y,
                        pos.Y + tileSize / zoom, startZoomLevel, endZoomLevel);
                }
                if (startZoomLevel == 0 && createTiles)
                {
                    SaveTileImage(0, 0, 0, pos, zoom, exportPath, renderFlags);
                    startZoomLevel++;
                }
            }
            if (!createTiles) return;
            for (int z = startZoomLevel; z <= endZoomLevel; z++) // https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
            {
                ZoomOutAndCenterMap((int)Math.Pow(2, z) * tileSize, (int)Math.Pow(2, z) * tileSize, out PointF pos, out float zoom); // get zoom and start coords for current tile level
                for (int x = 0; x < Math.Pow(2, z); x++)
                {
                    for (int y = 0; y < Math.Pow(2, z); y++)
                    {
                        SaveTileImage(z, x, y, pos, zoom, exportPath, renderFlags);
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
                MapPanel.Invalidate();
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
            _localizationSettingsForm = new LocalizationSettingsForm(_mapper.Localization.GetLocales(), _mapper.Localization.SelectedLocalization);
            _localizationSettingsForm.Show();
            _localizationSettingsForm.BringToFront();

            _localizationSettingsForm.UpdateLocalization += (localeName) =>
            {

                _mapper.Localization.ChangeLocalization(localeName);
                CityStripComboBox.Items.Clear();
                CityStripComboBox.Items.AddRange(_mapper.Cities.Where(x => !x.Hidden).ToArray());
                _localizationSettingsForm.Close();
                MapPanel.Invalidate();
            };
        }

        private void GenerateTileMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_tileMapGeneratorForm == null || _tileMapGeneratorForm.IsDisposed) _tileMapGeneratorForm = new TileMapGeneratorForm(_appSettings.LastTileMapPath, _renderFlags);
            _tileMapGeneratorForm.Show();
            _tileMapGeneratorForm.BringToFront();

            _tileMapGeneratorForm.GenerateTileMap += (exportPath, startZoomLevel, endZoomLevel, createTiles, exportFlags, renderFlags) => // Called when export button is pressed in TileMapGeneratorForm
            {
                _tileMapGeneratorForm.Close();
                _appSettings.LastTileMapPath = exportPath;
                JsonHelper.SaveSettings(_appSettings);
                _mapper.ExportInfo(exportFlags, exportPath);

                if (startZoomLevel < 0 || endZoomLevel < 0) return;
                if (startZoomLevel > endZoomLevel)
                {
                    var tmp = startZoomLevel;
                    startZoomLevel = endZoomLevel;
                    endZoomLevel = tmp;
                }

                GenerateTileMap(startZoomLevel, endZoomLevel, exportPath, createTiles, (exportFlags & ExportFlags.TileMapInfo) == ExportFlags.TileMapInfo, renderFlags);
                MessageBox.Show("Tile map has been generated!", "TsMap - Tile Map Generation Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Focus();
            };
        }

        private void FullMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ZoomOutAndCenterMap(MapPanel.Width, MapPanel.Height, out _startPoint, out _scale);
            MapPanel.Invalidate();
        }

        private void ResetMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _scale = 0.2f;
            _startPoint = (!_mapper.IsEts2) ? new PointF(-105000, 15000) : new PointF(-1000, -4000);
            MapPanel.Invalidate();
        }

        private void CityStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var city = (TsCityItem)((ToolStripComboBox)sender).SelectedItem;
            _scale = 0.2f;
            _startPoint = new PointF(city.X - 1000, city.Z - 1000);
            MapPanel.Invalidate();
        }
    }
}
