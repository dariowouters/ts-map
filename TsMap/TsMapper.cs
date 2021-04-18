using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TsMap.HashFiles;
using TsMap.TsItem;

namespace TsMap {
    public class TsMapper {
        private readonly Dictionary< ulong, TsCity >       _citiesLookup = new Dictionary< ulong, TsCity >();
        private readonly Dictionary< ulong, TsCountry >    _countriesLookup = new Dictionary< ulong, TsCountry >();
        private readonly List< TsFerryConnection >         _ferryConnectionLookup = new List< TsFerryConnection >();
        private readonly string                            _gameDir;
        private readonly List< Mod >                       _mods;
        private readonly Dictionary< ulong, TsMapOverlay > _overlayLookup = new Dictionary< ulong, TsMapOverlay >();

        private readonly Dictionary< ulong, TsPrefab >   _prefabLookup    = new Dictionary< ulong, TsPrefab >();
        private readonly Dictionary< ulong, TsRoadLook > _roadLookup      = new Dictionary< ulong, TsRoadLook >();
        public readonly  List< TsCityItem >              Cities           = new List< TsCityItem >();
        public readonly  List< TsCompanyItem >           Companies        = new List< TsCompanyItem >();
        public readonly  List< TsFerryItem >             FerryConnections = new List< TsFerryItem >();
        public readonly  List< TsMapAreaItem >           MapAreas         = new List< TsMapAreaItem >();
        public readonly  List< TsMapOverlayItem >        MapOverlays      = new List< TsMapOverlayItem >();

        public readonly Dictionary< ulong, TsNode > Nodes   = new Dictionary< ulong, TsNode >();
        public readonly List< TsPrefabItem >        Prefabs = new List< TsPrefabItem >();

        public readonly List< TsRoadItem >    Roads    = new List< TsRoadItem >();
        public readonly List< TsTriggerItem > Triggers = new List< TsTriggerItem >();

        private List< string > _sectorFiles;
        public  TsGame         Game;
        public  string         gameVersion      = "";
        public  bool           IsEts2           = true;
        public  List< string > LocalizationList = new List< string >();
        public  float          maxX             = float.MinValue;
        public  float          maxZ             = float.MinValue;

        public float minX = float.MaxValue;
        public float minZ = float.MaxValue;

        public RootFileSystem Rfs;
        public string         SelectedLocalization = "";
        public List< TsDlc >  TsDlcs               = new List< TsDlc >();

        public TsMapper( string gameDir, List< Mod > mods ) {
            this._gameDir = gameDir;
            this._mods    = mods;
            this.Sectors  = new List< TsSector >();
            this.LocalizationList.Add( "None" );
        }

        private List< TsSector > Sectors { get; set; }

        private void ParseCityFiles() {
            ScsDirectory defDirectory = this.Rfs.GetDirectory( "def" );
            if ( defDirectory == null ) {
                Log.Msg( "Could not read 'def' dir" );
                return;
            }

            List< ScsFile > cityFiles = defDirectory.GetFiles( "city" );
            if ( cityFiles == null ) {
                Log.Msg( "Could not read city files" );
                return;
            }

            foreach ( ScsFile cityFile in cityFiles ) {
                byte[]   data  = cityFile.Entry.Read();
                string[] lines = Encoding.UTF8.GetString( data ).Split( '\n' );
                foreach ( string line in lines ) {
                    if ( line.TrimStart().StartsWith( "#" ) ) continue;
                    if ( line.Contains( "@include" ) ) {
                        string path = Helper.GetFilePath( line.Split( '"' )[ 1 ], "def" );
                        var    city = new TsCity( this, path );
                        if ( city.Token != 0 && !this._citiesLookup.ContainsKey( city.Token ) )
                            this._citiesLookup.Add( city.Token, city );
                    }
                }
            }
        }

        private void ParseCountryFiles() {
            ScsDirectory defDirectory = this.Rfs.GetDirectory( "def" );
            if ( defDirectory == null ) {
                Log.Msg( "[Country] Could not read 'def' dir" );
                return;
            }

            List< ScsFile > countryFiles = defDirectory.GetFiles( "country" );
            if ( countryFiles == null ) {
                Log.Msg( "Could not read country files" );
                return;
            }

            foreach ( ScsFile countryFile in countryFiles ) {
                byte[]   data  = countryFile.Entry.Read();
                string[] lines = Encoding.UTF8.GetString( data ).Split( '\n' );
                foreach ( string line in lines ) {
                    if ( line.TrimStart().StartsWith( "#" ) ) continue;
                    if ( line.Contains( "@include" ) ) {
                        string path    = Helper.GetFilePath( line.Split( '"' )[ 1 ], "def" );
                        var    country = new TsCountry( this, path );
                        if ( country.Token != 0 && !this._countriesLookup.ContainsKey( country.Token ) )
                            this._countriesLookup.Add( country.Token, country );
                    }
                }
            }
        }

