using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SkylinesPlateau
{
    /*
        ３．区画テーブル
        ■districtsAndZonesType.tbl

        # key, 区画番号

　        ●「key」（string型）
　　        ・<urf:function>タグの値と比較される。
　　
　        ●「区画番号」（int型）
　　        ・区画番号を指定
     */
    class ZoneSgTbl
    {
        //--------------------------------------------------
        // 固定値
        //--------------------------------------------------
        public const string TBL_FILE = @"Files/SkylinesPlateau/tbl/districtsAndZonesType.tbl";

        //--------------------------------------------------
        // メンバ変数
        //--------------------------------------------------
        // 設定値一覧
        public Dictionary<string, int> dataDic = new Dictionary<string, int>();

        //--------------------------------------------------
        // メソッド
        //--------------------------------------------------
        public ZoneSgTbl()
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
                        dataDic[strList[0]] = int.Parse(strList[1]);
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
