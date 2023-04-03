//----------------------------------------------------------------------------
// GmlBuildingData.cs
//
// ■概要
//      主要建物を管理するクラス
//
//----------------------------------------------------------------------------
using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using UnityEngine;

namespace SkylinesPlateau
{
    public class GmlBuildingData
    {
        //-------------------------------------
        // 固定値
        //-------------------------------------
        public const string INPUT_PATH = @"Files/SkylinesPlateau/in";
        public const string INPUT_PATH2 = @"/udx/bldg";

        //-------------------------------------
        // メンバ変数
        //-------------------------------------
        // 名称
        public string name;
        // [Asset優先度１] 小分類
        public string detailedUsage;
        public string detailedUsageValue;
        // [Asset優先度２] 用途分類２
        public string orgUsage2;
        public string orgUsage2Value;
        // [Asset優先度３] 用途分類
        public string orgUsage;
        public string orgUsageValue;
        // [Asset優先度４] 用途区分
        public string usage;
        public string usageValue;
        // 地上階の階数
        public string kaisu;
        // 高さ (実際に利用する値)
        public double height;
        // 高さ
        public string gmeasuredHeight;
        // ポリゴン高さ
        public double polyHeight;
        // ポリゴン面積
        public double polyAreaSize;
        // 建物ＩＤ
        public string buildingID;
        // 形状ポリゴン (底面)
        public List<Vector3> points = new List<Vector3>();
        // アセット名
        public string assetName;

        // 道路ポリゴンの範囲
        public Vector3 areaMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        public Vector3 areaMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

        /// <summary>
        /// 建物のインポート処理
        /// </summary>
        static public int Import()
        {
            Logger.Log("建物の読み込み処理　開始");

            //-------------------------------------
            // XMLファイルの解析
            //-------------------------------------
            List<GmlBuildingData> dataList = GmlBuildingData.ReadXML();
            if (dataList.Count == 0)
            {
                // ファイルなし
                Logger.Log("建物データなし");
                return 0;
            }
            //-------------------------------------
            // 画面への反映処理
            //-------------------------------------
            int drawCnt = drawBuilding(dataList);

            Logger.Log("建物の読み込み処理　終了");

            return drawCnt;
        }

