using System.Drawing;

namespace TsMap.Canvas
{
    public class SimpleMapPalette : MapPalette
    {
        public SimpleMapPalette()
        {
            Background = new SolidBrush(Color.FromArgb(72, 78, 102));
            Road = Brushes.White;
            PrefabRoad = Brushes.White;
            PrefabLight = new SolidBrush(Color.FromArgb(236, 203, 153));
            PrefabDark = new SolidBrush(Color.FromArgb(225, 163, 56));
            PrefabGreen = new SolidBrush(Color.FromArgb(170, 203, 150)); // TODO: Check if green has a specific z-index

            CityName = Brushes.LightCoral;

            FerryLines = new SolidBrush(Color.FromArgb(80, 255, 255, 255));

            Error = Brushes.LightCoral;

            SpecialColor8 = new SolidBrush(Color.FromArgb(110, 62, 169));
            SpecialColor7 = new SolidBrush(Color.FromArgb(236, 212, 36));
            SpecialColor6 = new SolidBrush(Color.FromArgb(47, 119, 217));
            SpecialColor5 = new SolidBrush(Color.FromArgb(77, 165, 53));
            SpecialColor4 = new SolidBrush(Color.FromArgb(153, 0, 0));
        }
    }
}
