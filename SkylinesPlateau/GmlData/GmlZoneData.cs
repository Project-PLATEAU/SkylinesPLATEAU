//----------------------------------------------------------------------------
// GmlZoneData.cs
//
// ■概要
//      区画情報を管理するクラス
// 
//
//----------------------------------------------------------------------------
using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.XPath;
using UnityEngine;

namespace SkylinesPlateau
{
    /// <summary>
    /// 区画情報(建物)を管理するクラス
    /// </summary>
    public class GmlZoneData
    {
        //-------------------------------------
        // 固定値
        //-------------------------------------
        public const string INPUT_PATH = @"Files/SkylinesPlateau/in";
        public const string INPUT_PATH2 = @"/udx/urf";

        //-------------------------------------
        // メンバ変数
        //-------------------------------------
        // 区画番号
        public int zone;
        // 地域地区の区分
        public string function;
        // 形状ポリゴン (ポリゴン範囲)
        public List<Vector3> points = new List<Vector3>();
        // 道路ポリゴンの範囲
        public Vector3 areaMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        public Vector3 areaMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        // 形状ポリゴン (穴あき)
        public List<InteriorPoints> interiorList = new List<InteriorPoints>();

        /// <summary>
        /// 条例(区画)のインポート処理
        /// </summary>
        static public int Import()
        {
            Logger.Log("条例の読み込み処理　開始");

            //-------------------------------------
            // XMLファイルの解析
            //-------------------------------------
            List<GmlZoneData> dataList = GmlZoneData.ReadXML();
            if (dataList.Count == 0)
            {
                // ファイルなし
                Logger.Log("条例データなし");
                return 0;
            }
            //-------------------------------------
            // 画面への反映処理
            //-------------------------------------
            drawZone(dataList);
            // 接道対応
            drawZoneWithRoad();

            Logger.Log("条例の読み込み処理　終了");

            return dataList.Count;
        }

        /// <summary>
        /// 指定フォルダのXMLファイルを解析して読み込む
        /// </summary>
        static public List<GmlZoneData> ReadXML()
        {
            // 読み込みデータを保持
            List<GmlZoneData> dataList = new List<GmlZoneData>();

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
            ZoneSgTbl sgTbl = new ZoneSgTbl();

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
                    Logger.Log("条例ファイルの読み込み開始：" + str);
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
                            GmlZoneData gmldata = new GmlZoneData();

                            // 地域地区の区分
                            gml.GetTagData(nav, "urf:UseDistrict/urf:function", out gmldata.function);
                            // 区画番号を設定
                            if (!sgTbl.dataDic.TryGetValue(gmldata.function, out gmldata.zone))
                            {
                                if (ImportSettingData.Instance.zoneType == 0)
                                {
                                    // 区画番号に該当なし
                                    continue;
                                }
                                // 「指定がない場合」の設定値がある場合
                                gmldata.zone = ImportSettingData.Instance.zoneType + 1;
                            }

                            // 形状ポリゴン
                            XPathNodeIterator nodeList2 = gml.GetXmlNodeList(nav, "urf:UseDistrict/urf:lod1MultiSurface/gml:MultiSurface/gml:surfaceMember/gml:Polygon/gml:exterior/gml:LinearRing/gml:posList");
                            if (nodeList2.Count == 0)
                            {
                                // ※<gml:LinearRing>ではなく、<gml:Ring>で指定されている場合
                                nodeList2 = gml.GetXmlNodeList(nav, "urf:UseDistrict/urf:lod1MultiSurface/gml:MultiSurface/gml:surfaceMember/gml:Polygon/gml:exterior/gml:Ring/gml:curveMember/gml:LineString/gml:posList");
                            }
                            foreach (XPathNavigator nav2 in nodeList2)
                            {
                                List<Vector3> tmpPoints;
                                // 頂点を取得
                                tmpPoints = GmlUtil.ConvertStringToListVec(nav2.Value, 1);
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
                                }
                                // 頂点を取得できているか確認
                                if (tmpPoints.Count > 2)
                                {
                                    gmldata.points.Clear();
                                    gmldata.points.AddRange(tmpPoints);
                                }
                            }
                            // 形状ポリゴン（内側）
                            XPathNodeIterator nodeList3 = gml.GetXmlNodeList(nav, "urf:UseDistrict/urf:lod1MultiSurface/gml:MultiSurface/gml:surfaceMember/gml:Polygon/gml:interior/gml:LinearRing/gml:posList");
                            if (nodeList3.Count == 0)
                            {
                                // ※<gml:LinearRing>ではなく、<gml:Ring>で指定されている場合
                                nodeList3 = gml.GetXmlNodeList(nav, "urf:UseDistrict/urf:lod1MultiSurface/gml:MultiSurface/gml:surfaceMember/gml:Polygon/gml:interior/gml:Ring/gml:curveMember/gml:LineString/gml:posList");
                            }
                            foreach (XPathNavigator nav3 in nodeList3)
                            {
                                InteriorPoints interior = new InteriorPoints();
                                List<Vector3> tmpPoints;
                                // 頂点を取得
                                tmpPoints = GmlUtil.ConvertStringToListVec(nav3.Value);
                                foreach (Vector3 vec in tmpPoints)
                                {
                                    //----------------------------------
                                    // 最大最小範囲の設定
                                    //----------------------------------
                                    if (interior.areaMax.x < vec.x) interior.areaMax.x = (float)vec.x;
                                    if (interior.areaMax.y < vec.y) interior.areaMax.y = (float)vec.y;
                                    if (interior.areaMax.z < vec.z) interior.areaMax.z = (float)vec.z;
                                    if (interior.areaMin.x > vec.x) interior.areaMin.x = (float)vec.x;
                                    if (interior.areaMin.y > vec.y) interior.areaMin.y = (float)vec.y;
                                    if (interior.areaMin.z > vec.z) interior.areaMin.z = (float)vec.z;
                                }
                                // 頂点を取得できているか確認
                                if (tmpPoints.Count > 2)
                                {
                                    interior.points.Clear();

                                    // 頂点順を判定
                                    if (!CommonFunc.checkPolyL2R(tmpPoints))
                                    {
                                        // 時計回りになるよう再設定
                                        for (int i = tmpPoints.Count - 1; i >= 0; i--)
                                        {
                                            interior.points.Add(tmpPoints[i]);
                                        }
                                    }
                                    else
                                    {
                                        interior.points.AddRange(tmpPoints);
                                    }
                                    // 内側ポリゴンの設定
                                    gmldata.interiorList.Add(interior);
                                }
                            }

