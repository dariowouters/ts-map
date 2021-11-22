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

namespace TsMap
{
    public class TsMapper
    {
        private readonly string _gameDir;
        private List<Mod> _mods;

        public RootFileSystem Rfs;
        public bool IsEts2 = true;

        private List<string> _sectorFiles;
        public string SelectedLocalization = "";
        public List<string> LocalizationList = new List<string>();

        private readonly Dictionary<ulong, TsPrefab> _prefabLookup = new Dictionary<ulong, TsPrefab>();
        private readonly Dictionary<ulong, TsCity> _citiesLookup = new Dictionary<ulong, TsCity>();
        private readonly Dictionary<ulong, TsCountry> _countriesLookup = new Dictionary<ulong, TsCountry>();
        private readonly Dictionary<ulong, TsRoadLook> _roadLookup = new Dictionary<ulong, TsRoadLook>();
        private readonly Dictionary<ulong, TsMapOverlay> _overlayLookup = new Dictionary<ulong, TsMapOverlay>();
        private readonly List<TsFerryConnection> _ferryConnectionLookup = new List<TsFerryConnection>();

        public readonly List<TsRoadItem> Roads = new List<TsRoadItem>();
        public readonly List<TsPrefabItem> Prefabs = new List<TsPrefabItem>();
        public readonly List<TsMapAreaItem> MapAreas = new List<TsMapAreaItem>();
        public readonly List<TsCityItem> Cities = new List<TsCityItem>();
        public readonly List<TsMapOverlayItem> MapOverlays = new List<TsMapOverlayItem>();
        public readonly List<TsFerryItem> FerryConnections = new List<TsFerryItem>();
        public readonly List<TsCompanyItem> Companies = new List<TsCompanyItem>();
        public readonly List<TsTriggerItem> Triggers = new List<TsTriggerItem>();
        public readonly List<TsCutsceneItem> Viewpoints = new List<TsCutsceneItem>();

        public readonly Dictionary<ulong, TsNode> Nodes = new Dictionary<ulong, TsNode>();

        public float minX = float.MaxValue;
        public float maxX = float.MinValue;
        public float minZ = float.MaxValue;
        public float maxZ = float.MinValue;

        private List<TsSector> Sectors { get; set; }

        public TsMapper(string gameDir, List<Mod> mods)
        {
            _gameDir = gameDir;
            _mods = mods;
            Sectors = new List<TsSector>();
            LocalizationList.Add("None");
        }

        private void ParseCityFiles()
        {
            var defDirectory = Rfs.GetDirectory("def");
            if (defDirectory == null)
            {
                Log.Msg("Could not read 'def' dir");
                return;
            }

            var cityFiles = defDirectory.GetFiles("city");
            if (cityFiles == null)
            {
                Log.Msg("Could not read city files");
                return;
            }

            foreach (var cityFile in cityFiles)
            {
                var data = cityFile.Entry.Read();
                var lines = Encoding.UTF8.GetString(data).Split('\n');
                foreach (var line in lines)
                {
                    if (line.TrimStart().StartsWith("#")) continue;
                    if (line.Contains("@include"))
                    {
                        var path = Helper.GetFilePath(line.Split('"')[1], "def");
                        var city = new TsCity(this, path);
                        if (city.Token != 0 && !_citiesLookup.ContainsKey(city.Token))
                        {
                            _citiesLookup.Add(city.Token, city);
                        }
                    }
                }
            }
        }

        private void ParseCountryFiles()
        {
            var defDirectory = Rfs.GetDirectory("def");
            if (defDirectory == null)
            {
                Log.Msg("[Country] Could not read 'def' dir");
                return;
            }

            var countryFiles = defDirectory.GetFiles("country");
            if (countryFiles == null)
            {
                Log.Msg("Could not read country files");
                return;
            }

            foreach (var countryFile in countryFiles)
            {
                var data = countryFile.Entry.Read();
                var lines = Encoding.UTF8.GetString(data).Split('\n');
                foreach (var line in lines)
                {
                    if (line.TrimStart().StartsWith("#")) continue;
                    if (line.Contains("@include"))
                    {
                        var path = Helper.GetFilePath(line.Split('"')[1], "def");
                        var country = new TsCountry(this, path);
                        if (country.Token != 0 && !_countriesLookup.ContainsKey(country.Token))
                        {
                            _countriesLookup.Add(country.Token, country);
                        }
                    }
                }
            }
        }

