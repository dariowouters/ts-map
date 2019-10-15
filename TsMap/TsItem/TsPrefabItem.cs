using System.Collections.Generic;
using System.IO;

namespace TsMap.TsItem
{
    public class TsPrefabItem : TsItem
    {
        private const int NodeLookBlockSize = 0x3A;
        private const int NodeLookBlockSize825 = 0x38;
        private const int PrefabVegetaionBlockSize = 0x20;
        public int Origin { get; private set; }
        public TsPrefab Prefab { get; private set; }
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
            if (Sector.Version < 829)
                TsPrefabItem825(startOffset);
            else if (Sector.Version >= 829 && Sector.Version < 831)
                TsPrefabItem829(startOffset);
            else if (Sector.Version >= 831 && Sector.Version < 846)
                TsPrefabItem831(startOffset);
            else if (Sector.Version >= 846 && Sector.Version < 854)
                TsPrefabItem846(startOffset);
            else if (Sector.Version == 854)
                TsPrefabItem854(startOffset);
            else if (Sector.Version >= 855)
                TsPrefabItem855(startOffset);
            else
                Log.Msg(
                    $"Unknown base file version ({Sector.Version}) for item {Type} in file '{Path.GetFileName(Sector.FilePath)}' @ {startOffset}.");
        }

