using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TsMap.Common;
using TsMap.FileSystem;
using TsMap.Helpers;
using TsMap.Helpers.Logger;
using TsMap.TsItem;

namespace TsMap
{
    public class TsMapper
    {
        private readonly string _gameDir;
        private List<Mod> _mods;

        public bool IsEts2 = true;

        private List<string> _sectorFiles;

        public LocalizationManager Localization { get; private set; }

        private readonly Dictionary<ulong, TsPrefab> _prefabLookup = new Dictionary<ulong, TsPrefab>();
        private readonly Dictionary<ulong, TsCity> _citiesLookup = new Dictionary<ulong, TsCity>();
        private readonly Dictionary<ulong, TsCountry> _countriesLookup = new Dictionary<ulong, TsCountry>();
        private readonly Dictionary<ulong, TsRoadLook> _roadLookup = new Dictionary<ulong, TsRoadLook>();
        private readonly Dictionary<ulong, TsMapOverlay> _overlayCache = new Dictionary<ulong, TsMapOverlay>();
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
            Localization = new LocalizationManager();
        }

        private void ParseCityFiles()
        {
            var defDirectory = UberFileSystem.Instance.GetDirectory("def");
            if (defDirectory == null)
            {
                Logger.Instance.Error("Could not read 'def' dir");
                return;
            }

            foreach (var cityFileName in defDirectory.GetFiles("city"))
            {
                var cityFile = UberFileSystem.Instance.GetFile($"def/{cityFileName}");

                var data = cityFile.Entry.Read();
                var lines = Encoding.UTF8.GetString(data).Split('\n');
                foreach (var line in lines)
                {
                    if (line.TrimStart().StartsWith("#")) continue;
                    if (line.Contains("@include"))
                    {
                        var path = PathHelper.GetFilePath(line.Split('"')[1], "def");
                        var city = new TsCity(path);
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
            var defDirectory = UberFileSystem.Instance.GetDirectory("def");
            if (defDirectory == null)
            {
                Logger.Instance.Error("Could not read 'def' dir");
                return;
            }

            foreach (var countryFilePath in defDirectory.GetFiles("country"))
            {
                var countryFile = UberFileSystem.Instance.GetFile($"def/{countryFilePath}");

                var data = countryFile.Entry.Read();
                var lines = Encoding.UTF8.GetString(data).Split('\n');
                foreach (var line in lines)
                {
                    if (line.TrimStart().StartsWith("#")) continue;
                    if (line.Contains("@include"))
                    {
                        var path = PathHelper.GetFilePath(line.Split('"')[1], "def");
                        var country = new TsCountry(path);
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
            var worldDirectory = UberFileSystem.Instance.GetDirectory("def/world");
            if (worldDirectory == null)
            {
                Logger.Instance.Error("Could not read 'def/world' dir");
                return;
            }

            foreach (var prefabFileName in worldDirectory.GetFiles("prefab"))
            {
                if (!prefabFileName.StartsWith("prefab")) continue;
                var prefabFile = UberFileSystem.Instance.GetFile($"def/world/{prefabFileName}");

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
                            token = ScsToken.StringToToken(SiiHelper.Trim(value.Split('.')[1]));
                        }
                        else if (key == "prefab_desc")
                        {
                            path = PathHelper.EnsureLocalPath(value.Split('"')[1]);
                        }
                        else if (key == "category")
                        {
                            category = value.Contains('"') ? value.Split('"')[1] : value.Trim();
                        }
                    }

                    if (line.Contains("}") && token != 0 && path != "")
                    {
                        var prefab = new TsPrefab(path, token, category);
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
            var worldDirectory = UberFileSystem.Instance.GetDirectory("def/world");
            if (worldDirectory == null)
            {
                Logger.Instance.Error("Could not read 'def/world' dir");
                return;
            }

            foreach (var roadLookFileName in worldDirectory.GetFiles("road_look"))
            {
                if (!roadLookFileName.StartsWith("road")) continue;
                var roadLookFile = UberFileSystem.Instance.GetFile($"def/world/{roadLookFileName}");

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
                            roadLook = new TsRoadLook(ScsToken.StringToToken(SiiHelper.Trim(value.Split('.')[1].Trim('{'))));
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
            var connectionDirectory = UberFileSystem.Instance.GetDirectory("def/ferry/connection");
            if (connectionDirectory == null)
            {
                Logger.Instance.Error("Could not read 'def/ferry/connection' dir");
                return;
            }

            foreach (var ferryConnectionFilePath in connectionDirectory.GetFilesByExtension("def/ferry/connection", ".sui", ".sii"))
            {
                var ferryConnectionFile = UberFileSystem.Instance.GetFile(ferryConnectionFilePath);

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
                                StartPortToken = ScsToken.StringToToken(portIds[1]),
                                EndPortToken = ScsToken.StringToToken(portIds[2].TrimEnd('{').Trim())
                            };
                        }
                    }

                    if (!line.Contains("}") || conn == null) continue;

                    var existingItem = _ferryConnectionLookup.FirstOrDefault(item =>
                        (item.StartPortToken == conn.StartPortToken && item.EndPortToken == conn.EndPortToken) ||
                        (item.StartPortToken == conn.EndPortToken && item.EndPortToken == conn.StartPortToken)); // Check if connection already exists
                    if (existingItem == null) _ferryConnectionLookup.Add(conn);
                    conn = null;
                }
            }
        }

        private TsMapOverlay GetOverlayFromMatFile(string matFilePath)
        {
            var matFile = UberFileSystem.Instance.GetFile(matFilePath);
            if (matFile == null) return null;

            var data = matFile.Entry.Read();
            var lines = Encoding.UTF8.GetString(data).Split('\n');

            foreach (var line in lines)
            {
                var (validLine, key, value) = SiiHelper.ParseLine(line);
                if (!validLine) continue;
                if (key == "texture")
                {
                    var tobjPath = PathHelper.CombinePath(PathHelper.GetDirectoryPath(matFilePath), value.Split('"')[1]);

                    var tobjData = UberFileSystem.Instance.GetFile(tobjPath)?.Entry?.Read();

                    if (tobjData == null)
                    {
                        Logger.Instance.Warning($"Could not read tobj file for '{tobjPath}'");
                        break;
                    }

                    var path = PathHelper.EnsureLocalPath(Encoding.UTF8.GetString(tobjData, 0x30, tobjData.Length - 0x30));

                    var name = PathHelper.GetFileNameFromPath(matFilePath);
                    if (name.StartsWith("map")) break; // skip the large unused background files

                    return new TsMapOverlay(path);
                }
            }
            return null;
        }

        /// <summary>
        /// Parse all definition files
        /// </summary>
        private void ParseDefFiles()
        {
            var startTime = DateTime.Now.Ticks;
            ParseCityFiles();
            Logger.Instance.Info($"Loaded {_citiesLookup.Count} cities in {(DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond}ms");

            startTime = DateTime.Now.Ticks;
            ParseCountryFiles();
            Logger.Instance.Info($"Loaded {_countriesLookup.Count} countries in {(DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond}ms");

            startTime = DateTime.Now.Ticks;
            ParsePrefabFiles();
            Logger.Instance.Info($"Loaded {_prefabLookup.Count} prefabs in {(DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond}ms");

            startTime = DateTime.Now.Ticks;
            ParseRoadLookFiles();
            Logger.Instance.Info($"Loaded {_roadLookup.Count} roads in {(DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond}ms");

            startTime = DateTime.Now.Ticks;
            ParseFerryConnections();
            Logger.Instance.Info($"Loaded {_ferryConnectionLookup.Count} ferry connections in {(DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond}ms");
        }

        /// <summary>
        /// Parse all .base files
        /// </summary>
        private void LoadSectorFiles()
        {
            var baseMapEntry = UberFileSystem.Instance.GetDirectory("map");
            if (baseMapEntry == null)
            {
                Logger.Instance.Error("Could not read 'map' dir");
                return;
            }

            var mbdFilePaths = baseMapEntry.GetFilesByExtension("map", ".mbd"); // Get the map names from the mbd files
            if (mbdFilePaths.Count == 0)
            {
                Logger.Instance.Error("Could not find mbd file");
                return;
            }

            _sectorFiles = new List<string>();

            foreach (var filePath in mbdFilePaths)
            {
                var mapName = PathHelper.GetFileNameWithoutExtensionFromPath(filePath);
                IsEts2 = !(mapName == "usa");

                var mapFileDir = UberFileSystem.Instance.GetDirectory($"map/{mapName}");
                if (mapFileDir == null)
                {
                    Logger.Instance.Error($"Could not read 'map/{mapName}' directory");
                    return;
                }

                _sectorFiles.AddRange(mapFileDir.GetFilesByExtension($"map/{mapName}", ".base"));
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
                Logger.Instance.Error("Could not find Game directory.");
                return;
            }

            UberFileSystem.Instance.AddSourceDirectory(_gameDir);

            _mods.Reverse(); // Highest priority mods (top) need to be loaded last

            foreach (var mod in _mods)
            {
                if (mod.Load) UberFileSystem.Instance.AddSourceFile(mod.ModPath);
            }

            Logger.Instance.Info($"Loaded all .scs files in {(DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond}ms");

            ParseDefFiles();
            LoadSectorFiles();

            var preLocaleTime = DateTime.Now.Ticks;
            Localization.LoadLocaleValues();
            Logger.Instance.Info($"It took {(DateTime.Now.Ticks - preLocaleTime) / TimeSpan.TicksPerMillisecond} ms to read all locale files");

            if (_sectorFiles == null) return;
            var preMapParseTime = DateTime.Now.Ticks;
            Sectors = _sectorFiles.Select(file => new TsSector(this, file)).ToList();
            Sectors.ForEach(sec => sec.Parse());
            Sectors.ForEach(sec => sec.ClearFileData());
            Logger.Instance.Info($"It took {(DateTime.Now.Ticks - preMapParseTime) / TimeSpan.TicksPerMillisecond} ms to parse all (*.base) files");

            var invalidFerryConnections = _ferryConnectionLookup.Where(x => x.StartPortLocation == PointF.Empty || x.EndPortLocation == PointF.Empty).ToList();
            foreach (var invalidFerryConnection in invalidFerryConnections)
            {
                _ferryConnectionLookup.Remove(invalidFerryConnection);
                Logger.Instance.Debug($"Ignored ferry connection " +
                    $"'{ScsToken.TokenToString(invalidFerryConnection.StartPortToken)}-{ScsToken.TokenToString(invalidFerryConnection.EndPortToken)}' " +
                    $"due to not having Start/End location set.");
            }

            Logger.Instance.Info($"Loaded {_overlayCache.Count} overlays");
            Logger.Instance.Info($"It took {(DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond} ms to fully load.");
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
                if (_countriesLookup.ContainsKey(ScsToken.StringToToken(city.City.Country)))
                {
                    var country = _countriesLookup[ScsToken.StringToToken(city.City.Country)];
                    cityJObj["CountryId"] = country.CountryId;
                }
                else
                {
                    Logger.Instance.Warning($"Could not find country for {city.City.Name}");
                }

                if (exportFlags.IsActive(ExportFlags.CityLocalizedNames))
                {
                    cityJObj["LocalizedNames"] = new JObject();
                    foreach(var locale in Localization.GetLocales())
                    {
                        var locCityName = Localization.GetLocaleValue(city.City.LocalizationToken, locale);
                        if (locCityName != null)
                        {
                            cityJObj["LocalizedNames"][locale] = locCityName;
                        }
                    }
                }

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
                if (exportFlags.IsActive(ExportFlags.CountryLocalizedNames))
                {
                    countryJObj["LocalizedNames"] = new JObject();
                    foreach (var locale in Localization.GetLocales())
                    {
                        var locCountryName = Localization.GetLocaleValue(country.LocalizationToken, locale);
                        if (locCountryName != null)
                        {
                            countryJObj["LocalizedNames"][locale] = locCountryName;
                        }
                    }
                }
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
                var overlayName = ScsToken.TokenToString(company.OverlayToken);
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
                var overlayName = ScsToken.TokenToString(ferry.OverlayToken);
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
                    var overlay = LookupOverlay(overlayName, OverlayTypes.Map);
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

                    if (triggerPoint.TriggerActionToken != ScsToken.StringToToken("hud_parking")) continue;

                    const string overlayName = "parking_ico";
                    var overlay = LookupOverlay(overlayName, OverlayTypes.Map);
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

        public TsNode GetNodeByUid(ulong uid)
        {
            return Nodes.ContainsKey(uid) ? Nodes[uid] : null;
        }

        public TsCountry GetCountryByTokenName(string name)
        {
            var token = ScsToken.StringToToken(name);
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

        /// <summary>
        /// Get's the overlay for the given overlayName and overlayType from the cache or from the filesystem, returning null if it does not exist in either.
        /// </summary>
        /// <param name="overlayName">Name of the icon, road icons should not have 'road_' as a prefix</param>
        /// <param name="overlayType">Determines the path where it checks for the overlay.
        /// <para>If <see cref="OverlayTypes.Custom"/> is used, a full path to a .mat file should be provided in <paramref name="overlayName"/></para></param>
        /// <returns>
        /// <para><see cref="TsMapOverlay"></see> if found</para>
        /// <para>Null if not</para>
        /// </returns>
        public TsMapOverlay LookupOverlay(string overlayName, OverlayTypes overlayType)
        {
            if (overlayName == "") return null;
            ulong hash;
            string path;
            switch (overlayType)
            {
                case OverlayTypes.Road:
                    path = $"material/ui/map/road/road_{overlayName}.mat";
                    break;
                case OverlayTypes.Company:
                    path = $"material/ui/company/small/{overlayName}.mat";
                    break;
                case OverlayTypes.Map:
                    path = $"material/ui/map/{overlayName}.mat";
                    break;
                default:
                    path = PathHelper.EnsureLocalPath(overlayName);
                    break;
            }

            hash = CityHash.CityHash64(path);

            if (_overlayCache.ContainsKey(hash))
            {
                return _overlayCache[hash];
            }
            else
            {
                var overlay = GetOverlayFromMatFile(path);
                if (overlay != null)
                {
                    if (!_overlayCache.ContainsKey(hash))
                    {
                        _overlayCache.Add(hash, overlay);
                        return overlay;
                    }
                }
            }
            return null;
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
