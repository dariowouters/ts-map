using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using TsMap2.Model;
using TsMap2.Model.MapPalette;
using TsMap2.Model.TsMapItem;
using TsMap2.Scs;

namespace TsMap2.Helper.Map {
    public class MapRenderer {
        private const float itemDrawMargin = 1000f;

        private readonly Bitmap     _b;
        private readonly SolidBrush _cityShadowColor = new SolidBrush( Color.FromArgb( 210, 0, 0, 0 ) );

        private readonly Font        _defaultFont = new Font( "Arial", 10.0f, FontStyle.Bold );
        private readonly MapPalette  _palette;
        private readonly RenderFlags _renderFlags;

        private readonly int[]     _zoomCaps = { 1000, 5000, 18500, 45000 };
        private          Rectangle _clip;

        public MapRenderer( Bitmap b ) {
            _b           = b;
            _renderFlags = _store.Settings.RenderFlags & ~RenderFlags.TextOverlay;
            _palette     = _store.Settings.MapColor.ToBrushPalette();
            _clip        = new Rectangle( 0, 0, b.Width, b.Height );
        }

        private StoreHelper _store => StoreHelper.Instance;

        public void Render( float scale, PointF startPoint ) {
            long startTime = DateTime.Now.Ticks;

            Graphics g = Graphics.FromImage( _b );

            g.ResetTransform();
            g.FillRectangle( _palette.Background, new Rectangle( 0, 0, _clip.Width, _clip.Height ) );

            g.ScaleTransform( scale, scale );
            g.TranslateTransform( -startPoint.X, -startPoint.Y );
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode   = PixelOffsetMode.None;
            g.SmoothingMode     = SmoothingMode.AntiAlias;

            // if (_mapper == null)
            // {
            //     _g.DrawString("Map object not initialized", _defaultFont, palette.Error, 5, 5);
            //     return;
            // }

            var endPoint = new PointF( startPoint.X + _clip.Width / scale, startPoint.Y + _clip.Height / scale );

            long ferryTime = FerryConnectionsRender( g );

            long mapAreaTime = MapAreasRender( g, startPoint, endPoint );

            long prefabTime = PrefabRender( g, startPoint, endPoint );

            long roadTime = RoadRender( g, startPoint, endPoint );

            long mapOverlayTime = MapOverlaysRender( g, startPoint, endPoint );

            long mapOverlay2Time = MapOverlays2Render( g, startPoint, endPoint );

            long cityTime = CityRender( g, scale );

            g.ResetTransform();
            // g.Dispose();
            // long elapsedTime = DateTime.Now.Ticks - startTime;
            // if ( renderFlags.IsActive( RenderFlags.TextOverlay ) )
            // g.DrawString(
            // $"DrawTime: {elapsedTime / TimeSpan.TicksPerMillisecond} ms, x: {startPoint.X}, y: {startPoint.Y}, scale: {scale}",
            // _defaultFont, Brushes.WhiteSmoke, 5, 5 );

            // g.FillRectangle(new SolidBrush(Color.FromArgb(100, 0, 0, 0)), 5, 20, 150, 150);
            // Log.Debug( ">> Roads: {0}ms",       roadTime        / TimeSpan.TicksPerMillisecond );
            // Log.Debug( ">> Prefab: {0}ms",      prefabTime      / TimeSpan.TicksPerMillisecond );
            // Log.Debug( ">> Ferry: {0}ms",       ferryTime       / TimeSpan.TicksPerMillisecond );
            // Log.Debug( ">> MapOverlay: {0}ms",  mapOverlayTime  / TimeSpan.TicksPerMillisecond );
            // Log.Debug( ">> MapOverlay2: {0}ms", mapOverlay2Time / TimeSpan.TicksPerMillisecond );
            // Log.Debug( ">> MapArea: {0}ms",     mapAreaTime     / TimeSpan.TicksPerMillisecond );
            // Log.Debug( ">> City: {0}ms",        cityTime        / TimeSpan.TicksPerMillisecond );
            // g.DrawString($"Road: {roadTime / TimeSpan.TicksPerMillisecond}ms", _defaultFont, Brushes.White, 10, 40);
            // g.DrawString($"Prefab: {prefabTime / TimeSpan.TicksPerMillisecond}ms", _defaultFont, Brushes.White, 10, 55);
            // g.DrawString($"Ferry: {ferryTime / TimeSpan.TicksPerMillisecond}ms", _defaultFont, Brushes.White, 10, 70);
            // g.DrawString($"MapOverlay: {mapOverlayTime / TimeSpan.TicksPerMillisecond}ms", _defaultFont, Brushes.White, 10, 85);
            // g.DrawString($"MapOverlay2: {mapOverlay2Time / TimeSpan.TicksPerMillisecond}ms", _defaultFont, Brushes.White, 10, 100);
            // g.DrawString($"MapArea: {mapAreaTime / TimeSpan.TicksPerMillisecond}ms", _defaultFont, Brushes.White, 10, 115);
            // g.DrawString($"City: {cityTime / TimeSpan.TicksPerMillisecond}ms", _defaultFont, Brushes.White, 10, 130);

            g.Dispose();
        }

