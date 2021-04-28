using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using TsMap.TsItem;

namespace TsMap.Canvas {
    public partial class TsMapCanvas : Form {
        private readonly TsMapper                 _mapper;
        private readonly TsMapRenderer            _renderer;
        private          bool                     _dragging;
        private          ItemVisibilityForm       _itemVisibilityForm;
        private          PointF                   _lastPoint;
        private          LocalizationSettingsForm _localizationSettingsForm;

        private MapPalette        _palette;
        private PaletteEditorForm _paletteEditorForm;

        private RenderFlags _renderFlags = RenderFlags.All;
        private float       _scale       = 0.2f;
        private PointF      _startPoint;

        private TileMapGeneratorForm _tileMapGeneratorForm;
        private TsMapper             _tilesGeneratorMapper;
        private TsMapRenderer        _tilesGeneratorRenderer;

        private float movement;

        private PointF oldPos = new PointF( 0, 0 );

        private int savedZoomLevel;

        public TsMapCanvas( SetupForm f ) {
            this.InitializeComponent();

            //_appSettings = f.AppSettings;

            //JsonHelper.SaveSettings(_appSettings);

            this._mapper  = this.CreateMapper();
            this._palette = SettingsManager.Current.Settings.Palette.ToBrushPalette();

            this._mapper.Parse();

            this.CityStripComboBox.Items.AddRange( this._mapper.Cities.Where( x => !x.Hidden ).ToArray() );

            if ( !this._mapper.IsEts2 )
                this._startPoint = new PointF( -105000, 15000 );
            else
                this._startPoint = new PointF( -1000, -4000 );
            this._renderer = this.CreateRenderer( this._mapper );

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

            Version appVersion = Assembly.GetExecutingAssembly().GetName().Version;
            this.version.Text = $@"Version: v{appVersion}";
        }

        private TsMapper CreateMapper() => new TsMapper( SettingsManager.Current.Settings.LastGamePath, SettingsManager.Current.Settings.Mods );

        private TsMapRenderer CreateRenderer( TsMapper mapper ) => new TsMapRenderer( mapper );

        private void SaveTileImage( int         z,
                                    int         x,
                                    int         y,
                                    PointF      pos,
                                    float       zoom,
                                    string      exportPath,
                                    RenderFlags renderFlags ) // z = zoomLevel; x = row tile index; y = column tile index
        {
            using ( var bitmap = new Bitmap( SettingsManager.Current.Settings.TileGenerator.TileSize,
                                             SettingsManager.Current.Settings.TileGenerator.TileSize ) )
            using ( Graphics g = Graphics.FromImage( bitmap ) ) {
                pos.X = x == 0
                            ? pos.X
                            : pos.X + bitmap.Width / zoom * x; // get tile start coords
                pos.Y = y == 0
                            ? pos.Y
                            : pos.Y + bitmap.Height / zoom * y;

                if ( z == this.savedZoomLevel && this.oldPos.Y != 0 && this.oldPos.X != 0 && this.movement == 0 ) {
                    float difference = Math.Abs( this.oldPos.Y - pos.Y );
                    this.movement = difference / SettingsManager.Current.Settings.TileGenerator.TileSize;

                    Console.WriteLine( $"{difference}, scale: {difference / SettingsManager.Current.Settings.TileGenerator.TileSize}" );
                }

                this._tilesGeneratorRenderer.Render( g, new Rectangle( 0, 0, bitmap.Width, bitmap.Height ), zoom, pos,
                                                     this._palette,
                                                     renderFlags & ~RenderFlags.TextOverlay );

                Directory.CreateDirectory( $"{exportPath}/Tiles/{z}/{x}" );
                bitmap.Save( $"{exportPath}/Tiles/{z}/{x}/{y}.png", ImageFormat.Png );
            }
        }

        private void ZoomOutAndCenterMap( float targetWidth, float targetHeight, out PointF pos, out float zoom ) {
            float mapWidth = this._tilesGeneratorMapper.maxX
                             - this._tilesGeneratorMapper.minX
                             + SettingsManager.Current.Settings.TileGenerator.MapPadding * 2;
            float mapHeight = this._tilesGeneratorMapper.maxZ
                              - this._tilesGeneratorMapper.minZ
                              + SettingsManager.Current.Settings.TileGenerator.MapPadding * 2;

            if ( mapWidth > mapHeight ) // get the scale to have the map edge to edge on the biggest axis (with padding)
            {
                zoom = targetWidth / mapWidth;
                float z = this._tilesGeneratorMapper.minZ
                          - SettingsManager.Current.Settings.TileGenerator.MapPadding
                          + -( targetHeight / zoom ) / 2f
                          + mapHeight                / 2f;
                pos =
                    new
                        PointF( this._tilesGeneratorMapper.minX - SettingsManager.Current.Settings.TileGenerator.MapPadding,
                                z );
            } else {
                zoom = targetHeight / mapHeight;
                float x = this._tilesGeneratorMapper.minX
                          - SettingsManager.Current.Settings.TileGenerator.MapPadding
                          + -( targetWidth / zoom ) / 2f
                          + mapWidth                / 2f;
                pos =
                    new PointF( x,
                                this._tilesGeneratorMapper.minZ
                                - SettingsManager.Current.Settings.TileGenerator.MapPadding );
            }
        }