        private void ParsePrefabFiles() {
            ScsDirectory worldDirectory = this.Rfs.GetDirectory( "def/world" );
            if ( worldDirectory == null ) {
                Log.Msg( "Could not read 'def/world' dir" );
                return;
            }

            List< ScsFile > prefabFiles = worldDirectory.GetFiles( "prefab" );
            if ( prefabFiles == null ) {
                Log.Msg( "Could not read prefab files" );
                return;
            }

            foreach ( ScsFile prefabFile in prefabFiles ) {
                if ( !prefabFile.GetFileName().StartsWith( "prefab" ) ) continue;
                byte[]   data  = prefabFile.Entry.Read();
                string[] lines = Encoding.UTF8.GetString( data ).Split( '\n' );

                var token    = 0UL;
                var path     = "";
                var category = "";
                foreach ( string line in lines ) {
                    ( bool validLine, string key, string value ) = SiiHelper.ParseLine( line );
                    if ( validLine ) {
                        if ( key == "prefab_model" )
                            token = ScsHash.StringToToken( SiiHelper.Trim( value.Split( '.' )[ 1 ] ) );
                        else if ( key == "prefab_desc" )
                            path                               = Helper.GetFilePath( value.Split( '"' )[ 1 ] );
                        else if ( key == "category" ) category = value.Split( '"' )[ 1 ];
                    }

                    if ( line.Contains( "}" ) && token != 0 && path != "" ) {
                        var prefab = new TsPrefab( this, path, token, category );
                        if ( prefab.Token != 0 && !this._prefabLookup.ContainsKey( prefab.Token ) )
                            this._prefabLookup.Add( prefab.Token, prefab );

                        token    = 0;
                        path     = "";
                        category = "";
                    }
                }
            }
        }

        private void ParseRoadLookFiles() {
            ScsDirectory worldDirectory = this.Rfs.GetDirectory( "def/world" );
            if ( worldDirectory == null ) {
                Log.Msg( "Could not read 'def/world' dir" );
                return;
            }

            List< ScsFile > roadLookFiles = worldDirectory.GetFiles( "road_look" );
            if ( roadLookFiles == null ) {
                Log.Msg( "Could not read road look files" );
                return;
            }

            foreach ( ScsFile roadLookFile in roadLookFiles ) {
                if ( !roadLookFile.GetFileName().StartsWith( "road" ) ) continue;
                byte[]     data     = roadLookFile.Entry.Read();
                string[]   lines    = Encoding.UTF8.GetString( data ).Split( '\n' );
                TsRoadLook roadLook = null;

                foreach ( string line in lines ) {
                    ( bool validLine, string key, string value ) = SiiHelper.ParseLine( line );
                    if ( validLine ) {
                        if ( key == "road_look" )
                            roadLook =
                                new TsRoadLook( ScsHash.StringToToken( SiiHelper.Trim( value.Split( '.' )[ 1 ]
                                                                           .Trim( '{' ) ) ) );
                        if ( roadLook == null ) continue;
                        if ( key == "lanes_left[]" )
                            roadLook.LanesLeft.Add( value );
                        else if ( key == "lanes_right[]" )
                            roadLook.LanesRight.Add( value );
                        else if ( key == "road_offset" )
                            roadLook.Offset = float.Parse( value, CultureInfo.InvariantCulture );
                    }

                    if ( line.Contains( "}" ) && roadLook != null )
                        if ( roadLook.Token != 0 && !this._roadLookup.ContainsKey( roadLook.Token ) ) {
                            this._roadLookup.Add( roadLook.Token, roadLook );
                            roadLook = null;
                        }
                }
            }
        }

