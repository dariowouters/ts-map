using System.Reflection;
using TsMap2.Exceptions;
using TsMap2.Helper;

namespace TsMap2.Scs.FileSystem.Entry {
    public abstract class AbstractScsEntry< T > {
        protected StoreHelper Store => StoreHelper.Instance;

        protected void VerifyRfs() {
            // --- Check RFS
            if ( Store.Rfs == null )
                throw new ScsEntryException( $"[{MethodBase.GetCurrentMethod()}] The root file system was not initialized. Check the game path" );
        }

        public T Get( string path ) {
            VerifyRfs();

            ScsFile file = Store.Rfs.GetFileEntry( path );

            if ( file == null ) return default;
            byte[] fileContent = file.Entry.Read();

            return Generate( fileContent );
        }

        // public abstract Dictionary< ulong, T > List();
        public abstract T Generate( byte[] stream );
    }
}