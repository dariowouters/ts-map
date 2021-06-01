using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            InitializeComponent();

            //_appSettings = f.AppSettings;

            //JsonHelper.SaveSettings(_appSettings);

            _mapper  = CreateMapper();
            _palette = SettingsManager.Current.Settings.Palette.ToBrushPalette();

            _mapper.Parse();

            CityStripComboBox.Items.AddRange( _mapper.Cities.Where( x => !x.Hidden ).ToArray() );

            if ( !_mapper.IsEts2 )
                _startPoint = new PointF( -105000, 15000 );
            else
                _startPoint = new PointF( -1000, -4000 );
            _renderer = CreateRenderer( _mapper );

            // Panning around
            MapPanel.MouseDown += ( s, e ) => {
                _dragging  = true;
                _lastPoint = new PointF( e.X, e.Y );
            };
            MapPanel.MouseUp += ( s, e ) => _dragging = false;
            MapPanel.MouseMove += ( s, e ) => {
                if ( _dragging ) {
                    MapPanel.Invalidate();
                    _startPoint.X -= ( e.X - _lastPoint.X ) / _scale;
                    _startPoint.Y -= ( e.Y - _lastPoint.Y ) / _scale;
                }

                _lastPoint = new PointF( e.X, e.Y );
            };

            MapPanel.MouseWheel += ( s, e ) => {
                _scale += ( e.Delta > 0
                                ? 1
                                : -1 )
                          * 0.05f
                          * _scale;
                _scale = Math.Max( _scale, 0.0005f );
                MapPanel.Invalidate();
            };

            MapPanel.Resize += TsMapCanvas_Resize;

            Closed += ( s, e ) => {
                f.Close();
                _tileMapGeneratorForm?.Close();
            };

            Version appVersion = Assembly.GetExecutingAssembly().GetName().Version;
            version.Text = $@"Version: v{appVersion}";
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

                if ( z == savedZoomLevel && oldPos.Y != 0 && oldPos.X != 0 && movement == 0 ) {
                    float difference = Math.Abs( oldPos.Y - pos.Y );
                    movement = difference / SettingsManager.Current.Settings.TileGenerator.TileSize;

                    Console.WriteLine( $"{difference}, scale: {difference / SettingsManager.Current.Settings.TileGenerator.TileSize}" );
                }

                _tilesGeneratorRenderer.Render( g, new Rectangle( 0, 0, bitmap.Width, bitmap.Height ), zoom, pos,
                                                _palette,
                                                renderFlags & ~RenderFlags.TextOverlay );

                Directory.CreateDirectory( $"{exportPath}/Tiles/{z}/{x}" );
                bitmap.Save( $"{exportPath}/Tiles/{z}/{x}/{y}.png", ImageFormat.Png );
            }
        }

        private void ZoomOutAndCenterMap( float targetWidth, float targetHeight, out PointF pos, out float zoom ) {
            float mapWidth = _tilesGeneratorMapper.maxX
                             - _tilesGeneratorMapper.minX
                             + SettingsManager.Current.Settings.TileGenerator.MapPadding * 2;
            float mapHeight = _tilesGeneratorMapper.maxZ
                              - _tilesGeneratorMapper.minZ
                              + SettingsManager.Current.Settings.TileGenerator.MapPadding * 2;

            if ( mapWidth > mapHeight ) // get the scale to have the map edge to edge on the biggest axis (with padding)
            {
                zoom = targetWidth / mapWidth;
                float z = _tilesGeneratorMapper.minZ
                          - SettingsManager.Current.Settings.TileGenerator.MapPadding
                          + -( targetHeight / zoom ) / 2f
                          + mapHeight                / 2f;
                pos =
                    new
                        PointF( _tilesGeneratorMapper.minX - SettingsManager.Current.Settings.TileGenerator.MapPadding,
                                z );
            } else {
                zoom = targetHeight / mapHeight;
                float x = _tilesGeneratorMapper.minX
                          - SettingsManager.Current.Settings.TileGenerator.MapPadding
                          + -( targetWidth / zoom ) / 2f
                          + mapWidth                / 2f;
                pos =
                    new PointF( x,
                                _tilesGeneratorMapper.minZ
                                - SettingsManager.Current.Settings.TileGenerator.MapPadding );
            }
        }

        private void GenerateTileMap( int         startZoomLevel,
                                      int         endZoomLevel,
                                      string      exportPath,
                                      bool        createTiles,
                                      bool        saveInfo,
                                      RenderFlags renderFlags ) {
            savedZoomLevel = endZoomLevel;

            // --- Debug

            var data = new JObject {
                [ "def_cities" ]           = _mapper._citiesLookup.Count,
                [ "def_countries" ]        = _mapper._countriesLookup.Count,
                [ "def_ferryConnections" ] = _mapper._ferryConnectionLookup.Count,
                [ "def_overlays" ]         = _mapper._overlayLookup.Count,
                [ "def_prefab" ]           = _mapper._prefabLookup.Count,
                [ "def_roadLook" ]         = _mapper._roadLookup.Count,
                [ "map_cities" ]           = _mapper.Cities.Count,
                [ "map_companies" ]        = _mapper.Companies.Count,
                [ "map_ferryConnections" ] = _mapper.FerryConnections.Count,
                [ "map_areas" ]            = _mapper.MapAreas.Count,
                [ "map_overlays" ]         = _mapper.MapOverlays.Count,
                [ "map_prefab" ]           = _mapper.Prefabs.Count,
                [ "map_road" ]             = _mapper.Roads.Count,
                [ "map_triggers" ]         = _mapper.Triggers.Count,
                [ "map_maxX" ]             = _mapper.maxX,
                [ "map_maxZ" ]             = _mapper.maxZ,
                [ "map_minX" ]             = _mapper.minX,
                [ "map_minZ" ]             = _mapper.minZ,
                [ "map_nodes" ]            = _mapper.Nodes.Count,
                [ "item_all" ]             = _mapper.MapOverlays.Count,
                [ "item_companies" ]       = 0,
                [ "item_ferries" ]         = 0,
                [ "item_fuel" ]            = 0,
                [ "item_garage" ]          = 0,
                [ "item_overlay" ]         = 0,
                [ "item_parking" ]         = 0,
                [ "item_recruitment" ]     = 0,
                [ "item_service" ]         = 0,
                [ "item_train" ]           = 0,
                [ "item_truckDealer" ]     = 0,
                [ "item_weightStation" ]   = 0
            };
            File.WriteAllText( Path.Combine( exportPath, "DataOverview.json" ), JsonConvert.SerializeObject( data, Formatting.Indented ) );

            // --- ./Debug

            if ( saveInfo || startZoomLevel == 0 ) {
                ZoomOutAndCenterMap( SettingsManager.Current.Settings.TileGenerator.TileSize,
                                     SettingsManager.Current.Settings.TileGenerator.TileSize, out PointF pos,
                                     out float zoom ); // get zoom and start coords for tile level 0
                if ( saveInfo )
                    JsonHelper.SaveTileMapInfo( exportPath,
                                                SettingsManager.Current.Settings.TileGenerator.TileMapInfoStructure,
                                                _mapper.Game,
                                                SettingsManager.Current.Settings.TileGenerator.MapPadding,
                                                SettingsManager.Current.Settings.TileGenerator.TileSize,
                                                pos.X,
                                                pos.X + SettingsManager.Current.Settings.TileGenerator.TileSize / zoom,
                                                pos.Y,
                                                pos.Y + SettingsManager.Current.Settings.TileGenerator.TileSize / zoom,
                                                startZoomLevel,
                                                endZoomLevel,
                                                _mapper.gameVersion );

                if ( startZoomLevel == 0 && createTiles ) {
                    SaveTileImage( 0, 0, 0, pos, zoom, exportPath, renderFlags );
                    startZoomLevel++;
                }
            }

            if ( !createTiles ) return;
            for ( int z = startZoomLevel;
                  z <= endZoomLevel;
                  z++ ) // https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
            {
                UpdateProgress( "Generating zoom " + z, true );

                ZoomOutAndCenterMap( (int) Math.Pow( 2, z ) * SettingsManager.Current.Settings.TileGenerator.TileSize,
                                     (int) Math.Pow( 2, z )
                                     * SettingsManager.Current.Settings.TileGenerator.TileSize, out PointF pos,
                                     out float zoom ); // get zoom and start coords for current tile level
                for ( var x = 0; x < Math.Pow( 2, z ); x++ )
                for ( var y = 0; y < Math.Pow( 2, z ); y++ ) {
                    SaveTileImage( z, x, y, pos, zoom, exportPath, renderFlags );

                    oldPos = pos;
                }
            }
        }

        private void TsMapCanvas_Resize( object sender, EventArgs e ) {
            MapPanel.Invalidate();
        }

        private void ExitToolStripMenuItem_Click( object sender, EventArgs e ) {
            Application.Exit();
        }

        private void ItemVisibilityToolStripMenuItem_Click( object sender, EventArgs e ) {
            _itemVisibilityForm = new ItemVisibilityForm( _renderFlags );
            _itemVisibilityForm.Show();
            _itemVisibilityForm.BringToFront();

            _itemVisibilityForm.UpdateItemVisibility += renderFlags => {
                _renderFlags = renderFlags;
                MapPanel.Invalidate();
            };
        }

        private void paletteToolStripMenuItem_Click( object sender, EventArgs e ) {
            _paletteEditorForm = new PaletteEditorForm();
            _paletteEditorForm.Show();
            _paletteEditorForm.BringToFront();

            _paletteEditorForm.UpdatePalette += palette => {
                _palette = palette;
            };
        }

        private void MapPanel_Paint( object sender, PaintEventArgs e ) {
            _renderer.Render( e.Graphics, e.ClipRectangle, _scale, _startPoint, _palette,
                              _renderFlags );
        }

        private void localizationSettingsToolStripMenuItem_Click( object sender, EventArgs e ) {
            _localizationSettingsForm =
                new LocalizationSettingsForm( _mapper.LocalizationList, _mapper.SelectedLocalization );
            _localizationSettingsForm.Show();
            _localizationSettingsForm.BringToFront();

            _localizationSettingsForm.UpdateLocalization += locIndex => {
                _mapper.UpdateLocalization( locIndex );
                CityStripComboBox.Items.Clear();
                CityStripComboBox.Items.AddRange( _mapper.Cities.Where( x => !x.Hidden ).ToArray() );
                _localizationSettingsForm.Close();
                MapPanel.Invalidate();
            };
        }

        private void GenerateTileMapToolStripMenuItem_Click( object sender, EventArgs e ) {
            if ( _tileMapGeneratorForm == null || _tileMapGeneratorForm.IsDisposed )
                _tileMapGeneratorForm = new TileMapGeneratorForm();
            _tileMapGeneratorForm.Show();
            _tileMapGeneratorForm.BringToFront();

            _tileMapGeneratorForm.GenerateTileMap +=
                () => // Called when export button is pressed in TileMapGeneratorForm
                {
                    var th = new Thread( GenerateTilesMap );
                    th.Start();
                };
        }

        private void GenerateTilesMap() {
            _tileMapGeneratorForm.Close();

            MapPanel.Enabled = false;

            //Task.Run(() =>
            //{
            UpdateProgress( "Loading map...", true );

            _tilesGeneratorMapper = CreateMapper();

            _tilesGeneratorMapper.Parse();

            UpdateProgress( "Loading renderer...", true );

            _tilesGeneratorRenderer = CreateRenderer( _tilesGeneratorMapper );

            UpdateProgress( "Generating tiles...", true );

            _tilesGeneratorMapper.ExportInfo( SettingsManager.Current.Settings.TileGenerator.ExportFlags,
                                              SettingsManager.Current.Settings.TileGenerator
                                                             .LastTileMapPath );

            GenerateTileMap( SettingsManager.Current.Settings.TileGenerator.StartZoomLevel,
                             SettingsManager.Current.Settings.TileGenerator.EndZoomLevel,
                             SettingsManager.Current.Settings.TileGenerator.LastTileMapPath,
                             SettingsManager.Current.Settings.TileGenerator.GenerateTiles,
                             ( SettingsManager.Current.Settings.TileGenerator.ExportFlags
                               & ExportFlags.TileMapInfo )
                             == ExportFlags.TileMapInfo,
                             SettingsManager.Current.Settings.TileGenerator.RenderFlags );

            MessageBox.Show( "Tile map has been generated!", "TsMap - Tile Map Generation Finished",
                             MessageBoxButtons.OK, MessageBoxIcon.Information );

            UpdateProgress( "Ready.", false );

            Invoke( new Action( () => {
                Focus();
                MapPanel.Enabled = true;
            } ) );
            //});
        }

        private void FullMapToolStripMenuItem_Click( object sender, EventArgs e ) {
            ZoomOutAndCenterMap( MapPanel.Width, MapPanel.Height, out _startPoint,
                                 out _scale );
            MapPanel.Invalidate();
        }

        private void ResetMapToolStripMenuItem_Click( object sender, EventArgs e ) {
            _scale = 0.2f;
            _startPoint = !_mapper.IsEts2
                              ? new PointF( -105000, 15000 )
                              : new PointF( -1000,   -4000 );
            MapPanel.Invalidate();
        }

        private void CityStripComboBox_SelectedIndexChanged( object sender, EventArgs e ) {
            var city = (TsCityItem) ( (ToolStripComboBox) sender ).SelectedItem;
            _scale      = 0.2f;
            _startPoint = new PointF( city.X - 1000, city.Z - 1000 );
            MapPanel.Invalidate();
        }

        private void UpdateProgress( string text, bool showProgressBar ) {
            Invoke( new Action( () => {
                toolStripStatusLabel.Text    = text;
                toolStripProgressBar.Visible = showProgressBar;
            } ) );
        }
    }
}