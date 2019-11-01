using System.IO;
using TsMap.HashFiles;

namespace TsMap.TsItem
{
    public class TsCityItem : TsItem // TODO: Add zoom levels/range to show city names and icons correctly
    {
        public TsCity City { get; private set; }
        public ulong NodeUid { get; private set; }

        public TsCityItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = true;
            TsCityItem825(startOffset);
        }

        public void TsCityItem825(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags

            Hidden = (MemoryHelper.ReadUint8(Sector.Stream, fileOffset) & 0x01) != 0;
            var cityId = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset + 0x05);
            City = Sector.Mapper.LookupCity(cityId);
            if (City == null)
            {
                Valid = false;
                Log.Msg($"Could not find City: '{ScsHash.TokenToString(cityId)}'({cityId:X}), " +
                        $"in {Path.GetFileName(Sector.FilePath)} @ {fileOffset}");
            }
            NodeUid = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05 + 0x08 + 0x08); // 0x05(flags) + 0x08(cityId) + 0x08(width & height)
            fileOffset += 0x08; // nodeUid
            BlockSize = fileOffset - startOffset;
        }

        public override string ToString()
        {
            if (City == null) return "Error";
            var country = Sector.Mapper.GetCountryByTokenName(City.Country);
            var countryName = (country == null)
                ? City.Country
                : country.GetLocalizedName(Sector.Mapper.SelectedLocalization);
            return $"{countryName} - {City.GetLocalizedName(Sector.Mapper.SelectedLocalization)}";
        }
    }
}
