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

// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
                // ファイルが無ければ、リソースからコピーする
                if (!File.Exists(TBL_FILE))
                {
                    // フォルダ存在チェック
                    IniFileData.CreateSystemFolder();

                    Logger.Log("リソースからファイルコピー  : " + TBL_FILE);
                    Assembly executingAssembly = Assembly.GetExecutingAssembly();
                    using (Stream resourceStream = executingAssembly.GetManifestResourceStream("SkylinesPlateau.res.tbl." + Path.GetFileName(TBL_FILE)))
                    {
                        if (resourceStream != null)
                        {
                            // ファイルをコピー
                            using (FileStream fileStream2 = new FileStream(TBL_FILE, FileMode.Create))
                            {
                                Logger.Log("リソースからファイルコピー完了  : " + TBL_FILE);
                                resourceStream.CopyTo(fileStream2);
                                fileStream2.Close();
                            }
                            resourceStream.Close();
                        }
                    }
                }
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END

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
