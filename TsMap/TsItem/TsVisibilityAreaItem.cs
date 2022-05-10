﻿using System.IO;
using TsMap.Helpers;
using TsMap.Helpers.Logger;

namespace TsMap.TsItem
{
    public class TsVisibilityAreaItem : TsItem
    {
        public TsVisibilityAreaItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = false;

            if (Sector.Version >= 891)
                TsVisibilityAreaItem891(startOffset);
            else
                Logger.Instance.Error($"Unknown base file version ({Sector.Version}) for item {Type} " +
                    $"in file '{Path.GetFileName(Sector.FilePath)}' @ {startOffset} from '{Sector.GetUberFile().Entry.GetArchiveFile().GetPath()}'");
        }

        public void TsVisibilityAreaItem891(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var childrenCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x05 + 0x10); // 0x05(flags) + 0x10(node_uid, width, height)
            fileOffset += 0x04 + (0x08 * childrenCount); // 0x04(childrenCount) + childrenUids
            BlockSize = fileOffset - startOffset;
        }
    }
}