                            // 頂点を取得できているか確認
                            if (gmldata.points.Count > 2)
                            {
                                if (IniFileData.Instance.logOut)
                                {
                                    Logger.Log("地域地区の区分  : " + gmldata.function);
                                    Logger.Log("区画            : " + gmldata.zone);
                                    Logger.Log("形状ポリゴン    : ", gmldata.points);
                                    foreach (InteriorPoints pos in gmldata.interiorList)
                                    {
                                        Logger.Log("形状ポリゴン内  : ", pos.points);
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
        /// 区画を画面上に反映する
        /// </summary>
        static public void drawZone(List<GmlZoneData> dataList)
        {
            Logger.Log("区画データの登録開始：");

            ZoneManager instance = Singleton<ZoneManager>.instance;

            // 対象の建物形状の数でループ処理
            foreach (var segment in dataList)
            {
                //---------------------------------
                // 建物形状の画面上の位置を判定
                //---------------------------------
                int num5 = Mathf.Max((int)((segment.areaMin.x - 46f) / 64f + 75f), 0);
                int num6 = Mathf.Max((int)((segment.areaMin.y - 46f) / 64f + 75f), 0);
                int num7 = Mathf.Min((int)((segment.areaMax.x + 46f) / 64f + 75f), 149);
                int num8 = Mathf.Min((int)((segment.areaMax.y + 46f) / 64f + 75f), 149);
                bool flag = false;
                // 画面全体のゾーングリッドの該当範囲（Ｘ方向）
                for (int i = num6; i <= num8; i++)
                {
                    // 画面全体のゾーングリッドの該当範囲（Ｚ方向）
                    for (int j = num5; j <= num7; j++)
                    {
                        // ゾーングリッドを取得（Ｘ方向はMax150個）
                        ushort num9 = instance.m_zoneGrid[i * ZoneManager.ZONEGRID_RESOLUTION + j];
                        int num10 = 0;
                        // ゾーングリッドを取得できた場合
                        while (num9 != 0)
                        {
                            // ゾーングリッドの中心位置
                            Vector3 position = instance.m_blocks.m_buffer[num9].m_position;
                            float num11 = Mathf.Max(
                                                Mathf.Max(segment.areaMin.x - 46f - position.x,
                                                          segment.areaMin.y - 46f - position.z
                                                ),
                                                Mathf.Max(position.x - segment.areaMax.x - 46f,
                                                          position.z - segment.areaMax.y - 46f
                                                )
                                          );
                            if (num11 < 0f && ApplyZoning(num9, ref instance.m_blocks.m_buffer[num9], segment))
                            {
                                flag = true;
                            }
                            // 次のゾーングリッドを取得？
                            num9 = instance.m_blocks.m_buffer[num9].m_nextGridBlock;

                            // 登録数が多すぎる場合
                            if (++num10 >= ZoneManager.MAX_BLOCK_COUNT)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                break;
                            }
                        }
                    }
                }
                if (flag)
                {

                    ItemClass.Zone tmp_zone = (ItemClass.Zone)segment.zone;
                    if (tmp_zone != ItemClass.Zone.None)
                    {
                        instance.m_zonesNotUsed.Disable();
                        instance.m_zoneNotUsed[segment.zone].Disable();
                        switch (tmp_zone)
                        {
                            case ItemClass.Zone.ResidentialLow:
                            case ItemClass.Zone.ResidentialHigh:
                                instance.m_zoneDemandResidential.Deactivate();
                                break;
                            case ItemClass.Zone.CommercialLow:
                            case ItemClass.Zone.CommercialHigh:
                                instance.m_zoneDemandCommercial.Deactivate();
                                break;
                            case ItemClass.Zone.Industrial:
                            case ItemClass.Zone.Office:
                                instance.m_zoneDemandWorkplace.Deactivate();
                                break;
                        }
                    }
                }
            }

            instance.m_zonesNotUsed.Disable();
            instance.m_zoneDemandResidential.Deactivate();
            instance.m_zoneDemandCommercial.Deactivate();
            instance.m_zoneDemandWorkplace.Deactivate();
        }

        /// <summary>
        /// 区画の接道対応
        /// </summary>
        static public void drawZoneWithRoad()
        {
            ZoneManager instance = Singleton<ZoneManager>.instance;
            //-------------------------------------
            // 区画の接道対応
            //-------------------------------------
            // 画面全体のゾーングリッドの該当範囲（Ｘ方向）
            for (int i = 0; i < ZoneManager.ZONEGRID_RESOLUTION; i++)
            {
                // 画面全体のゾーングリッドの該当範囲（Ｚ方向）
                for (int j = 0; j < ZoneManager.ZONEGRID_RESOLUTION; j++)
                {
                    // ゾーングリッドを取得（Ｘ方向はMax150個）
                    ushort num9 = instance.m_zoneGrid[i * ZoneManager.ZONEGRID_RESOLUTION + j];
                    // ゾーングリッドを取得できた場合
                    while (num9 != 0)
                    {
                        // 区画の接道対応
                        ApplyFillZoning(num9, ref instance.m_blocks.m_buffer[num9]);
                        // 次のゾーングリッドを取得？
                        num9 = instance.m_blocks.m_buffer[num9].m_nextGridBlock;
                    }
                }
            }
            instance.m_zonesNotUsed.Disable();
            instance.m_zoneDemandResidential.Deactivate();
            instance.m_zoneDemandCommercial.Deactivate();
            instance.m_zoneDemandWorkplace.Deactivate();
        }

        /// <summary>
        /// 区画の判定処理
        /// </summary>
        static private bool ApplyZoning(ushort blockIndex, ref ZoneBlock data, GmlZoneData gml_data)
        {
            //---------------------------------------------
            // ゾーンの範囲を取得
            //---------------------------------------------
            // ゾーンの回転角
            Vector2 a = new Vector2(Mathf.Cos(data.m_angle), Mathf.Sin(data.m_angle)) * 8f;
            Vector2 a2 = new Vector2(a.y, 0f - a.x);
            // ソーンの中心位置
            Vector2 a3 = VectorUtils.XZ(data.m_position);
            // ゾーンの区画数
            int rowCount = data.RowCount;
            // ゾーンの範囲
            List<Vector3> posList = new List<Vector3>();
            posList.Add(a3 - 4f * a - 4f * a2);
            posList.Add(a3 + 4f * a - 4f * a2);
            posList.Add(a3 + 4f * a + (rowCount - 4) * a2);
            posList.Add(a3 - 4f * a + (rowCount - 4) * a2);
            posList.Add(a3 - 4f * a - 4f * a2);
            // 最大最小範囲を算出
            Vector2 min1 = new Vector2(posList[0].x, posList[0].y);
            Vector2 max1 = new Vector2(posList[0].x, posList[0].y);
            for (int i = 1; i < posList.Count; i++)
            {
                if (min1.x > posList[i].x) min1.x = posList[i].x;
                if (min1.y > posList[i].y) min1.y = posList[i].y;
                if (max1.x < posList[i].x) max1.x = posList[i].x;
                if (max1.y < posList[i].y) max1.y = posList[i].y;
            }

            //---------------------------------------------
            // GMLデータの範囲を取得
            //---------------------------------------------
            Vector2 min2 = gml_data.areaMin;
            Vector2 max2 = gml_data.areaMax;

            if (IniFileData.Instance.logOut)
            {
                Logger.Log("建物　　   :" + min2 + " , " + max2);
                Logger.Log("区画全体   :" + min1 + " , " + max1);
            }

            //---------------------------------------------
            // 重なり判定 (矩形)
            //---------------------------------------------
            int iRet = CommonFunc.checkAreaInArea(max1, min1, max2, min2);
            if (iRet == 0)
            {
                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("範囲外： [ポリゴン : ゾーン]");
                }
                return false;
            }

            //---------------------------------------------
            // 重なり判定 (ポリゴン)
            //---------------------------------------------
            ItemClass.Zone new_zone = (ItemClass.Zone)gml_data.zone;
            bool flag = false;
            // 区画の列数でループ処理（Max8列）
            for (int i = 0; i < rowCount; i++)
            {
                // 区画のＸ方向
                Vector2 b0 = ((float)i - 3.5f) * a2;    // 中心Ｘ
                Vector2 b1 = ((float)i - 4.0f) * a2;    // 最小Ｘ
                Vector2 b2 = ((float)i - 3.0f) * a2;    // 最大Ｘ

                // 区画の行数でループ処理（Max4列）
                for (int j = 0; j < 4; j++)
                {
                    // 区画のＹ方向
                    Vector2 c0 = ((float)j - 3.5f) * a; // 中心Ｙ
                    Vector2 c1 = ((float)j - 4.0f) * a; // 最小Ｙ
                    Vector2 c2 = ((float)j - 3.0f) * a; // 最大Ｙ

                    //-----------------------------
                    // 区画１つ分の範囲取得
                    //-----------------------------
                    // 区画の四隅座標
                    posList[0] = (a3 + c1 + b1);
                    posList[1] = (a3 + c1 + b2);
                    posList[2] = (a3 + c2 + b2);
                    posList[3] = (a3 + c2 + b1);
                    posList[4] = (a3 + c1 + b1);
                    // 頂点順を判定
                    if (!CommonFunc.checkPolyL2R(posList))
                    {
                        // 時計回りになるよう再設定
                        Vector2 tmpVec = posList[1];
                        posList[1] = posList[3];
                        posList[3] = tmpVec;
                    }
                    // 最大最小範囲
                    min1 = new Vector2(posList[0].x, posList[0].y);
                    max1 = new Vector2(posList[0].x, posList[0].y);
                    for (int k = 1; k < posList.Count; k++)
                    {
                        if (min1.x > posList[k].x) min1.x = posList[k].x;
                        if (min1.y > posList[k].y) min1.y = posList[k].y;
                        if (max1.x < posList[k].x) max1.x = posList[k].x;
                        if (max1.y < posList[k].y) max1.y = posList[k].y;
                    }

                    string msg = "";
                    for (int lp = 0; lp < gml_data.points.Count; lp++)
                    {
                        msg += gml_data.points[lp];
                    }

                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log("建物　　   :" + msg);
                        Logger.Log("区画　　   :" + posList[0] + posList[1] + posList[2] + posList[3] + posList[4]);
                    }

                    //-----------------------------
                    // 重なり判定
                    //-----------------------------
                    iRet = CommonFunc.checkAreaInArea(max2, min2, max1, min1);
                    if (iRet == 0)
                    {
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("範囲外： [ポリゴン : 区画]");
                        }
                        continue;
                    }

                    //---------------------------------------------
                    // ポリゴン形状の比較
                    //---------------------------------------------
                    if (!CommonFunc.checkPolyInPoly(posList, gml_data.points))
                    {
                        // 重なっていない
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("範囲外： [ポリゴン : ポリゴン]");
                        }
                        continue;
                    }

                    //---------------------------------------------
                    // 穴あきポリゴンとの内外判定
                    //---------------------------------------------
                    if (CheckInteriorArea(posList, min1, max1, gml_data))
                    {
                        // 穴の中
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("範囲外： 穴の中");
                        }
                        continue;
                    }

