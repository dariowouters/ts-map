using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TsMap2.Helper {
    public class JsonHelper {
        public void SaveFile( string fileName, string path, JObject data ) {
            string fullPath = Path.Combine( fileName, path );

            Directory.CreateDirectory( path );
            File.WriteAllTextAsync( fullPath, JsonConvert.SerializeObject( data ) );
        }

        public JObject LoadFile( string fileName, string path ) {
            string fullPath = Path.Combine( fileName, path );

            return !File.Exists( fullPath )
                       ? new JObject()
                       : JsonConvert.DeserializeObject< JObject >( File.ReadAllText( fullPath ) );
        }
    }
}