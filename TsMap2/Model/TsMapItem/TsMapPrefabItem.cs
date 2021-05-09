using System.Collections.Generic;
using System.IO;
using Serilog;
using TsMap;
using TsMap2.Helper;
using TsMap2.Scs;

namespace TsMap2.Model.TsMapItem {
    public class TsMapPrefabItem : TsMapItem {
        private const    int                  NodeLookBlockSize        = 0x3A;
        private const    int                  NodeLookBlockSize825     = 0x38;
        private const    int                  PrefabVegetaionBlockSize = 0x20;
        private readonly List< TsPrefabLook > _looks;

        public TsMapPrefabItem( TsSector sector, int startOffset ) : base( sector, startOffset ) {
            this.Valid  = true;
            this._looks = new List< TsPrefabLook >();
            this.Nodes  = new List< ulong >();
            if ( this.Sector.Version < 829 )
                this.TsPrefabItem825( startOffset );
            else if ( this.Sector.Version >= 829 && this.Sector.Version < 831 )
                this.TsPrefabItem829( startOffset );
            else if ( this.Sector.Version >= 831 && this.Sector.Version < 846 )
                this.TsPrefabItem831( startOffset );
            else if ( this.Sector.Version >= 846 && this.Sector.Version < 854 )
                this.TsPrefabItem846( startOffset );
            else if ( this.Sector.Version == 854 )
                this.TsPrefabItem854( startOffset );
            else if ( this.Sector.Version >= 855 )
                this.TsPrefabItem855( startOffset );
            else
                Log.Warning(
                            $"Unknown base file version ({this.Sector.Version}) for item {this.Type} in file '{Path.GetFileName( this.Sector.FilePath )}' @ {startOffset}." );
        }

        public int      Origin { get; private set; }
        public TsPrefab Prefab { get; private set; }

        public void AddLook( TsPrefabLook look ) {
            this._looks.Add( look );
        }

        public List< TsPrefabLook > GetLooks() => this._looks;

        public bool HasLooks() => this._looks != null && this._looks.Count != 0;

        public void TsPrefabItem825( int startOffset ) {
            int fileOffset = startOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            this.Hidden = MemoryHelper.ReadInt8( this.Sector.Stream, fileOffset + 0x01 )                > dlcGuardCount
                          || ( MemoryHelper.ReadUint8( this.Sector.Stream, fileOffset + 0x02 ) & 0x02 ) != 0;

            ulong prefabId = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)
            this.Prefab = Store().Def.LookupPrefab( prefabId );
            if ( this.Prefab == null ) {
                this.Valid = false;
                Log.Warning( $"Could not find Prefab: '{ScsHash.TokenToString( prefabId )}'({MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset ):X}), "
                             + $"in {Path.GetFileName( this.Sector.FilePath )} @ {fileOffset} (item uid: 0x{this.Uid:X})" );
            }