        private long FerryConnectionsRender( Graphics g ) {
            long ferryStartTime = DateTime.Now.Ticks;

            if ( !_renderFlags.IsActive( RenderFlags.FerryConnections ) ) return DateTime.Now.Ticks - ferryStartTime;

            IEnumerable< TsMapFerryItem > ferryConnections = _store.Map.FerryConnections.Where( item => !item.Hidden );

            var ferryPen = new Pen( _palette.FerryLines, 50 ) { DashPattern = new[] { 10f, 10f } };

            foreach ( TsMapFerryItem ferryConnection in ferryConnections ) {
                List< TsFerryConnection > connections = _store.Def.LookupFerryConnection( ferryConnection.FerryPortId );

                foreach ( TsFerryConnection conn in connections ) {
                    if ( conn.Connections.Count == 0 ) // no extra nodes -> straight line
                    {
                        g.DrawLine( ferryPen, conn.StartPortLocation, conn.EndPortLocation );
                        continue;
                    }

                    double startYaw = Math.Atan2( conn.Connections[ 0 ].Z - conn.StartPortLocation.Y, // get angle of the start port to the first node
                                                  conn.Connections[ 0 ].X - conn.StartPortLocation.X );
                    Tuple< PointF, PointF > bezierNodes = ScsRenderHelper.GetBezierControlNodes( conn.StartPortLocation.X,
                                                                                                 conn.StartPortLocation.Y, startYaw, conn.Connections[ 0 ].X,
                                                                                                 conn.Connections[ 0 ].Z,
                                                                                                 conn.Connections[ 0 ].Rotation );

                    var bezierPoints = new List< PointF > {
                        new PointF( conn.StartPortLocation.X,                       conn.StartPortLocation.Y ),                       // start
                        new PointF( conn.StartPortLocation.X + bezierNodes.Item1.X, conn.StartPortLocation.Y + bezierNodes.Item1.Y ), // control1
                        new PointF( conn.Connections[ 0 ].X  - bezierNodes.Item2.X, conn.Connections[ 0 ].Z  - bezierNodes.Item2.Y ), // control2
                        new PointF( conn.Connections[ 0 ].X,                        conn.Connections[ 0 ].Z )
                    };

                    for ( var i = 0; i < conn.Connections.Count - 1; i++ ) // loop all extra nodes
                    {
                        TsFerryPoint ferryPoint     = conn.Connections[ i ];
                        TsFerryPoint nextFerryPoint = conn.Connections[ i + 1 ];

                        bezierNodes = ScsRenderHelper.GetBezierControlNodes( ferryPoint.X, ferryPoint.Z, ferryPoint.Rotation,
                                                                             nextFerryPoint.X, nextFerryPoint.Z, nextFerryPoint.Rotation );

                        bezierPoints.Add( new PointF( ferryPoint.X     + bezierNodes.Item1.X, ferryPoint.Z     + bezierNodes.Item1.Y ) ); // control1
                        bezierPoints.Add( new PointF( nextFerryPoint.X - bezierNodes.Item2.X, nextFerryPoint.Z - bezierNodes.Item2.Y ) ); // control2
                        bezierPoints.Add( new PointF( nextFerryPoint.X,                       nextFerryPoint.Z ) );                       // end
                    }

                    TsFerryPoint lastFerryPoint = conn.Connections[ conn.Connections.Count - 1 ];
                    double endYaw = Math.Atan2( conn.EndPortLocation.Y - lastFerryPoint.Z, // get angle of the last node to the end port
                                                conn.EndPortLocation.X - lastFerryPoint.X );

                    bezierNodes = ScsRenderHelper.GetBezierControlNodes( lastFerryPoint.X,
                                                                         lastFerryPoint.Z, lastFerryPoint.Rotation, conn.EndPortLocation.X, conn.EndPortLocation.Y,
                                                                         endYaw );

                    bezierPoints.Add( new PointF( lastFerryPoint.X       + bezierNodes.Item1.X, lastFerryPoint.Z       + bezierNodes.Item1.Y ) ); // control1
                    bezierPoints.Add( new PointF( conn.EndPortLocation.X - bezierNodes.Item2.X, conn.EndPortLocation.Y - bezierNodes.Item2.Y ) ); // control2
                    bezierPoints.Add( new PointF( conn.EndPortLocation.X,                       conn.EndPortLocation.Y ) );                       // end

                    g.DrawBeziers( ferryPen, bezierPoints.ToArray() );
                }
            }

            ferryPen.Dispose();

            return DateTime.Now.Ticks - ferryStartTime;
        }

