using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Serilog;
using TsMap2.Helper;
using TsMap2.Scs;
using TsMap2.Scs.FileSystem.Map;

namespace TsMap2.Model.TsMapItem {
    public class TsMapRoadItem : TsMapItem {
        private const int StampBlockSize = 0x18;

        private List< PointF > _points;

        public TsMapRoadItem( ScsSector sector ) : base( sector ) {
            Valid = true;
            if ( sector.Version < 829 )
                TsRoadItem825();
            else if ( sector.Version >= 829 && sector.Version < 846 )
                TsRoadItem829();
            else if ( sector.Version >= 846 && sector.Version < 854 )
                TsRoadItem846();
            else if ( sector.Version >= 854 )
                TsRoadItem854();
            else
                Log.Warning( $"Unknown base file version ({Sector.Version}) for item {Type} in file '{Path.GetFileName( Sector.FilePath )}' @ {sector.LastOffset}." );
        }

        public TsRoadLook RoadLook { get; private set; }

        public void AddPoints( List< PointF > points ) {
            _points = points;
        }

        public bool HasPoints() => _points != null && _points.Count != 0;

        public PointF[] GetPoints() => _points?.ToArray();

        private void TsRoadItem825() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8( Sector.Stream, fileOffset + 0x06 )                > dlcGuardCount
                     || ( MemoryHelper.ReadUint8( Sector.Stream, fileOffset + 0x03 ) & 0x02 ) != 0;
            RoadLook = Store().Def.LookupRoadLook( MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x09 ) ); // 0x09(flags)

            if ( RoadLook == null ) {
                Valid = false;
                Log.Warning( $"Could not find RoadLook with id: {MemoryHelper.ReadUInt64( Sector.Stream, fileOffset ):X}, "
                             + $"in {Path.GetFileName( Sector.FilePath )} @ {fileOffset}" );
            }

            StartNodeUid = MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x08 + 0x48 ); // 0x08(RoadLook) + 0x48(sets cursor before node_uid[])
            EndNodeUid   = MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x08 ); // 0x08(startNodeUid)
            int stampCount            = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x08 + 0x130 ); // 0x08(endNodeUid) + 0x130(sets cursor before stampCount)
            int vegetationSphereCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + stampCount * StampBlockSize ); // 0x04(stampCount) + stamps
            fileOffset += 0x04       + VegetationSphereBlockSize825 * vegetationSphereCount; // 0x04(vegSphereCount) + vegSpheres
            BlockSize  =  fileOffset - Sector.LastOffset;
        }

        private void TsRoadItem829() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8( Sector.Stream, fileOffset + 0x06 )                > dlcGuardCount
                     || ( MemoryHelper.ReadUint8( Sector.Stream, fileOffset + 0x03 ) & 0x02 ) != 0;
            RoadLook = Store().Def.LookupRoadLook( MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x09 ) ); // 0x09(flags)

            if ( RoadLook == null ) {
                Valid = false;
                Log.Warning( $"Could not find RoadLook with id: {MemoryHelper.ReadUInt64( Sector.Stream, fileOffset ):X}, "
                             + $"in {Path.GetFileName( Sector.FilePath )} @ {fileOffset}" );
            }

            StartNodeUid = MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x08 + 0x48 ); // 0x08(RoadLook) + 0x48(sets cursor before node_uid[])
            EndNodeUid   = MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x08 ); // 0x08(startNodeUid)
            int stampCount            = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x08 + 0x130 ); // 0x08(endNodeUid) + 0x130(sets cursor before stampCount)
            int vegetationSphereCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + stampCount * StampBlockSize ); // 0x04(stampCount) + stamps
            fileOffset += 0x04       + VegetationSphereBlockSize * vegetationSphereCount; // 0x04(vegSphereCount) + vegSpheres
            BlockSize  =  fileOffset - Sector.LastOffset;
        }

        private void TsRoadItem846() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8( Sector.Stream, fileOffset + 0x06 )                > dlcGuardCount
                     || ( MemoryHelper.ReadUint8( Sector.Stream, fileOffset + 0x03 ) & 0x02 ) != 0;
            RoadLook = Store().Def.LookupRoadLook( MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x09 ) ); // 0x09(flags)
            if ( RoadLook == null ) {
                Valid = false;
                Log.Warning( $"Could not find RoadLook with id: {MemoryHelper.ReadUInt64( Sector.Stream, fileOffset ):X}, "
                             + $"in {Path.GetFileName( Sector.FilePath )} @ {fileOffset}" );
            }

            StartNodeUid = MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x08 + 0x50 ); // 0x08(RoadLook) + 0x50(sets cursor before node_uid[])
            EndNodeUid   = MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x08 ); // 0x08(startNodeUid)
            int stampCount            = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x08 + 0x134 ); // 0x08(endNodeUid) + 0x134(sets cursor before stampCount)
            int vegetationSphereCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + stampCount * StampBlockSize ); // 0x04(stampCount) + stamps
            fileOffset += 0x04       + VegetationSphereBlockSize * vegetationSphereCount; // 0x04(vegSphereCount) + vegSpheres
            BlockSize  =  fileOffset - Sector.LastOffset;
        }

        private void TsRoadItem854() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8( Sector.Stream, fileOffset + 0x06 )                > dlcGuardCount
                     || ( MemoryHelper.ReadUint8( Sector.Stream, fileOffset + 0x03 ) & 0x02 ) != 0;
            ulong roadLookId = MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x09 );
            RoadLook = Store().Def.LookupRoadLook( roadLookId );

            if ( RoadLook == null ) {
                Valid = false;
                Log.Warning( $"Could not find RoadLook: '{ScsHashHelper.TokenToString( roadLookId )}'({MemoryHelper.ReadUInt64( Sector.Stream, fileOffset ):X}), "
                             + $"in {Path.GetFileName( Sector.FilePath )} @ {fileOffset}" );
            }

            StartNodeUid =  MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x08 + 0xA4 ); // 0x08(RoadLook) + 0xA4(sets cursor before node_uid[])
            EndNodeUid   =  MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x08 );        // 0x08(startNodeUid)
            fileOffset   += 0x08 + 0x04;                                                         // 0x08(EndNodeUid) + 0x04(m_unk)

            BlockSize = fileOffset - Sector.LastOffset;
        }
    }
}