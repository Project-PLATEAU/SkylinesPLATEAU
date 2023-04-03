using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using System;
//----------------------------------------------------------------------------
// GmlRoadData.cs
//
// ■概要
//      道路を管理するクラス
//         https://maps.gsi.go.jp/help/pdf/vector/attribute.pdf
// 
//
//----------------------------------------------------------------------------
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using UnityEngine;

namespace SkylinesPlateau
{
    /// <summary>
    /// 道路同士の繋がり管理クラス
    /// </summary>
    public class RoadConnect
    {
        // 点群の要素番号
        public int posIdx1;
        public int posIdx2;
        // 繋がっている物の要素番号
        public int polyIdx_self;
        public int polyIdx_aite;

        public RoadConnect(int idx1, int idx2, int idx3, int idx4)
        {
            posIdx1 = idx1 < idx2 ? idx1 : idx2;
            posIdx2 = idx1 < idx2 ? idx2 : idx1;
            polyIdx_self = idx3;
            polyIdx_aite = idx4;
        }
        /// <summary>
        /// データの重複判定
        /// </summary>
        public bool isDuplicate(RoadConnect rc)
        {
            // 対象道路が一致しているか判定
            if ((this.polyIdx_self == rc.polyIdx_self || this.polyIdx_self == rc.polyIdx_aite) &&
                (this.polyIdx_aite == rc.polyIdx_self || this.polyIdx_aite == rc.polyIdx_aite))
            {
                // 対象頂点が一致しているか判定
                if ((this.posIdx1 == rc.posIdx1 || this.posIdx1 == rc.posIdx2) &&
                    (this.posIdx2 == rc.posIdx1 || this.posIdx2 == rc.posIdx2))
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// 垂線データの管理クラス
    /// </summary>
    public class PerpendData
    {
        // 垂線の始終点
        public Vector3 stPoint;
        public Vector3 edPoint;
        // 垂線の長さ
        public double width;

        // 垂線の生成元の点群リスト要素番号
        public int posIdx1;
        public int posIdx2;
        // 垂線の交差先の要素番号
        public int posIdxAite1;
        public int posIdxAite2;

        // 始終点の垂線か判定用
        public bool isStartEnd;
        // 削除対象か判定用
        public bool isRemove;
        // 中心点の算出判定済みか判断用
        public bool isCreatedCenterPos;
    }

    /// <summary>
    /// 中心点を求める際、入口からの距離でソートに利用する一時クラス
    /// </summary>
    public class PerpendSort
    {
        // 対象垂線のインデックス番号
        public int idx;
        // 基準点からの距離
        public double dist;

        public PerpendSort(int idx, double dist)
        {
            this.idx = idx;
            this.dist = dist;
        }
    }

    /// <summary>
    /// 道路情報を管理するクラス
    /// </summary>
    public class GmlRoadData
    {
        //-------------------------------------
        // 固定値 ( 列挙型 )
        //-------------------------------------
        /// <summary>
        /// 「道路区分(tran:function)」のコード
        /// </summary>
        private enum TAG_CODE_FUNCTION
        {
            /// <summary>
            /// 高速道路
            /// </summary>
            highway = 1
        }
        /// <summary>
        /// 「道路構造の種別 (uro:sectionType)」のコード
        /// </summary>
        private enum TAG_CODE_SECTIONTYPE
        {
            /// <summary>
            /// 1:土木区間
            /// </summary>
            construction = 1,
            /// <summary>
            /// 2:高架橋
            /// </summary>
            elevated = 2,
            /// <summary>
            /// 3:橋梁
            /// </summary>
            bridge = 3,
            /// <summary>
            /// 4:交差部
            /// </summary>
            crossing = 4,
            /// <summary>
            /// 5:アンダーパス
            /// </summary>
            underpass = 5,
            /// <summary>
            /// 6:トンネル
            /// </summary>
            tunnel = 6
        }
        /// <summary>
        /// 「幅員区分（uro:widthType）」のコード
        /// </summary>
        private enum TAG_CODE_WIDTHTYPE
        {
            /// <summary>
            /// 15m 以上
            /// </summary>
            m15 = 1,
            /// <summary>
            /// 6m 以上 15m 未満
            /// </summary>
            m6_15 = 2,
            /// <summary>
            /// 4m 以上 6m 未満
            /// </summary>
            m4_6 = 3,
            /// <summary>
            /// 4m 未満
            /// </summary>
            m4 = 4
        }
        /// <summary>
        /// 幅員のアセット判定に用いる値
        /// </summary>
        private enum WIDTH_VALUE
        {
            /// <summary>
            /// 優先順1（24m）
            /// </summary>
            priority1 = 24,
            /// <summary>
            /// 優先順2（15m）
            /// </summary>
            priority2 = 15,
            /// <summary>
            /// 優先順3（6m）
            /// </summary>
            priority3 = 6,
            /// <summary>
            /// 優先順4（4m）
            /// </summary>
            priority4 = 4
        }

        //-------------------------------------
        // 固定値 ( アセット名 )
        //-------------------------------------
        private const string DEF_NAME_BRIGE = " Bridge";
        private const string DEF_NAME_ELEVATED = " Elevated";
        private const string DEF_NAME_SLOPE = " Slope";
        private const string DEF_NAME_TUNNEL = " Tunnel";
        // 高速道路
        private const string PREFAB_NAME_HIGHWAY = "Highway";
        private const string PREFAB_NAME_HIGHWAY_E = PREFAB_NAME_HIGHWAY + DEF_NAME_ELEVATED;
        private const string PREFAB_NAME_HIGHWAY_B = PREFAB_NAME_HIGHWAY + DEF_NAME_BRIGE;
        private const string PREFAB_NAME_HIGHWAY_S = PREFAB_NAME_HIGHWAY + DEF_NAME_SLOPE;
        private const string PREFAB_NAME_HIGHWAY_T = PREFAB_NAME_HIGHWAY + DEF_NAME_TUNNEL;
        private const string PREFAB_NAME_ASSET_HIGHWAY = "2576525166.National Road_Data";
        private const string PREFAB_NAME_ASSET_HIGHWAY_E = "2576525166.National Road Elevated0";
        private const string PREFAB_NAME_ASSET_HIGHWAY_B = "2576525166.National Road Bridge0";
        private const string PREFAB_NAME_ASSET_HIGHWAY_S = "2576525166.National Road Slope0";
        private const string PREFAB_NAME_ASSET_HIGHWAY_T = "2576525166.National Road Tunnel0";
        // 24m 以上
        private const string PREFAB_NAME_M24 = "Large Road Decoration Trees";
        private const string PREFAB_NAME_M24_E = "Large Road" + DEF_NAME_ELEVATED;
        private const string PREFAB_NAME_M24_B = "Large Road" + DEF_NAME_BRIGE;
        private const string PREFAB_NAME_M24_S = "Large Road" + DEF_NAME_SLOPE;
        private const string PREFAB_NAME_M24_T = "Large Road" + DEF_NAME_TUNNEL;
        private const string PREFAB_NAME_ASSET_M24 = "2063122767.JP 6L Medium Roads KR6102_Data";
        private const string PREFAB_NAME_ASSET_M24_E = "2063122767.Medium Road Elevated0";
        private const string PREFAB_NAME_ASSET_M24_B = "2063122767.Medium Road Elevated1";
        private const string PREFAB_NAME_ASSET_M24_S = "2063122767.Medium Road Slope0";
        private const string PREFAB_NAME_ASSET_M24_T = "2063122767.Medium Road Tunnel0";
        // 15m 以上 24m 未満
        private const string PREFAB_NAME_M15_24 = "Basic Road";
        private const string PREFAB_NAME_M15_24_E = PREFAB_NAME_M15_24 + DEF_NAME_ELEVATED;
        private const string PREFAB_NAME_M15_24_B = PREFAB_NAME_M15_24 + DEF_NAME_BRIGE;
        private const string PREFAB_NAME_M15_24_S = PREFAB_NAME_M15_24 + DEF_NAME_SLOPE;
        private const string PREFAB_NAME_M15_24_T = PREFAB_NAME_M15_24 + DEF_NAME_TUNNEL;
        private const string PREFAB_NAME_ASSET_M15_24 = "2061610175.JP 4L Medium Roads KR4104_Data";
        private const string PREFAB_NAME_ASSET_M15_24_E = "2061610175.Medium Road Elevated0";
        private const string PREFAB_NAME_ASSET_M15_24_B = "2061610175.Medium Road Bridge0";
        private const string PREFAB_NAME_ASSET_M15_24_S = "2061610175.Medium Road Slope0";
        private const string PREFAB_NAME_ASSET_M15_24_T = "2061610175.Medium Road Tunnel0";
        // 6m 以上 15m 未満
        private const string PREFAB_NAME_M6_15 = "Basic Road";
        private const string PREFAB_NAME_M6_15_E = PREFAB_NAME_M6_15 + DEF_NAME_ELEVATED;
        private const string PREFAB_NAME_M6_15_B = PREFAB_NAME_M6_15 + DEF_NAME_BRIGE;
        private const string PREFAB_NAME_M6_15_S = PREFAB_NAME_M6_15 + DEF_NAME_SLOPE;
        private const string PREFAB_NAME_M6_15_T = PREFAB_NAME_M6_15 + DEF_NAME_TUNNEL;
        private const string PREFAB_NAME_ASSET_M6_15 = "2392987553.1u Two-Lane Two-Way Road NP_Data";
        private const string PREFAB_NAME_ASSET_M6_15_E = "2392987553.Basic Road Elevated0";
        private const string PREFAB_NAME_ASSET_M6_15_B = "2392987553.Basic Road Bridge0";
        private const string PREFAB_NAME_ASSET_M6_15_S = "2392987553.Basic Road Slope0";
        private const string PREFAB_NAME_ASSET_M6_15_T = "2392987553.Basic Road Tunnel0";
        // 4m 以上 6m 未満
        private const string PREFAB_NAME_M4_6 = "Basic Road";
        private const string PREFAB_NAME_M4_6_E = PREFAB_NAME_M4_6 + DEF_NAME_ELEVATED;
        private const string PREFAB_NAME_M4_6_B = PREFAB_NAME_M4_6 + DEF_NAME_BRIGE;
        private const string PREFAB_NAME_M4_6_S = PREFAB_NAME_M4_6 + DEF_NAME_SLOPE;
        private const string PREFAB_NAME_M4_6_T = PREFAB_NAME_M4_6 + DEF_NAME_TUNNEL;
        private const string PREFAB_NAME_ASSET_M4_6 = "1864625632.JP 5m Tiny Roads +ped KT203_Data";
        private const string PREFAB_NAME_ASSET_M4_6_E = "1864625632.Basic Road Elevated0";
        private const string PREFAB_NAME_ASSET_M4_6_B = "1864625632.Basic Road Elevated4";
        private const string PREFAB_NAME_ASSET_M4_6_S = ""; // 未対応
        private const string PREFAB_NAME_ASSET_M4_6_T = ""; // 未対応
        // 4m 未満
        private const string PREFAB_NAME_M4 = "Basic Road";
        private const string PREFAB_NAME_M4_E = PREFAB_NAME_M4 + DEF_NAME_ELEVATED;
        private const string PREFAB_NAME_M4_B = PREFAB_NAME_M4 + DEF_NAME_BRIGE;
        private const string PREFAB_NAME_M4_S = PREFAB_NAME_M4 + DEF_NAME_SLOPE;
        private const string PREFAB_NAME_M4_T = PREFAB_NAME_M4 + DEF_NAME_TUNNEL;
        private const string PREFAB_NAME_ASSET_M4 = "1864625841.JP 5m Tiny Roads KT202_Data";
        private const string PREFAB_NAME_ASSET_M4_E = "1864625841.Basic Road Elevated2";
        private const string PREFAB_NAME_ASSET_M4_B = "1864625841.Basic Road Elevated3";
        private const string PREFAB_NAME_ASSET_M4_S = ""; // 未対応
        private const string PREFAB_NAME_ASSET_M4_T = ""; // 未対応

        //-------------------------------------
        // 固定値
        //-------------------------------------
        public const string INPUT_PATH = @"Files/SkylinesPlateau/in";
        public const string INPUT_PATH2 = @"/udx/tran";

        //-------------------------------------
        // メンバ変数
        //-------------------------------------
        // 形状ポリゴン
        public List<Vector3> points = new List<Vector3>();
        // 道路区分
        public int function = -1;
        // 道路構造の種別
        public int sectionType = -1;
        // 幅員区分
        public int widthType = -1;
        // 幅員(タグから取得)
        public double widthTag = 0.0;
        // 幅員(形状ポリゴンから算出)
        public double widthPoly = 0.0;
        // 道路名称
        public string name = "";

        // 道路ポリゴンの範囲
        public Vector2 areaMax = new Vector2(float.MinValue, float.MinValue);
        public Vector2 areaMin = new Vector2(float.MaxValue, float.MaxValue);

        // 道路同士の繋がり
        // ※別道路と繋がっている、pointsの要素番号を格納
        public List<RoadConnect> connectionList = new List<RoadConnect>();

        // 道路の中心線
        public List<Vector3> centerLineList = new List<Vector3>();

        //-------------------------------------
        // ReadOnly
        //-------------------------------------
        // 幅員を取得
        public double width {
            get
            {
                //---------------------------------
                // WIDTHタグの判定
                //---------------------------------
                if (widthTag >= 0)
                {
                    return widthTag;
                }
                //---------------------------------
                // WIDTHTYPEタグの判定
                //---------------------------------
                TAG_CODE_WIDTHTYPE value = (TAG_CODE_WIDTHTYPE)widthType;
                if (widthType >= 0)
                {
                    if (value == TAG_CODE_WIDTHTYPE.m15)
                    {
                        return 16.0;
                    }
                    else if (value == TAG_CODE_WIDTHTYPE.m6_15)
                    {
                        return 10.5;
                    }
                    else if (value == TAG_CODE_WIDTHTYPE.m4_6)
                    {
                        return 5.0;
                    }
                    else if (value == TAG_CODE_WIDTHTYPE.m4)
                    {
                        return 3.0;
                    }
                }
                //---------------------------------
                // 形状ポリゴンから算出した幅員で判定
                //---------------------------------
                if (widthPoly >= 0)
                {
                    return widthPoly;
                }

                return -1;
            }
        }

        // アセット名を取得
        public string prefab_name
        {
            get
            {
                //---------------------------------
                // 追加アセットを取得
                //---------------------------------
                string assetName = this.prefab_name_asset;
                if (assetName != "" && PrefabCollection<NetInfo>.LoadedExists(assetName))
                {
                    // 追加アセットが存在するので利用
                    return assetName;
                }

                //---------------------------------
                // 高速道路の場合
                //---------------------------------
                if (isHighWay)
                {
                    if (isBridge)
                    {
                        // 橋
                        return PREFAB_NAME_HIGHWAY_B;
                    }
                    else if (isElevated)
                    {
                        // 高架
                        return PREFAB_NAME_HIGHWAY_E;
                    }
                    // 通常道路
                    return PREFAB_NAME_HIGHWAY;
                }

                //---------------------------------
                // 幅員判定
                //---------------------------------
                double w = this.width;
                if (w >= 0)
                {
                    if (w > (double)WIDTH_VALUE.priority1)
                    {
                        if (isBridge)
                        {
                            // 橋
                            return PREFAB_NAME_M24_B;
                        }
                        else if (isElevated)
                        {
                            // 高架
                            return PREFAB_NAME_M24_E;
                        }
                        // 通常道路
                        return PREFAB_NAME_M24;
                    }
                    else if (w > (double)WIDTH_VALUE.priority2)
                    {
                        if (isBridge)
                        {
                            // 橋
                            return PREFAB_NAME_M15_24_B;
                        }
                        else if (isElevated)
                        {
                            // 高架
                            return PREFAB_NAME_M15_24_E;
                        }
                        // 通常道路
                        return PREFAB_NAME_M15_24;
                    }
                    else if (w > (double)WIDTH_VALUE.priority3)
                    {
                        if (isBridge)
                        {
                            // 橋
                            return PREFAB_NAME_M6_15_B;
                        }
                        else if (isElevated)
                        {
                            // 高架
                            return PREFAB_NAME_M6_15_E;
                        }
                        // 通常道路
                        return PREFAB_NAME_M6_15;
                    }
                    else if (w > (double)WIDTH_VALUE.priority4)
                    {
                        if (isBridge)
                        {
                            // 橋
                            return PREFAB_NAME_M4_6_B;
                        }
                        else if (isElevated)
                        {
                            // 高架
                            return PREFAB_NAME_M4_6_E;
                        }
                        // 通常道路
                        return PREFAB_NAME_M4_6;
                    }
                    else
                    {
                        if (isBridge)
                        {
                            // 橋
                            return PREFAB_NAME_M4_B;
                        }
                        else if (isElevated)
                        {
                            // 高架
                            return PREFAB_NAME_M4_E;
                        }
                        // 通常道路
                        return PREFAB_NAME_M4;
                    }
                }

                return "";
            }
        }
        // アセット名を取得（トンネルのスロープ）
        public string prefab_name_Slope
        {
            get
            {
                if (!isTunnel)
                {
                    // トンネル形状ではない
                    return "";
                }

                //---------------------------------
                // 追加アセットを取得
                //---------------------------------
                string assetName = this.prefab_name_asset_Slope;
                if (assetName != "" && PrefabCollection<NetInfo>.LoadedExists(assetName))
                {
                    // 追加アセットが存在するので利用
                    return assetName;
                }

                //---------------------------------
                // 高速道路の場合
                //---------------------------------
                if (isHighWay)
                {
                    return PREFAB_NAME_HIGHWAY_S;
                }

                //---------------------------------
                // 幅員判定
                //---------------------------------
                double w = this.width;
                if (w >= 0)
                {
                    if (w > (double)WIDTH_VALUE.priority1)
                    {
                        return PREFAB_NAME_M24_S;
                    }
                    else if (w > (double)WIDTH_VALUE.priority2)
                    {
                        return PREFAB_NAME_M15_24_S;
                    }
                    else if (w > (double)WIDTH_VALUE.priority3)
                    {
                        return PREFAB_NAME_M6_15_S;
                    }
                    else if (w > (double)WIDTH_VALUE.priority4)
                    {
                        return PREFAB_NAME_M4_6_S;
                    }
                    else
                    {
                        return PREFAB_NAME_M4_S;
                    }
                }

                return "";
            }
        }
        // アセット名を取得（トンネル）
        public string prefab_name_Tunnel
        {
            get
            {
                if (!isTunnel)
                {
                    // トンネル形状ではない
                    return "";
                }

                //---------------------------------
                // 追加アセットを取得
                //---------------------------------
                string assetName = this.prefab_name_asset_Tunnel;
                if (assetName != "" && PrefabCollection<NetInfo>.LoadedExists(assetName))
                {
                    // 追加アセットが存在するので利用
                    return assetName;
                }

                //---------------------------------
                // 高速道路の場合
                //---------------------------------
                if (isHighWay)
                {
                    return PREFAB_NAME_HIGHWAY_T;
                }

                //---------------------------------
                // 幅員判定
                //---------------------------------
                double w = this.width;
                if (w >= 0)
                {
                    if (w > (double)WIDTH_VALUE.priority1)
                    {
                        return PREFAB_NAME_M24_T;
                    }
                    else if (w > (double)WIDTH_VALUE.priority2)
                    {
                        return PREFAB_NAME_M15_24_T;
                    }
                    else if (w > (double)WIDTH_VALUE.priority3)
                    {
                        return PREFAB_NAME_M6_15_T;
                    }
                    else if (w > (double)WIDTH_VALUE.priority4)
                    {
                        return PREFAB_NAME_M4_6_T;
                    }
                    else
                    {
                        return PREFAB_NAME_M4_T;
                    }
                }

                return "";
            }
        }

        // 追加アセットの名称取得
        private string prefab_name_asset
        {
            get
            {
                //---------------------------------
                // 高速道路の場合
                //---------------------------------
                if (isHighWay)
                {
                    if (isBridge)
                    {
                        // 橋
                        return PREFAB_NAME_ASSET_HIGHWAY_B;
                    }
                    else if (isElevated)
                    {
                        // 高架
                        return PREFAB_NAME_ASSET_HIGHWAY_E;
                    }
                    // 通常道路
                    return PREFAB_NAME_ASSET_HIGHWAY;
                }

                //---------------------------------
                // 幅員判定
                //---------------------------------
                double w = this.width;
                if (w >= 0)
                {
                    if (w > (double)WIDTH_VALUE.priority1)
                    {
                        if (isBridge)
                        {
                            // 橋
                            return PREFAB_NAME_ASSET_M24_B;
                        }
                        else if (isElevated)
                        {
                            // 高架
                            return PREFAB_NAME_ASSET_M24_E;
                        }
                        // 通常道路
                        return PREFAB_NAME_ASSET_M24;
                    }
                    else if (w > (double)WIDTH_VALUE.priority2)
                    {
                        if (isBridge)
                        {
                            // 橋
                            return PREFAB_NAME_ASSET_M15_24_B;
                        }
                        else if (isElevated)
                        {
                            // 高架
                            return PREFAB_NAME_ASSET_M15_24_E;
                        }
                        // 通常道路
                        return PREFAB_NAME_ASSET_M15_24;
                    }
                    else if (w > (double)WIDTH_VALUE.priority3)
                    {
                        if (isBridge)
                        {
                            // 橋
                            return PREFAB_NAME_ASSET_M6_15_B;
                        }
                        else if (isElevated)
                        {
                            // 高架
                            return PREFAB_NAME_ASSET_M6_15_E;
                        }
                        // 通常道路
                        return PREFAB_NAME_ASSET_M6_15;
                    }
                    else if (w > (double)WIDTH_VALUE.priority4)
                    {
                        if (isBridge)
                        {
                            // 橋
                            return PREFAB_NAME_ASSET_M4_6_B;
                        }
                        else if (isElevated)
                        {
                            // 高架
                            return PREFAB_NAME_ASSET_M4_6_E;
                        }
                        // 通常道路
                        return PREFAB_NAME_ASSET_M4_6;
                    }
                    else
                    {
                        if (isBridge)
                        {
                            // 橋
                            return PREFAB_NAME_ASSET_M4_B;
                        }
                        else if (isElevated)
                        {
                            // 高架
                            return PREFAB_NAME_ASSET_M4_E;
                        }
                        // 通常道路
                        return PREFAB_NAME_ASSET_M4;
                    }
                }

                return "";
            }
        }
        // 追加アセットの名称取得（トンネルのスロープ）
        private string prefab_name_asset_Slope
        {
            get
            {
                if (!isTunnel)
                {
                    // トンネル形状ではない
                    return "";
                }

                //---------------------------------
                // 高速道路の場合
                //---------------------------------
                if (isHighWay)
                {
                    return PREFAB_NAME_ASSET_HIGHWAY_S;
                }

                //---------------------------------
                // 幅員判定
                //---------------------------------
                double w = this.width;
                if (w >= 0)
                {
                    if (w > (double)WIDTH_VALUE.priority1)
                    {
                        return PREFAB_NAME_ASSET_M24_S;
                    }
                    else if (w > (double)WIDTH_VALUE.priority2)
                    {
                        return PREFAB_NAME_ASSET_M15_24_S;
                    }
                    else if (w > (double)WIDTH_VALUE.priority3)
                    {
                        return PREFAB_NAME_ASSET_M6_15_S;
                    }
                    else if (w > (double)WIDTH_VALUE.priority4)
                    {
                        return PREFAB_NAME_ASSET_M4_6_S;
                    }
                    else
                    {
                        return PREFAB_NAME_ASSET_M4_S;
                    }
                }

                return "";
            }
        }
        // 追加アセットの名称取得（トンネル）
        private string prefab_name_asset_Tunnel
        {
            get
            {
                if (!isTunnel)
                {
                    // トンネル形状ではない
                    return "";
                }

                //---------------------------------
                // 高速道路の場合
                //---------------------------------
                if (isHighWay)
                {
                    return PREFAB_NAME_ASSET_HIGHWAY_T;
                }

                //---------------------------------
                // 幅員判定
                //---------------------------------
                double w = this.width;
                if (w >= 0)
                {
                    if (w > (double)WIDTH_VALUE.priority1)
                    {
                        return PREFAB_NAME_ASSET_M24_T;
                    }
                    else if (w > (double)WIDTH_VALUE.priority2)
                    {
                        return PREFAB_NAME_ASSET_M15_24_T;
                    }
                    else if (w > (double)WIDTH_VALUE.priority3)
                    {
                        return PREFAB_NAME_ASSET_M6_15_T;
                    }
                    else if (w > (double)WIDTH_VALUE.priority4)
                    {
                        return PREFAB_NAME_ASSET_M4_6_T;
                    }
                    else
                    {
                        return PREFAB_NAME_ASSET_M4_T;
                    }
                }

                return "";
            }
        }

        /// <summary>
        /// 高速道路か判定用フラグ
        /// </summary>
        public bool isHighWay
        {
            get
            {
                // 高速道路の場合
                if ((TAG_CODE_FUNCTION)function == TAG_CODE_FUNCTION.highway)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 橋か判定用フラグ
        /// </summary>
        public bool isBridge
        {
            get
            {
                TAG_CODE_SECTIONTYPE value = (TAG_CODE_SECTIONTYPE)this.sectionType;
                // 橋として判断するコードの場合
                if (value == TAG_CODE_SECTIONTYPE.bridge)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 高架か判定用フラグ
        /// </summary>
        public bool isElevated
        {
            get
            {
                TAG_CODE_SECTIONTYPE value = (TAG_CODE_SECTIONTYPE)this.sectionType;
                // 橋として判断するコードの場合
                if (value == TAG_CODE_SECTIONTYPE.elevated)
                {
                    return true;
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
                TAG_CODE_SECTIONTYPE value = (TAG_CODE_SECTIONTYPE)this.sectionType;
                // 橋として判断するコードの場合
                if (value == TAG_CODE_SECTIONTYPE.tunnel ||
                    value == TAG_CODE_SECTIONTYPE.underpass)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 交差点か判定用フラグ
        /// </summary>
        public bool isCrossing
        {
            get
            {
                // 別道路と３つ以上、接している場合には交差点
                if (connectionList.Count > 2)
                {
                    return true;
                }
                // タグの指定が交差点の場合
                TAG_CODE_SECTIONTYPE value = (TAG_CODE_SECTIONTYPE)this.sectionType;
                if (value == TAG_CODE_SECTIONTYPE.crossing)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 交差点の中心座標
        /// </summary>
        public Vector3 CrossingPos
        {
            get
            {
                // 算出していない場合
                if (_CrossingPos.x == float.MinValue)
                {
#if true
                    //----------------------------------------------------------------
                    // T字路の場合
                    //----------------------------------------------------------------
                    if (this.connectionList.Count == 3)
                    {
                        // 交差点の入口から降ろした垂線が、別の入り口と交差するか判定。
                        // ※別の入り口と交差する場合、T字路として判断
                        for (int i = 0; i < connectionList.Count; i++)
                        {
                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("[" + i + "] ST : " + points[connectionList[i].posIdx1].x + "," + points[connectionList[i].posIdx1].y);
                                Logger.Log("[" + i + "] ED : " + points[connectionList[i].posIdx2].x + "," + points[connectionList[i].posIdx2].y);
                            }
                            // 中心点を算出
                            Vector3 centerPos = CommonFunc.GetLineCenterPos(points[connectionList[i].posIdx1], points[connectionList[i].posIdx2]);
                            // 中心点からの垂線 (直線)
                            Vector3 perpendicularPos = CommonFunc.GetLineCenterPerpendicularPoint(points[connectionList[i].posIdx1], points[connectionList[i].posIdx2]);

                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("[" + i + "] CEN: " + centerPos.x + "," + centerPos.y);
                                Logger.Log("[" + i + "] SUI: " + perpendicularPos.x + "," + perpendicularPos.y);
                            }

                            for (int j = 0; j < connectionList.Count; j++)
                            {
                                if (i == j) continue;

                                // 交差判定
                                Vector3 crossPos;
                                bool check = CommonFunc.GetCrossPos2(centerPos, perpendicularPos, points[connectionList[j].posIdx1], points[connectionList[j].posIdx2], out crossPos);
                                if (check)
                                {
                                    if (IniFileData.Instance.logOut)
                                    {
                                        Logger.Log("  ・始終点が入口になっている   [" + i + "," + j + "]");
                                    }

                                    // 交差する. T字路なので、このラインと、残り１箇所の入口から降ろした垂線との交差位置を交差点の中心とする。
                                    for (int k = 0; k < connectionList.Count; k++)
                                    {
                                        if (i == k) continue;
                                        if (j == k) continue;

                                        // 中心点を算出
                                        Vector3 centerPos2 = CommonFunc.GetLineCenterPos(points[connectionList[k].posIdx1], points[connectionList[k].posIdx2]);
                                        // 中心点からの垂線 (直線)
                                        Vector3 perpendicularPos2 = CommonFunc.GetLineCenterPerpendicularPoint(points[connectionList[k].posIdx1], points[connectionList[k].posIdx2]);
                                        // 交差判定
                                        Vector3 crossPos2;
                                        check = CommonFunc.GetCrossPos2(centerPos2, perpendicularPos2, centerPos, crossPos, out crossPos2);
                                        if (check)
                                        {
                                            if (IniFileData.Instance.logOut)
                                            {
                                                Logger.Log("  ・交点算出: " + crossPos2);
                                            }

                                            _CrossingPos = crossPos2;
                                            return _CrossingPos;
                                        }
                                    }
                                }
                            }
                        }

                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("交差しなかった、、、");
                        }
                    }
#endif
                    //----------------------------------------------------------------
                    // 別道路と接しているラインの中心位置を集計
                    //----------------------------------------------------------------
                    double numX, numY, numZ;
                    numX = numY = numZ = 0;
                    foreach (RoadConnect rc in this.connectionList)
                    {
                        // ラインの中心位置を算出
                        Vector3 center = CommonFunc.GetLineCenterPos(this.points[rc.posIdx1], this.points[rc.posIdx2]);
                        // 集計
                        numX += center.x;
                        numY += center.y;
                        numZ += center.z;
                    }

                    //----------------------------------------------------------------
                    // 交差点の中心位置を返却
                    //----------------------------------------------------------------
                    _CrossingPos = new Vector3(
                        (float)(numX / this.connectionList.Count),
                        (float)(numY / this.connectionList.Count),
                        (float)(numZ / this.connectionList.Count));
                }

                return _CrossingPos;
            }
        }
        private Vector3 _CrossingPos = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        /// <summary>
        /// 高速道路のインポート処理
        /// </summary>
        static public int ImportHighway()
        {
            Logger.Log("高速道路の読み込み処理　開始");

            //-------------------------------------
            // XMLファイルの解析
            //-------------------------------------
            List<GmlRoadData> dataList = GmlRoadData.ReadXML(true);
            if (dataList.Count == 0)
            {
                // ファイルなし
                Logger.Log("道路データなし");
                return 0;
            }
            //-------------------------------------
            // 画面への反映処理
            //-------------------------------------
            drawRoad(dataList);

            Logger.Log("高速道路の読み込み処理　終了");

            return dataList.Count;
        }

        /// <summary>
        /// 道路のインポート処理
        /// </summary>
        static public int Import()
        {
            Logger.Log("道路の読み込み処理　開始");

            //-------------------------------------
            // XMLファイルの解析
            //-------------------------------------
            List<GmlRoadData> dataList = GmlRoadData.ReadXML(false);
            if (dataList.Count == 0)
            {
                // ファイルなし
                Logger.Log("道路データなし");
                return 0;
            }
            //-------------------------------------
            // 画面への反映処理
            //-------------------------------------
            drawRoad(dataList);

            Logger.Log("道路の読み込み処理　終了");

            return dataList.Count;
        }

        /// <summary>
        /// 指定フォルダのXMLファイルを解析して読み込む
        /// </summary>
        static public List<GmlRoadData> ReadXML(bool isReadHighway)
        {
            // 読み込みデータを保持
            List<GmlRoadData> dataList = new List<GmlRoadData>();

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
                    Logger.Log("道路ファイルの読み込み開始：" + str);
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
                            GmlRoadData gmldata = new GmlRoadData();

                            // 名称取得
                            gml.GetTagData(nav, "tran:Road/@gml:id", out gmldata.name);
                            // 道路区分
                            gml.GetTagData(nav, "tran:Road/tran:function", out gmldata.function);
                            // 道路構造の種別
                            gml.GetTagData(nav, "tran:Road/uro:roadStructureAttribute/uro:RoadStructureAttribute/uro:sectionType", out gmldata.sectionType);
                            // 幅員
                            gml.GetTagData(nav, "tran:Road/uro:roadStructureAttribute/uro:RoadStructureAttribute/uro:width", out gmldata.widthTag);
                            // 幅員区分
                            gml.GetTagData(nav, "tran:Road/uro:roadStructureAttribute/uro:RoadStructureAttribute/uro:widthType", out gmldata.widthType);


                            // TINの三角形ポリゴン数分ループ
                            XPathNodeIterator nodeList2 = gml.GetXmlNodeList(nav, "tran:Road/tran:lod1MultiSurface/gml:MultiSurface/gml:surfaceMember/gml:Polygon/gml:exterior/gml:LinearRing/gml:posList");
                            if (nodeList2.Count == 0)
                            {
                                // ※<gml:LinearRing>ではなく、<gml:Ring>で指定されている場合
                                nodeList2 = gml.GetXmlNodeList(nav, "tran:Road/tran:lod1MultiSurface/gml:MultiSurface/gml:surfaceMember/gml:Polygon/gml:exterior/gml:Ring/gml:curveMember/gml:LineString/gml:posList");
                            }
                            foreach (XPathNavigator nav2 in nodeList2)
                            {
                                // 点群取得
                                gmldata.points = GmlUtil.ConvertStringToListVec(nav2.Value);
                                foreach (Vector3 vec in gmldata.points)
                                {
                                    //----------------------------------
                                    // 最大最小範囲の設定
                                    //----------------------------------
                                    if (gmldata.areaMax.x < vec.x) gmldata.areaMax.x = (float)vec.x;
                                    if (gmldata.areaMax.y < vec.y) gmldata.areaMax.y = (float)vec.y;
                                    if (gmldata.areaMin.x > vec.x) gmldata.areaMin.x = (float)vec.x;
                                    if (gmldata.areaMin.y > vec.y) gmldata.areaMin.y = (float)vec.y;
                                }
                                // 三角形ポリゴンで特定面積より小さい場合は除外
                                if (gmldata.points.Count == 4)
                                {
                                    // 面積比較
                                    double areaSize = ((gmldata.areaMax.x - gmldata.areaMin.x) * (gmldata.areaMax.y - gmldata.areaMin.y)) / 2;
                                    if (areaSize < IniFileData.Instance.roadFilterAreaSize)
                                    {
                                        gmldata.points.Clear();
                                        continue;
                                    }
                                }

                                // 頂点を取得できているか確認
                                if (gmldata.points.Count > 3)
                                {
                                    if (IniFileData.Instance.logOut)
                                    {
                                        Logger.Log("名称　　        : " + gmldata.name);
                                        Logger.Log("道路区分        : " + gmldata.function);
                                        Logger.Log("塘路構造種別    : " + gmldata.sectionType);
                                        Logger.Log("幅員区分        : " + gmldata.widthType);
                                        Logger.Log("幅員            : " + gmldata.widthTag);
                                        Logger.Log("形状ポリゴン    ：", gmldata.points);
                                        Logger.Log("--------------------------------");
                                    }
                                    break;
                                }
                                gmldata.points.Clear();
                            }

                            // 頂点を取得できているか確認
                            if (gmldata.points.Count > 3)
                            {
                                //--------------------------------------
                                // 別道路との繋がりチェック
                                //--------------------------------------
                                GmlRoadData data2 = null;
                                RoadConnect rc_self = null;
                                RoadConnect rc_aite = null;
                                // 読み込み済みの道路数分ループ
                                for (int loop_cnt7 = 0; loop_cnt7 < dataList.Count; loop_cnt7++)
                                {
                                    data2 = dataList[loop_cnt7];
                                    // 道路が重なるか判定 (矩形範囲でのチェック)
                                    if (CommonFunc.checkAreaInArea(gmldata.areaMax, gmldata.areaMin, data2.areaMax, data2.areaMin) == 0)
                                    {
                                        // 範囲外
                                        continue;
                                    }

                                    // 読み込み済み道路の頂点数分ループ
                                    for (int loop_cnt8 = 0; loop_cnt8 < data2.points.Count; loop_cnt8++)
                                    {
                                        // 道路の頂点数分ループ
                                        for (int loop_cnt9 = 0; loop_cnt9 < gmldata.points.Count; loop_cnt9++)
                                        {
                                            // 頂点が一致した場合
                                            if (gmldata.points[loop_cnt9].x == data2.points[loop_cnt8].x &&
                                                gmldata.points[loop_cnt9].y == data2.points[loop_cnt8].y)
                                            {
                                                // [前 == 前] の場合
                                                if (loop_cnt9 > 0 && loop_cnt8 > 0 &&
                                                    gmldata.points[loop_cnt9 - 1].x == data2.points[loop_cnt8 - 1].x &&
                                                    gmldata.points[loop_cnt9 - 1].y == data2.points[loop_cnt8 - 1].y)
                                                {
                                                    // 繋がり設定
                                                    rc_self = new RoadConnect(loop_cnt9 - 1, loop_cnt9, dataList.Count, loop_cnt7);
                                                    rc_aite = new RoadConnect(loop_cnt8 - 1, loop_cnt8, loop_cnt7, dataList.Count);
                                                    break;
                                                }
                                                // [前 == 次] の場合
                                                else if (loop_cnt9 > 0 && loop_cnt8 < data2.points.Count - 1 &&
                                                    gmldata.points[loop_cnt9 - 1].x == data2.points[loop_cnt8 + 1].x &&
                                                    gmldata.points[loop_cnt9 - 1].y == data2.points[loop_cnt8 + 1].y)
                                                {
                                                    // 繋がり設定
                                                    rc_self = new RoadConnect(loop_cnt9 - 1, loop_cnt9, dataList.Count, loop_cnt7);
                                                    rc_aite = new RoadConnect(loop_cnt8, loop_cnt8 + 1, loop_cnt7, dataList.Count);
                                                    loop_cnt8++;
                                                    break;
                                                }
                                                // [次 == 前] の場合
                                                else if (loop_cnt9 < gmldata.points.Count - 1 && loop_cnt8 > 0 &&
                                                    gmldata.points[loop_cnt9 + 1].x == data2.points[loop_cnt8 - 1].x &&
                                                    gmldata.points[loop_cnt9 + 1].y == data2.points[loop_cnt8 - 1].y)
                                                {
                                                    // 繋がり設定
                                                    rc_self = new RoadConnect(loop_cnt9, loop_cnt9 + 1, dataList.Count, loop_cnt7);
                                                    rc_aite = new RoadConnect(loop_cnt8 - 1, loop_cnt8, loop_cnt7, dataList.Count);
                                                    break;
                                                }
                                                // [次 == 次] の場合
                                                else if (loop_cnt9 < gmldata.points.Count - 1 && loop_cnt8 < data2.points.Count - 1 &&
                                                    gmldata.points[loop_cnt9 + 1].x == data2.points[loop_cnt8 + 1].x &&
                                                    gmldata.points[loop_cnt9 + 1].y == data2.points[loop_cnt8 + 1].y)
                                                {
                                                    // 繋がり設定
                                                    rc_self = new RoadConnect(loop_cnt9, loop_cnt9 + 1, dataList.Count, loop_cnt7);
                                                    rc_aite = new RoadConnect(loop_cnt8, loop_cnt8 + 1, loop_cnt7, dataList.Count);
                                                    break;
                                                }
                                            }
                                        }
                                        // 道路の繋がりを発見した場合
                                        if (rc_self != null && rc_aite != null)
                                        {
                                            //------------------------------------------------
                                            // 道路の接続情報の設定判定
                                            //------------------------------------------------
                                            bool addFlag = true;
                                            foreach (RoadConnect rc in gmldata.connectionList)
                                            {
                                                if (rc_self.isDuplicate(rc))
                                                {
                                                    // 既に登録済み
                                                    addFlag = false;
                                                    break;
                                                }
                                            }
                                            if (addFlag)
                                            {
                                                // 登録処理
                                                gmldata.connectionList.Add(rc_self);
                                                data2.connectionList.Add(rc_aite);
                                            }
                                        }
                                        rc_self = null;
                                        rc_aite = null;
                                    }
                                }

                                //--------------------------------------
                                // 道路情報を戻り値に追加
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

            Logger.Log("読み込み道路のデータ数：" + dataList.Count);

            //-------------------------------------
            // 道路中心線の生成
            //-------------------------------------
            for (int loop1 = 0; loop1 < dataList.Count; loop1++)
            {
                GmlRoadData data = dataList[loop1];
                // 独立した道路、行き止まり道路、通常道路の場合
                if (data.connectionList.Count < 3)
                {
                    // 中心線を生成
                    data.centerLineList = CreateRoadCenterLine(data, dataList);

                    // 「形状ポリゴン」「道路同士の接続情報」は使用済みなので解放
                    data.connectionList.Clear();
                    data.points.Clear();
                    data.points.AddRange(data.centerLineList);
                    data.centerLineList.Clear();
                    dataList[loop1] = data;
                }
            }

            //-------------------------------------
            // 不要なデータの除外
            //-------------------------------------
            int loop2 = 0;
            while (loop2 < dataList.Count)
            {
                if (dataList[loop2].points.Count == 0)
                {
                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log("[ERR] 【不正データ】頂点数が０件な不正データ：" + dataList[loop2].name);
                    }
                    dataList.Remove(dataList[loop2]);
                }
                // 交差点なら削除
                else if (dataList[loop2].isCrossing)
                {
                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log("[ERR] 交差点なので除外：" + dataList[loop2].name);
                    }
                    dataList.Remove(dataList[loop2]);
                }
                // 高速道路モードなら、通常道路を削除
                else if (isReadHighway)
                {
                    if (!dataList[loop2].isHighWay)
                    {
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[ERR] 一般道なので除外：" + dataList[loop2].name);
                        }
                        dataList.Remove(dataList[loop2]);
                    }
                    else
                    {
                        loop2++;
                    }
                }
                // 通常モードなら、高速道路を削除
                else
                {
                    if (dataList[loop2].isHighWay)
                    {
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[ERR] 高速道路なので除外：" + dataList[loop2].name);
                        }
                        dataList.Remove(dataList[loop2]);
                    }
                    else
                    {
                        loop2++;
                    }
                }
            }
            Logger.Log("読み込み道路のデータ数（交差点除外）：" + dataList.Count);

            //-------------------------------------
            // 中心線ラインの頂点数を削減
            //-------------------------------------
            for (loop2 = 0; loop2 < dataList.Count; loop2++)
            {
                thinLinePoints(dataList[loop2].points);
            }
            // ライン上の近い頂点を間引く
            thisLinePoints3(dataList);

            if (IniFileData.Instance.logOut)
            {
                for (loop2 = 0; loop2 < dataList.Count; loop2++)
                {
                    Logger.Log("最終頂点リスト：", dataList[loop2].points);
                }
            }

            return dataList;
        }

        /// <summary>
        /// 道路を画面上に反映する
        /// </summary>
        static private void drawRoad(List<GmlRoadData> dataList)
        {
            //-------------------------------------
            // 画面上に反映する
            //-------------------------------------
            Dictionary<short, List<SimpleNode>> nodeMap = new Dictionary<short, List<SimpleNode>>();
            Randomizer rand = new Randomizer();
            NetManager nm = NetManager.instance;
            SimulationManager sm = SimulationManager.instance;
            TerrainManager tm = TerrainManager.instance;
            NetInfo ni;
            NetInfo ni1;
            NetInfo ni2;
            foreach (var segment in dataList)
            {
                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("処理対象：" + segment.name);
                }
                //----------------------------------------
                // 道路タイプの判定
                //----------------------------------------
                ni = ni1 = ni2 = null;
                // アセットが対応しているか判定
                string assetName = segment.prefab_name;
                string assetName2 = null;
                if (segment.isTunnel)
                {
                    assetName = segment.prefab_name_Slope;
                    assetName2 = segment.prefab_name_Tunnel;

                    if (PrefabCollection<NetInfo>.LoadedExists(assetName) &&
                        PrefabCollection<NetInfo>.LoadedExists(assetName2))
                    {
                        // アセットの道路を用いる
                        ni1 = PrefabCollection<NetInfo>.FindLoaded(assetName);
                        ni2 = PrefabCollection<NetInfo>.FindLoaded(assetName2);
                    }
                }
                else
                {
                    if (PrefabCollection<NetInfo>.LoadedExists(assetName))
                    {
                        // アセットの道路を用いる
                        ni = PrefabCollection<NetInfo>.FindLoaded(assetName);
                    }
                }
                // 対応しているアセットがない場合
                if (ni == null && ni1 == null && ni2 == null)
                {
                    Logger.Log("assetなし：" + segment.name);
                    continue;
                }

                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("asset：" + assetName + " : " + assetName2 + "[ " + segment.widthType + " : " + segment.widthTag + " : " + segment.widthPoly + " ]");
                }

                //----------------------------------------
                // 道路の設定
                //----------------------------------------
                ushort startNetNodeId;
                ushort endNetNodeId;
                ushort startNetNodeId_bf = 0; // 手前で使用したノード番号（同じなら追加しない判定用）
                ushort endNetNodeId_bf = 0;   // 手前で使用したノード番号（同じなら追加しない判定用）
                double baseHeightST = 0;
                double baseHeightED = 0;
                double baseHeightAdd = 0;
                double lineDist = 0;
                double calcDist = 0;
                double elevatedST = 0;      // 高架の頂上まで上りきる距離  (始点からこの距離までで勾配調整)
                double elevatedED = 0;      // 高架の頂上から降り始める距離(この距離から終点までで勾配調整)
                double elevatedDist = 0;    // 高架の長さ
                double elevatedH = 0;       // 高架の高さ
                List<double> lineDistList = new List<double>();

                // トンネル、高架、橋の場合は高さを取得
                if (segment.isBridge || segment.isElevated || segment.isTunnel)
                {
                    //---------------------------------------
                    // トンネルの場合、始終点への頂点追加処理
                    //---------------------------------------
                    if (segment.isTunnel)
                    {
                        // トンネルの始点終点はスロープを設置するため、距離が長すぎる場合には、頂点を補完する
                        double checkDist = 30 * MapExtent.Instance.areaScaleX; // 30mを地図上のスケールにに合わせる

                        // 始点の確認
                        double dist = CommonFunc.Dist2Point(segment.points[0], segment.points[1]);
                        if (dist > checkDist)
                        {
                            // 頂点を生成する
                            Vector3 newPos1 = CommonFunc.GetLinePos(segment.points[0], segment.points[1], checkDist);
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
                            Vector3 newPos2 = CommonFunc.GetLinePos(segment.points[segment.points.Count - 1], segment.points[segment.points.Count - 2], checkDist);
                            // 頂点を追加する
                            segment.points.Insert(segment.points.Count - 1, newPos2);

                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("[T]トンネル終点に頂点追加：" + dist + " | " + checkDist + " | ( " + newPos2.x + " , " + newPos2.y + " )");
                            }
                        }

                        // 頂点数が少ない場合、頂点を追加する
                        if (segment.points.Count < 4)
                        {
                            dist = CommonFunc.Dist2Point(segment.points[0], segment.points[1]);
                            // スロープがあるので、最小でも４点必要
                            Vector3 newVec;
                            newVec = CommonFunc.GetLinePos(segment.points[0], segment.points[1], dist / 4);
                            segment.points.Insert(1, newVec);
                            newVec = CommonFunc.GetLinePos(segment.points[segment.points.Count - 1], segment.points[segment.points.Count - 2], dist / 4);
                            segment.points.Insert(segment.points.Count - 1, newVec);
                        }
                        else if (segment.points.Count < 3)
                        {
                            dist = CommonFunc.Dist2Point(segment.points[segment.points.Count - 1], segment.points[segment.points.Count - 2]);
                            // スロープがあるので、最小でも４点必要
                            Vector3 newVec;
                            newVec = CommonFunc.GetLinePos(segment.points[segment.points.Count - 1], segment.points[segment.points.Count - 2], dist / 2);
                            segment.points.Insert(segment.points.Count - 1, newVec);
                        }
                    }
                    //---------------------------------------
                    // 高架の場合、始終点の頂点を調整
                    //---------------------------------------
                    if (segment.isElevated)
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
                                Vector3 newPos1 = CommonFunc.GetLinePos(segment.points[idx], segment.points[idx + 1], newDist);
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
                                Vector3 newPos1 = CommonFunc.GetLinePos(segment.points[idx + 1], segment.points[idx], newDist);
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
                        if (segment.isTunnel)
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
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[T]　" + i + "点目： ( " + stPoint.x + " , " + stPoint.y + " )");
                        }

                        // Vector型に変換
                        var startNodePos = new Vector3((float)stPoint.x, 0, (float)stPoint.y);
                        // 高さを設定
                        startNodePos.y = tm.SampleRawHeightSmoothWithWater(startNodePos, false, 0f);
                        if (segment.isBridge || segment.isElevated || segment.isTunnel)
                        {
                            startNodePos.y = (float)(baseHeightST);

                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("[T]　" + i + "点目の高さ[ST]：" + startNodePos.y);
                            }
                        }

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

                    // 終点のセルに頂点が１つもない場合
                    if (!SimpleNode.FindNode(nodeMap, out endNetNodeId, edPoint))
                    {
                        // トンネルの場合、始終点とそれ以外とでアセットが異なる
                        if (segment.isTunnel)
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
                        if (segment.isBridge || segment.isElevated || segment.isTunnel)
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
                            if (segment.isElevated)
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
                        if (segment.isBridge || segment.isElevated || segment.isTunnel)
                        {
                            // 始点からの距離を設定
                            calcDist += lineDistList[i];
                        }
                    }

                    //--------------------------------------
                    // 登録処理
                    //--------------------------------------
                    if (startNetNodeId == endNetNodeId && startNetNodeId == endNetNodeId_bf)
                    {
                        Logger.Log("始終点が同じノードなので追加対象外： " + segment.name + " [ " + i + " 点目]");
                        continue;
                    }
                    startNetNodeId_bf = startNetNodeId;
                    endNetNodeId_bf = endNetNodeId;

                    //--------------------------------------
                    // 登録処理
                    //--------------------------------------
                    if (segment.isTunnel)
                    {
                        // トンネル入口の場合、アセットを正しく表示するためIDを逆転させる
                        if (i == 0)
                        {
                            var tmp = endNetNodeId;
                            endNetNodeId = startNetNodeId;
                            startNetNodeId = tmp;
                        }
                    }

                    ushort segmentId;
                    Vector3 endPos = nm.m_nodes.m_buffer[endNetNodeId].m_position;
                    Vector3 startPos = nm.m_nodes.m_buffer[startNetNodeId].m_position;
                    Vector3 startDirection = VectorUtils.NormalizeXZ(endPos - startPos);
                    NetManager net_manager = NetManager.instance;
                    try
                    {
                        // トンネルの場合、始終点とそれ以外とでアセットが異なる
                        if (segment.isTunnel)
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
                            // 建物のインデックスを加算
                            Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                            // 道路名を設定
                            net_manager.SetSegmentNameImpl(segmentId, segment.name);

                            //                            Logger.Log("〇画面への登録成功： " + segment.name);
                        }
                        else
                        {
                            Logger.Log("×画面への登録失敗： " + segment.name);
                        }
                    }
                    catch (Exception ex)
                    {
                        //try-catch just to prevent crashing by ignoring invalid trees and letting valid trees get created
                        //RaiseTreeMapperEvent (ex.Message);
                        Logger.Log("エラー発生。：" + ex.Message);
                    }
                }

#if true
                //-------------------------------------
                // 画面再描画
                //-------------------------------------
                int iMaxX = MapExtent.MAX_AREA_SIZE / 2;
                int iMaxY = MapExtent.MAX_AREA_SIZE / 2;
                int iIntX = (int)TerrainManager.RAW_CELL_SIZE; // グリッドサイズ
                int iIntY = (int)TerrainManager.RAW_CELL_SIZE; // グリッドサイズ
                int idxMinX = (int)Math.Floor((segment.areaMin.x + iMaxX) / iIntX);
                int idxMinY = (int)Math.Floor((segment.areaMin.y + iMaxY) / iIntY);
                int idxMaxX = (int)Math.Ceiling((segment.areaMax.x + iMaxX) / iIntX);
                int idxMaxY = (int)Math.Ceiling((segment.areaMax.y + iMaxY) / iIntY);
                // 再描画
                TerrainModify.UpdateArea(idxMinX, idxMinY, idxMaxX, idxMaxY, heights: true, surface: true, zones: true);
#endif
            }
        }