        private long MapAreasRender( Graphics g, PointF startPoint, PointF endPoint ) {
            long mapAreaStartTime = DateTime.Now.Ticks;
            if ( !_renderFlags.IsActive( RenderFlags.MapAreas ) ) return DateTime.Now.Ticks - mapAreaStartTime;

            IEnumerable< TsMapAreaItem > mapAreas = _store.Map.MapAreas.Where( item =>
                                                                                   item.X    >= startPoint.X - itemDrawMargin
                                                                                   && item.X <= endPoint.X   + itemDrawMargin
                                                                                   && item.Z >= startPoint.Y - itemDrawMargin
                                                                                   && item.Z <= endPoint.Y   + itemDrawMargin
                                                                                   && !item.Hidden )
                                                          .ToList();


            foreach ( TsMapAreaItem mapArea in mapAreas.OrderBy( x => x.DrawOver ) ) {
                var points = new List< PointF >();

                foreach ( ulong mapAreaNode in mapArea.NodeUids ) {
                    TsNode node = _store.Map.GetNodeByUid( mapAreaNode );
                    if ( node == null ) continue;
                    points.Add( new PointF( node.X, node.Z ) );
                }

                Brush fillColor = _palette.PrefabLight;
                if ( ( mapArea.ColorIndex      & 0x01 ) != 0 ) fillColor = _palette.PrefabLight;
                else if ( ( mapArea.ColorIndex & 0x02 ) != 0 ) fillColor = _palette.PrefabDark;
                else if ( ( mapArea.ColorIndex & 0x03 ) != 0 ) fillColor = _palette.PrefabGreen;

                g.FillPolygon( fillColor, points.ToArray() );
            }

            return DateTime.Now.Ticks - mapAreaStartTime;
        }

