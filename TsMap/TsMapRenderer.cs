using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using TsMap.Common;
using TsMap.Helpers.Logger;
using TsMap.Map.Overlays;

namespace TsMap
{
    public class TsMapRenderer
    {
        private readonly TsMapper _mapper;
        private const float itemDrawMargin = 1000f;

        private int[] zoomCaps = { 1000, 5000, 18500, 45000 };

        private readonly Font _defaultFont = new Font("Arial", 10.0f, FontStyle.Bold);
        private readonly SolidBrush _cityShadowColor = new SolidBrush(Color.FromArgb(210, 0, 0, 0));

        public TsMapRenderer(TsMapper mapper)
        {
            _mapper = mapper;
        }

        public void Render(Graphics g, Rectangle clip, float scale, PointF startPoint, MapPalette palette, RenderFlags renderFlags = RenderFlags.All)
        {
            var startTime = DateTime.Now.Ticks;
            g.ResetTransform();
            g.FillRectangle(palette.Background, new Rectangle(0, 0, clip.Width, clip.Height));

            g.ScaleTransform(scale, scale);
            g.TranslateTransform(-startPoint.X, -startPoint.Y);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.None;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (_mapper == null)
            {
                g.DrawString("Map object not initialized", _defaultFont, palette.Error, 5, 5);
                return;
            }

            var dlcGuards = _mapper.GetDlcGuardsForCurrentGame();

            var activeDlcGuards = dlcGuards.Where(x => x.Enabled).Select(x => x.Index).ToList();

            var zoomIndex = RenderHelper.GetZoomIndex(clip, scale);

            var endPoint = new PointF(startPoint.X + clip.Width / scale, startPoint.Y + clip.Height / scale);

            var ferryStartTime = DateTime.Now.Ticks;
            if (renderFlags.IsActive(RenderFlags.FerryConnections))
            {
                var ferryPen = new Pen(palette.FerryLines, 50) {DashPattern = new[] {10f, 10f}};

                foreach (var ferryConnection in _mapper.FerryConnections)
                {
                    var connections = _mapper.LookupFerryConnection(ferryConnection.FerryPortId);

                    foreach (var conn in connections)
                    {
                        if (conn.Connections.Count == 0) // no extra nodes -> straight line
                        {
                            g.DrawLine(ferryPen, conn.StartPortLocation, conn.EndPortLocation);
                            continue;
                        }

                        var startYaw = Math.Atan2(conn.Connections[0].Z - conn.StartPortLocation.Y, // get angle of the start port to the first node
                            conn.Connections[0].X - conn.StartPortLocation.X);
                        var bezierNodes = RenderHelper.GetBezierControlNodes(conn.StartPortLocation.X,
                            conn.StartPortLocation.Y, startYaw, conn.Connections[0].X, conn.Connections[0].Z,
                            conn.Connections[0].Rotation);

                        var bezierPoints = new List<PointF>
                        {
                            new PointF(conn.StartPortLocation.X, conn.StartPortLocation.Y), // start
                            new PointF(conn.StartPortLocation.X + bezierNodes.Item1.X, conn.StartPortLocation.Y + bezierNodes.Item1.Y), // control1
                            new PointF(conn.Connections[0].X - bezierNodes.Item2.X, conn.Connections[0].Z - bezierNodes.Item2.Y), // control2
                            new PointF(conn.Connections[0].X, conn.Connections[0].Z)
                        };

                        for (var i = 0; i < conn.Connections.Count - 1; i++) // loop all extra nodes
                        {
                            var ferryPoint = conn.Connections[i];
                            var nextFerryPoint = conn.Connections[i + 1];

                            bezierNodes = RenderHelper.GetBezierControlNodes(ferryPoint.X, ferryPoint.Z, ferryPoint.Rotation,
                                nextFerryPoint.X, nextFerryPoint.Z, nextFerryPoint.Rotation);

                            bezierPoints.Add(new PointF(ferryPoint.X + bezierNodes.Item1.X, ferryPoint.Z + bezierNodes.Item1.Y)); // control1
                            bezierPoints.Add(new PointF(nextFerryPoint.X - bezierNodes.Item2.X, nextFerryPoint.Z - bezierNodes.Item2.Y)); // control2
                            bezierPoints.Add(new PointF(nextFerryPoint.X, nextFerryPoint.Z)); // end
                        }

                        var lastFerryPoint = conn.Connections[conn.Connections.Count - 1];
                        var endYaw = Math.Atan2(conn.EndPortLocation.Y - lastFerryPoint.Z, // get angle of the last node to the end port
                            conn.EndPortLocation.X - lastFerryPoint.X);

                        bezierNodes = RenderHelper.GetBezierControlNodes(lastFerryPoint.X,
                            lastFerryPoint.Z, lastFerryPoint.Rotation, conn.EndPortLocation.X, conn.EndPortLocation.Y,
                            endYaw);

                        bezierPoints.Add(new PointF(lastFerryPoint.X + bezierNodes.Item1.X, lastFerryPoint.Z + bezierNodes.Item1.Y)); // control1
                        bezierPoints.Add(new PointF(conn.EndPortLocation.X - bezierNodes.Item2.X, conn.EndPortLocation.Y - bezierNodes.Item2.Y)); // control2
                        bezierPoints.Add(new PointF(conn.EndPortLocation.X, conn.EndPortLocation.Y)); // end

                        g.DrawBeziers(ferryPen, bezierPoints.ToArray());
                    }
                }
                ferryPen.Dispose();
            }
            var ferryTime = DateTime.Now.Ticks - ferryStartTime;

            var mapAreaStartTime = DateTime.Now.Ticks;
            if (renderFlags.IsActive(RenderFlags.MapAreas))
            {
                foreach (var mapArea in _mapper.MapAreas.OrderBy(x => x.DrawOver))
                {
                    if (!activeDlcGuards.Contains(mapArea.DlcGuard) ||
                        mapArea.IsSecret && !renderFlags.IsActive(RenderFlags.SecretRoads) ||
                        mapArea.X < startPoint.X - itemDrawMargin || mapArea.X > endPoint.X + itemDrawMargin ||
                        mapArea.Z < startPoint.Y - itemDrawMargin || mapArea.Z > endPoint.Y + itemDrawMargin)
                    {
                        continue;
                    }

                    var points = new List<PointF>();

                    foreach (var mapAreaNode in mapArea.NodeUids)
                    {
                        var node = _mapper.GetNodeByUid(mapAreaNode);
                        if (node == null) continue;
                        points.Add(new PointF(node.X, node.Z));
                    }

                    Brush fillColor = palette.PrefabLight;
                    if ((mapArea.ColorIndex & 0x01) != 0) fillColor = palette.PrefabLight;
                    else if ((mapArea.ColorIndex & 0x02) != 0) fillColor = palette.PrefabDark;
                    else if ((mapArea.ColorIndex & 0x03) != 0) fillColor = palette.PrefabGreen;

                    g.FillPolygon(fillColor, points.ToArray());
                }
            }
            var mapAreaTime = DateTime.Now.Ticks - mapAreaStartTime;

            var prefabStartTime = DateTime.Now.Ticks;
            if (renderFlags.IsActive(RenderFlags.Prefabs))
            {
                List<TsPrefabLook> drawingQueue = new List<TsPrefabLook>();

                foreach (var prefabItem in _mapper.Prefabs)
                {
                    if (!activeDlcGuards.Contains(prefabItem.DlcGuard) ||
                        prefabItem.IsSecret && !renderFlags.IsActive(RenderFlags.SecretRoads) ||
                        prefabItem.X < startPoint.X - itemDrawMargin || prefabItem.X > endPoint.X + itemDrawMargin ||
                        prefabItem.Z < startPoint.Y - itemDrawMargin || prefabItem.Z > endPoint.Y + itemDrawMargin)
                    {
                        continue;
                    }

                    var originNode = _mapper.GetNodeByUid(prefabItem.Nodes[0]);
                    if (prefabItem.Prefab.PrefabNodes == null) continue;

                    if (!prefabItem.HasLooks())
                    {
                        var mapPointOrigin = prefabItem.Prefab.PrefabNodes[prefabItem.Origin];

                        var rot = (float)(originNode.Rotation - Math.PI -
                                           Math.Atan2(mapPointOrigin.RotZ, mapPointOrigin.RotX) + Math.PI / 2);

                        var prefabstartX = originNode.X - mapPointOrigin.X;
                        var prefabStartZ = originNode.Z - mapPointOrigin.Z;

                        List<int> pointsDrawn = new List<int>();

                        for (var i = 0; i < prefabItem.Prefab.MapPoints.Count; i++)
                        {
                            var mapPoint = prefabItem.Prefab.MapPoints[i];
                            pointsDrawn.Add(i);

                            if (mapPoint.LaneCount == -1) // non-road Prefab
                            {
                                Dictionary<int, PointF> polyPoints = new Dictionary<int, PointF>();
                                var nextPoint = i;
                                do
                                {
                                    if (prefabItem.Prefab.MapPoints[nextPoint].Neighbours.Count == 0) break;

                                    foreach (var neighbour in prefabItem.Prefab.MapPoints[nextPoint].Neighbours)
                                    {
                                        if (!polyPoints.ContainsKey(neighbour)) // New Polygon Neighbour
                                        {
                                            nextPoint = neighbour;
                                            var newPoint = RenderHelper.RotatePoint(
                                                prefabstartX + prefabItem.Prefab.MapPoints[nextPoint].X,
                                                prefabStartZ + prefabItem.Prefab.MapPoints[nextPoint].Z, rot, originNode.X,
                                                originNode.Z);

                                            polyPoints.Add(nextPoint, new PointF(newPoint.X, newPoint.Y));
                                            break;
                                        }
                                        nextPoint = -1;
                                    }
                                } while (nextPoint != -1);

                                if (polyPoints.Count < 2) continue;

                                var colorFlag = prefabItem.Prefab.MapPoints[polyPoints.First().Key].PrefabColorFlags;

                                Brush fillColor = palette.PrefabLight;
                                if ((colorFlag & 0x02) != 0) fillColor = palette.PrefabLight;
                                else if ((colorFlag & 0x04) != 0) fillColor = palette.PrefabDark;
                                else if ((colorFlag & 0x08) != 0) fillColor = palette.PrefabGreen;
                                // else fillColor = _palette.Error; // Unknown

                                var prefabLook = new TsPrefabPolyLook(polyPoints.Values.ToList())
                                {
                                    ZIndex = ((colorFlag & 0x01) != 0) ? 3 : 2,
                                    Color = fillColor
                                };

                                prefabItem.AddLook(prefabLook);
                                continue;
                            }

                            var mapPointLaneCount = mapPoint.LaneCount;

                            if (mapPointLaneCount == -2 && i < prefabItem.Prefab.PrefabNodes.Count)
                            {
                                if (mapPoint.ControlNodeIndex != -1) mapPointLaneCount = prefabItem.Prefab.PrefabNodes[mapPoint.ControlNodeIndex].LaneCount;
                            }

                            foreach (var neighbourPointIndex in mapPoint.Neighbours) // TODO: Fix connection between road segments
                            {
                                if (pointsDrawn.Contains(neighbourPointIndex)) continue;
                                var neighbourPoint = prefabItem.Prefab.MapPoints[neighbourPointIndex];

                                if ((mapPoint.Hidden || neighbourPoint.Hidden) && prefabItem.Prefab.PrefabNodes.Count + 1 <
                                    prefabItem.Prefab.MapPoints.Count) continue;

                                var roadYaw = Math.Atan2(neighbourPoint.Z - mapPoint.Z, neighbourPoint.X - mapPoint.X);

                                var neighbourLaneCount = neighbourPoint.LaneCount;

                                if (neighbourLaneCount == -2 && neighbourPointIndex < prefabItem.Prefab.PrefabNodes.Count)
                                {
                                    if (neighbourPoint.ControlNodeIndex != -1) neighbourLaneCount = prefabItem.Prefab.PrefabNodes[neighbourPoint.ControlNodeIndex].LaneCount;
                                }

                                if (mapPointLaneCount == -2 && neighbourLaneCount != -2) mapPointLaneCount = neighbourLaneCount;
                                else if (neighbourLaneCount == -2 && mapPointLaneCount != -2) neighbourLaneCount = mapPointLaneCount;
                                else if (mapPointLaneCount == -2 && neighbourLaneCount == -2)
                                {
                                    Logger.Instance.Debug($"Could not find lane count for ({i}, {neighbourPointIndex}), defaulting to 1 for {prefabItem.Prefab.FilePath}");
                                    mapPointLaneCount = neighbourLaneCount = 1;
                                }

                                var cornerCoords = new List<PointF>();

                                var coords = RenderHelper.GetCornerCoords(prefabstartX + mapPoint.X, prefabStartZ + mapPoint.Z,
                                    (Consts.LaneWidth * mapPointLaneCount + mapPoint.LaneOffset) / 2f, roadYaw + Math.PI / 2);

                                cornerCoords.Add(RenderHelper.RotatePoint(coords.X, coords.Y, rot, originNode.X, originNode.Z));

                                coords = RenderHelper.GetCornerCoords(prefabstartX + neighbourPoint.X, prefabStartZ + neighbourPoint.Z,
                                    (Consts.LaneWidth * neighbourLaneCount + neighbourPoint.LaneOffset) / 2f,
                                    roadYaw + Math.PI / 2);
                                cornerCoords.Add(RenderHelper.RotatePoint(coords.X, coords.Y, rot, originNode.X, originNode.Z));

                                coords = RenderHelper.GetCornerCoords(prefabstartX + neighbourPoint.X, prefabStartZ + neighbourPoint.Z,
                                    (Consts.LaneWidth * neighbourLaneCount + mapPoint.LaneOffset) / 2f,
                                    roadYaw - Math.PI / 2);
                                cornerCoords.Add(RenderHelper.RotatePoint(coords.X, coords.Y, rot, originNode.X, originNode.Z));

                                coords = RenderHelper.GetCornerCoords(prefabstartX + mapPoint.X, prefabStartZ + mapPoint.Z,
                                    (Consts.LaneWidth * mapPointLaneCount + mapPoint.LaneOffset) / 2f, roadYaw - Math.PI / 2);
                                cornerCoords.Add(RenderHelper.RotatePoint(coords.X, coords.Y, rot, originNode.X, originNode.Z));

                                TsPrefabLook prefabLook = new TsPrefabPolyLook(cornerCoords)
                                {
                                    Color = palette.PrefabRoad,
                                    ZIndex = 4,
                                };

                                prefabItem.AddLook(prefabLook);
                            }
                        }
                    }

                    prefabItem.GetLooks().ForEach(x => drawingQueue.Add(x));
                }

                foreach (var prefabLook in drawingQueue.OrderBy(p => p.ZIndex))
                {
                    prefabLook.Draw(g);
                }
            }
            var prefabTime = DateTime.Now.Ticks - prefabStartTime;

            var roadStartTime = DateTime.Now.Ticks;
            if (renderFlags.IsActive(RenderFlags.Roads))
            {
                foreach (var road in _mapper.Roads)
                {
                    if (!activeDlcGuards.Contains(road.DlcGuard) ||
                        road.IsSecret && !renderFlags.IsActive(RenderFlags.SecretRoads) ||
                        road.X < startPoint.X - itemDrawMargin || road.X > endPoint.X + itemDrawMargin ||
                        road.Z < startPoint.Y - itemDrawMargin || road.Z > endPoint.Y + itemDrawMargin)
                    {
                        continue;
                    }

                    var startNode = road.GetStartNode();
                    var endNode = road.GetEndNode();

                    if (!road.HasPoints())
                    {
                        var newPoints = new List<PointF>();

                        var sx = startNode.X;
                        var sz = startNode.Z;
                        var ex = endNode.X;
                        var ez = endNode.Z;

                        var radius = Math.Sqrt(Math.Pow(sx - ex, 2) + Math.Pow(sz - ez, 2));

                        var tanSx = Math.Cos(-(Math.PI * 0.5f - startNode.Rotation)) * radius;
                        var tanEx = Math.Cos(-(Math.PI * 0.5f - endNode.Rotation)) * radius;
                        var tanSz = Math.Sin(-(Math.PI * 0.5f - startNode.Rotation)) * radius;
                        var tanEz = Math.Sin(-(Math.PI * 0.5f - endNode.Rotation)) * radius;

                        for (var i = 0; i < 8; i++)
                        {
                            var s = i / (float) (8 - 1);
                            var x = (float) TsRoadLook.Hermite(s, sx, ex, tanSx, tanEx);
                            var z = (float) TsRoadLook.Hermite(s, sz, ez, tanSz, tanEz);
                            newPoints.Add(new PointF(x, z));
                        }
                        road.AddPoints(newPoints);
                    }

                    var roadWidth = road.RoadLook.GetWidth();
                    Pen roadPen;
                    if (road.IsSecret)
                    {
                        if (zoomIndex < 3)
                        {
                            roadPen = new Pen(palette.Road, roadWidth) {DashPattern = new[] {1f, 1f}};
                        }
                        else // zoomed out with DashPattern causes OutOfMemory Exception
                        {
                            roadPen = new Pen(palette.Road, roadWidth);
                        }
                    }
                    else
                    {
                        roadPen = new Pen(palette.Road, roadWidth);
                    }
                    g.DrawCurve(roadPen, road.GetPoints()?.ToArray());
                    roadPen.Dispose();
                }
            }
            var roadTime = DateTime.Now.Ticks - roadStartTime;

            var mapOverlayStartTime = DateTime.Now.Ticks;
            if (renderFlags.IsActive(RenderFlags.MapOverlays))
            {
                foreach (var mapOverlay in _mapper.OverlayManager.GetOverlays())
                {
                    if (!activeDlcGuards.Contains(mapOverlay.DlcGuard) ||
                        mapOverlay.IsSecret && !renderFlags.IsActive(RenderFlags.SecretRoads) ||
                        mapOverlay.Position.X < startPoint.X - itemDrawMargin ||
                        mapOverlay.Position.X > endPoint.X + itemDrawMargin ||
                        mapOverlay.Position.Y < startPoint.Y - itemDrawMargin ||
                        mapOverlay.Position.Y > endPoint.Y + itemDrawMargin)
                    {
                        continue;
                    }

                    var b = mapOverlay.GetBitmap();

                    if (b == null || !renderFlags.IsActive(RenderFlags.BusStopOverlay) && mapOverlay.OverlayType == OverlayType.BusStop) continue;

                    g.DrawImage(b, mapOverlay.Position.X - (b.Width / 2f), mapOverlay.Position.Y - (b.Height / 2f),
                        b.Width, b.Height);

                }
            }
            var mapOverlayTime = DateTime.Now.Ticks - mapOverlayStartTime;

            var cityStartTime = DateTime.Now.Ticks;
            if (renderFlags.IsActive(RenderFlags.CityNames)) // TODO: Fix position and scaling
            {
                var cityFont = new Font("Arial", 100 + zoomCaps[zoomIndex] / 100, FontStyle.Bold);

                foreach (var city in _mapper.Cities)
                {
                    var name = _mapper.Localization.GetLocaleValue(city.City.LocalizationToken) ?? city.City.Name;
                    var node = _mapper.GetNodeByUid(city.NodeUid);
                    var coords = (node == null) ? new PointF(city.X, city.Z) : new PointF(node.X, node.Z);
                    if (city.City.XOffsets.Count > zoomIndex && city.City.YOffsets.Count > zoomIndex)
                    {
                        coords.X += city.City.XOffsets[zoomIndex] / (scale * zoomCaps[zoomIndex]);
                        coords.Y += city.City.YOffsets[zoomIndex] / (scale * zoomCaps[zoomIndex]);
                    }

                    var textSize = g.MeasureString(name, cityFont);
                    g.DrawString(name, cityFont, _cityShadowColor, coords.X + 2, coords.Y + 2);
                    g.DrawString(name, cityFont, palette.CityName, coords.X, coords.Y);
                }
                cityFont.Dispose();
            }
            var cityTime = DateTime.Now.Ticks - cityStartTime;

            g.ResetTransform();
            var elapsedTime = DateTime.Now.Ticks - startTime;
            if (renderFlags.IsActive(RenderFlags.TextOverlay))
            {
                g.DrawString(
                    $"DrawTime: {elapsedTime / TimeSpan.TicksPerMillisecond} ms, x: {startPoint.X}, y: {startPoint.Y}, scale: {scale}",
                    _defaultFont, Brushes.WhiteSmoke, 5, 5);

                //g.FillRectangle(new SolidBrush(Color.FromArgb(100, 0, 0, 0)), 5, 20, 150, 150);
                //g.DrawString($"Road: {roadTime / TimeSpan.TicksPerMillisecond}ms", _defaultFont, Brushes.White, 10, 40);
                //g.DrawString($"Prefab: {prefabTime / TimeSpan.TicksPerMillisecond}ms", _defaultFont, Brushes.White, 10, 55);
                //g.DrawString($"Ferry: {ferryTime / TimeSpan.TicksPerMillisecond}ms", _defaultFont, Brushes.White, 10, 70);
                //g.DrawString($"MapOverlay: {mapOverlayTime / TimeSpan.TicksPerMillisecond}ms", _defaultFont, Brushes.White, 10, 85);
                //g.DrawString($"MapArea: {mapAreaTime / TimeSpan.TicksPerMillisecond}ms", _defaultFont, Brushes.White, 10, 115);
                //g.DrawString($"City: {cityTime / TimeSpan.TicksPerMillisecond}ms", _defaultFont, Brushes.White, 10, 130);
            }

        }
    }
}
