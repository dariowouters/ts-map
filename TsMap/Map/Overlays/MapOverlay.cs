using System.Drawing;

namespace TsMap.Map.Overlays
{
    internal class MapOverlay
    {
        private readonly OverlayImage _overlayImage;

        internal readonly string OverlayName;

        internal bool IsSecret { get; private set; }

        public byte ZoomLevelVisibility { get; private set; }

        public byte DlcGuard { get; private set; }

        internal MapOverlay(OverlayImage overlayImage, string overlayName)
        {
            _overlayImage = overlayImage;
            OverlayName = overlayName;
        }

        internal string TypeName { get; private set; }

        internal PointF Position { get; private set; }

        internal bool IsValid()
        {
            return _overlayImage.Valid;
        }

        internal void SetPosition(float x, float y)
        {
            Position = new PointF(x, y);
        }

        internal void SetSecret(bool secret)
        {
            IsSecret = secret;
        }

        internal void SetTypeName(string name)
        {
            TypeName = name;
        }

        internal void SetZoomLevelVisibility(byte flags)
        {
            ZoomLevelVisibility = flags;
        }

        internal void SetDlcGuard(byte dlcGuard)
        {
            DlcGuard = dlcGuard;
        }

        internal Bitmap GetBitmap()
        {
            return _overlayImage.GetBitmap();
        }
    }
}