        private void ParseFerryConnections() {
            ScsDirectory connectionDirectory = this.Rfs.GetDirectory( "def/ferry/connection" );
            if ( connectionDirectory == null ) {
                Log.Msg( "Could not read 'def/ferry/connection' dir" );
                return;
            }

            List< ScsFile > ferryConnectionFiles = connectionDirectory.GetFiles( "sii" );
            if ( ferryConnectionFiles == null ) {
                Log.Msg( "Could not read ferry connection files files" );
                return;
            }

            foreach ( ScsFile ferryConnectionFile in ferryConnectionFiles ) {
                byte[]   data  = ferryConnectionFile.Entry.Read();
                string[] lines = Encoding.UTF8.GetString( data ).Split( '\n' );

                TsFerryConnection conn = null;

                foreach ( string line in lines ) {
                    ( bool validLine, string key, string value ) = SiiHelper.ParseLine( line );
                    if ( validLine ) {
                        if ( conn != null ) {
                            if ( key.Contains( "connection_positions" ) ) {
                                int      index  = int.Parse( key.Split( '[' )[ 1 ].Split( ']' )[ 0 ] );
                                string   vector = value.Split( '(' )[ 1 ].Split( ')' )[ 0 ];
                                string[] values = vector.Split( ',' );
                                float    x      = float.Parse( values[ 0 ], CultureInfo.InvariantCulture );
                                float    z      = float.Parse( values[ 2 ], CultureInfo.InvariantCulture );
                                conn.AddConnectionPosition( index, x, z );
                            } else if ( key.Contains( "connection_directions" ) ) {
                                int      index  = int.Parse( key.Split( '[' )[ 1 ].Split( ']' )[ 0 ] );
                                string   vector = value.Split( '(' )[ 1 ].Split( ')' )[ 0 ];
                                string[] values = vector.Split( ',' );
                                float    x      = float.Parse( values[ 0 ], CultureInfo.InvariantCulture );
                                float    z      = float.Parse( values[ 2 ], CultureInfo.InvariantCulture );
                                conn.AddRotation( index, Math.Atan2( z, x ) );
                            }
                        }

                        if ( key == "ferry_connection" ) {
                            string[] portIds = value.Split( '.' );
                            conn = new TsFerryConnection {
                                StartPortToken = ScsHash.StringToToken( portIds[ 1 ] ),
                                EndPortToken   = ScsHash.StringToToken( portIds[ 2 ].TrimEnd( '{' ).Trim() )
                            };
                        }
                    }

                    if ( !line.Contains( "}" ) || conn == null ) continue;
                    ;

                    TsFerryConnection existingItem = this._ferryConnectionLookup.FirstOrDefault( item =>
                        item.StartPortToken == conn.StartPortToken && item.EndPortToken == conn.EndPortToken
                        || item.StartPortToken == conn.EndPortToken
                        && item.EndPortToken   == conn.StartPortToken ); // Check if connection already exists
                    if ( existingItem == null ) this._ferryConnectionLookup.Add( conn );
                    conn = null;
                }
            }
        }

        private void
            ParseOverlays() // TODO: Fix Road overlays and company (road_quarry & quarry) from interfering (or however you spell that)
        {
            ScsDirectory uiMapDirectory = this.Rfs.GetDirectory( "material/ui/map" );
            if ( uiMapDirectory == null ) {
                Log.Msg( "Could not read 'material/ui/map' dir" );
                return;
            }

            List< ScsFile > matFiles = uiMapDirectory.GetFiles( ".mat" );
            if ( matFiles == null ) {
                Log.Msg( "Could not read .mat files" );
                return;
            }

            ScsDirectory uiCompanyDirectory = this.Rfs.GetDirectory( "material/ui/company/small" );
            if ( uiCompanyDirectory != null ) {
                List< ScsFile > data = uiCompanyDirectory.GetFiles( ".mat" );
                if ( data != null ) matFiles.AddRange( data );
            } else
                Log.Msg( "Could not read 'material/ui/company/small' dir" );

            ScsDirectory uiMapRoadDirectory = this.Rfs.GetDirectory( "material/ui/map/road" );
            if ( uiMapRoadDirectory != null ) {
                List< ScsFile > data = uiMapRoadDirectory.GetFiles( ".mat" );
                if ( data != null ) matFiles.AddRange( data );
            } else
                Log.Msg( "Could not read 'material/ui/map/road' dir" );

            foreach ( ScsFile matFile in matFiles ) {
                byte[]   data  = matFile.Entry.Read();
                string[] lines = Encoding.UTF8.GetString( data ).Split( '\n' );

                foreach ( string line in lines ) {
                    ( bool validLine, string key, string value ) = SiiHelper.ParseLine( line );
                    if ( !validLine ) continue;
                    if ( key == "texture" ) {
                        string tobjPath = Helper.CombinePath( matFile.GetLocalPath(), value.Split( '"' )[ 1 ] );

                        byte[] tobjData = this.Rfs.GetFileEntry( tobjPath )?.Entry?.Read();

                        if ( tobjData == null ) break;

                        string path =
                            Helper.GetFilePath( Encoding.UTF8.GetString( tobjData, 0x30, tobjData.Length - 0x30 ) );

                        string name = matFile.GetFileName();
                        if ( name.StartsWith( "map" ) ) continue;
                        if ( name.StartsWith( "road_" ) ) name = name.Substring( 5 );

                        ulong token = ScsHash.StringToToken( name );
                        if ( !this._overlayLookup.ContainsKey( token ) )
                            this._overlayLookup.Add( token, new TsMapOverlay( this, path ) );
                    }
                }
            }
        }

