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
    public class TsMapPrefabItem : TsMapItem {
        private const int NodeLookBlockSize        = 0x3A;
        private const int NodeLookBlockSize825     = 0x38;
        private const int PrefabVegetaionBlockSize = 0x20;

        private readonly List< TsPrefabLook > _looks;

        public TsMapPrefabItem( ScsSector sector ) : base( sector ) {
            Valid  = true;
            _looks = new List< TsPrefabLook >();
            Nodes  = new List< ulong >();
            if ( Sector.Version < 829 )
                TsPrefabItem825();
            else if ( Sector.Version >= 829 && Sector.Version < 831 )
                TsPrefabItem829();
            else if ( Sector.Version >= 831 && Sector.Version < 846 )
                TsPrefabItem831();
            else if ( Sector.Version >= 846 && Sector.Version < 854 )
                TsPrefabItem846();
            else if ( Sector.Version == 854 )
                TsPrefabItem854();
            else if ( Sector.Version >= 855 )
                TsPrefabItem855();
            else
                Log.Warning( $"Unknown base file version ({Sector.Version}) for item {Type} in file '{Path.GetFileName( Sector.FilePath )}' @ {Sector.LastOffset}." );
        }

        public int      Origin { get; private set; }
        public TsPrefab Prefab { get; private set; }

        public void AddLook( TsPrefabLook look ) {
            _looks.Add( look );
        }

        public List< TsPrefabLook > GetLooks() => _looks;

        public bool HasLooks() => _looks != null && _looks.Count != 0;

        private void TsPrefabItem825() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8( Sector.Stream, fileOffset + 0x01 )                > dlcGuardCount
                     || ( MemoryHelper.ReadUint8( Sector.Stream, fileOffset + 0x02 ) & 0x02 ) != 0;

            ulong prefabId = MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)
            Prefab = Store().Def.LookupPrefab( prefabId );
            if ( Prefab == null ) {
                Valid = false;
                Log.Warning( $"Could not find Prefab: '{ScsHashHelper.TokenToString( prefabId )}'({MemoryHelper.ReadUInt64( Sector.Stream, fileOffset ):X}), "
                             + $"in {Path.GetFileName( Sector.FilePath )} @ {fileOffset} (item uid: 0x{Uid:X})" );
            }

            int nodeCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x18 ); // 0x18(id & look & variant)
            fileOffset += 0x04;                                                          // set cursor after nodeCount
            for ( var i = 0; i < nodeCount; i++ ) {
                Nodes.Add( MemoryHelper.ReadUInt64( Sector.Stream, fileOffset ) );
                fileOffset += 0x08;
            }

            int connectedItemCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset );
            Origin = MemoryHelper.ReadUint8( Sector.Stream,
                                             fileOffset += 0x04 + 0x08 * connectedItemCount + 0x08 ); // 0x04(connItemCount) + connItemUids + 0x08(m_some_uid)
            int prefabVegetationCount = MemoryHelper.ReadInt32( Sector.Stream,
                                                                fileOffset += 0x01
                                                                              + 0x01
                                                                              + NodeLookBlockSize825 * nodeCount ); // 0x01(origin) + 0x01(padding) + nodeLooks
            int vegetationSphereCount = MemoryHelper.ReadInt32( Sector.Stream,
                                                                fileOffset += 0x04
                                                                              + PrefabVegetaionBlockSize * prefabVegetationCount
                                                                              + 0x04 ); // 0x04(prefabVegCount) + prefabVegs + 0x04(padding2)
            fileOffset += 0x04 + VegetationSphereBlockSize825 * vegetationSphereCount;  // 0x04(vegSphereCount) + vegSpheres


            BlockSize = fileOffset - Sector.LastOffset;
        }

        private void TsPrefabItem829() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8( Sector.Stream, fileOffset + 0x01 )                > dlcGuardCount
                     || ( MemoryHelper.ReadUint8( Sector.Stream, fileOffset + 0x02 ) & 0x02 ) != 0;

            ulong prefabId = MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)
            Prefab = Store().Def.LookupPrefab( prefabId );
            if ( Prefab == null ) {
                Valid = false;
                Log.Warning( $"Could not find Prefab: '{ScsHashHelper.TokenToString( prefabId )}'({MemoryHelper.ReadUInt64( Sector.Stream, fileOffset ):X}), "
                             + $"in {Path.GetFileName( Sector.FilePath )} @ {fileOffset} (item uid: 0x{Uid:X})" );
            }

            int  additionalPartsCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x18 ); // 0x18(id & look & variant)
            byte nodeCount = MemoryHelper.ReadUint8( Sector.Stream, fileOffset += 0x04 + 0x08 * additionalPartsCount ); // 0x04(addPartsCount) + additionalParts
            fileOffset += 0x04; // set cursor after nodeCount
            for ( var i = 0; i < nodeCount; i++ ) {
                Nodes.Add( MemoryHelper.ReadUInt64( Sector.Stream, fileOffset ) );
                fileOffset += 0x08;
            }

            int connectedItemCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset );
            Origin = MemoryHelper.ReadUint8( Sector.Stream,
                                             fileOffset += 0x04 + 0x08 * connectedItemCount + 0x08 ); // 0x04(connItemCount) + connItemUids + 0x08(m_some_uid)
            int prefabVegetationCount = MemoryHelper.ReadInt32( Sector.Stream,
                                                                fileOffset += 0x01
                                                                              + 0x01
                                                                              + NodeLookBlockSize825 * nodeCount ); // 0x01(origin) + 0x01(padding) + nodeLooks
            int vegetationSphereCount = MemoryHelper.ReadInt32( Sector.Stream,
                                                                fileOffset += 0x04
                                                                              + PrefabVegetaionBlockSize * prefabVegetationCount
                                                                              + 0x04 ); // 0x04(prefabVegCount) + prefabVegs + 0x04(padding2)
            fileOffset += 0x04 + VegetationSphereBlockSize * vegetationSphereCount;     // 0x04(vegSphereCount) + vegSpheres


            BlockSize = fileOffset - Sector.LastOffset;
        }

        private void TsPrefabItem831() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8( Sector.Stream, fileOffset + 0x01 )                > dlcGuardCount
                     || ( MemoryHelper.ReadUint8( Sector.Stream, fileOffset + 0x02 ) & 0x02 ) != 0;

            ulong prefabId = MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)
            Prefab = Store().Def.LookupPrefab( prefabId );
            if ( Prefab == null ) {
                Valid = false;
                Log.Warning( $"Could not find Prefab: '{ScsHashHelper.TokenToString( prefabId )}'({MemoryHelper.ReadUInt64( Sector.Stream, fileOffset ):X}), "
                             + $"in {Path.GetFileName( Sector.FilePath )} @ {fileOffset} (item uid: 0x{Uid:X})" );
            }

            int  additionalPartsCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x18 ); // 0x18(id & look & variant)
            byte nodeCount = MemoryHelper.ReadUint8( Sector.Stream, fileOffset += 0x04 + 0x08 * additionalPartsCount ); // 0x04(addPartsCount) + additionalParts
            fileOffset += 0x04; // set cursor after nodeCount
            for ( var i = 0; i < nodeCount; i++ ) {
                Nodes.Add( MemoryHelper.ReadUInt64( Sector.Stream, fileOffset ) );
                fileOffset += 0x08;
            }

            int connectedItemCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset );
            Origin = MemoryHelper.ReadUint8( Sector.Stream,
                                             fileOffset += 0x04 + 0x08 * connectedItemCount + 0x08 ); // 0x04(connItemCount) + connItemUids + 0x08(m_some_uid)
            int prefabVegetationCount = MemoryHelper.ReadInt32( Sector.Stream,
                                                                fileOffset += 0x01
                                                                              + 0x01
                                                                              + NodeLookBlockSize825 * nodeCount ); // 0x01(origin) + 0x01(padding) + nodeLooks
            int vegetationSphereCount = MemoryHelper.ReadInt32( Sector.Stream,
                                                                fileOffset += 0x04
                                                                              + PrefabVegetaionBlockSize * prefabVegetationCount
                                                                              + 0x04 );                      // 0x04(prefabVegCount) + prefabVegs + 0x04(padding2)
            fileOffset += 0x04       + VegetationSphereBlockSize * vegetationSphereCount + 0x18 * nodeCount; // 0x04(vegSphereCount) + vegSpheres + padding
            BlockSize  =  fileOffset - Sector.LastOffset;
        }

        private void TsPrefabItem846() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8( Sector.Stream, fileOffset + 0x01 )                > dlcGuardCount
                     || ( MemoryHelper.ReadUint8( Sector.Stream, fileOffset + 0x02 ) & 0x02 ) != 0;

            ulong prefabId = MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)
            Prefab = Store().Def.LookupPrefab( prefabId );
            if ( Prefab == null ) {
                Valid = false;
                Log.Warning( $"Could not find Prefab: '{ScsHashHelper.TokenToString( prefabId )}'({MemoryHelper.ReadUInt64( Sector.Stream, fileOffset ):X}), "
                             + $"in {Path.GetFileName( Sector.FilePath )} @ {fileOffset} (item uid: 0x{Uid:X})" );
            }

            int  additionalPartsCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x18 ); // 0x18(id & look & variant)
            byte nodeCount = MemoryHelper.ReadUint8( Sector.Stream, fileOffset += 0x04 + 0x08 * additionalPartsCount ); // 0x04(addPartsCount) + additionalParts
            fileOffset += 0x04; // set cursor after nodeCount
            for ( var i = 0; i < nodeCount; i++ ) {
                Nodes.Add( MemoryHelper.ReadUInt64( Sector.Stream, fileOffset ) );
                fileOffset += 0x08;
            }

            int connectedItemCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset );
            Origin = MemoryHelper.ReadUint8( Sector.Stream,
                                             fileOffset += 0x04 + 0x08 * connectedItemCount + 0x08 ); // 0x04(connItemCount) + connItemUids + 0x08(m_some_uid)
            int prefabVegetationCount = MemoryHelper.ReadInt32( Sector.Stream,
                                                                fileOffset += 0x01
                                                                              + 0x01
                                                                              + NodeLookBlockSize * nodeCount ); // 0x01(origin) + 0x01(padding) + nodeLooks
            int vegetationSphereCount = MemoryHelper.ReadInt32( Sector.Stream,
                                                                fileOffset += 0x04
                                                                              + PrefabVegetaionBlockSize * prefabVegetationCount
                                                                              + 0x04 );                      // 0x04(prefabVegCount) + prefabVegs + 0x04(padding2)
            fileOffset += 0x04       + VegetationSphereBlockSize * vegetationSphereCount + 0x18 * nodeCount; // 0x04(vegSphereCount) + vegSpheres + padding
            BlockSize  =  fileOffset - Sector.LastOffset;
        }

        private void TsPrefabItem854() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8( Sector.Stream, fileOffset + 0x01 )                > dlcGuardCount
                     || ( MemoryHelper.ReadUint8( Sector.Stream, fileOffset + 0x02 ) & 0x02 ) != 0;

            ulong prefabId = MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)
            Prefab = Store().Def.LookupPrefab( prefabId );
            if ( Prefab == null ) {
                Valid = false;
                Log.Warning( $"Could not find Prefab: '{ScsHashHelper.TokenToString( prefabId )}'({MemoryHelper.ReadUInt64( Sector.Stream, fileOffset ):X}), "
                             + $"in {Path.GetFileName( Sector.FilePath )} @ {fileOffset} (item uid: 0x{Uid:X})" );
            }

            int additionalPartsCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x08 + 0x08 ); // 0x08(prefabId) + 0x08(m_variant)
            int nodeCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + additionalPartsCount * 0x08 ); // 0x04(addPartsCount) + additionalParts
            fileOffset += 0x04; // set cursor after nodeCount
            for ( var i = 0; i < nodeCount; i++ ) {
                Nodes.Add( MemoryHelper.ReadUInt64( Sector.Stream, fileOffset ) );
                fileOffset += 0x08;
            }

            int connectedItemCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset );
            Origin = MemoryHelper.ReadUint8( Sector.Stream,
                                             fileOffset += 0x04 + 0x08 * connectedItemCount + 0x08 ); // 0x04(connItemCount) + connItemUids + 0x08(m_some_uid)
            fileOffset += 0x02 + nodeCount * 0x0C;                                                    // 0x02(origin & padding) + nodeLooks

            BlockSize = fileOffset - Sector.LastOffset;
        }

        private void TsPrefabItem855() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8( Sector.Stream, fileOffset + 0x01 )                > dlcGuardCount
                     || ( MemoryHelper.ReadUint8( Sector.Stream, fileOffset + 0x02 ) & 0x02 ) != 0;

            ulong prefabId = MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)
            Prefab = Store().Def.LookupPrefab( prefabId );
            if ( Prefab == null ) {
                Valid = false;
                Log.Warning( $"Could not find Prefab: '{ScsHashHelper.TokenToString( prefabId )}'({MemoryHelper.ReadUInt64( Sector.Stream, fileOffset ):X}), "
                             + $"in {Path.GetFileName( Sector.FilePath )} @ {fileOffset} (item uid: 0x{Uid:X})" );
            }

            int additionalPartsCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x08 + 0x08 ); // 0x08(prefabId) + 0x08(m_variant)
            int nodeCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + additionalPartsCount * 0x08 ); // 0x04(addPartsCount) + additionalParts
            fileOffset += 0x04; // set cursor after nodeCount
            for ( var i = 0; i < nodeCount; i++ ) {
                Nodes.Add( MemoryHelper.ReadUInt64( Sector.Stream, fileOffset ) );
                fileOffset += 0x08;
            }

            int connectedItemCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset );
            Origin = MemoryHelper.ReadUint8( Sector.Stream,
                                             fileOffset += 0x04 + 0x08 * connectedItemCount + 0x08 ); // 0x04(connItemCount) + connItemUids + 0x08(m_some_uid)
            fileOffset += 0x02 + nodeCount * 0x0C + 0x08;                                             // 0x02(origin & padding) + nodeLooks + 0x08(padding2)

            BlockSize = fileOffset - Sector.LastOffset;
        }

        public void UpdateLook() {
            TsNode                originNode = Store().Map.GetNodeByUid( Nodes[ 0 ] );
            MapPalette.MapPalette palette    = Store().Settings.MapColor.ToBrushPalette();

            if ( HasLooks() ) return;

            TsPrefabNode mapPointOrigin = Prefab.PrefabNodes[ Origin ];

            var rot = (float) ( originNode.Rotation - Math.PI - Math.Atan2( mapPointOrigin.RotZ, mapPointOrigin.RotX ) + Math.PI / 2 );

            float prefabstartX = originNode.X - mapPointOrigin.X;
            float prefabStartZ = originNode.Z - mapPointOrigin.Z;

            var pointsDrawn = new List< int >();

            for ( var i = 0; i < Prefab.MapPoints.Count; i++ ) {
                TsMapPoint mapPoint = Prefab.MapPoints[ i ];
                pointsDrawn.Add( i );

                if ( mapPoint.LaneCount == -1 ) // non-road Prefab
                {
                    var polyPoints = new Dictionary< int, PointF >();
                    int nextPoint  = i;
                    do {
                        if ( Prefab.MapPoints[ nextPoint ].Neighbours.Count == 0 ) break;

                        foreach ( int neighbour in Prefab.MapPoints[ nextPoint ].Neighbours ) {
                            if ( !polyPoints.ContainsKey( neighbour ) ) // New Polygon Neighbour
                            {
                                nextPoint = neighbour;
                                PointF newPoint = ScsRenderHelper.RotatePoint(
                                                                              prefabstartX + Prefab.MapPoints[ nextPoint ].X,
                                                                              prefabStartZ + Prefab.MapPoints[ nextPoint ].Z, rot, originNode.X,
                                                                              originNode.Z );

                                polyPoints.Add( nextPoint, new PointF( newPoint.X, newPoint.Y ) );
                                break;
                            }

                            nextPoint = -1;
                        }
                    } while ( nextPoint != -1 );

                    if ( polyPoints.Count < 2 ) continue;

                    byte colorFlag = Prefab.MapPoints[ polyPoints.First().Key ].PrefabColorFlags;

                    Brush fillColor = palette.PrefabLight;
                    if ( ( colorFlag      & 0x02 ) != 0 ) fillColor = palette.PrefabLight;
                    else if ( ( colorFlag & 0x04 ) != 0 ) fillColor = palette.PrefabDark;
                    else if ( ( colorFlag & 0x08 ) != 0 ) fillColor = palette.PrefabGreen;
                    // else fillColor = palette.Error; // Unknown

                    var prefabLook = new TsPrefabPolyLook( polyPoints.Values.ToList() ) {
                        ZIndex = ( colorFlag & 0x01 ) != 0
                                     ? 3
                                     : 2,
                        Color = fillColor
                    };

                    AddLook( prefabLook );
                    continue;
                }

                int mapPointLaneCount = mapPoint.LaneCount;

                if ( mapPointLaneCount == -2 && i < Prefab.PrefabNodes.Count )
                    if ( mapPoint.ControlNodeIndex != -1 )
                        mapPointLaneCount = Prefab.PrefabNodes[ mapPoint.ControlNodeIndex ].LaneCount;

                foreach ( int neighbourPointIndex in mapPoint.Neighbours ) // TODO: Fix connection between road segments
                {
                    if ( pointsDrawn.Contains( neighbourPointIndex ) ) continue;
                    TsMapPoint neighbourPoint = Prefab.MapPoints[ neighbourPointIndex ];

                    if ( ( mapPoint.Hidden || neighbourPoint.Hidden )
                         && Prefab.PrefabNodes.Count + 1 < Prefab.MapPoints.Count ) continue;

                    double roadYaw = Math.Atan2( neighbourPoint.Z - mapPoint.Z, neighbourPoint.X - mapPoint.X );

                    int neighbourLaneCount = neighbourPoint.LaneCount;

                    if ( neighbourLaneCount == -2 && neighbourPointIndex < Prefab.PrefabNodes.Count )
                        if ( neighbourPoint.ControlNodeIndex != -1 )
                            neighbourLaneCount = Prefab.PrefabNodes[ neighbourPoint.ControlNodeIndex ].LaneCount;

                    if ( mapPointLaneCount       == -2 && neighbourLaneCount != -2 ) mapPointLaneCount  = neighbourLaneCount;
                    else if ( neighbourLaneCount == -2 && mapPointLaneCount  != -2 ) neighbourLaneCount = mapPointLaneCount;
                    else if ( mapPointLaneCount  == -2 && neighbourLaneCount == -2 ) {
                        Log.Debug( "[MapRenderer][Prefab] Could not find lane count for ({0}, {1}), defaulting to 1 for {2}", i, neighbourPointIndex,
                                   Prefab.FilePath );
                        mapPointLaneCount = neighbourLaneCount = 1;
                    }

                    var cornerCoords = new List< PointF >();

                    PointF coords = ScsRenderHelper.GetCornerCoords( prefabstartX + mapPoint.X, prefabStartZ + mapPoint.Z,
                                                                     ( Common.LaneWidth * mapPointLaneCount + mapPoint.LaneOffset ) / 2f,
                                                                     roadYaw + Math.PI / 2 );

                    cornerCoords.Add( ScsRenderHelper.RotatePoint( coords.X, coords.Y, rot, originNode.X, originNode.Z ) );

                    coords = ScsRenderHelper.GetCornerCoords( prefabstartX + neighbourPoint.X, prefabStartZ + neighbourPoint.Z,
                                                              ( Common.LaneWidth * neighbourLaneCount + neighbourPoint.LaneOffset ) / 2f,
                                                              roadYaw + Math.PI / 2 );
                    cornerCoords.Add( ScsRenderHelper.RotatePoint( coords.X, coords.Y, rot, originNode.X, originNode.Z ) );

                    coords = ScsRenderHelper.GetCornerCoords( prefabstartX + neighbourPoint.X, prefabStartZ + neighbourPoint.Z,
                                                              ( Common.LaneWidth * neighbourLaneCount + mapPoint.LaneOffset ) / 2f,
                                                              roadYaw - Math.PI / 2 );
                    cornerCoords.Add( ScsRenderHelper.RotatePoint( coords.X, coords.Y, rot, originNode.X, originNode.Z ) );

                    coords = ScsRenderHelper.GetCornerCoords( prefabstartX + mapPoint.X, prefabStartZ + mapPoint.Z,
                                                              ( Common.LaneWidth * mapPointLaneCount + mapPoint.LaneOffset ) / 2f, roadYaw - Math.PI / 2 );
                    cornerCoords.Add( ScsRenderHelper.RotatePoint( coords.X, coords.Y, rot, originNode.X, originNode.Z ) );

                    TsPrefabLook prefabLook = new TsPrefabPolyLook( cornerCoords ) {
                        Color  = palette.PrefabRoad,
                        ZIndex = 4
                    };

                    AddLook( prefabLook );
                }
            }
        }
    }
}