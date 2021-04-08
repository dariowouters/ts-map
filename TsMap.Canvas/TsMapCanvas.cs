using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TsMap.TsItem;

namespace TsMap.Canvas {
    public partial class TsMapCanvas : Form {
        private readonly TsMapper      _mapper;
        private readonly TsMapRenderer _renderer;
        private readonly int           tileSize = 256;
        private readonly Settings      _appSettings;

        private bool                     _dragging;
        private ItemVisibilityForm       _itemVisibilityForm;
        private PointF                   _lastPoint;
        private LocalizationSettingsForm _localizationSettingsForm;

        private MapPalette        _palette;
        private PaletteEditorForm _paletteEditorForm;

        private RenderFlags _renderFlags = RenderFlags.All;
        private float       _scale       = 0.2f;
        private PointF      _startPoint;

        private          TileMapGeneratorForm _tileMapGeneratorForm;
        private readonly int                  mapPadding = 500;

        public TsMapCanvas( SetupForm f, string path, List< Mod > mods ) {
            this.InitializeComponent();

            this._appSettings = f.AppSettings;

            JsonHelper.SaveSettings( this._appSettings );

            this._mapper  = new TsMapper( path, mods );
            this._palette = new SimpleMapPalette();

            this._mapper.Parse();

            this.CityStripComboBox.Items.AddRange( this._mapper.Cities.Where( x => !x.Hidden ).ToArray() );

            if ( !this._mapper.IsEts2 )
                this._startPoint = new PointF( -105000, 15000 );
            else
                this._startPoint = new PointF( -1000, -4000 );
            this._renderer = new TsMapRenderer( this._mapper );

            // Panning around
            this.MapPanel.MouseDown += ( s, e ) => {
                this._dragging  = true;
                this._lastPoint = new PointF( e.X, e.Y );
            };
            this.MapPanel.MouseUp += ( s, e ) => this._dragging = false;
            this.MapPanel.MouseMove += ( s, e ) => {
                if ( this._dragging ) {
                    this.MapPanel.Invalidate();
                    this._startPoint.X -= ( e.X - this._lastPoint.X ) / this._scale;
                    this._startPoint.Y -= ( e.Y - this._lastPoint.Y ) / this._scale;
                }

                this._lastPoint = new PointF( e.X, e.Y );
            };

            this.MapPanel.MouseWheel += ( s, e ) => {
                this._scale += ( e.Delta > 0
                                     ? 1
                                     : -1 )
                               * 0.05f
                               * this._scale;
                this._scale = Math.Max( this._scale, 0.0005f );
                this.MapPanel.Invalidate();
            };

            this.MapPanel.Resize += this.TsMapCanvas_Resize;

            Closed += ( s, e ) => {
                f.Close();
                this._tileMapGeneratorForm?.Close();
            };
        }

        private void SaveTileImage( int z,
                                    int x,
                                    int y,
                                    PointF pos,
                                    float zoom,
                                    string exportPath,
                                    RenderFlags renderFlags ) // z = zoomLevel; x = row tile index; y = column tile index
        {
            using ( var bitmap = new Bitmap( this.tileSize, this.tileSize ) )
            using ( Graphics g = Graphics.FromImage( bitmap ) ) {
                pos.X = x == 0
                            ? pos.X
                            : pos.X + bitmap.Width / zoom * x; // get tile start coords
                pos.Y = y == 0
                            ? pos.Y
                            : pos.Y + bitmap.Height / zoom * y;

                this._renderer.Render( g, new Rectangle( 0, 0, bitmap.Width, bitmap.Height ), zoom, pos, this._palette,
                                       renderFlags & ~RenderFlags.TextOverlay );

                Directory.CreateDirectory( $"{exportPath}/Tiles/{z}/{x}" );
                bitmap.Save( $"{exportPath}/Tiles/{z}/{x}/{y}.png", ImageFormat.Png );
            }
        }

        private void ZoomOutAndCenterMap( float targetWidth, float targetHeight, out PointF pos, out float zoom ) {
            float mapWidth  = this._mapper.maxX - this._mapper.minX + this.mapPadding * 2;
            float mapHeight = this._mapper.maxZ - this._mapper.minZ + this.mapPadding * 2;
            if ( mapWidth > mapHeight ) // get the scale to have the map edge to edge on the biggest axis (with padding)
            {
                zoom = targetWidth / mapWidth;
                float z = this._mapper.minZ         - this.mapPadding + -( targetHeight / zoom ) / 2f + mapHeight / 2f;
                pos = new PointF( this._mapper.minX - this.mapPadding, z );
            } else {
                zoom = targetHeight / mapHeight;
                float x = this._mapper.minX            - this.mapPadding + -( targetWidth / zoom ) / 2f + mapWidth / 2f;
                pos = new PointF( x, this._mapper.minZ - this.mapPadding );
            }
        }