        private long PrefabRender( Graphics g, PointF startPoint, PointF endPoint ) {
            long prefabStartTime = DateTime.Now.Ticks;

            IEnumerable< TsMapPrefabItem > prefabs = _store.Map.Prefabs.Where( item =>
                                                                                   item.X    >= startPoint.X - itemDrawMargin
                                                                                   && item.X <= endPoint.X   + itemDrawMargin
                                                                                   && item.Z >= startPoint.Y - itemDrawMargin
                                                                                   && item.Z <= endPoint.Y   + itemDrawMargin
                                                                                   && !item.Hidden )
                                                           .ToList();

            if ( !_renderFlags.IsActive( RenderFlags.Prefabs ) ) return DateTime.Now.Ticks - prefabStartTime;

            var drawingQueue = new List< TsPrefabLook >();

            foreach ( TsMapPrefabItem prefabItem in prefabs ) {
                // TsNode originNode = _store.Map.GetNodeByUid( prefabItem.Nodes[ 0 ] );
                if ( prefabItem.Prefab.PrefabNodes == null ) continue;

                /*if ( !prefabItem.HasLooks() ) {
                    TsPrefabNode mapPointOrigin = prefabItem.Prefab.PrefabNodes[ prefabItem.Origin ];

                    var rot = (float) ( originNode.Rotation - Math.PI - Math.Atan2( mapPointOrigin.RotZ, mapPointOrigin.RotX ) + Math.PI / 2 );

                    float prefabstartX = originNode.X - mapPointOrigin.X;
                    float prefabStartZ = originNode.Z - mapPointOrigin.Z;

                    var pointsDrawn = new List< int >();

                    for ( var i = 0; i < prefabItem.Prefab.MapPoints.Count; i++ ) {
                        TsMapPoint mapPoint = prefabItem.Prefab.MapPoints[ i ];
                        pointsDrawn.Add( i );

                        if ( mapPoint.LaneCount == -1 ) // non-road Prefab
                        {
                            var polyPoints = new Dictionary< int, PointF >();
                            int nextPoint  = i;
                            do {
                                if ( prefabItem.Prefab.MapPoints[ nextPoint ].Neighbours.Count == 0 ) break;

                                foreach ( int neighbour in prefabItem.Prefab.MapPoints[ nextPoint ].Neighbours ) {
                                    if ( !polyPoints.ContainsKey( neighbour ) ) // New Polygon Neighbour
                                    {
                                        nextPoint = neighbour;
                                        PointF newPoint = ScsRenderHelper.RotatePoint(
                                                                                      prefabstartX + prefabItem.Prefab.MapPoints[ nextPoint ].X,
                                                                                      prefabStartZ + prefabItem.Prefab.MapPoints[ nextPoint ].Z, rot, originNode.X,
                                                                                      originNode.Z );

                                        polyPoints.Add( nextPoint, new PointF( newPoint.X, newPoint.Y ) );
                                        break;
                                    }

                                    nextPoint = -1;
                                }
                            } while ( nextPoint != -1 );

                            if ( polyPoints.Count < 2 ) continue;

                            byte colorFlag = prefabItem.Prefab.MapPoints[ polyPoints.First().Key ].PrefabColorFlags;

                            Brush fillColor = _palette.PrefabLight;
                            if ( ( colorFlag      & 0x02 ) != 0 ) fillColor = _palette.PrefabLight;
                            else if ( ( colorFlag & 0x04 ) != 0 ) fillColor = _palette.PrefabDark;
                            else if ( ( colorFlag & 0x08 ) != 0 ) fillColor = _palette.PrefabGreen;
                            // else fillColor = _palette.Error; // Unknown

                            var prefabLook = new TsPrefabPolyLook( polyPoints.Values.ToList() ) {
                                ZIndex = ( colorFlag & 0x01 ) != 0
                                             ? 3
                                             : 2,
                                Color = fillColor
                            };

                            prefabItem.AddLook( prefabLook );
                            continue;
                        }

                        int mapPointLaneCount = mapPoint.LaneCount;

                        if ( mapPointLaneCount == -2 && i < prefabItem.Prefab.PrefabNodes.Count )
                            if ( mapPoint.ControlNodeIndex != -1 )
                                mapPointLaneCount = prefabItem.Prefab.PrefabNodes[ mapPoint.ControlNodeIndex ].LaneCount;

                        foreach ( int neighbourPointIndex in mapPoint.Neighbours ) // TODO: Fix connection between road segments
                        {
                            if ( pointsDrawn.Contains( neighbourPointIndex ) ) continue;
                            TsMapPoint neighbourPoint = prefabItem.Prefab.MapPoints[ neighbourPointIndex ];

                            if ( ( mapPoint.Hidden || neighbourPoint.Hidden )
                                 && prefabItem.Prefab.PrefabNodes.Count + 1 < prefabItem.Prefab.MapPoints.Count ) continue;

                            double roadYaw = Math.Atan2( neighbourPoint.Z - mapPoint.Z, neighbourPoint.X - mapPoint.X );

                            int neighbourLaneCount = neighbourPoint.LaneCount;

                            if ( neighbourLaneCount == -2 && neighbourPointIndex < prefabItem.Prefab.PrefabNodes.Count )
                                if ( neighbourPoint.ControlNodeIndex != -1 )
                                    neighbourLaneCount = prefabItem.Prefab.PrefabNodes[ neighbourPoint.ControlNodeIndex ].LaneCount;

                            if ( mapPointLaneCount       == -2 && neighbourLaneCount != -2 ) mapPointLaneCount  = neighbourLaneCount;
                            else if ( neighbourLaneCount == -2 && mapPointLaneCount  != -2 ) neighbourLaneCount = mapPointLaneCount;
                            else if ( mapPointLaneCount  == -2 && neighbourLaneCount == -2 ) {
                                Log.Debug( "[MapRenderer][Prefab] Could not find lane count for ({0}, {1}), defaulting to 1 for {2}", i, neighbourPointIndex,
                                           prefabItem.Prefab.FilePath );
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
                                Color  = _palette.PrefabRoad,
                                ZIndex = 4
                            };

                            prefabItem.AddLook( prefabLook );
                        }
                    }
                }*/

                drawingQueue.AddRange( prefabItem.GetLooks() );
                // prefabItem.GetLooks().ForEach( x => drawingQueue.Add( x ) );
            }

            // foreach ( TsPrefabLook prefabLook in drawingQueue.OrderBy( p => p.ZIndex ) ) prefabLook.Draw( g );
            foreach ( TsPrefabLook prefabLook in drawingQueue.OrderBy( p => p.ZIndex ) )
                g.FillPolygon( prefabLook.Color, prefabLook.Points.ToArray() );

            return DateTime.Now.Ticks - prefabStartTime;
        }

