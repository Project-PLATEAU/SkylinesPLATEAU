//----------------------------------------------------------------------------
// MapExtent.cs
//
// ■概要
//      Cities:Skylinesのゲーム画面の座標系を管理するクラス
//
//----------------------------------------------------------------------------
using System.IO;                    // ファイル読み込み用
using UnityEngine;

namespace SkylinesPlateau
{
    /// <summary>
    /// ゲーム画面の座標系を管理するクラス
    /// </summary>
    public class MapExtent
    {
        //-------------------------------------
        // 固定値定義
        //-------------------------------------
        /// <summary>
        /// readmeファイルパス（Cities.appからの相対パス）
        /// </summary>
        private const string INPUT_README_FILENAME = @"Files/SkylinesCityGml/readme.txt";

        /// <summary>
        /// ゲーム画面で表示可能な最大範囲 (17.28km)
        /// </summary>
        public const int MAX_AREA_SIZE = 17280;
        public const int MAX_AREA_SIZE_H = (MAX_AREA_SIZE / 5) / 2;
        public const int MAX_AREA_SIZE_H1 = (MAX_AREA_SIZE / 50); // ZONEの1/3の範囲(345.6)

        /// <summary>
        /// ZONEサイズ (8m)
        /// </summary>
        public const int ZONE_SIZE = 8;
        /// <summary>
        /// タイル１つ分のサイズ (1920m)
        /// </summary>
        public const int TILE_AREA_SIZE = 1920;
        /// <summary>
        /// タイル１つにあるZONE数 (240個)
        /// </summary>
        public const int TILE_AREA_ZONE_NUM = TILE_AREA_SIZE / ZONE_SIZE;
        /// <summary>
        /// タイル１つにある区画数? (ZONEを8個まとめた物) (30個)
        /// </summary>
        public const int TILE_AREA_KUKAKU_NUM = TILE_AREA_ZONE_NUM / 8;

        /// <summary>
        /// タイル 3*3の範囲
        /// </summary>
        public const int AREA_SIZE_3 = (int)(TILE_AREA_SIZE * 1.5);  // 区画 [3*3]
        /// <summary>
        /// タイル 5*5の範囲
        /// </summary>
        public const int AREA_SIZE_5 = (int)(TILE_AREA_SIZE * 2.5);  // 区画 [5*5]
        /// <summary>
        /// タイル 9*9の範囲
        /// </summary>
        public const int AREA_SIZE_9 = (int)(TILE_AREA_SIZE * 4.5);  // 区画 [9*9]

        //-------------------------------------
        // メンバ変数
        //-------------------------------------
        private static MapExtent instance;
        public static MapExtent Instance => instance ?? (instance = new MapExtent());

        public double centerX { get; set; }         // 中心位置（Webメルカトル）
        public double centerY { get; set; }         // 中心位置（Webメルカトル）
        public double areaScaleX { get; set; }      // 範囲の割合
        public double areaScaleY { get; set; }      // 範囲の割合
        public double centerLon = 0;                // 緯度経度の中心
        public double centerLat = 0;                // 緯度経度の中心
        public double minlon = 0;                   // readmeの範囲
        public double minlat = 0;                   // readmeの範囲
        public double maxlon = 0;                   // readmeの範囲
        public double maxlat = 0;                   // readmeの範囲
        public uint zoomLevel = 16;                 // ズームレベル
        public double importDist = MapExtent.AREA_SIZE_9; // インポート範囲(画面中央からのメートル単位)

