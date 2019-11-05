using System.IO;

namespace TsMap.TsItem
{
    public class TsRoadSideItem : TsItem
    {
        public TsRoadSideItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = false;
            if (Sector.Version < 846)
                TsRoadSideItem825(startOffset);
            else if (Sector.Version >= 846 && Sector.Version < 855)
                TsRoadSideItem846(startOffset);
            else if (Sector.Version >= 855 && Sector.Version < 875)
                TsRoadSideItem855(startOffset);
            else if (Sector.Version >= 875)
                TsRoadSideItem875(startOffset);
            else
                Log.Msg(
                    $"Unknown base file version ({Sector.Version}) for item {Type} in file '{Path.GetFileName(Sector.FilePath)}' @ {startOffset}.");
        }

        public void TsRoadSideItem825(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            fileOffset += 0x15 + 3 * 0x18; // 0x15(flags & sign_id & node_uid) + 3 * 0x18(sign_template_t)

            var tmplTextLength = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
            if (tmplTextLength != 0)
            {
                fileOffset += 0x04 + tmplTextLength; // 0x04(textPadding)
            }
            var signAreaCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04); // 0x04(cursor after tmplTextLength)
            fileOffset += 0x04; // cursor after signAreaCount
            for (var i = 0; i < signAreaCount; i++)
            {
                var subItemCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04); // 0x04(some float value)
                fileOffset += 0x04; // cursor after subItemCount
                for (var x = 0; x < subItemCount; x++)
                {
                    var itemType = MemoryHelper.ReadInt16(Sector.Stream, fileOffset);

                    fileOffset += 0x06;// cursor after (count & itemType)

                    if (itemType == 0x05)
                    {
                        var textLength = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
                        fileOffset += 0x04 + 0x04 + textLength; // 0x04(cursor after textlength) + 0x04(padding)
                    }
                    //else if (itemType == 0x06)
                    //{
                    //    fileOffset += 0x04; // 0x04(padding)
                    //}
                    else if (itemType == 0x01)
                    {
                        fileOffset += 0x01; // 0x01(padding)
                    }
                    else
                    {
                        fileOffset += 0x04; // 0x04(padding)
                    }
                }
            }
            BlockSize = fileOffset - startOffset;
        }
        public void TsRoadSideItem846(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            fileOffset += 0x15 + 3 * 0x18; // 0x15(flags & sign_id & node_uid) + 3 * 0x18(sign_template_t)

            var tmplTextLength = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
            if (tmplTextLength != 0)
            {
                fileOffset += 0x04 + tmplTextLength; // 0x04(textPadding)
            }
            var signAreaCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04); // 0x04(cursor after tmplTextLength)
            fileOffset += 0x04; // cursor after signAreaCount
            for (var i = 0; i < signAreaCount; i++)
            {
                var subItemCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x0C); // 0x0C(padding)
                fileOffset += 0x04; // cursor after subItemCount
                for (var x = 0; x < subItemCount; x++)
                {
                    var itemType = MemoryHelper.ReadInt16(Sector.Stream, fileOffset);

                    fileOffset += 0x06;// cursor after (count & itemType)

                    if (itemType == 0x05)
                    {
                        var textLength = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
                        fileOffset += 0x04 + 0x04 + textLength; // 0x04(cursor after textlength) + 0x04(padding)
                    }
                    //else if (itemType == 0x06)
                    //{
                    //    fileOffset += 0x04; // 0x04(padding)
                    //}
                    else if (itemType == 0x01)
                    {
                        fileOffset += 0x01; // 0x01(padding)
                    }
                    else
                    {
                        fileOffset += 0x04; // 0x04(padding)
                    }
                }
            }
            BlockSize = fileOffset - startOffset;
        }
        public void TsRoadSideItem855(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            fileOffset += 0x15 + 3 * 0x18; // 0x15(flags & sign_id & node_uid) + 3 * 0x18(sign_template_t)

            var tmplTextLength = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
            if (tmplTextLength != 0)
            {
                fileOffset += 0x04 + tmplTextLength; // 0x04(textPadding)
            }
            var signAreaCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04); // 0x04(cursor after tmplTextLength)
            fileOffset += 0x04; // cursor after signAreaCount
            for (var i = 0; i < signAreaCount; i++)
            {
                var subItemCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x0C); // 0x0C(padding)
                fileOffset += 0x04; // cursor after subItemCount
                for (var x = 0; x < subItemCount; x++)
                {
                    var itemType = MemoryHelper.ReadInt16(Sector.Stream, fileOffset);

                    var itemCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x02); // 0x02(itemType)

                    fileOffset += 0x04;// cursor after count

                    if (itemType == 0x05)
                    {
                        var textLength = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
                        fileOffset += 0x04 + 0x04 + textLength; // 0x04(cursor after textlength) + 0x04(padding)
                    }
                    else if (itemType == 0x06)
                    {
                        fileOffset += 0x08 * itemCount; // 0x08(m_stand_id)
                    }
                    else if (itemType == 0x01)
                    {
                        fileOffset += 0x01; // 0x01(padding)
                    }
                    else
                    {
                        fileOffset += 0x04; // 0x04(padding)
                    }
                }
            }
            BlockSize = fileOffset - startOffset;
        }
        public void TsRoadSideItem875(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var templateCount = MemoryHelper.ReadInt8(Sector.Stream, fileOffset += 0x25); // 0x20(flags & sign_id & uid & look & variant)
            fileOffset += 0x01 + templateCount * 0x18; // 0x01(templateCount) + templateCount * 0x18(sign_template_t)

            var tmplTextLength = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
            if (tmplTextLength != 0)
            {
                fileOffset += 0x04 + tmplTextLength; // 0x04(textPadding)
            }
            var signAreaCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04); // 0x04(cursor after tmplTextLength)
            fileOffset += 0x04; // cursor after signAreaCount
            for (var i = 0; i < signAreaCount; i++)
            {
                var subItemCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x0C); // 0x0C(padding)
                fileOffset += 0x04; // cursor after subItemCount
                for (var x = 0; x < subItemCount; x++)
                {
                    var itemType = MemoryHelper.ReadInt16(Sector.Stream, fileOffset);

                    var itemCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x02); // 0x02(itemType)

                    fileOffset += 0x04;// cursor after count

                    if (itemType == 0x05)
                    {
                        var textLength = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
                        fileOffset += 0x04 + 0x04 + textLength; // 0x04(cursor after textlength) + 0x04(padding)
                    }
                    else if (itemType == 0x06)
                    {
                        fileOffset += 0x08 * itemCount; // 0x08(m_stand_id)
                    }
                    else if (itemType == 0x01)
                    {
                        fileOffset += 0x01; // 0x01(padding)
                    }
                    else
                    {
                        fileOffset += 0x04; // 0x04(padding)
                    }
                }
            }
            BlockSize = fileOffset - startOffset;
        }
    }
}