        /// <summary>
        ///     Parse all definition files
        /// </summary>
        private void ParseDefFiles() {
            long startTime = DateTime.Now.Ticks;
            this.Game = new TsGame( this );
            this.ParseCityFiles();
            this.ParseCountryFiles();
            Log.Msg( $"Loaded city files in {( DateTime.Now.Ticks - startTime ) / TimeSpan.TicksPerMillisecond}ms" );

            startTime = DateTime.Now.Ticks;
            this.ParsePrefabFiles();
            Log.Msg( $"Loaded prefab files in {( DateTime.Now.Ticks - startTime ) / TimeSpan.TicksPerMillisecond}ms" );

            startTime = DateTime.Now.Ticks;
            this.ParseRoadLookFiles();
            Log.Msg( $"Loaded road files in {( DateTime.Now.Ticks - startTime ) / TimeSpan.TicksPerMillisecond}ms" );

            startTime = DateTime.Now.Ticks;
            this.ParseFerryConnections();
            Log.Msg( $"Loaded ferry files in {( DateTime.Now.Ticks - startTime ) / TimeSpan.TicksPerMillisecond}ms" );

            startTime = DateTime.Now.Ticks;
            this.ParseOverlays();
            Log.Msg( $"Loaded overlay files in {( DateTime.Now.Ticks - startTime ) / TimeSpan.TicksPerMillisecond}ms" );
        }

        /// <summary>
        ///     Parse all .base files
        /// </summary>
        private void ParseMapFiles() {
            ScsDirectory baseMapEntry = this.Rfs.GetDirectory( "map" );
            if ( baseMapEntry == null ) {
                Log.Msg( "Could not read 'map' dir" );
                return;
            }

            List< ScsFile >
                mbd = baseMapEntry.Files.Values.Where( x => x.GetExtension().Equals( "mbd" ) )
                                  .ToList(); // Get the map names from the mbd files
            if ( mbd.Count == 0 ) {
                Log.Msg( "Could not find mbd file" );
                return;
            }

            this._sectorFiles = new List< string >();

            foreach ( ScsFile file in mbd ) {
                string mapName = file.GetFileName();
                this.IsEts2 = !( mapName == "usa" );

                ScsDirectory mapFileDir = this.Rfs.GetDirectory( $"map/{mapName}" );
                if ( mapFileDir == null ) {
                    Log.Msg( $"Could not read 'map/{mapName}' directory" );
                    return;
                }

                this._sectorFiles.AddRange( mapFileDir.GetFiles( ".base" ).Select( x => x.GetPath() ).ToList() );
            }
        }

        private void ParseLocaleFile( ScsFile localeFile, string locale ) {
            if ( localeFile == null ) return;
            byte[] entryContents = localeFile.Entry.Read();
            uint   magic         = MemoryHelper.ReadUInt32( entryContents, 0 );
            string fileContents = magic == 21720627
                                      ? Helper.Decrypt3Nk( entryContents )
                                      : Encoding.UTF8.GetString( entryContents );
            if ( fileContents == null ) {
                Log.Msg( $"Could not read locale file '{localeFile.GetPath()}'" );
                return;
            }

            var key = string.Empty;

            foreach ( string l in fileContents.Split( '\n' ) ) {
                if ( !l.Contains( ':' ) ) continue;

                if ( l.Contains( "key[]" ) )
                    key = l.Split( '"' )[ 1 ];
                else if ( l.Contains( "val[]" ) ) {
                    string val = l.Split( '"' )[ 1 ];
                    if ( key != string.Empty && val != string.Empty ) {
                        IEnumerable< TsCity > cities =
                            this._citiesLookup.Values.Where( x => x.LocalizationToken == key );
                        foreach ( TsCity city in cities )
                            this._citiesLookup[ city.Token ].AddLocalizedName( locale, val );

                        TsCountry country =
                            this._countriesLookup.Values.FirstOrDefault( x => x.LocalizationToken == key );
                        if ( country != null ) this._countriesLookup[ country.Token ].AddLocalizedName( locale, val );
                    }
                }
            }
        }