        /// <summary>
        /// 道路の中心線を取得する
        /// </summary>
        static private List<Vector3> CreateRoadCenterLine(GmlRoadData data, List<GmlRoadData> dataList)
        {
            if (IniFileData.Instance.logOut)
            {
                Logger.Log("道路の中心線算出　開始");
                Logger.Log("[" + data.name + "] 対象ポリゴン： ", data.points);
            }

            int loop1;
            bool bRet;
            // 道路中心線の格納用
            List<Vector3> centerLineList = new List<Vector3>();

            //----------------------------------------
            // 垂線の一覧取得
            //----------------------------------------
            // 垂線リスト
            List<PerpendData> perpendList = new List<PerpendData>();

            // 垂線の一覧取得
            GetPerpendLines(data, perpendList);
            if (perpendList.Count < 2)
            {
                // 最低でも道路出入口の２本はあるはず？
                Logger.Log("【不正データ】垂線が少なすぎる：" + data.name);
                Logger.Log("道路ポリゴン: ", data.points);
                Logger.Log("垂線リスト件数: " + perpendList.Count);
                return centerLineList;
            }

            //----------------------------------------
            // 独立道路の入口を設定
            //----------------------------------------
            SetRoadStartLine(data, perpendList);

            //----------------------------------------
            // 垂線の間引き処理
            //----------------------------------------
            ThinPerpendList(perpendList);

            //----------------------------------------
            // 幅員算出
            //----------------------------------------
            data.widthPoly = GetRoadWidth(data, perpendList);

            //----------------------------------------
            // 道路連結 (道路の出入口)
            //----------------------------------------
            Vector3 tmpVec;
            // 道路出入口の要素番号
            int idxConSt1 = 0;
            int idxConEd1 = 1;
            int idxConSt2 = -1;
            int idxConEd2 = -1;
            // 入口が分かっている場合
            if (data.connectionList.Count > 0)
            {
                idxConSt1 = data.connectionList[0].posIdx1;
                idxConEd1 = data.connectionList[0].posIdx2;
                // 道路入口の設定
                tmpVec = CommonFunc.GetLineCenterPos(data.points[idxConSt1], data.points[idxConEd1]);
                centerLineList.Add(tmpVec);

                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("入口側の中心点を追加: ", centerLineList);
                }
            }
            // 出口が分かっている場合
            if (data.connectionList.Count > 1)
            {
                // 出口の垂線を特定
                int endLineIdx;
                for (endLineIdx = 1; endLineIdx < perpendList.Count; endLineIdx++)
                {
                    if (perpendList[endLineIdx].isStartEnd)
                    {
                        break;
                    }
                }
                if (endLineIdx == -1)
                {
                    Logger.Log("【不正データ】通常道路で出口を特定できなかった：" + data.name);
                    Logger.Log("道路ポリゴン: ", data.points);
                    Logger.Log("垂線リスト件数: " + perpendList.Count);
                    return centerLineList;
                }

                idxConSt2 = data.connectionList[1].posIdx1;
                idxConEd2 = data.connectionList[1].posIdx2;

                // L字路の場合、交点を設定
                bool check = CommonFunc.GetCrossPos(perpendList[0].stPoint, perpendList[0].edPoint, perpendList[endLineIdx].stPoint, perpendList[endLineIdx].edPoint, out tmpVec);
                if (check)
                {
                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log("L字道路の交差点を設定: ", centerLineList);
                    }
                    centerLineList.Add(tmpVec);
                }