        private void ParsePrefabFiles()
        {
            var worldDirectory = Rfs.GetDirectory("def/world");
            if (worldDirectory == null)
            {
                Log.Msg("Could not read 'def/world' dir");
                return;
            }

            var prefabFiles = worldDirectory.GetFiles("prefab");
            if (prefabFiles == null)
            {
                Log.Msg("Could not read prefab files");
                return;
            }

            foreach (var prefabFile in prefabFiles)
            {
                if (!prefabFile.GetFileName().StartsWith("prefab")) continue;
                var data = prefabFile.Entry.Read();
                var lines = Encoding.UTF8.GetString(data).Split('\n');

                var token = 0UL;
                var path = "";
                var category = "";
                foreach (var line in lines)
                {
                    var (validLine, key, value) = SiiHelper.ParseLine(line);
                    if (validLine)
                    {
                        if (key == "prefab_model")
                        {
                            token = ScsHash.StringToToken(SiiHelper.Trim(value.Split('.')[1]));
                        }
                        else if (key == "prefab_desc")
                        {
                            path = Helper.GetFilePath(value.Split('"')[1]);
                        }
                        else if (key == "category")
                        {
                            category = value.Split('"')[1];
                        }
                    }

                    if (line.Contains("}") && token != 0 && path != "")
                    {
                        var prefab = new TsPrefab(this, path, token, category);
                        if (prefab.Token != 0 && !_prefabLookup.ContainsKey(prefab.Token))
                        {
                            _prefabLookup.Add(prefab.Token, prefab);
                        }

                        token = 0;
                        path = "";
                        category = "";
                    }
                }
            }
        }

        private void ParseRoadLookFiles()
        {
            var worldDirectory = Rfs.GetDirectory("def/world");
            if (worldDirectory == null)
            {
                Log.Msg("Could not read 'def/world' dir");
                return;
            }

            var roadLookFiles = worldDirectory.GetFiles("road_look");
            if (roadLookFiles == null)
            {
                Log.Msg("Could not read road look files");
                return;
            }

            foreach (var roadLookFile in roadLookFiles)
            {
                if (!roadLookFile.GetFileName().StartsWith("road")) continue;
                var data = roadLookFile.Entry.Read();
                var lines = Encoding.UTF8.GetString(data).Split('\n');
                TsRoadLook roadLook = null;

                foreach (var line in lines)
                {
                    var (validLine, key, value) = SiiHelper.ParseLine(line);
                    if (validLine)
                    {
                        if (key == "road_look")
                        {
                            roadLook = new TsRoadLook(ScsHash.StringToToken(SiiHelper.Trim(value.Split('.')[1].Trim('{'))));
                        }
                        if (roadLook == null) continue;
                        if (key == "lanes_left[]")
                        {
                            roadLook.LanesLeft.Add(value);
                        }
                        else if (key == "lanes_right[]")
                        {
                            roadLook.LanesRight.Add(value);
                        }
                        else if (key == "road_offset")
                        {
                            roadLook.Offset = float.Parse(value, CultureInfo.InvariantCulture);
                        }
                    }

                    if (line.Contains("}") && roadLook != null)
                    {
                        if (roadLook.Token != 0 && !_roadLookup.ContainsKey(roadLook.Token))
                        {
                            _roadLookup.Add(roadLook.Token, roadLook);
                            roadLook = null;
                        }
                    }
                }
            }
        }