        private void ParseVersionFile() {
            ScsFile versionFile = this.Rfs.GetFileEntry( "version.txt" );
            byte[]  content     = versionFile.Entry.Read();

            this.gameVersion = Encoding.UTF8.GetString( content ).Split( '\n' )[ 0 ];
        }

        private void ParseDlcFiles() {
            // TODO: List all DLC files and parse it
            var currentDlcFile = "dlc_fr.manifest.sii";
            this.TsDlcs.Add( new TsDlc( this, currentDlcFile ) );
        }

        private void ReadLocalizationOptions() {
            ScsDirectory localeDir = this.Rfs.GetDirectory( "locale" );
            if ( localeDir == null ) {
                Log.Msg( "Could not find locale directory." );
                return;
            }

            foreach ( KeyValuePair< ulong, ScsDirectory > localeDirDirectory in localeDir.Directories ) {
                this.LocalizationList.Add( localeDirDirectory.Value.GetCurrentDirectoryName() );
                foreach ( KeyValuePair< ulong, ScsFile > localeFile in localeDirDirectory.Value.Files )
                    this.ParseLocaleFile( localeFile.Value, localeDirDirectory.Value.GetCurrentDirectoryName() );
            }
        }

        /// <summary>
        ///     Parse through all .scs files and retrieve all necessary files
        /// </summary>
        public void Parse() {
            long startTime = DateTime.Now.Ticks;

            if ( !Directory.Exists( this._gameDir ) ) {
                Log.Msg( "Could not find Game directory." );
                return;
            }

            this.Rfs = new RootFileSystem( this._gameDir );

            this._mods.Reverse(); // Highest priority mods (top) need to be loaded last

            foreach ( Mod mod in this._mods )
                if ( mod.Load )
                    this.Rfs.AddSourceFile( mod.ModPath );

            Log.Msg( $"Loaded all .scs files in {( DateTime.Now.Ticks - startTime ) / TimeSpan.TicksPerMillisecond}ms" );

            this.ParseVersionFile();
            this.ParseDlcFiles();
            this.ParseDefFiles();
            this.ParseMapFiles();

            this.ReadLocalizationOptions();

            if ( this._sectorFiles == null ) return;
            long preMapParseTime = DateTime.Now.Ticks;
            this.Sectors = this._sectorFiles.Select( file => new TsSector( this, file ) ).ToList();
            this.Sectors.ForEach( sec => sec.Parse() );
            this.Sectors.ForEach( sec => sec.ClearFileData() );
            Log.Msg( $"It took {( DateTime.Now.Ticks - preMapParseTime ) / TimeSpan.TicksPerMillisecond} ms to parse all (*.base)"
                     + $" map files and {( DateTime.Now.Ticks - startTime ) / TimeSpan.TicksPerMillisecond} ms total." );
        }

        public void ExportInfo( ExportFlags exportFlags, string exportPath ) {
            if ( exportFlags.IsActive( ExportFlags.CityList ) ) this.ExportCities( exportFlags, exportPath );
            if ( exportFlags.IsActive( ExportFlags.CountryList ) ) this.ExportCountries( exportFlags, exportPath );
            if ( exportFlags.IsActive( ExportFlags.OverlayList ) ) this.ExportOverlays( exportFlags, exportPath );
        }

        /// <summary>
        ///     Creates a json file with the positions and names (w/ localizations) of all cities
        /// </summary>
        public void ExportCities( ExportFlags exportFlags, string path ) {
            if ( !Directory.Exists( path ) ) return;
            var citiesJArr = new JArray();
            foreach ( TsCityItem city in this.Cities ) {
                if ( city.Hidden ) continue;
                JObject cityJObj = JObject.FromObject( city.City );
                cityJObj[ "X" ] = city.X;
                cityJObj[ "Y" ] = city.Z;
                if ( this._countriesLookup.ContainsKey( ScsHash.StringToToken( city.City.Country ) ) ) {
                    TsCountry country = this._countriesLookup[ ScsHash.StringToToken( city.City.Country ) ];
                    cityJObj[ "CountryId" ] = country.CountryId;
                } else
                    Log.Msg( $"Could not find country for {city.City.Name}" );

                if ( exportFlags.IsActive( ExportFlags.CityLocalizedNames ) )
                    cityJObj[ "LocalizedNames" ] = JObject.FromObject( city.City.LocalizedNames );

                citiesJArr.Add( cityJObj );
            }

            File.WriteAllText( Path.Combine( path, "Cities.json" ), citiesJArr.ToString( Formatting.Indented ) );
        }

