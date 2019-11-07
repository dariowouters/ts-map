using System.IO;

namespace TsMap.TsItem
{
    public class TsFuelPumpItem : TsItem
    {
        public TsFuelPumpItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = false;
            if (Sector.Version < 855)
                TsFuelPumpItem825(startOffset);
            else if (Sector.Version >= 855)
                TsFuelPumpItem855(startOffset);
            else
                Log.Msg(
                    $"Unknown base file version ({Sector.Version}) for item {Type} in file '{Path.GetFileName(Sector.FilePath)}' @ {startOffset}.");
        }

        public void TsFuelPumpItem825(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            fileOffset += 0x05 + 0x10; // 0x05(flags) + 0x10(node_uid & prefab_uid)
            BlockSize = fileOffset - startOffset;
        }
        public void TsFuelPumpItem855(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var subItemUidCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x05 + 0x10); // 0x05(flags) + 0x10(2 uids)
            fileOffset += 0x04 + (0x08 * subItemUidCount); // 0x04(subItemUidCount) + subItemUids
            BlockSize = fileOffset - startOffset;
        }
    }
}
