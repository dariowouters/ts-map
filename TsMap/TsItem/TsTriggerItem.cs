using System;
using System.IO;

namespace TsMap.TsItem
{
    public class TsTriggerItem : TsItem
    {
        public TsMapOverlay Overlay { get; private set; }

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
                Log.Msg(
                    $"Unknown base file version ({Sector.Version}) for item {Type} in file '{Path.GetFileName(Sector.FilePath)}' @ {startOffset}.");
        }

        public void TsTriggerItem825(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x01) > dlcGuardCount;
            var nodeCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x05); // 0x05(flags)
            var tagCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * nodeCount)); // 0x04(nodeCount) + nodeUids
            var triggerActionCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * tagCount)); // 0x04(tagCount) + tags
            fileOffset += 0x04; // cursor after triggerActionCount

            for (var i = 0; i < triggerActionCount; i++)
            {
                var action = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset);
                if (action == 0x18991B7A99E279C) // hud_parking
                {
                    Overlay = Sector.Mapper.LookupOverlay(0x2358E762E112CD4);
                    if (Overlay == null)
                    {
                        Console.WriteLine("Could not find parking overlay");
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
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x01) > dlcGuardCount;
            var tagCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x05); // 0x05(flags)
            var nodeCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * tagCount)); // 0x04(nodeCount) + tags

            var triggerActionCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * nodeCount)); // 0x04(nodeCount) + nodeUids
            fileOffset += 0x04; // cursor after triggerActionCount

            for (var i = 0; i < triggerActionCount; i++)
            {
                var action = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset);
                if (action == 0x18991B7A99E279C) // hud_parking
                {
                    Overlay = Sector.Mapper.LookupOverlay(0x2358E762E112CD4);
                    if (Overlay == null)
                    {
                        Console.WriteLine("Could not find parking overlay");
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
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x01) > dlcGuardCount;
            var tagCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x05); // 0x05(flags)
            var nodeCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * tagCount)); // 0x04(nodeCount) + tags

            var triggerActionCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * nodeCount)); // 0x04(nodeCount) + nodeUids
            fileOffset += 0x04; // cursor after triggerActionCount

            for (var i = 0; i < triggerActionCount; i++)
            {
                var action = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset);
                if (action == 0x18991B7A99E279C) // hud_parking
                {
                    Overlay = Sector.Mapper.LookupOverlay(0x2358E762E112CD4);
                    if (Overlay == null)
                    {
                        Console.WriteLine("Could not find parking overlay");
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