        /// <summary>
        ///     Creates a json file with the positions and names (w/ localizations) of all countries
        /// </summary>
        public void ExportCountries( ExportFlags exportFlags, string path ) {
            if ( !Directory.Exists( path ) ) return;
            var countriesJArr = new JArray();
            foreach ( TsCountry country in this._countriesLookup.Values ) {
                JObject countryJObj = JObject.FromObject( country );
                if ( exportFlags.IsActive( ExportFlags.CityLocalizedNames ) )
                    countryJObj[ "LocalizedNames" ] = JObject.FromObject( country.LocalizedNames );
                countriesJArr.Add( countryJObj );
            }

            File.WriteAllText( Path.Combine( path, "Countries.json" ), countriesJArr.ToString( Formatting.Indented ) );
        }

        /// <summary>
        ///     Saves all overlays as .png images.
        ///     Creates a json file with all positions of said overlays
        /// </summary>
        /// <remarks>
        ///     ZoomLevelVisibility flags: Multiple can be selected at the same time,
        ///     eg. if value is 3 then 0 and 1 are both selected
        ///     Selected = hidden (0-7 => numbers in game editor)
        ///     1 = (Nav map, 3D view, zoom 0) (0)
        ///     2 = (Nav map, 3D view, zoom 1) (1)
        ///     4 = (Nav map, 2D view, zoom 0) (2)
        ///     8 = (Nav map, 2D view, zoom 1) (3)
        ///     16 = (World map, zoom 0) (4)
        ///     32 = (World map, zoom 1) (5)
        ///     64 = (World map, zoom 2) (6)
        ///     128 = (World map, zoom 3) (7)
        /// </remarks>
        /// <param name="path"></param>
        public void ExportOverlays( ExportFlags exportFlags, string path ) {
            if ( !Directory.Exists( path ) ) return;

            bool saveAsPNG = exportFlags.IsActive( ExportFlags.OverlayPNGs );

            string overlayPath = Path.Combine( path, "Overlays" );
            if ( saveAsPNG ) Directory.CreateDirectory( overlayPath );

            var overlaysJArr = new JArray();
            foreach ( TsMapOverlayItem overlay in this.MapOverlays ) {
                if ( overlay.Hidden ) continue;
                string overlayName = overlay.OverlayName;
                Bitmap b           = overlay.Overlay?.GetBitmap();
                if ( b == null ) continue;
                var overlayJObj = new JObject {
                    [ "X" ]                   = overlay.X,
                    [ "Y" ]                   = overlay.Z,
                    [ "ZoomLevelVisibility" ] = overlay.ZoomLevelVisibility,
                    [ "Name" ]                = overlayName,
                    [ "Type" ]                = "Overlay",
                    [ "Width" ]               = b.Width,
                    [ "Height" ]              = b.Height
                };
                overlaysJArr.Add( overlayJObj );
                if ( saveAsPNG && !File.Exists( Path.Combine( overlayPath, $"{overlayName}.png" ) ) )
                    b.Save( Path.Combine( overlayPath, $"{overlayName}.png" ) );
            }

            foreach ( TsCompanyItem company in this.Companies ) {
                if ( company.Hidden ) continue;
                string overlayName = ScsHash.TokenToString( company.OverlayToken );
                var    point       = new PointF( company.X, company.Z );
                if ( company.Nodes.Count > 0 ) {
                    TsPrefabItem prefab = this.Prefabs.FirstOrDefault( x => x.Uid == company.Nodes[ 0 ] );
                    if ( prefab != null ) {
                        TsNode originNode = this.GetNodeByUid( prefab.Nodes[ 0 ] );
                        if ( prefab.Prefab.PrefabNodes == null ) continue;
                        TsPrefabNode mapPointOrigin = prefab.Prefab.PrefabNodes[ prefab.Origin ];

                        var rot = (float) ( originNode.Rotation
                                            - Math.PI
                                            - Math.Atan2( mapPointOrigin.RotZ, mapPointOrigin.RotX )
                                            + Math.PI / 2 );

                        float prefabstartX = originNode.X - mapPointOrigin.X;
                        float prefabStartZ = originNode.Z - mapPointOrigin.Z;
                        TsSpawnPoint companyPos =
                            prefab.Prefab.SpawnPoints.FirstOrDefault( x => x.Type == TsSpawnPointType.CompanyPos );
                        if ( companyPos != null )
                            point = RenderHelper.RotatePoint( prefabstartX + companyPos.X, prefabStartZ + companyPos.Z,
                                                              rot,
                                                              originNode.X, originNode.Z );
                    }
                }

                Bitmap b = company.Overlay?.GetBitmap();
                if ( b == null ) continue;
                var overlayJObj = new JObject {
                    [ "X" ]      = point.X,
                    [ "Y" ]      = point.Y,
                    [ "Name" ]   = overlayName,
                    [ "Type" ]   = "Company",
                    [ "Width" ]  = b.Width,
                    [ "Height" ] = b.Height
                };
                overlaysJArr.Add( overlayJObj );
                if ( saveAsPNG && !File.Exists( Path.Combine( overlayPath, $"{overlayName}.png" ) ) )
                    b.Save( Path.Combine( overlayPath, $"{overlayName}.png" ) );
            }

            foreach ( TsTriggerItem trigger in this.Triggers ) {
                if ( trigger.Hidden ) continue;
                string overlayName = trigger.OverlayName;
                Bitmap b           = trigger.Overlay?.GetBitmap();
                if ( b == null ) continue;
                var overlayJObj = new JObject {
                    [ "X" ]      = trigger.X,
                    [ "Y" ]      = trigger.Z,
                    [ "Name" ]   = overlayName,
                    [ "Type" ]   = "Parking",
                    [ "Width" ]  = b.Width,
                    [ "Height" ] = b.Height
                };
                overlaysJArr.Add( overlayJObj );
                if ( saveAsPNG && !File.Exists( Path.Combine( overlayPath, $"{overlayName}.png" ) ) )
                    b.Save( Path.Combine( overlayPath, $"{overlayName}.png" ) );
            }

            foreach ( TsFerryItem ferry in this.FerryConnections ) {
                if ( ferry.Hidden ) continue;
                string overlayName = ScsHash.TokenToString( ferry.OverlayToken );
                Bitmap b           = ferry.Overlay?.GetBitmap();
                if ( b == null ) continue;
                var overlayJObj = new JObject {
                    [ "X" ]    = ferry.X,
                    [ "Y" ]    = ferry.Z,
                    [ "Name" ] = overlayName,
                    [ "Type" ] = ferry.Train
                                     ? "Train"
                                     : "Ferry",
                    [ "Width" ]  = b.Width,
                    [ "Height" ] = b.Height
                };
                overlaysJArr.Add( overlayJObj );
                if ( saveAsPNG && !File.Exists( Path.Combine( overlayPath, $"{overlayName}.png" ) ) )
                    b.Save( Path.Combine( overlayPath, $"{overlayName}.png" ) );
            }

            foreach ( TsPrefabItem prefab in this.Prefabs ) {
                if ( prefab.Hidden ) continue;
                TsNode originNode = this.GetNodeByUid( prefab.Nodes[ 0 ] );
                if ( prefab.Prefab.PrefabNodes == null ) continue;
                TsPrefabNode mapPointOrigin = prefab.Prefab.PrefabNodes[ prefab.Origin ];

                var rot = (float) ( originNode.Rotation
                                    - Math.PI
                                    - Math.Atan2( mapPointOrigin.RotZ, mapPointOrigin.RotX )
                                    + Math.PI / 2 );

                float prefabStartX = originNode.X - mapPointOrigin.X;
                float prefabStartZ = originNode.Z - mapPointOrigin.Z;
                foreach ( TsSpawnPoint spawnPoint in prefab.Prefab.SpawnPoints ) {
                    PointF newPoint = RenderHelper.RotatePoint( prefabStartX + spawnPoint.X,
                                                                prefabStartZ + spawnPoint.Z, rot,
                                                                originNode.X, originNode.Z );

                    var overlayJObj = new JObject {
                        [ "X" ] = newPoint.X,
                        [ "Y" ] = newPoint.Y
                    };

                    string overlayName;

                    switch ( spawnPoint.Type ) {
                        case TsSpawnPointType.GasPos: {
                            overlayName           = "gas_ico";
                            overlayJObj[ "Type" ] = "Fuel";
                            break;
                        }
                        case TsSpawnPointType.ServicePos: {
                            overlayName           = "service_ico";
                            overlayJObj[ "Type" ] = "Service";
                            break;
                        }
                        case TsSpawnPointType.WeightStationPos: {
                            overlayName           = "weigh_station_ico";
                            overlayJObj[ "Type" ] = "WeightStation";
                            break;
                        }
                        case TsSpawnPointType.TruckDealerPos: {
                            overlayName           = "dealer_ico";
                            overlayJObj[ "Type" ] = "TruckDealer";
                            break;
                        }
                        case TsSpawnPointType.BuyPos: {
                            overlayName           = "garage_large_ico";
                            overlayJObj[ "Type" ] = "Garage";
                            break;
                        }
                        case TsSpawnPointType.RecruitmentPos: {
                            overlayName           = "recruitment_ico";
                            overlayJObj[ "Type" ] = "Recruitment";
                            break;
                        }
                        default:
                            continue;
                    }

                    overlayJObj[ "Name" ] = overlayName;
                    TsMapOverlay overlay = this.LookupOverlay( ScsHash.StringToToken( overlayName ) );
                    Bitmap       b       = overlay.GetBitmap();
                    if ( b == null ) continue;
                    overlayJObj[ "Width" ]  = b.Width;
                    overlayJObj[ "Height" ] = b.Height;
                    overlaysJArr.Add( overlayJObj );
                    if ( saveAsPNG && !File.Exists( Path.Combine( overlayPath, $"{overlayName}.png" ) ) )
                        b.Save( Path.Combine( overlayPath, $"{overlayName}.png" ) );
                }

                int lastId = -1;
                foreach ( TsTriggerPoint triggerPoint in prefab.Prefab.TriggerPoints ) {
                    PointF newPoint = RenderHelper.RotatePoint( prefabStartX + triggerPoint.X,
                                                                prefabStartZ + triggerPoint.Z, rot,
                                                                originNode.X, originNode.Z );

                    if ( triggerPoint.TriggerId == lastId ) continue;
                    lastId = (int) triggerPoint.TriggerId;
                    var overlayJObj = new JObject {
                        [ "X" ]    = newPoint.X,
                        [ "Y" ]    = newPoint.Y,
                        [ "Name" ] = "parking_ico",
                        [ "Type" ] = "Parking"
                    };

                    if ( triggerPoint.TriggerActionToken != ScsHash.StringToToken( "hud_parking" ) ) continue;

                    const string overlayName = "parking_ico";
                    TsMapOverlay overlay     = this.LookupOverlay( ScsHash.StringToToken( overlayName ) );
                    Bitmap       b           = overlay.GetBitmap();
                    if ( b == null ) continue;
                    overlayJObj[ "Width" ]  = b.Width;
                    overlayJObj[ "Height" ] = b.Height;
                    overlaysJArr.Add( overlayJObj );
                    if ( saveAsPNG && !File.Exists( Path.Combine( overlayPath, $"{overlayName}.png" ) ) )
                        b.Save( Path.Combine( overlayPath, $"{overlayName}.png" ) );
                }
            }

            File.WriteAllText( Path.Combine( path, "Overlays.json" ), overlaysJArr.ToString( Formatting.Indented ) );
        }