                    //---------------------------------
                    // ゾーンのステータス判定
                    //---------------------------------
                    var zoneStatus = data.GetZone(j, i);
                    // ゾーンが無い場合
                    if (zoneStatus == ItemClass.Zone.None)
                    {
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("ゾーン変更対象外：" + zoneStatus + " -> " + new_zone);
                        }
                    }
                    // 未設定の場合
                    else if (zoneStatus == ItemClass.Zone.Unzoned)
                    {
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("ゾーン変更　　　：" + zoneStatus + " -> " + new_zone);
                        }
                        flag = data.SetZone(j, i, new_zone);
                    }
#if false
                    // 変更前が産業区画の場合
                    else if (zoneStatus == ItemClass.Zone.Industrial)
                    {
                        // 変更しない
                        Logger.Log("ゾーン変更対象外：" + zoneStatus + " -> " + new_zone);
                    }
                    // 変更後が産業区画の場合
                    else if (new_zone == ItemClass.Zone.Industrial)
                    {
                        if (zoneStatus != ItemClass.Zone.Unzoned)
                        {
                            // まずは解除する
                            data.SetZone(j, i, ItemClass.Zone.Unzoned);
                        }
                        Logger.Log("ゾーン変更　　　：" + zoneStatus + " -> " + new_zone);
                        flag = data.SetZone(j, i, new_zone);
                    }
                    // 今回設定するゾーンの方が、優先度が高い場合
                    else if (zoneStatus < new_zone)
                    {
                        if (zoneStatus != ItemClass.Zone.Unzoned)
                        {
                            // まずは解除する
                            data.SetZone(j, i, ItemClass.Zone.Unzoned);
                        }
                        Logger.Log("ゾーン変更　　　：" + zoneStatus + " -> " + new_zone);
                        flag = data.SetZone(j, i, new_zone);
                    }
