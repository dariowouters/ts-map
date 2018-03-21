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
        }
    }
}
