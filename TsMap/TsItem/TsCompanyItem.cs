using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using TsMap.Common;
using TsMap.Helpers;
using TsMap.Helpers.Logger;
using TsMap.Map.Overlays;

namespace TsMap.TsItem
{
    public class TsCompanyItem : TsItem
    {
        private ulong _companyNameToken;
        private ulong _prefabUid;

        public TsCompanyItem(TsSector sector, int startOffset) : base(sector, startOffset)
        {
            Valid = true;
            if (Sector.Version < 858)
                TsCompanyItem825(startOffset);
            else if (Sector.Version >= 858 && Sector.Version < 900)
                TsCompanyItem858(startOffset);
            else if (Sector.Version >= 900)
                TsCompanyItem900(startOffset);
            else
                Logger.Instance.Error($"Unknown base file version ({Sector.Version}) for item {Type} " +
                    $"in file '{Path.GetFileName(Sector.FilePath)}' @ {startOffset} from '{Sector.GetUberFile().Entry.GetArchiveFile().GetPath()}'");
        }

        public void TsCompanyItem825(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            DlcGuard = MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x01);

            _companyNameToken = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05); // 0x05(flags)

            _prefabUid = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08 + 0x08); // 0x08(_companyNameToken) + 0x08(city_name)

            Nodes = new List<ulong>(1)
            {
                MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08) // 0x08(_prefabUid)
            };

            var count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x08); // count | 0x08 (node_uid)
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count)); // count2
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count)); // count3
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count)); // count4
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count)); // count5
            fileOffset += 0x04 + (0x08 * count);
            BlockSize = fileOffset - startOffset;
        }
        public void TsCompanyItem858(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            DlcGuard = MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x01);

            _companyNameToken = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05); // 0x05(flags)

            _prefabUid = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08 + 0x08); // 0x08(_companyNameToken) + 0x08(city_name)

            Nodes = new List<ulong>(1)
            {
                MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08) // 0x08(_prefabUid)
            };

            var count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x08); // count | 0x08 (node_uid)
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count)); // count2
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count)); // count3
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count)); // count4
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count)); // count5
            count = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x04 + (0x08 * count)); // count6
            fileOffset += 0x04 + (0x08 * count);
            BlockSize = fileOffset - startOffset;
        }

        public void TsCompanyItem900(int startOffset)
        {
            var fileOffset = startOffset + 0x34; // Set position at start of flags
            DlcGuard = MemoryHelper.ReadUint8(Sector.Stream, fileOffset + 0x01);

            _companyNameToken = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x05); // 0x05(flags)

            _prefabUid = MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08 + 0x08); // 0x08(_companyNameToken) + 0x08(city_name)

            Nodes = new List<ulong>(1)
            {
                MemoryHelper.ReadUInt64(Sector.Stream, fileOffset += 0x08) // 0x08(_prefabUid)
            };

            var nodeCount = MemoryHelper.ReadInt32(Sector.Stream, fileOffset += 0x08); // count | 0x08 (node_uid)
            fileOffset += 0x04 + (0x0C * nodeCount); // 0x04(nodeCount) + 0x0C(node_uid + node_flag)
            BlockSize = fileOffset - startOffset;
        }

        internal override void Update()
        {
            var prefab = Sector.Mapper.Prefabs.FirstOrDefault(x => x.Uid == _prefabUid);
            if (prefab == null)
            {
                Logger.Instance.Error(
                    $"Could not find prefab for company (uid: 0x{Uid:X}, name: '{ScsToken.TokenToString(_companyNameToken)}') " +
                    $"in file '{Path.GetFileName(Sector.FilePath)}' from '{Sector.GetUberFile().Entry.GetArchiveFile().GetPath()}'");
                return;
            }

            var originNode = Sector.Mapper.GetNodeByUid(prefab.Nodes[0]);
            if (prefab.Prefab.PrefabNodes == null) return;
            var mapPointOrigin = prefab.Prefab.PrefabNodes[prefab.Origin];

            var rot = (float)(originNode.Rotation - Math.PI -
                Math.Atan2(mapPointOrigin.RotZ, mapPointOrigin.RotX) + Math.PI / 2);

            var prefabStartX = originNode.X - mapPointOrigin.X;
            var prefabStartZ = originNode.Z - mapPointOrigin.Z;
            var companyPos = prefab.Prefab.SpawnPoints.FirstOrDefault(x => x.Type == TsSpawnPointType.CompanyPos);
            PointF point;
            if (companyPos == null)
            {
                // place icon at the company node position if the prefab does not have a company spawn point
                var companyNode = Sector.Mapper.GetNodeByUid(Nodes[0]);

                if (companyNode == null)
                {
                    Logger.Instance.Error($"Could not find company spawn point and company node for prefab UID: 0x{_prefabUid}.");
                    return;
                }

                point = new PointF(companyNode.X, companyNode.Z);
            }
            else
            {

                point = RenderHelper.RotatePoint(prefabStartX + companyPos.X,
                    prefabStartZ + companyPos.Z, rot,
                    originNode.X, originNode.Z);
            }

            if (!Sector.Mapper.OverlayManager.AddOverlay(ScsToken.TokenToString(_companyNameToken), OverlayType.Company,
                    point.X, point.Y, "Company", DlcGuard, prefab.IsSecret))
            {
                Logger.Instance.Error(
                    $"Could not find Company Overlay: '{ScsToken.TokenToString(_companyNameToken)}'({_companyNameToken:X}), item uid: 0x{Uid:X}, " +
                    $"in {Path.GetFileName(Sector.FilePath)} from '{Sector.GetUberFile().Entry.GetArchiveFile().GetPath()}'");
            }
        }
    }
}
