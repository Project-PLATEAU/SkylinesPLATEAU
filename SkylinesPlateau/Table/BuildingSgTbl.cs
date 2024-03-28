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
        １．建物テーブル

        ■detailedUsage.tbl
        ■orgUsage.tbl
        ■usage.tbl
        ■buildingID.tbl

        # key, 建物名称, 種類名称, 高さ, 面積, アセット, デフォルトアセット

　        ●「key」「建物名称」「種類名称」（string型）
　　        ・「key」「建物名称」「種類名称」のいずれも指定がない設定は無効とする。
　　        ・「key」は、detailedUsage.tblの場合、<uro:detailedUsage>タグの値と比較される。
　　        ・「key」は、orgUsage.tblの場合、<uro:orgUsage>タグの値と比較される。
　　        ・「key」は、usage.tblの場合、<bldg:usage>タグの値と比較される。
　　        ・「key」は、buildingID.tblの場合、<buro:buildingID>タグの値と比較される。
　　        ・「建物名称」は、<tran:name>タグの値に、指定文字が含まれるか判定される。
　　        ・「種類名称」は、「key」で参照するタグの属性値codeSpaceで指定されたXMLファイルから、該当する<gml:description>タグを取得し、指定文字が含まれるか判定される。
　　        ・判定条件の優先順は、「key」＞「建物名称」＞「種類名称」とする。
　　　　        優先度１：「key」有、「建物名称」有、「種類名称」有
　　　　        優先度２：「key」有、「建物名称」有、「種類名称」　
　　　　        優先度３：「key」有、「建物名称」　、「種類名称」有
　　　　        優先度４：「key」有、「建物名称」　、「種類名称」　
　　　　        優先度５：「key」　、「建物名称」有、「種類名称」有
　　　　        優先度６：「key」　、「建物名称」有、「種類名称」　
　　　　        優先度７：「key」　、「建物名称」　、「種類名称」有

　        ●「高さ」「面積」（double型）
　　        ・「高さ」はメートル単位とし、指定した高さ以上の設定で最も小さい高さの設定を有効とする。
　　        ・「面積」は平方メートル単位とし、指定した面積以上の設定で最も小さい面積の設定を有効とする。
　　        ・「高さ」、「面積」どちらも指定した場合、両方の値で判定し、高さ＞面積の優先度で、最も小さい設定を有効とする。

　        ●「アセット」「デフォルトアセット」（string型）
　　        ・"/"(半角スラッシュ)区切りで複数指定可能とし、複数ある場合にはランダムで採用される。
　　        ・以下の３つの値を指定可能。
　　　        ・拡張アセット：asset.tblのkey値を指定
　　　        ・標準アセット：先頭に"B"を付与した建物番号を指定（例：B628 <- 墓地）
　　　　　　　　　　　　        ⇒建物番号はDLLを解析し一覧を作成予定。
　　　        ・区画　　　　：先頭に"Z"を付与した区画番号を指定（例：Z2   <- 低密度住宅）
　　　　　　　　　　　　        ⇒（2:低密度住宅, 3:高密度住宅, 4=低密度商業, 5=高密度商業, 6=工業, 7=オフィス）
　　        ・「アセット」に指定されたアセットがゲーム上に存在しない場合、空欄や"none"の場合には「デフォルトアセット」を有効とする。
　　        ・「デフォルトアセット」が有効で、指定されたアセットがゲーム上に存在しない場合、空欄や"none"の場合には、次のタグを判定対象とする。
　　　　        （detailedUsage.tblで判定していた場合、orgUsage.tblでの判定処理に移行する）

        ---------------------------------------
        （例）
