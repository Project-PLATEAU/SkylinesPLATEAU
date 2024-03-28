// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
using ColossalFramework.IO;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
using System.Reflection;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END
using System.Text;

namespace SkylinesPlateau
{
    public class IniFileData
    {
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
        private const string SYSTEM_FOLDER_1 = @"Files/SkylinesPlateau";
        private const string SYSTEM_FOLDER_2 = @"Files/SkylinesPlateau/tbl";
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END
        private const string SETTING_FILE = @"Files/SkylinesPlateau/tbl/SkylinesPlateau.ini";
        private const string KEY_DEM_TIN_FILTER_AREASIZE = @"TIN除外面積";
        private const string KEY_DEM_WATERAREA_DOWNHEIGHT = @"水面補正高さ";
        private const string KEY_DEM_SEA_LEVEL = @"海面高さ";
        private const string KEY_ROAD_FILTER_AREASIZE = @"三角道路ポリゴン除外面積";
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
        private const string DIALOG_MSG_IMPTYPE_1 = "インポート有無（地形）";
        private const string DIALOG_MSG_IMPTYPE_2 = "インポート有無（高速道路）";
        private const string DIALOG_MSG_IMPTYPE_3 = "インポート有無（道路）";
        private const string DIALOG_MSG_IMPTYPE_4 = "インポート有無（線路）";
        private const string DIALOG_MSG_IMPTYPE_5 = "インポート有無（一般建築物）";
        private const string DIALOG_MSG_IMPTYPE_6 = "インポート有無（特定建築物）";
        private const string DIALOG_MSG_IMPTYPE_7 = "インポート有無（区域区分）";
        private const string DIALOG_MSG_CENTER = "中心座標読み込み範囲";
        private const string DIALOG_MSG_IMPAREA = "インポート範囲有無";
        private const string DIALOG_MSG_AREA = "読み込み範囲指定";
        private const string DIALOG_MSG_SYS = "平面直角座標系の系番号";
        private const string DIALOG_MSG_FOLDER = "３Ｄ都市モデルパス";
        private const string DIALOG_MSG_HELPURL = "マニュアルＵＲＬ";
        private const string DIALOG_HELPURL_DEF = @"https://186nobu.github.io/SkylinesPLATEAU/";
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END
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

        // 詳細ログの出力有無
        public bool logOut = false;
        // 線路の動作検証用 (INI指定値に強制的に書き換えるモード。各スタイルの確認に)
        public bool rail_isTestMode = false;
        public long rail_ftCode = -1;
        public long rail_railState = -1;
        public string rail_rtCode = "";

// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
        private bool hiddenMode1 = false;
        private bool hiddenMode2 = false;
        private bool hiddenMode3 = false;
        private bool hiddenMode4 = false;
        private bool hiddenMode5 = false;

        // インポート有無（地形）
        public bool isImpMap;
        // インポート有無（高速道路）
        public bool isImpHighway;
        // インポート有無（道路）
        public bool isImpRoad;
        // インポート有無（線路）
        public bool isImpRail;
        // インポート有無（一般建築物）
        public bool isImpBuilding;
        // インポート有無（特定建築物）
        public bool isImpUniqueBuilding;
        // インポート有無（区域区分）
        public bool isImpZone;
        // 中心座標読み込み範囲
        public string center;
        // インポート範囲有無
        public bool isUseAreaSize;
        // 読み込み範囲指定[km]
        public string areaSize;
        // 平面直角座標系の系番号
        public int isystem;
        // 区画用途が判定できない場合のプルダウン項目
        public int zoneType;
        // 地形・３Ｄ都市モデル
        public string inputFolderPath;
        // ヘルプＵＲＬ
        public string helpUrl;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END

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

            // 詳細ログの出力有無
            logOut = false;
            // 線路の動作検証用
            rail_isTestMode = false;
            rail_ftCode = -2;
            rail_railState = -2;
            rail_rtCode = "";

// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
            // インポート有無（道路）
            isImpRoad = true;
            // インポート有無（線路）
            isImpRail = true;
            // インポート有無（一般建築物）
            isImpBuilding = true;
            // インポート有無（特定建築物）
            isImpUniqueBuilding = true;
            // インポート有無（区域区分）
            isImpZone = true;
            // 中心座標読み込み範囲
            center = "36.0,140.0";
            // インポート範囲有無
            isUseAreaSize = true;
            // 読み込み範囲指定[km]
            areaSize = "6";
            // 平面直角座標系の系番号(9系)
            isystem = 8;
            // 区画用途が判定できない場合のプルダウン項目
            zoneType = 0;

            inputFolderPath = "";
            helpUrl = DIALOG_HELPURL_DEF;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END

        }


