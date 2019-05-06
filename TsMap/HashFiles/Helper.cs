using System.IO;
using System.Text;

namespace TsMap.HashFiles
{
    internal static class Helper
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

        internal static string CombinePath(string firstPath, string secondPath)
        {
            var fullPath = Path.Combine(firstPath, secondPath);

            fullPath = fullPath.Replace('\\', '/');

            if (fullPath.StartsWith("/")) // absolute path
            {
                fullPath = fullPath.Substring(1);
            }

            return fullPath;
        }

        internal static string GetFilePath(string path, string currentPath = "")
        {
            if (path.StartsWith("/")) // absolute path
            {
                path = path.Substring(1);
                return path;
            }
            return CombinePath(currentPath, path);
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
    }
}
