using System.IO;
using Serilog;
using TsMap2.Helper;
using TsMap2.Model;
using TsMap2.Model.TsMapItem;
using TsMap2.Scs;

namespace TsMap2.Factory.TsItems {
    public class TsRoadItemFactory : TsMapItemFactory< TsRoadItem > {
        public TsRoadItemFactory( TsSector sector ) : base( sector ) { }

        private TsRoadItem TsRoadItem825( int startOffset ) {
            var valid = true;

            ulong uid = MemoryHelper.ReadUInt64( this.Sector.Stream, startOffset += 0x04 );
            float x   = MemoryHelper.ReadSingle( this.Sector.Stream, startOffset += 0x08 );
            float z   = MemoryHelper.ReadSingle( this.Sector.Stream, startOffset += 0x08 );

            int fileOffset = startOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = _store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            bool hidden = MemoryHelper.ReadInt8( this.Sector.Stream, fileOffset + 0x06 )                > dlcGuardCount
                          || ( MemoryHelper.ReadUint8( this.Sector.Stream, fileOffset + 0x03 ) & 0x02 ) != 0;
            TsRoadLook roadLook = _store().Def.LookupRoadLook( MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x09 ) ); // 0x09(flags)

            if ( roadLook == null ) {
                valid = false;
                Log.Warning( $"Could not find RoadLook with id: {MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset ):X}, "
                             + $"in {Path.GetFileName( this.Sector.FilePath )} @ {fileOffset}" );
            }

