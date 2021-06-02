using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Serilog;
using TsMap2.Helper;
using TsMap2.Scs;
using TsMap2.Scs.FileSystem.Map;

namespace TsMap2.Model.TsMapItem {
    public class TsMapCompanyItem : TsMapItem {
        public PointF Position;

        public TsMapCompanyItem( ScsSector sector ) : base( sector ) {
            Valid = true;
            Nodes = new List< ulong >();
            if ( Sector.Version < 858 )
                TsCompanyItem825();
            else if ( Sector.Version >= 858 )
                TsCompanyItem858();
            else
                Log.Warning( $"Unknown base file version ({Sector.Version}) for item {Type} in file '{Path.GetFileName( Sector.FilePath )}' @ {Sector.LastOffset}." );
        }

        public ulong        OverlayToken { get; private set; }
        public TsMapOverlay Overlay      { get; private set; }

        private void TsCompanyItem825() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8( Sector.Stream, fileOffset + 0x01 ) > dlcGuardCount;

            OverlayToken = MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)

            Overlay = Store().Def.LookupOverlay( OverlayToken );
            if ( Overlay == null ) {
                Valid = false;
                if ( OverlayToken != 0 )
                    Log.Warning( $"Could not find Company Overlay: '{ScsHashHelper.TokenToString( OverlayToken )}'({OverlayToken:X}), in {Path.GetFileName( Sector.FilePath )} @ {fileOffset}" );
            }

            Nodes.Add( MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x08 + 0x08 ) ); // (prefab uid) | 0x08(OverlayToken) + 0x08(uid[0])

            int count = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x08 + 0x08 );           // count | 0x08 (uid[1] & uid[2])
            count      =  MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + 0x08 * count ); // count2
            count      =  MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + 0x08 * count ); // count3
            count      =  MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + 0x08 * count ); // count4
            count      =  MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + 0x08 * count ); // count5
            fileOffset += 0x04       + 0x08 * count;
            BlockSize  =  fileOffset - Sector.LastOffset;
        }

        private void TsCompanyItem858() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8( Sector.Stream, fileOffset + 0x01 ) > dlcGuardCount;

            OverlayToken = MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)

            Overlay = Store().Def.LookupOverlay( OverlayToken );
            if ( Overlay == null ) {
                Valid = false;
                if ( OverlayToken != 0 )
                    Log.Warning(
                                $"Could not find Company Overlay: '{ScsHashHelper.TokenToString( OverlayToken )}'({OverlayToken:X}), in {Path.GetFileName( Sector.FilePath )} @ {fileOffset}" );
            }

            Nodes.Add( MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x08 + 0x08 ) ); // (prefab uid) | 0x08(OverlayToken) + 0x08(uid[0])

            int count = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x08 + 0x08 );           // count | 0x08 (uid[1] & uid[2])
            count      =  MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + 0x08 * count ); // count2
            count      =  MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + 0x08 * count ); // count3
            count      =  MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + 0x08 * count ); // count4
            count      =  MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + 0x08 * count ); // count5
            count      =  MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + 0x08 * count ); // count6
            fileOffset += 0x04       + 0x08 * count;
            BlockSize  =  fileOffset - Sector.LastOffset;
        }

        public void UpdatePrefabItem() {
            var point = new PointF( X, Z );
            if ( Nodes.Count > 0 ) {
                TsMapPrefabItem prefab = Store().Map.Prefabs.FirstOrDefault( x => x.Uid == Nodes[ 0 ] );
                if ( prefab != null ) {
                    TsNode originNode = Store().Map.GetNodeByUid( prefab.Nodes[ 0 ] );

                    if ( prefab.Prefab.PrefabNodes == null ) return;

                    TsPrefabNode mapPointOrigin = prefab.Prefab.PrefabNodes[ prefab.Origin ];

                    var rot = (float) ( originNode.Rotation - Math.PI - Math.Atan2( mapPointOrigin.RotZ, mapPointOrigin.RotX ) + Math.PI / 2 );

                    float        prefabstartX = originNode.X - mapPointOrigin.X;
                    float        prefabStartZ = originNode.Z - mapPointOrigin.Z;
                    TsSpawnPoint companyPos   = prefab.Prefab.SpawnPoints.FirstOrDefault( x => x.Type == TsSpawnPointType.CompanyPos );
                    if ( companyPos != null )
                        point = ScsRenderHelper.RotatePoint( prefabstartX + companyPos.X,
                                                             prefabStartZ + companyPos.Z, rot,
                                                             originNode.X, originNode.Z );
                }
            }

            Position = point;
        }
    }
}