using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace TsMap.TsItem
{
    public class TsRoadItem : TsItem
    {
        private const int StampBlockSize = 0x18;
        public TsRoadLook RoadLook { get; private set; }

        private List<PointF> _points;

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
                Log.Msg(
                    $"Unknown base file version ({Sector.Version}) for item {Type} in file '{Path.GetFileName(Sector.FilePath)}' @ {startOffset}.");
        }

        public void TsRoadItem825(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x06) > dlcGuardCount || (MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x03) & 0x02) != 0;
            RoadLook = Sector.Mapper.LookupRoadLook(MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x09)); // 0x09(flags)

            if (RoadLook == null)
            {
                Valid = false;
                Log.Msg($"Could not find RoadLook with id: {MemoryHelper.ReadUInt64(Sector.Stream, fileOffset):X}, " +
                        $"in {Path.GetFileName(Sector.FilePath)} @ {fileOffset}");
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
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x06) > dlcGuardCount || (MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x03) & 0x02) != 0;
            RoadLook = Sector.Mapper.LookupRoadLook(MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x09)); // 0x09(flags)

            if (RoadLook == null)
            {
                Valid = false;
                Log.Msg($"Could not find RoadLook with id: {MemoryHelper.ReadUInt64(Sector.Stream, fileOffset):X}, " +
                        $"in {Path.GetFileName(Sector.FilePath)} @ {fileOffset}");
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
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x06) > dlcGuardCount || (MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x03) & 0x02) != 0;
            RoadLook = Sector.Mapper.LookupRoadLook(MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x09)); // 0x09(flags)
            if (RoadLook == null)
            {
                Valid = false;
                Log.Msg($"Could not find RoadLook with id: {MemoryHelper.ReadUInt64(Sector.Stream, fileOffset):X}, " +
                        $"in {Path.GetFileName(Sector.FilePath)} @ {fileOffset}");
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
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x06) > dlcGuardCount || (MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x03) & 0x02) != 0;
            RoadLook = Sector.Mapper.LookupRoadLook(MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x09));

            if (RoadLook == null)
            {
                Valid = false;
                Log.Msg($"Could not find RoadLook with id: {MemoryHelper.ReadUInt64(Sector.Stream, fileOffset):X}, " +
                        $"in {Path.GetFileName(Sector.FilePath)} @ {fileOffset}");
            }

            StartNodeUid = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08 + 0xA4); // 0x08(RoadLook) + 0xA4(sets cursor before node_uid[])
            EndNodeUid = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08); // 0x08(startNodeUid)
            fileOffset += 0x08 + 0x04; // 0x08(EndNodeUid) + 0x04(m_unk)

            BlockSize = fileOffset - startOffset;
        }
    }
}
