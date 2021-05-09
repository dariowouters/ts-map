using System.IO;
using Serilog;
using TsMap2.Helper;

namespace TsMap2.Model.TsMapItem {
    public class TsRoadSideItem : TsItem {
        public TsRoadSideItem( TsSector sector, int startOffset ) : base( sector, startOffset ) {
            this.Valid = false;
            if ( this.Sector.Version < 846 )
                this.TsRoadSideItem825( startOffset );
            else if ( this.Sector.Version >= 846 && this.Sector.Version < 855 )
                this.TsRoadSideItem846( startOffset );
            else if ( this.Sector.Version >= 855 && this.Sector.Version < 875 )
                this.TsRoadSideItem855( startOffset );
            else if ( this.Sector.Version >= 875 )
                this.TsRoadSideItem875( startOffset );
            else
                Log.Warning(
                            $"Unknown base file version ({this.Sector.Version}) for item {this.Type} in file '{Path.GetFileName( this.Sector.FilePath )}' @ {startOffset}." );
        }

        public void TsRoadSideItem825( int startOffset ) {
            int fileOffset = startOffset + 0x34; // Set position at start of flags
            fileOffset += 0x15 + 3 * 0x18;       // 0x15(flags & sign_id & node_uid) + 3 * 0x18(sign_template_t)

            int tmplTextLength                    = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset );
            if ( tmplTextLength != 0 ) fileOffset += 0x04 + tmplTextLength; // 0x04(textPadding)

            int signAreaCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 ); // 0x04(cursor after tmplTextLength)
            fileOffset += 0x04;                                                                   // cursor after signAreaCount
            for ( var i = 0; i < signAreaCount; i++ ) {
                int subItemCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 ); // 0x04(some float value)
                fileOffset += 0x04;                                                                  // cursor after subItemCount
                for ( var x = 0; x < subItemCount; x++ ) {
                    short itemType = MemoryHelper.ReadInt16( this.Sector.Stream, fileOffset );

                    fileOffset += 0x06; // cursor after (count & itemType)

                    if ( itemType == 0x05 ) {
                        int textLength = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset );
                        fileOffset += 0x04 + 0x04 + textLength; // 0x04(cursor after textlength) + 0x04(padding)
                    }
                    //else if (itemType == 0x06)
                    //{
                    //    fileOffset += 0x04; // 0x04(padding)
                    //}
                    else if ( itemType == 0x01 )
                        fileOffset += 0x01; // 0x01(padding)
                    else
                        fileOffset += 0x04; // 0x04(padding)
                }
            }

            this.BlockSize = fileOffset - startOffset;
        }

        public void TsRoadSideItem846( int startOffset ) {
            int fileOffset = startOffset + 0x34; // Set position at start of flags
            fileOffset += 0x15 + 3 * 0x18;       // 0x15(flags & sign_id & node_uid) + 3 * 0x18(sign_template_t)

            int tmplTextLength                    = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset );
            if ( tmplTextLength != 0 ) fileOffset += 0x04 + tmplTextLength; // 0x04(textPadding)

            int signAreaCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 ); // 0x04(cursor after tmplTextLength)
            fileOffset += 0x04;                                                                   // cursor after signAreaCount
            for ( var i = 0; i < signAreaCount; i++ ) {
                int subItemCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x0C ); // 0x0C(padding)
                fileOffset += 0x04;                                                                  // cursor after subItemCount
                for ( var x = 0; x < subItemCount; x++ ) {
                    short itemType = MemoryHelper.ReadInt16( this.Sector.Stream, fileOffset );

                    fileOffset += 0x06; // cursor after (count & itemType)

                    if ( itemType == 0x05 ) {
                        int textLength = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset );
                        fileOffset += 0x04 + 0x04 + textLength; // 0x04(cursor after textlength) + 0x04(padding)
                    }
                    //else if (itemType == 0x06)
                    //{
                    //    fileOffset += 0x04; // 0x04(padding)
                    //}
                    else if ( itemType == 0x01 )
                        fileOffset += 0x01; // 0x01(padding)
                    else
                        fileOffset += 0x04; // 0x04(padding)
                }
            }

            this.BlockSize = fileOffset - startOffset;
        }

        public void TsRoadSideItem855( int startOffset ) {
            int fileOffset = startOffset + 0x34; // Set position at start of flags
            fileOffset += 0x15 + 3 * 0x18;       // 0x15(flags & sign_id & node_uid) + 3 * 0x18(sign_template_t)

            int tmplTextLength                    = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset );
            if ( tmplTextLength != 0 ) fileOffset += 0x04 + tmplTextLength; // 0x04(textPadding)

            int signAreaCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 ); // 0x04(cursor after tmplTextLength)
            fileOffset += 0x04;                                                                   // cursor after signAreaCount
            for ( var i = 0; i < signAreaCount; i++ ) {
                int subItemCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x0C ); // 0x0C(padding)
                fileOffset += 0x04;                                                                  // cursor after subItemCount
                for ( var x = 0; x < subItemCount; x++ ) {
                    short itemType = MemoryHelper.ReadInt16( this.Sector.Stream, fileOffset );

                    int itemCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x02 ); // 0x02(itemType)

                    fileOffset += 0x04; // cursor after count

                    if ( itemType == 0x05 ) {
                        int textLength = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset );
                        fileOffset += 0x04 + 0x04 + textLength; // 0x04(cursor after textlength) + 0x04(padding)
                    } else if ( itemType == 0x06 )
                        fileOffset += 0x08 * itemCount; // 0x08(m_stand_id)
                    else if ( itemType == 0x01 )
                        fileOffset += 0x01; // 0x01(padding)
                    else
                        fileOffset += 0x04; // 0x04(padding)
                }
            }

            this.BlockSize = fileOffset - startOffset;
        }

        public void TsRoadSideItem875( int startOffset ) {
            int   fileOffset    = startOffset + 0x34;                                              // Set position at start of flags
            sbyte templateCount = MemoryHelper.ReadInt8( this.Sector.Stream, fileOffset += 0x25 ); // 0x20(flags & sign_id & uid & look & variant)
            fileOffset += 0x01 + templateCount * 0x18;                                             // 0x01(templateCount) + templateCount * 0x18(sign_template_t)

            int tmplTextLength                    = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset );
            if ( tmplTextLength != 0 ) fileOffset += 0x04 + tmplTextLength; // 0x04(textPadding)

            int signAreaCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x04 ); // 0x04(cursor after tmplTextLength)
            fileOffset += 0x04;                                                                   // cursor after signAreaCount
            for ( var i = 0; i < signAreaCount; i++ ) {
                int subItemCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x0C ); // 0x0C(padding)
                fileOffset += 0x04;                                                                  // cursor after subItemCount
                for ( var x = 0; x < subItemCount; x++ ) {
                    short itemType = MemoryHelper.ReadInt16( this.Sector.Stream, fileOffset );

                    int itemCount = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset += 0x02 ); // 0x02(itemType)

                    fileOffset += 0x04; // cursor after count

                    if ( itemType == 0x05 ) {
                        int textLength = MemoryHelper.ReadInt32( this.Sector.Stream, fileOffset );
                        fileOffset += 0x04 + 0x04 + textLength; // 0x04(cursor after textlength) + 0x04(padding)
                    } else if ( itemType == 0x06 )
                        fileOffset += 0x08 * itemCount; // 0x08(m_stand_id)
                    else if ( itemType == 0x01 )
                        fileOffset += 0x01; // 0x01(padding)
                    else
                        fileOffset += 0x04; // 0x04(padding)
                }
            }

            this.BlockSize = fileOffset - startOffset;
        }
    }
}