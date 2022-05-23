using System;
using System.IO;
using TsMap.FileSystem;
using TsMap.TsItem;
using TsMap.Helpers;
using TsMap.Helpers.Logger;

namespace TsMap
{
    public class TsSector
    {
        public string FilePath { get; }
        public TsMapper Mapper { get; }


        public int Version { get; private set; }
        private bool _empty;

        public byte[] Stream { get; private set; }

        private readonly UberFile _file;

        public TsSector(TsMapper mapper, string filePath)
        {
            Mapper = mapper;
            FilePath = filePath;
            _file = UberFileSystem.Instance.GetFile(FilePath);
            if (_file == null)
            {
                _empty = true;
                return;
            }

            Stream = _file.Entry.Read();
        }

        public void Parse()
        {
            Version = BitConverter.ToInt32(Stream, 0x0);

            if (Version < 825)
            {
                Logger.Instance.Error($"{FilePath} version ({Version}) is too low, min. is 825");
                return;
            }

            var itemCount = BitConverter.ToUInt32(Stream, 0x10);
            if (itemCount == 0) _empty = true;
            if (_empty) return;

            var lastOffset = 0x14;

            for (var i = 0; i < itemCount; i++)
            {
                var type = (TsItemType)MemoryHelper.ReadUInt32(Stream, lastOffset);
                if (Version <= 825) type++; // after version 825 all types were pushed up 1

                switch (type)
                {
                    case TsItemType.Road:
                    {
                        var item = new TsRoadItem(this, lastOffset);
                        lastOffset += item.BlockSize;
                        if (item.Valid) Mapper.Roads.Add(item);
                        break;
                    }
                    case TsItemType.Prefab:
                    {
                        var item = new TsPrefabItem(this, lastOffset);
                        lastOffset += item.BlockSize;
                        if (item.Valid) Mapper.Prefabs.Add(item);
                        break;
                    }
                    case TsItemType.Company:
                    {
                        var item = new TsCompanyItem(this, lastOffset);
                        lastOffset += item.BlockSize;
                        if (item.Valid) Mapper.Companies.Add(item);
                        break;
                    }
                    case TsItemType.Service:
                    {
                        var item = new TsServiceItem(this, lastOffset);
                        lastOffset += item.BlockSize;
                        break;
                    }
                    case TsItemType.CutPlane:
                    {
                        var item = new TsCutPlaneItem(this, lastOffset);
                        lastOffset += item.BlockSize;
                        break;
                    }
                    case TsItemType.City:
                    {
                        var item = new TsCityItem(this, lastOffset);
                        lastOffset += item.BlockSize;
                        if (item.Valid) Mapper.Cities.Add(item); break;
                    }
                    case TsItemType.MapOverlay:
                    {
                        var item = new TsMapOverlayItem(this, lastOffset);
                        lastOffset += item.BlockSize;
                        if (item.Valid) Mapper.MapOverlays.Add(item); break;
                    }
                    case TsItemType.Ferry:
                    {
                        var item = new TsFerryItem(this, lastOffset);
                        lastOffset += item.BlockSize;
                        if (item.Valid) Mapper.FerryConnections.Add(item); break;
                    }
                    case TsItemType.Garage:
                    {
                        var item = new TsGarageItem(this, lastOffset);
                        lastOffset += item.BlockSize;
                        break;
                    }
                    case TsItemType.Trigger:
                    {
                        var item = new TsTriggerItem(this, lastOffset);
                        lastOffset += item.BlockSize;
                        if (item.Valid) Mapper.Triggers.Add(item);
                        break;
                    }
                    case TsItemType.FuelPump:
                    {
                        var item = new TsFuelPumpItem(this, lastOffset);
                        lastOffset += item.BlockSize;
                        break;
                    }
                    case TsItemType.RoadSideItem:
                    {
                        var item = new TsRoadSideItem(this, lastOffset);
                        lastOffset += item.BlockSize;
                        break;
                    }
                    case TsItemType.BusStop:
                    {
                        var item = new TsBusStopItem(this, lastOffset);
                        lastOffset += item.BlockSize;
                        break;
                    }
                    case TsItemType.TrafficRule:
                    {
                        var item = new TsTrafficRuleItem(this, lastOffset);
                        lastOffset += item.BlockSize;
                        break;
                    }
                    case TsItemType.TrajectoryItem:
                    {
                        var item = new TsTrajectoryItem(this, lastOffset);
                        lastOffset += item.BlockSize;
                        break;
                    }
                    case TsItemType.MapArea:
                    {
                        var item = new TsMapAreaItem(this, lastOffset);
                        lastOffset += item.BlockSize;
                        if (item.Valid) Mapper.MapAreas.Add(item);
                        break;
                    }
                    case TsItemType.Cutscene:
                    {
                        var item = new TsCutsceneItem(this, lastOffset);
                        lastOffset += item.BlockSize;
                        if (item.Valid) Mapper.Viewpoints.Add(item);
                        break;
                    }
                    case TsItemType.VisibilityArea:
                    {
                        var item = new TsVisibilityAreaItem(this, lastOffset);
                        lastOffset += item.BlockSize;
                        break;
                    }
                    default:
                    {
                        Logger.Instance.Warning($"Unknown Type: {type} in {Path.GetFileName(FilePath)} @ {lastOffset}");
                        break;
                    }
                }
            }

            var nodeCount = MemoryHelper.ReadInt32(Stream, lastOffset);
            for (var i = 0; i < nodeCount; i++)
            {
                TsNode node = new TsNode(this, lastOffset += 0x04);
                Mapper.UpdateEdgeCoords(node);
                if (!Mapper.Nodes.ContainsKey(node.Uid))
                    Mapper.Nodes.Add(node.Uid, node);
                lastOffset += 0x34;
            }

            lastOffset += 0x04;
            if (Version >= 891)
            {
                var visAreaChildCount = BitConverter.ToInt32(Stream, lastOffset);
                lastOffset += 0x04 + (0x08 * visAreaChildCount); // 0x04(visAreaChildCount) + (visAreaChildUids)
            }
            if (lastOffset != Stream.Length)
            {
                Logger.Instance.Warning($"File '{Path.GetFileName(FilePath)}' from '{GetUberFile().Entry.GetArchiveFile().GetPath()}' was not read correctly. " +
                    $"Read offset was at 0x{lastOffset:X} while file is 0x{Stream.Length:X} bytes long.");
            }
        }

        internal UberFile GetUberFile()
        {
            return _file;
        }

        public void ClearFileData()
        {
            Stream = null;
        }
    }
}
