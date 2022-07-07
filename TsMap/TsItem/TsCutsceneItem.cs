using System.IO;
using TsMap.Helpers;
using TsMap.Helpers.Logger;

namespace TsMap.TsItem
{
    public class TsCutsceneItem : TsItem
    {
        public bool IsSecret { get; private set; }

        public TsCutsceneItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = false;

            if (Sector.Version >= 884)
                TsCutsceneItem844(startOffset);
            else
                Logger.Instance.Error($"Unknown base file version ({Sector.Version}) for item {Type} " +
                    $"in file '{Path.GetFileName(Sector.FilePath)}' @ {startOffset} from '{Sector.GetUberFile().Entry.GetArchiveFile().GetPath()}'");
        }

        public void TsCutsceneItem844(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            DlcGuard = MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x01);
            IsSecret = MemoryHelper.IsBitSet(MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x02), 4);
            var isViewpoint = MemoryHelper.ReadUint8(Sector.Stream, fileOffset) == 0;
            if (isViewpoint)
            {
                Valid = true;
            }
            var tagsCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x05); // 0x05(flags)
            var actionCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * tagsCount) + 0x08); // 0x04(tagsCount) + tags + 0x08(node_uid)
            fileOffset += 0x04; // 0x04(actionCount)
            for (var i = 0; i < actionCount; ++i)
            {
                var numParamCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
                var stringParamCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x04 * numParamCount)); // 0x04
                fileOffset += 0x04; // 0x04(stringParamCount)
                for (var s = 0; s < stringParamCount; ++s)
                {
                    var textLength = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
                    fileOffset += 0x04 + 0x04 + textLength; // 0x04(textLength, could be Uint64) + 0x04(padding) + textLength
                }
                var targetTagCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
                fileOffset += 0x04 + (targetTagCount * 0x08) + 0x08; // 0x04(targetTagCount) + targetTags + 0x08(target_range + action_flags)
            }
            BlockSize = fileOffset - startOffset;
        }
    }
}