        //-------------------------------------
        // メンバ変数
        //-------------------------------------
        /// <summary>
        /// ゲーム画面の最小タイル番号Ｘ
        /// </summary>
        public uint minTileX
        {
            get
            {
                return (uint)System.Math.Floor(((minlon + 180.0) / 360.0) * System.Math.Pow(2.0, zoomLevel));
            }
        }
        /// <summary>
        /// ゲーム画面の最大タイル番号Ｘ
        /// </summary>
        public uint maxTileX
        {
            get
            {
                return (uint)System.Math.Floor(((maxlon + 180.0) / 360.0) * System.Math.Pow(2.0, zoomLevel));
            }
        }
        /// <summary>
        /// ゲーム画面の最小タイル番号Ｙ
        /// </summary>
        public uint minTileY
        {
            get
            {
                var sinLatitude = System.Math.Sin(maxlat * System.Math.PI / 180.0);
                return (uint)((0.5 - System.Math.Log((1.0 + sinLatitude) / (1.0 - sinLatitude)) / (4.0 * System.Math.PI)) * System.Math.Pow(2.0, zoomLevel));
            }
        }
        /// <summary>
        /// ゲーム画面の最大タイル番号Ｙ
        /// </summary>
        public uint maxTileY
        {
            get
            {
                var sinLatitude = System.Math.Sin(minlat * System.Math.PI / 180.0);
                return (uint)((0.5 - System.Math.Log((1.0 + sinLatitude) / (1.0 - sinLatitude)) / (4.0 * System.Math.PI)) * System.Math.Pow(2.0, zoomLevel));
            }
        }

        /// <summary>
        /// 読み込み範囲の最大位置（ＸＹ座標系）
        /// </summary>
        public Vector3 readAreaMax
        {
            get
            {
                return new Vector3((float)(centerX + importDist), (float)(centerY + importDist), 0.0f);
            }
        }
        /// <summary>
        /// 読み込み範囲の最小位置（ＸＹ座標系）
        /// </summary>
        public Vector3 readAreaMin
        {
            get
            {
                return new Vector3((float)(centerX - importDist), (float)(centerY - importDist), 0.0f);
            }
        }

        //-------------------------------------
        // メソッド定義
        //-------------------------------------
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MapExtent()
        {
//            // readmeファイルを読み込む
//            loadReadMe();
        }

