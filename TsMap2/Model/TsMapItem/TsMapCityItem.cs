using System.IO;
using Serilog;
using TsMap2.Helper;
using TsMap2.Scs.FileSystem.Map;

namespace TsMap2.Model.TsMapItem {
    public class TsMapCityItem : TsMapItem // TODO: Add zoom levels/range to show city names and icons correctly
    {
        public TsMapCityItem( ScsSector sector ) : base( sector ) {
            Valid = true;
            TsCityItem825();
        }

        public TsCity City    { get; private set; }
        public ulong  NodeUid { get; private set; }
        public float  Width   { get; private set; }
        public float  Height  { get; private set; }

        private void TsCityItem825() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags

            Hidden = ( MemoryHelper.ReadUint8( Sector.Stream, fileOffset ) & 0x01 ) != 0;
            ulong cityId = MemoryHelper.ReadUInt64( Sector.Stream, fileOffset + 0x05 );
            City = Store().Def.LookupCity( cityId );
            if ( City == null ) {
                Valid = false;
                Log.Warning( $"Could not find City: '{ScsHashHelper.TokenToString( cityId )}'({cityId:X}), "
                             + $"in {Path.GetFileName( Sector.FilePath )} @ {fileOffset}" );
            }

            Width      =  MemoryHelper.ReadSingle( Sector.Stream, fileOffset += 0x05 + 0x08 ); // 0x05(flags) + 0x08(cityId)
            Height     =  MemoryHelper.ReadSingle( Sector.Stream, fileOffset += 0x04 );        // 0x08(Width)
            NodeUid    =  MemoryHelper.ReadUInt64( Sector.Stream, fileOffset += 0x04 );        // 0x08(height)
            fileOffset += 0x08;                                                                // nodeUid
            BlockSize  =  fileOffset - Sector.LastOffset;
        }

        public override string ToString() {
            if ( City == null ) return "Error";
            TsCountry country = Store().Def.GetCountryByTokenName( City.CountryName );
            string countryName = country == null
                                     ? City.CountryName
                                     : country.GetLocalizedName( Store().Settings.SelectedLocalization );
            return $"{countryName} - {City.GetLocalizedName( Store().Settings.SelectedLocalization )}";
        }
    }
}