        private long MapOverlaysRender( Graphics g, PointF startPoint, PointF endPoint ) {
            long mapOverlayStartTime = DateTime.Now.Ticks;
            if ( !_renderFlags.IsActive( RenderFlags.MapOverlays ) ) return DateTime.Now.Ticks - mapOverlayStartTime;

            IEnumerable< TsMapMapOverlayItem > overlays = _store.Map.MapOverlays.Where( item =>
                                                                                            item.X    >= startPoint.X - itemDrawMargin
                                                                                            && item.X <= endPoint.X   + itemDrawMargin
                                                                                            && item.Z >= startPoint.Y - itemDrawMargin
                                                                                            && item.Z <= endPoint.Y   + itemDrawMargin
                                                                                            && !item.Hidden )
                                                                .ToList();

            foreach ( TsMapMapOverlayItem overlayItem in overlays ) // TODO: Scaling
            {
                Bitmap b = overlayItem.Overlay.GetBitmap();
                if ( b != null )
                    g.DrawImage( b, overlayItem.X - b.Width, overlayItem.Z - b.Height, b.Width * 2, b.Height * 2 );
            }

            return DateTime.Now.Ticks - mapOverlayStartTime;
        }

        private long MapOverlays2Render( Graphics g, PointF startPoint, PointF endPoint ) {
            long mapOverlay2StartTime = DateTime.Now.Ticks;
            IEnumerable< TsMapPrefabItem > prefabs = _store.Map.Prefabs.Where( item =>
                                                                                   item.X    >= startPoint.X - itemDrawMargin
                                                                                   && item.X <= endPoint.X   + itemDrawMargin
                                                                                   && item.Z >= startPoint.Y - itemDrawMargin
                                                                                   && item.Z <= endPoint.Y   + itemDrawMargin
                                                                                   && !item.Hidden )
                                                           .ToList();

            if ( _renderFlags.IsActive( RenderFlags.MapOverlays ) ) {
                IEnumerable< TsMapCompanyItem > companies = _store.Map.Companies.Where( item =>
                                                                                            item.X    >= startPoint.X - itemDrawMargin
                                                                                            && item.X <= endPoint.X   + itemDrawMargin
                                                                                            && item.Z >= startPoint.Y - itemDrawMargin
                                                                                            && item.Z <= endPoint.Y   + itemDrawMargin
                                                                                            && !item.Hidden )
                                                                  .ToList();

                foreach ( TsMapCompanyItem companyItem in companies ) // TODO: Scaling
                {
                    // var point = new PointF( companyItem.X, companyItem.Z );
                    // if ( companyItem.Nodes.Count > 0 ) {
                    //     TsMapPrefabItem prefab = _store.Map.Prefabs.FirstOrDefault( x => x.Uid == companyItem.Nodes[ 0 ] );
                    //     if ( prefab != null ) {
                    //         TsNode originNode = _store.Map.GetNodeByUid( prefab.Nodes[ 0 ] );
                    //         if ( prefab.Prefab.PrefabNodes == null ) continue;
                    //         TsPrefabNode mapPointOrigin = prefab.Prefab.PrefabNodes[ prefab.Origin ];
                    //
                    //         var rot = (float) ( originNode.Rotation - Math.PI - Math.Atan2( mapPointOrigin.RotZ, mapPointOrigin.RotX ) + Math.PI / 2 );
                    //
                    //         float        prefabstartX = originNode.X - mapPointOrigin.X;
                    //         float        prefabStartZ = originNode.Z - mapPointOrigin.Z;
                    //         TsSpawnPoint companyPos   = prefab.Prefab.SpawnPoints.FirstOrDefault( x => x.Type == TsSpawnPointType.CompanyPos );
                    //         if ( companyPos != null )
                    //             point = ScsRenderHelper.RotatePoint( prefabstartX + companyPos.X,
                    //                                                  prefabStartZ + companyPos.Z, rot,
                    //                                                  originNode.X, originNode.Z );
                    //     }
                    // }

                    // Log.Debug( "cName: {0}", companyItem.Overlay.Token );

                    Bitmap b = companyItem.Overlay?.GetBitmap();
                    if ( b != null )
                        // lock ( b )
                        g.DrawImage( b, companyItem.Position.X, companyItem.Position.Y, b.Width, b.Height );
                }

                foreach ( TsMapPrefabItem prefab in prefabs ) // Draw all prefab overlays
                {
                    TsNode originNode = _store.Map.GetNodeByUid( prefab.Nodes[ 0 ] );
                    if ( prefab.Prefab.PrefabNodes == null ) continue;
                    TsPrefabNode mapPointOrigin = prefab.Prefab.PrefabNodes[ prefab.Origin ];

                    var rot = (float)( originNode.Rotation - Math.PI - Math.Atan2( mapPointOrigin.RotZ, mapPointOrigin.RotX ) + Math.PI / 2 );

                    float prefabstartX = originNode.X - mapPointOrigin.X;
                    float prefabStartZ = originNode.Z - mapPointOrigin.Z;
                    foreach ( TsSpawnPoint spawnPoint in prefab.Prefab.SpawnPoints ) {
                        PointF newPoint = ScsRenderHelper.RotatePoint( prefabstartX + spawnPoint.X, prefabStartZ + spawnPoint.Z, rot,
                                                                       originNode.X, originNode.Z );

                        Bitmap b = null;

                        switch ( spawnPoint.Type ) {
                            case TsSpawnPointType.GasPos: {
                                TsMapOverlay overlay = _store.Def.LookupOverlay( ScsHashHelper.StringToToken( "gas_ico" ) );
                                b = overlay?.GetBitmap();
                                break;
                            }
                            case TsSpawnPointType.ServicePos: {
                                TsMapOverlay overlay = _store.Def.LookupOverlay( ScsHashHelper.StringToToken( "service_ico" ) );
                                b = overlay?.GetBitmap();
                                break;
                            }
                            case TsSpawnPointType.WeightStationPos: {
                                TsMapOverlay overlay = _store.Def.LookupOverlay( ScsHashHelper.StringToToken( "weigh_station_ico" ) );
                                b = overlay?.GetBitmap();
                                break;
                            }
                            case TsSpawnPointType.TruckDealerPos: {
                                TsMapOverlay overlay = _store.Def.LookupOverlay( ScsHashHelper.StringToToken( "dealer_ico" ) );
                                b = overlay?.GetBitmap();
                                break;
                            }
                            case TsSpawnPointType.BuyPos: {
                                TsMapOverlay overlay = _store.Def.LookupOverlay( ScsHashHelper.StringToToken( "garage_large_ico" ) );
                                b = overlay?.GetBitmap();
                                break;
                            }
                            case TsSpawnPointType.RecruitmentPos: {
                                TsMapOverlay overlay = _store.Def.LookupOverlay( ScsHashHelper.StringToToken( "recruitment_ico" ) );
                                b = overlay?.GetBitmap();
                                break;
                            }
                        }

                        if ( b != null )
                            // lock ( b )
                            g.DrawImage( b, newPoint.X - b.Width / 2f, newPoint.Y - b.Height / 2f, b.Width, b.Height );
                    }

                    int lastId = -1;
                    foreach ( TsTriggerPoint triggerPoint in prefab.Prefab.TriggerPoints ) // trigger points in prefabs: garage, hotel, ...
                    {
                        PointF newPoint = ScsRenderHelper.RotatePoint( prefabstartX + triggerPoint.X, prefabStartZ + triggerPoint.Z, rot,
                                                                       originNode.X, originNode.Z );

                        if ( triggerPoint.TriggerId == lastId ) continue;
                        lastId = (int)triggerPoint.TriggerId;

                        if ( triggerPoint.TriggerActionToken == ScsHashHelper.StringToToken( "hud_parking" ) ) // parking trigger
                        {
                            TsMapOverlay overlay = _store.Def.LookupOverlay( ScsHashHelper.StringToToken( "parking_ico" ) );
                            Bitmap       b       = overlay?.GetBitmap();

                            if ( b != null )
                                // lock ( b )
                                g.DrawImage( b, newPoint.X - b.Width / 2f, newPoint.Y - b.Height / 2f, b.Width, b.Height );
                        }
                    }
                }

                List< TsMapTriggerItem > triggers = _store.Map.Triggers.Where( item =>
                                                                                   item.X    >= startPoint.X - itemDrawMargin
                                                                                   && item.X <= endPoint.X   + itemDrawMargin
                                                                                   && item.Z >= startPoint.Y - itemDrawMargin
                                                                                   && item.Z <= endPoint.Y   + itemDrawMargin
                                                                                   && !item.Hidden )
                                                          .ToList();

                foreach ( TsMapTriggerItem triggerItem in triggers ) // TODO: Scaling
                {
                    Bitmap b = triggerItem.Overlay?.GetBitmap();
                    if ( b != null )
                        // lock ( b )
                        g.DrawImage( b, triggerItem.X, triggerItem.Z, b.Width, b.Height );
                }

                List< TsMapFerryItem > ferryItems = _store.Map.FerryConnections.Where( item =>
                                                                                           item.X    >= startPoint.X - itemDrawMargin
                                                                                           && item.X <= endPoint.X   + itemDrawMargin
                                                                                           && item.Z >= startPoint.Y - itemDrawMargin
                                                                                           && item.Z <= endPoint.Y   + itemDrawMargin )
                                                          .ToList();

                foreach ( TsMapFerryItem ferryItem in ferryItems ) // TODO: Scaling
                {
                    Bitmap b = ferryItem.Overlay?.GetBitmap();
                    if ( b != null )
                        // lock ( b )
                        g.DrawImage( b, ferryItem.X, ferryItem.Z, b.Width, b.Height );
                }
            }

            return DateTime.Now.Ticks - mapOverlay2StartTime;
        }

