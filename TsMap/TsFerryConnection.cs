using System.Collections.Generic;
using System.Drawing;

namespace TsMap
{


    public class TsFerryConnection
    {
        public ulong StartPortToken { get; set; }
        public PointF StartPortLocation { get; private set; }
        public ulong EndPortToken { get; set; }
        public PointF EndPortLocation { get; private set; }
        public List<PointF> connections = new List<PointF>();

        public string StartId { get; set; }
        public string EndId { get; set; }

        public void AddConnectionPosition(float x, float z)
        {
            connections.Add(new PointF(x / 256f, z / 256f));
        }

        public void SetPortLocation(ulong ferryPortId, float x, float z)
        {
            if (ferryPortId == StartPortToken)
            {
                StartPortLocation = new PointF(x, z);
            }
            else if (ferryPortId == EndPortToken)
            {
                EndPortLocation = new PointF(x, z);
            }
        }
    }
}
