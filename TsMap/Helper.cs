using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TsMap
{

    public static class RenderHelper
    {

        public static PointF RotatePoint(float x, float z, float angle, float rotX, float rotZ)
        {
            var s = Math.Sin(angle);
            var c = Math.Cos(angle);
            double newX = x - rotX;
            double newZ = z - rotZ;
            return new PointF((float)((newX * c) - (newZ * s) + rotX), (float)((newX * s) + (newZ * c) + rotZ));
        }

        public static PointF GetCornerCoords(float x, float z, float width, double angle)
        {
            return new PointF(
                (float)(x + width * Math.Cos(angle)),
                (float)(z + width * Math.Sin(angle))
            );
        }
        public static double Hypotenuse(float x, float y)
        {
            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
        }

        // https://stackoverflow.com/a/45881662
        public static Tuple<PointF, PointF> GetBezierControlNodes(float startX, float startZ, double startRot, float endX, float endZ, double endRot)
        {
            var len = Hypotenuse(endX - startX, endZ - startZ);
            var ax1 = (float)(Math.Cos(startRot) * len * (1 / 3f));
            var az1 = (float)(Math.Sin(startRot) * len * (1 / 3f));
            var ax2 = (float)(Math.Cos(endRot) * len * (1 / 3f));
            var az2 = (float)(Math.Sin(endRot) * len * (1 / 3f));
            return new Tuple<PointF, PointF>(new PointF(ax1, az1), new PointF(ax2, az2));
        }

        public static int GetZoomIndex(Rectangle clip, float scale)
        {
            var smallestSize = (clip.Width > clip.Height) ? clip.Height / scale : clip.Width / scale;
            if (smallestSize < 1000) return 0;
            if (smallestSize < 5000) return 1;
            if (smallestSize < 18500) return 2;
            return 3;
        }

        public static PointF GetPoint(float x, float y)
        {
            return new PointF(x, y);
        }

        public static PointF GetPoint((float, float) p)
        {
            return new PointF(p.Item1, p.Item2);
        }

        public static PointF GetPoint(TsItem.TsItem item)
        {
            return new PointF(item.X, item.Z);
        }

        public static (float, float) ScsMapCorrection(float x, float y)
        {
            if (x < -31100 && x > -60720 && y < -5500 && y > -56300)
            {
                x = (x + 31100f) * 0.75f - 31700f;
                y = (y + 5500f) * 0.75f - 5100f;
            }
            return (x, y);
        }

        public static PointF ScsMapCorrection(PointF p)
        {
            return GetPoint(ScsMapCorrection(p.X, p.Y));
        }

        public static PointF[] ScsMapCorrection(PointF[] p)
        {
            return p.Select(pp => ScsMapCorrection(pp)).ToArray();
        }

        public static List<PointF> ScsMapCorrection(List<PointF> p)
        {
            return p.Select(pp => ScsMapCorrection(pp)).ToList();
        }

    }

    public static class SiiHelper
    {
        private const string CharNotToTrim = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

        public static string Trim(string src)
        {
            var startTrimIndex = 0;
            var endTrimIndex = src.Length;
            for (var i = 0; i < src.Length; i++)
            {
                if (!CharNotToTrim.Contains(src[i].ToString()))
                {
                    startTrimIndex = i + 1;
                }
                else break;
            }

            for (var i = src.Length - 1; i >= 0; i--)
            {
                if (!CharNotToTrim.Contains(src[i].ToString()))
                {
                    endTrimIndex = i;
                }
                else break;
            }

            if (startTrimIndex == src.Length || startTrimIndex >= endTrimIndex) return "";
            return src.Substring(startTrimIndex, endTrimIndex - startTrimIndex);
        }

        public static (bool Valid, string Key, string Value) ParseLine(string line)
        {
            line = Trim(line);
            if (!line.Contains(":") || line.StartsWith("#") || line.StartsWith("//")) return (false, line, line);
            var key = Trim(line.Split(':')[0]);
            var val = line.Split(':')[1];
            if (val.Contains("//"))
            {
                var commentIndex = val.LastIndexOf("//", StringComparison.OrdinalIgnoreCase);
                val = val.Substring(0, commentIndex);
            }

            val = Trim(val);
            return (true, key, val);
        }
    }
}
