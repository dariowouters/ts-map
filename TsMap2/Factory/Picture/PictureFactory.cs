using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using TsMap2.Helper;

namespace TsMap2.Factory.Picture {
    public class PictureFactory {
        private readonly Bitmap      _bitmap;
        private static   StoreHelper Store => StoreHelper.Instance;

        protected PictureFactory( Bitmap bitmap ) => _bitmap = bitmap;

        protected void Save( string dir, string fileName, ImageFormat format ) {
            string fullDir  = Path.Combine( Store.Settings.OutputPath, Store.Game.Code, "latest/", dir );
            string fullPath = Path.Combine( fullDir,                   fileName );

            Directory.CreateDirectory( fullDir );
            _bitmap.Save( fullPath, format );
        }
    }
}