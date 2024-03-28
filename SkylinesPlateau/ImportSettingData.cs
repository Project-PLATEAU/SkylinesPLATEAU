using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SkylinesPlateau
{
    public class ImportSettingData
    {
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
//        private const string SETTING_FILE = @"Files/SkylinesPlateau/setting.dat";
        public const string SETTING_FILE = @"Files/SkylinesPlateau/setting.dat";
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END
        private static ImportSettingData instance;
        public static ImportSettingData Instance => instance ?? (instance = new ImportSettingData());

        // インポート有無（河川）
        public bool isImpWaterway;
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
        // インポート有無（土地利用）
        public bool isImpArea;
        // 中心座標読み込み範囲
        public string center;
        // インポート範囲有無
        public bool isUseAreaSize;
        // 読み込み範囲指定[km]
        public string areaSize;
        // 区画用途が判定できない場合のプルダウン項目
        public int zoneType;
        // 平面直角座標系の系番号
        public int isystem;

        // コンストラクタ
        public ImportSettingData()
        {
            // 規定値の設定
            SetDefData();
            // 設定ファイルの読み込み
            Load();
        }

        private void SetDefData()
        {
            // インポート有無（河川）
            isImpWaterway = true;
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
            // インポート有無（土地利用）
            isImpArea = true;
            // 中心座標読み込み範囲
            center = "";
            // インポート範囲有無
            isUseAreaSize = false;
            // 読み込み範囲指定[km]
            areaSize = "";
            // 区画用途が判定できない場合のプルダウン項目
            zoneType = 0;
            // 平面直角座標系の系番号(9系)
            isystem = 8;
        }


        // 保存
        public void Save()
        {
            try
            {
                // 中心位置のトリミング処理
                trimCenterStr();

                FileStream fileStream = File.Create(SETTING_FILE);
                StreamWriter sw = new StreamWriter(fileStream);
                sw.WriteLine(isImpWaterway);
                sw.WriteLine(isImpRoad);
                sw.WriteLine(isImpRail);
                sw.WriteLine(isImpBuilding);
                sw.WriteLine(isImpUniqueBuilding);
                sw.WriteLine(isImpZone);
                sw.WriteLine(isImpArea);
                sw.WriteLine(center);
                sw.WriteLine(isUseAreaSize);
                sw.WriteLine(areaSize);
                sw.WriteLine(zoneType);
                sw.WriteLine(isystem);
                sw.Close();
                fileStream.Close();
            }
            catch (Exception ex)
            {
                Logger.Log("設定ファイルの保存失敗 : " + ex.Message);
            }
        }
        // 読み込み
        public void Load()
        {
            try
            {
                // 規定値を設定
                SetDefData();

                // ファイルが無ければ終了
                if (!File.Exists(SETTING_FILE))
                {
                    return;
                }
                // ファイル読み込み
                FileStream fileStream = File.Open(SETTING_FILE, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(fileStream);
                string text = sr.ReadToEnd();
                string[] splitStr = text.Split('\n');
                Logger.Log("設定ファイル読み込み  : " + text);
                if (splitStr.Length > 10)
                {
                    // 読み込み値をメンバに設定
                    isImpWaterway = bool.Parse(splitStr[0]);
                    isImpRoad = bool.Parse(splitStr[1]);
                    isImpRail = bool.Parse(splitStr[2]);
                    isImpBuilding = bool.Parse(splitStr[3]);
                    isImpUniqueBuilding = bool.Parse(splitStr[4]);
                    isImpZone = bool.Parse(splitStr[5]);
                    isImpArea = bool.Parse(splitStr[6]);
                    center = splitStr[7].Trim();
                    isUseAreaSize = bool.Parse(splitStr[8]); ;
                    areaSize = splitStr[9].Trim();
                    zoneType = int.Parse(splitStr[10]);
                    if (splitStr.Length > 11)
                    {
                        isystem = int.Parse(splitStr[11]);
                    }

                    // 中心位置のトリミング処理
                    trimCenterStr();
                }
                sr.Close();
                fileStream.Close();
            }
            catch (Exception ex)
            {
                Logger.Log("設定ファイルの読み込み失敗 : " + ex.Message);
                try
                {
                    // 読み込めないファイルなようなので、削除指定おく
                    File.Delete(SETTING_FILE);
                }
                catch (Exception ex2)
                {
                    Logger.Log("設定ファイルの削除失敗 : " + ex2.Message);
                }
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
                center = lat + "," + lon;
            }
        }
    }
}
