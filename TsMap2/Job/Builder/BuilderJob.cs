using System.Collections.Generic;
using Serilog;
using TsMap2.Model.TsMapItem;

namespace TsMap2.Job.Builder {
    public class BuilderJob : ThreadJob {
        protected override void Do() {
            Log.Information( "[Job][Builder] Loading" );

            foreach ( KeyValuePair< ulong, TsMapCityItem > pair in Store().Map.Cities ) {
                TsMapCityItem item = pair.Value;
                TsNode?       node = Store().Map.GetNodeByUid( item.NodeUid );
                ( float x, float z ) = node == null
                                           ? ( item.X, item.Z )
                                           : ( node.X, node.Z );

                item.City.X = x;
                item.City.Y = z;

                Log.Debug( "[Job][Builder] City '{0}' at {1},{2}", item.City.Name, item.City.X, item.City.Y );
            }

            Log.Information( "[Job][Builder] Done" );
        }
    }
}