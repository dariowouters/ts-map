﻿using System.Collections.Generic;
using System.Drawing;
using System.IO;
using TsMap.Common;
using TsMap.Helpers;
using TsMap.Helpers.Logger;

namespace TsMap.TsItem
{
    public class TsRoadItem : TsItem
    {
        private const int StampBlockSize = 0x18;
        public TsRoadLook RoadLook { get; private set; }

        private List<PointF> _points;

        public bool IsSecret { get; private set; }

        public void AddPoints(List<PointF> points)
        {
            _points = points;
        }

        public bool HasPoints()
        {
            return _points != null && _points.Count != 0;
        }

        public PointF[] GetPoints()
        {
            return _points?.ToArray();
        }

        public TsRoadItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = true;
            if (sector.Version < 829)
                TsRoadItem825(startOffset);
            else if (sector.Version >= 829 && sector.Version < 846)
                TsRoadItem829(startOffset);
            else if (sector.Version >= 846 && sector.Version < 854)
                TsRoadItem846(startOffset);
            else if (sector.Version >= 854)
                TsRoadItem854(startOffset);
            else
                Logger.Instance.Error($"Unknown base file version ({Sector.Version}) for item {Type} " +
                    $"in file '{Path.GetFileName(Sector.FilePath)}' @ {startOffset} from '{Sector.GetUberFile().Entry.GetArchiveFile().GetPath()}'");
        }

        public void TsRoadItem825(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Consts.Ets2DlcGuardCount : Consts.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x06) > dlcGuardCount || (MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x03) & 0x02) != 0;
            var roadLookId = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x09); // 0x09(flags)
            RoadLook = Sector.Mapper.LookupRoadLook(roadLookId);

            if (RoadLook == null)
            {
                Valid = false;
                Logger.Instance.Error($"Could not find RoadLook: '{ScsToken.TokenToString(roadLookId)}'({roadLookId:X}), item uid: 0x{Uid:X}, " +
                        $"in {Path.GetFileName(Sector.FilePath)} @ {fileOffset} from '{Sector.GetUberFile().Entry.GetArchiveFile().GetPath()}'");
            }
            StartNodeUid = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08 + 0x48); // 0x08(RoadLook) + 0x48(sets cursor before node_uid[])
            EndNodeUid = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08); // 0x08(startNodeUid)
            var stampCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x08 + 0x130); // 0x08(endNodeUid) + 0x130(sets cursor before stampCount)
            var vegetationSphereCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + stampCount * StampBlockSize); // 0x04(stampCount) + stamps
            fileOffset += 0x04 + (VegetationSphereBlockSize825 * vegetationSphereCount); // 0x04(vegSphereCount) + vegSpheres
            BlockSize = fileOffset - startOffset;
        }
        public void TsRoadItem829(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Consts.Ets2DlcGuardCount : Consts.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x06) > dlcGuardCount || (MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x03) & 0x02) != 0;
            var roadLookId = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x09); // 0x09(flags)
            RoadLook = Sector.Mapper.LookupRoadLook(roadLookId);

            if (RoadLook == null)
            {
                Valid = false;
                Logger.Instance.Error($"Could not find RoadLook: '{ScsToken.TokenToString(roadLookId)}'({roadLookId:X}), item uid: 0x{Uid:X}, " +
                        $"in {Path.GetFileName(Sector.FilePath)} @ {fileOffset} from '{Sector.GetUberFile().Entry.GetArchiveFile().GetPath()}'");
            }
            StartNodeUid = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08 + 0x48); // 0x08(RoadLook) + 0x48(sets cursor before node_uid[])
            EndNodeUid = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08); // 0x08(startNodeUid)
            var stampCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x08 + 0x130); // 0x08(endNodeUid) + 0x130(sets cursor before stampCount)
            var vegetationSphereCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + stampCount * StampBlockSize); // 0x04(stampCount) + stamps
            fileOffset += 0x04 + (VegetationSphereBlockSize * vegetationSphereCount); // 0x04(vegSphereCount) + vegSpheres
            BlockSize = fileOffset - startOffset;
        }
        public void TsRoadItem846(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Consts.Ets2DlcGuardCount : Consts.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x06) > dlcGuardCount || (MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x03) & 0x02) != 0;
            var roadLookId = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x09);
            RoadLook = Sector.Mapper.LookupRoadLook(roadLookId); // 0x09(flags)
            if (RoadLook == null)
            {
                Valid = false;
                Logger.Instance.Error($"Could not find RoadLook: '{ScsToken.TokenToString(roadLookId)}'({roadLookId:X}), item uid: 0x{Uid:X}, " +
                        $"in {Path.GetFileName(Sector.FilePath)} @ {fileOffset} from '{Sector.GetUberFile().Entry.GetArchiveFile().GetPath()}'");
            }
            StartNodeUid = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08 + 0x50); // 0x08(RoadLook) + 0x50(sets cursor before node_uid[])
            EndNodeUid = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08); // 0x08(startNodeUid)
            var stampCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x08 + 0x134); // 0x08(endNodeUid) + 0x134(sets cursor before stampCount)
            var vegetationSphereCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + stampCount * StampBlockSize); // 0x04(stampCount) + stamps
            fileOffset += 0x04 + (VegetationSphereBlockSize * vegetationSphereCount); // 0x04(vegSphereCount) + vegSpheres
            BlockSize = fileOffset - startOffset;
        }
        public void TsRoadItem854(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Consts.Ets2DlcGuardCount : Consts.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x06) > dlcGuardCount || (MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x03) & 0x02) != 0;
            IsSecret = MemoryHelper.IsBitSet(MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 2), 0);
            var roadLookId = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x09); // 0x09(flags)
            RoadLook = Sector.Mapper.LookupRoadLook(roadLookId);

            if (RoadLook == null)
            {
                Valid = false;
                Logger.Instance.Error($"Could not find RoadLook: '{ScsToken.TokenToString(roadLookId)}'({roadLookId:X}), item uid: 0x{Uid:X}, " +
                        $"in {Path.GetFileName(Sector.FilePath)} @ {fileOffset} from '{Sector.GetUberFile().Entry.GetArchiveFile().GetPath()}'");
            }

            StartNodeUid = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08 + 0xA4); // 0x08(RoadLook) + 0xA4(sets cursor before node_uid[])
            EndNodeUid = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08); // 0x08(startNodeUid)
            fileOffset += 0x08 + 0x04; // 0x08(EndNodeUid) + 0x04(m_unk)

            BlockSize = fileOffset - startOffset;
        }
    }
}
