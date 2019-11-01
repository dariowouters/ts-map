using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TsMap.HashFiles;

namespace TsMap
{
    public class TsCity
    {
        private TsMapper _mapper;

        public string Name { get; set; }
        [JsonIgnore]
        public string LocalizationToken { get; set; }
        public string Country { get; set; }
        [JsonIgnore]
        public ulong Token { get; set; }
        [JsonIgnore]
        public List<int> XOffsets { get; }
        [JsonIgnore]
        public List<int> YOffsets { get; }
        public Dictionary<string, string> LocalizedNames { get; }

        public TsCity(TsMapper mapper, string path)
        {
            _mapper = mapper;
            var file = _mapper.Rfs.GetFileEntry(path);

            if (file == null) return;
            LocalizedNames = new Dictionary<string, string>();
            var fileContent = file.Entry.Read();

            var lines = Encoding.UTF8.GetString(fileContent).Split('\n');
            var offsetCount = 0;
            XOffsets = new List<int>();
            YOffsets = new List<int>();

            foreach (var line in lines)
            {
                var (validLine, key, value) = SiiHelper.ParseLine(line);
                if (!validLine) continue;

                if (key == "city_data")
                {
                    Token = ScsHash.StringToToken(SiiHelper.Trim(value.Split('.')[1]));
                }
                else if (key == "city_name")
                {
                    Name = line.Split('"')[1];
                }
                else if (key == "city_name_localized")
                {
                    LocalizationToken = value.Split('"')[1];
                    LocalizationToken = LocalizationToken.Replace("@", "");
                }
                else if (key == "country")
                {
                    Country = value;
                }
                else if (key.Contains("map_x_offsets[]"))
                {
                    if (++offsetCount > 4)
                    {
                        if (int.TryParse(value, out var offset)) XOffsets.Add(offset);
                    }
                    if (offsetCount == 8) offsetCount = 0;
                }
                else if (key.Contains("map_y_offsets[]"))
                {
                    if (++offsetCount > 4)
                    {
                        if (int.TryParse(value, out var offset)) YOffsets.Add(offset);
                    }
                }
            }
        }

        public void AddLocalizedName(string locale, string name)
        {
            if (!LocalizedNames.ContainsKey(locale)) LocalizedNames.Add(locale, name);
        }

        public string GetLocalizedName(string locale)
        {
            return (LocalizedNames.ContainsKey(locale)) ? LocalizedNames[locale] : Name;
        }
    }
}
