using System;
using System.IO;
using Serilog;
using TsMap2.Helper;
using TsMap2.Scs;
using TsMap2.Scs.FileSystem.Map;

namespace TsMap2.Model.TsMapItem {
    public class TsMapTriggerItem : TsMapItem {
        public TsMapTriggerItem( ScsSector sector ) : base( sector ) {
            Valid = true;
            if ( Sector.Version < 829 )
                TsTriggerItem825();
            else if ( Sector.Version >= 829 && Sector.Version < 875 )
                TsTriggerItem829();
            else if ( Sector.Version >= 875 )
                TsTriggerItem875();
            else
                Log.Warning( $"Unknown base file version ({Sector.Version}) for item {Type} in file '{Path.GetFileName( Sector.FilePath )}' @ {Sector.LastOffset}." );
        }

        public string       OverlayName { get; private set; }
        public TsMapOverlay Overlay     { get; private set; }

        private void TsTriggerItem825() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8( Sector.Stream, fileOffset + 0x01 ) > dlcGuardCount;
            int nodeCount          = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x05 );                    // 0x05(flags)
            int tagCount           = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + 0x08 * nodeCount ); // 0x04(nodeCount) + nodeUids
            int triggerActionCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + 0x08 * tagCount );  // 0x04(tagCount) + tags
            fileOffset += 0x04;                                                                                      // cursor after triggerActionCount

            for ( var i = 0; i < triggerActionCount; i++ ) {
                ulong action = MemoryHelper.ReadUInt64( Sector.Stream, fileOffset );
                if ( action == ScsHashHelper.StringToToken( "hud_parking" ) ) {
                    OverlayName = "parking_ico";
                    Overlay     = Store().Def.LookupOverlay( ScsHashHelper.StringToToken( OverlayName ) );
                    if ( Overlay == null ) {
                        Console.WriteLine( "Could not find parking overlay" );
                        Valid = false;
                    }
                }

                int hasParameters = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x08 ); // 0x08(action)
                fileOffset += 0x04;                                                              // set cursor after hasParameters
                if ( hasParameters == 1 ) {
                    int parametersLength = MemoryHelper.ReadInt32( Sector.Stream, fileOffset );
                    fileOffset += 0x04 + 0x04 + parametersLength;    // 0x04(parametersLength) + 0x04(padding) + text(parametersLength * 0x01)
                } else if ( hasParameters == 3 ) fileOffset += 0x08; // 0x08 (m_some_uid)

                int targetTagCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset );
                fileOffset += 0x04 + targetTagCount * 0x08; // 0x04(targetTagCount) + targetTags
            }

            fileOffset += 0x18; // 0x18(range & reset_delay & reset_distance & min_speed & max_speed & flags2)
            BlockSize  =  fileOffset - Sector.LastOffset;
        }

        private void TsTriggerItem829() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8( Sector.Stream, fileOffset + 0x01 ) > dlcGuardCount;
            int tagCount  = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x05 );                   // 0x05(flags)
            int nodeCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + 0x08 * tagCount ); // 0x04(nodeCount) + tags

            int triggerActionCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + 0x08 * nodeCount ); // 0x04(nodeCount) + nodeUids
            fileOffset += 0x04;                                                                                      // cursor after triggerActionCount

            for ( var i = 0; i < triggerActionCount; i++ ) {
                ulong action = MemoryHelper.ReadUInt64( Sector.Stream, fileOffset );
                if ( action == ScsHashHelper.StringToToken( "hud_parking" ) ) {
                    OverlayName = "parking_ico";
                    Overlay     = Store().Def.LookupOverlay( ScsHashHelper.StringToToken( OverlayName ) );
                    if ( Overlay == null ) {
                        Console.WriteLine( "Could not find parking overlay" );
                        Valid = false;
                    }
                }

                int hasOverride                   = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x08 ); // 0x08(action)
                if ( hasOverride > 0 ) fileOffset += 0x04 * hasOverride;

                int hasParameters = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 ); // 0x04(hasOverride)
                fileOffset += 0x04;                                                              // set cursor after hasParameters
                if ( hasParameters == 1 ) {
                    int parametersLength = MemoryHelper.ReadInt32( Sector.Stream, fileOffset );
                    fileOffset += 0x04 + 0x04 + parametersLength;    // 0x04(parametersLength) + 0x04(padding) + text(parametersLength * 0x01)
                } else if ( hasParameters == 3 ) fileOffset += 0x08; // 0x08 (m_some_uid)

                int targetTagCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x08 ); // 0x08(unk/padding)
                fileOffset += 0x04 + targetTagCount * 0x08;                                       // 0x04(targetTagCount) + targetTags
            }

            fileOffset += 0x18; // 0x18(range & reset_delay & reset_distance & min_speed & max_speed & flags2)
            BlockSize  =  fileOffset - Sector.LastOffset;
        }

        private void TsTriggerItem875() {
            int fileOffset = Sector.LastOffset + 0x34; // Set position at start of flags
            int dlcGuardCount = Store().Game.IsEts2()
                                    ? ScsConst.Ets2DlcGuardCount
                                    : ScsConst.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8( Sector.Stream, fileOffset + 0x01 ) > dlcGuardCount;
            int tagCount  = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x05 );                   // 0x05(flags)
            int nodeCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + 0x08 * tagCount ); // 0x04(nodeCount) + tags

            int triggerActionCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + 0x08 * nodeCount ); // 0x04(nodeCount) + nodeUids
            fileOffset += 0x04;                                                                                      // cursor after triggerActionCount

            for ( var i = 0; i < triggerActionCount; i++ ) {
                ulong action = MemoryHelper.ReadUInt64( Sector.Stream, fileOffset );
                if ( action == ScsHashHelper.StringToToken( "hud_parking" ) ) {
                    OverlayName = "parking_ico";
                    Overlay     = Store().Def.LookupOverlay( ScsHashHelper.StringToToken( OverlayName ) );
                    if ( Overlay == null ) {
                        Console.WriteLine( "Could not find parking overlay" );
                        Valid = false;
                    }
                }

                int hasOverride = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x08 ); // 0x08(action)
                fileOffset += 0x04;                                                            // set cursor after hasOverride
                if ( hasOverride < 0 ) continue;
                fileOffset += 0x04 * hasOverride; // set cursor after override values

                int parameterCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset );
                fileOffset += 0x04; // set cursor after parameterCount

                for ( var j = 0; j < parameterCount; j++ ) {
                    int paramLength = MemoryHelper.ReadInt32( Sector.Stream, fileOffset );
                    fileOffset += 0x04 + 0x04 + paramLength; // 0x04(paramLength) + 0x04(padding) + (param)
                }

                int targetTagCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset );

                fileOffset += 0x04 + targetTagCount * 0x08 + 0x08; // 0x04(targetTagCount) + targetTags + 0x04(m_range & m_type)
            }

            if ( nodeCount == 1 ) fileOffset += 0x04; // 0x04(m_radius)
            BlockSize = fileOffset - Sector.LastOffset;
        }
    }
}