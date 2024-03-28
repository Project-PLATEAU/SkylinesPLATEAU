//----------------------------------------------------------------------------
// GmlRailData.cs
//
// ■概要
//      線路を管理するクラス
// 
// ■改版履歴
//      Ver00.00.01     2020.12.16      G.Arakawa@Cmind     新規作成
//
//----------------------------------------------------------------------------
using ColossalFramework;
using ColossalFramework.Math;
using Mapbox.VectorTile;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
// 2023.09.11 G.Arakawa@cmind [PBFのダウンロード先変更対応] ADD_START
using UnityEngine.Networking;
// 2023.09.11 G.Arakawa@cmind [PBFのダウンロード先変更対応] ADD_END

namespace SkylinesPlateau
{
    public class GmlRailData
    {
        //-------------------------------------
        // 固定値
        //-------------------------------------
        public const string PBF_RAIL_KEY_FTCODE = "ftCode";
        public const string PBF_RAIL_KEY_ORGGILVL = "orgGILvl";
        public const string PBF_RAIL_KEY_SNGLDBL = "snglDbl";
        public const string PBF_RAIL_KEY_OPESTATE = "opeState";
        public const string PBF_RAIL_KEY_RAILSTATE = "railState";
        public const string PBF_RAIL_KEY_RTCODE = "rtCode";
        public const string PBF_RAIL_KEY_STACODE = "staCode";

        public const string PBF_RAIL_KEY_LVORDER = "lvOrder";

        /// <summary>
        /// PBFフォルダ（Cities.appからの相対パス）
        /// </summary>
        public const string INPUT_PATH_PBF = @"Files/SkylinesPlateau/pbf";
        /// <summary>
        /// PBFファイルのタイルサイズ
        /// </summary>
        private const int PBF_TILE_SIZE = 4096;
        // ズームレベル
        private const int ZOOM_LEVEL_RAIL = 14;


        //-------------------------------------
        // 固定値
        //-------------------------------------
        /// <summary>
        /// [ftCode]の設定値　（ZoomLevel16,17）
        /// </summary>
        public enum PBF_RAIL_FTCODE
        {
            /// <summary>
            /// 通常-通常部
            /// </summary>
            normal = 2801,
            /// <summary>
            /// 通常-橋・高架
            /// </summary>
            brige = 2803,
            /// <summary>
            /// 通常-トンネル
            /// </summary>
            tunnel = 2804,
            /// <summary>
            /// 通常-運休中
            /// </summary>
            stop = 2806,
            /// <summary>
            /// 特殊-通常部
            /// </summary>
            specialNormal = 2811,
            /// <summary>
            /// 特殊-橋・高架
            /// </summary>
            specialBrige = 2813,
            /// <summary>
            /// 索道-通常部
            /// </summary>
            cablewayNormal = 2821,
            /// <summary>
            /// 路面-通常部
            /// </summary>
            roadNormal = 2831,
            /// <summary>
            /// 側線-通常部
            /// </summary>
            lateralNormal = 2841,
            /// <summary>
            /// 側線-橋・高架
            /// </summary>
            lateralBrige = 2843,
            /// <summary>
            /// 地下鉄駅
            /// </summary>
            subwayStation = 8201
        }
        /// <summary>
        /// [snglDbl]の設定値
        /// </summary>
        public enum PBF_RAIL_SNGLDBL
        {
            /// <summary>
            /// 非表示
            /// </summary>
            hide = 0,
            /// <summary>
            /// 単線
            /// </summary>
            single = 1,
            /// <summary>
            /// 複線以上
            /// </summary>
            multie = 2,
            /// <summary>
            /// 側線
            /// </summary>
            lateral = 3,
            /// <summary>
            /// 駅部分
            /// </summary>
            station = 4
        }
        /// <summary>
        /// [opeState]の設定値
        /// </summary>
        public enum PBF_RAIL_OPESTATE
        {
            /// <summary>
            /// 運行中
            /// </summary>
            service = 0,
            /// <summary>
            /// 建設・休止中
            /// </summary>
            stop = 1
        }
        /// <summary>
        /// [railState]の設定値
        /// </summary>
        public enum PBF_RAIL_RAILSTATE
        {
            /// <summary>
            /// 通常部
            /// </summary>
            normal = 0,
            /// <summary>
            /// 橋・高架
            /// </summary>
            brige = 1,
            /// <summary>
            /// トンネル
            /// </summary>
            tunnel = 2,
            /// <summary>
            /// 地下
            /// </summary>
            underground = 3,
            /// <summary>
            /// 雪覆い
            /// </summary>
            snow = 4,
            /// <summary>
            /// 運休中
            /// </summary>
            stop = 5,
            /// <summary>
            /// その他
            /// </summary>
            other = 6,
            /// <summary>
            /// 不明
            /// </summary>
            unknown = 7
        }
        /// <summary>
        /// [rtCode]の設定値
        /// </summary>
        public enum PBF_RAIL_RTCODE
        {
            /// <summary>
            /// JR（上５桁が一致する場合）
            /// </summary>
            jr1 = 40201,
            /// <summary>
            /// JR（上５桁が一致する場合）
            /// </summary>
            jr2 = 40216,
            /// <summary>
            /// JR以外（上５桁が一致する場合）
            /// </summary>
            nJr1 = 40202,
            /// <summary>
            /// JR以外（上５桁が一致する場合）
            /// </summary>
            nJr2 = 40205,
            /// <summary>
            /// 地下鉄（上５桁が一致する場合）
            /// </summary>
            subway = 40203,
            /// <summary>
            /// 路面（上５桁が一致する場合）
            /// </summary>
            road = 40204,
            /// <summary>
            /// 索道（上５桁が一致する場合）
            /// </summary>
            cableway = 40206,
            /// <summary>
            /// 特殊鉄道（上５桁が一致する場合）
            /// </summary>
            special = 40217
        }

        /// <summary>
        /// [staCode]の設定値
        /// </summary>
        public enum PBF_RAIL_STACODE
        {
            /// <summary>
            /// 駅以外
            /// </summary>
            nStation = 0,
            /// <summary>
            /// 駅（4で始まる11桁）
            /// </summary>
            station = 4
        }

        //-------------------------------------
        // メンバ変数
        //-------------------------------------
        public long ftCode = -1;
        public string orgGILvl = "";
        public long snglDbl = -1;
        public long opeState = -1;
        public long railState = -1;
        public string rtCode = "";
        public string staCode = "";
        public bool isCrossWaterway = false;    // 河川と交差するか（橋、架橋判定に使用）
        public bool isCrossRoad = false;        // 道路と交差するか（橋、架橋判定に使用）
        public List<Vector3> points = new List<Vector3>();