                // 道路出口の設定
                tmpVec = CommonFunc.GetLineCenterPos(data.points[idxConSt2], data.points[idxConEd2]);
                centerLineList.Add(tmpVec);

                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("出口側の中心点を追加: ", centerLineList);
                }
            }

            /*---------------------------------------------------------
             * 道路入口の頂点から順に、ポリゴン外周に降ろされた垂線位置を参照し、
             * 入口に近い垂線から順に中心点を結んでいく。
             * 
             * 出口に着くころには全点処理されるはずだね！！
             * 
            ------------------------------------------------------------*/
#if true
            // 折り返し判定用
            bool lastCheckFlag = false;
            // 別道路との接続頂点を開始位置とする
            loop1 = idxConSt1;
            // 各ライン毎にループ処理
            while (true)
            {
                //----------------------------------------
                // 処理対象ライン取得
                //----------------------------------------
                Vector3 stPos1 = data.points[loop1];
                Vector3 edPos1 = data.points[0];
                if (loop1 < data.points.Count - 1)
                {
                    edPos1 = data.points[loop1 + 1];
                }

                // 対象ラインに含まれる垂線を求める
                List<PerpendSort> sortList = new List<PerpendSort>();
                int loop2;
                double dist;
                for (loop2 = 1; loop2 < perpendList.Count; loop2++)
                {
                    // 始終点のポリゴンの場合
                    if (perpendList[loop2].isStartEnd)
                    {
                        if (loop1 == perpendList[loop2].posIdx1 || loop1 == perpendList[loop2].posIdxAite1)
                        {
                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("[中心点算出] [" + loop1 + "] 出口垂線に到達、以降は折り返し [LogCheck]");
                            }

                            // 折り返し判定用
                            lastCheckFlag = true;
                        }
                        continue;
                    }
                    // 既に垂線から中心点算出済みな場合
                    if (perpendList[loop2].isCreatedCenterPos)
                    {
                        continue;
                    }

                    // 垂線の始点側が線上にある場合
                    if (loop1 == perpendList[loop2].posIdx1)
                    {
                        dist = CommonFunc.Dist2Point(stPos1, perpendList[loop2].stPoint);
                        sortList.Add(new PerpendSort(loop2, dist));
                    }
                    // 垂線の終点側が線上にある場合
                    else if (loop1 == perpendList[loop2].posIdxAite1)
                    {
                        dist = CommonFunc.Dist2Point(stPos1, perpendList[loop2].edPoint);
                        sortList.Add(new PerpendSort(loop2, dist));
                    }
                }

                // 処理対象のラインに垂線が含まれている場合
                if (sortList.Count > 0)
                {
                    // 含まれる垂線のなかで、最も始点に近い垂線を求める
                    sortList.Sort((a, b) => (a.dist - b.dist) > 0 ? 1 : -1); // 距離で昇順ソート

                    // 近い垂線の順番で中心点処理を行う
                    foreach (PerpendSort sdata in sortList)
                    {
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[中心点算出] [" + loop1 + "] 指定垂線で処理 [" + sdata.idx + "]");
                        }
                        bRet = AddCenterPos(perpendList[sdata.idx], centerLineList, data.connectionList.Count, null, lastCheckFlag);
                        perpendList[sdata.idx].isCreatedCenterPos = true;
                    }
                }

                //----------------------------------------
                // ループ継続判定
                //----------------------------------------
                loop1++;
                if (loop1 >= data.points.Count - 1)
                {
                    // 点群の最後となった場合には最初に戻す
                    loop1 = 0;
                }
                if (loop1 == idxConSt1)
                {
                    // 最後の頂点
                    break;
                }
            }