　        421,,,,,1/2,B??(市庁舎の番号)
　        422,小学校,,,,3,B??(小学校の番号)
　        422,中学校,,,,3,B??(中学校の番号)
　        422,高等学校,,,,3,B??(高校の番号)
　        422,,,,,3,B??(高校の番号)
　        422701,,,,,4,Z4
     */

    class BuildingSgTblData
    {
        //--------------------------------------------------
        // メンバ変数
        //--------------------------------------------------
// 2022.11.18 G.Arakawa@cmind [建物TBL検索時、複数ヒットしたら先頭レコードを優先する] ADD_START
		// テーブル登録順
		public ulong dataNo = 0;
// 2022.11.18 G.Arakawa@cmind [建物TBL検索時、複数ヒットしたら先頭レコードを優先する] ADD_END
        // key
        public string key = "";
    	// 建物名称
        public string nameStr = "";
    	// 種類名称
        public string typeStr = "";
    	// 高さ
        public double height = 0.0;
    	// 面積
        public double area = 0.0;
    	// アセット
        public string asset = "";
    	// デフォルトアセット
        public string def_asset = "";

        //--------------------------------------------------
        // メソッド
        //--------------------------------------------------
// 2022.11.18 G.Arakawa@cmind [建物TBL検索時、複数ヒットしたら先頭レコードを優先する] UPD_START
//        public BuildingSgTblData(string key, string nameStr, string typeStr, string height, string area, string asset, string def_asset)
        public BuildingSgTblData(ulong dataNo, string key, string nameStr, string typeStr, string height, string area, string asset, string def_asset)
// 2022.11.18 G.Arakawa@cmind [建物TBL検索時、複数ヒットしたら先頭レコードを優先する] UPD_END
        {
// 2022.11.18 G.Arakawa@cmind [建物TBL検索時、複数ヒットしたら先頭レコードを優先する] ADD_START
        	this.dataNo = dataNo;
// 2022.11.18 G.Arakawa@cmind [建物TBL検索時、複数ヒットしたら先頭レコードを優先する] ADD_END
            this.key = key;
            this.nameStr = nameStr;
            this.typeStr = typeStr;
            this.height = height.Length == 0 ? 0 : double.Parse(height);
            this.area = area.Length == 0 ? 0 : double.Parse(area);
            this.asset = asset;
            this.def_asset = def_asset;
        }

// 2022.11.18 G.Arakawa@cmind [建物TBL検索時、複数ヒットしたら先頭レコードを優先する] UPD_START
/*
        // 最も優先度の高い設定値を返却する
        static public BuildingSgTblData getBestSgTblData(List<BuildingSgTblData> list)
        {
            // 項目が無い場合
            if (list == null || list.Count == 0) return null;
            // 項目が１件しかない場合
            if (list.Count == 1) return list[0];

            //--------------------------------------------------
            // ソート処理
            //  1.降順：Key        （null, 空文字を後方にするため降順）
            //  2.降順：nmeStr     （null, 空文字を後方にするため降順）
            //  3.降順：typeStr    （null, 空文字を後方にするため降順）
            //  4.降順：height     （null, 空文字を後方にするため降順）
            //  5.降順：area
            //--------------------------------------------------
            IOrderedEnumerable<BuildingSgTblData> sortList = list.OrderByDescending(rec => rec.key).ThenByDescending(rec => rec.nameStr).ThenByDescending(rec => rec.typeStr).ThenByDescending(rec => rec.height).ThenByDescending(rec => rec.area);
            foreach (BuildingSgTblData rec in sortList)
            {
                return rec;
            }

            return null;
        }
*/
        // 最も優先度の高い設定値を返却する
        static public BuildingSgTblData getBestSgTblData(List<BuildingSgTblData> list)
        {
            if (IniFileData.Instance.logOut)
            {
                Logger.Log("優先度の高いアセット判定");
            }

            //---------------------------------------------
            // 項目が無い場合
            //---------------------------------------------
            if (list == null || list.Count == 0)
            {
                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("アセットなし");
                }
                return null;
            }

            //---------------------------------------------
            // 項目が１件しかない場合
            //---------------------------------------------
            if (list.Count == 1)
            {
                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("アセット１件のみ：" + list[0]);
                }
                return list[0];
            }

            //---------------------------------------------
            // 複数項目が存在する場合 (Key指定ありをチェック)
            //---------------------------------------------
            var filterList = list.FindAll(item =>
            {
                //---------------------------------------------
                // 空白データを除外
                //---------------------------------------------
                if (item.nameStr == "") return false;
                return true;
            });
            if (filterList.Count == 1)
            {
                //---------------------------------------------
                // 空白データ以外のデータ１件のみ
                // ⇒確定
                //---------------------------------------------
                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("[空白除外後] アセット１件のみ：" + filterList[0]);
                }
                return filterList[0];
            }
            else if (filterList.Count > 1)
            {
                //---------------------------------------------
                // 空白データ以外のデータ１件以上
                // ⇒テーブル順のトップと同じKeyを、高さ＆面積で判定
                //---------------------------------------------
                // テーブル順でソートし、先頭データ取得
                var firstData = filterList.OrderBy(rec => rec.dataNo).FirstOrDefault();
                // 先頭データのKey名でフィルタリング
                filterList = filterList.FindAll(item =>
                {
                    if (item.nameStr == firstData.nameStr) return true;
                    return false;
                });
                // 高さ降順、面積降順、テーブル昇順でソート
                IOrderedEnumerable<BuildingSgTblData> sortList = filterList.OrderByDescending(rec => rec.height).ThenByDescending(rec => rec.area).ThenBy(rec => rec.dataNo);

                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("[空白除外後] ソートした先頭：" + sortList.FirstOrDefault());
                }

                // 先頭データを返却
                return sortList.FirstOrDefault();
            }

            //---------------------------------------------
            // 複数項目が存在する場合 (Key指定なし)
            //---------------------------------------------
            filterList = list.FindAll(item =>
            {
                //---------------------------------------------
                // 空白データのみ残す
                //---------------------------------------------
                if (item.nameStr != "") return false;
                return true;
            });
            if (filterList.Count == 1)
            {
                //---------------------------------------------
                // 空白データ１件のみ
                // ⇒確定
                //---------------------------------------------
                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("[空白を対象] アセット１件のみ：" + filterList[0]);
                }
                return filterList[0];
            }
            else if (filterList.Count > 1)
            {
                //---------------------------------------------
                // 空白データ１件以上
                // ⇒高さ＆面積で判定
                //---------------------------------------------
                // 高さ降順、面積降順、テーブル昇順でソート
                IOrderedEnumerable<BuildingSgTblData> sortList = filterList.OrderByDescending(rec => rec.height).ThenByDescending(rec => rec.area).ThenBy(rec => rec.dataNo);

                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("[空白を対象] ソートした先頭：" + sortList.FirstOrDefault());
                }

                // 先頭データを返却
                return sortList.FirstOrDefault();
            }

            // ここに来ることはない
            return null;
        }