        private void ParseFerryConnections()
        {
            var connectionDirectory = Rfs.GetDirectory("def/ferry/connection");
            if (connectionDirectory == null)
            {
                Log.Msg("Could not read 'def/ferry/connection' dir");
                return;
            }

            var ferryConnectionFiles = connectionDirectory.GetFiles("sii");
            if (ferryConnectionFiles == null)
            {
                Log.Msg("Could not read ferry connection files files");
                return;
            }

            foreach (var ferryConnectionFile in ferryConnectionFiles)
            {
                var data = ferryConnectionFile.Entry.Read();
                var lines = Encoding.UTF8.GetString(data).Split('\n');

                TsFerryConnection conn = null;

                foreach (var line in lines)
                {
                    var (validLine, key, value) = SiiHelper.ParseLine(line);
                    if (validLine)
                    {
                        if (conn != null)
                        {
                            if (key.Contains("connection_positions"))
                            {
                                var index = int.Parse(key.Split('[')[1].Split(']')[0]);
                                var vector = value.Split('(')[1].Split(')')[0];
                                var values = vector.Split(',');
                                var x = float.Parse(values[0], CultureInfo.InvariantCulture);
                                var z = float.Parse(values[2], CultureInfo.InvariantCulture);
                                conn.AddConnectionPosition(index, x, z);
                            }
                            else if (key.Contains("connection_directions"))
                            {
                                var index = int.Parse(key.Split('[')[1].Split(']')[0]);
                                var vector = value.Split('(')[1].Split(')')[0];
                                var values = vector.Split(',');
                                var x = float.Parse(values[0], CultureInfo.InvariantCulture);
                                var z = float.Parse(values[2], CultureInfo.InvariantCulture);
                                conn.AddRotation(index, Math.Atan2(z, x));
                            }
                        }

                        if (key == "ferry_connection")
                        {
                            var portIds = value.Split('.');
                            conn = new TsFerryConnection
                            {
                                StartPortToken = ScsHash.StringToToken(portIds[1]),
                                EndPortToken = ScsHash.StringToToken(portIds[2].TrimEnd('{').Trim())
                            };
                        }
                    }

                    if (!line.Contains("}") || conn == null) continue;;

                    var existingItem = _ferryConnectionLookup.FirstOrDefault(item =>
                        (item.StartPortToken == conn.StartPortToken && item.EndPortToken == conn.EndPortToken) ||
                        (item.StartPortToken == conn.EndPortToken && item.EndPortToken == conn.StartPortToken)); // Check if connection already exists
                    if (existingItem == null) _ferryConnectionLookup.Add(conn);
                    conn = null;
                }
            }
        }

