using System.Text;

namespace TsMap2.Scs.FileSystem.Entry {
    public class ScsVersionEntry : AbstractScsEntry< string > {
        public string Get() {
            VerifyRfs();

            ScsFile versionFile = Store.Rfs.GetFileEntry( ScsPath.GameVersion );

            return Generate( versionFile.Entry.Read() );
        }

        public override string Generate( byte[] stream ) => Encoding.UTF8.GetString( stream ).Split( '\n' )[ 0 ];
    }
}