using System;
using System.Collections.Generic;
using System.IO;

namespace TsMap
{
    public class TsItem
    {
        private const int StampBlockSize = 0x18;
        private const int VegetationSphereBlockSize = 0x14;
        private const int NodeLookBlockSize = 0x3A;
        private const int PrefabVegetaionBlockSize = 0x20;

        private readonly TsSector _sector;

        public ulong Uid { get; }
        private readonly ulong _startNodeUid;
        private readonly ulong _endNodeUid;

        private TsNode _startNode;
        private TsNode _endNode;

        public List<ulong> Nodes { get; }

        public int BlockSize { get; }

        public bool Valid { get; }

        public TsItemType Type { get; }
        public float X { get; }
        public float Z { get; }
        public bool Hidden { get; }

        public int Origin { get; }

        public TsRoadLook RoadLook { get; }
        public TsPrefab Prefab { get; }
        public string CityName { get; }

        public ulong OverlayId { get; private set; }

        public TsItem(TsSector sector, int offset)
        {
            _sector = sector;
            var startOffset = offset;

            var fileOffset = offset;

            Type = (TsItemType)BitConverter.ToUInt32(_sector.Stream, fileOffset);

            Uid = BitConverter.ToUInt64(_sector.Stream, fileOffset += 0x04);

            X = BitConverter.ToSingle(_sector.Stream, fileOffset += 0x08);
            Z = BitConverter.ToSingle(_sector.Stream, fileOffset += 0x08);

            switch (Type)
            {
                case TsItemType.Road:
                    {
                        Valid = true;
                        fileOffset += 0x20; // Set position at start of flags
                        Hidden = (_sector.Stream[fileOffset += 0x03] & 0x02) != 0;
                        RoadLook = _sector.Mapper.LookupRoadLook(BitConverter.ToUInt64(_sector.Stream, fileOffset += 0x06));
                        if (RoadLook == null)
                        {
                            Valid = false;
                            Log.Msg($"Could not find RoadLook with id: {BitConverter.ToUInt64(_sector.Stream, fileOffset):X}, " +
                                    $"in {Path.GetFileName(_sector.FilePath)} @ {fileOffset}");
                        }
                        _startNodeUid = BitConverter.ToUInt64(_sector.Stream, fileOffset += 0x08 + 0x50);
                        _endNodeUid = BitConverter.ToUInt64(_sector.Stream, fileOffset += 0x08);
                        var stampCount = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x08 + 0x134);
                        var vegetationSphereCount = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x04 + (StampBlockSize * stampCount));
                        fileOffset += 0x04 + (VegetationSphereBlockSize * vegetationSphereCount);
                        BlockSize = fileOffset - startOffset;
                        break;
                    }
                case TsItemType.Prefab:
                    {
                        Valid = true;
                        Nodes = new List<ulong>();
                        fileOffset += 0x20; // Set position at start of flags
                        Hidden = (_sector.Stream[fileOffset += 0x02] & 0x02) != 0;
                        var prefabId = BitConverter.ToUInt64(_sector.Stream, fileOffset += 0x03);
                        Prefab = _sector.Mapper.LookupPrefab(prefabId);
                        if (Prefab == null)
                        {
                            Valid = false;
                            Log.Msg($"Could not find Prefab with id: {BitConverter.ToUInt64(_sector.Stream, fileOffset):X}, " +
                                    $"in {Path.GetFileName(_sector.FilePath)} @ {fileOffset}");
                        }
                        var additionalItemCount = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x08 + 0x10);
                        var nodeCount = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x04 + (0x08 * additionalItemCount));
                        fileOffset += 0x04;
                        for (var i = 0; i < nodeCount; i++)
                        {
                            Nodes.Add(BitConverter.ToUInt64(_sector.Stream, fileOffset));
                            fileOffset += 0x08;
                        }
                        var unkEntityCount = BitConverter.ToInt32(_sector.Stream, fileOffset);
                        Origin = BitConverter.ToChar(_sector.Stream, fileOffset += 0x04 + (0x08 * unkEntityCount) + 0x08);
                        var prefabVegetationCount = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x01 + (NodeLookBlockSize * nodeCount) + 0x01);
                        var vegetationSphereCount = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x04 + (PrefabVegetaionBlockSize * prefabVegetationCount) + 0x04);
                        fileOffset += 0x04 + (VegetationSphereBlockSize * vegetationSphereCount) + (0x18 * nodeCount);
                        BlockSize = fileOffset - startOffset;
                        break;
                    }
                case TsItemType.Company:
                    {
                        Valid = false;
                        fileOffset += 0x20; // Set position at start of flags
                        var count = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x25);
                        count = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x04 + (0x08 * count));
                        count = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x04 + (0x08 * count));
                        count = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x04 + (0x08 * count));
                        count = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x04 + (0x08 * count));
                        fileOffset += 0x04 + (0x08 * count);
                        BlockSize = fileOffset - startOffset;
                        break;
                    }
                case TsItemType.Service:
                    {
                        Valid = false;
                        fileOffset += 0x20 + 0x15;
                        BlockSize = fileOffset - startOffset;
                        break;
                    }
                case TsItemType.CutPlane:
                    {
                        Valid = false;
                        fileOffset += 0x20; // Set position at start of flags
                        var nodeCount = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x05);
                        fileOffset += 0x04 + (0x08 * nodeCount);
                        BlockSize = fileOffset - startOffset;
                        break;
                    }
                case TsItemType.City: // TODO: Add zoom levels/range to show city names and icons correctly
                    {
                        Valid = true;
                        fileOffset += 0x20;
                        Hidden = (_sector.Stream[fileOffset] & 0x01) != 0;
                        var cityId = BitConverter.ToUInt64(_sector.Stream, fileOffset += 0x05);
                        CityName = _sector.Mapper.LookupCity(cityId);
                        if (CityName == null)
                        {
                            Valid = false;
                            Log.Msg($"Could not find City with id: {BitConverter.ToUInt64(_sector.Stream, fileOffset):X}, " +
                                    $"in {Path.GetFileName(_sector.FilePath)} @ {fileOffset}");
                        }
                        fileOffset += 0x08 + 0x10;
                        BlockSize = fileOffset - startOffset;
                        break;
                    }
                case TsItemType.MapOverlay: // TODO: Figure out gas station / service station icons
                    {
                        Valid = true;
                        fileOffset += 0x20; // Set position at start of flags
                        var zoomLevelVisibility = (byte)BitConverter.ToChar(_sector.Stream, fileOffset);
                        OverlayId = BitConverter.ToUInt64(_sector.Stream, fileOffset += 0x05);
                        fileOffset += 0x08 + 0x08;
                        BlockSize = fileOffset - startOffset;
                        break;
                    }
                case TsItemType.Ferry: // TODO: Draw ferry lines
                    {
                        Valid = false;
                        fileOffset += 0x20; // Set position at start of flags
                        var ferryId = BitConverter.ToUInt64(_sector.Stream, fileOffset += 0x05);
                        fileOffset += 0x08 + 0x1C;
                        BlockSize = fileOffset - startOffset;
                        break;
                    }
                case TsItemType.Garage:
                    {
                        Valid = false;
                        fileOffset += 0x20 + 0x05 + 0x1C;
                        BlockSize = fileOffset - startOffset;
                        break;
                    }
                case TsItemType.Trigger:
                    {
                        Valid = false;
                        fileOffset += 0x20; // Set position at start of flags
                        var tagCount = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x05);
                        var nodeCount = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x04 + (0x08 * tagCount));
                        var triggerActionCount = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x04 + (0x08 * nodeCount));
                        fileOffset += 0x04;

                        for (var i = 0; i < triggerActionCount; i++)
                        {
                            var isCustom = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x08);
                            if (isCustom > 0) fileOffset += 0x04;
                            var hasText = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x04);
                            if (hasText > 0)
                            {
                                var textLength = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x04);
                                fileOffset += 0x04 + textLength;
                            }
                            fileOffset += 0x04 + 0x0C;
                        }

                        fileOffset += 0x18;
                        BlockSize = fileOffset - startOffset;
                        break;
                    }
                case TsItemType.FuelPump:
                    {
                        Valid = false;
                        fileOffset += 0x20 + 0x05 + 0x10;
                        BlockSize = fileOffset - startOffset;
                        break;
                    }
                case TsItemType.RoadSideItem: // Lights, signs, ...
                    {
                        Valid = false;
                        fileOffset += 0x20; // Set position at start of flags
                        var tmplTextLength = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x05 + 0x58);
                        if (tmplTextLength != 0)
                        {
                            fileOffset += 0x04 + tmplTextLength;
                        }
                        var count = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x04);
                        fileOffset += 0x04;
                        for (var i = 0; i < count; i++)
                        {
                            var subItemCount = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x0C);
                            fileOffset += 0x04;
                            for (var x = 0; x < subItemCount; x++)
                            {
                                var itemType = BitConverter.ToInt16(_sector.Stream, fileOffset);
                                fileOffset += 0x02 + 0x04;
                                if (itemType == 0x05)
                                {
                                    var textLength = BitConverter.ToInt32(_sector.Stream, fileOffset);
                                    fileOffset += 0x04 + 0x04 + textLength;
                                }
                                else if (itemType == 0x01)
                                {
                                    fileOffset += 0x01;
                                }
                                else
                                {
                                    fileOffset += 0x04;
                                }
                            }
                        }
                        BlockSize = fileOffset - startOffset;
                        break;
                    }
                case TsItemType.BusStop:
                    {
                        Valid = false;
                        fileOffset += 0x20; // Set position at start of flags
                        var someValue = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x05);
                        fileOffset += 0x04 + 0x14;
                        // if (someValue != 0) fileOffset += 0x04;
                        BlockSize = fileOffset - startOffset;
                        break;
                    }
                case TsItemType.TrafficRule:
                    {
                        Valid = false;
                        fileOffset += 0x20; // Set position at start of flags
                        var count = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x05);
                        count = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x04 + (0x08 * count));
                        count = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x04 + (0x08 * count));
                        fileOffset += 0x04 + 0x08;
                        BlockSize = fileOffset - startOffset;
                        break;
                    }
                case TsItemType.TrajectoryItem:
                    {
                        Valid = false;
                        fileOffset += 0x20; // Set position at start of flags
                        var nodeCount = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x05);
                        var ruleCount = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x04 + (0x08 * nodeCount) + 0x0C);
                        var checkPointCount = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x04 + (0x1C * ruleCount));
                        fileOffset += 0x04 + (0x10 * checkPointCount) + 0x04;
                        BlockSize = fileOffset - startOffset;
                        break;
                    }
                case TsItemType.MapArea:
                    {
                        Valid = false;
                        fileOffset += 0x20; // Set position at start of flags
                        var nodeCount = BitConverter.ToInt32(_sector.Stream, fileOffset += 0x05);
                        fileOffset += 0x04 + (0x08 * nodeCount) + 0x04;
                        BlockSize = fileOffset - startOffset;
                        break;
                    }
                default:
                    Log.Msg($"Unknown Item Type: {Type} in {_sector.FilePath} @ {startOffset:X}");
                    Valid = false;
                    break;
            }

        }

        public TsNode GetStartNode()
        {
            return _startNode ?? (_startNode = _sector.Mapper.GetNodeByUid(_startNodeUid));
        }

        public TsNode GetEndNode()
        {
            return _endNode ?? (_endNode = _sector.Mapper.GetNodeByUid(_endNodeUid));
        }
    }
}
