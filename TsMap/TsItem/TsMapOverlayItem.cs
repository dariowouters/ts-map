using System.IO;
using TsMap.Common;
using TsMap.Helpers;
using TsMap.Helpers.Logger;

namespace TsMap.TsItem
{
    public class TsMapOverlayItem : TsItem
    {
        public string OverlayName { get; private set; }
        public TsMapOverlay Overlay { get; private set; }
        public byte ZoomLevelVisibility { get; private set; }

        public bool IsSecret { get; private set; }

        public TsMapOverlayItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = true;
            TsMapOverlayItem825(startOffset);
        }

        public void TsMapOverlayItem825(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            ZoomLevelVisibility = MemoryHelper.ReadUint8(Sector.Stream, fileOffset);
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Consts.Ets2DlcGuardCount : Consts.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x01) > dlcGuardCount || ZoomLevelVisibility == 255;
            IsSecret = MemoryHelper.IsBitSet(MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x02), 3);

            var type = MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x02);
            var overlayToken = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05);
            if (type == 1 && overlayToken == 0)
            {
                overlayToken = ScsToken.StringToToken("parking_ico"); // parking
                Overlay = Sector.Mapper.LookupOverlay("parking_ico", OverlayTypes.Map);
            } else
            {
                Overlay = Sector.Mapper.LookupOverlay(ScsToken.TokenToString(overlayToken), OverlayTypes.Road);
            }

            OverlayName = ScsToken.TokenToString(overlayToken);

            if (Overlay == null)
            {
                Valid = false;
                if (overlayToken != 0) Logger.Instance.Error($"Could not find Overlay: '{OverlayName}'({ScsToken.StringToToken(OverlayName):X}), item uid: 0x{Uid:X}, " +
                    $"in {Path.GetFileName(Sector.FilePath)} @ {fileOffset} from '{Sector.GetUberFile().Entry.GetArchiveFile().GetPath()}'");
            }
            fileOffset += 0x08 + 0x08; // 0x08(overlayId) + 0x08(nodeUid)
            BlockSize = fileOffset - startOffset;
        }
    }
}
