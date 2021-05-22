using System.Collections.Generic;
using System.IO;
using Serilog;
using TsMap2.Helper;
using TsMap2.Scs;

namespace TsMap2.Model.TsMapItem {
    public class TsMapCompanyItem : TsMapItem {
        public TsMapCompanyItem( TsSector sector, int startOffset ) : base( sector, startOffset ) {
            this.Valid = true;
            this.Nodes = new List< ulong >();
            if ( this.Sector.Version < 858 )
                this.TsCompanyItem825( startOffset );
            else if ( this.Sector.Version >= 858 )
                this.TsCompanyItem858( startOffset );
            else
                Log.Warning( $"Unknown base file version ({this.Sector.Version}) for item {this.Type} in file '{Path.GetFileName( this.Sector.FilePath )}' @ {startOffset}." );
        }

        public ulong        OverlayToken { get; private set; }
        public TsMapOverlay Overlay      { get; private set; }

        public void TsCompanyItem825( int startOffset ) {
            int fileOffset = startOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            this.Hidden = MemoryHelper.ReadInt8( this.Sector.Stream, fileOffset + 0x01 ) > dlcGuardCount;

            this.OverlayToken = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)

            this.Overlay = Store().Def.LookupOverlay( this.OverlayToken );
            if ( this.Overlay == null ) {
                this.Valid = false;
                if ( this.OverlayToken != 0 )
                    Log.Warning( $"Could not find Company Overlay: '{ScsHash.TokenToString( this.OverlayToken )}'({this.OverlayToken:X}), in {Path.GetFileName( this.Sector.FilePath )} @ {fileOffset}" );
            }

            this.Nodes.Add( MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x08 + 0x08 ) ); // (prefab uid) | 0x08(OverlayToken) + 0x08(uid[0])

            int count = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x08 + 0x08 );               // count | 0x08 (uid[1] & uid[2])
            count          =  MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + 0x08 * count ); // count2
            count          =  MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + 0x08 * count ); // count3
            count          =  MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + 0x08 * count ); // count4
            count          =  MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + 0x08 * count ); // count5
            fileOffset     += 0x04       + 0x08 * count;
            this.BlockSize =  fileOffset - startOffset;
        }

        public void TsCompanyItem858( int startOffset ) {
            int fileOffset = startOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            this.Hidden = MemoryHelper.ReadInt8( this.Sector.Stream, fileOffset + 0x01 ) > dlcGuardCount;

            this.OverlayToken = MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x05 ); // 0x05(flags)

            this.Overlay = Store().Def.LookupOverlay( this.OverlayToken );
            if ( this.Overlay == null ) {
                this.Valid = false;
                if ( this.OverlayToken != 0 )
                    Log.Warning(
                                $"Could not find Company Overlay: '{ScsHash.TokenToString( this.OverlayToken )}'({this.OverlayToken:X}), in {Path.GetFileName( this.Sector.FilePath )} @ {fileOffset}" );
            }

            this.Nodes.Add( MemoryHelper.ReadUInt64( this.Sector.Stream, fileOffset += 0x08 + 0x08 ) ); // (prefab uid) | 0x08(OverlayToken) + 0x08(uid[0])

            int count = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x08 + 0x08 );               // count | 0x08 (uid[1] & uid[2])
            count          =  MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + 0x08 * count ); // count2
            count          =  MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + 0x08 * count ); // count3
            count          =  MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + 0x08 * count ); // count4
            count          =  MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + 0x08 * count ); // count5
            count          =  MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 + 0x08 * count ); // count6
            fileOffset     += 0x04       + 0x08 * count;
            this.BlockSize =  fileOffset - startOffset;
        }
    }
}