using System;
using System.IO;

namespace TsMap.HashFiles
{
    internal static class Helper
    {
        internal static ushort ReadUInt16(BinaryReader br, long offset)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            return br.ReadUInt16();
        }

        internal static uint ReadUInt32(BinaryReader br, long offset)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            return br.ReadUInt32();
        }
        internal static int ReadInt32(BinaryReader br, long offset)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            return br.ReadInt32();
        }

        internal static ulong ReadUInt64(BinaryReader br, long offset)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            return br.ReadUInt64();
        }
        internal static long ReadInt64(BinaryReader br, long offset)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            return br.ReadInt64();
        }

        internal static string CombinePath(string firstPath, string secondPath)
        {
            var fullPath = Path.Combine(firstPath, secondPath);
            return fullPath.Replace('\\', '/');
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
    }
}