        public void UpdateEdgeCoords( TsNode node ) {
            if ( this.minX > node.X ) this.minX = node.X;
            if ( this.maxX < node.X ) this.maxX = node.X;
            if ( this.minZ > node.Z ) this.minZ = node.Z;
            if ( this.maxZ < node.Z ) this.maxZ = node.Z;
        }

        public void UpdateLocalization( int index ) {
            if ( index < this.LocalizationList.Count ) this.SelectedLocalization = this.LocalizationList[ index ];
        }

        public TsNode GetNodeByUid( ulong uid ) =>
            this.Nodes.ContainsKey( uid )
                ? this.Nodes[ uid ]
                : null;

        public TsCountry GetCountryByTokenName( string name ) {
            ulong token = ScsHash.StringToToken( name );
            return this._countriesLookup.ContainsKey( token )
                       ? this._countriesLookup[ token ]
                       : null;
        }

        public TsRoadLook LookupRoadLook( ulong lookId ) =>
            this._roadLookup.ContainsKey( lookId )
                ? this._roadLookup[ lookId ]
                : null;

        public TsPrefab LookupPrefab( ulong prefabId ) =>
            this._prefabLookup.ContainsKey( prefabId )
                ? this._prefabLookup[ prefabId ]
                : null;

        public TsCity LookupCity( ulong cityId ) =>
            this._citiesLookup.ContainsKey( cityId )
                ? this._citiesLookup[ cityId ]
                : null;

        public TsMapOverlay LookupOverlay( ulong overlayId ) =>
            this._overlayLookup.ContainsKey( overlayId )
                ? this._overlayLookup[ overlayId ]
                : null;

        public List< TsFerryConnection > LookupFerryConnection( ulong ferryPortId ) {
            return this._ferryConnectionLookup.Where( item => item.StartPortToken == ferryPortId ).ToList();
        }

        public void AddFerryPortLocation( ulong ferryPortId, float x, float z ) {
            IEnumerable< TsFerryConnection > ferry =
                this._ferryConnectionLookup.Where( item => item.StartPortToken  == ferryPortId
                                                           || item.EndPortToken == ferryPortId );
            foreach ( TsFerryConnection connection in ferry ) connection.SetPortLocation( ferryPortId, x, z );
        }
    }
}