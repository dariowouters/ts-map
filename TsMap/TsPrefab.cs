using System;
using System.Collections.Generic;
using TsMap.FileSystem;
using TsMap.Helpers;
using TsMap.Helpers.Logger;

namespace TsMap
{
    public struct TsPrefabNode
    {
        public float X;
        public float Y;
        public float Z;
        public float RotX;
        public float RotZ;
        public int LaneCount;
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
        public sbyte ControlNodeIndex;
    }

    public class TsSpawnPoint
    {
        public float X;
        public float Y;
        public float Z;
        public TsSpawnPointType Type;
    }

    public class TsTriggerPoint
    {
        public uint TriggerId;
        public ulong TriggerActionToken;
        public float X;
        public float Y;
        public float Z;
    }

    public struct TsPrefabCurve
    {
        public float StartX;
        public float StartY;
        public float StartZ;
        public float EndX;
        public float EndY;
        public float EndZ;
        public float Lenght;
        public uint NavigationNodeIndex;
    }

    public struct TsNavNode
    {
        public TsNavNodeType Type;
        public ushort ReferenceIndex;
        public uint ConnectionsCount;
        public List<TsNavConnection> Connections;
    }

    public struct TsNavConnection
    {
        public ushort TargetNodeIndex;
        public float Length;
        public uint CurvesCount;
        public List<TsPrefabCurve> CurvePath;
    }

    public class TsPrefab
    {
        private const int NodeBlockSize = 0x68;
        private const int CurveBlockSize = 0x84;
        private const int MapPointBlockSize = 0x30;
        private const int SpawnPointBlockSize = 0x20;
        private const int TriggerPointBlockSize = 0x30;
        private const int NavNodeBlockSize = 0xBC;
        private const int NavConnectionBlockSize = 0x17;

        public string FilePath { get; }
        public ulong Token { get; }
        public string Category { get; }

        private byte[] _stream;

        public bool ValidRoad { get; private set; }

        public List<TsPrefabNode> PrefabNodes { get; private set; }
        public List<TsPrefabCurve> PrefabCurves { get; private set; }
        public List<TsSpawnPoint> SpawnPoints { get; private set; }
        public List<TsMapPoint> MapPoints { get; private set; }
        public List<TsTriggerPoint> TriggerPoints { get; private set; }
        public List<TsNavNode> NavNodes { get; private set; }

        public TsPrefab(string filePath, ulong token, string category)
        {
            FilePath = filePath;
            Token = token;
            Category = category;

            var file = UberFileSystem.Instance.GetFile(FilePath);

            if (file == null) return;

            _stream = file.Entry.Read();

            Parse();
        }

