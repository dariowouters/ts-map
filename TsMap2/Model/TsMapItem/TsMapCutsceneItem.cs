using System.IO;
using Serilog;
using TsMap2.Helper;

namespace TsMap2.Model.TsMapItem {
    public class TsMapCutsceneItem : TsMapItem {
        public TsMapCutsceneItem( TsSector sector, int startOffset ) : base( sector, startOffset ) {
            Valid = false;

            if ( Sector.Version >= 884 )
                TsCutsceneItem844( startOffset );
            else
                Log.Warning(
                            $"Unknown base file version ({Sector.Version}) for item {Type} in file '{Path.GetFileName( Sector.FilePath )}' @ {startOffset}." );
        }

        public void TsCutsceneItem844( int startOffset ) {
            int fileOffset  = startOffset + 0x34;                                                                    // Set position at start of flags
            int tagsCount   = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x05 );                           // 0x05(flags)
            int actionCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + 0x08 * tagsCount + 0x08 ); // 0x04(tagsCount) + tags + 0x08(node_uid)
            fileOffset += 0x04;                                                                                      // 0x04(actionCount)
            for ( var i = 0; i < actionCount; ++i ) {
                int numParamCount    = MemoryHelper.ReadInt32( Sector.Stream, fileOffset );
                int stringParamCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset += 0x04 + 0x04 * numParamCount ); // 0x04
                fileOffset += 0x04;                                                                                        // 0x04(stringParamCount)
                for ( var s = 0; s < stringParamCount; ++s ) {
                    int textLength = MemoryHelper.ReadInt32( Sector.Stream, fileOffset );
                    fileOffset += 0x04 + 0x04 + textLength; // 0x04(textLength, could be Uint64) + 0x04(padding) + textLength
                }

                int targetTagCount = MemoryHelper.ReadInt32( Sector.Stream, fileOffset );
                fileOffset += 0x04 + targetTagCount * 0x08 + 0x08; // 0x04(targetTagCount) + targetTags + 0x08(target_range + action_flags)
            }

            BlockSize = fileOffset - startOffset;
        }
    }
}