// 2022.11.18 G.Arakawa@cmind [建物TBL検索時、複数ヒットしたら先頭レコードを優先する] UPD_END

        // メンバで設定されている値とマッチするか判定
        public bool checkSgTblData(string key, string nameStr, string typeStr, double height, double area)
        {
            // Key
            if ( this.key != "" && this.key != key ) return false;
            // 建物名称 <tran:name>
            if (this.nameStr != "")
            {
                if (nameStr == "")
                {
                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log("アセット不一致  : " + this.nameStr + "!=" + nameStr);
                    }
                    return false;
                }
                if (!nameStr.Contains(this.nameStr))
                {
                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log("アセット不一致  : " + this.nameStr + "!=" + nameStr);
                    }
                    return false;
                }
            }
            // 種類名称 CodeListの<gml:description>
            if (this.typeStr != "")
            {

                if (typeStr == "")
                {
                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log("アセット不一致  : " + this.typeStr + "!=" + typeStr);
                    }
                    return false;
                }
                if (!typeStr.Contains(this.typeStr))
                {
                    if (IniFileData.Instance.logOut)
                    {
                        Logger.Log("アセット不一致  : " + this.typeStr + "!=" + typeStr);
                    }
                    return false;
                }
            }
        	// 高さ
        	if ( this.height > height ) return false;
        	// 面積
        	if ( this.area > area ) return false;

            Logger.Log("アセット一致  : " + this.key + "=" + key + ", " +
                                            this.nameStr + "=" + nameStr + ", " +
                                            this.typeStr + "=" + typeStr + ", " +
                                            this.height + "=" + height + ", " +
                                            this.area + "=" + area + ", " +
                                            this.asset + ", " + this.def_asset);

            return true;
        }

    	// アセット名を取得
        public string getAssetName()
        {
            string tmpStr = "";

            // アセット指定がある場合
            if ( this.asset != "" )
            {
                Logger.Log("アセット判定  : " + this.asset);
                // アセット指定を分割する
                List<string> list = this.asset.Split('/').ToList();
        		while(list.Count > 0)
        		{
        			Random random = new Random();
					int rnd = random.Next(list.Count);

                    // 区画指定の場合
                    if (BuildingSgTblData.CheckZoneNo(list[rnd]) != 0)
                    {
                        Logger.Log("アセット確定  : " + list[rnd]);
                        return list[rnd];
                    }

                    // アセットリストから取得
                    tmpStr = GetAssetName(list[rnd]);
                    if (tmpStr != "" && tmpStr != "none")
                    {
                        // アセットが存在するか判定
                        if (PrefabCollection<BuildingInfo>.LoadedExists(tmpStr))
                        {
                            Logger.Log("アセット確定  : " + tmpStr);
                            return tmpStr;
                        }
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("対象アセットなし  : " + tmpStr);
                        }
                    }
        			// アセットが存在しないためリストから除外
        			list.RemoveAt(rnd);
        		}
        	}

            // アセット指定がある場合
            if (this.def_asset != "")
            {
                Logger.Log("アセット判定(DEF)  : " + this.def_asset);
                // アセット指定を分割する
                List<string> list = this.def_asset.Split('/').ToList();
                while (list.Count > 0)
                {
                    Random random = new Random();
                    int rnd = random.Next(list.Count);

                    // 区画指定の場合
                    if (BuildingSgTblData.CheckZoneNo(list[rnd]) != 0)
                    {
                        Logger.Log("アセット確定  : " + list[rnd]);
                        return list[rnd];
                    }

                    // アセットリストから取得
                    tmpStr = GetAssetName(list[rnd]);
                    if (tmpStr != "" && tmpStr != "none")
                    {
                        // アセットが存在するか判定
                        if (PrefabCollection<BuildingInfo>.LoadedExists(tmpStr))
                        {
                            Logger.Log("アセット確定  : " + tmpStr);
                            return tmpStr;
                        }
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("対象アセットなし  : " + tmpStr);
                        }
                    }
                    // アセットが存在しないためリストから除外
                    list.RemoveAt(rnd);
                }
            }

            Logger.Log("アセットなし");
            return "";
        }

        // アセットIDからアセット名を取得
        private string GetAssetName(string assetId)
        {
            // 該当IDが含まれているかチェック
            if (!AssetTbl.Instance.dataDic.ContainsKey(assetId))
            {
                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("AssetTblに該当コードなし  : " + assetId);
                }
                return "";
            }
            return AssetTbl.Instance.dataDic[assetId];
        }

        // 該当アセットが区画番号か判定 (return 0:区画以外, 2-7:区画番号)
        static public int CheckZoneNo(string assetId)
        {
            if (assetId.Length != 2) return 0;
            if (assetId == "Z2") return 2;   // 2:低密度住宅
            if (assetId == "Z3") return 3;   // 3:高密度住宅
            if (assetId == "Z4") return 4;   // 4=低密度商業
            if (assetId == "Z5") return 5;   // 5=高密度商業
            if (assetId == "Z6") return 6;   // 6=工業
            if (assetId == "Z7") return 7;   // 7=オフィス

            return 0;
        }
    }

    class BuildingSgTbl
    {
        //--------------------------------------------------
        // 固定値
        //--------------------------------------------------
        public const string TBL_FILE_DETAILED_USAGE = @"Files/SkylinesPlateau/tbl/detailedUsage.tbl";
        public const string TBL_FILE_ORG_USAGE2 = @"Files/SkylinesPlateau/tbl/orgUsage2.tbl";
        public const string TBL_FILE_ORG_USAGE = @"Files/SkylinesPlateau/tbl/orgUsage.tbl";
        public const string TBL_FILE_USAGE = @"Files/SkylinesPlateau/tbl/usage.tbl";
        public const string TBL_FILE_BUILDING_ID = @"Files/SkylinesPlateau/tbl/buildingID.tbl";
        public const string TBL_FILE_BUILDING_NAME = @"Files/SkylinesPlateau/tbl/bldgname.tbl";

        //--------------------------------------------------
        // メンバ変数
        //--------------------------------------------------
        // 設定値一覧
        private List<BuildingSgTblData> _dataList_DetailedUsage = null;
        private List<BuildingSgTblData> _dataList_OrgUsage2 = null;
        private List<BuildingSgTblData> _dataList_OrgUsage = null;
        private List<BuildingSgTblData> _dataList_Usage = null;
        private List<BuildingSgTblData> _dataList_BuildingId = null;
        private List<BuildingSgTblData> _dataList_BuildingName = null;

        //--------------------------------------------------
        // メソッド
        //--------------------------------------------------
        public BuildingSgTbl()
        {
            // テーブル展開
            Load();
        }

        /// <summary>
        /// ＳＧテーブルから該当するアセット名を取得する
        /// </summary>
        /// <param name="bdata">建物データ</param>
        /// <param name="isReadUserData">ユーザ独自のアセットを読み込むか判定フラグ</param>
        /// <returns></returns>
        public string GetAssetName(GmlBuildingData bdata)
        {
            string rtnStr = "";

            if (IniFileData.Instance.logOut)
            {
                Logger.Log("アセット名の検索開始  : " + bdata.buildingID + ", " +
                                                 bdata.name + ", " +
                                                 bdata.detailedUsage + " : " + bdata.detailedUsageValue + ", " +
                                                 bdata.orgUsage2 + " : " + bdata.orgUsage2Value + ", " +
                                                 bdata.orgUsage + " : " + bdata.orgUsageValue + ", " +
                                                 bdata.usage + " : " + bdata.usageValue + ", " +
                                                 bdata.height + ", " + bdata.polyAreaSize);
            }

            //------------------------------------------
            // 主要建物を読み込む場合
            //------------------------------------------
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
//            if (ImportSettingData.Instance.isImpUniqueBuilding)
            if (IniFileData.Instance.isImpUniqueBuilding)
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END
            {
                // 【優先度０】buildingID
                Logger.Log("buildingIDテーブルの検索開始");
                rtnStr = GetAssetName(_dataList_BuildingId, bdata.buildingID, "", bdata);
                if (rtnStr != "") return rtnStr;

                // 【優先度１】bldgname
                Logger.Log("bldgnameテーブルの検索開始");
                rtnStr = GetAssetName(_dataList_BuildingName, bdata);
                if (rtnStr != "") return rtnStr;

                // 【優先度２】detailedUsage
                Logger.Log("detailedUsageテーブルの検索開始");
                rtnStr = GetAssetName(_dataList_DetailedUsage, bdata.detailedUsage, bdata.detailedUsageValue, bdata);
                if (rtnStr != "") return rtnStr;

                // 【優先度３】orgUsage2
                Logger.Log("orgUsage2テーブルの検索開始");
                rtnStr = GetAssetName(_dataList_OrgUsage2, bdata.orgUsage2, bdata.orgUsage2Value, bdata);
                if (rtnStr != "") return rtnStr;

                // 【優先度４】orgUsage
                Logger.Log("orgUsageテーブルの検索開始");
                rtnStr = GetAssetName(_dataList_OrgUsage, bdata.orgUsage, bdata.orgUsageValue, bdata);
                if (rtnStr != "") return rtnStr;
                Logger.Log("テーブルとヒットせず");
            }
            //------------------------------------------
            // 一般建物を読み込む場合
            //------------------------------------------
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
//            if (ImportSettingData.Instance.isImpBuilding)
            if (IniFileData.Instance.isImpBuilding)
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END
            {
                // 【優先度５】usage
                // ※ID指定がない場合でも対象とする
                Logger.Log("usageテーブルの検索開始");
                rtnStr = GetAssetName(_dataList_Usage, bdata.usage, bdata.usageValue, bdata, false);
                if (rtnStr != "") return rtnStr;
            }

            return rtnStr;
        }

        /// <summary>
        /// アセット名を取得する (ID, 種類名称を指定しないパターン）
        /// </summary>
        /// <param name="list">検索対象の建物TBLリスｔ</param>
        /// <param name="bdata">対象建物</param>
        /// <returns></returns>
        private string GetAssetName(List<BuildingSgTblData> list, GmlBuildingData bdata)
        {
            string rtnStr = "";

            // テーブルからヒットするレコードを取得
            var filterList = list.FindAll(item =>
            {
                return item.checkSgTblData("", bdata.name, "", bdata.height, bdata.polyAreaSize);
            });
            // ヒットレコードから最適なレコードを取得
            var bestData = BuildingSgTblData.getBestSgTblData(filterList);
            // アセット名を取得
            if (bestData != null)
            {
                rtnStr = bestData.getAssetName();
            }

            return rtnStr;
        }

        /// <summary>
        /// アセット名を取得する
        /// </summary>
        /// <param name="list">検索対象の建物TBLリスｔ</param>
        /// <param name="idStr">対象建物のＫｅｙ値</param>
        /// <param name="typeStr">対象建物の種類名称</param>
        /// <param name="bdata">対象建物</param>
        /// <returns></returns>
        private string GetAssetName(List<BuildingSgTblData> list, string idStr, string typeStr, GmlBuildingData bdata, bool isIdChk=true)
        {
            string rtnStr = "";

            if (isIdChk)
            {
                // ID指定なし
                if (idStr == "") return rtnStr;
            }

            // テーブルからヒットするレコードを取得
            var filterList = list.FindAll(item =>
            {
                return item.checkSgTblData(idStr, bdata.name, typeStr, bdata.height, bdata.polyAreaSize);
            });
            // ヒットレコードから最適なレコードを取得
            var bestData = BuildingSgTblData.getBestSgTblData(filterList);
            // アセット名を取得
            if (bestData != null)
            {
                rtnStr = bestData.getAssetName();
            }

            return rtnStr;
        }

        /// <summary>
        /// 建物用の外部テーブルを読み込む
        /// </summary>
        private void Load()
        {
            try
            {
                _dataList_DetailedUsage = Load(TBL_FILE_DETAILED_USAGE);
                _dataList_OrgUsage2 = Load(TBL_FILE_ORG_USAGE2);
                _dataList_OrgUsage = Load(TBL_FILE_ORG_USAGE);
                _dataList_Usage = Load(TBL_FILE_USAGE);
                _dataList_BuildingId = Load(TBL_FILE_BUILDING_ID);
                _dataList_BuildingName = Load2(TBL_FILE_BUILDING_NAME);
            }
            catch (Exception ex)
            {
                Logger.Log("ファイルの読み込み失敗 : " + ex.Message);
            }
        }

        /// <summary>
        /// 建物用の外部テーブルを読み込む
        /// </summary>
        /// <param name="fileName">外部テーブル名</param>
        /// <returns></returns>
        private List<BuildingSgTblData> Load(string fileName)
        {
            List<BuildingSgTblData> rtnDataList = new List<BuildingSgTblData>();
            try
            {
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
                // ファイルが無ければ、リソースからコピーする
                if (!File.Exists(fileName))
                {
                    // フォルダ存在チェック
                    IniFileData.CreateSystemFolder();

                    Logger.Log("リソースからファイルコピー  : " + fileName);
                    Assembly executingAssembly = Assembly.GetExecutingAssembly();
                    using (Stream resourceStream = executingAssembly.GetManifestResourceStream("SkylinesPlateau.res.tbl." + Path.GetFileName(fileName)))
                    {
                        if (resourceStream != null)
                        {
                            // ファイルをコピー
                            using (FileStream fileStream2 = new FileStream(fileName, FileMode.Create))
                            {
                                Logger.Log("リソースからファイルコピー完了  : " + fileName);
                                resourceStream.CopyTo(fileStream2);
                                fileStream2.Close();
                            }
                            resourceStream.Close();
                        }
                    }
                }
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END

                // ファイルが無ければ終了
                if (!File.Exists(fileName))
                {
                    return rtnDataList;
                }

                Logger.Log("ファイル読み込み  : " + fileName);

                // ファイル読み込み
                FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(fileStream);
                string text = sr.ReadToEnd();
                string[] splitStr = text.Split('\n');
// 2022.11.18 G.Arakawa@cmind [建物TBL検索時、複数ヒットしたら先頭レコードを優先する] ADD_START
                ulong dataCount = 0;
// 2022.11.18 G.Arakawa@cmind [建物TBL検索時、複数ヒットしたら先頭レコードを優先する] ADD_END
                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("ファイル読み込み  : " + text);
                }
                foreach (string line in splitStr)
                {
                    try
                    {
                        if (line.StartsWith("#")) continue;
                        if (line.Length == 0) continue;
                        string[] strList = line.Trim().Split(',');
                        if (strList.Length >= 7)
                        {
                            if (IniFileData.Instance.logOut)
                            {
                                Logger.Log(line);
                            }
// 2022.11.18 G.Arakawa@cmind [建物TBL検索時、複数ヒットしたら先頭レコードを優先する] ADD_START
                            dataCount++;
// 2022.11.18 G.Arakawa@cmind [建物TBL検索時、複数ヒットしたら先頭レコードを優先する] ADD_END
                            rtnDataList.Add(new BuildingSgTblData(dataCount, strList[0], strList[1], strList[2], strList[3], strList[4], strList[5], strList[6]));
                        }
                        else
                        {
                            Logger.Log("不正レコード : " + line);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("不正レコード : " + line + " : " + ex.Message);
                        rtnDataList.Clear();
                    }
                }
                sr.Close();
                fileStream.Close();
            }
            catch (Exception ex)
            {
                Logger.Log("ファイルの読み込み失敗 : " + ex.Message);
                rtnDataList.Clear();
            }
            return rtnDataList;
        }

        /// <summary>
        /// 建物用の外部テーブルを読み込む (bldgname.tbl用、項目にIDが無いバージョン)
        /// </summary>
        /// <param name="fileName">外部テーブル名</param>
        /// <returns></returns>
        private List<BuildingSgTblData> Load2(string fileName)
        {
            List<BuildingSgTblData> rtnDataList = new List<BuildingSgTblData>();
            try
            {
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
                // ファイルが無ければ、リソースからコピーする
                if (!File.Exists(fileName))
                {
                    // フォルダ存在チェック
                    IniFileData.CreateSystemFolder();

                    Logger.Log("リソースからファイルコピー  : " + fileName);
                    Assembly executingAssembly = Assembly.GetExecutingAssembly();
                    using (Stream resourceStream = executingAssembly.GetManifestResourceStream("SkylinesPlateau.res.tbl." + Path.GetFileName(fileName)))
                    {
                        if (resourceStream != null)
                        {
                            // ファイルをコピー
                            using (FileStream fileStream2 = new FileStream(fileName, FileMode.Create))
                            {
                                Logger.Log("リソースからファイルコピー完了  : " + fileName);
                                resourceStream.CopyTo(fileStream2);
                                fileStream2.Close();
                            }
                            resourceStream.Close();
                        }
                    }
                }
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END
                // ファイルが無ければ終了
                if (!File.Exists(fileName))
                {
                    return rtnDataList;
                }

                Logger.Log("ファイル読み込み  : " + fileName);

                // ファイル読み込み
                FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(fileStream);
                string text = sr.ReadToEnd();
                string[] splitStr = text.Split('\n');
// 2022.11.18 G.Arakawa@cmind [建物TBL検索時、複数ヒットしたら先頭レコードを優先する] ADD_START
                ulong dataCount = 0;
// 2022.11.18 G.Arakawa@cmind [建物TBL検索時、複数ヒットしたら先頭レコードを優先する] ADD_END
                if (IniFileData.Instance.logOut)
                {
                    Logger.Log("ファイル読み込み  : " + text);
                }
                foreach (string line in splitStr)
                {
                    try
                    {
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log(line);
                        }
                        if (line.StartsWith("#")) continue;
                        if (line.Length == 0) continue;
                        string[] strList = line.Trim().Split(',');
                        if (strList.Length >= 6)
                        {
// 2022.11.18 G.Arakawa@cmind [建物TBL検索時、複数ヒットしたら先頭レコードを優先する] ADD_START
                            dataCount++;
// 2022.11.18 G.Arakawa@cmind [建物TBL検索時、複数ヒットしたら先頭レコードを優先する] ADD_END
                            rtnDataList.Add(new BuildingSgTblData(dataCount, "", strList[0], strList[1], strList[2], strList[3], strList[4], strList[5]));
                        }
                        else
                        {
                            Logger.Log("不正レコード : " + line);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("不正レコード : " + line + " : " + ex.Message);
                        rtnDataList.Clear();
                    }
                }
                sr.Close();
                fileStream.Close();
            }
            catch (Exception ex)
            {
                Logger.Log("ファイルの読み込み失敗 : " + ex.Message);
                rtnDataList.Clear();
            }
            return rtnDataList;
        }
    }
}