        private void Parse()
        {
            PrefabNodes = new List<TsPrefabNode>();
            PrefabCurves = new List<TsPrefabCurve>();
            SpawnPoints = new List<TsSpawnPoint>();
            MapPoints = new List<TsMapPoint>();
            TriggerPoints = new List<TsTriggerPoint>();
            NavNodes = new List<TsNavNode>();

            var fileOffset = 0x0;

            var version = MemoryHelper.ReadInt32(_stream, fileOffset);

            if (version < 0x15)
            {
                Logger.Instance.Error($"{FilePath} file version ({version}) too low, min. is {0x15}");
                return;
            }

            var nodeCount = BitConverter.ToInt32(_stream, fileOffset += 0x04);
            var navCurveCount = BitConverter.ToInt32(_stream, fileOffset += 0x04);
            ValidRoad = navCurveCount != 0;
            var spawnPointCount = BitConverter.ToInt32(_stream, fileOffset += 0x0C);
            var mapPointCount = BitConverter.ToInt32(_stream, fileOffset += 0x0C);
            var triggerPointCount = BitConverter.ToInt32(_stream, fileOffset += 0x04);
            var navNodesCount = BitConverter.ToInt32(_stream, fileOffset += 0x08);

            var nodeOffset = MemoryHelper.ReadInt32(_stream, fileOffset += 0x04);
            var curveOffset = MemoryHelper.ReadInt32(_stream, fileOffset += 0x04);
            var spawnPointOffset = MemoryHelper.ReadInt32(_stream, fileOffset += 0x0C);
            var mapPointOffset = MemoryHelper.ReadInt32(_stream, fileOffset += 0x10);
            var triggerPointOffset = MemoryHelper.ReadInt32(_stream, fileOffset += 0x04);
            var navNodeOffset = MemoryHelper.ReadInt32(_stream, fileOffset += 0x08);

            for (var i = 0; i < nodeCount; i++)
            {
                var nodeBaseOffset = nodeOffset + (i * NodeBlockSize);
                var node = new TsPrefabNode
                {
                    X = MemoryHelper.ReadSingle(_stream, nodeBaseOffset + 0x10),
                    Y = MemoryHelper.ReadSingle(_stream, nodeBaseOffset + 0x14),
                    Z = MemoryHelper.ReadSingle(_stream, nodeBaseOffset + 0x18),
                    RotX = MemoryHelper.ReadSingle(_stream, nodeBaseOffset + 0x1C),
                    RotZ = MemoryHelper.ReadSingle(_stream, nodeBaseOffset + 0x24),
                };

                int laneCount = 0;
                var nodeFileOffset = nodeBaseOffset + 0x24;
                for (var j = 0; j < 8; j++)
                {
                    if (MemoryHelper.ReadInt32(_stream, nodeFileOffset += 0x04) != -1) laneCount++;
                }

                for (var j = 0; j < 8; j++)
                {
                    if (MemoryHelper.ReadInt32(_stream, nodeFileOffset += 0x04) != -1) laneCount++;
                }
                node.LaneCount = laneCount;

                PrefabNodes.Add(node);
            }

            for (var i = 0; i < navCurveCount; i++)
            {
                var curveBaseOffset = curveOffset + (i * CurveBlockSize);
                var curve = new TsPrefabCurve
                {
                    StartX = MemoryHelper.ReadSingle(_stream, curveBaseOffset + 0x10),
                    StartY = MemoryHelper.ReadSingle(_stream, curveBaseOffset + 0x14),
                    StartZ = MemoryHelper.ReadSingle(_stream, curveBaseOffset + 0x18),
                    EndX = MemoryHelper.ReadSingle(_stream, curveBaseOffset + 0x1C),
                    EndY = MemoryHelper.ReadSingle(_stream, curveBaseOffset + 0x20),
                    EndZ = MemoryHelper.ReadSingle(_stream, curveBaseOffset + 0x24),
                    Lenght = MemoryHelper.ReadSingle(_stream, curveBaseOffset + 0x48),
                    NavigationNodeIndex = MemoryHelper.ReadUInt32(_stream, curveBaseOffset + 0x80)
                };
                PrefabCurves.Add(curve);
            }

            for (var i = 0; i < spawnPointCount; i++)
            {
                var spawnPointBaseOffset = spawnPointOffset + (i * SpawnPointBlockSize);
                var spawnPoint = new TsSpawnPoint
                {
                    X = MemoryHelper.ReadSingle(_stream, spawnPointBaseOffset),
                    Y = MemoryHelper.ReadSingle(_stream, spawnPointBaseOffset + 0x04),
                    Z = MemoryHelper.ReadSingle(_stream, spawnPointBaseOffset + 0x08),
                    Type = (TsSpawnPointType)MemoryHelper.ReadUInt32(_stream, spawnPointBaseOffset + 0x1C)
                };
                SpawnPoints.Add(spawnPoint);
                // Log.Msg($"Spawn point of type: {spawnPoint.Type} in {_filePath}");
            }

            for (var i = 0; i < mapPointCount; i++)
            {
                var mapPointBaseOffset = mapPointOffset + (i * MapPointBlockSize);
                var roadLookFlags = MemoryHelper.ReadUint8(_stream, mapPointBaseOffset + 0x01);
                var laneTypeFlags = (byte)(roadLookFlags & 0x0F);
                var laneOffsetFlags = (byte)(roadLookFlags >> 4);
                var controlNodeIndexFlags = MemoryHelper.ReadInt8(_stream, mapPointBaseOffset + 0x04);
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
                    default: laneOffset = 0; break;

                }
                int laneCount;
                switch (laneTypeFlags) // TODO: Change these (not really used atm)
                {
                    case 0: laneCount = 1; break;
                    case 1: laneCount = 2; break;
                    case 2: laneCount = 4; break;
                    case 3: laneCount = 6; break;
                    case 4: laneCount = 8; break;
                    case 5: laneCount = 5; break;
                    case 6: laneCount = 7; break;
                    case 8: laneCount = 3; break;
                    case 13: laneCount = -1; break;
                    case 14: laneCount = -2; break; // auto
                    default:
                        laneCount = 1;
                        // Log.Msg($"Unknown LaneType: {laneTypeFlags}");
                        break;
                }
                sbyte controlNodeIndex = -1;
                switch (controlNodeIndexFlags)
                {
                    case 1: controlNodeIndex = 0; break;
                    case 2: controlNodeIndex = 1; break;
                    case 4: controlNodeIndex = 2; break;
                    case 8: controlNodeIndex = 3; break;
                    case 16: controlNodeIndex = 4; break;
                    case 32: controlNodeIndex = 5; break;
                }
                var prefabColorFlags = MemoryHelper.ReadUint8(_stream, mapPointBaseOffset + 0x02);

                var navFlags = MemoryHelper.ReadUint8(_stream, mapPointBaseOffset + 0x05);
                var hidden = (navFlags & 0x02) != 0; // Map Point is Control Node

                var point = new TsMapPoint
                {
                    LaneCount = laneCount,
                    LaneOffset = laneOffset,
                    Hidden = hidden,
                    PrefabColorFlags = prefabColorFlags,
                    X = MemoryHelper.ReadSingle(_stream, mapPointBaseOffset + 0x08),
                    Z = MemoryHelper.ReadSingle(_stream, mapPointBaseOffset + 0x10),
                    Neighbours = new List<int>(),
                    NeighbourCount = MemoryHelper.ReadInt32(_stream, mapPointBaseOffset + 0x14 + (0x04 * 6)),
                    ControlNodeIndex = controlNodeIndex
                };

                for (var x = 0; x < point.NeighbourCount; x++)
                {
                    point.Neighbours.Add(MemoryHelper.ReadInt32(_stream, mapPointBaseOffset + 0x14 + (x * 0x04)));
                }

                MapPoints.Add(point);
            }