        // 読み込み
        public void Load()
        {
            try
            {
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
                // ファイルが無ければ、リソースからコピーする
                if (!File.Exists(SETTING_FILE))
                {
                    // フォルダ存在チェック
                    CreateSystemFolder();

                    Logger.Log("リソースからファイルコピー  : " + SETTING_FILE);
                    Assembly executingAssembly = Assembly.GetExecutingAssembly();
                    using (Stream resourceStream = executingAssembly.GetManifestResourceStream("SkylinesPlateau.res.tbl." + Path.GetFileName(SETTING_FILE)))
                    {
                        if (resourceStream != null)
                        {
                            // ファイルをコピー
                            using (FileStream fileStream2 = new FileStream(SETTING_FILE, FileMode.Create))
                            {
                                Logger.Log("リソースからファイルコピー完了  : " + SETTING_FILE);
                                resourceStream.CopyTo(fileStream2);
                                fileStream2.Close();
                            }
                            resourceStream.Close();
                        }
                    }
                }
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END
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
                        // 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] DEL_START
                        //                        else if (strList[0] == KEY_TEST_LOG) logOut = int.Parse(strList[1]) > 0 ? true : false;
                        // 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] DEL_END
                        else if (strList[0] == KEY_ROAD_FILTER_AREASIZE) roadFilterAreaSize = double.Parse(strList[1]);
                        // 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
                        else if (strList[0] == DIALOG_MSG_IMPTYPE_1) isImpMap = int.Parse(strList[1]) > 0 ? true : false;
                        else if (strList[0] == DIALOG_MSG_IMPTYPE_2) isImpHighway = int.Parse(strList[1]) > 0 ? true : false;
                        else if (strList[0] == DIALOG_MSG_IMPTYPE_3) isImpRoad = int.Parse(strList[1]) > 0 ? true : false;
                        else if (strList[0] == DIALOG_MSG_IMPTYPE_4) isImpRail = int.Parse(strList[1]) > 0 ? true : false;
                        else if (strList[0] == DIALOG_MSG_IMPTYPE_5) isImpBuilding = int.Parse(strList[1]) > 0 ? true : false;
                        else if (strList[0] == DIALOG_MSG_IMPTYPE_6) isImpUniqueBuilding = int.Parse(strList[1]) > 0 ? true : false;
                        else if (strList[0] == DIALOG_MSG_IMPTYPE_7) isImpZone = int.Parse(strList[1]) > 0 ? true : false;
                        else if (strList[0] == DIALOG_MSG_CENTER) center = strList[1];
                        else if (strList[0] == DIALOG_MSG_IMPAREA) isUseAreaSize = int.Parse(strList[1]) > 0 ? true : false;
                        else if (strList[0] == DIALOG_MSG_AREA) areaSize = strList[1];
                        else if (strList[0] == DIALOG_MSG_SYS) isystem = int.Parse(strList[1]);
                        else if (strList[0] == DIALOG_MSG_FOLDER) inputFolderPath = strList[1];
                        else if (strList[0] == DIALOG_MSG_HELPURL) helpUrl = DIALOG_HELPURL_DEF;// strList[1];
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END

// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
                        // テスト用
                        else if (strList[0] == KEY_TEST_RAIL_MODE)
                        {
                            rail_isTestMode = int.Parse(strList[1]) > 0 ? true : false;
                            hiddenMode1 = true;
                        }
                        else if (strList[0] == KEY_TEST_RAIL_FTCODE)
                        {
                            rail_ftCode = long.Parse(strList[1]);
                            hiddenMode2 = true;
                        }
                        else if (strList[0] == KEY_TEST_RAIL_RAILSTATE)
                        {
                            rail_railState = long.Parse(strList[1]);
                            hiddenMode3 = true;
                        }
                        else if (strList[0] == KEY_TEST_RAIL_RTCODE)
                        {
                            rail_rtCode = strList[1];
                            hiddenMode4 = true;
                        }
                        else if (strList[0] == KEY_TEST_LOG)
                        {
                            logOut = int.Parse(strList[1]) > 0 ? true : false;
                            hiddenMode5 = true;
                        }
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END
                    }
                }
                sr.Close();
                fileStream.Close();
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
                trimCenterStr();
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END

// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
                // DATファイルがあればIniファイルに設定値を引き継ぐ
                LoadDatFile();

                // ヘルプのURLが存在しない場合、ひとまず固定値を設定しておく
                if (helpUrl.Length == 0) helpUrl = DIALOG_HELPURL_DEF;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END
            }
            catch (Exception ex)
            {
                Logger.Log("ファイルの読み込み失敗 : " + ex.Message);
                Logger.Log("ファイルの読み込み失敗 : " + ex);
            }
        }

// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
        // 書き込み
        public void Save()
        {
            try
            {
                trimCenterStr();
                // ファイル書き込み
                FileStream fileStream = File.Create(SETTING_FILE);
                StreamWriter sw = new StreamWriter(fileStream);
                sw.WriteLine(KEY_DEM_TIN_FILTER_AREASIZE + "=" + demFilterAreaSize);
                sw.WriteLine(KEY_DEM_WATERAREA_DOWNHEIGHT + "=" + demWaterAreaDownHeight);
                sw.WriteLine(KEY_DEM_SEA_LEVEL + "=" + demSeaLevel);
                sw.WriteLine(KEY_ROAD_FILTER_AREASIZE + "=" + roadFilterAreaSize);
                sw.WriteLine(DIALOG_MSG_IMPTYPE_1 + "=" + (isImpMap ? 1 : 0));
                sw.WriteLine(DIALOG_MSG_IMPTYPE_2 + "=" + (isImpHighway ? 1 : 0));
                sw.WriteLine(DIALOG_MSG_IMPTYPE_3 + "=" + (isImpRoad ? 1 : 0));
                sw.WriteLine(DIALOG_MSG_IMPTYPE_4 + "=" + (isImpRail ? 1 : 0));
                sw.WriteLine(DIALOG_MSG_IMPTYPE_5 + "=" + (isImpBuilding ? 1 : 0));
                sw.WriteLine(DIALOG_MSG_IMPTYPE_6 + "=" + (isImpUniqueBuilding ? 1 : 0));
                sw.WriteLine(DIALOG_MSG_IMPTYPE_7 + "=" + (isImpZone ? 1 : 0));
                sw.WriteLine(DIALOG_MSG_CENTER + "=" + center);
                sw.WriteLine(DIALOG_MSG_IMPAREA + "=" + (isUseAreaSize ? 1 : 0));
                sw.WriteLine(DIALOG_MSG_AREA + "=" + areaSize);
                sw.WriteLine(DIALOG_MSG_SYS + "=" + isystem);
                sw.WriteLine(DIALOG_MSG_FOLDER + "=" + inputFolderPath);
                sw.WriteLine(DIALOG_MSG_HELPURL + "=" + helpUrl);
                // テスト用
                if (hiddenMode1) sw.WriteLine(KEY_TEST_RAIL_MODE + "=" + (rail_isTestMode ? 1 : 0));
                if (hiddenMode2) sw.WriteLine(KEY_TEST_RAIL_FTCODE + "=" + rail_ftCode);
                if (hiddenMode3) sw.WriteLine(KEY_TEST_RAIL_RAILSTATE + "=" + rail_railState);
                if (hiddenMode4) sw.WriteLine(KEY_TEST_RAIL_RTCODE + "=" + rail_rtCode);
                if (hiddenMode5) sw.WriteLine(KEY_TEST_LOG + "=" + (logOut ? 1 : 0));
                sw.Close();
                fileStream.Close();;
            }
            catch (Exception ex)
            {
                Logger.Log("ファイルの書き込み失敗 : " + ex.Message);
            }
        }

        private void trimCenterStr()
        {
            // 中央の座標値の桁数チェック
            double lat = 0.0, lon = 0.0;
            string[] splitStr = center.Split(',');
            if (splitStr.Length > 1)
            {
                lat = double.Parse(splitStr[0]);
                lon = double.Parse(splitStr[1]);
                // 小数点以下６桁で丸める
                lat = (Math.Floor(lat * 1000000) / 1000000);
                lon = (Math.Floor(lon * 1000000) / 1000000);
                // 文字列として再設定
                center = lat.ToString("F6") + "," + lon.ToString("F6");
            }
        }

        private void LoadDatFile() {
            // DATファイルの移行処理
            if (!File.Exists(ImportSettingData.SETTING_FILE))
            {
                return;
            }

            Logger.Log("DATファイルが存在するため、設定値をIniファイルに移行する。 : " + ImportSettingData.SETTING_FILE);
            // インポート有無（道路）
            isImpRoad = ImportSettingData.Instance.isImpRoad;
            // インポート有無（線路）
            isImpRail = ImportSettingData.Instance.isImpRail;
            // インポート有無（一般建築物）
            isImpBuilding = ImportSettingData.Instance.isImpBuilding;
            // インポート有無（特定建築物）
            isImpUniqueBuilding = ImportSettingData.Instance.isImpUniqueBuilding;
            // インポート有無（区域区分）
            isImpZone = ImportSettingData.Instance.isImpZone;
            // 中心座標読み込み範囲
            center = ImportSettingData.Instance.center;
            // インポート範囲有無
            isUseAreaSize = ImportSettingData.Instance.isUseAreaSize;
            // 読み込み範囲指定[km]
            areaSize = ImportSettingData.Instance.areaSize;
            // 平面直角座標系の系番号(9系)
            isystem = ImportSettingData.Instance.isystem;
            // 区画用途が判定できない場合のプルダウン項目
            zoneType = ImportSettingData.Instance.zoneType;

            Logger.Log("データ移行したため、DATファイルは削除する。 : " + ImportSettingData.SETTING_FILE);
            File.Delete(ImportSettingData.SETTING_FILE);

            // 設定した内容をIniファイルに上書き保存
            Save();
        }

        static public void CreateSystemFolder()
        {
            if (!System.IO.Directory.Exists(SYSTEM_FOLDER_1))
            {
                Directory.CreateDirectory(SYSTEM_FOLDER_1);
            }
            if (!System.IO.Directory.Exists(SYSTEM_FOLDER_2))
            {
                Directory.CreateDirectory(SYSTEM_FOLDER_2);
            }
        }
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END
    }
}
