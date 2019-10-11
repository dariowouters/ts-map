using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using TsMap.HashFiles;

namespace TsMap
{

    public class TsItem // Might want to start keeping seperate items for each (major) version change
    {
        protected const int VegetationSphereBlockSize = 0x14;

        protected readonly TsSector Sector;
        public ulong Uid { get; }
        protected ulong StartNodeUid;
        protected ulong EndNodeUid;
        protected TsNode StartNode;
        protected TsNode EndNode;

        public List<ulong> Nodes { get; protected set; }

        public int BlockSize { get; protected set; }

        public bool Valid { get; protected set; }

        public TsItemType Type { get; }
        public float X { get; }
        public float Z { get; }
        public bool Hidden { get; protected set; }

        public TsItem(TsSector sector, int offset)
        {
            Sector = sector;

            var fileOffset = offset;

            Type = (TsItemType)MemoryHelper.ReadUInt32(Sector.Stream, fileOffset);

            Uid = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x04);

            X = MemoryHelper.ReadSingle(Sector.Stream, fileOffset += 0x08);
            Z = MemoryHelper.ReadSingle(Sector.Stream, fileOffset += 0x08);
        }

        public TsNode GetStartNode()
        {
            return StartNode ?? (StartNode = Sector.Mapper.GetNodeByUid(StartNodeUid));
        }

