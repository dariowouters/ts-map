using System.IO;
using Serilog;
using TsMap2.Helper;
using TsMap2.Scs;

namespace TsMap2.Model.TsMapItem {
    public class TsMapCityItem : TsMapItem // TODO: Add zoom levels/range to show city names and icons correctly
    {
        public TsMapCityItem( TsSector sector, int startOffset ) : base( sector, startOffset ) {
            this.Valid = true;
            this.TsCityItem825( startOffset );
        }

        public TsCity City    { get; private set; }
        public ulong  NodeUid { get; private set; }
        public float  Width   { get; private set; }
        public float  Height  { get; private set; }

        public void TsCityItem825( int startOffset ) {
            int fileOffset = startOffset + 0x34; // Set position at start of flags

            this.Hidden = ( MemoryHelper.ReadUint8( this.Sector.Stream, fileOffset ) & 0x01 ) != 0;
            ulong cityId = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset + 0x05 );
            this.City = Store().Def.LookupCity( cityId );
            if ( this.City == null ) {
                this.Valid = false;
                Log.Warning( $"Could not find City: '{ScsHash.TokenToString( cityId )}'({cityId:X}), "
                             + $"in {Path.GetFileName( this.Sector.FilePath )} @ {fileOffset}" );
            }

            this.Width     =  MemoryHelper.ReadSingle( this.Sector.Stream, fileOffset += 0x05 + 0x08 ); // 0x05(flags) + 0x08(cityId)
            this.Height    =  MemoryHelper.ReadSingle( this.Sector.Stream, fileOffset += 0x04 );        // 0x08(Width)
            this.NodeUid   =  MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x04 );        // 0x08(height)
            fileOffset     += 0x08;                                                                     // nodeUid
            this.BlockSize =  fileOffset - startOffset;
        }

        public override string ToString() {
            if ( this.City == null ) return "Error";
            TsCountry country = Store().Def.GetCountryByTokenName( this.City.CountryName );
            string countryName = country == null
                                     ? this.City.CountryName
                                     : country.GetLocalizedName( Store().Settings.SelectedLocalization );
            return $"{countryName} - {this.City.GetLocalizedName( Store().Settings.SelectedLocalization )}";
        }
    }
}