        private long CityRender( Graphics g, float scale ) {
            long cityStartTime = DateTime.Now.Ticks;
            int  zoomIndex     = ScsRenderHelper.GetZoomIndex( _clip, scale );

            if ( !_renderFlags.IsActive( RenderFlags.CityNames ) ) return DateTime.Now.Ticks - cityStartTime;

            // IEnumerable< KeyValuePair< ulong, TsMapCityItem > > cities = _store.Map.Cities.Where( item => !item.Value.Hidden );

            var cityFont = new Font( "Arial", 100 + _zoomCaps[ zoomIndex ] / 100, FontStyle.Bold );

            foreach ( KeyValuePair< ulong, TsCity > kv in _store.Def.Cities ) {
                TsCity city = kv.Value;
                string name = city.GetLocalizedName( _store.Settings.SelectedLocalization );

                // TsNode node = _store.Map.GetNodeByUid( item.NodeUid );
                // PointF coords = node == null
                // ? new PointF( item.X, item.Z )
                // : new PointF( node.X, node.Z );
                ( float x, float y ) = ( city.X, city.Y );
                if ( city.XOffsets.Count > zoomIndex && city.YOffsets.Count > zoomIndex ) {
                    x += city.XOffsets[ zoomIndex ] / ( scale * _zoomCaps[ zoomIndex ] );
                    y += city.YOffsets[ zoomIndex ] / ( scale * _zoomCaps[ zoomIndex ] );
                }

                // SizeF textSize = g.MeasureString( name, cityFont );
                g.DrawString( name, cityFont, _cityShadowColor,  x + 2, y + 2 );
                g.DrawString( name, cityFont, _palette.CityName, x,     y );
            }

            cityFont.Dispose();

            return DateTime.Now.Ticks - cityStartTime;
        }

