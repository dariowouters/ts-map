using System;
using System.Drawing;
using System.Linq;
using Serilog;
using TsMap2.Helper;
using TsMap2.Model;
using TsMap2.Model.TsMapItem;
using TsMap2.Scs;

namespace TsMap2.Job.Parse.Overlays {
    public class ParseOverlayCompanyJob : ThreadJob {
        protected override void Do() {
            Log.Information( "[Job][OverlayCompany] Parsing..." );

            foreach ( TsMapCompanyItem company in Store().Map.Companies ) {
                Bitmap b = company.Overlay?.GetBitmap();

                if ( !company.Valid || company.Hidden || b == null ) continue;

                string overlayName = ScsHashHelper.TokenToString( company.OverlayToken );
                var    point       = new PointF( company.X, company.Z );

                if ( company.Nodes.Count > 0 ) {
                    TsMapPrefabItem prefab = Store().Map.Prefabs.FirstOrDefault( x => x.Uid == company.Nodes[ 0 ] );
                    if ( prefab != null ) {
                        TsNode originNode = Store().Map.GetNodeByUid( prefab.Nodes[ 0 ] );
                        if ( prefab.Prefab.PrefabNodes == null )
                            return;

                        TsPrefabNode mapPointOrigin = prefab.Prefab.PrefabNodes[ prefab.Origin ];

                        var rot = (float) ( originNode.Rotation
                                            - Math.PI
                                            - Math.Atan2( mapPointOrigin.RotZ, mapPointOrigin.RotX )
                                            + Math.PI / 2 );

                        float prefabStartX = originNode.X - mapPointOrigin.X;
                        float prefabStartZ = originNode.Z - mapPointOrigin.Z;

                        TsSpawnPoint companyPos =
                            prefab.Prefab.SpawnPoints.FirstOrDefault( x => x.Type == TsSpawnPointType.CompanyPos );

                        if ( companyPos != null )
                            point = ScsRenderHelper.RotatePoint( prefabStartX + companyPos.X, prefabStartZ + companyPos.Z,
                                                                 rot,
                                                                 originNode.X, originNode.Z );
                    }
                }


                var ov = new TsMapOverlayItem( point.X, point.Y, overlayName, TsMapOverlayType.Company, b );
                Store().Map.Overlays.Company.Add( ov );
            }

            Log.Information( "[Job][OverlayCompany] Loaded: {0}", Store().Map.Overlays.Company.Count );
        }
    }
}