            ulong startNodeUid = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x08 + 0x48 ); // 0x08(RoadLook) + 0x48(sets cursor before node_uid[])
            ulong endNodeUid   = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x08 );        // 0x08(startNodeUid)
            int   stampCount   = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x08 + 0x130 ); // 0x08(endNodeUid) + 0x130(sets cursor before stampCount)
            int vegetationSphereCount =
                MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + stampCount * TsRoadItem.StampBlockSize ); // 0x04(stampCount) + stamps
            fileOffset += 0x04 + TsItem.VegetationSphereBlockSize825 * vegetationSphereCount;                              // 0x04(vegSphereCount) + vegSpheres
            int blockSize = fileOffset - startOffset;


            return new TsRoadItem( roadLook, startNodeUid, endNodeUid, uid, blockSize, valid, this.Sector.ItemType, x, z, hidden );
        }

        private TsRoadItem TsRoadItem829( int startOffset ) {
            var valid = true;

            ulong uid = MemoryHelper.ReadUInt64( this.Sector.Stream, startOffset += 0x04 );
            float x   = MemoryHelper.ReadSingle( this.Sector.Stream, startOffset += 0x08 );
            float z   = MemoryHelper.ReadSingle( this.Sector.Stream, startOffset += 0x08 );

            int fileOffset = startOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = _store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            bool hidden = MemoryHelper.ReadInt8( this.Sector.Stream, fileOffset + 0x06 )                > dlcGuardCount
                          || ( MemoryHelper.ReadUint8( this.Sector.Stream, fileOffset + 0x03 ) & 0x02 ) != 0;
            TsRoadLook roadLook = _store().Def.LookupRoadLook( MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x09 ) ); // 0x09(flags)

            if ( roadLook == null ) {
                valid = false;
                Log.Warning( $"Could not find RoadLook with id: {MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset ):X}, "
                             + $"in {Path.GetFileName( this.Sector.FilePath )} @ {fileOffset}" );
            }

            ulong startNodeUid = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x08 + 0x48 ); // 0x08(RoadLook) + 0x48(sets cursor before node_uid[])
            ulong endNodeUid   = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x08 );        // 0x08(startNodeUid)
            int   stampCount   = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x08 + 0x130 ); // 0x08(endNodeUid) + 0x130(sets cursor before stampCount)
            int vegetationSphereCount =
                MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + stampCount * TsRoadItem.StampBlockSize ); // 0x04(stampCount) + stamps
            fileOffset += 0x04 + TsItem.VegetationSphereBlockSize * vegetationSphereCount;                                 // 0x04(vegSphereCount) + vegSpheres
            int blockSize = fileOffset - startOffset;

            return new TsRoadItem( roadLook, startNodeUid, endNodeUid, uid, blockSize, valid, this.Sector.ItemType, x, z, hidden );
        }

        private TsRoadItem TsRoadItem846( int startOffset ) {
            var valid = true;

            ulong uid = MemoryHelper.ReadUInt64( this.Sector.Stream, startOffset += 0x04 );
            float x   = MemoryHelper.ReadSingle( this.Sector.Stream, startOffset += 0x08 );
            float z   = MemoryHelper.ReadSingle( this.Sector.Stream, startOffset += 0x08 );

            int fileOffset = startOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = _store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            bool hidden = MemoryHelper.ReadInt8( this.Sector.Stream, fileOffset + 0x06 )                > dlcGuardCount
                          || ( MemoryHelper.ReadUint8( this.Sector.Stream, fileOffset + 0x03 ) & 0x02 ) != 0;
            TsRoadLook roadLook = _store().Def.LookupRoadLook( MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x09 ) ); // 0x09(flags)
            if ( roadLook == null ) {
                valid = false;
                Log.Warning( $"Could not find RoadLook with id: {MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset ):X}, "
                             + $"in {Path.GetFileName( this.Sector.FilePath )} @ {fileOffset}" );
            }

            ulong startNodeUid = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x08 + 0x50 ); // 0x08(RoadLook) + 0x50(sets cursor before node_uid[])
            ulong endNodeUid   = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x08 );        // 0x08(startNodeUid)
            int   stampCount   = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x08 + 0x134 ); // 0x08(endNodeUid) + 0x134(sets cursor before stampCount)
            int vegetationSphereCount =
                MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + stampCount * TsRoadItem.StampBlockSize ); // 0x04(stampCount) + stamps
            fileOffset += 0x04 + TsItem.VegetationSphereBlockSize * vegetationSphereCount;                                 // 0x04(vegSphereCount) + vegSpheres
            int blockSize = fileOffset - startOffset;

            return new TsRoadItem( roadLook, startNodeUid, endNodeUid, uid, blockSize, valid, this.Sector.ItemType, x, z, hidden );
        }

        private TsRoadItem TsRoadItem854( int startOffset ) {
            var valid = true;

            int off = startOffset;

            ulong uid = MemoryHelper.ReadUInt64( this.Sector.Stream, off += 0x04 );
            float x   = MemoryHelper.ReadSingle( this.Sector.Stream, off += 0x08 );
            float z   = MemoryHelper.ReadSingle( this.Sector.Stream, off += 0x08 );

            int fileOffset = startOffset + 0x34; // Set position at start of flags

            int dlcGuardCount = _store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            bool hidden = MemoryHelper.ReadInt8( this.Sector.Stream, fileOffset + 0x06 )                > dlcGuardCount
                          || ( MemoryHelper.ReadUint8( this.Sector.Stream, fileOffset + 0x03 ) & 0x02 ) != 0;
            ulong      roadLookId = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x09 );
            TsRoadLook roadLook   = _store().Def.LookupRoadLook( roadLookId );

            if ( roadLook == null ) valid = false;
            // Log.Warning( $"Could not find RoadLook: '{ScsHash.TokenToString( roadLookId )}'({MemoryHelper.ReadUInt64( this._sector.Stream, fileOffset ):X}), "
            // + $"in {Path.GetFileName( this._sector.FilePath )} @ {fileOffset}" );

            ulong startNodeUid = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x08 + 0xA4 ); // 0x08(RoadLook) + 0xA4(sets cursor before node_uid[])
            ulong endNodeUid   = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x08 );        // 0x08(startNodeUid)
            fileOffset += 0x08 + 0x04;                                                                     // 0x08(EndNodeUid) + 0x04(m_unk)

            int blockSize = fileOffset - startOffset;

            return new TsRoadItem( roadLook, startNodeUid, endNodeUid, uid, blockSize, valid, this.Sector.ItemType, x, z, hidden );
        }

        public override TsRoadItem Retrieve( int startOffset ) {
            TsRoadItem roadItem = null;

            if ( this.Sector.Version < 829 )
                roadItem = this.TsRoadItem825( startOffset );
            else if ( this.Sector.Version >= 829 && this.Sector.Version < 846 )
                roadItem = this.TsRoadItem829( startOffset );
            else if ( this.Sector.Version >= 846 && this.Sector.Version < 854 )
                roadItem = this.TsRoadItem846( startOffset );
            else if ( this.Sector.Version >= 854 )
                roadItem = this.TsRoadItem854( startOffset );
            else
                Log.Warning( $"Unknown base file version ({this.Sector.Version}) for item {this.Sector.ItemType} in file '{Path.GetFileName( this.Sector.FilePath )}' @ {startOffset}." );

            return roadItem;
        }
    }
}