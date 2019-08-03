using System.Collections.Generic;
using System.Text;
using TsMap.HashFiles;

namespace TsMap
{
    public class TsCity
    {
        private TsMapper _mapper;

        public string Name { get; set; }
        public string NameLocalized { get; set; }
        public string Country { get; set; }
        public ulong Token { get; set; }
        public List<int> XOffsets { get; }
        public List<int> YOffsets { get; }

        public TsCity(TsMapper mapper, string path)
        {
            _mapper = mapper;
            var file = _mapper.Rfs.GetFileEntry(path);

            if (file == null) return;

            var fileContent = file.Entry.Read();

            var lines = Encoding.UTF8.GetString(fileContent).Split('\n');
            var offsetCount = 0;
            XOffsets = new List<int>();
            YOffsets = new List<int>();

            foreach (var line in lines)
            {
                if (line.Contains("city_data"))
                {
                    Token = ScsHash.StringToToken(line.Split('.')[1].Trim());
                }
                else if (line.Contains("city_name") && !line.Contains("uppercase") && !line.Contains("short") && !line.Contains("localized"))
                {
                    Name = line.Split('"')[1];
                }
                else if (line.Contains("city_name_localized"))
                {
                    NameLocalized = line.Split('"')[1];
                    NameLocalized = NameLocalized.Substring(2, NameLocalized.Length - 4);
                }
                else if (line.Contains("country"))
                {
                    Country = line.Split(':')[1].Trim();
                }
                else if (line.Contains("map_x_offsets[]"))
                {
                    if (++offsetCount > 4)
                    {
                        var offset = 0;
                        if (int.TryParse(line.Split(':')[1].Trim(), out offset)) XOffsets.Add(offset);
                    }
                    if (offsetCount == 8) offsetCount = 0;
                }
                else if (line.Contains("map_y_offsets[]"))
                {
                    if (++offsetCount > 4)
                    {
                        var offset = 0;
                        if (int.TryParse(line.Split(':')[1].Trim(), out offset)) YOffsets.Add(offset);
                    }
                }
            }
        }
    }
}
