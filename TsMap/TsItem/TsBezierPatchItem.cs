using System.IO;
using TsMap.Helpers;
using TsMap.Helpers.Logger;
using TsMap.TsItem.Shared;

namespace TsMap.TsItem
{
    public class TsBezierPatchItem : TsItem
    {
        public TsBezierPatchItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = false;

            if (Sector.Version >= 884)
                TsBezierPatchItem884(startOffset);
            else
                Logger.Instance.Error($"Unknown base file version ({Sector.Version}) for item {Type} " +
                                      $"in file '{Path.GetFileName(Sector.FilePath)}' @ {startOffset} from '{Sector.GetUberFile().Entry.GetArchiveFile().GetPath()}'");
        }

        public void TsBezierPatchItem884(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var vegSphereCount =
                MemoryHelper.ReadInt32(Sector.Stream,
                    fileOffset += 0x05 + 0xF1); // 0x05(flags) + 0xF1(offset to vegSphereCount)
            fileOffset +=
                0x04 + VegetationSphereBlockSize *
                vegSphereCount; // 0x04(vegSphereCount) + vegSpheres
            fileOffset += QuadInfo.Parse(Sector, fileOffset);
            BlockSize = fileOffset - startOffset;
        }
    }
}