        private void GenerateTileMap( int         startZoomLevel,
                                      int         endZoomLevel,
                                      string      exportPath,
                                      bool        createTiles,
                                      bool        saveInfo,
                                      RenderFlags renderFlags ) {
            this._appSettings.LastTileMapPath = exportPath;
            JsonHelper.SaveSettings( this._appSettings );

            if ( saveInfo || startZoomLevel == 0 ) {
                this.ZoomOutAndCenterMap( this.tileSize, this.tileSize, out PointF pos,
                                          out float zoom ); // get zoom and start coords for tile level 0
                if ( saveInfo )
                    JsonHelper.SaveTileMapInfo( exportPath, pos.X, pos.X + this.tileSize / zoom, pos.Y,
                                                pos.Y + this.tileSize / zoom, startZoomLevel, endZoomLevel,
                                                this._mapper.gameVersion );
                if ( startZoomLevel == 0 && createTiles ) {
                    this.SaveTileImage( 0, 0, 0, pos, zoom, exportPath, renderFlags );
                    startZoomLevel++;
                }
            }

            if ( !createTiles ) return;
            for ( int z = startZoomLevel;
                  z <= endZoomLevel;
                  z++ ) // https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
            {
                this.ZoomOutAndCenterMap( (int) Math.Pow( 2, z ) * this.tileSize,
                                          (int) Math.Pow( 2, z ) * this.tileSize, out PointF pos,
                                          out float zoom ); // get zoom and start coords for current tile level
                for ( var x = 0; x < Math.Pow( 2, z ); x++ )
                for ( var y = 0; y < Math.Pow( 2, z ); y++ )
                    this.SaveTileImage( z, x, y, pos, zoom, exportPath, renderFlags );
            }
        }

        private void TsMapCanvas_Resize( object sender, EventArgs e ) {
            this.MapPanel.Invalidate();
        }

        private void ExitToolStripMenuItem_Click( object sender, EventArgs e ) {
            Application.Exit();
        }

        private void ItemVisibilityToolStripMenuItem_Click( object sender, EventArgs e ) {
            this._itemVisibilityForm = new ItemVisibilityForm( this._renderFlags );
            this._itemVisibilityForm.Show();
            this._itemVisibilityForm.BringToFront();

            this._itemVisibilityForm.UpdateItemVisibility += renderFlags => {
                this._renderFlags = renderFlags;
                this.MapPanel.Invalidate();
            };
        }

        private void paletteToolStripMenuItem_Click( object sender, EventArgs e ) {
            this._paletteEditorForm = new PaletteEditorForm( this._palette );
            this._paletteEditorForm.Show();
            this._paletteEditorForm.BringToFront();

            this._paletteEditorForm.UpdatePalette += palette => {
                this._palette = palette;
            };
        }

        private void MapPanel_Paint( object sender, PaintEventArgs e ) {
            this._renderer.Render( e.Graphics, e.ClipRectangle, this._scale, this._startPoint, this._palette,
                                   this._renderFlags );
        }

        private void localizationSettingsToolStripMenuItem_Click( object sender, EventArgs e ) {
            this._localizationSettingsForm =
                new LocalizationSettingsForm( this._mapper.LocalizationList, this._mapper.SelectedLocalization );
            this._localizationSettingsForm.Show();
            this._localizationSettingsForm.BringToFront();

            this._localizationSettingsForm.UpdateLocalization += locIndex => {
                this._mapper.UpdateLocalization( locIndex );
                this.CityStripComboBox.Items.Clear();
                this.CityStripComboBox.Items.AddRange( this._mapper.Cities.Where( x => !x.Hidden ).ToArray() );
                this._localizationSettingsForm.Close();
                this.MapPanel.Invalidate();
            };
        }

        private void GenerateTileMapToolStripMenuItem_Click( object sender, EventArgs e ) {
            if ( this._tileMapGeneratorForm == null || this._tileMapGeneratorForm.IsDisposed )
                this._tileMapGeneratorForm =
                    new TileMapGeneratorForm( this._appSettings.LastTileMapPath, this._renderFlags );
            this._tileMapGeneratorForm.Show();
            this._tileMapGeneratorForm.BringToFront();

            this._tileMapGeneratorForm.GenerateTileMap +=
                ( exportPath,
                  startZoomLevel,
                  endZoomLevel,
                  createTiles,
                  exportFlags,
                  renderFlags ) => // Called when export button is pressed in TileMapGeneratorForm
                {
                    this._tileMapGeneratorForm.Close();
                    this._appSettings.LastTileMapPath = exportPath;
                    JsonHelper.SaveSettings( this._appSettings );
                    this._mapper.ExportInfo( exportFlags, exportPath );

                    if ( startZoomLevel < 0 || endZoomLevel < 0 ) return;
                    if ( startZoomLevel > endZoomLevel ) {
                        int tmp = startZoomLevel;
                        startZoomLevel = endZoomLevel;
                        endZoomLevel   = tmp;
                    }

                    this.GenerateTileMap( startZoomLevel, endZoomLevel, exportPath, createTiles,
                                          ( exportFlags & ExportFlags.TileMapInfo ) == ExportFlags.TileMapInfo,
                                          renderFlags );
                    MessageBox.Show( "Tile map has been generated!", "TsMap - Tile Map Generation Finished",
                                     MessageBoxButtons.OK, MessageBoxIcon.Information );
                    this.Focus();
                };
        }

        private void FullMapToolStripMenuItem_Click( object sender, EventArgs e ) {
            this.ZoomOutAndCenterMap( this.MapPanel.Width, this.MapPanel.Height, out this._startPoint,
                                      out this._scale );
            this.MapPanel.Invalidate();
        }

        private void ResetMapToolStripMenuItem_Click( object sender, EventArgs e ) {
            this._scale = 0.2f;
            this._startPoint = !this._mapper.IsEts2
                                   ? new PointF( -105000, 15000 )
                                   : new PointF( -1000,   -4000 );
            this.MapPanel.Invalidate();
        }

        private void CityStripComboBox_SelectedIndexChanged( object sender, EventArgs e ) {
            var city = (TsCityItem) ( (ToolStripComboBox) sender ).SelectedItem;
            this._scale      = 0.2f;
            this._startPoint = new PointF( city.X - 1000, city.Z - 1000 );
            this.MapPanel.Invalidate();
        }
    }
}