        /// <summary>
        /// RaedMeファイルからゲーム画面の範囲を取得
        /// </summary>
        public void loadReadMe()
        {
            //-------------------------------------
            // ファイルの存在チェック
            //-------------------------------------
            if (!File.Exists(INPUT_README_FILENAME))
            {
                // ファイルなし
                Logger.Log("ReadMeファイルがありません：" + INPUT_README_FILENAME);
                return;
            }

            //-------------------------------------
            // ファイル読み込み
            //-------------------------------------
            string keyStr = "&box=";
            StreamReader inSr = File.OpenText(INPUT_README_FILENAME);
            while (!inSr.EndOfStream)
            {
                string lineStr = inSr.ReadLine();

                // 四隅座標の書かれている行か判定
                int idx = lineStr.IndexOf(keyStr);
                if (idx >= 0)
                {
                    // 四隅座標を取得
                    string tmpStr = lineStr.Substring(idx + keyStr.Length);
                    // 分割する
                    string[] strlist = tmpStr.Split(',');
                    // 要素数の確認
                    if (strlist.Length != 4)
                    {
                        Logger.Log("座標値の取得に失敗：" + tmpStr);
                        inSr.Close();
                        return;
                    }
                    // 座標値をメンバ変数に設定
                    double.TryParse(strlist[0], out maxlon);
                    double.TryParse(strlist[1], out maxlat);
                    double.TryParse(strlist[2], out minlon);
                    double.TryParse(strlist[3], out minlat);
                    Logger.Log("        緯度経度の範囲：" + "( " + minlat + " , " + minlon + " ) ( " + maxlat + " , " + maxlon + " )");
                    // Webメルカトルに変換する
                    double maxX, maxY, minX, minY;
//                    CommonBL.latlng2merc(maxlat, maxlon, out maxX, out maxY);
//                    CommonBL.latlng2merc(minlat, minlon, out minX, out minY);
                    CommonBL.Instance.bl2xy(maxlat, maxlon, out maxX, out maxY);
                    CommonBL.Instance.bl2xy(minlat, minlon, out minX, out minY);
                    Logger.Log("ＷＥＢメルカトルの範囲：" + "( " + minX + " , " + minY + " ) ( " + maxX + " , " + maxY + " )");
                    // 中心座標を算出
                    centerX = minX + ((maxX - minX) / 2.0);
                    centerY = minY + ((maxY - minY) / 2.0);
                    Logger.Log("ＷＥＢメルカトルの中央：" + "( " + centerX + " , " + centerY + " )");
                    // ゲーム画面に対する範囲のスケール
                    areaScaleX = MAX_AREA_SIZE / (maxX - minX);
                    areaScaleY = MAX_AREA_SIZE / (maxY - minY);
                    Logger.Log("ＷＥＢメルカトルの割合：" + "( " + areaScaleX + " , " + areaScaleY + " )");
                }
            }
            inSr.Close();
        }
        /// <summary>
        /// Map範囲の設定
        /// </summary>
        public void SetMapExtent(double centerLat, double centerLon, double dist)
        {
            // 中心座標を算出
            double x,y;
//            CommonBL.latlng2merc(centerLat, centerLon, out x, out y);
            CommonBL.Instance.bl2xy(centerLat, centerLon, out x, out y);
            this.centerLat = centerLat;
            this.centerLon = centerLon;
            centerX = x;
            centerY = y;
            // (平面直角の場合、内部ではXYを逆転させている。描画範囲を回転させたいので）
            Logger.Log("平面直角座標系の中央：" + "( " + y + " , " + x + " )");
            // 範囲を算出
            double maxX, maxY, minX, minY;
            minX = x - MAX_AREA_SIZE / 2;
            minY = y - MAX_AREA_SIZE / 2;
            maxX = x + MAX_AREA_SIZE / 2;
            maxY = y + MAX_AREA_SIZE / 2;
            // (平面直角の場合、内部ではXYを逆転させている。描画範囲を回転させたいので）
            Logger.Log("平面直角座標系の範囲：" + "( " + minY + " , " + minX + " ) ( " + maxY + " , " + maxX + " )");
            // ゲーム画面に対する範囲のスケール
            areaScaleX = 1.0;
            areaScaleY = 1.0;

            // 座標値をメンバ変数に設定
            CommonBL.Instance.ido_keido(minX, minY, out minlat, out minlon);
            CommonBL.Instance.ido_keido(maxX, maxY, out maxlat, out maxlon);
            Logger.Log("        緯度経度の範囲：" + "( " + minlat + " , " + minlon + " ) ( " + maxlat + " , " + maxlon + " )");

            // インポート範囲を設定
            importDist = dist;
            Logger.Log("インポート対象の半径：" + "( " + importDist + " )");
        }

        /// <summary>
        /// Map範囲の設定
        /// </summary>
        public void SetMapExtent(double dist)
        {
            //------------------------------------
            // 設定ファイルの読み込み
            //------------------------------------
            double lat = 0.0, lon = 0.0;
            string[] splitStr = ImportSettingData.Instance.center.Split(',');
            if (splitStr.Length > 1)
            {
                lat = double.Parse(splitStr[0]);
                lon = double.Parse(splitStr[1]);
            }
            else
            {
                return;
            }

            //------------------------------------
            // 範囲設定
            //------------------------------------
            SetMapExtent(lat, lon, dist);
        }

        /// <summary>
        /// Map範囲の設定
        /// </summary>
        public void SetMapExtent()
        {
            //------------------------------------
            // 設定ファイルの読み込み
            //------------------------------------
            double lat = 0.0, lon = 0.0;
            string[] splitStr = ImportSettingData.Instance.center.Split(',');
            if (splitStr.Length > 1)
            {
                lat = double.Parse(splitStr[0]);
                lon = double.Parse(splitStr[1]);
            }
            else
            {
                return;
            }

            double dist;
            // 距離が有効
            if (ImportSettingData.Instance.isUseAreaSize)
            {
                // km単位で格納されているため1000倍する
                dist = double.Parse(ImportSettingData.Instance.areaSize) * 1000.0;
                // 直径から半径に変換
                dist = dist / 2;
            }
            else
            {
                // 全域を対象とする
                dist = MapExtent.AREA_SIZE_9;
            }

            //------------------------------------
            // 範囲設定
            //------------------------------------
            SetMapExtent(lat, lon, dist);
        }
    }
}
