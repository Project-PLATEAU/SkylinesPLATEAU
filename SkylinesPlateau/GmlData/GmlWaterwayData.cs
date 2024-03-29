//----------------------------------------------------------------------------
// GmlWaterwayData.cs
//
// ■概要
//      河川を管理するクラス
// 
//
//----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.XPath;
using UnityEngine;

namespace SkylinesPlateau
{
    public class GmlWaterwayData
    {
        //-------------------------------------
        // 固定値
        //-------------------------------------
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] DEL_START
//        public const string INPUT_PATH = @"Files/SkylinesPlateau/in";
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] DEL_END
        public const string INPUT_PATH2 = @"/udx/luse";
        // 読み込み対象の区分
        public const int KASEN_LANDUSE = 5;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
        // 読み込み対象の区分
        public const int KASEN_LANDUSETYPE = 204;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END

        //-------------------------------------
        // メンバ変数
        //-------------------------------------
        // 名称
        public string name;
        // 土地利用用途の区分
        public int landUse;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
        // 土地利用用途の区分
        public int landUseType;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END
        // 形状ポリゴン
        public List<Vector3> points = new List<Vector3>();
        // 道路ポリゴンの範囲
        public Vector3 areaMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        public Vector3 areaMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        // 形状ポリゴン (穴あき)
        public List<InteriorPoints> interiorList = new List<InteriorPoints>();

        /// <summary>
        /// 建物のインポート処理
        /// </summary>
        static public int Import()
        {
            Logger.Log("河川の読み込み処理　開始");

            //-------------------------------------
            // XMLファイルの解析
            //-------------------------------------
            List<GmlWaterwayData> dataList = GmlWaterwayData.ReadXML();
            if (dataList.Count == 0)
            {
                // ファイルなし
                Logger.Log("建物データなし");
                return 0;
            }
            //-------------------------------------
            // 画面への反映処理
            //-------------------------------------
            drawWaterway(dataList);

            Logger.Log("建物の読み込み処理　終了");

            return dataList.Count;
        }

        /// <summary>
        /// 指定フォルダのXMLファイルを解析して読み込む
        /// </summary>
        static public List<GmlWaterwayData> ReadXML()
        {
            // 読み込みデータを保持
            List<GmlWaterwayData> dataList = new List<GmlWaterwayData>();
            
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
/*
            //-------------------------------------
            // フォルダの存在チェック
            //-------------------------------------
            if (!Directory.Exists(INPUT_PATH))
            {
                // ファイルなし
                Logger.Log("フォルダがありません：" + INPUT_PATH);
                return dataList;
            }

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
*/

            //-------------------------------------
            // フォルダの存在チェック
            //-------------------------------------
            if (!Directory.Exists(IniFileData.Instance.inputFolderPath))
            {
                // ファイルなし
                Logger.Log("フォルダがありません：" + IniFileData.Instance.inputFolderPath);
                return dataList;
            }

            DirectoryInfo dir = new DirectoryInfo(IniFileData.Instance.inputFolderPath);
            if (!Directory.Exists(dir.FullName + INPUT_PATH2))
            {
                // ファイルなし
                Logger.Log("フォルダがありません：" + dir.FullName + INPUT_PATH2);
                return dataList;
            }
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END

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
                    Logger.Log("河川ファイルの読み込み開始：" + str);
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
                            GmlWaterwayData gmldata = new GmlWaterwayData();

                            //-------------------------------------------------
                            // GMLから各種要素を取得
                            //-------------------------------------------------
                            // 識別する名称
                            gml.GetTagData(nav, "luse:LandUse/@gml:id", out gmldata.name);
                            
                            // 土地利用用途の区分
                            gml.GetTagData(nav, "luse:LandUse/uro:landUseDetailAttribute/uro:LandUseDetailAttribute/uro:orgLandUse", out gmldata.landUse);
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] DEL_START
//                            if (gmldata.landUse != KASEN_LANDUSE)
//                            {
//                                continue;
//                            }
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] DEL_END

// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
                            // 土地利用用途の区分
                            gml.GetTagData(nav, "luse:LandUse/luse:class", out gmldata.landUseType);
                            if (gmldata.landUseType != KASEN_LANDUSETYPE)
                            {
                                continue;
                            }
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END

                            // 形状ポリゴン
                            XPathNodeIterator nodeList2 = gml.GetXmlNodeList(nav, "luse:LandUse/luse:lod1MultiSurface/gml:MultiSurface/gml:surfaceMember/gml:Polygon/gml:exterior/gml:LinearRing/gml:posList");
                            if (nodeList2.Count == 0)
                            {
                                // ※<gml:LinearRing>ではなく、<gml:Ring>で指定されている場合
                                nodeList2 = gml.GetXmlNodeList(nav, "luse:LandUse/luse:lod1MultiSurface/gml:MultiSurface/gml:surfaceMember/gml:Polygon/gml:exterior/gml:Ring/gml:curveMember/gml:LineString/gml:posList");
                            }
                            foreach (XPathNavigator nav2 in nodeList2)
                            {
                                // 頂点を取得
                                gmldata.points = GmlUtil.ConvertStringToListVec(nav2.Value, 1);
                                foreach (Vector3 vec in gmldata.points)
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
                                if (gmldata.points.Count > 2)
                                {
                                    break;
                                }
                                gmldata.points.Clear();
                            }
                            // 形状ポリゴン（内側）
                            XPathNodeIterator nodeList3 = gml.GetXmlNodeList(nav, "luse:LandUse/luse:lod1MultiSurface/gml:MultiSurface/gml:surfaceMember/gml:Polygon/gml:interior/gml:LinearRing/gml:posList");
                            if (nodeList3.Count == 0)
                            {
                                // ※<gml:LinearRing>ではなく、<gml:Ring>で指定されている場合
                                nodeList3 = gml.GetXmlNodeList(nav, "luse:LandUse/luse:lod1MultiSurface/gml:MultiSurface/gml:surfaceMember/gml:Polygon/gml:interior/gml:Ring/gml:curveMember/gml:LineString/gml:posList");
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

                            //-------------------------------------------------
                            // 描画用の建物リストに追加
                            //-------------------------------------------------
                            // 頂点を取得できているか確認
                            if (gmldata.points.Count > 2)
                            {
                                if (IniFileData.Instance.logOut)
                                {
                                    Logger.Log("名称　　        : " + gmldata.name);
                                    Logger.Log("区分            : " + gmldata.landUse);
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
                                    Logger.Log("区分(TYPE)      : " + gmldata.landUseType);
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END
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
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] DEL_START
//            }
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] DEL_END

            Logger.Log("読み込み河川のデータ数：" + dataList.Count);

            return dataList;
        }

        /// <summary>
        /// 画面上に反映する
        /// </summary>
        static private void drawWaterway(List<GmlWaterwayData> dataList)
        {
            TerrainManager tm = TerrainManager.instance;
            Vector3 baseVec = new Vector3(MapExtent.MAX_AREA_SIZE / 2, MapExtent.MAX_AREA_SIZE / 2, 0);
            // 何度も標高減算処理を行わないよう、セルに対するフラグ管理を行う
            bool[] meshChkFlags = new bool[(TerrainManager.RAW_RESOLUTION + 1) * (TerrainManager.RAW_RESOLUTION + 1)];
            for (int i = 0; i < meshChkFlags.Length; i++) meshChkFlags[i] = false;

            // 高さをushort範囲内で表現するための比率
            float oneMetreBytes = ushort.MaxValue / (ushort)TerrainManager.TERRAIN_HEIGHT;
            ushort waterH = (ushort)(IniFileData.Instance.demWaterAreaDownHeight * oneMetreBytes);

            // 対象の数でループ処理
            foreach (var segment in dataList)
            {
                if (IniFileData.Instance.logOut)
                {
                    Logger.Log(segment.name);
                }

                //--------------------------------
                // ローカル変数定義
                //--------------------------------
                int iXstep, iYstep, iXstart, iYstart, iXend, iYend;
                int xidx, yidx, iXnum, iYnum;
                int iCellSize;
                bool bRet, bRet2;
                int idx;

                // グリッドサイズ
                iCellSize = (int)TerrainManager.RAW_CELL_SIZE;

                // Game画面中央ではなく、左上原点に変更
                Vector3 areaMin = segment.areaMin + baseVec;
                Vector3 areaMax = segment.areaMax + baseVec;

                // XY方向の始終点 (グリッドサイズで丸める)
                iXstart = (int)Math.Floor(areaMin.x / iCellSize) * iCellSize;
                iYstart = (int)Math.Floor(areaMin.y / iCellSize) * iCellSize;
                iXend = (int)Math.Ceiling(areaMax.x / iCellSize) * iCellSize;
                iYend = (int)Math.Ceiling(areaMax.y / iCellSize) * iCellSize;
                // グリッド数
                iXnum = (iXend - iXstart) / iCellSize;
                iYnum = (iYend - iYstart) / iCellSize;

                // 始終点をGame画面中央の座標系に戻す
                iXstart -= (int)baseVec.x;
                iYstart -= (int)baseVec.y;
                iXend -= (int)baseVec.x;
                iYend -= (int)baseVec.y;

                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("範囲： [" + iXnum + "," + iYnum + "]");
                }

                //----------------------------------------
                // ポリゴン範囲内の矩形数分ループ処理
                //----------------------------------------
                List<Vector3> tmpPoints = new List<Vector3>();
                for (iYstep = iYstart; iYstep <= iYend; iYstep += iCellSize)
                {
                    for (iXstep = iXstart; iXstep <= iXend; iXstep += iCellSize)
                    {
                        // 矩形範囲生成
                        tmpPoints.Clear();
                        tmpPoints.Add(new Vector3(iXstep, iYstep, 0));
                        tmpPoints.Add(new Vector3(iXstep + iCellSize, iYstep, 0));
                        tmpPoints.Add(new Vector3(iXstep + iCellSize, iYstep + iCellSize, 0));
                        tmpPoints.Add(new Vector3(iXstep, iYstep + iCellSize, 0));

                        // 内外判定
                        bRet = CommonFunc.checkPolyInPoly(tmpPoints, segment.points);
                        if (bRet)
                        {
                            bRet2 = false;
                            // ポリゴン内側の場合、穴あきの内部になっていないか判定する
                            foreach (InteriorPoints pos in segment.interiorList)
                            {
                                bRet2 = CommonFunc.checkPolyInPoly(tmpPoints, pos.points);
                                if (bRet2)
                                {
                                    if (IniFileData.Instance.logOut)
                                    {
                                        Logger.Log("穴の内側");
                                    }
                                    break;
                                }
                            }
                            if (bRet2)
                            {
                                // 穴の内側なのでスルーする
                                continue;
                            }

                            //--------------------------------------
                            // Index番号の取得
                            //--------------------------------------
                            // 該当する番号のセルを算出（CitiesSkylinesの座標系であるため、tmpPにMaxを加算することにより、左上原点にしている）
                            xidx = (int)Math.Floor((double)((iXstep + (iCellSize / 2)) + (MapExtent.MAX_AREA_SIZE / 2)) / iCellSize);
                            yidx = (int)Math.Floor((double)((iYstep + (iCellSize / 2)) + (MapExtent.MAX_AREA_SIZE / 2)) / iCellSize);
                            // 内外判定
                            if (xidx > TerrainManager.RAW_RESOLUTION || yidx > TerrainManager.RAW_RESOLUTION)
                            {
                                if (IniFileData.Instance.logOut)
                                {
                                    Logger.Log("画面範囲外なのでスルー: [" + xidx + "," + yidx + "]");
                                }
                                // 画面範囲外
                                continue;
                            }
                            idx = yidx * (TerrainManager.RAW_RESOLUTION + 1) + xidx;

                            // 河川高さ導入済みの場合
                            if (meshChkFlags[idx])
                            {
                                if (IniFileData.Instance.logOut)
                                {
                                    Logger.Log("[Skip] 二重導入を防ぐ: [" + xidx + "," + yidx + "] : " + idx);
                                }
                                continue;
                            }
                            // 二重導入され内容フラグを立てる
                            meshChkFlags[idx] = true;
                            
                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("対象の番号: [" + xidx + "," + yidx + "] : " + idx);
                            }

                            //-------------------------------------
                            // 地形の高さ設定
                            //-------------------------------------
                            var currentHeight = tm.RawHeights[idx];
                            if (tm.RawHeights[idx] < waterH) tm.RawHeights[idx] = 0;
                            else tm.RawHeights[idx] -= waterH;

                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("標高変更 : " + currentHeight + " -> " + tm.RawHeights[idx]);
                            }
                        }
                    }
                }
            }

            //-------------------------------------
            // 画面再描画
            //-------------------------------------
            int num4 = 120;
            for (int k = 0; k < 9; k++)
            {
                for (int l = 0; l < 9; l++)
                {
                    TerrainModify.UpdateArea(l * num4, k * num4, (l + 1) * num4, (k + 1) * num4, heights: true, surface: true, zones: true);
                }
            }
        }
    }
}
