using System.Drawing;

namespace TsMap2.Helper.Map {
    public static class MapHelper {
        private static StoreHelper Store => StoreHelper.Instance;

        public static void ZoomOutAndCenterMap( float targetWidth, float targetHeight, out PointF pos, out float zoom ) {
            float mapWidth = Store.Map.MaxX
                             - Store.Map.MinX
                             + Store.Settings.ExportSettings.TilePadding * 2;
            float mapHeight = Store.Map.MaxZ
                              - Store.Map.MinZ
                              + Store.Settings.ExportSettings.TilePadding * 2;

            if ( mapWidth > mapHeight ) // get the scale to have the map edge to edge on the biggest axis (with padding)
            {
                zoom = targetWidth / mapWidth;
                float z = Store.Map.MinZ
                          - Store.Settings.ExportSettings.TilePadding
                          + -( targetHeight / zoom ) / 2f
                          + mapHeight                / 2f;
                pos =
                    new
                        PointF( Store.Map.MinX - Store.Settings.ExportSettings.TilePadding,
                                z );
            } else {
                zoom = targetHeight / mapHeight;
                float x = Store.Map.MinX
                          - Store.Settings.ExportSettings.TilePadding
                          + -( targetWidth / zoom ) / 2f
                          + mapWidth                / 2f;
                pos =
                    new PointF( x,
                                Store.Map.MinZ
                                - Store.Settings.ExportSettings.TilePadding );
            }
        }
    }
}