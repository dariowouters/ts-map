using System.IO;

namespace TsMap.TsItem
{
    public class TsMapOverlayItem : TsItem
    {
        public TsMapOverlay Overlay { get; private set; }
        public sbyte ZoomLevelVisibility { get; private set; }

        public TsMapOverlayItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = true;
            TsMapOverlayItem825(startOffset);
        }

        public void TsMapOverlayItem825(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            ZoomLevelVisibility = MemoryHelper.ReadInt8(Sector.Stream, fileOffset);
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x01) > dlcGuardCount || ZoomLevelVisibility == -1;

            var type = MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x02);
            var overlayId = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05);
            if (type == 1 && overlayId == 0) overlayId = 0x2358E762E112CD4; // parking
            Overlay = Sector.Mapper.LookupOverlay(overlayId);
            if (Overlay == null)
            {
                Valid = false;
                if (overlayId != 0) Log.Msg($"Could not find Overlay with id: {overlayId:X}, in {Path.GetFileName(Sector.FilePath)} @ {fileOffset}");
            }
            fileOffset += 0x08 + 0x08; // 0x08(overlayId) + 0x08(nodeUid)
            BlockSize = fileOffset - startOffset;
        }
    }
}
