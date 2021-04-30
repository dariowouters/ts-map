using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TsMap2.Helper {
    public static class JsonHelper {
        public static void SaveFile( string fileName, string path, JObject data ) {
            string fullPath = Path.Combine( path, fileName );

            Directory.CreateDirectory( path );
            File.WriteAllTextAsync( fullPath, JsonConvert.SerializeObject( data, Formatting.Indented ) );
        }

        public static JObject LoadFile( string fileName, string path ) {
            string fullPath = Path.Combine( path, fileName );

            return !File.Exists( fullPath )
                       ? new JObject()
                       : JsonConvert.DeserializeObject< JObject >( File.ReadAllText( fullPath ) );
        }
    }
}