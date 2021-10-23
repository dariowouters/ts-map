using System.Collections.Generic;
using System.IO;
using TsMap2.Helper;
using TsMap2.Model;

namespace TsMap2.Factory.Binaries {
    public class TsCitiesBinaryFactory : BinaryFactory< List< TsCity > > {
        private readonly List< TsCity > _cities;
        public TsCitiesBinaryFactory( List< TsCity > cities ) => _cities = cities;

        public override string GetSavingPath() => Path.Combine( Store.Settings.OutputPath, Store.Game.Code, "latest/", AppPath.CitiesBinary );

        public override void Save() {
            Writer().Write( 1 );
            Writer().Write( _cities.Count );
            Writer().Write( 0 );

            foreach ( TsCity city in _cities ) {
                Writer().Write( city.X );
                Writer().Write( city.Y );
            }
        }
    }
}