#endif

            // 出口が分からない場合
            if (data.connectionList.Count == 1)
            {
                // 出口を特定してライン中央位置を終点に追加しないといけない
                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("[中心点算出] 出口不明なので、出口ラインを特定する");
                }
                // 中心線が取得できていない
                if (centerLineList.Count == 0)
                {
                    // 最初に入り口を設定しているので、ここに来ることはあり得ない。
                    // 念のためIF文書いているだけ、、、
                }
                // 中心線がライン形状で取得できている
                else if (centerLineList.Count == 1)
                {
                    // 入口しか存在しない場合？
                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log("[中心点算出] 中心線が頂点１つしかないんだが、、、：", centerLineList);
                    }
                }
                // 中心線がライン形状で取得できている
                else
                {
                    //---------------------------------------------------------------
                    // 終点の延長上で、最も近くに交差する頂点を算出する。
                    //---------------------------------------------------------------
                    // ポリゴン全体の対角線の長さ算出
                    Vector3 vec = data.areaMax - data.areaMin;
                    double dist = Math.Sqrt(vec.x * vec.x + vec.y * vec.y); // 入力ポリゴンの対角線の長さ
                                                                            // 延長する
                    int tmpIdx = centerLineList.Count - 1;
                    Vector3 vec2 = CommonFunc.GetLinePos(centerLineList[tmpIdx - 1], centerLineList[tmpIdx], dist);
                    // 交点を算出
                    double dist2 = double.MaxValue;
                    int lastLineIdx = -1;

                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log("[中心点算出] 中心線を延長したライン：" + centerLineList[tmpIdx].x + " " + centerLineList[tmpIdx].y + vec2.x + " " + vec2.y);
                    }

                    for (int loop3 = 0; loop3 < data.points.Count - 1; loop3++)
                    {
                        // 交点を求める
                        bRet = CommonFunc.GetCrossPos(centerLineList[tmpIdx], vec2, data.points[loop3], data.points[loop3 + 1], out vec);
                        if (bRet)
                        {
                            // 交点までの距離算出
                            dist = CommonFunc.Dist2Point(centerLineList[tmpIdx], vec);

                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("[中心点算出] [" + loop3 + "] 交差あり：" + data.points[loop3].x + " " + data.points[loop3].y);
                                Logger.Log("[中心点算出] [" + loop3 + "] 距離比較：[" + dist + " < " + dist2 + "] ?");
                            }

                            if (dist < dist2)
                            {
                                dist2 = dist;

                                if (IniFileData.Instance.logOut)
                                {
                                    Logger.Log("[中心点算出] [" + loop3 + "] 最も近い交点位置を更新：" + dist + " -> " + dist2);
                                }
                                // 最も近い交点を持つラインの要素番号を保持
                                lastLineIdx = loop3;
                            }
                        }
                    }
                    // 交点を取得できている場合
                    if (lastLineIdx != -1)
                    {
                        // 該当ラインの中心位置を算出
                        vec = CommonFunc.GetLineCenterPos(data.points[lastLineIdx], data.points[lastLineIdx + 1]);
                        // 終点として追加
                        centerLineList.Add(vec);

                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("出口側が行き止まりなので頂点追加: ", centerLineList);
                        }
                    }
                }
            }

            //----------------------------------------
            // 道路連結 (前後の交差点)
            //----------------------------------------
            // 入口が分かっている場合
            if (data.connectionList.Count > 0 && data.connectionList[0].polyIdx_aite > 0)
            {
                GmlRoadData aite = dataList[data.connectionList[0].polyIdx_aite];
                if (aite.isCrossing)
                {
                    // 相手で交差点なので、入口側に頂点追加
                    centerLineList.Insert(0, aite.CrossingPos);

                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log("入口側が交差点なので頂点追加: ", centerLineList);
                    }
                }
            }
            // 出口が分かっている場合
            if (data.connectionList.Count > 1 && data.connectionList[1].polyIdx_aite > 0)
            {
                GmlRoadData aite = dataList[data.connectionList[1].polyIdx_aite];
                if (aite.isCrossing)
                {
                    // 相手で交差点なので、出口側に頂点追加
                    centerLineList.Add(aite.CrossingPos);

                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log("出口側が交差点なので頂点追加: ", centerLineList);
                    }
                }
            }

            //----------------------------------------
            // 点群の座標位置を丸める
            //----------------------------------------
            for (loop1 = 1; loop1 < centerLineList.Count; loop1++)
            {
                Vector3 vec = centerLineList[loop1];
                // 座標値をまるめることにより、頂点数を削減
                vec.x = (float)(((int)(centerLineList[loop1].x * 1000.0)) / 1000.0);
                vec.y = (float)(((int)(centerLineList[loop1].y * 1000.0)) / 1000.0);
                centerLineList[loop1] = vec;
            }

            if (IniFileData.Instance.logOut)
            {
                Logger.Log("中心点: ", centerLineList);

                // 角度をログ出力
                {
                    string logStr = "";
                    for (int i = 0; i < centerLineList.Count - 1; i++)
                    {
                        if (i > 0) logStr += " ";
                        logStr += CommonFunc.GetAngle(centerLineList[i], centerLineList[i + 1]);
                    }
                    Logger.Log("角度　: " + logStr);
                }

                Logger.Log("道路の中心線算出　終了");
            }

            return centerLineList;
        }

        /// <summary>
        /// [中心線作成] 【１】指定した形状ポリゴンから、各ラインの垂線リストを取得する
        /// </summary>
        static private void GetPerpendLines(GmlRoadData data, List<PerpendData> perpendList)
        {
            if (IniFileData.Instance.logOut)
            {
                Logger.Log("[垂線算出処理] 道路の垂線算出処理の開始");
            }

            // 値を設定するので初期化しておく
            perpendList.Clear();

            // 道路出入口の要素番号
            int idxConSt1 = 0;
            int idxConEd1 = 1;
            int idxConSt2 = -1;
            int idxConEd2 = -1;
            if (data.connectionList.Count > 0)
            {
                idxConSt1 = data.connectionList[0].posIdx1;
                idxConEd1 = data.connectionList[0].posIdx2;
            }
            if (data.connectionList.Count > 1)
            {
                idxConSt2 = data.connectionList[1].posIdx1;
                idxConEd2 = data.connectionList[1].posIdx2;
            }

/*-----------------------------------------------------
* 各ラインから一定間隔で垂線をおろしていくパターン
* ※まだ対応入れていないので、↑と同じ処理です。（2022/7/30）
------------------------------------------------------*/
#if true
            // 別道路との接続頂点を開始位置とする
            int loop1 = idxConSt1;
            // 各ライン毎にループ処理
            while (true)
            {
                // 値設定用
                PerpendData perpendData = new PerpendData();

                //----------------------------------------
                // 処理対象ライン取得
                //----------------------------------------
                Vector3 stPos1 = data.points[loop1];
                Vector3 edPos1 = data.points[0];
                if (loop1 < data.points.Count - 1)
                {
                    edPos1 = data.points[loop1 + 1];
                }

                // 中心点を算出
                Vector3 centerPos = CommonFunc.GetLineCenterPos(stPos1, edPos1);
                // 中心点からの垂線 (直線)
                Vector3 perpendicularPos = CommonFunc.GetLineCenterPerpendicularPoint(stPos1, edPos1);

                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("[垂線算出処理] [" + loop1 + "] " + "比較元（直線）：" + stPos1 + " : " + edPos1);
                    Logger.Log("[垂線算出処理] [" + loop1 + "] " + "比較元（垂線）：" + centerPos + " : " + perpendicularPos);
                }

                //----------------------------------------
                // 垂線の終点判定
                //----------------------------------------
                // ---------
                // ※コの字の道路対策に、最も短い垂線を利用する作りとする
                int crossIdx = -1;
                Vector3 crossVec = new Vector3();
                double crossDist = Double.MaxValue;
                // ---------
                // 別のラインとの交差判定
                int loop2 = idxConSt1;
                while (true)
                {
                    //-----------------------------------------------------------------
                    // 比較元のラインと異なっている場合、垂線の交点判定する
                    //-----------------------------------------------------------------
                    if (loop2 != loop1)
                    {
                        //----------------------------------------
                        // 処理対象ラインを取得
                        //----------------------------------------
                        Vector3 stPos2 = data.points[loop2];
                        Vector3 edPos2 = data.points[0];
                        if (loop2 < data.points.Count - 1)
                        {
                            edPos2 = data.points[loop2 + 1];
                        }

                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[垂線算出処理] [" + loop1 + "] " + "[" + loop2 + "] " + "比較先（直線）：" + stPos2 + " : " + edPos2);
                        }

                        //----------------------------------------
                        // 出入口のラインから降ろした垂線の場合
                        // 出入口のラインに降ろされた垂線の場合
                        //----------------------------------------                        Vector3 crossPos;
                        Vector3 crossPos;
                        bool check = CommonFunc.GetCrossPos2(centerPos, perpendicularPos, stPos2, edPos2, out crossPos);
                        if (check)
                        {
                            // ---------
                            // ※コの字の道路対策に、最も短い垂線を利用する作りとする
                            // 垂線の長さが最も短くなる交点を有効とする。
                            double tmpDist = CommonFunc.Dist2Point(centerPos, crossPos);
                            if (tmpDist < crossDist)
                            {
                                // 短い垂線の場合、値を保持
                                crossDist = tmpDist;
                                crossVec.x = crossPos.x;
                                crossVec.y = crossPos.y;
                                crossVec.z = crossPos.z;
                                crossIdx = loop2;
                            }
                            // ---------
                        }
                    }

                    //----------------------------------------
                    // ループ継続判定
                    //----------------------------------------
                    loop2++;
                    if (loop2 >= data.points.Count - 1)
                    {
                        // 点群の最後となった場合には最初に戻す
                        loop2 = 0;
                    }
                    if (loop2 == idxConSt1)
                    {
                        break;
                    }
                }

                // ---------
                // ※コの字の道路対策に、最も短い垂線を利用する作りとする
                // 垂線を生成できた場合
                if (crossIdx != -1)
                {
                    //---------------------------------------
                    // 垂線の始終点、幅員を設定
                    //---------------------------------------
                    // 垂線の始終点、幅を設定する
                    perpendData.stPoint = centerPos;
                    perpendData.edPoint = crossVec;
                    perpendData.width = crossDist;
                    perpendData.posIdx1 = loop1;
                    perpendData.posIdx2 = loop1 + 1 == data.points.Count ? 0 : loop1 + 1;
                    perpendData.posIdxAite1 = crossIdx;
                    perpendData.posIdxAite2 = crossIdx + 1 == data.points.Count ? 0 : crossIdx + 1;
                    perpendData.isRemove = false;
                    perpendData.isStartEnd = false;
                    perpendData.isCreatedCenterPos = false;

                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log("[垂線算出処理] [" + loop1 + "] " + "[" + crossIdx + "] " + "算出した垂線：" + perpendData.stPoint.x + " " + perpendData.stPoint.y + " " + perpendData.edPoint.x + " " + perpendData.edPoint.y);
                    }

                    // 出入口のラインから降ろした垂線の場合
                    if (loop1 == idxConSt1 || loop1 == idxConSt2)
                    {
                        perpendData.isStartEnd = true;
                    }
                    // 出入口のラインに降ろされた垂線の場合
                    else if ((crossIdx == idxConSt1 || crossIdx == idxConSt2) && data.connectionList.Count > 0)
                    {
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[垂線算出処理] 出入口のラインに降ろした垂線は無効");
                        }
                        perpendData.isRemove = true;
                    }
                    // 垂線を生成したラインの左右のラインに交点がある場合、対向側のラインに引けていないので除外
                    else if (crossIdx == loop1 - 1 || crossIdx == loop1 + 1 || (loop1 + 1 == data.points.Count && crossIdx == 0))
                    {
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[垂線算出処理] 隣のラインに降ろした垂線は無効");
                        }
                        perpendData.isRemove = true;
                    }

                    // 戻り値に設定
                    perpendList.Add(perpendData);
                }
                // ---------


                //----------------------------------------
                // ループ継続判定
                //----------------------------------------
                loop1++;
                if (loop1 >= data.points.Count - 1)
                {
                    // 点群の最後となった場合には最初に戻す
                    loop1 = 0;
                }
                if (loop1 == idxConSt1)
                {
                    // 最後の頂点
                    break;
                }
