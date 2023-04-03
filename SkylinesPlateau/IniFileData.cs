using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SkylinesPlateau
{
    public class IniFileData
    {
        private const string SETTING_FILE = @"Files/SkylinesPlateau/tbl/SkylinesPlateau.ini";
        private const string KEY_DEM_TIN_FILTER_AREASIZE = @"TIN除外面積";
        private const string KEY_DEM_WATERAREA_DOWNHEIGHT = @"水面補正高さ";
        private const string KEY_DEM_SEA_LEVEL = @"海面高さ";
        private const string KEY_ROAD_FILTER_AREASIZE = @"三角道路ポリゴン除外面積";
        //水面コード対応
        private const string KEY_WATERAREA_INT = @"水面コード番号";

        // 検証用の隠しコマンド
        private const string KEY_TEST_LOG = @"ログ出力";
        private const string KEY_TEST_RAIL_MODE = @"rail_TestMode";
        private const string KEY_TEST_RAIL_FTCODE = @"rail_FTCODE";
        private const string KEY_TEST_RAIL_RAILSTATE = @"rail_RAILSTATE";
        private const string KEY_TEST_RAIL_RTCODE = @"rail_RTCODE";

        private static IniFileData instance;
        public static IniFileData Instance => instance ?? (instance = new IniFileData());

        // TIN除外面積
        public double demFilterAreaSize = 0.0;
        // 水面補正高さ
        public double demWaterAreaDownHeight = 0.0;
        // 海面高さ
        public double demSeaLevel = 0.0;
        // 三角道路ポリゴン除外面積
        public double roadFilterAreaSize = 0.0;

        //水面コード
        public int waterAreaInt = 5;

        // 詳細ログの出力有無
        public bool logOut = false;
        // 線路の動作検証用 (INI指定値に強制的に書き換えるモード。各スタイルの確認に)
        public bool rail_isTestMode = false;
        public long rail_ftCode = -1;
        public long rail_railState = -1;
        public string rail_rtCode = "";

        // コンストラクタ
        public IniFileData()
        {
            // 既定値
            SetDefData();

            // 設定ファイルがあれば読み込む
            Load();
        }

        private void SetDefData()
        {
            // 既定値
            demFilterAreaSize = 20;
            demWaterAreaDownHeight = 5;
            demSeaLevel = 40;
            roadFilterAreaSize = 10;
            waterAreaInt = 5;

            // 詳細ログの出力有無
            logOut = false;
            // 線路の動作検証用
            rail_isTestMode = false;
            rail_ftCode = -2;
            rail_railState = -2;
            rail_rtCode = "";
        }


        // 読み込み
        public void Load()
        {
            try
            {
                // ファイルが無ければ終了
                if (!File.Exists(SETTING_FILE))
                {
                    // 既定値
                    SetDefData();
                    return;
                }
                // ファイル読み込み
                FileStream fileStream = File.Open(SETTING_FILE, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(fileStream);
                string text = sr.ReadToEnd();
                string[] splitStr = text.Split('\n');
                Logger.Log("ファイル読み込み  : " + text);
                foreach (string line in splitStr)
                {
                    if (line.StartsWith("#")) continue;
                    string[] strList = line.Trim().Split('=');
                    if (strList.Length >= 2)
                    {
                        if (strList[0] == KEY_DEM_TIN_FILTER_AREASIZE) demFilterAreaSize = double.Parse(strList[1]);
                        else if (strList[0] == KEY_DEM_WATERAREA_DOWNHEIGHT) demWaterAreaDownHeight = double.Parse(strList[1]);
                        else if (strList[0] == KEY_DEM_SEA_LEVEL) demSeaLevel = double.Parse(strList[1]);
                        else if (strList[0] == KEY_TEST_LOG) logOut = int.Parse(strList[1]) > 0 ? true : false;
                        else if (strList[0] == KEY_ROAD_FILTER_AREASIZE) roadFilterAreaSize = double.Parse(strList[1]);
                        //水面コード番号読込追加
                        else if (strList[0] == KEY_WATERAREA_INT) waterAreaInt = int.Parse(strList[1]);

                        // テスト用
                        else if (strList[0] == KEY_TEST_RAIL_MODE) rail_isTestMode = int.Parse(strList[1]) > 0 ? true : false;
                        else if (strList[0] == KEY_TEST_RAIL_FTCODE) rail_ftCode = long.Parse(strList[1]);
                        else if (strList[0] == KEY_TEST_RAIL_RAILSTATE) rail_railState = long.Parse(strList[1]);
                        else if (strList[0] == KEY_TEST_RAIL_RTCODE) rail_rtCode = strList[1];

                    }
                }
                sr.Close();
                fileStream.Close();
            }
            catch (Exception ex)
            {
                Logger.Log("ファイルの読み込み失敗 : " + ex.Message);
            }
        }
    }
}
