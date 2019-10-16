using System.IO;
using TsMap.HashFiles;

namespace TsMap.TsItem
{
    public class TsFerryItem : TsItem
    {
        public ulong FerryPortId { get; private set; }
        public bool Train { get; private set; }
        public TsMapOverlay Overlay { get; private set; }

        public TsFerryItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = true;
            TsFeryItem825(startOffset);
        }
        public void TsFeryItem825(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            Train = MemoryHelper.ReadUint8(Sector.Stream, fileOffset) != 0;
            if (Train) Overlay = Sector.Mapper.LookupOverlay(ScsHash.StringToToken("train_ico"));
            else Overlay = Sector.Mapper.LookupOverlay(ScsHash.StringToToken("port_overlay"));

            FerryPortId = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05);
            Sector.Mapper.AddFerryPortLocation(FerryPortId, X, Z);
            fileOffset += 0x08 + 0x1C; // 0x08(ferryPorId) + 0x1C(prefab_uid & node_uid & unloadoffset)
            BlockSize = fileOffset - startOffset;
        }
    }
}