            int nodeCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x18 ); // 0x18(id & look & variant)
            fileOffset += 0x04;                                                               // set cursor after nodeCount
            for ( var i = 0; i < nodeCount; i++ ) {
                this.Nodes.Add( MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset ) );
                fileOffset += 0x08;
            }

            int connectedItemCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset );
            this.Origin = MemoryHelper.ReadUint8( this.Sector.Stream,
                                                  fileOffset += 0x04 + 0x08 * connectedItemCount + 0x08 ); // 0x04(connItemCount) + connItemUids + 0x08(m_some_uid)
            int prefabVegetationCount = MemoryHelper.ReadInt32( this.Sector.Stream,
                                                                fileOffset += 0x01
                                                                              + 0x01
                                                                              + NodeLookBlockSize825 * nodeCount ); // 0x01(origin) + 0x01(padding) + nodeLooks
            int vegetationSphereCount = MemoryHelper.ReadInt32( this.Sector.Stream,
                                                                fileOffset += 0x04
                                                                              + PrefabVegetaionBlockSize * prefabVegetationCount
                                                                              + 0x04 ); // 0x04(prefabVegCount) + prefabVegs + 0x04(padding2)
            fileOffset += 0x04 + VegetationSphereBlockSize825 * vegetationSphereCount;  // 0x04(vegSphereCount) + vegSpheres


            this.BlockSize = fileOffset - startOffset;
        }

        public void TsPrefabItem829( int startOffset ) {
            int fileOffset = startOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            this.Hidden = MemoryHelper.ReadInt8( this.Sector.Stream, fileOffset + 0x01 )                > dlcGuardCount
                          || ( MemoryHelper.ReadUint8( this.Sector.Stream, fileOffset + 0x02 ) & 0x02 ) != 0;

            ulong prefabId = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)
            this.Prefab = Store().Def.LookupPrefab( prefabId );
            if ( this.Prefab == null ) {
                this.Valid = false;
                Log.Warning( $"Could not find Prefab: '{ScsHash.TokenToString( prefabId )}'({MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset ):X}), "
                             + $"in {Path.GetFileName( this.Sector.FilePath )} @ {fileOffset} (item uid: 0x{this.Uid:X})" );
            }

            int  additionalPartsCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x18 ); // 0x18(id & look & variant)
            byte nodeCount = MemoryHelper.ReadUint8( this.Sector.Stream, fileOffset += 0x04 + 0x08 * additionalPartsCount ); // 0x04(addPartsCount) + additionalParts
            fileOffset += 0x04; // set cursor after nodeCount
            for ( var i = 0; i < nodeCount; i++ ) {
                this.Nodes.Add( MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset ) );
                fileOffset += 0x08;
            }

            int connectedItemCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset );
            this.Origin = MemoryHelper.ReadUint8( this.Sector.Stream,
                                                  fileOffset += 0x04 + 0x08 * connectedItemCount + 0x08 ); // 0x04(connItemCount) + connItemUids + 0x08(m_some_uid)
            int prefabVegetationCount = MemoryHelper.ReadInt32( this.Sector.Stream,
                                                                fileOffset += 0x01
                                                                              + 0x01
                                                                              + NodeLookBlockSize825 * nodeCount ); // 0x01(origin) + 0x01(padding) + nodeLooks
            int vegetationSphereCount = MemoryHelper.ReadInt32( this.Sector.Stream,
                                                                fileOffset += 0x04
                                                                              + PrefabVegetaionBlockSize * prefabVegetationCount
                                                                              + 0x04 ); // 0x04(prefabVegCount) + prefabVegs + 0x04(padding2)
            fileOffset += 0x04 + VegetationSphereBlockSize * vegetationSphereCount;     // 0x04(vegSphereCount) + vegSpheres


            this.BlockSize = fileOffset - startOffset;
        }

        public void TsPrefabItem831( int startOffset ) {
            int fileOffset = startOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            this.Hidden = MemoryHelper.ReadInt8( this.Sector.Stream, fileOffset + 0x01 )                > dlcGuardCount
                          || ( MemoryHelper.ReadUint8( this.Sector.Stream, fileOffset + 0x02 ) & 0x02 ) != 0;

            ulong prefabId = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)
            this.Prefab = Store().Def.LookupPrefab( prefabId );
            if ( this.Prefab == null ) {
                this.Valid = false;
                Log.Warning( $"Could not find Prefab: '{ScsHash.TokenToString( prefabId )}'({MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset ):X}), "
                             + $"in {Path.GetFileName( this.Sector.FilePath )} @ {fileOffset} (item uid: 0x{this.Uid:X})" );
            }

            int  additionalPartsCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x18 ); // 0x18(id & look & variant)
            byte nodeCount = MemoryHelper.ReadUint8( this.Sector.Stream, fileOffset += 0x04 + 0x08 * additionalPartsCount ); // 0x04(addPartsCount) + additionalParts
            fileOffset += 0x04; // set cursor after nodeCount
            for ( var i = 0; i < nodeCount; i++ ) {
                this.Nodes.Add( MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset ) );
                fileOffset += 0x08;
            }

            int connectedItemCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset );
            this.Origin = MemoryHelper.ReadUint8( this.Sector.Stream,
                                                  fileOffset += 0x04 + 0x08 * connectedItemCount + 0x08 ); // 0x04(connItemCount) + connItemUids + 0x08(m_some_uid)
            int prefabVegetationCount = MemoryHelper.ReadInt32( this.Sector.Stream,
                                                                fileOffset += 0x01
                                                                              + 0x01
                                                                              + NodeLookBlockSize825 * nodeCount ); // 0x01(origin) + 0x01(padding) + nodeLooks
            int vegetationSphereCount = MemoryHelper.ReadInt32( this.Sector.Stream,
                                                                fileOffset += 0x04
                                                                              + PrefabVegetaionBlockSize * prefabVegetationCount
                                                                              + 0x04 );                          // 0x04(prefabVegCount) + prefabVegs + 0x04(padding2)
            fileOffset     += 0x04       + VegetationSphereBlockSize * vegetationSphereCount + 0x18 * nodeCount; // 0x04(vegSphereCount) + vegSpheres + padding
            this.BlockSize =  fileOffset - startOffset;
        }

        public void TsPrefabItem846( int startOffset ) {
            int fileOffset = startOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            this.Hidden = MemoryHelper.ReadInt8( this.Sector.Stream, fileOffset + 0x01 )                > dlcGuardCount
                          || ( MemoryHelper.ReadUint8( this.Sector.Stream, fileOffset + 0x02 ) & 0x02 ) != 0;

            ulong prefabId = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)
            this.Prefab = Store().Def.LookupPrefab( prefabId );
            if ( this.Prefab == null ) {
                this.Valid = false;
                Log.Warning( $"Could not find Prefab: '{ScsHash.TokenToString( prefabId )}'({MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset ):X}), "
                             + $"in {Path.GetFileName( this.Sector.FilePath )} @ {fileOffset} (item uid: 0x{this.Uid:X})" );
            }

            int  additionalPartsCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x18 ); // 0x18(id & look & variant)
            byte nodeCount = MemoryHelper.ReadUint8( this.Sector.Stream, fileOffset += 0x04 + 0x08 * additionalPartsCount ); // 0x04(addPartsCount) + additionalParts
            fileOffset += 0x04; // set cursor after nodeCount
            for ( var i = 0; i < nodeCount; i++ ) {
                this.Nodes.Add( MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset ) );
                fileOffset += 0x08;
            }

            int connectedItemCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset );
            this.Origin = MemoryHelper.ReadUint8( this.Sector.Stream,
                                                  fileOffset += 0x04 + 0x08 * connectedItemCount + 0x08 ); // 0x04(connItemCount) + connItemUids + 0x08(m_some_uid)
            int prefabVegetationCount = MemoryHelper.ReadInt32( this.Sector.Stream,
                                                                fileOffset += 0x01
                                                                              + 0x01
                                                                              + NodeLookBlockSize * nodeCount ); // 0x01(origin) + 0x01(padding) + nodeLooks
            int vegetationSphereCount = MemoryHelper.ReadInt32( this.Sector.Stream,
                                                                fileOffset += 0x04
                                                                              + PrefabVegetaionBlockSize * prefabVegetationCount
                                                                              + 0x04 );                          // 0x04(prefabVegCount) + prefabVegs + 0x04(padding2)
            fileOffset     += 0x04       + VegetationSphereBlockSize * vegetationSphereCount + 0x18 * nodeCount; // 0x04(vegSphereCount) + vegSpheres + padding
            this.BlockSize =  fileOffset - startOffset;
        }

        public void TsPrefabItem854( int startOffset ) {
            int fileOffset = startOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            this.Hidden = MemoryHelper.ReadInt8( this.Sector.Stream, fileOffset + 0x01 )                > dlcGuardCount
                          || ( MemoryHelper.ReadUint8( this.Sector.Stream, fileOffset + 0x02 ) & 0x02 ) != 0;

            ulong prefabId = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)
            this.Prefab = Store().Def.LookupPrefab( prefabId );
            if ( this.Prefab == null ) {
                this.Valid = false;
                Log.Warning( $"Could not find Prefab: '{ScsHash.TokenToString( prefabId )}'({MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset ):X}), "
                             + $"in {Path.GetFileName( this.Sector.FilePath )} @ {fileOffset} (item uid: 0x{this.Uid:X})" );
            }

            int additionalPartsCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x08 + 0x08 ); // 0x08(prefabId) + 0x08(m_variant)
            int nodeCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + additionalPartsCount * 0x08 ); // 0x04(addPartsCount) + additionalParts
            fileOffset += 0x04; // set cursor after nodeCount
            for ( var i = 0; i < nodeCount; i++ ) {
                this.Nodes.Add( MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset ) );
                fileOffset += 0x08;
            }

            int connectedItemCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset );
            this.Origin = MemoryHelper.ReadUint8( this.Sector.Stream,
                                                  fileOffset += 0x04 + 0x08 * connectedItemCount + 0x08 ); // 0x04(connItemCount) + connItemUids + 0x08(m_some_uid)
            fileOffset += 0x02 + nodeCount * 0x0C;                                                         // 0x02(origin & padding) + nodeLooks

            this.BlockSize = fileOffset - startOffset;
        }

        public void TsPrefabItem855( int startOffset ) {
            int fileOffset = startOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            this.Hidden = MemoryHelper.ReadInt8( this.Sector.Stream, fileOffset + 0x01 )                > dlcGuardCount
                          || ( MemoryHelper.ReadUint8( this.Sector.Stream, fileOffset + 0x02 ) & 0x02 ) != 0;

            ulong prefabId = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)
            this.Prefab = Store().Def.LookupPrefab( prefabId );
            if ( this.Prefab == null ) {
                this.Valid = false;
                Log.Warning( $"Could not find Prefab: '{ScsHash.TokenToString( prefabId )}'({MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset ):X}), "
                             + $"in {Path.GetFileName( this.Sector.FilePath )} @ {fileOffset} (item uid: 0x{this.Uid:X})" );
            }

            int additionalPartsCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x08 + 0x08 ); // 0x08(prefabId) + 0x08(m_variant)
            int nodeCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + additionalPartsCount * 0x08 ); // 0x04(addPartsCount) + additionalParts
            fileOffset += 0x04; // set cursor after nodeCount
            for ( var i = 0; i < nodeCount; i++ ) {
                this.Nodes.Add( MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset ) );
                fileOffset += 0x08;
            }

            int connectedItemCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset );
            this.Origin = MemoryHelper.ReadUint8( this.Sector.Stream,
                                                  fileOffset += 0x04 + 0x08 * connectedItemCount + 0x08 ); // 0x04(connItemCount) + connItemUids + 0x08(m_some_uid)
            fileOffset += 0x02 + nodeCount * 0x0C + 0x08;                                                  // 0x02(origin & padding) + nodeLooks + 0x08(padding2)

            this.BlockSize = fileOffset - startOffset;
        }
    }
}