        public TsNode GetEndNode()
        {
            return EndNode ?? (EndNode = Sector.Mapper.GetNodeByUid(EndNodeUid));
        }

    }

    public class TsRoadItem : TsItem
    {
        private const int StampBlockSize = 0x18;
        public TsRoadLook RoadLook { get; }

        private List<PointF> _points;

        public void AddPoints(List<PointF> points)
        {
            _points = points;
        }

        public bool HasPoints()
        {
            return _points != null && _points.Count != 0;
        }

        public PointF[] GetPoints()
        {
            return _points?.ToArray();
        }

        public TsRoadItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = true;
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount: Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x06) > dlcGuardCount || (MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x03) & 0x02) != 0;
            RoadLook = Sector.Mapper.LookupRoadLook(MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x09));
            if (RoadLook == null)
            {
                Valid = false;
                Log.Msg($"Could not find RoadLook with id: {MemoryHelper.ReadUInt64(Sector.Stream, fileOffset):X}, " +
                        $"in {Path.GetFileName(Sector.FilePath)} @ {fileOffset}");
            }

            if (Sector.Version >= Common.BaseFileVersion130)
            {
                StartNodeUid = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08 + 0xA4);
                EndNodeUid = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08);
                fileOffset += 0x08 + 0x04;
            }
            else
            {
                StartNodeUid = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08 + 0x50);
                EndNodeUid = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08);
                var stampCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x08 + 0x134);
                var vegetationSphereCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (StampBlockSize * stampCount));
                fileOffset += 0x04 + (VegetationSphereBlockSize * vegetationSphereCount);

            }

            BlockSize = fileOffset - startOffset;
        }
    }

    public class TsPrefabItem : TsItem
    {
        private const int NodeLookBlockSize = 0x3A;
        private const int PrefabVegetaionBlockSize = 0x20;
        public int Origin { get; }
        public TsPrefab Prefab { get; }
        private List<TsPrefabLook> _looks;

        public void AddLook(TsPrefabLook look)
        {
            _looks.Add(look);
        }

        public List<TsPrefabLook> GetLooks()
        {
            return _looks;
        }

        public bool HasLooks()
        {
            return _looks != null && _looks.Count != 0;
        }

        public TsPrefabItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = true;
            _looks = new List<TsPrefabLook>();
            Nodes = new List<ulong>();
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x01) > dlcGuardCount || (MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x02) & 0x02) != 0;

            var prefabId = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05);
            Prefab = Sector.Mapper.LookupPrefab(prefabId);
            if (Prefab == null)
            {
                Valid = false;
                Log.Msg($"Could not find Prefab with id: {MemoryHelper.ReadUInt64(Sector.Stream, fileOffset):X}, " +
                        $"in {Path.GetFileName(Sector.FilePath)} @ {fileOffset} (item uid: 0x{Uid:X})");
            }

            if (Sector.Version >= Common.BaseFileVersion130)
            {
                var additionalItemCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x08 + 0x08);
                var nodeCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (additionalItemCount * 0x08));
                fileOffset += 0x04;
                for (var i = 0; i < nodeCount; i++)
                {
                    Nodes.Add(MemoryHelper.ReadUInt64(Sector.Stream, fileOffset));
                    fileOffset += 0x08;
                }
                var connectedItemCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
                Origin = MemoryHelper.ReadUint8(Sector.Stream, fileOffset += 0x04 + (0x08 * connectedItemCount) + 0x08);
                fileOffset += 0x02 + nodeCount * 0x0C;
                if (Sector.Version >= Common.BaseFileVersion132) fileOffset += 0x08;
            }
            else
            {
                var additionalItemCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x08 + 0x10);

                var nodeCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * additionalItemCount));
                fileOffset += 0x04;
                for (var i = 0; i < nodeCount; i++)
                {
                    Nodes.Add(MemoryHelper.ReadUInt64(Sector.Stream, fileOffset));
                    fileOffset += 0x08;
                }

                var connectedItemCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
                Origin = MemoryHelper.ReadUint8(Sector.Stream, fileOffset += 0x04 + (0x08 * connectedItemCount) + 0x08);
                var prefabVegetationCount = MemoryHelper.ReadInt32(Sector.Stream,
                    fileOffset += 0x01 + (NodeLookBlockSize * nodeCount) + 0x01);
                var vegetationSphereCount = MemoryHelper.ReadInt32(Sector.Stream,
                    fileOffset += 0x04 + (PrefabVegetaionBlockSize * prefabVegetationCount) + 0x04);
                fileOffset += 0x04 + (VegetationSphereBlockSize * vegetationSphereCount) + (0x18 * nodeCount);
            }

            BlockSize = fileOffset - startOffset;
        }
    }

    public class TsCompanyItem : TsItem
    {
        public TsMapOverlay Overlay { get; }

        public TsCompanyItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = true;
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x01) > dlcGuardCount;

            var overlayId = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05);

            Overlay = Sector.Mapper.LookupOverlay(overlayId);
            if (Overlay == null)
            {
                Valid = false;
                if (overlayId != 0) Log.Msg($"Could not find Company Overlay with id: {overlayId:X}, in {Path.GetFileName(Sector.FilePath)} @ {fileOffset}");
            }

            var count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x20);
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count));
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count));
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count));
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count));
            if (sector.Version >= Common.BaseFileVersion133) count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count));
            fileOffset += 0x04 + (0x08 * count);
            BlockSize = fileOffset - startOffset;
        }
    }

    public class TsServiceItem : TsItem
    {
        public TsServiceItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = false;
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            fileOffset += 0x05 + 0x10;
            if (Sector.Version >= Common.BaseFileVersion132)
            {
                var count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
                fileOffset += 0x08 * count + 0x04;
            }
            BlockSize = fileOffset - startOffset;
        }
    }

    public class TsCutPlaneItem : TsItem
    {
        public TsCutPlaneItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = false;
            var fileOffset = startOffset + 0x34; // Set position at start of flags

            var nodeCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x05);
            fileOffset += 0x04 + (0x08 * nodeCount);
            BlockSize = fileOffset - startOffset;
        }
    }

    public class TsCityItem : TsItem // TODO: Add zoom levels/range to show city names and icons correctly
    {
        public TsCity City { get; }
        public ulong NodeUid { get; }

        public TsCityItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = true;
            var fileOffset = startOffset + 0x34; // Set position at start of flags

            Hidden = (MemoryHelper.ReadUint8(Sector.Stream, fileOffset) & 0x01) != 0;
            var cityId = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05);
            NodeUid = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08 + 0x08);
            City = Sector.Mapper.LookupCity(cityId);
            if (City == null)
            {
                Valid = false;
                Log.Msg($"Could not find City with id: {cityId:X}, " +
                        $"in {Path.GetFileName(Sector.FilePath)} @ {fileOffset}");
            }
            fileOffset += 0x08;
            BlockSize = fileOffset - startOffset;
        }

        public override string ToString()
        {
            var name = City.Name;
            if (City.NameLocalized != string.Empty)
            {
                var localName = Sector.Mapper.GetLocalizedName(City.NameLocalized);
                if (localName != null) name = localName;
            }
            return $"{City.Country} - {name}";
        }
    }

    public class TsMapOverlayItem : TsItem
    {
        public TsMapOverlay Overlay { get; }
        public sbyte ZoomLevelVisibility { get; }

        public TsMapOverlayItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = true;
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            ZoomLevelVisibility = MemoryHelper.ReadInt8(Sector.Stream, fileOffset);
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x01) > dlcGuardCount || ZoomLevelVisibility == -1;

            var type = MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x02);
            var overlayId = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05);
            if (type == 1 && overlayId == 0) overlayId = 0x2358E762E112CD4; // parking
            Overlay = Sector.Mapper.LookupOverlay(overlayId);
            if (Overlay == null)
            {
                Valid = false;
                if (overlayId != 0) Log.Msg($"Could not find Overlay with id: {overlayId:X}, in {Path.GetFileName(Sector.FilePath)} @ {fileOffset}");
            }
            fileOffset += 0x08 + 0x08;
            BlockSize = fileOffset - startOffset;
        }
    }

    public class TsFerryItem : TsItem
    {
        public ulong FerryPortId { get; }
        public bool Train { get; }
        public TsMapOverlay Overlay { get; }

        public TsFerryItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = true;
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            Train = MemoryHelper.ReadUint8(Sector.Stream, fileOffset) != 0;
            if (Train) Overlay = Sector.Mapper.LookupOverlay(ScsHash.StringToToken("train_ico"));
            else Overlay = Sector.Mapper.LookupOverlay(ScsHash.StringToToken("port_overlay"));

            FerryPortId = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05);
            sector.Mapper.AddFerryPortLocation(FerryPortId, X, Z);
            fileOffset += 0x08 + 0x1C;
            BlockSize = fileOffset - startOffset;
        }
    }

    public class TsGarageItem : TsItem
    {
        public TsGarageItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = false;
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            fileOffset += 0x05 + 0x1C;
            if (Sector.Version >= Common.BaseFileVersion132)
            {
                var count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
                fileOffset += 0x08 * count + 0x04;
            }
            BlockSize = fileOffset - startOffset;
        }
    }

    public class TsTriggerItem : TsItem
    {
        public TsMapOverlay Overlay { get; }

        public TsTriggerItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = true;
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x01) > dlcGuardCount;
            var tagCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x05);
            var nodeCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * tagCount));
            var triggerActionCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * nodeCount));
            fileOffset += 0x04;

            for (var i = 0; i < triggerActionCount; i++)
            {
                var action = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset);
                if (action == 0x18991B7A99E279C) // hud_parking
                {
                    Overlay = Sector.Mapper.LookupOverlay(0x2358E762E112CD4);
                    if (Overlay == null)
                    {
                        Console.WriteLine("Could not find parking overlay");
                        Valid = false;
                    }
                }
                var overloadTag = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x08);
                fileOffset += 0x04; // set cursor after overloadTag

                if (overloadTag > 0) fileOffset += 0x04 * overloadTag; // set cursor after all overloaded values
                if (overloadTag == -1 && Sector.Version >= Common.BaseFileVersion136) continue; // triggerAction stops if overloadTag == -1, cursor already at end of overloadTag so just continue

                var hasParameters = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
                fileOffset += 0x04; // set cursor after hasParameters
                if (hasParameters == 1)
                {
                    var parametersLength = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
                    fileOffset += 0x04 + 0x04 + parametersLength; // 0x04(parametersLength) + 0x04(padding) + text(parametersLength * 0x01)
                }
                else if (hasParameters == 3) fileOffset += 0x08;

                if (Sector.Version < Common.BaseFileVersion136) fileOffset += 0x08;

                var targetTagCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
                fileOffset += 0x04 + targetTagCount * 0x08; // 0x04(targetTagCount) + targetTags
                if (Sector.Version >= Common.BaseFileVersion136) fileOffset += 0x04; // cursor after target_range
            }

            if (Sector.Version < Common.BaseFileVersion136) fileOffset += 0x18;
            else if (nodeCount == 1) fileOffset += 0x04; // radius ??
            BlockSize = fileOffset - startOffset;
        }
    }

    public class TsFuelPumpItem : TsItem
    {
        public TsFuelPumpItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = false;
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            fileOffset += 0x05 + 0x10;
            if (Sector.Version >= Common.BaseFileVersion132)
            {
                var count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
                fileOffset += 0x08 * count + 0x04;
            }
            BlockSize = fileOffset - startOffset;
        }
    }

    public class TsRoadSideItem : TsItem
    {
        public TsRoadSideItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = false;
            var fileOffset = startOffset + 0x34; // Set position at start of flags

            var templateCount = 3;

            if (Sector.Version >= Common.BaseFileVersion136)
            {
                templateCount = MemoryHelper.ReadUint8(Sector.Stream, fileOffset += 0x05 + 0x20);
                fileOffset += 0x01;
            }
            else fileOffset += 0x05 + 0x10;

            fileOffset += templateCount * 0x18;

            var tmplTextLength = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
            if (tmplTextLength != 0)
            {
                fileOffset += 0x04 + tmplTextLength;
            }
            var count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04);
            fileOffset += 0x04;
            for (var i = 0; i < count; i++)
            {
                var subItemCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x0C);
                fileOffset += 0x04;
                for (var x = 0; x < subItemCount; x++)
                {
                    var itemType = MemoryHelper.ReadInt16(Sector.Stream, fileOffset);

                    var someCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x02);
                    fileOffset += 0x04;

                    if (itemType == 0x05)
                    {
                        var textLength = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
                        fileOffset += 0x04 + 0x04 + textLength;
                    }
                    else if (itemType == 0x06)
                    {
                        fileOffset += 0x04 + 0x04 * someCount;
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
        }
    }

    public class TsBusStopItem : TsItem
    {
        public TsBusStopItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = false;
            var fileOffset = startOffset + 0x34; // Set position at start of flags

            fileOffset += 0x05 + 0x18;
            BlockSize = fileOffset - startOffset;
        }
    }

    public class TsTrafficRuleItem : TsItem
    {
        public TsTrafficRuleItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = false;
            var fileOffset = startOffset + 0x34; // Set position at start of flags

            var count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x05);
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count));
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count));
            fileOffset += 0x04 + 0x08;
            BlockSize = fileOffset - startOffset;
        }
    }

    public class TsTrajectoryItem : TsItem
    {
        public TsTrajectoryItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = false;
            var fileOffset = startOffset + 0x34; // Set position at start of flags

            var nodeCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x05);
            var ruleCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * nodeCount) + 0x0C);
            var checkPointCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x1C * ruleCount));
            fileOffset += 0x04 + (0x10 * checkPointCount) + 0x04;
            BlockSize = fileOffset - startOffset;
        }
    }

    public class TsMapAreaItem : TsItem
    {
        public List<ulong> NodeUids { get; }
        public uint ColorIndex { get; }
        public bool DrawOver { get; }

        public TsMapAreaItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = true;
            var fileOffset = startOffset + 0x34; // Set position at start of flags

            DrawOver = MemoryHelper.ReadUint8(Sector.Stream, fileOffset) != 0;
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x01) > dlcGuardCount;

            NodeUids = new List<ulong>();

            var nodeCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x05);
            fileOffset += 0x04;
            for (var i = 0; i < nodeCount; i++)
            {
                NodeUids.Add(MemoryHelper.ReadUInt64(Sector.Stream, fileOffset));
                fileOffset += 0x08;
            }

            ColorIndex = MemoryHelper.ReadUInt32(Sector.Stream, fileOffset);
            fileOffset += 0x04;
            BlockSize = fileOffset - startOffset;
        }
    }

}
