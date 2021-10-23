using System.Collections.Generic;
using System.IO;
using TsMap2.Helper;
using TsMap2.Model.TsMapItem;

namespace TsMap2.Factory.Binaries {
    public class PointBinaryFactory : BinaryFactory< List< TsMapRoadItem > > {
        private readonly List< TsMapRoadItem > _roadItems;

        public PointBinaryFactory( List< TsMapRoadItem > roadItems ) => _roadItems = roadItems;

        public override string GetSavingPath() => Path.Combine( Store.Settings.OutputPath, Store.Game.Code, "latest/", AppPath.PointsBinary );

        public override void Save() {
            Writer().Write( 1 );
            Writer().Write( _roadItems.Count );
            Writer().Write( 0 );

            foreach ( TsMapRoadItem roadItem in _roadItems ) {
                Writer().Write( roadItem.GetStartNode().X );
                Writer().Write( roadItem.GetStartNode().Z );
                Writer().Write( roadItem.GetEndNode().X );
                Writer().Write( roadItem.GetEndNode().Z );
            }
        }
    }
}