        public void TsPrefabItem825(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x01) > dlcGuardCount || (MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x02) & 0x02) != 0;

            var prefabId = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05); // 0x05(flags)
            Prefab = Sector.Mapper.LookupPrefab(prefabId);
            if (Prefab == null)
            {
                Valid = false;
                Log.Msg($"Could not find Prefab with id: {MemoryHelper.ReadUInt64(Sector.Stream, fileOffset):X}, " +
                        $"in {Path.GetFileName(Sector.FilePath)} @ {fileOffset} (item uid: 0x{Uid:X})");
            }
            var nodeCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x18); // 0x18(id & look & variant)
            fileOffset += 0x04; // set cursor after nodeCount
            for (var i = 0; i < nodeCount; i++)
            {
                Nodes.Add(MemoryHelper.ReadUInt64(Sector.Stream, fileOffset));
                fileOffset += 0x08;
            }

            var connectedItemCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
            Origin = MemoryHelper.ReadUint8(Sector.Stream, fileOffset += 0x04 + (0x08 * connectedItemCount) + 0x08); // 0x04(connItemCount) + connItemUids + 0x08(m_some_uid)
            var prefabVegetationCount = MemoryHelper.ReadInt32(Sector.Stream,
                fileOffset += 0x01 + 0x01 + (NodeLookBlockSize825 * nodeCount)); // 0x01(origin) + 0x01(padding) + nodeLooks
            var vegetationSphereCount = MemoryHelper.ReadInt32(Sector.Stream,
                fileOffset += 0x04 + (PrefabVegetaionBlockSize * prefabVegetationCount) + 0x04); // 0x04(prefabVegCount) + prefabVegs + 0x04(padding2)
            fileOffset += 0x04 + (VegetationSphereBlockSize825 * vegetationSphereCount); // 0x04(vegSphereCount) + vegSpheres


            BlockSize = fileOffset - startOffset;
        }

        public void TsPrefabItem829(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x01) > dlcGuardCount || (MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x02) & 0x02) != 0;

            var prefabId = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05); // 0x05(flags)
            Prefab = Sector.Mapper.LookupPrefab(prefabId);
            if (Prefab == null)
            {
                Valid = false;
                Log.Msg($"Could not find Prefab with id: {MemoryHelper.ReadUInt64(Sector.Stream, fileOffset):X}, " +
                        $"in {Path.GetFileName(Sector.FilePath)} @ {fileOffset} (item uid: 0x{Uid:X})");
            }

            var additionalPartsCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x18); // 0x18(id & look & variant)
            var nodeCount = MemoryHelper.ReadUint8(Sector.Stream, fileOffset += 0x04 + (0x08 * additionalPartsCount)); // 0x04(addPartsCount) + additionalParts
            fileOffset += 0x04; // set cursor after nodeCount
            for (var i = 0; i < nodeCount; i++)
            {
                Nodes.Add(MemoryHelper.ReadUInt64(Sector.Stream, fileOffset));
                fileOffset += 0x08;
            }

            var connectedItemCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
            Origin = MemoryHelper.ReadUint8(Sector.Stream, fileOffset += 0x04 + (0x08 * connectedItemCount) + 0x08); // 0x04(connItemCount) + connItemUids + 0x08(m_some_uid)
            var prefabVegetationCount = MemoryHelper.ReadInt32(Sector.Stream,
                fileOffset += 0x01 + 0x01 + (NodeLookBlockSize825 * nodeCount)); // 0x01(origin) + 0x01(padding) + nodeLooks
            var vegetationSphereCount = MemoryHelper.ReadInt32(Sector.Stream,
                fileOffset += 0x04 + (PrefabVegetaionBlockSize * prefabVegetationCount) + 0x04); // 0x04(prefabVegCount) + prefabVegs + 0x04(padding2)
            fileOffset += 0x04 + (VegetationSphereBlockSize * vegetationSphereCount); // 0x04(vegSphereCount) + vegSpheres


            BlockSize = fileOffset - startOffset;
        }

        public void TsPrefabItem831(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x01) > dlcGuardCount || (MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x02) & 0x02) != 0;

            var prefabId = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05); // 0x05(flags)
            Prefab = Sector.Mapper.LookupPrefab(prefabId);
            if (Prefab == null)
            {
                Valid = false;
                Log.Msg($"Could not find Prefab with id: {MemoryHelper.ReadUInt64(Sector.Stream, fileOffset):X}, " +
                        $"in {Path.GetFileName(Sector.FilePath)} @ {fileOffset} (item uid: 0x{Uid:X})");
            }

            var additionalPartsCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x18); // 0x18(id & look & variant)
            var nodeCount = MemoryHelper.ReadUint8(Sector.Stream, fileOffset += 0x04 + (0x08 * additionalPartsCount)); // 0x04(addPartsCount) + additionalParts
            fileOffset += 0x04; // set cursor after nodeCount
            for (var i = 0; i < nodeCount; i++)
            {
                Nodes.Add(MemoryHelper.ReadUInt64(Sector.Stream, fileOffset));
                fileOffset += 0x08;
            }

            var connectedItemCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
            Origin = MemoryHelper.ReadUint8(Sector.Stream, fileOffset += 0x04 + (0x08 * connectedItemCount) + 0x08); // 0x04(connItemCount) + connItemUids + 0x08(m_some_uid)
            var prefabVegetationCount = MemoryHelper.ReadInt32(Sector.Stream,
                fileOffset += 0x01 + 0x01 + (NodeLookBlockSize825 * nodeCount)); // 0x01(origin) + 0x01(padding) + nodeLooks
            var vegetationSphereCount = MemoryHelper.ReadInt32(Sector.Stream,
                fileOffset += 0x04 + (PrefabVegetaionBlockSize * prefabVegetationCount) + 0x04); // 0x04(prefabVegCount) + prefabVegs + 0x04(padding2)
            fileOffset += 0x04 + (VegetationSphereBlockSize * vegetationSphereCount) + (0x18 * nodeCount); // 0x04(vegSphereCount) + vegSpheres + padding
            BlockSize = fileOffset - startOffset;
        }
        public void TsPrefabItem846(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x01) > dlcGuardCount || (MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x02) & 0x02) != 0;

            var prefabId = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05); // 0x05(flags)
            Prefab = Sector.Mapper.LookupPrefab(prefabId);
            if (Prefab == null)
            {
                Valid = false;
                Log.Msg($"Could not find Prefab with id: {MemoryHelper.ReadUInt64(Sector.Stream, fileOffset):X}, " +
                        $"in {Path.GetFileName(Sector.FilePath)} @ {fileOffset} (item uid: 0x{Uid:X})");
            }

            var additionalPartsCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x18); // 0x18(id & look & variant)
            var nodeCount = MemoryHelper.ReadUint8(Sector.Stream, fileOffset += 0x04 + (0x08 * additionalPartsCount)); // 0x04(addPartsCount) + additionalParts
            fileOffset += 0x04; // set cursor after nodeCount
            for (var i = 0; i < nodeCount; i++)
            {
                Nodes.Add(MemoryHelper.ReadUInt64(Sector.Stream, fileOffset));
                fileOffset += 0x08;
            }

            var connectedItemCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
            Origin = MemoryHelper.ReadUint8(Sector.Stream, fileOffset += 0x04 + (0x08 * connectedItemCount) + 0x08); // 0x04(connItemCount) + connItemUids + 0x08(m_some_uid)
            var prefabVegetationCount = MemoryHelper.ReadInt32(Sector.Stream,
                fileOffset += 0x01 + 0x01 + (NodeLookBlockSize * nodeCount)); // 0x01(origin) + 0x01(padding) + nodeLooks
            var vegetationSphereCount = MemoryHelper.ReadInt32(Sector.Stream,
                fileOffset += 0x04 + (PrefabVegetaionBlockSize * prefabVegetationCount) + 0x04); // 0x04(prefabVegCount) + prefabVegs + 0x04(padding2)
            fileOffset += 0x04 + (VegetationSphereBlockSize * vegetationSphereCount) + (0x18 * nodeCount); // 0x04(vegSphereCount) + vegSpheres + padding
            BlockSize = fileOffset - startOffset;
        }

        public void TsPrefabItem854(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x01) > dlcGuardCount || (MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x02) & 0x02) != 0;

            var prefabId = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05); // 0x05(flags)
            Prefab = Sector.Mapper.LookupPrefab(prefabId);
            if (Prefab == null)
            {
                Valid = false;
                Log.Msg($"Could not find Prefab with id: {MemoryHelper.ReadUInt64(Sector.Stream, fileOffset):X}, " +
                        $"in {Path.GetFileName(Sector.FilePath)} @ {fileOffset} (item uid: 0x{Uid:X})");
            }
            var additionalPartsCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x08 + 0x08); // 0x08(prefabId) + 0x08(m_variant)
            var nodeCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (additionalPartsCount * 0x08)); // 0x04(addPartsCount) + additionalParts
            fileOffset += 0x04; // set cursor after nodeCount
            for (var i = 0; i < nodeCount; i++)
            {
                Nodes.Add(MemoryHelper.ReadUInt64(Sector.Stream, fileOffset));
                fileOffset += 0x08;
            }
            var connectedItemCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
            Origin = MemoryHelper.ReadUint8(Sector.Stream, fileOffset += 0x04 + (0x08 * connectedItemCount) + 0x08); // 0x04(connItemCount) + connItemUids + 0x08(m_some_uid)
            fileOffset += 0x02 + nodeCount * 0x0C; // 0x02(origin & padding) + nodeLooks

            BlockSize = fileOffset - startOffset;
        }
        public void TsPrefabItem855(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            var dlcGuardCount = (Sector.Mapper.IsEts2) ? Common.Ets2DlcGuardCount : Common.AtsDlcGuardCount;
            Hidden = MemoryHelper.ReadInt8(Sector.Stream, fileOffset + 0x01) > dlcGuardCount || (MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x02) & 0x02) != 0;

            var prefabId = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05); // 0x05(flags)
            Prefab = Sector.Mapper.LookupPrefab(prefabId);
            if (Prefab == null)
            {
                Valid = false;
                Log.Msg($"Could not find Prefab with id: {MemoryHelper.ReadUInt64(Sector.Stream, fileOffset):X}, " +
                        $"in {Path.GetFileName(Sector.FilePath)} @ {fileOffset} (item uid: 0x{Uid:X})");
            }
            var additionalPartsCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x08 + 0x08); // 0x08(prefabId) + 0x08(m_variant)
            var nodeCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (additionalPartsCount * 0x08)); // 0x04(addPartsCount) + additionalParts
            fileOffset += 0x04; // set cursor after nodeCount
            for (var i = 0; i < nodeCount; i++)
            {
                Nodes.Add(MemoryHelper.ReadUInt64(Sector.Stream, fileOffset));
                fileOffset += 0x08;
            }
            var connectedItemCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset);
            Origin = MemoryHelper.ReadUint8(Sector.Stream, fileOffset += 0x04 + (0x08 * connectedItemCount) + 0x08); // 0x04(connItemCount) + connItemUids + 0x08(m_some_uid)
            fileOffset += 0x02 + nodeCount * 0x0C + 0x08; // 0x02(origin & padding) + nodeLooks + 0x08(padding2)

            BlockSize = fileOffset - startOffset;
        }
    }
}