        private void GenerateTileMap( int         startZoomLevel,
                                      int         endZoomLevel,
                                      string      exportPath,
                                      bool        createTiles,
                                      bool        saveInfo,
                                      RenderFlags renderFlags ) {
            this.savedZoomLevel = endZoomLevel;

            if ( saveInfo || startZoomLevel == 0 ) {
                this.ZoomOutAndCenterMap( SettingsManager.Current.Settings.TileGenerator.TileSize,
                                          SettingsManager.Current.Settings.TileGenerator.TileSize, out PointF pos,
                                          out float zoom ); // get zoom and start coords for tile level 0
                if ( saveInfo )
                    JsonHelper.SaveTileMapInfo( exportPath,
                                                SettingsManager.Current.Settings.TileGenerator.TileMapInfoStructure,
                                                this._mapper.Game,
                                                SettingsManager.Current.Settings.TileGenerator.MapPadding,
                                                SettingsManager.Current.Settings.TileGenerator.TileSize,
                                                pos.X,
                                                pos.X + SettingsManager.Current.Settings.TileGenerator.TileSize / zoom,
                                                pos.Y,
                                                pos.Y + SettingsManager.Current.Settings.TileGenerator.TileSize / zoom,
                                                startZoomLevel,
                                                endZoomLevel,
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
                this.UpdateProgress( "Generating zoom " + z, true );

                this.ZoomOutAndCenterMap( (int) Math.Pow( 2, z ) * SettingsManager.Current.Settings.TileGenerator.TileSize,
                                          (int) Math.Pow( 2, z )
                                          * SettingsManager.Current.Settings.TileGenerator.TileSize, out PointF pos,
                                          out float zoom ); // get zoom and start coords for current tile level
                for ( var x = 0; x < Math.Pow( 2, z ); x++ )
                for ( var y = 0; y < Math.Pow( 2, z ); y++ ) {
                    this.SaveTileImage( z, x, y, pos, zoom, exportPath, renderFlags );

                    this.oldPos = pos;
                }
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
            this._paletteEditorForm = new PaletteEditorForm();
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
                this._tileMapGeneratorForm = new TileMapGeneratorForm();
            this._tileMapGeneratorForm.Show();
            this._tileMapGeneratorForm.BringToFront();

            this._tileMapGeneratorForm.GenerateTileMap +=
                () => // Called when export button is pressed in TileMapGeneratorForm
                {
                    var th = new Thread( this.GenerateTilesMap );
                    th.Start();
                };
        }

        private void GenerateTilesMap() {
            this._tileMapGeneratorForm.Close();

            this.MapPanel.Enabled = false;

            //Task.Run(() =>
            //{
            this.UpdateProgress( "Loading map...", true );

            this._tilesGeneratorMapper = this.CreateMapper();

            this._tilesGeneratorMapper.Parse();

            this.UpdateProgress( "Loading renderer...", true );

            this._tilesGeneratorRenderer = this.CreateRenderer( this._tilesGeneratorMapper );

            this.UpdateProgress( "Generating tiles...", true );

            this._tilesGeneratorMapper.ExportInfo( SettingsManager.Current.Settings.TileGenerator.ExportFlags,
                                                   SettingsManager.Current.Settings.TileGenerator
                                                                  .LastTileMapPath );

            this.GenerateTileMap( SettingsManager.Current.Settings.TileGenerator.StartZoomLevel,
                                  SettingsManager.Current.Settings.TileGenerator.EndZoomLevel,
                                  SettingsManager.Current.Settings.TileGenerator.LastTileMapPath,
                                  SettingsManager.Current.Settings.TileGenerator.GenerateTiles,
                                  ( SettingsManager.Current.Settings.TileGenerator.ExportFlags
                                    & ExportFlags.TileMapInfo )
                                  == ExportFlags.TileMapInfo,
                                  SettingsManager.Current.Settings.TileGenerator.RenderFlags );

            MessageBox.Show( "Tile map has been generated!", "TsMap - Tile Map Generation Finished",
                             MessageBoxButtons.OK, MessageBoxIcon.Information );

            this.UpdateProgress( "Ready.", false );

            this.Invoke( new Action( () => {
                this.Focus();
                this.MapPanel.Enabled = true;
            } ) );
            //});
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

        private void UpdateProgress( string text, bool showProgressBar ) {
            this.Invoke( new Action( () => {
                this.toolStripStatusLabel.Text    = text;
                this.toolStripProgressBar.Visible = showProgressBar;
            } ) );
        }
    }
}