using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TsMap.Common;
using TsMap.FileSystem;

namespace TsMap
{
    public class TsCountry
    {
        [JsonIgnore]
        public ulong Token { get; }

        public int CountryId { get; }
        public string Name { get; }
        [JsonIgnore]
        public string LocalizationToken { get; }
        public string CountryCode { get; }
        public float X { get; }
        public float Y { get; }

        public Dictionary<TsVehicleType, Dictionary<string, Dictionary<TsSpeedType, float>>> Speeds = new Dictionary<TsVehicleType, Dictionary<string, Dictionary<TsSpeedType, float>>>();

        public TsCountry(string path)
        {
            var file = UberFileSystem.Instance.GetFile(path);

            if (file == null) return;

            var fileContent = file.Entry.Read();

            var lines = Encoding.UTF8.GetString(fileContent).Split('\n');

            foreach (var line in lines)
            {
                var (validLine, key, value) = SiiHelper.ParseLine(line);
                if (!validLine) continue;

                if (key == "country_data")
                {
                    Token = ScsToken.StringToToken(SiiHelper.Trim(value.Split('.')[2]));
                }
                else if (key == "country_id")
                {
                    CountryId = int.Parse(value);
                }
                else if (key == "name")
                {
                    Name = value.Split('"')[1];
                }
                else if (key == "name_localized")
                {
                    LocalizationToken = value.Split('"')[1];
                    LocalizationToken = LocalizationToken.Replace("@", "");
                }
                else if (key == "country_code")
                {
                    CountryCode = value.Split('"')[1];
                }
                else if (key == "pos")
                {
                    var vector = value.Split('(')[1].Split(')')[0];
                    var values = vector.Split(',');
                    X = float.Parse(values[0], CultureInfo.InvariantCulture);
                    Y = float.Parse(values[2], CultureInfo.InvariantCulture);
                }
            }

            var fileSpeed = UberFileSystem.Instance.GetFile(path.Split('.')[0] + "/speed_limits.sii");
            if (fileSpeed == null) return;
            fileContent = fileSpeed.Entry.Read();
            lines = Encoding.UTF8.GetString(fileContent).Split('\n');

            TsVehicleType vehicle = default;
            string road = "";
            foreach (var line in lines)
            {
                string newLine = line.Split(new[] { "//" }, StringSplitOptions.None)[0].Split('#')[0];
                if (newLine.Contains("\tvehicle_speed_class:"))
                {
                    switch (newLine.Split(':')[1].Trim())
                    {
                        case "car": vehicle = TsVehicleType.Car; break;
                        case "bus": vehicle = TsVehicleType.Bus; break;
                        case "tram": vehicle = TsVehicleType.Tram; break;
                        case "truck": vehicle = TsVehicleType.Truck; break;
                        case "train": vehicle = TsVehicleType.Train; break;
                    }
                    Speeds.Add(vehicle, new Dictionary<string, Dictionary<TsSpeedType, float>>());
                }
                else if (newLine.Contains("limit[]:"))
                {
                    string[] speedLine = newLine.Split(':');
                    float speed = float.Parse(speedLine[1], CultureInfo.InvariantCulture);
                    switch (speedLine[0].Trim())
                    {
                        case "limit[]": Speeds[vehicle][road][TsSpeedType.Limit] = speed; break;
                        case "max_limit[]": Speeds[vehicle][road][TsSpeedType.MaxLimit] = speed; break;
                        case "urban_limit[]": Speeds[vehicle][road][TsSpeedType.UrbanLimit] = speed; break;
                    }
                }
                else if (newLine.Contains("\tlane_speed_class[]:"))
                {
                    road = newLine.Split(':')[1].Trim();
                    Speeds[vehicle][road] = new Dictionary<TsSpeedType, float>();
                }
            }
        }
    }
}
