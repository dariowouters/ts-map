using System.IO;
using System.Text;

namespace TsMap.Helpers
{
    internal class MemoryHelper
    {
        internal static ushort ReadUInt16(BinaryReader br, long offset, SeekOrigin so = SeekOrigin.Begin)
        {
            br.BaseStream.Seek(offset, so);
            return br.ReadUInt16();
        }

        internal static uint ReadUInt32(BinaryReader br, long offset, SeekOrigin so = SeekOrigin.Begin)
        {
            br.BaseStream.Seek(offset, so);
            return br.ReadUInt32();
        }
        internal static int ReadInt32(BinaryReader br, long offset, SeekOrigin so = SeekOrigin.Begin)
        {
            br.BaseStream.Seek(offset, so);
            return br.ReadInt32();
        }

        internal static ulong ReadUInt64(BinaryReader br, long offset, SeekOrigin so = SeekOrigin.Begin)
        {
            br.BaseStream.Seek(offset, so);
            return br.ReadUInt64();
        }
        internal static long ReadInt64(BinaryReader br, long offset, SeekOrigin so = SeekOrigin.Begin)
        {
            br.BaseStream.Seek(offset, so);
            return br.ReadInt64();
        }

        internal static byte[] ReadBytes(BinaryReader br, long offset, int length, SeekOrigin so = SeekOrigin.Begin)
        {
            br.BaseStream.Seek(offset, so);
            return br.ReadBytes(length);
        }

        internal static string ReadString(BinaryReader br, long offset, int length, SeekOrigin so = SeekOrigin.Begin)
        {
            return Encoding.UTF8.GetString(ReadBytes(br, offset, length, so));
        }

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

        internal static string Decrypt3Nk(byte[] src) // from quickbms scsgames.bms script
        {
            if (src.Length < 0x05 || src[0] != 0x33 && src[1] != 0x6E && src[2] != 0x4B) return null;
            var decrypted = new byte[src.Length - 6];
            var key = src[5];

            for (var i = 6; i < src.Length; i++)
            {
                decrypted[i - 6] = (byte)(((((key << 2) ^ (key ^ 0xff)) << 3) ^ key) ^ src[i]);
                key++;
            }
            return Encoding.UTF8.GetString(decrypted);
        }

        internal static bool IsBitSet(uint flags, byte pos)
        {
            return (flags & (1 << pos)) != 0;
        }
    }
}
