using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace TsMap
{
    public class TsMapper
    {
        private readonly string _prefabDir;
        private readonly string _jsonLutDir;
        private readonly string _siiLutDir;

        private readonly string[] _sectorFiles;

        private readonly Dictionary<ulong, TsPrefab> _prefabLookup = new Dictionary<ulong, TsPrefab>();
        private readonly Dictionary<ulong, string> _citiesLookup = new Dictionary<ulong, string>();
        private readonly List<TsRoadLook> _roadLookLookup = new List<TsRoadLook>();
        private readonly Dictionary<ulong, TsRoadLook> _roadLookup = new Dictionary<ulong, TsRoadLook>();

        public readonly Dictionary<ulong, TsItem> Items = new Dictionary<ulong, TsItem>();
        public readonly Dictionary<ulong, TsNode> Nodes = new Dictionary<ulong, TsNode>();

        private List<TsSector> Sectors { get; set; }

        public TsMapper(string sectorDir, string prefabDir, string siiLutDir, string jsonLutDir)
        {
            _prefabDir = prefabDir;
            _siiLutDir = siiLutDir;
            _jsonLutDir = jsonLutDir;

            if (!Directory.Exists(sectorDir))
            {
                Log.Msg("Could not find the Sector Dir.");
                return;
            }
            _sectorFiles = Directory.GetFiles(sectorDir, "*.base");

        }

        public TsMapper(string mainDir) : this(mainDir + @"SCS/map/", mainDir + @"SCS/prefab/", mainDir + @"SCS/LUT/", mainDir + @"LUT/")
        {
           
        }

        private void LoadLut()
        {
            // PREFABS
            var prefabJson = _jsonLutDir + "prefabs.json";

            if (File.Exists(prefabJson))
            {
                var lines = File.ReadAllLines(prefabJson);
                var filePath = "";
                ulong token = 0;
                foreach (var line in lines)
                {
                    if (line.Trim() == "]")
                    {
                        break;
                    }
                    if (line.Contains("token"))
                    {
                        token = ulong.Parse(line.Split(':')[1].Split('"')[1], NumberStyles.HexNumber);
                    }
                    if (line.Contains("prefab_desc"))
                    {
                        filePath = line.Split(':')[1].Split('"')[1];
                    }
                    if (line.Contains("}"))
                    {
                        if (token != 0 && filePath != "")
                        {
                            filePath = filePath.Substring(filePath.IndexOf('/', 1) + 1);
                            if (File.Exists(_prefabDir + filePath))
                            {
                                _prefabLookup.Add(token, new TsPrefab(_prefabDir + filePath));
                            }
                        }

                        filePath = "";
                        token = 0;
                    }
                }

            }
            else
            {
                Log.Msg($"Cannot find file: {prefabJson}");
            }

            // CITIES
            if (File.Exists(_jsonLutDir + "cities.json"))
            {

                var lines = File.ReadAllLines(_jsonLutDir + "cities.json");
                var cityName = "";
                ulong token = 0;
                foreach (var line in lines)
                {
                    if (line.Trim() == "]")
                    {
                        break;
                    }
                    if (line.Contains(":"))
                    {
                        var k = line.Split(':')[0].Split('"')[1];
                        var v = line.Split(':')[1].Split('"')[1];

                        switch (k)
                        {
                            case "token":
                                token = ulong.Parse(v, NumberStyles.HexNumber);
                                break;
                            case "fullName":
                                cityName = v;
                                break;
                        }
                    }
                    if (!line.Contains("}")) continue;
                    if (token != 0 && cityName != "")
                    {
                        _citiesLookup.Add(token, cityName);
                    }

                    cityName = "";
                    token = 0;
                }
            }
            else
            {
                Log.Msg($"Cannot find file: {_jsonLutDir + "cities.json"}");
            }

            // ROAD LOOKS
            var roadsJson = _jsonLutDir + "roads.json";
            if (File.Exists(roadsJson))
            {
                var lines = File.ReadAllLines(roadsJson);
                var idName = "";
                ulong token = 0;
                foreach (var line in lines)
                {
                    if (line.Trim() == "]")
                    {
                        break;
                    }
                    if (line.Contains(":"))
                    {
                        var k = line.Split(':')[0].Split('"')[1];
                        var v = line.Split(':')[1].Split('"')[1];

                        switch (k)
                        {
                            case "token":
                                token = ulong.Parse(v, NumberStyles.HexNumber);
                                break;
                            case "idName":
                                idName = v;
                                break;
                        }
                    }

                    if (!line.Contains("}")) continue;
                    if (token != 0 && idName != "")
                    {
                        var obj = _roadLookLookup.FirstOrDefault(x => x.LookId == idName);
                        if (obj != null)
                        {
                            _roadLookup.Add(token, obj);
                        }
                    }

                    idName = "";
                    token = 0;
                }

                _roadLookLookup.Clear();
            }
            else
            {
                Log.Msg($"Cannot find file: {roadsJson}");
            }
        }

        private void ParseRoadLookFiles()
        {
            if (!Directory.Exists(_siiLutDir + @"road\"))
            {
                Log.Msg("No roadlook files found");
                return;
            }
            var roadLookFiles = Directory.GetFiles(_siiLutDir + @"road\", "*.sii");

            roadLookFiles.ToList().ForEach(file =>
            {
                var road = String.Empty;
                var fileData = File.ReadLines(file);
                TsRoadLook look = null;
                foreach (var k in fileData)
                {
                    if (k.Contains(":") && !road.Equals(String.Empty) && look != null)
                    {
                        var key = k;
                        var data = key.Substring(key.IndexOf(':') + 1).Trim();
                        key = key.Substring(0, key.IndexOf(':')).Trim();

                        switch (key)
                        {
                            case "road_size_left":
                                float.TryParse(data.Replace('.', ','), out look.SizeLeft);
                                break;

                            case "road_size_right":
                                float.TryParse(data.Replace('.', ','), out look.SizeRight);
                                break;

                            case "shoulder_size_right":
                                float.TryParse(data.Replace('.', ','), out look.ShoulderLeft);
                                break;

                            case "shoulder_size_left":
                                float.TryParse(data.Replace('.', ','), out look.ShoulderRight);
                                break;

                            case "road_offset":
                                float.TryParse(data.Replace('.', ','), out look.Offset);
                                break;
                            case "lanes_left[]":
                                look.LanesLeft.Add(data);
                                break;

                            case "lanes_right[]":
                                look.LanesRight.Add(data);
                                break;
                        }
                    }
                    if (k.StartsWith("road_look"))
                    {
                        var d = k.Split(':');
                        d[1] = d[1].Trim();
                        if (d[1].Length > 3)
                        {
                            road = d[1].Substring(0, d[1].Length - 1).Trim();
                            look = new TsRoadLook(road);
                        }
                    }
                    if (k.Trim() != "}") continue;
                    if (look != null && !_roadLookLookup.Contains(look))
                    {
                        _roadLookLookup.Add(look);
                    }
                    road = String.Empty;
                }
            });
        }

        public void Parse()
        {
            ParseRoadLookFiles();
            Log.Msg($"RoadLook Count: {_roadLookLookup.Count}");
            LoadLut();
            Log.Msg($"Roads Count: {_roadLookup.Count}");
            Log.Msg($"Prefabs Count: {_prefabLookup.Count}");
            Log.Msg($"Cities Count: {_citiesLookup.Count}");

            Sectors = _sectorFiles.Select(file => new TsSector(this, file)).ToList();
            Sectors.ForEach(sec => sec.Parse());
            Sectors.ForEach(sec => sec.ClearFileData());
            Log.Msg($"# Items: {Items.Count}, # nodes: {Nodes.Count}");
        }

        public TsNode GetNodeByUid(ulong uid)
        {
            return Nodes.ContainsKey(uid) ? Nodes[uid] : null;
        }

        public TsRoadLook LookupRoadLook(ulong lookId)
        {
            return _roadLookup.ContainsKey(lookId) ? _roadLookup[lookId] : null;
        }

        public TsPrefab LookupPrefab(ulong prefabId)
        {
            return _prefabLookup.ContainsKey(prefabId) ? _prefabLookup[prefabId] : null;
        }

        public string LookupCity(ulong cityId)
        {
            return _citiesLookup.ContainsKey(cityId) ? _citiesLookup[cityId] : null;
        }
    }
}
