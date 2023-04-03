using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SkylinesPlateau
{
    /*
        ２．アセットテーブル

        ■asset.tbl

        # key, アセット名

　        ●「key」（string型）
　　        ・","(半角カンマ)、"/"(半角スラッシュ)は指定不可。

　        ●「アセット名」（string型）
　　        ・アセット名を指定する。（基本的には「アセットのID」+"."+「建物名称」+"_Data"の形式）
        ---------------------------------------
        （例）
　        1,1100810441.???????_Data
　        2,1337303718.???????_Data
　        3,447188028,.???????_Data
     */
    class AssetTbl
    {
        //--------------------------------------------------
        // 固定値
        //--------------------------------------------------
        public const string TBL_FILE = @"Files/SkylinesPlateau/tbl/asset.tbl";

        //--------------------------------------------------
        // メンバ変数
        //--------------------------------------------------
        // インスタンス
        private static AssetTbl instance;
        public static AssetTbl Instance => instance ?? (instance = new AssetTbl());

        // 設定値一覧
        public Dictionary<string, string> dataDic = new Dictionary<string, string>();

        //--------------------------------------------------
        // メソッド
        //--------------------------------------------------
        public AssetTbl()
        {
            Load();
        }

        // 読み込み
        public void Load()
        {
            try
            {
                dataDic.Clear();

                // ファイルが無ければ終了
                if (!File.Exists(TBL_FILE))
                {
                    return;
                }

                Logger.Log("ファイル読み込み  : " + TBL_FILE);

                // ファイル読み込み
                FileStream fileStream = File.Open(TBL_FILE, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(fileStream);
                string text = sr.ReadToEnd();
                string[] splitStr = text.Split('\n');
                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("ファイル読み込み  : " + text);
                }
                foreach (string line in splitStr)
                {
                    if (line.StartsWith("#")) continue;
                    string[] strList = line.Trim().Split(',');
                    if (strList.Length >= 2)
                    {
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log(line);
                        }
                        dataDic[strList[0]] = strList[1];
                    }
                    else
                    {
                        Logger.Log("不正レコード : " + line);
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
