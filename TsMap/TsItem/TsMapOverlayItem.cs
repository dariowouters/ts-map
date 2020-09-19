using System.IO;
using TsMap.HashFiles;

namespace TsMap.TsItem
{
    public class TsMapOverlayItem : TsItem
    {
        public string OverlayName { get; private set; }
        public TsMapOverlay Overlay { get; private set; }
        public byte ZoomLevelVisibility { get; private set; }

        public TsMapOverlayItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = true;
            TsMapOverlayItem825(startOffset);
        }

        public void TsMapOverlayItem825(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            ZoomLevelVisibility = MemoryHelper.ReadUint8(Sector.Stream, fileOffset);
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x01) > dlcGuardCount || ZoomLevelVisibility == 255;

            var type = MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x02);
            var overlayToken = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05);
            if (type == 1 && overlayToken == 0) overlayToken = ScsHash.StringToToken("parking_ico"); // parking
            Overlay = Sector.Mapper.LookupOverlay(overlayToken);
            OverlayName = ScsHash.TokenToString(overlayToken);
            if (Overlay == null)
            {
                Valid = false;
                if (overlayToken != 0) Log.Msg($"Could not find Overlay: '{OverlayName}'({ScsHash.StringToToken(OverlayName):X}), in {Path.GetFileName(Sector.FilePath)} @ {fileOffset}");
            }
            fileOffset += 0x08 + 0x08; // 0x08(overlayId) + 0x08(nodeUid)
            BlockSize = fileOffset - startOffset;
        }
    }
}