#endif
            }

            if (IniFileData.Instance.logOut)
            {
                Logger.Log("[垂線算出処理] 道路の垂線算出処理の終了");
            }
        }

        /// <summary>
        /// [中心線作成] 【２】垂線リストから道路の入り口を特定する
        /// </summary>
        static private void SetRoadStartLine(GmlRoadData data, List<PerpendData> perpendList)
        {
            // 入口を特定済みな場合
            if (data.connectionList.Count > 0)
            {
                return;
            }

            //----------------------------------------
            // 独立道路の入口判定
            // ※入口を特定できていないため、最も長い垂線のラインを入口とする
            //----------------------------------------
            if (IniFileData.Instance.logOut)
            {
                Logger.Log("独立した道路");
            }

            //----------------------------------------
            // 道路の進行方向をチェック
            //----------------------------------------
            double dist;
            double maxDist = double.MinValue;
            int idx1 = -1;
            for (int loop1 = 0; loop1 < perpendList.Count; loop1++)
            {
                // 垂線の長さ取得
                dist = perpendList[loop1].width;
                if (dist > maxDist)
                {
                    maxDist = dist;
                    idx1 = loop1;
                }
            }
            //---------------------------------------
            // 最も長い垂線のラインを入口として扱う
            //---------------------------------------
            data.connectionList.Add(new RoadConnect(perpendList[idx1].posIdx1, perpendList[idx1].posIdx2, -1, -1));
            perpendList[idx1].isStartEnd = true;

            //---------------------------------------
            // 垂線一覧を、入口の垂線から順に格納しなおす
            //---------------------------------------
            // 一次変数にコピーしてリストをクリア
            List<PerpendData> perpendList_tmp = new List<PerpendData>();
            perpendList_tmp.AddRange(perpendList);
            perpendList.Clear();
            // 入口の垂線から順に格納しなおす
            int loop2 = idx1;
            while (true)
            {
                if (loop2 != idx1)
                {
                    if (perpendList_tmp[idx1].posIdx1 == perpendList_tmp[loop2].posIdxAite1)
                    {
                        // 入口に降ろした垂線は無効とする
                        perpendList_tmp[loop2].isRemove = true;
                    }
                }

                perpendList.Add(perpendList_tmp[loop2]);
                //----------------------------------------
                // ループ継続判定
                //----------------------------------------
                loop2++;
                if (loop2 == data.points.Count - 1)
                {
                    // 点群の最後となった場合には最初に戻す
                    loop2 = 0;
                }
                if (loop2 == idx1)
                {
                    // 最後の頂点
                    break;
                }
            }
        }

        /// <summary>
        /// [中心線作成] 【３】垂線リストから不要な垂線を間引く
        /// </summary>
        static private void ThinPerpendList(List<PerpendData> perpendList)
        {
            //----------------------------------------
            // 垂線の除外判定
            // ・出入口の垂線は残す
            // ・垂線同士の交差が１以上ある場合は除外？
            //----------------------------------------
            int loop1, loop2;
            int crossNum = 0;
            bool bRet;
            for (loop1 = 1; loop1 < perpendList.Count; loop1++)
            {
                // 出入口の垂線は除外しないので対象外。
                if (perpendList[loop1].isStartEnd) continue;

                crossNum = 0;
                for (loop2 = 1; loop2 < perpendList.Count; loop2++)
                {
                    // 交差判定
                    bRet = CommonFunc.Check_LineCross(perpendList[loop1].stPoint, perpendList[loop1].edPoint, perpendList[loop2].stPoint, perpendList[loop2].edPoint);
                    if (bRet)
                    {
                        crossNum++;
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[ThinPerpendList] [" + loop1 + "] [" + loop2 + "] 交差あり[" + crossNum + "]： " + perpendList[loop1].stPoint.x + " " + perpendList[loop1].stPoint.y + " " + perpendList[loop1].edPoint.x + " " + perpendList[loop1].edPoint.y + " " + perpendList[loop2].stPoint.x + " " + perpendList[loop2].stPoint.y + " " + perpendList[loop2].edPoint.x + " " + perpendList[loop2].edPoint.y);
                        }
                    }
                }

                // 出口側を含めて交差４までを許容？
                // 直線　　：出入口からの垂線と交差する可能性があり、通常垂線も交差２があり得る。
                // Ｌ字　　：出入口と、出入口の対向直線からの垂線とで、通常垂線も交差２があり得る。
                // Ｌ字角丸：角だと入り口側、出口側とで交差４がありえる。
                // コ字　　：Ｌ字と同様に、通常垂線も交差３があり得る。
                // コ字角丸：角だと交差４があり得る。
                // 
                // 余計な垂線が残る、、、ひとまず２つまでだけ許容しとくか、、、
                if (crossNum > 2)
                {
                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log("[ThinPerpendList] [" + loop1 + "] 削除対象となる[" + crossNum + "]： " + perpendList[loop1].stPoint.x + " " + perpendList[loop1].stPoint.y + " " + perpendList[loop1].edPoint.x + " " + perpendList[loop1].edPoint.y);
                    }
                    perpendList[loop1].isRemove = true;
                }
            }
            // 項目削除＆幅員集計
            loop1 = 0;
            while (true)
            {
                // ループ継続判定
                if (loop1 >= perpendList.Count) break;

                // 削除判定
                if (perpendList[loop1].isRemove)
                {
                    perpendList.RemoveAt(loop1);
                    continue;
                }
                else
                {
                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log("[ThinPerpendList] 残り垂線：" + perpendList[loop1].stPoint.x + " " + perpendList[loop1].stPoint.y + " " + perpendList[loop1].edPoint.x + " " + perpendList[loop1].edPoint.y);
                    }
                }
                loop1++;
            }
        }

        /// <summary>
        /// [中心線作成] 【４】垂線リストから道路の幅員を算出
        /// </summary>
        static private double GetRoadWidth(GmlRoadData data, List<PerpendData> perpendList)
        {
            //----------------------------------------
            // 幅員算出
            //----------------------------------------
            double totalWidth = 0.0;
            double rtnWidth;
            int widthNum = 0;
            List<double> distList = new List<double>();
            foreach (PerpendData pdata in perpendList)
            {
                if (pdata.isStartEnd) continue;
                totalWidth += pdata.width;
                widthNum++;

                distList.Add(pdata.width);
            }
            if (widthNum > 0)
            {
//                // 平均値を幅員とする
//                rtnWidth = totalWidth / widthNum;
                // 中央値を幅員とする
                distList.Sort();
                rtnWidth = distList[distList.Count / 2];

                if (IniFileData.Instance.logOut)
                {
                    double distAve = totalWidth / widthNum;
                    string logStr = "";
                    foreach (double a in distList) logStr += a + " ";
                    Logger.Log("算出した幅員（算出）平均、中央、リスト：" + distAve + ", " + rtnWidth + ", " + logStr);
                }
            }
            else
            {
                // 垂線が無いので入り口のライン長さを幅員とする
                rtnWidth = CommonFunc.Dist2Point(data.points[data.connectionList[0].posIdx1], data.points[data.connectionList[0].posIdx2]);

                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("算出した幅員（入口）：" + rtnWidth);
                }
            }

            return rtnWidth;
        }

        /// <summary>
        /// [中心線作成] 【５】中心線に、垂線から求めた中心点を追加
        /// 引数　： 垂線、中心点リスト、道路形状、出口以降の折り返し処理中か判定用フラグ
        /// 戻り値： 追加した頂点でのライン
        /// </summary>
        static private bool AddCenterPos(PerpendData pData, List<Vector3> centerLineList, int connectNum, List<Vector3> points, bool isReturnLast)
        {
            Vector3 tmpVec;
            int idx = -1;
            float LINECROSS_BUFFSIZE = 3.0f;
            bool bRet;

            //------------------------------------------------------------
            // 垂線の中心を、道路中心線の最も近い頂点の場所に設定
            //------------------------------------------------------------
            // 垂線の中心位置を取得
            tmpVec = CommonFunc.GetLineCenterPos(pData.stPoint, pData.edPoint);

            if (IniFileData.Instance.logOut)
            {
                Logger.Log("[AddCenterPos] 対象の垂線：" + pData.stPoint.x + " " + pData.stPoint.y + " " + pData.edPoint.x + " " + pData.edPoint.y);
                Logger.Log("[AddCenterPos] 中心点　　：" + tmpVec.x + " " + tmpVec.y);
            }

            /*----------------------------------------------------------------------
            * 先頭から順に繋ぐとともに、垂線をおろした側のラインも、同時に見ていく
            * 同時に見ていけば、出口地点はバッティングして特定できる的な？
            * 同時にみるので、L字箇所も対応できるんじゃね？
            ----------------------------------------------------------------------*/
#if true
            //--------------------------
            // 垂線中心点の挿入処理
            //--------------------------
            for (int loop2 = 0; loop2 < centerLineList.Count - 1; loop2++)
            {
                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("[AddCenterPos] [" + loop2 + "] 比較対象の中心線ライン：" + centerLineList[loop2].x + " " + centerLineList[loop2].y + centerLineList[loop2 + 1].x + " " + centerLineList[loop2 + 1].y);
                }

                //--------------------------
                // 内外判定
                //--------------------------
                bRet = CommonFunc.checkLineInPoint(centerLineList[loop2], centerLineList[loop2 + 1], tmpVec, LINECROSS_BUFFSIZE);
                if (!bRet) continue; // 範囲外

                //--------------------------
                // 範囲内の除外判定
                //--------------------------
                // [終点-1 ～ 終点]で範囲内となった場合、終点の近くにある頂点は追加対象外とする
                if (loop2 == centerLineList.Count - 1)
                {
                    // 終点と追加頂点との距離を算出
                    double dist1 = CommonFunc.Dist2Point(centerLineList[loop2 + 1], tmpVec);
                    // ラインバッファより近い場合
                    if (dist1 < LINECROSS_BUFFSIZE)
                    {
                        // 範囲内だが終点に近い頂点は、ひとまず追加しない！
                        return true;
                    }
                }

                //--------------------------
                // 頂点の挿入対象
                //--------------------------
                idx = loop2 + 1;

                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("[AddCenterPos] ライン上の範囲内なので「頂点」～「次の頂点」の間に挿入 : [" + idx + "]");
                }
            }

            // 範囲内に含まれない場合
            if (idx == -1)
            {
                // 出口から折り返していない場合
                if (!isReturnLast)
                {
                    // 出口特定済みの場合
                    if (connectNum > 1)
                    {
                        // 出口の手前に頂点挿入
                        idx = centerLineList.Count - 1;

                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[AddCenterPos] 「出口」～「手前の頂点」の間に挿入 : [" + idx + "]");
                        }

                        //-------------------------------------------------
                        // 追加頂点の妥当性を確認
                        //-------------------------------------------------
                        // 追加した頂点が道路ポリゴンと交差するか判定
                        if (points != null)
                        {
                            List<Vector3> lv = new List<Vector3>();
                            lv.Add(centerLineList[idx-1]);
                            lv.Add(tmpVec);
                            bRet = CommonFunc.checkLineInLine(lv, points);
                            if (bRet)
                            {
                                if (IniFileData.Instance.logOut)
                                {
                                    Logger.Log("×　[AddCenterPos] 道路ポリゴンと交差するのでスキップ");
                                }
                                return false;
                            }
                        }
                    }
                    else
                    {
                        // 出口として頂点挿入
                        idx = centerLineList.Count;

                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[AddCenterPos] 「出口」に挿入 : [" + idx + "]");
                        }

                        //-------------------------------------------------
                        // 追加頂点の妥当性を確認
                        //-------------------------------------------------
                        // 追加した頂点が道路ポリゴンと交差するか判定
                        if (points != null)
                        {
                            List<Vector3> lv = new List<Vector3>();
                            lv.Add(centerLineList[idx-1]);
                            lv.Add(tmpVec);
                            bRet = CommonFunc.checkLineInLine(lv, points);
                            if (bRet)
                            {
                                if (IniFileData.Instance.logOut)
                                {
                                    Logger.Log("×　[AddCenterPos] 道路ポリゴンと交差するのでスキップ");
                                }
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    /*---------------------------------------------
                    * 最も近い頂点を探し、間に挿入するパターン
                    * ・近すぎる頂点がある場合に不正な動作
                    ---------------------------------------------*/
#if true
                    // 道路中心点リスト内で、最も近い頂点を特定する
                    double dist1 = double.MaxValue;
                    double dist2 = 0;
                    for (int loop2 = 0; loop2 < centerLineList.Count; loop2++)
                    {
                        dist2 = CommonFunc.Dist2Point(tmpVec, centerLineList[loop2]);

                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[中心点算出] [" + loop2 + "] 比較対象の中心線頂点：" + centerLineList[loop2].x + " " + centerLineList[loop2].y);
                            Logger.Log("[中心点算出] [" + loop2 + "] 距離　　　　　　　　：" + dist2 + " / " + dist1 + " : 近い頂点のIndex=" + idx);
                        }

                        if (dist1 > dist2)
                        {
                            dist1 = dist2;
                            idx = loop2;

                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("[中心点算出] [" + loop2 + "] 距離を差替え　　　　：" + dist2 + " / " + dist1 + " : 近い頂点のIndex=" + idx);
                            }
                        }
                    }
                    // 挿入位置を特定する
                    if (idx == 0)
                    {
                        // 「入り口」～「次の頂点」の間に挿入
                        idx++;

                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[中心点算出] 「入り口」～「次の頂点」の間に挿入 : [" + idx + "]");
                        }
                    }
                    else if (idx == centerLineList.Count && connectNum > 1)
                    {
                        // 出口が特定できている場合、「出口」～「手前の頂点」の間に挿入
                        idx--;

                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[中心点算出] 「出口」～「手前の頂点」の間に挿入 : [" + idx + "]");
                        }
                    }
                    else
                    {
                        // 最も近い頂点と、その手前との距離
                        dist1 = CommonFunc.Dist2Point(centerLineList[idx], centerLineList[idx - 1]);
                        // 最も近い頂点の手前と、今回追加頂点との距離
                        dist2 = CommonFunc.Dist2Point(tmpVec, centerLineList[idx - 1]);

                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[中心点算出] 「頂点」の間に挿入");
                            Logger.Log("[中心点算出] ①最も近い頂点の手前　：" + centerLineList[idx - 1].x + " " + centerLineList[idx - 1].y);
                            Logger.Log("[中心点算出] ②最も近い頂点　　　　：" + centerLineList[idx].x + " " + centerLineList[idx].y);
                            Logger.Log("[中心点算出] ③追加する頂点　　　　：" + tmpVec.x + " " + tmpVec.y);
                            Logger.Log("[中心点算出] ①②の距離　　　　　　：" + dist1);
                            Logger.Log("[中心点算出] ①③の距離　　　　　　：" + dist2);
                        }

                        // 最も近い頂点の次に頂点追加
                        if (dist1 < dist2)
                        {
                            idx++;
                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("[中心点算出] 「近い頂点」～「次の頂点」の間に挿入 : [" + idx + "]");
                            }
                        }
                        // 最も近い頂点の手前側に頂点追加
                        else
                        {
                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("[中心点算出] 「近い頂点」～「前の頂点」の間に挿入 : [" + idx + "]");
                            }
                        }
                    }
