using System.Collections.Generic;
using System.Drawing;
using TsMap2.Scs;

namespace TsMap2.Model.TsMapItem {
    public class TsRoadItem : TsItem {
        public const int StampBlockSize = 0x18;

        private List< PointF > _points;

        public TsRoadItem( TsRoadLook roadLook,
                           ulong      startNodeUid,
                           ulong      endNodeUid,
                           ulong      uid,
                           int        blockSize,
                           bool       valid,
                           TsItemType type,
                           float      x,
                           float      z,
                           bool       hidden )
            : base( startNodeUid, endNodeUid, uid, blockSize, valid, type, x, z, hidden ) =>
            this.RoadLook = roadLook;

        public TsRoadLook RoadLook { get; }

        // --

        public void AddPoints( List< PointF > points ) {
            this._points = points;
        }

        public bool HasPoints() => this._points != null && this._points.Count != 0;

        public PointF[] GetPoints() => this._points?.ToArray();

        // --

        // public void TsRoadItem825( int startOffset ) {
        //     var fileOffset = startOffset + 0x34; // Set position at start of flags
        //     var dlcGuardCount = ( this.Sector.Mapper.IsEts2 )
        //                             ? ScsConst.Ets2DlcGuardCount
        //                             : ScsConst.AtsDlcGuardCount;
        //     this.Hidden = MemoryHelper.ReadInt8( this.Sector.Stream, fileOffset + 0x06 )                > dlcGuardCount
        //                   || ( MemoryHelper.ReadUint8( this.Sector.Stream, fileOffset + 0x03 ) & 0x02 ) != 0;
        //     this.RoadLook = this.Sector.Mapper.LookupRoadLook( MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x09 ) ); // 0x09(flags)
        //
        //     if ( this.RoadLook == null ) {
        //         this.Valid = false;
        //         Log.Msg( $"Could not find RoadLook with id: {MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset ):X}, "
        //                  + $"in {Path.GetFileName( this.Sector.FilePath )} @ {fileOffset}" );
        //     }
        //
        //     this.StartNodeUid = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x08 + 0x48 ); // 0x08(RoadLook) + 0x48(sets cursor before node_uid[])
        //     this.EndNodeUid = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x08 ); // 0x08(startNodeUid)
        //     var stampCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x08 + 0x130 ); // 0x08(endNodeUid) + 0x130(sets cursor before stampCount)
        //     var vegetationSphereCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + stampCount * StampBlockSize ); // 0x04(stampCount) + stamps
        //     fileOffset     += 0x04       + ( VegetationSphereBlockSize825 * vegetationSphereCount ); // 0x04(vegSphereCount) + vegSpheres
        //     this.BlockSize =  fileOffset - startOffset;
        // }

        // public void TsRoadItem829( int startOffset ) {
        //     var fileOffset = startOffset + 0x34; // Set position at start of flags
        //     var dlcGuardCount = ( this.Sector.Mapper.IsEts2 )
        //                             ? ScsConst.Ets2DlcGuardCount
        //                             : ScsConst.AtsDlcGuardCount;
        //     this.Hidden = MemoryHelper.ReadInt8( this.Sector.Stream, fileOffset + 0x06 )                > dlcGuardCount
        //                   || ( MemoryHelper.ReadUint8( this.Sector.Stream, fileOffset + 0x03 ) & 0x02 ) != 0;
        //     this.RoadLook = this.Sector.Mapper.LookupRoadLook( MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x09 ) ); // 0x09(flags)
        //
        //     if ( this.RoadLook == null ) {
        //         this.Valid = false;
        //         Log.Msg( $"Could not find RoadLook with id: {MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset ):X}, "
        //                  + $"in {Path.GetFileName( this.Sector.FilePath )} @ {fileOffset}" );
        //     }
        //
        //     this.StartNodeUid = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x08 + 0x48 ); // 0x08(RoadLook) + 0x48(sets cursor before node_uid[])
        //     this.EndNodeUid = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x08 ); // 0x08(startNodeUid)
        //     var stampCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x08 + 0x130 ); // 0x08(endNodeUid) + 0x130(sets cursor before stampCount)
        //     var vegetationSphereCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + stampCount * StampBlockSize ); // 0x04(stampCount) + stamps
        //     fileOffset     += 0x04       + ( VegetationSphereBlockSize * vegetationSphereCount ); // 0x04(vegSphereCount) + vegSpheres
        //     this.BlockSize =  fileOffset - startOffset;
        // }