        //-------------------------------------
        // ReadOnly
        //-------------------------------------
        /// <summary>
        /// インポート対象とするか判定
        /// </summary>
        public bool isImport
        {
            get
            {
                // すべてを対象とする
                return true;
            }
        }
        /// <summary>
        /// 橋か判定用フラグ
        /// </summary>
        public bool isBridge
        {
            get
            {
                if (isRailTypeSpecial)
                {
                    // 特殊鉄道は地上のみとしておく
                    return false;
                }

                if (railState == -1)
                {
                    if (ftCode == (long)PBF_RAIL_FTCODE.brige ||
                        ftCode == (long)PBF_RAIL_FTCODE.specialBrige)
                    {
                        return true;
                    }
                }
                else
                {
                    PBF_RAIL_RAILSTATE value = (PBF_RAIL_RAILSTATE)railState;
                    if (value == PBF_RAIL_RAILSTATE.brige)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// トンネルか判定用フラグ
        /// </summary>
        public bool isTunnel
        {
            get
            {
                if (isRailTypeSpecial)
                {
                    // 特殊鉄道は地上のみとしておく
                    return false;
                }

                if (railState == -1)
                {
                    if (ftCode == (long)PBF_RAIL_FTCODE.tunnel)
                    {
                        return true;
                    }
                }
                else
                {
                    PBF_RAIL_RAILSTATE value = (PBF_RAIL_RAILSTATE)railState;
                    if (value == PBF_RAIL_RAILSTATE.tunnel)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// 地下か判定用フラグ
        /// </summary>
        public bool isUnderground
        {
            get
            {
                if (isRailTypeSpecial)
                {
                    // 特殊鉄道は地上のみとしておく
                    return false;
                }

                PBF_RAIL_RAILSTATE value = (PBF_RAIL_RAILSTATE)railState;
                if (value == PBF_RAIL_RAILSTATE.underground)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 通常の線路か判定用フラグ
        /// </summary>
        public bool isRailTypeNormal
        {
            get
            {
                if (isRailTypeSubway || isRailTypeRoad || isRailTypeCableway || isRailTypeSpecial)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 地下鉄か判定用フラグ
        /// </summary>
        public bool isRailTypeSubway
        {
            get
            {
                int data = (int)PBF_RAIL_RTCODE.subway;
                if (rtCode.StartsWith(data.ToString()))
                {
                    return true;
                }

                return false;
            }
        }
        /// <summary>
        /// 路面列車か判定用フラグ
        /// </summary>
        public bool isRailTypeRoad
        {
            get
            {
                if (rtCode == "")
                {
                    if (ftCode == (long)PBF_RAIL_FTCODE.roadNormal)
                    {
                        return true;
                    }
                }
                else
                {
                    int data = (int)PBF_RAIL_RTCODE.road;
                    if (rtCode.StartsWith(data.ToString()))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
        /// <summary>
        /// 索道(ロープウェイ)か判定用フラグ
        /// </summary>
        public bool isRailTypeCableway
        {
            get
            {
                if (rtCode == "")
                {
                    if (ftCode == (long)PBF_RAIL_FTCODE.cablewayNormal)
                    {
                        return true;
                    }
                }
                else
                {
                    int data = (int)PBF_RAIL_RTCODE.cableway;
                    if (rtCode.StartsWith(data.ToString()))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
        /// <summary>
        /// 特殊な線路か判定用フラグ
        /// </summary>
        public bool isRailTypeSpecial
        {
            get
            {
                if (rtCode == "")
                {
                    if (ftCode == (long)PBF_RAIL_FTCODE.specialNormal ||
                        ftCode == (long)PBF_RAIL_FTCODE.specialBrige)
                    {
                        return true;
                    }
                }
                else
                {
                    int data = (int)PBF_RAIL_RTCODE.special;
                    if (rtCode.StartsWith(data.ToString()))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// インポート処理
        /// </summary>
        static public int Import()
        {
            //-------------------------------------
            // フォルダが存在するか確認
            //-------------------------------------
            if (!Directory.Exists(INPUT_PATH_PBF))
            {
                // フォルダを生成
                Directory.CreateDirectory(INPUT_PATH_PBF);
            }

            //-------------------------------------
            // フォルダ内のファイルを取得
            //-------------------------------------
            Dictionary<short, List<SimpleNode>> nodeMap = new Dictionary<short, List<SimpleNode>>();
            // 読み込みデータを保持
            List<GmlRailData> dataList = new List<GmlRailData>();
            // 読み込みデータを保持（トンネル）
            List<GmlRailData> dataList_Tunnel = new List<GmlRailData>();
            // 読み込みデータを保持（橋・高架）
            List<GmlRailData> dataList_Brige = new List<GmlRailData>();
            // 読み込みデータを保持（地下）
            List<GmlRailData> dataList_Under = new List<GmlRailData>();

            // ズームレベル設定
            MapExtent.Instance.zoomLevel = 14;

            // Ｘ方向のタイル
            for (var idxX = MapExtent.Instance.minTileX; idxX <= MapExtent.Instance.maxTileX; idxX++)
            {
                // Ｙ方向のタイル
                for (var idxY = MapExtent.Instance.minTileY; idxY <= MapExtent.Instance.maxTileY; idxY++)
                {
                    string str = getVectorTile(MapExtent.Instance.zoomLevel, idxX, idxY);
                    if (str == "")
                    {
                        continue;
                    }

                    //-------------------------------------
                    // ファイル読み込み
                    //-------------------------------------
                    Logger.Log("線路ファイルの読み込み開始：" + str);

                    //-------------------------------------
                    // タイル情報を取得
                    //-------------------------------------
                    ulong zoom = 0;
                    ulong tileCol = 0;
                    ulong tileRow = 0;
                    if (!parseArg(Path.GetFileName(str), out zoom, out tileCol, out tileRow))
                    {
                        Logger.Log("ファイル名のパース失敗 : " + str);
                        continue;
                    }

                    //-------------------------------------
                    // レイヤ存在チェック
                    //-------------------------------------
                    string layerName = "railway";
                    var bufferedData = File.ReadAllBytes(str);
                    bool chkFlag = false;
                    VectorTile tile = new VectorTile(bufferedData);
                    foreach (string lyrName in tile.LayerNames())
                    {
                        if (lyrName == layerName)
                        {
                            chkFlag = true;
                            break;
                        }
                    }
                    if (!chkFlag)
                    {
                        Logger.Log("レイヤなし");
                        continue;
                    }

                    //-------------------------------------
                    // ファイル読み込み
                    //-------------------------------------
                    VectorTileLayer lyr = tile.GetLayer(layerName);
                    int featCnt = lyr.FeatureCount();
                    for (int i = 0; i < featCnt; i++)
                    {
                        GmlRailData inData = new GmlRailData();
                        VectorTileFeature feat = lyr.GetFeature(i, null);
                        // 各フィーチャの情報を取得する
                        Dictionary<string, object> props = feat.GetProperties();
                        foreach (var prop in props)
                        {
                            if (prop.Key == GmlRailData.PBF_RAIL_KEY_FTCODE)
                            {
                                inData.ftCode = (long)prop.Value;
                            }
                            else if (prop.Key == GmlRailData.PBF_RAIL_KEY_ORGGILVL)
                            {
                                inData.orgGILvl = (string)prop.Value;
                            }
                            else if (prop.Key == GmlRailData.PBF_RAIL_KEY_SNGLDBL)
                            {
                                inData.snglDbl = (long)prop.Value;
                            }
                            else if (prop.Key == GmlRailData.PBF_RAIL_KEY_OPESTATE)
                            {
                                inData.opeState = (long)prop.Value;
                            }
                            else if (prop.Key == GmlRailData.PBF_RAIL_KEY_RAILSTATE)
                            {
                                if (prop.Value is string)
                                {
                                    Logger.Log("データ型が不正：" + prop.Value);
                                    string tmpStr = (string)prop.Value;
                                    if (tmpStr == "通常部")
                                    {
                                        inData.railState = 0;
                                    }
                                    else if (tmpStr == "橋・高架")
                                    {
                                        inData.railState = 1;
                                    }
                                    else if (tmpStr == "トンネル")
                                    {
                                        inData.railState = 2;
                                    }
                                    else if (tmpStr == "地下")
                                    {
                                        inData.railState = 3;
                                    }
                                }
                                else if (prop.Value is long)
                                {
                                    inData.railState = (long)prop.Value;
                                }
                            }
                            else if (prop.Key == GmlRailData.PBF_RAIL_KEY_RTCODE)
                            {
                                inData.rtCode = (string)prop.Value;
                                Logger.Log("RTCODE：" + inData.rtCode);
                            }
                            else if (prop.Key == GmlRailData.PBF_RAIL_KEY_STACODE)
                            {
                                inData.staCode = (string)prop.Value;
                            }
                        }

                        // 読み込み対象か判定
                        if (!inData.isImport)
                        {
                            continue;
                        }

                        var pointsStr = "";
                        var pointsStr2 = "";
                        double bk_x = double.MaxValue;
                        double bk_y = double.MaxValue;
                        bool posAddFlag = false;
                        // 各頂点の情報を取得
                        foreach (var part in feat.Geometry<int>())
                        {
                            //iterate through coordinates of the part
                            foreach (var geom in part)
                            {
                                if (geom.X < 0 || geom.X > PBF_TILE_SIZE ||
                                    geom.Y < 0 || geom.Y > PBF_TILE_SIZE)
                                {
                                    // タイル範囲外
                                    continue;
                                }

                                // ログ出力用の文字列設定
                                if (pointsStr.Length > 0)
                                {
                                    pointsStr += ", ";
                                    pointsStr2 += ", ";
                                }
                                else
                                {
                                    pointsStr2 = "[" + zoom + ":" + tileCol + "," + tileRow + "] ";
                                }

                                // 座標位置を取得（Webメルカトル）
                                Vector2 pos = convTile2Point(geom.X, geom.Y, zoom, tileCol, tileRow, lyr.Extent);
                                // Webメルカトルから緯度経度に変換
                                double lon = CommonBL.XToLon(pos.x);
                                double lat = CommonBL.YToLat(pos.y);
                                // 緯度経度から平面直角座標に変換
                                double x, y;
                                CommonBL.Instance.bl2xy(lat, lon, out x, out y);
                                // 画面中央を(0,0)とした座標系に変換
                                pos.x = (float)(x - MapExtent.Instance.centerX);
                                pos.y = (float)(y - MapExtent.Instance.centerY);
                                // 画面範囲に対応するよう倍率を調整
                                pos.x = (pos.x * (float)MapExtent.Instance.areaScaleX);
                                pos.y = (pos.y * (float)MapExtent.Instance.areaScaleY);
                                // 座標値をまるめることにより、頂点数を削減
                                pos.x = (float)(((int)(pos.x * 1000.0)) / 1000.0);
                                pos.y = (float)(((int)(pos.y * 1000.0)) / 1000.0);

                                pointsStr += ("(" + pos.x + "," + pos.y + ")");
                                pointsStr2 += ("(" + geom.X + "," + geom.Y + ")");

                                //--------------------------------------------------
                                // 頂点の内外判定
                                //--------------------------------------------------
                                // 範囲外の頂点の場合
                                if (Math.Abs(pos.x) > MapExtent.Instance.importDist || Math.Abs(pos.y) > MapExtent.Instance.importDist)
                                {
                                    // 範囲内の頂点から範囲外となった場合
                                    if (inData.points.Count > 0 && !posAddFlag)
                                    {
                                        // 頂点を追加
                                        inData.points.Add(new Vector2((float)pos.x, (float)pos.y));
                                        posAddFlag = true;
                                    }
                                    // 最初から範囲外の場合
                                    if (!posAddFlag)
                                    {
                                        // 頂点情報を保持しておく
                                        bk_x = pos.x;
                                        bk_y = pos.y;
                                    }
                                    continue;
                                }
                                else
                                {
                                    // 範囲外から範囲内に入った場合
                                    if (!posAddFlag && (bk_x != double.MaxValue || bk_y != double.MaxValue))
                                    {
                                        // 頂点を追加
                                        inData.points.Add(new Vector2((float)bk_x, (float)bk_y));
                                        posAddFlag = true;
                                    }
                                }

                                inData.points.Add(pos);
                            }
                        }
                        // 頂点を取得できているか確認
                        if (inData.points.Count > 1)
                        {
                            // テストモードか判定
                            if (IniFileData.Instance.rail_isTestMode)
                            {
                                if (IniFileData.Instance.rail_ftCode >= -1)
                                {
                                    Logger.Log("[TEST] 強制差し替え：ftCode： " + inData.ftCode + " -> " + IniFileData.Instance.rail_ftCode);
                                    inData.ftCode = IniFileData.Instance.rail_ftCode;
                                }
                                if (IniFileData.Instance.rail_railState >= -1)
                                {
                                    Logger.Log("[TEST] 強制差し替え：railState： " + inData.railState + " -> " + IniFileData.Instance.rail_railState);
                                    inData.railState = IniFileData.Instance.rail_railState;
                                }
                                if (IniFileData.Instance.rail_rtCode != "")
                                {
                                    Logger.Log("[TEST] 強制差し替え：rtCode： " + inData.rtCode + " -> " + IniFileData.Instance.rail_rtCode);
                                    inData.rtCode = IniFileData.Instance.rail_rtCode;
                                }
                            }

                            // 読み込んだ情報を保持
                            if (inData.isTunnel)
                            {
                                dataList_Tunnel.Add(inData);
                            }
                            else if (inData.isBridge)
                            {
                                dataList_Brige.Add(inData);
                            }
                            else if (inData.isUnderground)
                            {
                                dataList_Under.Add(inData);
                            }
                            else
                            {
                                dataList.Add(inData);
                            }
//                        Logger.Log("POS  : " + pointsStr);
//                        Logger.Log("POS2 : " + pointsStr2);
//                        Logger.Log("--------------------------------");
                        }
                    }
                }
            }

            // 1点もない場合
            if (dataList_Brige.Count == 0 &&
                dataList_Tunnel.Count == 0 &&
                dataList_Under.Count == 0 &&
                dataList.Count == 0)
            {
                //-------------------------------------
                // PBFファイルを削除する
                //-------------------------------------
                // フォルダが存在するか確認
                if (Directory.Exists(INPUT_PATH_PBF))
                {
                    Logger.Log("フォルダ削除");
                    // フォルダを削除
                    Directory.Delete(INPUT_PATH_PBF, true);
                }

                return 0;
            }

            int count = 0;

            //-------------------------------------
            // トンネルを反映する
            //-------------------------------------
            // 頂点をマージする
            mergePoints(dataList_Tunnel);
            Logger.Log("トンネルの登録開始： " + dataList_Tunnel.Count + "件");
            // 画面上に反映する
            draw(dataList_Tunnel, nodeMap);
            count += dataList_Tunnel.Count;
            dataList_Tunnel.Clear();

            //-------------------------------------
            // 地下を反映する
            //-------------------------------------
            // 頂点をマージする
            mergePoints(dataList_Under);
            Logger.Log("地下の登録開始： " + dataList_Under.Count + "件");
            // 画面上に反映する
            draw(dataList_Under, nodeMap);
            count += dataList_Under.Count;
            dataList_Under.Clear();

            //-------------------------------------
            // 橋・高架を反映する
            //-------------------------------------
            // 橋、高架の判定
            if (dataList_Brige.Count > 0)
            {
                //---------------------------
                // 河川との交差判定
                //---------------------------
                List<GmlWaterwayData> dataList_Water = GmlWaterwayData.ReadXML();
                // 橋の数ループ
                foreach (var data1 in dataList_Brige)
                {
                    // 河川の数ループ
                    foreach (var data2 in dataList_Water)
                    {
                        // 交差判定
                        if (CommonFunc.checkLineInPoly(data1.points, data2.points))
                        {
                            // 交差する
                            data1.isCrossWaterway = true;
                            break;
                        }
                    }
                }
                dataList_Water.Clear();

                //---------------------------
                // 道路との交差判定
                //---------------------------
                List<GmlRoadData> dataList_Road = GmlRoadData.ReadXML(false);
                //                List<GmlRoadData> dataList_Road = GmlRoadData
                // 橋の数ループ
                foreach (var data1 in dataList_Brige)
                {
                    if (data1.isCrossWaterway)
                    {
                        continue;
                    }

                    // 道路の数ループ
                    foreach (var data2 in dataList_Road)
                    {
                        // 交差判定
                        if (CommonFunc.checkLineInLine(data1.points, data2.points))
                        {
                            // 交差する
                            data1.isCrossRoad = true;
                            break;
                        }
                    }
                }
                dataList_Road.Clear();
            }
            // 頂点をマージする
            mergePoints(dataList_Brige);
            Logger.Log("橋・高架の登録開始： " + dataList_Brige.Count + "件");
            // 画面上に反映する
            draw(dataList_Brige, nodeMap);
            count += dataList_Brige.Count;
            dataList_Brige.Clear();

            //-------------------------------------
            // トンネル、橋・高架以外のデータを画面上に反映する
            //-------------------------------------
            Logger.Log("線路データの登録開始： " + dataList.Count + "件");
            draw(dataList, nodeMap);
            count += dataList.Count;
            dataList.Clear();

            //-------------------------------------
            // PBFファイルを削除する
            //-------------------------------------
            // フォルダが存在するか確認
            if (Directory.Exists(INPUT_PATH_PBF))
            {
                Logger.Log("フォルダ削除");
                // フォルダを削除
                Directory.Delete(INPUT_PATH_PBF, true);
            }

            return count;
        }

        /// <summary>
        /// 画面に描画
        /// </summary>
        private static void draw(List<GmlRailData> dataList, Dictionary<short, List<SimpleNode>> nodeMap)
        {
            //-------------------------------------
            // 画面上に反映する
            //-------------------------------------
            Randomizer rand = new Randomizer();
            SimulationManager sm = SimulationManager.instance;
            NetManager nm = NetManager.instance;
            TerrainManager tm = TerrainManager.instance;
            NetInfo ni;
            NetInfo ni1;
            NetInfo ni2;
            foreach (var segment in dataList)
            {
                if (segment.isRailTypeNormal)
                {
                    Logger.Log("線路タイプ：ノーマル");
                }
                else if (segment.isRailTypeSubway)
                {
                    Logger.Log("線路タイプ：地下鉄");
                }
                else if (segment.isRailTypeRoad)
                {
                    Logger.Log("線路タイプ：路面");
                }
                else if (segment.isRailTypeCableway)
                {
                    Logger.Log("線路タイプ：索道");
                }
                else if (segment.isRailTypeSpecial)
                {
                    Logger.Log("線路タイプ：その他");
                }

                if (IniFileData.Instance.logOut)
                {
                    if (!(segment.isBridge && !segment.isCrossWaterway))
                    {
                        Logger.Log("[T] ■頂点追加前");
                        Logger.Log("[T] 　　頂点数：" + segment.points.Count);
                        for (int idx = 0; idx < segment.points.Count; idx++)
                        {
                            Logger.Log("[T] 　　" + idx + "点目：" + segment.points[idx].x + " , " + segment.points[idx].y);
                        }
                    }
                }

                //----------------------------------------
                // タイプの判定
                //----------------------------------------
                ni = ni1 = ni2 = null;
                // アセットが対応しているか判定
                string assetName = "Train Track";
                string assetName_Slope = "Train Track Slope";
                string assetName_Tunnel = "Train Track Tunnel";
                // アセットが対応しているか判定
                string assetName_Subway = "Metro Track Ground 01";
                string assetName_SubwaySlope = "Metro Track Slope 01";
                string assetName_SubwayTunnel = "Metro Track";
                string assetName_SubwayBridge = "Metro Track Elevated 01";

                // 地下鉄の場合
                if (segment.isRailTypeSubway)
                {
                    if (segment.isBridge)
                    {
                        if (PrefabCollection<NetInfo>.LoadedExists(assetName_SubwayBridge))
                        {
                            ni = PrefabCollection<NetInfo>.FindLoaded(assetName_SubwayBridge);
                        }
                    }
                    else if (segment.isTunnel)
                    {
                        if (PrefabCollection<NetInfo>.LoadedExists(assetName_SubwaySlope) &&
                            PrefabCollection<NetInfo>.LoadedExists(assetName_SubwayTunnel))
                        {
                            // アセットの道路を用いる
                            ni1 = PrefabCollection<NetInfo>.FindLoaded(assetName_SubwaySlope);
                            ni2 = PrefabCollection<NetInfo>.FindLoaded(assetName_SubwayTunnel);
                        }
                    }
                    else if (segment.isUnderground)
                    {
                        if (PrefabCollection<NetInfo>.LoadedExists(assetName_SubwaySlope) &&
                            PrefabCollection<NetInfo>.LoadedExists(assetName_SubwayTunnel))
                        {
                            // アセットの道路を用いる
                            ni1 = PrefabCollection<NetInfo>.FindLoaded(assetName_SubwaySlope);
                            ni2 = PrefabCollection<NetInfo>.FindLoaded(assetName_SubwayTunnel);
                        }
                        /*
                                                if (PrefabCollection<NetInfo>.LoadedExists(assetName_SubwayTunnel))
                                                {
                                                    ni = PrefabCollection<NetInfo>.FindLoaded(assetName_SubwayTunnel);
                                                }
                        */
                    }
                    else
                    {
                        if (PrefabCollection<NetInfo>.LoadedExists(assetName_Subway))
                        {
                            ni = PrefabCollection<NetInfo>.FindLoaded(assetName_Subway);
                        }
                    }
                }
                /*
                                // モノレールの場合
                                else if (segment.isRailTypeSpecial)
                                {
                                    ni = PrefabCollection<NetInfo>.FindLoaded("Medium Road Monorail");
                                }
                */
                // 通常線路の場合
                else
                {
                    ni = PrefabCollection<NetInfo>.FindLoaded("Train Track");
                    if (segment.isTunnel)
                    {
                        if (PrefabCollection<NetInfo>.LoadedExists(assetName_Slope) &&
                            PrefabCollection<NetInfo>.LoadedExists(assetName_Tunnel))
                        {
                            // アセットの道路を用いる
                            ni1 = PrefabCollection<NetInfo>.FindLoaded(assetName_Slope);
                            ni2 = PrefabCollection<NetInfo>.FindLoaded(assetName_Tunnel);
                        }
                    }
                    else if (segment.isUnderground)
                    {
                        if (PrefabCollection<NetInfo>.LoadedExists(assetName_Slope) &&
                            PrefabCollection<NetInfo>.LoadedExists(assetName_Tunnel))
                        {
                            // アセットの道路を用いる
                            ni1 = PrefabCollection<NetInfo>.FindLoaded(assetName_Slope);
                            ni2 = PrefabCollection<NetInfo>.FindLoaded(assetName_Tunnel);
                        }
                    }
                    else if (segment.isBridge)
                    {
                        if (segment.isCrossWaterway)
                        {
                            // 橋
                            assetName += " Bridge";
                        }
                        else
                        {
                            // 高架
                            assetName += " Elevated";
                        }

                        if (PrefabCollection<NetInfo>.LoadedExists(assetName))
                        {
                            ni = PrefabCollection<NetInfo>.FindLoaded(assetName);
                        }
                    }
                    else
                    {
                        // トンネル、橋以外
                        if (PrefabCollection<NetInfo>.LoadedExists(assetName))
                        {
                            ni = PrefabCollection<NetInfo>.FindLoaded(assetName);
                        }
                    }
                }

                if (ni == null && ni1 == null && ni2 == null)
                {
                    Logger.Log("アセットなし");
                    continue;
                }

                //----------------------------------------
                // 線路の設定
                //----------------------------------------
                ushort startNetNodeId;
                ushort endNetNodeId;
                double baseHeightST = 0;
                double baseHeightED = 0;
                double baseHeightAdd = 0;
                double lineDist = 0;
                double calcDist = 0;
                double elevatedST = 0;      // 高架の頂上まで上りきる距離  (始点からこの距離までで勾配調整)
                double elevatedED = 0;      // 高架の頂上から降り始める距離(この距離から終点までで勾配調整)
                double elevatedDist = 0;    // 高架の長さ
                double elevatedH = 0;       // 高架の高さ
                double undergroundH = 0;    // 地下の深さ
                List<double> lineDistList = new List<double>();

                // トンネル、高架、橋の場合は高さを取得
                if (segment.isBridge || segment.isTunnel)
                {
                    // 地下の場合
                    if (segment.isUnderground)
                    {
                        // 地下の深さ
                        //                        undergroundH = 20 * mapExtent.areaScaleX; // mを地図上のスケールにに合わせる
                        undergroundH = 0;
                    }
                    //---------------------------------------
                    // トンネルの場合、始終点への頂点追加処理
                    //---------------------------------------
                    else if (segment.isTunnel)
                    {
                        // トンネルの始点終点はスロープを設置するため、距離が長すぎる場合には、頂点を補完する
                        double checkDist = 60 * MapExtent.Instance.areaScaleX; // 60mを地図上のスケールにに合わせる

                        // 始点の確認
                        double dist = CommonFunc.Dist2Point(segment.points[0], segment.points[1]);
                        if (dist > checkDist)
                        {
                            // 頂点を生成する
                            Vector2 newPos1 = CommonFunc.GetLinePos(segment.points[0], segment.points[1], checkDist);
                            // 頂点を追加する
                            segment.points.Insert(1, newPos1);

                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("[T]トンネル始点に頂点追加：" + dist + " | " + checkDist + " | ( " + newPos1.x + " , " + newPos1.y + " )");
                            }
                        }
                        // 終点の確認
                        dist = CommonFunc.Dist2Point(segment.points[segment.points.Count - 1], segment.points[segment.points.Count - 2]);
                        if (dist > checkDist)
                        {
                            // 頂点を生成する
                            Vector2 newPos2 = CommonFunc.GetLinePos(segment.points[segment.points.Count - 1], segment.points[segment.points.Count - 2], checkDist);
                            // 頂点を追加する
                            segment.points.Insert(segment.points.Count - 1, newPos2);

                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("[T]トンネル終点に頂点追加：" + dist + " | " + checkDist + " | ( " + newPos2.x + " , " + newPos2.y + " )");
                            }
                        }
                    }
                    //---------------------------------------
                    // 高架の場合、始終点の頂点を調整
                    //---------------------------------------
                    else if (segment.isBridge && !segment.isCrossWaterway)
                    {
                        // 高架の高さ
                        elevatedH = 7 * MapExtent.Instance.areaScaleX; // mを地図上のスケールにに合わせる

                        // 勾配調整用の距離
                        double checkDist = elevatedH * 3.0;

                        // 高架の全長を算出
                        for (int idx = 0; idx < segment.points.Count - 1; idx++)
                        {
                            elevatedDist += CommonFunc.Dist2Point(segment.points[idx], segment.points[idx + 1]);
                        }

                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[T] 高架の全長　　：" + elevatedDist);
                            Logger.Log("[T] 高架の勾配距離：" + checkDist);
                        }

                        // 高架を上げるまでの高さが確保されているか確認
                        if (elevatedDist < checkDist * 3)
                        {
                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("[T] 高架の距離が短い：" + elevatedDist + " < " + checkDist + " * 3 | " + (elevatedDist / 3));
                            }
                            // 高架が短いため勾配を調整する
                            checkDist = (elevatedDist / 3);

                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("[T] 高架の勾配距離：" + checkDist);
                            }
                        }


                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[T] ■頂点追加前");
                            Logger.Log("[T] 　　頂点数：" + segment.points.Count);
                            for (int idx = 0; idx < segment.points.Count; idx++)
                            {
                                Logger.Log("[T] 　　" + idx + "点目：" + segment.points[idx].x + " , " + segment.points[idx].y);
                            }
                        }

                        if (checkDist > 0.0)
                        {
                            // 高架の坂を上り切った位置に頂点追加（始点側）
                            double bf_dist = 0;
                            double tmp_dist = 0;
                            for (int idx = 0; idx < segment.points.Count - 1; idx++)
                            {
                                // 前回までの距離を保持
                                bf_dist = tmp_dist;
                                // 今回の距離を加算
                                tmp_dist += CommonFunc.Dist2Point(segment.points[idx], segment.points[idx + 1]);

                                if (IniFileData.Instance.logOut)
                                {
                                    Logger.Log("[T] 始点頂点の追加判定 " + idx + "点目" + "の合計距離：" + tmp_dist);
                                }

                                if (tmp_dist == checkDist)
                                {
                                    // 指定位置に頂点あり
                                    if (IniFileData.Instance.logOut)
                                    {
                                        Logger.Log("[T] 指定位置に頂点があるため追加不要");
                                    }
                                    break;
                                }
                                else if (tmp_dist > checkDist)
                                {
                                    // 頂点を生成する
                                    var newDist = checkDist - bf_dist;
                                    Vector2 newPos1 = CommonFunc.GetLinePos(segment.points[idx], segment.points[idx + 1], newDist);
                                    // 頂点を追加する
                                    segment.points.Insert(idx + 1, newPos1);

                                    if (IniFileData.Instance.logOut)
                                    {
                                        Logger.Log("[T] 高架に頂点追加（始点側）：" + newDist + " (" + newPos1.x + "," + newPos1.y + ")");
                                    }
                                    break;
                                }
                            }

                            // 高架の坂を上り切った位置に頂点追加（終点側）
                            bf_dist = 0;
                            tmp_dist = 0;
                            for (int idx = segment.points.Count - 2; idx >= 0; idx--)
                            {
                                // 前回までの距離を保持
                                bf_dist = tmp_dist;
                                // 今回の距離を加算
                                tmp_dist += CommonFunc.Dist2Point(segment.points[idx + 1], segment.points[idx]);

                                if (IniFileData.Instance.logOut)
                                {
                                    Logger.Log("[T] 終点頂点の追加判定 " + idx + "点目" + "の合計距離：" + tmp_dist);
                                }

                                if (tmp_dist == checkDist)
                                {
                                    // 指定位置に頂点あり
                                    if (IniFileData.Instance.logOut)
                                    {
                                        Logger.Log("[T] 指定位置に頂点があるため追加不要");
                                    }
                                    break;
                                }
                                else if (tmp_dist > checkDist)
                                {
                                    // 頂点を生成する
                                    var newDist = checkDist - bf_dist;
                                    Vector2 newPos1 = CommonFunc.GetLinePos(segment.points[idx + 1], segment.points[idx], newDist);
                                    // 頂点を追加する
                                    segment.points.Insert(idx + 1, newPos1);

                                    if (IniFileData.Instance.logOut)
                                    {
                                        Logger.Log("[T] 高架に頂点追加（終点側）：" + newDist + " (" + newPos1.x + "," + newPos1.y + ")");
                                    }
                                    break;
                                }
                            }

                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("[T] ■頂点追加後");
                                Logger.Log("[T] 　　頂点数：" + segment.points.Count);
                                for (int idx = 0; idx < segment.points.Count; idx++)
                                {
                                    Logger.Log("[T] 　　" + idx + "点目：" + segment.points[idx].x + " , " + segment.points[idx].y);
                                }
                            }
                        }
                        // 勾配調整用の距離を設定
                        elevatedST = checkDist;
                        elevatedED = elevatedDist - checkDist;
                    }

                    //---------------------------------------
                    // 距離を算出
                    //---------------------------------------
                    // ライン全体の長さ、頂点間の長さを取得
                    lineDist = CommonFunc.Dist2PointList(segment.points, out lineDistList);

                    //---------------------------------------
                    // 高さを算出
                    //---------------------------------------
                    // 始点の高さ
                    var tmpNodePos = new Vector3((float)segment.points[0].x, 0, (float)segment.points[0].y);
                    baseHeightST = tm.SampleRawHeightSmoothWithWater(tmpNodePos, false, 0f);
                    // 終点の高さ
                    tmpNodePos = new Vector3((float)segment.points[segment.points.Count - 1].x, 0, (float)segment.points[segment.points.Count - 1].y);
                    baseHeightED = tm.SampleRawHeightSmoothWithWater(tmpNodePos, false, 0f);
                    // 距離１辺りの増量を算出
                    baseHeightAdd = (baseHeightED - baseHeightST) / lineDist;

                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log("[T]始点の高さ　　　：" + baseHeightST);
                        Logger.Log("[T]終点の高さ　　　：" + baseHeightED);
                        Logger.Log("[T]増量　　　　　　：" + baseHeightAdd);
                        Logger.Log("[T]ライン全体の長さ：" + lineDist);
                    }
                }

                // 頂点数分ループ処理
                for (int i = 0; i < segment.points.Count - 1; i++)
                {
                    Vector2 stPoint = segment.points[i];
                    Vector2 edPoint = segment.points[i + 1];

                    // 同じ頂点なら処理しない
                    if (stPoint.x == edPoint.x && stPoint.y == edPoint.y)
                    {
                        continue;
                    }

                    //--------------------------------------
                    // ノードの生成処理
                    //--------------------------------------
                    // 始点のセルに頂点が１つもない場合
                    if (!SimpleNode.FindNode(nodeMap, out startNetNodeId, stPoint))
                    {
                        // トンネルの場合、始終点とそれ以外とでアセットが異なる
                        if (segment.isTunnel || segment.isUnderground)
                        {
                            if (i == 0)
                            {
                                // スロープ
                                ni = ni1;
                            }
                            else
                            {
                                // トンネル
                                ni = ni2;
                            }
                        }

                        //------------------------------
                        // 該当セルに頂点を生成する
                        //------------------------------
                        // Vector型に変換
                        var startNodePos = new Vector3((float)stPoint.x, 0, (float)stPoint.y);
                        // 高さを設定
                        startNodePos.y = tm.SampleRawHeightSmoothWithWater(startNodePos, false, 0f);
                        if (segment.isBridge || segment.isTunnel || segment.isUnderground)
                        {
                            startNodePos.y = (float)(baseHeightST);

                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("[T]　" + i + "点目の高さ[ST]：" + startNodePos.y);
                            }
                        }

                        // ノードを生成する
                        if (nm.CreateNode(out startNetNodeId, ref rand, ni, startNodePos, sm.m_currentBuildIndex))
                        {
                            sm.m_currentBuildIndex += 1u;
                        }

                        // 同一セルの判定用に値を保持する
                        short xRound = (short)Math.Round(stPoint.x);
                        if (!nodeMap.ContainsKey(xRound))
                        {
                            nodeMap.Add(xRound, new List<SimpleNode>());
                        }
                        SimpleNode simpleNode = new SimpleNode(startNetNodeId, startNodePos);
                        nodeMap[xRound].Add(simpleNode);
                    }
                    else
                    {
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[T]　" + i + "点目 [始点：登録済み]");
                        }
                    }

                    // 終点のセルに頂点が１つもない場合
                    if (!SimpleNode.FindNode(nodeMap, out endNetNodeId, edPoint))
                    {
                        // トンネルの場合、始終点とそれ以外とでアセットが異なる
                        if (segment.isTunnel || segment.isUnderground)
                        {
                            if (i == segment.points.Count - 2)
                            {
                                // スロープ
                                ni = ni1;
                            }
                            else
                            {
                                // トンネル
                                ni = ni2;
                            }
                        }

                        //------------------------------
                        // 該当セルに頂点を生成する
                        //------------------------------
                        // Vector型に変換
                        var endNodePos = new Vector3((float)edPoint.x, 0, (float)edPoint.y);
                        // 高さを設定
                        float yStart = tm.SampleRawHeightSmoothWithWater(endNodePos, false, 0f);
                        endNodePos.y = yStart;

                        // 地下の場合
                        if (segment.isUnderground)
                        {
                            // 最後の頂点以外は、地下の深さを反映する
                            if (i != segment.points.Count - 2)
                            {
                                // 地下の深さ
                                endNodePos.y -= (float)undergroundH;
                            }
                        }
                        else if (segment.isBridge || segment.isTunnel)
                        {
                            // 始点からの距離を設定
                            calcDist += lineDistList[i];

                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("[T]距離加算：" + calcDist + " | 増量：" + lineDistList[i]);
                            }

                            // 始終点で高さが均一となるよう調整
                            endNodePos.y = (float)(baseHeightST + (calcDist * baseHeightAdd));

                            // 高架の場合
                            if (segment.isBridge && !segment.isCrossWaterway)
                            {
                                // 勾配の頂上まで達していない場合
                                if (calcDist < elevatedST)
                                {
                                    var h = (elevatedH * (calcDist / elevatedST));

                                    if (IniFileData.Instance.logOut)
                                    {
                                        Logger.Log("[T] 頂上まで達していない：" + calcDist + " < " + elevatedST);
                                        Logger.Log("[T] 加算する高さ：" + h);
                                    }

                                    // 高架の頂上まで上り途中
                                    endNodePos.y += (float)(elevatedH * (calcDist / elevatedST));
                                }
                                else if (calcDist > elevatedED)
                                {
                                    var h = (elevatedH * ((elevatedDist - calcDist) / (elevatedDist - elevatedED)));

                                    if (IniFileData.Instance.logOut)
                                    {
                                        Logger.Log("[T] 頂上から下っている：" + calcDist + " > " + elevatedED);
                                        Logger.Log("[T] 加算する高さ：" + h);
                                    }

                                    // 高架の頂上から降り始める
                                    endNodePos.y += (float)(elevatedH * ((elevatedDist - calcDist) / (elevatedDist - elevatedED)));
                                }
                                else
                                {
                                    var h = elevatedH;

                                    if (IniFileData.Instance.logOut)
                                    {
                                        Logger.Log("[T] 頂上まで達している　：" + calcDist + " < " + elevatedED);
                                        Logger.Log("[T] 加算する高さ：" + h);
                                    }

                                    // 高架の頂上
                                    endNodePos.y += (float)elevatedH;
                                }
                            }

                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("[T]　" + (i + 1) + "点目の高さ[ED]：" + endNodePos.y);
                            }
                        }

                        // ノードの生成
                        if (nm.CreateNode(out endNetNodeId, ref rand, ni, endNodePos, sm.m_currentBuildIndex))
                        {
                            sm.m_currentBuildIndex += 1u;
                        }

                        // 同一セルの判定用に値を保持する
                        short xRound = (short)Math.Round(edPoint.x);
                        if (!nodeMap.ContainsKey(xRound))
                        {
                            nodeMap.Add(xRound, new List<SimpleNode>());
                        }
                        SimpleNode simpleNode = new SimpleNode(endNetNodeId, endNodePos);
                        nodeMap[xRound].Add(simpleNode);
                    }
                    else
                    {
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[T]　" + (i + 1) + "点目 [終点：登録済み]");
                        }
                    }

                    //--------------------------------------
                    // 登録処理
                    //--------------------------------------
                    if (segment.isTunnel || segment.isUnderground)
                    {
                        // トンネル入口の場合、アセットを正しく表示するためIDを逆転させる
                        if (i == 0)
                        {
                            var tmp = endNetNodeId;
                            endNetNodeId = startNetNodeId;
                            startNetNodeId = tmp;
                        }
                    }
                    //                        SimulationManager.instance.AddAction(AddRail(rand, ni, startNetNodeId, endNetNodeId, startDirection));
                    ushort segmentId;
                    Vector3 endPos = nm.m_nodes.m_buffer[endNetNodeId].m_position;
                    Vector3 startPos = nm.m_nodes.m_buffer[startNetNodeId].m_position;
                    Vector3 startDirection = VectorUtils.NormalizeXZ(endPos - startPos);
                    NetManager net_manager = NetManager.instance;
                    try
                    {
                        // トンネルの場合、始終点とそれ以外とでアセットが異なる
                        if (segment.isTunnel || segment.isUnderground)
                        {
                            if (i == 0 || i == segment.points.Count - 2)
                            {
                                // スロープ
                                ni = ni1;
                            }
                            else
                            {
                                // トンネル
                                ni = ni2;
                            }
                        }

                        // セグメントの生成
                        bool bRet = net_manager.CreateSegment(
                            out segmentId,
                            ref rand,
                            ni,
                            startNetNodeId,
                            endNetNodeId,
                            startDirection,
                            -startDirection,
                            Singleton<SimulationManager>.instance.m_currentBuildIndex,
                            Singleton<SimulationManager>.instance.m_currentBuildIndex,
                            false
                        );
                        // 生成成功時
                        if (bRet)
                        {
                            // インデックスを加算
                            Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                        }
                        else
                        {
                            Logger.Log("×画面への登録失敗");
                        }
                    }
                    catch (Exception ex)
                    {
                        //try-catch just to prevent crashing by ignoring invalid trees and letting valid trees get created
                        //RaiseTreeMapperEvent (ex.Message);
                        Logger.Log("エラー発生。：" + ex.Message);
                    }
                }
            }
        }

// 2023.09.11 G.Arakawa@cmind [PBFのダウンロード先変更対応] UPD_START
#if false
        /// <summary>
        /// 地理院ベクターファイルをダウンロード
        /// </summary>
        private static string getVectorTile(uint z, uint x, uint y)
        {
            var fileName = INPUT_PATH_PBF + "/" + z + "_" + x + "_" + y + ".pbf";
            var url = "http://cyberjapandata.gsi.go.jp/xyz/experimental_bvmap/" + z + "/" + x + "/" + y + ".pbf";

            // ファイルが存在するか確認
            if (Directory.Exists(fileName))
            {
                // 存在するためダウンロードしない
                return fileName;
            }

            Logger.Log("ダウンロード開始: " + url);
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls |
                    System.Net.SecurityProtocolType.Tls;
                System.Net.WebClient wc = new System.Net.WebClient();
                wc.DownloadFile(url, fileName);
                wc.Dispose();
            }
            catch
            {
                Logger.Log("ダウンロード失敗: " + url);
                return "";
            }
            return fileName;
        }
#endif
        /// <summary>
        /// 地理院ベクターファイルをダウンロード
        /// </summary>
        private static string getVectorTile(uint z, uint x, uint y)
        {
            var fileName = INPUT_PATH_PBF + "/" + z + "_" + x + "_" + y + ".pbf";
            var url = "https://cyberjapandata.gsi.go.jp/xyz/experimental_bvmap/" + z + "/" + x + "/" + y + ".pbf";

            // ファイルが存在するか確認
            if (Directory.Exists(fileName))
            {
                // 存在するためダウンロードしない
                return fileName;
            }

            Logger.Log("ダウンロード開始: " + url);
            try
            {
                UnityWebRequest www = UnityWebRequest.Get(url);
                www.Send();
                while (!www.isDone)
                {
                    // ダウンロード中
                }
                File.WriteAllBytes(fileName, www.downloadHandler.data);
                Debug.Log("done.");
            }
            catch (Exception ex)
            {
                Logger.Log("ダウンロード失敗: " + url);
                Logger.Log("エラー発生。：" + ex.Message);
                Logger.Log("エラー発生。：" + ex);
                return "";
            }
            return fileName;
        }
// 2023.09.11 G.Arakawa@cmind [PBFのダウンロード先変更対応] UPD_END

        /// <summary>
        /// 線路の始終点を比較し、同一の場合には１つのラインとする
        /// </summary>
        private static void mergePoints(List<GmlRailData> dataList)
        {
            bool chkFlag = false;

            while (true)
            {
                int dataCnt = dataList.Count;
                for (int idx = 0; idx < dataCnt; idx++)
                {
                    for (int idx2 = 0; idx2 < dataCnt; idx2++)
                    {
                        if (idx == idx2)
                        {
                            continue;
                        }

                        // 橋・高架の種類が異なる場合
                        if (dataList[idx].isCrossWaterway != dataList[idx2].isCrossWaterway)
                        {
                            continue;
                        }

                        // 線路の種類が同じ場合（通常線路と特殊線路などをマージしないように）
                        if (dataList[idx].isRailTypeNormal != dataList[idx2].isRailTypeNormal ||
                            dataList[idx].isRailTypeCableway != dataList[idx2].isRailTypeCableway ||
                            dataList[idx].isRailTypeRoad != dataList[idx2].isRailTypeRoad ||
                            dataList[idx].isRailTypeSpecial != dataList[idx2].isRailTypeSpecial ||
                            dataList[idx].isRailTypeSubway != dataList[idx2].isRailTypeSubway)
                        {
                            continue;
                        }

                        // 始点と終点が同じ場合
                        if (dataList[idx].points[0].x == dataList[idx2].points[dataList[idx2].points.Count - 1].x &&
                            dataList[idx].points[0].y == dataList[idx2].points[dataList[idx2].points.Count - 1].y)
                        {
                            // 頂点をマージ
                            dataList[idx2].points.AddRange(dataList[idx].points);
                            // マージしたデータを削除する
                            dataList.RemoveAt(idx);
                            // データを削除したためカウント調整
                            idx -= 1;
                            dataCnt -= 1;
                            chkFlag = true;
                            break;
                        }
                        // 終点と始点が同じ場合
                        if (dataList[idx].points[dataList[idx].points.Count - 1].x == dataList[idx2].points[0].x &&
                            dataList[idx].points[dataList[idx].points.Count - 1].y == dataList[idx2].points[0].y)
                        {
                            // 頂点をマージ
                            dataList[idx].points.AddRange(dataList[idx2].points);
                            dataList[idx2].points.Clear();
                            dataList[idx2].points.AddRange(dataList[idx].points);
                            // マージしたデータを削除する
                            dataList.RemoveAt(idx);
                            // データを削除したためカウント調整
                            idx -= 1;
                            dataCnt -= 1;
                            chkFlag = true;
                            break;
                        }
                    }
                }
                // 1件もマージしていない場合
                if (!chkFlag)
                {
                    break;
                }
                chkFlag = false;
            }
        }

        /// <summary>
        /// Geometryの座標位置を緯度経度に変換
        /// </summary>
        private static Vector2 convTile2Point(int tileX, int tileY, ulong zoom, ulong tileCol, ulong tileRow, ulong extent)
        {
            //--------------------------------------------------------------
            // 座標系の変換
            // https://qiita.com/tohka383/items/2a3588a9f5bf34cbb7d0
            //--------------------------------------------------------------
            // EPSG:3857
            double S = 40075016.685;
            double x = (((double)tileX / (double)extent) + (tileCol - Math.Pow(2, zoom - 1))) * (S / Math.Pow(2, zoom));
            double y = (((double)tileY / (double)extent) + (tileRow - Math.Pow(2, zoom - 1))) * (S / Math.Pow(2, zoom)) * -1;

            return new Vector2((float)x, (float)y);
        }

        /// <summary>
        /// ファイル名からズームレベル、タイル番号などを取得するメソッド
        /// </summary>
        private static bool parseArg(string fileName, out ulong zoom, out ulong tileCol, out ulong tileRow)
        {
            zoom = 0;
            tileCol = 0;
            tileRow = 0;

            // 拡張子を取り除く
            string zxyTxt = fileName.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
            // 分割する
            string[] zxy = zxyTxt.Split("_".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (zxy.Length != 3)
            {
                Console.WriteLine("invalid zoom, tileCol or tileRow [{0}]", zxyTxt);
                return false;
            }

            ulong z;
            if (!ulong.TryParse(zxy[0], out z))
            {
                //                Console.WriteLine($"could not parse zoom: {zxy[0]}");
                return false;
            }
            zoom = z;

            ulong x;
            if (!ulong.TryParse(zxy[1], out x))
            {
                //                Console.WriteLine($"could not parse tileCol: {zxy[1]}");
                return false;
            }
            tileCol = x;

            ulong y;
            if (!ulong.TryParse(zxy[2], out y))
            {
                //                Console.WriteLine($"could not parse tileRow: {zxy[2]}");
                return false;
            }
            tileRow = y;

            return true;
        }
    }
}
