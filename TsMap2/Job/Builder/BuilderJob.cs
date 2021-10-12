using System.Collections.Generic;
using System.Linq;
using Serilog;
using TsMap2.Model.TsMapItem;

namespace TsMap2.Job.Builder {
    public class BuilderJob : ThreadJob {
        protected override void Do() {
            Log.Information( "[Job][Builder] Loading" );

            foreach ( KeyValuePair< ulong, TsMapCityItem > pair in Store().Map.Cities ) {
                TsMapCityItem item = pair.Value;
                TsNode?       node = Store().Map.GetNodeByUid( item.NodeUid );

                if ( node == null ) {
                    Log.Debug( "[Job][Builder] City '{0}' skipped and will be removed", item.City.Name );
                    continue;
                }

                ( float x, float z ) = ( node.X, node.Z );
                item.City.X          = x;
                item.City.Y          = z;

                Log.Debug( "[Job][Builder] City '{0}' at {1},{2}", item.City.Name, item.City.X, item.City.Y );
            }

            // FIXME: Check if it's possible to read data from Node
            Store().Def.Cities = Store()
                                 .Def.Cities
                                 .Where( kv => kv.Value.X != 0 && kv.Value.Y != 0 )
                                 .ToDictionary( kv => kv.Key, kv => kv.Value );

            Log.Information( "[Job][Builder] Done" );
        }
    }
}