        private long RoadRender( Graphics g, PointF startPoint, PointF endPoint ) {
            long roadStartTime = DateTime.Now.Ticks;
            if ( !_renderFlags.IsActive( RenderFlags.Roads ) ) return DateTime.Now.Ticks - roadStartTime;

            IEnumerable< TsMapRoadItem > roads = _store.Map.Roads.Where( item =>
                                                                             item.X    >= startPoint.X - itemDrawMargin
                                                                             && item.X <= endPoint.X   + itemDrawMargin
                                                                             && item.Z >= startPoint.Y - itemDrawMargin
                                                                             && item.Z <= endPoint.Y   + itemDrawMargin
                                                                        /*&& !item.Hidden*/ )
                                                       .ToList();

            foreach ( TsMapRoadItem road in roads ) {
                // TsNode startNode = road.GetStartNode();
                // TsNode endNode   = road.GetEndNode();

                /*if ( !road.HasPoints() ) {
                    var newPoints = new List< PointF >();

                    float sx = startNode.X;
                    float sz = startNode.Z;
                    float ex = endNode.X;
                    float ez = endNode.Z;

                    double radius = Math.Sqrt( Math.Pow( sx - ex, 2 ) + Math.Pow( sz - ez, 2 ) );

                    double tanSx = Math.Cos( -( Math.PI * 0.5f - startNode.Rotation ) ) * radius;
                    double tanEx = Math.Cos( -( Math.PI * 0.5f - endNode.Rotation ) )   * radius;
                    double tanSz = Math.Sin( -( Math.PI * 0.5f - startNode.Rotation ) ) * radius;
                    double tanEz = Math.Sin( -( Math.PI * 0.5f - endNode.Rotation ) )   * radius;

                    for ( var i = 0; i < 8; i++ ) {
                        float s = i / (float)( 8 - 1 );
                        var   x = (float)TsRoadLook.Hermite( s, sx, ex, tanSx, tanEx );
                        var   z = (float)TsRoadLook.Hermite( s, sz, ez, tanSz, tanEz );
                        newPoints.Add( new PointF( x, z ) );
                    }

                    road.AddPoints( newPoints );
                }*/

                // if ( road.GetPoints().Length <= 0 ) continue;

                float roadWidth = road.RoadLook.GetWidth();
                var   roadPen   = new Pen( _palette.Road, roadWidth );
                g.DrawCurve( roadPen, road.GetPoints().ToArray() );
                roadPen.Dispose();
            }

            return DateTime.Now.Ticks - roadStartTime;
        }
    }
}