        private void ParseOverlays() // TODO: Fix Road overlays and company (road_quarry & quarry) from interfering (or however you spell that)
        {
            var uiMapDirectory = Rfs.GetDirectory("material/ui/map");
            if (uiMapDirectory == null)
            {
                Log.Msg("Could not read 'material/ui/map' dir");
                return;
            }

            var matFiles = uiMapDirectory.GetFiles(".mat");
            if (matFiles == null)
            {
                Log.Msg("Could not read .mat files");
                return;
            }

            var uiCompanyDirectory = Rfs.GetDirectory("material/ui/company/small");
            if (uiCompanyDirectory != null)
            {
                var data = uiCompanyDirectory.GetFiles(".mat");
                if (data != null) matFiles.AddRange(data);
            }
            else
            {
                Log.Msg("Could not read 'material/ui/company/small' dir");
            }

            var uiMapRoadDirectory = Rfs.GetDirectory("material/ui/map/road");
            if (uiMapRoadDirectory != null)
            {
                var data = uiMapRoadDirectory.GetFiles(".mat");
                if (data != null) matFiles.AddRange(data);
            }
            else
            {
                Log.Msg("Could not read 'material/ui/map/road' dir");
            }

            foreach (var matFile in matFiles)
            {
                var data = matFile.Entry.Read();
                var lines = Encoding.UTF8.GetString(data).Split('\n');

                foreach (var line in lines)
                {
                    var (validLine, key, value) = SiiHelper.ParseLine(line);
                    if (!validLine) continue;
                    if (key == "texture")
                    {
                        var tobjPath = Helper.CombinePath(matFile.GetLocalPath(), value.Split('"')[1]);

                        var tobjData = Rfs.GetFileEntry(tobjPath)?.Entry?.Read();

                        if (tobjData == null) break;

                        var path = Helper.GetFilePath(Encoding.UTF8.GetString(tobjData, 0x30, tobjData.Length - 0x30));

                        var name = matFile.GetFileName();
                        if (name.StartsWith("map")) continue;
                        if (name.StartsWith("road_")) name = name.Substring(5);

                        var token = ScsHash.StringToToken(name);
                        if (!_overlayLookup.ContainsKey(token))
                        {
                            _overlayLookup.Add(token, new TsMapOverlay(this, path));
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Parse all definition files
        /// </summary>
        private void ParseDefFiles()
        {
            var startTime = DateTime.Now.Ticks;
            ParseCityFiles();
            ParseCountryFiles();
            Log.Msg($"Loaded city files in {(DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond}ms");

            startTime = DateTime.Now.Ticks;
            ParsePrefabFiles();
            Log.Msg($"Loaded prefab files in {(DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond}ms");

            startTime = DateTime.Now.Ticks;
            ParseRoadLookFiles();
            Log.Msg($"Loaded road files in {(DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond}ms");

            startTime = DateTime.Now.Ticks;
            ParseFerryConnections();
            Log.Msg($"Loaded ferry files in {(DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond}ms");

            startTime = DateTime.Now.Ticks;
            ParseOverlays();
            Log.Msg($"Loaded overlay files in {(DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond}ms");
        }

        /// <summary>
        /// Parse all .base files
        /// </summary>
        private void ParseMapFiles()
        {
            var baseMapEntry = Rfs.GetDirectory("map");
            if (baseMapEntry == null)
            {
                Log.Msg("Could not read 'map' dir");
                return;
            }

            var mbd = baseMapEntry.Files.Values.Where(x => x.GetExtension().Equals("mbd")).ToList(); // Get the map names from the mbd files
            if (mbd.Count == 0)
            {
                Log.Msg("Could not find mbd file");
                return;
            }

            _sectorFiles = new List<string>();

            foreach (var file in mbd)
            {
                var mapName = file.GetFileName();
                IsEts2 = !(mapName == "usa");

                var mapFileDir = Rfs.GetDirectory($"map/{mapName}");
                if (mapFileDir == null)
                {
                    Log.Msg($"Could not read 'map/{mapName}' directory");
                    return;
                }

                _sectorFiles.AddRange(mapFileDir.GetFiles(".base").Select(x => x.GetPath()).ToList());
            }
        }

        private void ParseLocaleFile(ScsFile localeFile, string locale)
        {
            if (localeFile == null) return;
            var entryContents = localeFile.Entry.Read();
            var magic = MemoryHelper.ReadUInt32(entryContents, 0);
            var fileContents = (magic == 21720627) ? Helper.Decrypt3Nk(entryContents) : Encoding.UTF8.GetString(entryContents);
            if (fileContents == null)
            {
                Log.Msg($"Could not read locale file '{localeFile.GetPath()}'");
                return;
            }

            var key = string.Empty;

            foreach (var l in fileContents.Split('\n'))
            {
                if (!l.Contains(':')) continue;

                if (l.Contains("key[]"))
                {
                    key = l.Split('"')[1];
                }
                else if (l.Contains("val[]"))
                {
                    var val = l.Split('"')[1];
                    if (key != string.Empty && val != string.Empty)
                    {
                        var cities = _citiesLookup.Values.Where(x => x.LocalizationToken == key);
                        foreach (var city in cities)
                        {
                            _citiesLookup[city.Token].AddLocalizedName(locale, val);
                        }

                        var country = _countriesLookup.Values.FirstOrDefault(x => x.LocalizationToken == key);
                        if (country != null) _countriesLookup[country.Token].AddLocalizedName(locale, val);
                    }
                }
            }
        }

        private void ReadLocalizationOptions()
        {
            var localeDir = Rfs.GetDirectory("locale");
            if (localeDir == null)
            {
                Log.Msg("Could not find locale directory.");
                return;
            }
            foreach (var localeDirDirectory in localeDir.Directories)
            {
                LocalizationList.Add(localeDirDirectory.Value.GetCurrentDirectoryName());
                foreach (var localeFile in localeDirDirectory.Value.Files)
                {
                    ParseLocaleFile(localeFile.Value, localeDirDirectory.Value.GetCurrentDirectoryName());
                }
            }
        }

        /// <summary>
        /// Parse through all .scs files and retrieve all necessary files
        /// </summary>
        public void Parse()
        {
            var startTime = DateTime.Now.Ticks;

            if (!Directory.Exists(_gameDir))
            {
                Log.Msg("Could not find Game directory.");
                return;
            }

            Rfs = new RootFileSystem(_gameDir);

            _mods.Reverse(); // Highest priority mods (top) need to be loaded last

            foreach (var mod in _mods)
            {
                if (mod.Load) Rfs.AddSourceFile(mod.ModPath);
            }

            Log.Msg($"Loaded all .scs files in {(DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond}ms");

            ParseDefFiles();
            ParseMapFiles();

            ReadLocalizationOptions();

            if (_sectorFiles == null) return;
            var preMapParseTime = DateTime.Now.Ticks;
            Sectors = _sectorFiles.Select(file => new TsSector(this, file)).ToList();
            Sectors.ForEach(sec => sec.Parse());
            Sectors.ForEach(sec => sec.ClearFileData());
            Log.Msg($"It took {(DateTime.Now.Ticks - preMapParseTime) / TimeSpan.TicksPerMillisecond} ms to parse all (*.base)" +
                    $" map files and {(DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond} ms total.");
        }

        public void ExportInfo(ExportFlags exportFlags, string exportPath)
        {
            if (exportFlags.IsActive(ExportFlags.CityList)) ExportCities(exportFlags, exportPath);
            if (exportFlags.IsActive(ExportFlags.CountryList)) ExportCountries(exportFlags, exportPath);
            if (exportFlags.IsActive(ExportFlags.OverlayList)) ExportOverlays(exportFlags, exportPath);
        }

        /// <summary>
        /// Creates a json file with the positions and names (w/ localizations) of all cities
        /// </summary>
        public void ExportCities(ExportFlags exportFlags, string path)
        {
            if (!Directory.Exists(path)) return;
            var citiesJArr = new JArray();
            foreach (var city in Cities)
            {
                if (city.Hidden) continue;
                var cityJObj = JObject.FromObject(city.City);
                cityJObj["X"] = city.X;
                cityJObj["Y"] = city.Z;
                if (_countriesLookup.ContainsKey(ScsHash.StringToToken(city.City.Country)))
                {
                    var country = _countriesLookup[ScsHash.StringToToken(city.City.Country)];
                    cityJObj["CountryId"] = country.CountryId;
                }
                else
                {
                    Log.Msg($"Could not find country for {city.City.Name}");
                }

                if (exportFlags.IsActive(ExportFlags.CityLocalizedNames))
                    cityJObj["LocalizedNames"] = JObject.FromObject(city.City.LocalizedNames);

                citiesJArr.Add(cityJObj);
            }
            File.WriteAllText(Path.Combine(path, "Cities.json"), citiesJArr.ToString(Formatting.Indented));
        }
        /// <summary>
        /// Creates a json file with the positions and names (w/ localizations) of all countries
        /// </summary>
        public void ExportCountries(ExportFlags exportFlags, string path)
        {
            if (!Directory.Exists(path)) return;
            var countriesJArr = new JArray();
            foreach (var country in _countriesLookup.Values)
            {
                var countryJObj = JObject.FromObject(country);
                if (exportFlags.IsActive(ExportFlags.CityLocalizedNames))
                    countryJObj["LocalizedNames"] = JObject.FromObject(country.LocalizedNames);
                countriesJArr.Add(countryJObj);
            }
            File.WriteAllText(Path.Combine(path, "Countries.json"), countriesJArr.ToString(Formatting.Indented));
        }

        /// <summary>
        /// Saves all overlays as .png images.
        /// Creates a json file with all positions of said overlays
        /// </summary>
        /// <remarks>
        /// ZoomLevelVisibility flags: Multiple can be selected at the same time,
        /// eg. if value is 3 then 0 and 1 are both selected
        /// Selected = hidden (0-7 => numbers in game editor)
        /// 1 = (Nav map, 3D view, zoom 0) (0)
        /// 2 = (Nav map, 3D view, zoom 1) (1)
        /// 4 = (Nav map, 2D view, zoom 0) (2)
        /// 8 = (Nav map, 2D view, zoom 1) (3)
        /// 16 = (World map, zoom 0) (4)
        /// 32 = (World map, zoom 1) (5)
        /// 64 = (World map, zoom 2) (6)
        /// 128 = (World map, zoom 3) (7)
        /// </remarks>
        /// <param name="path"></param>
        public void ExportOverlays(ExportFlags exportFlags, string path)
        {
            if (!Directory.Exists(path)) return;

            var saveAsPNG = exportFlags.IsActive(ExportFlags.OverlayPNGs);

            var overlayPath = Path.Combine(path, "Overlays");
            if (saveAsPNG) Directory.CreateDirectory(overlayPath);

            var overlaysJArr = new JArray();
            foreach (var overlay in MapOverlays)
            {
                if (overlay.Hidden) continue;
                var overlayName = overlay.OverlayName;
                var b = overlay.Overlay?.GetBitmap();
                if (b == null) continue;
                var overlayJObj = new JObject
                {
                    ["X"] = overlay.X,
                    ["Y"] = overlay.Z,
                    ["ZoomLevelVisibility"] = overlay.ZoomLevelVisibility,
                    ["Name"] = overlayName,
                    ["Type"] = "Overlay",
                    ["Width"] = b.Width,
                    ["Height"] = b.Height,
                };
                overlaysJArr.Add(overlayJObj);
                if (saveAsPNG && !File.Exists(Path.Combine(overlayPath, $"{overlayName}.png")))
                    b.Save(Path.Combine(overlayPath, $"{overlayName}.png"));
            }
            foreach (var company in Companies)
            {
                if (company.Hidden) continue;
                var overlayName = ScsHash.TokenToString(company.OverlayToken);
                var point = new PointF(company.X, company.Z);
                if (company.Nodes.Count > 0)
                {
                    var prefab = Prefabs.FirstOrDefault(x => x.Uid == company.Nodes[0]);
                    if (prefab != null)
                    {
                        var originNode = GetNodeByUid(prefab.Nodes[0]);
                        if (prefab.Prefab.PrefabNodes == null) continue;
                        var mapPointOrigin = prefab.Prefab.PrefabNodes[prefab.Origin];

                        var rot = (float)(originNode.Rotation - Math.PI -
                                           Math.Atan2(mapPointOrigin.RotZ, mapPointOrigin.RotX) + Math.PI / 2);

                        var prefabstartX = originNode.X - mapPointOrigin.X;
                        var prefabStartZ = originNode.Z - mapPointOrigin.Z;
                        var companyPos = prefab.Prefab.SpawnPoints.FirstOrDefault(x => x.Type == TsSpawnPointType.CompanyPos);
                        if (companyPos != null)
                        {
                            point = RenderHelper.RotatePoint(prefabstartX + companyPos.X, prefabStartZ + companyPos.Z,
                                rot,
                                originNode.X, originNode.Z);
                        }
                    }
                }
                var b = company.Overlay?.GetBitmap();
                if (b == null) continue;
                var overlayJObj = new JObject
                {
                    ["X"] = point.X,
                    ["Y"] = point.Y,
                    ["Name"] = overlayName,
                    ["Type"] = "Company",
                    ["Width"] = b.Width,
                    ["Height"] = b.Height,
                };
                overlaysJArr.Add(overlayJObj);
                if (saveAsPNG && !File.Exists(Path.Combine(overlayPath, $"{overlayName}.png")))
                    b.Save(Path.Combine(overlayPath, $"{overlayName}.png"));
            }
            foreach (var trigger in Triggers)
            {
                if (trigger.Hidden) continue;
                var overlayName = trigger.OverlayName;
                var b = trigger.Overlay?.GetBitmap();
                if (b == null) continue;
                var overlayJObj = new JObject
                {
                    ["X"] = trigger.X,
                    ["Y"] = trigger.Z,
                    ["Name"] = overlayName,
                    ["Type"] = "Parking",
                    ["Width"] = b.Width,
                    ["Height"] = b.Height,
                };
                overlaysJArr.Add(overlayJObj);
                if (saveAsPNG && !File.Exists(Path.Combine(overlayPath, $"{overlayName}.png")))
                    b.Save(Path.Combine(overlayPath, $"{overlayName}.png"));
            }
            foreach (var ferry in FerryConnections)
            {
                if (ferry.Hidden) continue;
                var overlayName = ScsHash.TokenToString(ferry.OverlayToken);
                var b = ferry.Overlay?.GetBitmap();
                if (b == null) continue;
                var overlayJObj = new JObject
                {
                    ["X"] = ferry.X,
                    ["Y"] = ferry.Z,
                    ["Name"] = overlayName,
                    ["Type"] = (ferry.Train) ? "Train" : "Ferry",
                    ["Width"] = b.Width,
                    ["Height"] = b.Height,
                };
                overlaysJArr.Add(overlayJObj);
                if (saveAsPNG && !File.Exists(Path.Combine(overlayPath, $"{overlayName}.png")))
                    b.Save(Path.Combine(overlayPath, $"{overlayName}.png"));
            }

            foreach (var prefab in Prefabs)
            {
                if (prefab.Hidden) continue;
                var originNode = GetNodeByUid(prefab.Nodes[0]);
                if (prefab.Prefab.PrefabNodes == null) continue;
                var mapPointOrigin = prefab.Prefab.PrefabNodes[prefab.Origin];

                var rot = (float) (originNode.Rotation - Math.PI -
                                   Math.Atan2(mapPointOrigin.RotZ, mapPointOrigin.RotX) + Math.PI / 2);

                var prefabStartX = originNode.X - mapPointOrigin.X;
                var prefabStartZ = originNode.Z - mapPointOrigin.Z;
                foreach (var spawnPoint in prefab.Prefab.SpawnPoints)
                {
                    var newPoint = RenderHelper.RotatePoint(prefabStartX + spawnPoint.X, prefabStartZ + spawnPoint.Z, rot,
                        originNode.X, originNode.Z);

                    var overlayJObj = new JObject
                    {
                        ["X"] = newPoint.X,
                        ["Y"] = newPoint.Y,
                    };

                    string overlayName;

                    switch (spawnPoint.Type)
                    {
                        case TsSpawnPointType.GasPos:
                            {
                                overlayName = "gas_ico";
                                overlayJObj["Type"] = "Fuel";
                                break;
                            }
                        case TsSpawnPointType.ServicePos:
                            {
                                overlayName = "service_ico";
                                overlayJObj["Type"] = "Service";
                                break;
                            }
                        case TsSpawnPointType.WeightStationPos:
                            {
                                overlayName = "weigh_station_ico";
                                overlayJObj["Type"] = "WeightStation";
                                break;
                            }
                        case TsSpawnPointType.TruckDealerPos:
                            {
                                overlayName = "dealer_ico";
                                overlayJObj["Type"] = "TruckDealer";
                                break;
                            }
                        case TsSpawnPointType.BuyPos:
                            {
                                overlayName = "garage_large_ico";
                                overlayJObj["Type"] = "Garage";
                                break;
                            }
                        case TsSpawnPointType.RecruitmentPos:
                            {
                                overlayName = "recruitment_ico";
                                overlayJObj["Type"] = "Recruitment";
                                break;
                            }
                        default:
                            continue;
                    }

                    overlayJObj["Name"] = overlayName;
                    var overlay = LookupOverlay(ScsHash.StringToToken(overlayName));
                    var b = overlay.GetBitmap();
                    if (b == null) continue;
                    overlayJObj["Width"] = b.Width;
                    overlayJObj["Height"] = b.Height;
                    overlaysJArr.Add(overlayJObj);
                    if (saveAsPNG && !File.Exists(Path.Combine(overlayPath, $"{overlayName}.png")))
                        b.Save(Path.Combine(overlayPath, $"{overlayName}.png"));

                }

                var lastId = -1;
                foreach (var triggerPoint in prefab.Prefab.TriggerPoints)
                {
                    var newPoint = RenderHelper.RotatePoint(prefabStartX + triggerPoint.X, prefabStartZ + triggerPoint.Z, rot,
                        originNode.X, originNode.Z);

                    if (triggerPoint.TriggerId == lastId) continue;
                    lastId = (int)triggerPoint.TriggerId;
                    var overlayJObj = new JObject
                    {
                        ["X"] = newPoint.X,
                        ["Y"] = newPoint.Y,
                        ["Name"] = "parking_ico",
                        ["Type"] = "Parking",
                    };

                    if (triggerPoint.TriggerActionToken != ScsHash.StringToToken("hud_parking")) continue;

                    const string overlayName = "parking_ico";
                    var overlay = LookupOverlay(ScsHash.StringToToken(overlayName));
                    var b = overlay.GetBitmap();
                    if (b == null) continue;
                    overlayJObj["Width"] = b.Width;
                    overlayJObj["Height"] = b.Height;
                    overlaysJArr.Add(overlayJObj);
                    if (saveAsPNG && !File.Exists(Path.Combine(overlayPath, $"{overlayName}.png")))
                        b.Save(Path.Combine(overlayPath, $"{overlayName}.png"));
                }
            }
            File.WriteAllText(Path.Combine(path, "Overlays.json"), overlaysJArr.ToString(Formatting.Indented));
        }

        public void UpdateEdgeCoords(TsNode node)
        {
            if (minX > node.X) minX = node.X;
            if (maxX < node.X) maxX = node.X;
            if (minZ > node.Z) minZ = node.Z;
            if (maxZ < node.Z) maxZ = node.Z;
        }

        public void UpdateLocalization(int index)
        {
            if (index < LocalizationList.Count)
            {
                SelectedLocalization = LocalizationList[index];
            }
        }

        public TsNode GetNodeByUid(ulong uid)
        {
            return Nodes.ContainsKey(uid) ? Nodes[uid] : null;
        }

        public TsCountry GetCountryByTokenName(string name)
        {
            var token = ScsHash.StringToToken(name);
            return _countriesLookup.ContainsKey(token) ? _countriesLookup[token] : null;
        }

        public TsRoadLook LookupRoadLook(ulong lookId)
        {
            return _roadLookup.ContainsKey(lookId) ? _roadLookup[lookId] : null;
        }

        public TsPrefab LookupPrefab(ulong prefabId)
        {
            return _prefabLookup.ContainsKey(prefabId) ? _prefabLookup[prefabId] : null;
        }

        public TsCity LookupCity(ulong cityId)
        {
            return _citiesLookup.ContainsKey(cityId) ? _citiesLookup[cityId] : null;
        }

        public TsMapOverlay LookupOverlay(ulong overlayId)
        {
            return _overlayLookup.ContainsKey(overlayId) ? _overlayLookup[overlayId] : null;
        }

        public List<TsFerryConnection> LookupFerryConnection(ulong ferryPortId)
        {
            return _ferryConnectionLookup.Where(item => item.StartPortToken == ferryPortId).ToList();
        }

        public void AddFerryPortLocation(ulong ferryPortId, float x, float z)
        {
            var ferry = _ferryConnectionLookup.Where(item => item.StartPortToken == ferryPortId || item.EndPortToken == ferryPortId);
            foreach (var connection in ferry)
            {
                connection.SetPortLocation(ferryPortId, x, z);
            }
        }
    }
}