        // public void TsRoadItem846( int startOffset ) {
        //     var fileOffset = startOffset + 0x34; // Set position at start of flags
        //     var dlcGuardCount = ( this.Sector.Mapper.IsEts2 )
        //                             ? ScsConst.Ets2DlcGuardCount
        //                             : ScsConst.AtsDlcGuardCount;
        //     this.Hidden = MemoryHelper.ReadInt8( this.Sector.Stream, fileOffset + 0x06 )                > dlcGuardCount
        //                   || ( MemoryHelper.ReadUint8( this.Sector.Stream, fileOffset + 0x03 ) & 0x02 ) != 0;
        //     this.RoadLook = this.Sector.Mapper.LookupRoadLook( MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x09 ) ); // 0x09(flags)
        //     if ( this.RoadLook == null ) {
        //         this.Valid = false;
        //         Log.Msg( $"Could not find RoadLook with id: {MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset ):X}, "
        //                  + $"in {Path.GetFileName( this.Sector.FilePath )} @ {fileOffset}" );
        //     }
        //
        //     this.StartNodeUid = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x08 + 0x50 ); // 0x08(RoadLook) + 0x50(sets cursor before node_uid[])
        //     this.EndNodeUid = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x08 ); // 0x08(startNodeUid)
        //     var stampCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x08 + 0x134 ); // 0x08(endNodeUid) + 0x134(sets cursor before stampCount)
        //     var vegetationSphereCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + stampCount * StampBlockSize ); // 0x04(stampCount) + stamps
        //     fileOffset     += 0x04       + ( VegetationSphereBlockSize * vegetationSphereCount ); // 0x04(vegSphereCount) + vegSpheres
        //     this.BlockSize =  fileOffset - startOffset;
        // }

        // public void TsRoadItem854( int startOffset ) {
        //     var fileOffset = startOffset + 0x34; // Set position at start of flags
        //     var dlcGuardCount = ( this.Sector.Mapper.IsEts2 )
        //                             ? ScsConst.Ets2DlcGuardCount
        //                             : ScsConst.AtsDlcGuardCount;
        //     this.Hidden = MemoryHelper.ReadInt8( this.Sector.Stream, fileOffset + 0x06 )                > dlcGuardCount
        //                   || ( MemoryHelper.ReadUint8( this.Sector.Stream, fileOffset + 0x03 ) & 0x02 ) != 0;
        //     var roadLookId = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x09 );
        //     this.RoadLook = this.Sector.Mapper.LookupRoadLook( roadLookId );
        //
        //     if ( this.RoadLook == null ) {
        //         this.Valid = false;
        //         Log.Msg( $"Could not find RoadLook: '{ScsHash.TokenToString( roadLookId )}'({MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset ):X}), "
        //                  + $"in {Path.GetFileName( this.Sector.FilePath )} @ {fileOffset}" );
        //     }
        //
        //     this.StartNodeUid =  MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x08 + 0xA4 ); // 0x08(RoadLook) + 0xA4(sets cursor before node_uid[])
        //     this.EndNodeUid   =  MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x08 );        // 0x08(startNodeUid)
        //     fileOffset        += 0x08 + 0x04;                                                              // 0x08(EndNodeUid) + 0x04(m_unk)
        //
        //     this.BlockSize = fileOffset - startOffset;
        // }
    }
}