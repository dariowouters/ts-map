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

        public TsCity(TsMapper mapper, string path)
        {
            _mapper = mapper;
            var file = _mapper.Rfs.GetFileEntry(path);

            if (file == null) return;

            var fileContent = file.Entry.Read();

            var lines = Encoding.UTF8.GetString(fileContent).Split('\n');

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
            }
        }
    }
}
