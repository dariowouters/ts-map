using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System;
using TsMap.Canvas;

namespace TsMap.Console
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var mods = new System.Collections.Generic.List<Mod> {
                new Mod(@"D:\Documents\Euro Truck Simulator 2\mod\promods-me-assets-v241.scs"),
                new Mod(@"D:\Documents\Euro Truck Simulator 2\mod\promods-me-defmap-v241.scs"),
                new Mod(@"D:\Documents\Euro Truck Simulator 2\mod\promods-def-st-v241.scs"),
                new Mod(@"D:\Documents\Euro Truck Simulator 2\mod\RusMap-map_v1.8.1.2.scs"),
                new Mod(@"D:\Documents\Euro Truck Simulator 2\mod\RusMap-model2_v1.8.1.2.scs"),
                new Mod(@"D:\Documents\Euro Truck Simulator 2\mod\RusMap-model_v1.8.1.2.scs"),
                new Mod(@"D:\Documents\Euro Truck Simulator 2\mod\promods-assets-v241.scs"),
                new Mod(@"D:\Documents\Euro Truck Simulator 2\mod\promods-map-v241.scs"),
                new Mod(@"D:\Documents\Euro Truck Simulator 2\mod\promods-media-v241.scs"),
                new Mod(@"D:\Documents\Euro Truck Simulator 2\mod\promods-model1-v241.scs"),
                new Mod(@"D:\Documents\Euro Truck Simulator 2\mod\promods-model2-v241.scs"),
                new Mod(@"D:\Documents\Euro Truck Simulator 2\mod\promods-model3-v241.scs"),
                new Mod(@"D:\Documents\Euro Truck Simulator 2\mod\RusMap-def_v1.8.1.2.scs"),
            };
            mods.ForEach(x => x.Load = true);
            TsMapper mapper = new TsMapper("D:/Apps/Steam/steamapps/common/Euro Truck Simulator 2/", mods);
            mapper.IsEts2 = true;
            mapper.Parse();

            var renderer = new TsMapRenderer(mapper);
            var palette = new SimpleMapPalette();

            var path = @"F:/Screenshots";

            int width, height, scale;
            width = height = scale = 4000;

            int upperX = 160_000;// 160_000;
            int lowerX = -120_000;// -120_000;

            int upperY = 130_000;// 130_000;
            int lowerY = -210_000;// -210_000;

            for (int y = lowerY; y < upperY; y += scale * 2)
            {
                for (int x = lowerX; x < upperX; x += scale * 2)
                {
                    var bitmap = new Bitmap(width, height);

                    PointF pos = new PointF(x, y);

                    renderer.Render(Graphics.FromImage(bitmap), new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        scale, pos, palette, RenderFlags.All);

                    using (FileStream fileStream = File.Open($"{path}/{x};{y}.png", FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        bitmap.Save(fileStream, ImageFormat.Png);
                    }
                    bitmap.Dispose();
                    GC.Collect();
                }
            }
        }
    }
}
