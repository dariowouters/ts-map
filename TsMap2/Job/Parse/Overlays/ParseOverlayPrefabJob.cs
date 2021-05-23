using System;
using System.Drawing;
using Serilog;
using TsMap2.Helper;
using TsMap2.Model;
using TsMap2.Model.TsMapItem;
using TsMap2.Scs;

namespace TsMap2.Job.Parse.Overlays {
    public class ParseOverlayPrefabJob : ThreadJob {
        protected override void Do() {
            Log.Information( "[Job][OverlayPrefab] Parsing..." );

            foreach ( TsMapPrefabItem prefab in this.Store().Map.Prefabs ) {
                // Bitmap b          = prefab.Overlay?.GetBitmap();
                TsNode originNode = this.Store().Map.GetNodeByUid( prefab.Nodes[ 0 ] );

                if ( !prefab.Valid || prefab.Hidden || prefab.Prefab.PrefabNodes == null ) continue;

                TsPrefabNode mapPointOrigin = prefab.Prefab.PrefabNodes[ prefab.Origin ];

                var rot = (float) ( originNode.Rotation
                                    - Math.PI
                                    - Math.Atan2( mapPointOrigin.RotZ, mapPointOrigin.RotX )
                                    + Math.PI / 2 );

                float prefabStartX = originNode.X - mapPointOrigin.X;
                float prefabStartZ = originNode.Z - mapPointOrigin.Z;
                foreach ( TsSpawnPoint spawnPoint in prefab.Prefab.SpawnPoints ) {
                    this.ParseTrigger( prefab, prefabStartX, prefabStartZ, rot, spawnPoint, originNode );
                    TsMapOverlayItem ov = this.GenerateMapItem( prefabStartX, prefabStartZ, rot, spawnPoint, originNode );
                    this.AddMapItem( ov );
                    // if ( saveAsPNG && !File.Exists( Path.Combine( overlayPath, $"{overlayName}.png" ) ) )
                    // b.Save( Path.Combine( overlayPath, $"{overlayName}.png" ) );
                }
            }

            Log.Information( "[Job][OverlayPrefab] Fuel: {0}",          this.Store().Map.Overlays.Fuel.Count );
            Log.Information( "[Job][OverlayPrefab] Service: {0}",       this.Store().Map.Overlays.Service.Count );
            Log.Information( "[Job][OverlayPrefab] WeightStation: {0}", this.Store().Map.Overlays.WeightStation.Count );
            Log.Information( "[Job][OverlayPrefab] TruckDealer: {0}",   this.Store().Map.Overlays.TruckDealer.Count );
            Log.Information( "[Job][OverlayPrefab] Garage: {0}",        this.Store().Map.Overlays.Garage.Count );
            Log.Information( "[Job][OverlayPrefab] Recruitment: {0}",   this.Store().Map.Overlays.Recruitment.Count );
        }

        private TsMapOverlayItem GenerateMapItem( float prefabStartX, float prefabStartZ, float rot, TsSpawnPoint spawnPoint, TsNode originNode ) {
            PointF newPoint = ScsRenderHelper.RotatePoint( prefabStartX + spawnPoint.X,
                                                           prefabStartZ + spawnPoint.Z, rot,
                                                           originNode.X, originNode.Z );

            string           overlayName;
            TsMapOverlayType type;

            switch ( spawnPoint.Type ) {
                case TsSpawnPointType.GasPos: {
                    overlayName = "gas_ico";
                    type        = TsMapOverlayType.Fuel;
                    break;
                }
                case TsSpawnPointType.ServicePos: {
                    overlayName = "service_ico";
                    type        = TsMapOverlayType.Service;
                    break;
                }
                case TsSpawnPointType.WeightStationPos: {
                    overlayName = "weigh_station_ico";
                    type        = TsMapOverlayType.WeightStation;
                    break;
                }
                case TsSpawnPointType.TruckDealerPos: {
                    overlayName = "dealer_ico";
                    type        = TsMapOverlayType.TruckDealer;
                    break;
                }
                case TsSpawnPointType.BuyPos: {
                    overlayName = "garage_large_ico";
                    type        = TsMapOverlayType.Garage;
                    break;
                }
                case TsSpawnPointType.RecruitmentPos: {
                    overlayName = "recruitment_ico";
                    type        = TsMapOverlayType.Recruitment;
                    break;
                }
                default:
                    return null;
            }

            TsMapOverlay overlay = this.Store().Def.LookupOverlay( ScsHash.StringToToken( overlayName ) );
            Bitmap       b       = overlay.GetBitmap();

            return b == null
                       ? null
                       : new TsMapOverlayItem( newPoint.X, newPoint.Y, overlayName, type, b );
        }

        private void ParseTrigger( TsMapPrefabItem prefab, float prefabStartX, float prefabStartZ, float rot, TsSpawnPoint spawnPoint, TsNode originNode ) {
            int lastId = -1;
            foreach ( TsTriggerPoint triggerPoint in prefab.Prefab.TriggerPoints ) {
                TsMapOverlay overlay = this.Store().Def.LookupOverlay( ScsHash.StringToToken( "parking_ico" ) );
                Bitmap       b       = overlay.GetBitmap();

                if ( triggerPoint.TriggerId             == lastId
                     || b                               == null
                     || triggerPoint.TriggerActionToken != ScsHash.StringToToken( "hud_parking" ) ) continue;


                PointF newPoint = ScsRenderHelper.RotatePoint( prefabStartX + triggerPoint.X,
                                                               prefabStartZ + triggerPoint.Z, rot,
                                                               originNode.X, originNode.Z );

                lastId = (int) triggerPoint.TriggerId;
                var ov = new TsMapOverlayItem( newPoint.X, newPoint.Y, "parking_ico", TsMapOverlayType.Parking, b );
                this.Store().Map.Overlays.Parking.Add( ov );

                // if ( saveAsPNG && !File.Exists( Path.Combine( overlayPath, $"{overlayName}.png" ) ) )
                //     b.Save( Path.Combine( overlayPath, $"{overlayName}.png" ) );
            }
        }

        private void AddMapItem( TsMapOverlayItem item ) {
            switch ( item.OverlayType ) {
                case TsMapOverlayType.Fuel: {
                    this.Store().Map.Overlays.Fuel.Add( item );
                    break;
                }
                case TsMapOverlayType.Service: {
                    this.Store().Map.Overlays.Service.Add( item );
                    break;
                }
                case TsMapOverlayType.WeightStation: {
                    this.Store().Map.Overlays.WeightStation.Add( item );
                    break;
                }
                case TsMapOverlayType.TruckDealer: {
                    this.Store().Map.Overlays.TruckDealer.Add( item );
                    break;
                }
                case TsMapOverlayType.Garage: {
                    this.Store().Map.Overlays.Garage.Add( item );
                    break;
                }
                case TsMapOverlayType.Recruitment: {
                    this.Store().Map.Overlays.Recruitment.Add( item );
                    break;
                }
            }
        }
    }
}