using System.Drawing;
using System.Drawing.Imaging;

namespace TsMap2.Factory.Picture {
    public class TileFactory : PictureFactory {
        public TileFactory( Bitmap bitmap ) : base( bitmap ) { }

        public void Save( int z, int x, int y ) {
            var fileName = $"{y}.png";
            var dir      = $"Tiles/{z}/{x}";
            Save( dir, fileName, ImageFormat.Png );
        }
    }
}