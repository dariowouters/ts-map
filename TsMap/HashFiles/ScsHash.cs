// Translated to C#
// original source https://ideone.com/gpZrQc (by mwl4)

namespace TsMap.HashFiles
{
    public static class ScsHash
    {
        private static readonly char[] Letters =
        { '\0', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b',
            'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o',
            'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '_'
        };

        private static ulong PowUl(int num)
        {
            ulong res = 1;
            for (var i = 0; num > i; i++)
            {
                res *= (ulong) Letters.Length;
            }

            return res;
        }

        private static int GetIdChar(char letter)
        {
            for (var i = 0; i < Letters.Length; i++)
            {
                if (letter == Letters[i]) return i;
            }

            return 0;
        }

        public static ulong StringToToken(string text)
        {
            ulong res = 0;
            var len = text.Length;
            for (var i = 0; i < len; i++)
            {
                res += PowUl(i) * (ulong) GetIdChar(text.ToLower()[i]);
            }

            return res;
        }
    }
}