        /// <summary>
        /// 指定フォルダのXMLファイルを解析して読み込む
        /// </summary>
        static public List<GmlBuildingData> ReadXML()
        {
            // 読み込みデータを保持
            List<GmlBuildingData> dataList = new List<GmlBuildingData>();

            //-------------------------------------
            // フォルダの存在チェック
            //-------------------------------------
            if (!Directory.Exists(INPUT_PATH))
            {
                // ファイルなし
                Logger.Log("フォルダがありません：" + INPUT_PATH);
                return dataList;
            }

            // TBL読み込み
            BuildingSgTbl sgTbl = new BuildingSgTbl();

            //-------------------------------------
            // 設定ファイルを保持するための変数定義
            //  Key: 設定ファイルのパス [bldg:Building/gml:name/@codeSpace]
            //  Val: 設定ファイルの解析結果(Dic)
            //        Key: [gml:Definition/gml:name]
            //        Val: [gml:Definition/gml:description]
            //-------------------------------------
            Dictionary<string, Dictionary<string, string>> gmlDataDic = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, string> tmpDic = new Dictionary<string, string>();
            // アセットロード
            AssetTbl.Instance.Load();

            // 指定フォルダの全フォルダを取得する
            DirectoryInfo di = new DirectoryInfo(INPUT_PATH);
            DirectoryInfo[] subFolders = di.GetDirectories();
            foreach (DirectoryInfo dir in subFolders)
            {
                if (!Directory.Exists(dir.FullName + INPUT_PATH2))
                {
                    // ファイルなし
                    Logger.Log("フォルダがありません：" + dir.FullName + INPUT_PATH2);
                    continue;
                }

                //-------------------------------------
                // フォルダ内のXMLファイルを取得
                //-------------------------------------
                IEnumerable<string> files = Directory.GetFiles(dir.FullName + INPUT_PATH2, "*.gml");
                // ファイル数分ループ処理
                foreach (string str in files)
                {
                    //-------------------------------------
                    // ファイル読み込み
                    //-------------------------------------
                    Logger.Log("建物ファイルの読み込み開始：" + str);
                    try
                    {
                        // GMLファイルのオープン
                        GmlUtil gml = new GmlUtil(str);
                        // 範囲チェック
                        if (!gml.CheckXmlArea()) continue;

                        // cityObjectMemberタグを抽出
                        XPathNodeIterator nodeList = gml.GetXmlNodeList(null, "core:CityModel/core:cityObjectMember");
                        // cityObjectMemberタグでループ
                        foreach (XPathNavigator nav in nodeList)
                        {
                            GmlBuildingData gmldata = new GmlBuildingData();

                            //-------------------------------------------------
                            // GMLから各種要素を取得
                            //-------------------------------------------------
                            // 建築物を識別する名称
                            string xmlPath, value;
                            gml.GetTagData(nav, "bldg:Building/gml:name/@codeSpace", out xmlPath);
                            gml.GetTagData(nav, "bldg:Building/gml:name", out value);
                            gml.GetTagDataGML(xmlPath, value, gmlDataDic, out gmldata.name);
                            if (gmldata.name == null) gmldata.name = "";
                            // 小分類
                            gml.GetTagData(nav, "bldg:Building/uro:buildingDetailAttribute/uro:BuildingDetailAttribute/uro:detailedUsage/@codeSpace", out xmlPath);
                            gml.GetTagData(nav, "bldg:Building/uro:buildingDetailAttribute/uro:BuildingDetailAttribute/uro:detailedUsage", out gmldata.detailedUsage);
                            gml.GetTagDataGML(xmlPath, gmldata.detailedUsage, gmlDataDic, out gmldata.detailedUsageValue);
                            if (gmldata.detailedUsageValue == null) gmldata.detailedUsageValue = "";
                            // 用途分類２
                            gml.GetTagData(nav, "bldg:Building/uro:buildingDetailAttribute/uro:BuildingDetailAttribute/uro:orgUsage2/@codeSpace", out xmlPath);
                            gml.GetTagData(nav, "bldg:Building/uro:buildingDetailAttribute/uro:BuildingDetailAttribute/uro:orgUsage2", out gmldata.orgUsage2);
                            gml.GetTagDataGML(xmlPath, gmldata.orgUsage2, gmlDataDic, out gmldata.orgUsage2Value);
                            if (gmldata.orgUsage2Value == null) gmldata.orgUsage2Value = "";
                            // 用途分類
                            gml.GetTagData(nav, "bldg:Building/uro:buildingDetailAttribute/uro:BuildingDetailAttribute/uro:orgUsage/@codeSpace", out xmlPath);
                            gml.GetTagData(nav, "bldg:Building/uro:buildingDetailAttribute/uro:BuildingDetailAttribute/uro:orgUsage", out gmldata.orgUsage);
                            gml.GetTagDataGML(xmlPath, gmldata.orgUsage, gmlDataDic, out gmldata.orgUsageValue);
                            if (gmldata.orgUsageValue == null) gmldata.orgUsageValue = "";
                            // 用途区分
                            gml.GetTagData(nav, "bldg:Building/bldg:usage/@codeSpace", out xmlPath);
                            gml.GetTagData(nav, "bldg:Building/bldg:usage", out gmldata.usage);
                            gml.GetTagDataGML(xmlPath, gmldata.usage, gmlDataDic, out gmldata.usageValue);
                            if (gmldata.usageValue == null) gmldata.usageValue = "";
                            // 地上階の階数
                            gml.GetTagData(nav, "bldg:Building/bldg:storeysAboveGround", out gmldata.kaisu);
                            // 高さ
                            gml.GetTagData(nav, "bldg:Building/bldg:measuredHeight", out gmldata.gmeasuredHeight);
                            // 建物ＩＤ
                            gml.GetTagData(nav, "bldg:Building/uro:buildingIDAttribute/uro:BuildingIDAttribute/uro:buildingID", out gmldata.buildingID);
                            // 形状ポリゴン
                            XPathNodeIterator nodeList2 = gml.GetXmlNodeList(nav, "bldg:Building/bldg:lod1Solid/gml:Solid/gml:exterior/gml:CompositeSurface/gml:surfaceMember/gml:Polygon/gml:exterior/gml:LinearRing/gml:posList");
                            if (nodeList2.Count == 0)
                            {
                                // ※<gml:LinearRing>ではなく、<gml:Ring>で指定されている場合
                                nodeList2 = gml.GetXmlNodeList(nav, "bldg:Building/bldg:lod1Solid/gml:Solid/gml:exterior/gml:CompositeSurface/gml:surfaceMember/gml:Polygon/gml:exterior/gml:Ring/gml:curveMember/gml:LineString/gml:posList");
                            }
                            double minAveZ = double.MaxValue;
                            foreach (XPathNavigator nav2 in nodeList2)
                            {
                                List<Vector3> tmpPoints;
                                double sumZ = 0;
                                double aveZ = 0;
                                // 頂点を取得
                                tmpPoints = GmlUtil.ConvertStringToListVec(nav2.Value);
//                                tmpPoints = GmlUtil.ConvertStringToListVec(nav2.Value, 2);
                                foreach (Vector3 vec in tmpPoints)
                                {
                                    //----------------------------------
                                    // 最大最小範囲の設定
                                    //----------------------------------
                                    if (gmldata.areaMax.x < vec.x) gmldata.areaMax.x = (float)vec.x;
                                    if (gmldata.areaMax.y < vec.y) gmldata.areaMax.y = (float)vec.y;
                                    if (gmldata.areaMax.z < vec.z) gmldata.areaMax.z = (float)vec.z;
                                    if (gmldata.areaMin.x > vec.x) gmldata.areaMin.x = (float)vec.x;
                                    if (gmldata.areaMin.y > vec.y) gmldata.areaMin.y = (float)vec.y;
                                    if (gmldata.areaMin.z > vec.z) gmldata.areaMin.z = (float)vec.z;

                                    sumZ += vec.z;
                                }
                                // 頂点を取得できているか確認
                                if (tmpPoints.Count > 2)
                                {
                                    // 高さの平均
                                    aveZ = sumZ / tmpPoints.Count;
                                    if (aveZ < minAveZ)
                                    {
                                        // 下側の面の範囲を保持
                                        gmldata.points.Clear();
                                        gmldata.points.AddRange(tmpPoints);
                                        // 平均高さを差し替え
                                        minAveZ = aveZ;
                                    }
                                }
                            }
                            // 高さ
                            gmldata.polyHeight = gmldata.areaMax.z - gmldata.areaMin.z;
                            // 面積
                            gmldata.polyAreaSize = (gmldata.areaMax.x - gmldata.areaMin.x) * (gmldata.areaMax.y - gmldata.areaMin.y);

                            //-------------------------------------------------
                            // アセット名を取得
                            //-------------------------------------------------
                            // 階数を高さに変換
                            int kai = gmldata.kaisu != "" ? int.Parse(gmldata.kaisu) : 0;
                            double gh = gmldata.gmeasuredHeight != "" ? double.Parse(gmldata.gmeasuredHeight) : 0;
                            if (kai >= 4 && gh < 12)
                            {
                                gmldata.height = 12;
                            }
                            // アセット判定に利用する高さを算出
                            else if (gmldata.gmeasuredHeight == "")
                            {
                                gmldata.height = gmldata.polyHeight;
                            }
                            else
                            {
                                gmldata.height = gh;
                            }

                            gmldata.assetName = sgTbl.GetAssetName(gmldata);

                            // アセット指定なし
                            if (gmldata.assetName == "")
                            {
                                if (IniFileData.Instance.logOut)
                                {
                                    Logger.Log("detailedUsage   : " + gmldata.detailedUsage + " , " + gmldata.detailedUsageValue);
                                    Logger.Log("orgUsage2       : " + gmldata.orgUsage2 + " , " + gmldata.orgUsage2Value);
                                    Logger.Log("orgUsage        : " + gmldata.orgUsage + " , " + gmldata.orgUsageValue);
                                    Logger.Log("usage           : " + gmldata.usage + " , " + gmldata.usageValue);
                                    Logger.Log("アセットなし");
                                    Logger.Log("--------------------------------");
                                }
                                continue;
                            }

                            //-------------------------------------------------
                            // 描画用の建物リストに追加
                            //-------------------------------------------------
                            // 頂点を取得できているか確認
                            if (gmldata.points.Count > 2)
                            {
                                if (IniFileData.Instance.logOut)
                                {
                                    Logger.Log("名称　　        : " + gmldata.name);
                                    Logger.Log("detailedUsage   : " + gmldata.detailedUsage + " , " + gmldata.detailedUsageValue);
                                    Logger.Log("orgUsage2       : " + gmldata.orgUsage2 + " , " + gmldata.orgUsage2Value);
                                    Logger.Log("orgUsage        : " + gmldata.orgUsage + " , " + gmldata.orgUsageValue);
                                    Logger.Log("usage           : " + gmldata.usage + " , " + gmldata.usageValue);
                                    Logger.Log("階数            : " + gmldata.kaisu);
                                    Logger.Log("高さ            : " + gmldata.gmeasuredHeight);
                                    Logger.Log("建物ＩＤ        : " + gmldata.buildingID);
                                    Logger.Log("Poly高さ        : " + gmldata.polyHeight);
                                    Logger.Log("Poly面積        : " + gmldata.polyAreaSize);
                                    Logger.Log("アセット名      : " + gmldata.assetName);


                                    if (gmldata.detailedUsage != "" || gmldata.orgUsage2 != "" || gmldata.orgUsage != "" || gmldata.usage != "")
                                    {
                                        Logger.Log("属性あり＿形状ポリゴン    : ", gmldata.points);
                                    }
                                    else if (gmldata.assetName == "")
                                    {
                                        Logger.Log("Aなし＿形状ポリゴン    : ", gmldata.points);
                                    }
                                    else
                                    {
                                        Logger.Log("Aあり＿形状ポリゴン    : ", gmldata.points);
                                    }
                                    Logger.Log("--------------------------------");
                                }
                                    //--------------------------------------
                                    // 情報を戻り値に追加
                                    //--------------------------------------
                                    dataList.Add(gmldata);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("xmlファイルの解析に失敗しました。：" + ex.Message);
                    }
                }
            }

            Logger.Log("読み込み建物のデータ数：" + dataList.Count);

            return dataList;
        }

        /// <summary>
        /// 建物を画面上に反映する
        /// </summary>
        static private int drawBuilding(List<GmlBuildingData> dataList)
        {
            Logger.Log("主要建物データの登録開始：");

            //-------------------------------------
            // 画面上に反映する
            //-------------------------------------
            int drawCount = 0;
            Randomizer rand = new Randomizer();
            TerrainManager tm = TerrainManager.instance;
            BuildingInfo bi;
            List<GmlZoneData> zoneList = new List<GmlZoneData>();

            if (IniFileData.Instance.logOut)
            {
                Logger.Log("建物の描画処理開始");
            }
            //-------------------------------------
            // 建物を追加
            //-------------------------------------
            foreach (var segment in dataList)
            {
                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("建物ＩＤ        : " + segment.buildingID);
                }
                //----------------------------------------
                // アセット取得
                //----------------------------------------
                // 区画指定の場合
                int zoneNo = BuildingSgTblData.CheckZoneNo(segment.assetName);
                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("アセット名      : " + segment.assetName);
                }
                if (zoneNo != 0)
                {
                    // 区画をリストに追加して、後続で追加する
                    GmlZoneData zonedata = new GmlZoneData();
                    zonedata.areaMin = segment.areaMin;
                    zonedata.areaMax = segment.areaMax;
                    zonedata.points = segment.points;
                    zonedata.zone = zoneNo;
                    zoneList.Add(zonedata);
                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log("区画としてインポート     : " + zonedata.zone);
                    }
                    continue;
                }

                bi = PrefabCollection<BuildingInfo>.FindLoaded(segment.assetName);
                // 対応しているアセットがない場合
                if (bi == null)
                {
                    Logger.Log("assetなし : " + segment.assetName);
                    continue;
                }

                //-----------------------------------------
                // 配置位置の算出
                //-----------------------------------------
                Vector3 centerPos = segment.areaMin + ((segment.areaMax - segment.areaMin) / 2);
                Vector3 servicePos = new Vector3(centerPos.x, 0, centerPos.y);
                float yCoord = tm.SampleRawHeightSmoothWithWater(servicePos, false, 0f);
                servicePos.y = yCoord;

                //-----------------------------------------
                // 建物を配置
                //-----------------------------------------
                BuildingManager bm = BuildingManager.instance;
                ushort bldId = 0;
                try
                {
                    if (bm.CreateBuilding(out bldId, ref rand, bi, servicePos, 0, bi.GetLength(), Singleton<SimulationManager>.instance.m_currentBuildIndex))
                    {
                        ++Singleton<SimulationManager>.instance.m_currentBuildIndex;
//                            Debug.Log("New segment ID: " + segmentId.ToString() + " and name: " + net_manager.GetSegmentName(segmentId));

                        // 名称設定
                        if (segment.name.Length > 0)
                        {
                            Building.Flags flags = bm.m_buildings.m_buffer[bldId].m_flags;
                            // 建物名称が変更されるまで待ち処理
                            var res = bm.SetBuildingName(bldId, segment.name);
                            while (res.MoveNext()) { }
                            Logger.Log("〇建物名称：" + segment.name + " [ " + bldId + " ]" + " : " + flags + " : " + segment.points[0]);
                        }
                        else
                        {
                            Logger.Log("○建物：" + segment.assetName + " : " + centerPos);
                        }
                        drawCount++;
                    }
                    else
                    {
                        Logger.Log("×建物：" + segment.assetName + " : " + centerPos);
                    }
                }
                catch (Exception ex)
                {
                    //try-catch just to prevent crashing by ignoring invalid trees and letting valid trees get created
                    //RaiseTreeMapperEvent (ex.Message);
                    Logger.Log("エラー発生。：" + ex.Message);
                }
//                    SimulationManager.instance.AddAction(AddBuilding(bi, servicePos, angle));
            }

            //-------------------------------------
            // 区画を追加
            //-------------------------------------
            if (zoneList.Count > 0)
            {
                GmlZoneData.drawZone(zoneList);
                // 接道対応
                GmlZoneData.drawZoneWithRoad();

                drawCount += zoneList.Count;
            }

            return drawCount;
        }
    }
}