            for (var i = 0; i < triggerPointCount; i++)
            {
                var triggerPointBaseOffset = triggerPointOffset + (i * TriggerPointBlockSize);
                var triggerPoint = new TsTriggerPoint
                {
                    TriggerId = MemoryHelper.ReadUInt32(_stream, triggerPointBaseOffset),
                    TriggerActionToken = MemoryHelper.ReadUInt64(_stream, triggerPointBaseOffset + 0x04),
                    X = MemoryHelper.ReadSingle(_stream, triggerPointBaseOffset + 0x1C),
                    Y = MemoryHelper.ReadSingle(_stream, triggerPointBaseOffset + 0x20),
                    Z = MemoryHelper.ReadSingle(_stream, triggerPointBaseOffset + 0x24),
                };
                TriggerPoints.Add(triggerPoint);
            }

            if (version > 22)
            {
                for (var i = 0; i < navNodesCount; i++)
                {
                    var navNodeBaseOffset = navNodeOffset + (i * NavNodeBlockSize);
                    var navNode = new TsNavNode
                    {
                        Type = (MemoryHelper.ReadUint8(_stream, navNodeBaseOffset) == 0) ? TsNavNodeType.BorderNode : TsNavNodeType.AINode,
                        ReferenceIndex = MemoryHelper.ReadUInt16(_stream, navNodeBaseOffset + 0x01),
                        ConnectionsCount = MemoryHelper.ReadUint8(_stream, navNodeBaseOffset + 0x03),
                        Connections = new List<TsNavConnection>()
                    };
                    navNodeBaseOffset += 0x04;

                    for (var j = 0; j < navNode.ConnectionsCount && navNode.ConnectionsCount != 255; j++)
                    {
                        var navConnectionBaseOffset = navNodeBaseOffset + (j * NavConnectionBlockSize);

                        var navConnection = new TsNavConnection
                        {
                            TargetNodeIndex = MemoryHelper.ReadUInt16(_stream, navConnectionBaseOffset),
                            Length = MemoryHelper.ReadSingle(_stream, navConnectionBaseOffset + 0x02),
                            CurvesCount = MemoryHelper.ReadUint8(_stream, navConnectionBaseOffset + 0x06),
                            CurvePath = new List<TsPrefabCurve>()
                        };
                        navConnectionBaseOffset += 0x07;

                        for (var k = 0; k < navConnection.CurvesCount && navConnection.CurvesCount != 255; k++)
                        {
                            var index = MemoryHelper.ReadUInt16(_stream, navConnectionBaseOffset + (k * 0x02));
                            navConnection.CurvePath.Add(PrefabCurves[index]);
                        }
                        navNode.Connections.Add(navConnection);
                    }
                    NavNodes.Add(navNode);
                }
            }

            _stream = null;

        }
    }
}
