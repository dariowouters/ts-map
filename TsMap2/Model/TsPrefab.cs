using System.Collections.Generic;
using TsMap2.Scs;

namespace TsMap2.Model {
    public struct TsPrefabNode {
        public float X;
        public float Z;
        public float RotX;
        public float RotZ;
        public int   LaneCount;
    }

    public struct TsMapPoint {
        public float       X;
        public float       Z;
        public int         LaneOffset;
        public int         LaneCount;
        public bool        Hidden;
        public byte        PrefabColorFlags;
        public int         NeighbourCount;
        public List< int > Neighbours;
        public sbyte       ControlNodeIndex;
    }

    public class TsSpawnPoint {
        public TsSpawnPointType Type;
        public float            X;
        public float            Z;
    }

    public class TsTriggerPoint {
        public ulong TriggerActionToken;
        public uint  TriggerId;
        public float X;
        public float Z;
    }

    public class TsPrefab {
        public const int NodeBlockSize         = 0x68;
        public const int MapPointBlockSize     = 0x30;
        public const int SpawnPointBlockSize   = 0x20;
        public const int TriggerPointBlockSize = 0x30;

        public TsPrefab( ulong                  token,
                         string                 category,
                         bool                   validRoad,
                         List< TsPrefabNode >   prefabNodes,
                         List< TsSpawnPoint >   spawnPoints,
                         List< TsMapPoint >     mapPoints,
                         List< TsTriggerPoint > triggerPoints ) {
            Token         = token;
            Category      = category;
            ValidRoad     = validRoad;
            PrefabNodes   = prefabNodes;
            SpawnPoints   = spawnPoints;
            MapPoints     = mapPoints;
            TriggerPoints = triggerPoints;
        }

        public ulong                  Token         { get; }
        public string                 Category      { get; }
        public bool                   ValidRoad     { get; }
        public List< TsPrefabNode >   PrefabNodes   { get; }
        public List< TsSpawnPoint >   SpawnPoints   { get; }
        public List< TsMapPoint >     MapPoints     { get; }
        public List< TsTriggerPoint > TriggerPoints { get; }
    }
}