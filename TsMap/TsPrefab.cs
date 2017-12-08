using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace TsMap
{
    public struct TsPrefabNode
    {
        public float X;
        public float Z;
        public float RotX;
        public float RotZ;
    }

    public struct TsMapPoint
    {
        public float X;
        public float Z;
        public int LaneOffset;
        public int LaneCount;
        public bool Hidden;
        public byte PrefabColorFlags;
        public int NeighbourCount;
        public List<int> Neighbours;
    }

    public class TsPrefab
    {
        private const int NodeBlockSize = 0x68;
        private const int MapPointBlockSize = 0x30;

        private string _filePath;

        private byte[] _stream;

        public List<TsPrefabNode> PrefabNodes { get; private set; }
        public List<TsMapPoint> MapPoints { get; private set; }

        public TsPrefab(string filePath)
        {
            _filePath = filePath;

            if (!File.Exists(filePath)) return;
            _stream = File.ReadAllBytes(filePath);
            Parse();
        }

        private void Parse()
        {
            PrefabNodes = new List<TsPrefabNode>();
            MapPoints = new List<TsMapPoint>();

            var fileOffset = 0x0;

            var version = BitConverter.ToInt32(_stream, fileOffset);

            var nodeCount = BitConverter.ToInt32(_stream, fileOffset += 0x04);
            var mapPointCount = BitConverter.ToInt32(_stream, fileOffset += 0x1C);

            if (version > 0x15) fileOffset += 0x04;

            var nodeOffset = BitConverter.ToInt32(_stream, fileOffset += 0x0C);
            var mapPointOffset = BitConverter.ToInt32(_stream, fileOffset + 0x20);

            for (var i = 0; i < nodeCount; i++)
            {
                var nodeBaseOffset = nodeOffset + (i * NodeBlockSize);
                var node = new TsPrefabNode
                {
                    X = BitConverter.ToSingle(_stream, nodeBaseOffset + 0x10),
                    Z = BitConverter.ToSingle(_stream, nodeBaseOffset + 0x18),
                    RotX = BitConverter.ToSingle(_stream, nodeBaseOffset + 0x1C),
                    RotZ = BitConverter.ToSingle(_stream, nodeBaseOffset + 0x24),
                };
                PrefabNodes.Add(node);
            }

            for (var i = 0; i < mapPointCount; i++)
            {
                var mapPointBaseOffset = mapPointOffset + (i * MapPointBlockSize);
                var roadLookFlags = BitConverter.ToChar(_stream, mapPointBaseOffset + 0x01);
                var laneTypeFlags = (byte) (roadLookFlags & 0x0F);
                var laneOffsetFlags = (byte)(roadLookFlags >> 4);
                int laneOffset;
                switch (laneOffsetFlags)
                {
                    case 1: laneOffset = 1; break;
                    case 2: laneOffset = 2; break;
                    case 3: laneOffset = 5; break;
                    case 4: laneOffset = 10; break;
                    case 5: laneOffset = 15; break;
                    case 6: laneOffset = 20; break;
                    case 7: laneOffset = 25; break;
                    default: laneOffset = 1; break;

                }
                int laneCount;
                switch (laneTypeFlags) // TODO: Change these (not really used atm)
                {
                    case 1: laneCount = 2; break;
                    case 2: laneCount = 4; break;
                    case 3: laneCount = 6; break;
                    case 4: laneCount = 8; break;
                    case 5: laneCount = 5; break;
                    case 6: laneCount = 7; break;
                    case 8: laneCount = 3; break;
                    case 13: laneCount = -1; break;
                    case 14: laneCount = 0; break;
                    default:
                        laneCount = 0;
                        // Log.Msg($"Unknown LaneType: {laneTypeFlags}");
                        break;
                }

                var prefabColorFlags = (byte)BitConverter.ToChar(_stream, mapPointBaseOffset + 0x02);

                var navFlags = (byte)BitConverter.ToChar(_stream, mapPointBaseOffset + 0x05);
                var hidden = (navFlags & 0x02) != 0; // Map Point is Control Node

                var point = new TsMapPoint
                {
                    LaneCount = laneCount,
                    LaneOffset = laneOffset,
                    Hidden = hidden,
                    PrefabColorFlags = prefabColorFlags,
                    X = BitConverter.ToSingle(_stream, mapPointBaseOffset + 0x08),
                    Z = BitConverter.ToSingle(_stream, mapPointBaseOffset + 0x10),
                    Neighbours = new List<int>(),
                    NeighbourCount = BitConverter.ToInt32(_stream, mapPointBaseOffset + 0x14 + (0x04 * 6))
                };

                for (var x = 0; x < point.NeighbourCount; x++)
                {
                    point.Neighbours.Add(BitConverter.ToInt32(_stream, mapPointBaseOffset + 0x14 + (x * 0x04)));
                }

                MapPoints.Add(point);
            }

            _stream = null;

        }
    }
}
