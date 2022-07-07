using System.IO;
using TsMap.Common;
using TsMap.Helpers;
using TsMap.Helpers.Logger;

namespace TsMap.TsItem
{
    public class TsTriggerItem : TsItem
    {
        public string OverlayName { get; private set; }
        public TsMapOverlay Overlay { get; private set; }

        public bool IsSecret { get; private set; }

        public TsTriggerItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = true;
            if (Sector.Version < 829)
                TsTriggerItem825(startOffset);
            else if (Sector.Version >= 829 && Sector.Version < 875)
                TsTriggerItem829(startOffset);
            else if (Sector.Version >= 875)
                TsTriggerItem875(startOffset);
            else
                Logger.Instance.Error($"Unknown base file version ({Sector.Version}) for item {Type} " +
                    $"in file '{Path.GetFileName(Sector.FilePath)}' @ {startOffset} from '{Sector.GetUberFile().Entry.GetArchiveFile().GetPath()}'");
        }

        public void TsTriggerItem825(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            DlcGuard = MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x01);
            var nodeCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x05); // 0x05(flags)
            var tagCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * nodeCount)); // 0x04(nodeCount) + nodeUids
            var triggerActionCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * tagCount)); // 0x04(tagCount) + tags
            fileOffset += 0x04; // cursor after triggerActionCount

            for (var i = 0; i < triggerActionCount; i++)
            {
                var action = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset);
                if (action == ScsToken.StringToToken("hud_parking"))
                {
                    OverlayName = "parking_ico";
                    Overlay = Sector.Mapper.LookupOverlay(OverlayName, OverlayTypes.Map);
                    if (Overlay == null)
                    {
                        Logger.Instance.Error("Could not find parking overlay");
                        Valid = false;
                    }
                }
                var hasParameters = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x08); // 0x08(action)
                fileOffset += 0x04; // set cursor after hasParameters
                if (hasParameters == 1)
                {
                    var parametersLength = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
                    fileOffset += 0x04 + 0x04 + parametersLength; // 0x04(parametersLength) + 0x04(padding) + text(parametersLength * 0x01)
                }
                else if (hasParameters == 3) fileOffset += 0x08; // 0x08 (m_some_uid)

                var targetTagCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
                fileOffset += 0x04 + targetTagCount * 0x08; // 0x04(targetTagCount) + targetTags
            }

            fileOffset += 0x18; // 0x18(range & reset_delay & reset_distance & min_speed & max_speed & flags2)
            BlockSize = fileOffset - startOffset;
        }
        public void TsTriggerItem829(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            DlcGuard = MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x01);
            var tagCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x05); // 0x05(flags)
            var nodeCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * tagCount)); // 0x04(nodeCount) + tags

            var triggerActionCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * nodeCount)); // 0x04(nodeCount) + nodeUids
            fileOffset += 0x04; // cursor after triggerActionCount

            for (var i = 0; i < triggerActionCount; i++)
            {
                var action = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset);
                if (action == ScsToken.StringToToken("hud_parking"))
                {
                    OverlayName = "parking_ico";
                    Overlay = Sector.Mapper.LookupOverlay(OverlayName, OverlayTypes.Map);
                    if (Overlay == null)
                    {
                        Logger.Instance.Error("Could not find parking overlay");
                        Valid = false;
                    }
                }

                var hasOverride = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x08); // 0x08(action)
                if (hasOverride > 0) fileOffset += 0x04 * hasOverride;

                var hasParameters = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04); // 0x04(hasOverride)
                fileOffset += 0x04; // set cursor after hasParameters
                if (hasParameters == 1)
                {
                    var parametersLength = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
                    fileOffset += 0x04 + 0x04 + parametersLength; // 0x04(parametersLength) + 0x04(padding) + text(parametersLength * 0x01)
                }
                else if (hasParameters == 3) fileOffset += 0x08; // 0x08 (m_some_uid)

                var targetTagCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x08); // 0x08(unk/padding)
                fileOffset += 0x04 + targetTagCount * 0x08; // 0x04(targetTagCount) + targetTags
            }

            fileOffset += 0x18; // 0x18(range & reset_delay & reset_distance & min_speed & max_speed & flags2)
            BlockSize = fileOffset - startOffset;
        }

        public void TsTriggerItem875(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            DlcGuard = MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x01);
            IsSecret = MemoryHelper.IsBitSet(MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x02), 2);
            var tagCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x05); // 0x05(flags)
            var nodeCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * tagCount)); // 0x04(nodeCount) + tags

            var triggerActionCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * nodeCount)); // 0x04(nodeCount) + nodeUids
            fileOffset += 0x04; // cursor after triggerActionCount

            for (var i = 0; i < triggerActionCount; i++)
            {
                var action = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset);
                if (action == ScsToken.StringToToken("hud_parking"))
                {
                    OverlayName = "parking_ico";
                    Overlay = Sector.Mapper.LookupOverlay(OverlayName, OverlayTypes.Map);
                    if (Overlay == null)
                    {
                        Logger.Instance.Error("Could not find parking overlay");
                        Valid = false;
                    }
                }

                var hasOverride = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x08); // 0x08(action)
                fileOffset += 0x04; // set cursor after hasOverride
                if (hasOverride < 0) continue;
                fileOffset += 0x04 * hasOverride; // set cursor after override values

                var parameterCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
                fileOffset += 0x04; // set cursor after parameterCount

                for (var j = 0; j < parameterCount; j++)
                {
                    var paramLength = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
                    fileOffset += 0x04 + 0x04 + paramLength; // 0x04(paramLength) + 0x04(padding) + (param)
                }
                var targetTagCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);

                fileOffset += 0x04 + targetTagCount * 0x08 + 0x08; // 0x04(targetTagCount) + targetTags + 0x04(m_range & m_type)
            }

            if (nodeCount == 1) fileOffset += 0x04; // 0x04(m_radius)
            BlockSize = fileOffset - startOffset;
        }
    }
}