#endif
                }
            }
#endif

            //------------------------------------------------------
            // 追加位置の前後に同一頂点がある場合には追加しない
            //------------------------------------------------------
            if (idx > 0)
            {
                if (tmpVec.x == centerLineList[idx - 1].x &&
                    tmpVec.y == centerLineList[idx - 1].y)
                {
                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log("[中心点算出] 同一頂点が連続して存在するため、やっぱり追加しない");
                    }
                    return true;
                }
            }
            if (idx < centerLineList.Count)
            {
                if (tmpVec.x == centerLineList[idx].x &&
                    tmpVec.y == centerLineList[idx].y)
                {
                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log("[中心点算出] 同一頂点が連続して存在するため、やっぱり追加しない");
                    }
                    return true;
                }
            }

            // 算出した位置に頂点挿入
            centerLineList.Insert(idx, tmpVec);

            if (IniFileData.Instance.logOut)
            {
                Logger.Log("[AddCenterPos] 中心点を追加: ", centerLineList);
            }

            return true;
        }

        /// <summary>
        /// 道路の始終点を比較し、同一の場合には１つのラインとする
        /// </summary>
        static private void mergePoints(List<GmlRoadData> dataList)
        {
            bool chkFlag = false;

            while (true)
            {
                int dataCnt = dataList.Count;
//                Logger.Log("[mergePoints] dataCnt：" + dataCnt + " , ListCnt : " + dataList.Count);
                for (int idx = 0; idx < dataCnt; idx++)
                {
                    if (idx >= dataList.Count)
                    {
                        break;
                    }
                    for (int idx2 = 0; idx2 < dataCnt; idx2++)
                    {
                        if (idx == idx2)
                        {
                            continue;
                        }
                        if (idx >= dataList.Count)
                        {
                            break;
                        }
                        if (idx2 >= dataList.Count)
                        {
                            break;
                        }
//                        Logger.Log("[Merge] dataCnt：" + dataCnt + " , ListCnt : " + dataList.Count + " , [" + idx + "][" + idx2 + "][" + dataList[idx].points.Count + "][" + dataList[idx2].points.Count + "]");

                        // アセットが違う場合はマージ対象外
                        if (dataList[idx].prefab_name_asset != dataList[idx2].prefab_name_asset) continue;

                        // 始点と終点が同じ場合
                        if (dataList[idx].points[0].x == dataList[idx2].points[dataList[idx2].points.Count - 1].x &&
                            dataList[idx].points[0].y == dataList[idx2].points[dataList[idx2].points.Count - 1].y)
                        {
//                            Logger.Log("idx2側にマージ。idxを削除　（始点と終点が同じ）");
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
//                            Logger.Log("idx2側にマージ。idxを削除　（終点と始点が同じ）");
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
        /// ラインから同一線上の頂点を間引く
        /// ※ノイズ扱いの頂点も取り除く
        /// </summary>
        static private void thinLinePoints(List<Vector3> linePoints)
        {
            // ループカウンタ
            int loop1 = 0;
            Vector3 vec1, vec2;
            float a, b, c;

            if (IniFileData.Instance.logOut)
            {
                Logger.Log("[thinLinePoints] 削減前：Num[" + linePoints.Count + "] : ", linePoints);
            }

            // 入り口頂点は削減の対象外
            loop1 = 1;
            while (true)
            {
                if (loop1 >= linePoints.Count - 1)
                {
                    // 最終頂点まで達したので、ループを抜ける
                    break;
                }

                // ベクトル算出
                vec1 = linePoints[loop1] - linePoints[loop1 - 1];
                vec2 = linePoints[loop1 + 1] - linePoints[loop1 - 1];

                a = vec2.x * vec1.y;
                b = vec1.x * vec2.y;
                c = Math.Abs(Math.Abs(a) - Math.Abs(b));

                // 同一線上なら頂点を削除する
                if (c < 1.0)
                {
                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log("[thinLinePoints] [" + loop1 + "] 同一線上なので除外 : " + vec1.x + " " + vec1.y + " " + vec2.x + " " + vec2.y + " " + a + " " + b + " " + c);
                    }
                    linePoints.RemoveAt(loop1);
                }
                else
                {
                    // 手前の頂点からも確認する。
                    int CHK_NUM = 2;    // ノイズが間に２個あった場合までを対処。３個あったら、もう妥当なデータとする。
                    int loop2 = loop1;
                    bool noiseFlag = false;
                    double angle1 = 0;
                    double angle2 = 0;
                    double angle3 = 0;

                    // 基準点～次の点までの角度
                    angle1 = CommonFunc.GetAngle(linePoints[loop1], linePoints[loop1 + 1]);

                    while (true)
                    {
                        if (noiseFlag)
                        {
                            break;
                        }
                        if (loop2 < 2)
                        {
                            // 最終頂点まで達したので、ループを抜ける
                            break;
                        }
                        if (loop2 < loop1 - CHK_NUM)
                        {
                            // 最終頂点まで達したので、ループを抜ける
                            break;
                        }

                        // 手前のラインの角度
                        angle2 = CommonFunc.GetAngle(linePoints[loop2 - 2], linePoints[loop2 - 1]);

                        angle3 = Math.Abs(angle1 - angle2);

                        // 許容範囲を超えているか確認
                        if (angle3 < 0.1)
                        {
                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("[thinLinePoints] [" + loop1 + "] 同一線上なので除外 : " + vec1.x + " " + vec1.y + " " + vec2.x + " " + vec2.y + " " + a + " " + b + " " + c);
                            }

                            linePoints.RemoveAt(loop1);

                            //---------------------------------
                            // 間のノイズ扱いの頂点を削除
                            //---------------------------------
                            int d = (loop1 - loop2) + 1;
                            for (int e = 0; e < d; e++)
                            {
                                if (IniFileData.Instance.logOut)
                                {
                                    Logger.Log("[thinLinePoints] [" + loop1 + "] [" + (loop1 - 1) + "] ノイズなので除去");
                                }
                                linePoints.RemoveAt(loop1 - 1);
                                loop1--;
                            }
                            noiseFlag = true;
                            break;
                        }
                        else
                        {
                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("[thinLinePoints] [" + loop1 + "] [" + loop2 + "] ノイズ判定で同一線上ではない");
                            }
                        }
                        loop2--;
                    }
                    if (!noiseFlag)
                    {
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[thinLinePoints] [" + loop1 + "] 線上ではない　　　 : " + vec1.x + " " + vec1.y + " " + vec2.x + " " + vec2.y + " " + a + " " + b + " " + c);
                        }
                        loop1++;
                    }

                }
            }

            if (IniFileData.Instance.logOut)
            {
                Logger.Log("[thinLinePoints] 削減後：Num[" + linePoints.Count + "] : ", linePoints);
            }
        }

        /// <summary>
        /// 道路ポリゴンの入口を構成するライン頂点を始点とし、idx1,idx2の位置関係を判定する
        /// [ 0:同一位置, 1:idx1の方が入口に近い, 2:idx2の方が入口に近い ]
        /// </summary>
        static private int CheckPointIdx(GmlRoadData data, int idx1, int idx2)
        {
            // 要素番号が同じなら同一
            if (idx1 == idx2)
            {
                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("[CheckPointIdx] [" + idx1 + "][" + idx2 + "] 同一");
                }
                return 0;
            }
            // 入口ラインの終点側なので、ポリゴン全体だと始点として扱う
            int posSt = data.connectionList[0].posIdx2;
            // 入口ラインの始点側なので、ポリゴン全体だと終点として扱う
            int posEd = data.connectionList[0].posIdx1;

            double dist;
            double dist1, dist1_st, dist1_ed;
            double dist2, dist2_st, dist2_ed;
            bool eflg1, eflg2;
            int index1, index2;

            //-----------------------------------
            // 始点からの距離
            //-----------------------------------
            index1 = posSt;
            eflg1 = eflg2 = false;
            dist = dist1_st = dist2_st = 0;
            while (true)
            {
                index2 = index1 + 1 == data.points.Count ? 0 : index1 + 1;
                // 距離の算出
                dist = CommonFunc.Dist2Point(data.points[index1], data.points[index2]);
                // 距離の加算
                if (!eflg1) dist1_st += dist;
                if (!eflg2) dist2_st += dist;

                // ループカウンタの調整
                index1 = index2;
                // ループ終了判定
                if (index1 == idx1) eflg1 = true;
                if (index1 == idx2) eflg2 = true;
                if (eflg1 && eflg2) break;
            }

            //-----------------------------------
            // 終点からの距離
            //-----------------------------------
            index1 = posEd;
            eflg1 = eflg2 = false;
            dist = dist1_ed = dist2_ed = 0;
            while (true)
            {
                index2 = index1 - 1 == -1 ? data.points.Count-1 : index1 - 1;
                // 距離の算出
                dist = CommonFunc.Dist2Point(data.points[index1], data.points[index2]);
                // 距離の加算
                if (!eflg1) dist1_ed += dist;
                if (!eflg2) dist2_ed += dist;

                // ループカウンタの調整
                index1 = index2;
                // ループ終了判定
                if (index1 == idx1) eflg1 = true;
                if (index1 == idx2) eflg2 = true;
                if (eflg1 && eflg2) break;
            }

            //-------------------------------------------
            // 小さい方を有効とする
            //-------------------------------------------
            if (dist1_st < dist1_ed)
            {
                dist1 = dist1_st;
            }
            else
            {
                dist1 = dist1_ed;
            }
            if (dist2_st < dist2_ed)
            {
                dist2 = dist2_st;
            }
            else
            {
                dist2 = dist2_ed;
            }

            if (IniFileData.Instance.logOut)
            {
                Logger.Log("[CheckPointIdx] [" + idx1 + "][" + idx2 + "] 始終点位置 [" + posSt + "," + posEd + "]");
                Logger.Log("[CheckPointIdx] [" + idx1 + "][" + idx2 + "] idx1の距離 [" + dist1_st + "," + dist1_ed + "]");
                Logger.Log("[CheckPointIdx] [" + idx1 + "][" + idx2 + "] idx2の距離 [" + dist2_st + "," + dist2_ed + "]");
            }

            //-------------------------------------------
            // 判定処理
            //-------------------------------------------
            if (dist1 < dist2)
            {
                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("[CheckPointIdx] [" + idx1 + "][" + idx2 + "] idx1が始点に近い [" + posSt + "]");
                }
                return 1;
            }

            if (IniFileData.Instance.logOut)
            {
                Logger.Log("[CheckPointIdx] [" + idx1 + "][" + idx2 + "] idx2が始点に近い [" + posSt + "]");
            }
            return 2;
        }

        /// <summary>
        /// **mメッシュ内に複数頂点存在する場合、平均をとった１点に集約する
        /// </summary>
        static private void thisLinePoints2(List<GmlRoadData> dataList)
        {
            //------------------------------------------------------
            // 読み込み範囲をメッシュサイズで分割
            //------------------------------------------------------
            // グリッドサイズ (範囲が狭すぎると、本来つながっていない道路がつながってしまう懸念あり。)
            int iCellSize = 3;
            // 始終点 (グリッドサイズで丸める)
            int baseSize = MapExtent.MAX_AREA_SIZE / 2;
            int istartIdx = (int)Math.Floor((baseSize - MapExtent.Instance.importDist) / iCellSize);
            int iendIdx = (int)Math.Ceiling((baseSize + MapExtent.Instance.importDist) / iCellSize);

            int istart = (int)Math.Floor((baseSize - MapExtent.Instance.importDist) / iCellSize) * iCellSize;
            int iend = (int)Math.Ceiling((baseSize + MapExtent.Instance.importDist) / iCellSize) * iCellSize;
            // グリッド数
            int inum = (iend - istart) / iCellSize;
            // 始終点をGame画面中央の座標系に戻す
            istart += baseSize;
            iend += baseSize;

            // ・メッシュXY数分の配列確保
            //  ・該当メッシュ内に含まれる情報を配列で確保
            //   ・該当メッシュ内の情報 ([0]:対象ポリゴン番号, [1]:頂点番号)
            List<int[]>[,] meshArea = new List<int[]>[inum, inum];

            //------------------------------------------------------
            // メッシュ分割したエリアに、各頂点情報を設定
            //------------------------------------------------------
            for (int loop1 = 0; loop1 < dataList.Count; loop1++)
            {
                for (int loop2 = 0; loop2 < dataList[loop1].points.Count; loop2++)
                {
                    // Game座標系(画面中央が[0,0])から、Game座標系(画面左上[0,0])に変換。
                    // その後、メッシュサイズで分割したIndex番号に変換。
                    int xidx = (int)Math.Floor((dataList[loop1].points[loop2].x + baseSize) / iCellSize);
                    int yidx = (int)Math.Floor((dataList[loop1].points[loop2].y + baseSize) / iCellSize);
                    // 今回読み込む範囲の最小値を始点とした、Index番号に変換
                    xidx -= istartIdx;
                    yidx -= istartIdx;
                    // 読み込み範囲内か判定
                    if (xidx < 0 || yidx < 0 || xidx >= inum || yidx >= inum) continue;
                    // 対象メッシュに、今回の頂点情報を追加
                    if (meshArea[xidx, yidx] == null)
                    {
                        meshArea[xidx, yidx] = new List<int[]>();
                    }
                    meshArea[xidx, yidx].Add(new int[2] { loop1, loop2});
                }
            }

            if (IniFileData.Instance.logOut)
            {
                for (int loop1 = 0; loop1 < dataList.Count; loop1++)
                {
                    Logger.Log("間引き前：[" + loop1 + "] ", dataList[loop1].points);
                }
            }

            //------------------------------------------------------
            // メッシュ分割したエリア内の頂点を、平均値に差し替える
            //------------------------------------------------------
            for (int loop1 = 0; loop1 < inum; loop1++)
            {
                for (int loop2 = 0; loop2 < inum; loop2++)
                {
                    if (meshArea[loop1, loop2] == null || meshArea[loop1, loop2].Count < 2)
                    {
                        // メッシュ内に複数頂点なし
                        continue;
                    }

                    // 各頂点の合計を算出
                    double sumX = 0;
                    double sumY = 0;

                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log("[間引き処理]：[" + loop1 + "," + loop2 + "]" + " : " + meshArea[loop1, loop2].Count);
                    }

                    foreach (int[] posData in meshArea[loop1, loop2])
                    {
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("    " + dataList[posData[0]].points[posData[1]]);
                        }
                        sumX += dataList[posData[0]].points[posData[1]].x;
                        sumY += dataList[posData[0]].points[posData[1]].y;
                    }
                    // 平均を算出
                    double aveX = 0;
                    double aveY = 0;
                    aveX = sumX / meshArea[loop1, loop2].Count;
                    aveY = sumY / meshArea[loop1, loop2].Count;

                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log("        平均：" + aveX + "," + aveY);
                    }
                    // 各頂点に平均値を再設定
                    foreach (int[] posData in meshArea[loop1, loop2])
                    {
                        dataList[posData[0]].points[posData[1]] = new Vector3((float)aveX, (float)aveY, 0);
                    }
                }
            }

            if (IniFileData.Instance.logOut)
            {
                for (int loop1 = 0; loop1 < dataList.Count; loop1++)
                {
                    Logger.Log("間引き後：[" + loop1 + "] ", dataList[loop1].points);
                }
            }
        }

        /// <summary>
        /// ライン上の頂点の間隔を間引く
        /// </summary>
        static private void thisLinePoints3(List<GmlRoadData> dataList)
        {
            double dist = 0;
            double checkDist = 20;
            int loop2 = 0;

            //------------------------------------------------------
            // メッシュ分割したエリアに、各頂点情報を設定
            //------------------------------------------------------
            for (int loop1 = 0; loop1 < dataList.Count; loop1++)
            {
                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("頂点間引き処理前：", dataList[loop1].points);
                }

                loop2 = 0;
                while (loop2 < dataList[loop1].points.Count - 1 && dataList[loop1].points.Count > 2)
                {
                    dist = CommonFunc.Dist2Point(dataList[loop1].points[loop2], dataList[loop1].points[loop2+1]);

                    if (dist < checkDist)
                    {
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("　‐頂点間引き：[" + loop1 + " - " + loop2 + "] : " + dist);
                        }

                        if (loop2 == dataList[loop1].points.Count - 2)
                        {
                            // 一定間隔未満は間引く
                            dataList[loop1].points.RemoveAt(loop2);
                        }
                        else
                        {
                            // 一定間隔未満は間引く
                            dataList[loop1].points.RemoveAt(loop2+1);
                        }
                    }
                    else
                    {
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("　＋頂点間引き：[" + loop1 + " - " + loop2 + "] : " + dist);
                        }
                        loop2++;
                    }
                }
                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("頂点間引き処理後：", dataList[loop1].points);
                }
            }
        }

        /// <summary>
        /// **mメッシュ内に複数頂点存在する場合、平均をとった１点に集約する
        /// </summary>
        static private void thisLinePoints4(List<GmlRoadData> dataList)
        {
            //------------------------------------------------------
            // 読み込み範囲をメッシュサイズで分割
            //------------------------------------------------------
            // グリッドサイズ (範囲が狭すぎると、本来つながっていない道路がつながってしまう懸念あり。)
            int iCellSize = 16;
            // 始終点 (グリッドサイズで丸める)
            int baseSize = MapExtent.MAX_AREA_SIZE / 2;
            int istartIdx = (int)Math.Floor((baseSize - MapExtent.Instance.importDist) / iCellSize);
            int iendIdx = (int)Math.Ceiling((baseSize + MapExtent.Instance.importDist) / iCellSize);

            int istart = (int)Math.Floor((baseSize - MapExtent.Instance.importDist) / iCellSize) * iCellSize;
            int iend = (int)Math.Ceiling((baseSize + MapExtent.Instance.importDist) / iCellSize) * iCellSize;
            // グリッド数
            int inum = (iend - istart) / iCellSize;
            // 始終点をGame画面中央の座標系に戻す
            istart += baseSize;
            iend += baseSize;

            //------------------------------------------------------
            // メッシュ分割したエリアに、各頂点情報を設定
            //------------------------------------------------------
            for (int loop1 = 0; loop1 < dataList.Count; loop1++)
            {
                // ・メッシュXY数分の配列確保
                //  ・該当メッシュ内に含まれる情報を配列で確保
                //   ・該当メッシュ内の情報 ([0]:対象ポリゴン番号, [1]:頂点番号)
                List<int[]>[,] meshArea = new List<int[]>[inum, inum];

                for (int loop2 = 1; loop2 < dataList[loop1].points.Count-1; loop2++)
                {
                    // Game座標系(画面中央が[0,0])から、Game座標系(画面左上[0,0])に変換。
                    // その後、メッシュサイズで分割したIndex番号に変換。
                    int xidx = (int)Math.Floor((dataList[loop1].points[loop2].x + baseSize) / iCellSize);
                    int yidx = (int)Math.Floor((dataList[loop1].points[loop2].y + baseSize) / iCellSize);
                    // 今回読み込む範囲の最小値を始点とした、Index番号に変換
                    xidx -= istartIdx;
                    yidx -= istartIdx;
                    // 読み込み範囲内か判定
                    if (xidx < 0 || yidx < 0 || xidx >= inum || yidx >= inum) continue;
                    // 対象メッシュに、今回の頂点情報を追加
                    if (meshArea[xidx, yidx] == null)
                    {
                        meshArea[xidx, yidx] = new List<int[]>();
                    }
                    meshArea[xidx, yidx].Add(new int[2] { loop1, loop2 });
                }

                if (IniFileData.Instance.logOut)
                {
                    for (int loop3 = 0; loop3 < dataList.Count; loop3++)
                    {
                        Logger.Log("間引き前：[" + loop3 + "] ", dataList[loop3].points);
                    }
                }

                //------------------------------------------------------
                // メッシュ分割したエリア内の頂点を、平均値に差し替える
                //------------------------------------------------------
                for (int loop3 = 0; loop3 < inum; loop3++)
                {
                    for (int loop4 = 0; loop4 < inum; loop4++)
                    {
                        if (meshArea[loop3, loop4] == null || meshArea[loop3, loop4].Count < 2)
                        {
                            // メッシュ内に複数頂点なし
                            continue;
                        }

                        // 各頂点の合計を算出
                        double sumX = 0;
                        double sumY = 0;

                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[間引き処理]：[" + loop3 + "," + loop4 + "]" + " : " + meshArea[loop3, loop4].Count);
                        }

                        foreach (int[] posData in meshArea[loop3, loop4])
                        {
                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log("    " + dataList[posData[0]].points[posData[1]]);
                            }

                            sumX += dataList[posData[0]].points[posData[1]].x;
                            sumY += dataList[posData[0]].points[posData[1]].y;
                        }
                        // 平均を算出
                        double aveX = 0;
                        double aveY = 0;
                        aveX = sumX / meshArea[loop3, loop4].Count;
                        aveY = sumY / meshArea[loop3, loop4].Count;

                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("        平均：" + aveX + "," + aveY);
                        }

                        // 各頂点に平均値を再設定
                        foreach (int[] posData in meshArea[loop3, loop4])
                        {
                            dataList[posData[0]].points[posData[1]] = new Vector3((float)aveX, (float)aveY, 0);
                        }
                    }
                }

                if (IniFileData.Instance.logOut)
                {
                    for (int loop3 = 0; loop3 < dataList.Count; loop3++)
                    {
                        Logger.Log("間引き後：[" + loop3 + "] ", dataList[loop3].points);
                    }
                }
            }
        }

        /// <summary>
        /// 交差点の周囲にある頂点をマージ
        /// </summary>
        static private void thisLinePoints5(List<GmlRoadData> dataList, GmlRoadData crossData)
        {
            double checkDist = 10;
            double dist = 0;

            foreach (RoadConnect con in crossData.connectionList)
            {
                Vector3 crossPos = crossData.CrossingPos;
                dist = 0;

                // 始点側が交差点位置と同じ場合
                if (dataList[con.polyIdx_aite].points[0].x == crossPos.x &&
                    dataList[con.polyIdx_aite].points[0].y == crossPos.y)
                {
                    for (int loop2 = 1; loop2 < dataList[con.polyIdx_aite].points.Count - 2; loop2++)
                    {
                        dist += CommonFunc.Dist2Point(dataList[con.polyIdx_aite].points[loop2], crossPos);
                        if (dist < checkDist)
                        {
                            dataList[con.polyIdx_aite].points[loop2] = crossPos;
                        }
                    }
                }
                // 終点側が交差点位置と同じ場合
                else if (dataList[con.polyIdx_aite].points[dataList[con.polyIdx_aite].points.Count - 1].x == crossPos.x &&
                    dataList[con.polyIdx_aite].points[dataList[con.polyIdx_aite].points.Count - 1].y == crossPos.y)
                {
                    for (int loop2 = dataList[con.polyIdx_aite].points.Count - 2; loop2 > 0; loop2--)
                    {
                        dist += CommonFunc.Dist2Point(dataList[con.polyIdx_aite].points[loop2], crossPos);
                        if (dist < checkDist)
                        {
                            dataList[con.polyIdx_aite].points[loop2] = crossPos;
                        }
                    }
                }
            }
        }
    }
}