#endif
                    else
                    {
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("ゾーン変更対象外：" + zoneStatus + " -> " + new_zone);
                        }
                    }
                }
            }
            if (flag)
            {
                // 更新
                data.RefreshZoning(blockIndex);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Zone範囲と、穴あきポリゴンとの内外判定
        /// </summary>
        /// <param name="areaPos">Zone範囲の各頂点</param>
        /// <param name="areaMin">Zone範囲（最小）</param>
        /// <param name="areaMax">Zone範囲（最大）</param>
        /// <param name="gml_data">建物情報</param>
        /// <returns>true:穴あきポリゴンの内側 , false:穴あきポリゴンの外側</returns>
        static private bool CheckInteriorArea(List<Vector3> areaPos, Vector2 areaMin, Vector2 areaMax, GmlZoneData gml_data)
        {
            int iRet;
            // 穴あきポリゴン分、ループ処理
            foreach (InteriorPoints interior in gml_data.interiorList)
            {
                //-----------------------------
                // 重なり判定
                //-----------------------------
                iRet = CommonFunc.checkAreaInArea(interior.areaMax, interior.areaMin, areaMax, areaMin);
                if (iRet == 0)
                {
                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log(" [Zoneと穴あき比較] 範囲外： [ポリゴン : 区画]");
                    }
                    continue;
                }

                //---------------------------------------------
                // ポリゴン形状の比較
                //---------------------------------------------
                if (!CommonFunc.checkPolyInPoly(areaPos, gml_data.points))
                {
                    // 重なっていない
                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log(" [Zoneと穴あき比較] 範囲外： [ポリゴン : ポリゴン]");
                    }
                    continue;
                }

                // 範囲内
                return true;
            }

            return false;
        }

        /// <summary>
        /// 区画を接道するよう調整処理
        /// </summary>
        static private bool ApplyFillZoning(ushort blockIndex, ref ZoneBlock data)
        {
            //---------------------------------------------
            // 設定済みなゾーンが存在するか確認
            //---------------------------------------------
            bool zoneCheckFlag = false;
            // 区画の列数でループ処理（Max8列）
            for (int i = 0; i < data.RowCount; i++)
            {
                // 区画の行数でループ処理（Max4列）
                for (int j = 0; j < 4; j++)
                {
                    var zoneStatus = data.GetZone(j, i);
                    if (zoneStatus != ItemClass.Zone.None &&
                        zoneStatus != ItemClass.Zone.Unzoned)
                    {
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[区画処理] 範囲内に何かしら区画設定あり");
                        }
                        // 「未設定 or 対象外」以外のゾーンあり
                        zoneCheckFlag = true;
                        break;
                    }
                }
                if (zoneCheckFlag)
                {
                    break;
                }
            }
            // 全てのゾーンが未設定な場合
            if (!zoneCheckFlag)
            {
                return false;
            }

            //---------------------------------------------
            // ゾーンの範囲を取得
            //---------------------------------------------
            // ゾーンの回転角
            Vector2 a = new Vector2(Mathf.Cos(data.m_angle), Mathf.Sin(data.m_angle)) * 8f;
            Vector2 a2 = new Vector2(a.y, 0f - a.x);
            // ソーンの中心位置
            Vector2 a3 = VectorUtils.XZ(data.m_position);
            // ゾーンの区画数
            int rowCount = data.RowCount;
            // ゾーンの範囲
            List<Vector2> posList = new List<Vector2>();
            posList.Add(a3 - 4f * a - 4f * a2);
            posList.Add(a3 + 4f * a - 4f * a2);
            posList.Add(a3 + 4f * a + (rowCount - 4) * a2);
            posList.Add(a3 - 4f * a + (rowCount - 4) * a2);
            posList.Add(a3 - 4f * a - 4f * a2);
            // 最大最小範囲を算出
            Vector2 min1 = new Vector2(posList[0].x, posList[0].y);
            Vector2 max1 = new Vector2(posList[0].x, posList[0].y);
            for (int i = 1; i < posList.Count; i++)
            {
                if (min1.x > posList[i].x) min1.x = posList[i].x;
                if (min1.y > posList[i].y) min1.y = posList[i].y;
                if (max1.x < posList[i].x) max1.x = posList[i].x;
                if (max1.y < posList[i].y) max1.y = posList[i].y;
            }

            //---------------------------------------------
            // 重なり判定 (ポリゴン)
            //---------------------------------------------
            ItemClass.Zone new_zone = ItemClass.Zone.None;
            bool flag = false;
            // 区画の列数でループ処理（Max8列）
            for (int i = 0; i < rowCount; i++)
            {
                int idx1 = 0;
                int idx2 = 0;
                // 区画の行数でループ処理（Max4列）
                for (idx1 = 0; idx1 < 4; idx1++)
                {
                    // ゾーンが有効か判定用（０だと非表示のゾーン）
                    var valid = (long)data.m_valid & (1L << ((i << 3) | idx1));
                    // ゾーンの種類を取得
                    var zoneStatus = data.GetZone(idx1, i);
                    // 表示されているゾーンで、種類も指定されている場合ループを抜ける
                    // ※接道位置から何マス空白があるかの判定
                    if (valid != 0 &&
                        zoneStatus != ItemClass.Zone.None &&
                        zoneStatus != ItemClass.Zone.Unzoned)
                    {
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[区画処理] 範囲内に何かしら区画設定あり２");
                        }
                        // 「未設定 or 対象外」以外のゾーンあり
                        new_zone = zoneStatus;
                        break;
                    }
                }

                // ０列目以降のゾーン指定ありの場合
                if (idx1 != 0 && idx1 != 4)
                {
                    // 区画の行数でループ処理（Max4列）
                    for (idx2 = 0; idx2 < idx1; idx2++)
                    {
                        var zoneStatus = data.GetZone(idx2, i);
                        if (zoneStatus == ItemClass.Zone.Unzoned && (((long)data.m_valid & (1L << ((i << 3) | 0))) != 0))
                        {
                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("ゾーン前詰め対応　　　：" + zoneStatus + " -> " + new_zone);
                            }
                            flag = data.SetZone(idx2, i, new_zone);
                        }
                    }
                }
            }
            if (flag)
            {
                // 更新
                data.RefreshZoning(blockIndex);
                return true;
            }
            return false;
        }
    }
}
