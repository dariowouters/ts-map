using System;

namespace TsMap
{
    internal static class MemoryHelper
    {
        internal static byte ReadUint8(byte[] s, int pos)
        {
            return s[pos];
        }
        internal static sbyte ReadInt8(byte[] s, int pos)
        {
            return (sbyte)s[pos];
        }

        internal static unsafe ushort ReadUInt16(byte[] s, int pos)
        {
            fixed (byte* p = &s[0])
            {
                return *(ushort*)(p + pos);
            }
        }
        internal static unsafe short ReadInt16(byte[] s, int pos)
        {
            fixed (byte* p = &s[0])
            {
                return *(short*)(p + pos);
            }
        }

        internal static unsafe uint ReadUInt32(byte[] s, int pos)
        {
            fixed (byte* p = &s[0])
            {
                return *(uint*)(p + pos);
            }
        }
        internal static unsafe int ReadInt32(byte[] s, int pos)
        {
            fixed (byte* p = &s[0])
            {
                return *(int*)(p + pos);
            }
        }

        internal static unsafe float ReadSingle(byte[] s, int pos)
        {
            fixed (byte* p = &s[0])
            {
                return *(float*)(p + pos);
            }
        }

        internal static unsafe ulong ReadUInt64(byte[] s, int pos)
        {
            fixed (byte* p = &s[0])
            {
                return *(ulong*)(p + pos);
            }
        }
        internal static unsafe long ReadInt64(byte[] s, int pos)
        {
            fixed (byte* p = &s[0])
            {
                return *(long*)(p + pos);
            }
        }
    }
}
