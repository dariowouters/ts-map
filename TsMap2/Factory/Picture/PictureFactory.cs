using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using TsMap2.Helper;

namespace TsMap2.Factory.Picture {
    public class PictureFactory {
        private readonly Bitmap _bitmap;

        protected PictureFactory( Bitmap bitmap ) => _bitmap = bitmap;

        protected void Save( string dir, string fileName, ImageFormat format ) {
            string fullDir  = Path.Combine( AppPath.OutputDir, dir );
            string fullPath = Path.Combine( fullDir,           fileName );

            // Log.Debug( "Dir: {0} | Filename: {1} | Full dir: {2} | Full path: {3} ",dir, fileName, fullDir, fullPath );

            // Directory.CreateDirectory( $"{exportPath}/Tiles/{z}/{x}" );
            Directory.CreateDirectory( fullDir );
            // _bitmap.Save( $"{exportPath}/Tiles/{z}/{x}/{y}.png", ImageFormat.Png );
            _bitmap.Save( fullPath, format );
        }
    }
}