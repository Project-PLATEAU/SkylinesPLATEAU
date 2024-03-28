using ColossalFramework;
using ColossalFramework.Importers;
using ColossalFramework.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.XPath;
using UnityEngine;

namespace SkylinesPlateau
{
    public class GmlDemData
    {
        public class MeshData
        {
            /// <summary>
            /// TINから生成したメッシュ頂点
            /// </summary>
            public Vector3 pos;
            /// <summary>
            /// 面積が設定値以上のTINから生成されたデータか否か
            /// </summary>
            public bool isTinOverArea;

            public MeshData(float x, float y, float z, bool isTinOverArea)
            {
                this.pos = new Vector3(x, y, z);
                this.isTinOverArea = isTinOverArea;
            }
        }

        public class TriangleData
        {
            public Vector3 pos1;
            public Vector3 pos2;
            public Vector3 pos3;

            public TriangleData(List<Vector3> points)
            {
                pos1 = points[0];
                pos2 = points[1];
                pos3 = points[2];
            }
        }

        //-------------------------------------
        // 固定値
        //-------------------------------------
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] DEL_START
//        private const string INPUT_PATH = @"Files/SkylinesPlateau/in/";
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] DEL_END
        private const string INPUT_PATH2 = @"/udx/dem";

        /// <summary>
        /// メッシュサイズ(16m)
        /// </summary>
        private const int MESH_SIZE = (int)TerrainManager.RAW_CELL_SIZE;
        /// <summary>
        /// メッシュ数(1081個)
        /// ※9タイル(17280m) / メッシュサイズ(16m)
        /// </summary>
        private const int MESH_NUM = (MapExtent.MAX_AREA_SIZE / MESH_SIZE) + 1;

        /// <summary>
        /// CitiesSkylines上で表現可能な最大高さ(1024m)
        /// </summary>
        private const int MAX_HEIGHT = 1024;

        //-------------------------------------
        // メンバ変数
        //-------------------------------------
        /// <summary>
        // 数値地図メッシュの標高データ（16m間隔で、1081*1081）
        /// </summary>
        public MeshData[,] heighMesh = new MeshData[MESH_NUM, MESH_NUM];
        /// <summary>
        // 数値地図メッシュ範囲の最小値
        /// </summary>
        public Vector3 areaMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        /// <summary>
        // 数値地図メッシュ範囲の最大値
        /// </summary>
        public Vector3 areaMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        //-------------------------------------
        // ReadOnly
        //-------------------------------------
        /// <summary>
        /// エラー発生時にエラーメッセージが格納される
        /// </summary>
        private string _errMessage = "";
        public string errorMessage { get { return _errMessage; } }


        /// <summary>
        /// インポート処理
        /// </summary>
        static public int Import()
        {
            Logger.Log("地表の読み込み処理開始");

            //-------------------------------------
            // XMLファイルの解析
            //-------------------------------------
            GmlDemData demData = new GmlDemData();
            int readCnt = demData.ReadXML();
            if (demData.areaMax.x == float.MinValue && 
                demData.areaMax.y == float.MinValue && 
                demData.areaMax.z == float.MinValue)
            {
                // ファイルなし
                Logger.Log("地表データなし");
                return 0;
            }

            //-------------------------------------
            // 標高値の変換
            //-------------------------------------
            System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();
            w.Start();
            ushort[,] hmap = demData.ConvertHeight();
            w.Stop();
            Logger.Log("標高変換完了 : " + w.ElapsedMilliseconds);



#if TOOL_DEBUG_TINDATA
            FileStream fileStream2 = File.Create("C:\\work\\DemMapImg.txt");
            StreamWriter sw2 = new StreamWriter(fileStream2);
            //-------------------------------------
            // 地形の高さ設定
            //-------------------------------------
            int loop1, loop2;
            // Z方向のループ
            for (loop1 = 0; loop1 < MESH_NUM; loop1++)
            {
                // X方向のループ
                for (loop2 = 0; loop2 < MESH_NUM; loop2++)
                {
                    if (hmap[loop2, loop1] == 0) continue;
                    sw2.WriteLine((((loop2 * MESH_SIZE) - MapExtent.MAX_AREA_SIZE / 2) + MESH_SIZE / 2) + "," + (((loop1 * MESH_SIZE) - MapExtent.MAX_AREA_SIZE / 2) + MESH_SIZE / 2) + "," + hmap[loop2, loop1]);
                }
            }
            sw2.Close();
            fileStream2.Close();
#endif


            //-------------------------------------
            // 画面への反映処理
            //-------------------------------------
            demData.DrawDem(hmap);

            return readCnt;
        }

        /// <summary>
        /// 指定フォルダのXMLファイルを解析して読み込む
        /// </summary>
        private int ReadXML()
        {
            Logger.Log("地表の読み込み処理開始");
            
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
/*
            //-------------------------------------
            // フォルダの存在チェック
            //-------------------------------------
            if (!Directory.Exists(INPUT_PATH))
            {
                // ファイルなし
                _errMessage = "ＤＥＭファイルの入力フォルダがありません。\n " + INPUT_PATH;
                Logger.Log(_errMessage);
                return 0;
            }
*/
            //-------------------------------------
            // フォルダの存在チェック
            //-------------------------------------
            if (!Directory.Exists(IniFileData.Instance.inputFolderPath))
            {
                // ファイルなし
                _errMessage = "ＤＥＭファイルの入力フォルダがありません。\n " + IniFileData.Instance.inputFolderPath;
                Logger.Log(_errMessage);
                return 0;
            }
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END

#if TOOL_DEBUG_TINDATA
            FileStream fileStream = File.Create("C:\\work\\SkylinesPlateauDem.txt");
            StreamWriter sw = new StreamWriter(fileStream);
#endif

            //-------------------------------------
            // 土地の読み込み処理
            //-------------------------------------
            object lock_obj = new object();
            List<MeshData> allMeshPoints = new List<MeshData>();
            int threadNum = 0;

// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
/*
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
*/
            DirectoryInfo dir = new DirectoryInfo(IniFileData.Instance.inputFolderPath);
            if (!Directory.Exists(dir.FullName + INPUT_PATH2))
            {
                // ファイルなし
                Logger.Log("フォルダがありません：" + dir.FullName + INPUT_PATH2);
                return 0;
            }
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END

                //-------------------------------------
                // フォルダ内のXMLファイルを取得
                //-------------------------------------
                IEnumerable<string> files = Directory.GetFiles(dir.FullName + INPUT_PATH2, "*.gml");
                // ファイル数分ループ処理
                foreach (string str in files)
                {
#if TOOL_DEBUG
                    List<MeshData> meshPoints = new List<MeshData>();

                    //-------------------------------------
                    // ファイル読み込み
                    //-------------------------------------
                    try
                    {
                        List<Vector3> points = new List<Vector3>();

                        // GMLファイルのオープン
                        GmlUtil gml = new GmlUtil(str);
                        // 範囲チェック
                        if (!gml.CheckXmlArea())
                        {
                            lock (lock_obj)
                            {
                                threadNum--;
                                Logger.Log("範囲外");
                                continue;
                            }
                        }

                        FileStream fileStream3 = File.Create("C:\\work\\SkylinesPlateauDem_Poly_" + Path.GetFileName(str) + ".txt");
                        StreamWriter sw3 = new StreamWriter(fileStream3);

                        System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();
                        w.Start();

                        System.Diagnostics.Stopwatch w2 = new System.Diagnostics.Stopwatch();
                        ulong cnt1 = 0;

                        // cityObjectMemberタグを抽出
                        XPathNodeIterator nodeList = gml.GetXmlNodeList(null, "core:CityModel/core:cityObjectMember");
                        // cityObjectMemberタグでループ
                        foreach (XPathNavigator nav in nodeList)
                        {
                            w2.Reset();
                            w2.Start();
                            // TINの三角形ポリゴン数分ループ
                            XPathNodeIterator nodeList2 = gml.GetXmlNodeList(nav, "dem:ReliefFeature/dem:reliefComponent/dem:TINRelief/dem:tin/gml:TriangulatedSurface/gml:trianglePatches/gml:Triangle/gml:exterior/gml:LinearRing/gml:posList");
                            foreach (XPathNavigator nav2 in nodeList2)
                            {
                                // 点群取得 (範囲外の頂点を持つ三角形も判定対象とする）
                                points = GmlUtil.ConvertStringToListVec(nav2.Value, 1);

                                // 頂点を取得できているか確認
                                if (points.Count == 4)
                                {
                                    // 三角形から数値地図メッシュに値を設定
                                    SetTriangleHeight(points, meshPoints);

#if TOOL_DEBUG_TINDATA
                                    for (int a = 0; a < 3; a++)
                                    {
                                        sw.WriteLine(points[a].x + "," + points[a].y + "," + points[a].z);
                                    }
#endif
                                    // TINの面積チェックに引っかかっていない場合
                                    if (!(meshPoints.Count > 0 && meshPoints[0].isTinOverArea))
                                    {
                                        sw3.WriteLine(points[0].x + " " + points[0].y + " " + points[0].z + " " +
                                            points[1].x + " " + points[1].y + " " + points[1].z + " " +
                                            points[2].x + " " + points[2].y + " " + points[2].z + " " +
                                            points[3].x + " " + points[3].y + " " + points[3].z);
                                    }
                                }
                                points.Clear();

                                cnt1++;
                                if (cnt1 % 10000 == 0)
                                {
                                    w2.Stop();
                                    Logger.Log("[" + cnt1 + "] 10000件の速度計測 : " + w2.ElapsedMilliseconds);
                                    w2.Reset();
                                    w2.Start();
                                }
                            }
                        }

                        w.Stop();

                        allMeshPoints.AddRange(meshPoints);
                        threadNum--;
                        Logger.Log("読み込み完了 : " + w.ElapsedMilliseconds);

                        sw3.Close();
                        fileStream3.Close();
                    }
                    catch (Exception ex)
                    {
                        _errMessage = "ＤＥＭファイルの解析に失敗しました。\n " + ex.Message;
                        Logger.Log(_errMessage);
                    }

#else
                    while (threadNum > 5)
                    {
                        Thread.Sleep(100);
                    }

                    Logger.Log("[" + threadNum + "] スレッド起動");
                    lock (lock_obj)
                    {
                        threadNum++;
                    }
                    ThreadHelper.taskDistributor.Dispatch(delegate
                    {
                        List<MeshData> meshPoints = new List<MeshData>();

                        //-------------------------------------
                        // ファイル読み込み
                        //-------------------------------------
                        try
                        {
                            List<Vector3> points = new List<Vector3>();

                            // GMLファイルのオープン
                            GmlUtil gml = new GmlUtil(str);
                            // 範囲チェック
                            if (!gml.CheckXmlArea())
                            {
                                lock (lock_obj)
                                {
                                    threadNum--;
                                    Logger.Log("範囲外");
                                    return;
                                }
                            }

                            System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();
                            w.Start();

                            System.Diagnostics.Stopwatch w2 = new System.Diagnostics.Stopwatch();
                            ulong cnt1 = 0;

                            // cityObjectMemberタグを抽出
                            XPathNodeIterator nodeList = gml.GetXmlNodeList(null, "core:CityModel/core:cityObjectMember");
                            // cityObjectMemberタグでループ
                            foreach (XPathNavigator nav in nodeList)
                            {
                                w2.Reset();
                                w2.Start();
                                // TINの三角形ポリゴン数分ループ
                                XPathNodeIterator nodeList2 = gml.GetXmlNodeList(nav, "dem:ReliefFeature/dem:reliefComponent/dem:TINRelief/dem:tin/gml:TriangulatedSurface/gml:trianglePatches/gml:Triangle/gml:exterior/gml:LinearRing/gml:posList");
                                if (nodeList2.Count == 0)
                                {
                                    // ※<gml:LinearRing>ではなく、<gml:Ring>で指定されている場合
                                    nodeList2 = gml.GetXmlNodeList(nav, "dem:ReliefFeature/dem:reliefComponent/dem:TINRelief/dem:tin/gml:TriangulatedSurface/gml:trianglePatches/gml:Triangle/gml:exterior/gml:Ring/gml:curveMember/gml:LineString/gml:posList");
                                }
                                foreach (XPathNavigator nav2 in nodeList2)
                                {
                                    // 点群取得 (範囲外の頂点を持つ三角形も判定対象とする）
                                    points = GmlUtil.ConvertStringToListVec(nav2.Value, 1);

                                    // 頂点を取得できているか確認
                                    if (points.Count == 4)
                                    {
                                        // 三角形から数値地図メッシュに値を設定
                                        SetTriangleHeight(points, meshPoints);
                                    }
                                    points.Clear();

                                    cnt1++;
                                    if (cnt1 % 10000 == 0)
                                    {
                                        w2.Stop();
                                        Logger.Log("[" + cnt1 + "] 10000件の速度計測 : " + w2.ElapsedMilliseconds);
                                        w2.Reset();
                                        w2.Start();
                                    }
                                }
                            }

                            w.Stop();
                            lock (lock_obj)
                            {
                                allMeshPoints.AddRange(meshPoints);
                                threadNum--;
                                Logger.Log("読み込み完了 : " + w.ElapsedMilliseconds);
                            }
                        }
                        catch (Exception ex)
                        {
                            lock (lock_obj)
                            {
                                _errMessage = "ＤＥＭファイルの解析に失敗しました。\n " + ex.Message;
                                Logger.Log(_errMessage);
                                return;
                            }
                        }
                    });
#endif
                }
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] DEL_START
//            }
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] DEL_END

#if TOOL_DEBUG_TINDATA
            sw.Close();
            fileStream.Close();
#endif
#if TOOL_DEBUG
#else
            // 処理が終わるまで待つ
            while (threadNum != 0)
            {
                Thread.Sleep(100);
            }
#endif

#if TOOL_DEBUG_TINDATA
            FileStream fileStream2 = File.Create("C:\\work\\dem_Merge.txt");
            StreamWriter sw2 = new StreamWriter(fileStream2);
#endif

            //-------------------------------------
            // ＤＥＭをマージする
            //-------------------------------------
#if DEM_TEST_MODE_2
// 【動作検証用コード】標高差を2048m程度とし、ゲーム画面上0.5mをDEM1mとするコード
            bool chkF = false;
#endif
            int xidx, yidx;
            foreach (MeshData mdata in allMeshPoints)
            {
#if DEM_TEST_MODE_1
// 【動作検証用コード】標高範囲を2000m～3024m差とし、DEM最小値を基準とするコード
                // テスト用にZ値を調整 (標高値が0-1024範囲外だが、全体の差分は範囲内パターン)
                mdata.pos.z += 2000;
#endif
#if DEM_TEST_MODE_2
// 【動作検証用コード】標高差を2048m程度とし、ゲーム画面上0.5mをDEM1mとするコード
                // テスト用にZ値を調整 (標高値が0-1024範囲外で、差分も範囲外パターン)
                if (!chkF)
                {
                    chkF = true;
                    mdata.pos.z = 2048; // 最初の１点のみ標高調整、1m=0.5mとなるよう最大高さを調整
                }
#endif
                //--------------------------------------
                // メッシュに設定
                //--------------------------------------
                // 該当する番号のセルを算出（CitiesSkylinesの座標系であるため、tmpPにMaxを加算することにより、左上原点にしている）
                xidx = (int)mdata.pos.x;
                yidx = (int)mdata.pos.y;
                // 該当メッシュに標高設定済みで、今回の方が大きい場合はスキップ
                if (this.heighMesh[xidx, yidx] != null && this.heighMesh[xidx, yidx].pos.z < mdata.pos.z) continue;
                // 標高値の設定
                this.heighMesh[xidx, yidx] = mdata;
                // 標高値の最大最小を設定
                if (this.areaMax.z < mdata.pos.z) this.areaMax.z = (float)mdata.pos.z;
                if (this.areaMin.z > mdata.pos.z) this.areaMin.z = (float)mdata.pos.z;

#if TOOL_DEBUG_TINDATA
                sw2.WriteLine((((xidx * MESH_SIZE) - MapExtent.MAX_AREA_SIZE / 2) + MESH_SIZE / 2) + "," + (((yidx * MESH_SIZE) - MapExtent.MAX_AREA_SIZE / 2) + MESH_SIZE / 2) + "," + mdata.pos.z);
#endif
            }

            // DEM読込データが存在している場合
            if (allMeshPoints.Count > 0)
            {
                //-------------------------------------
                // 河川の深さを反映
                //-------------------------------------
                SetWaterWayHeight();
            }

#if TOOL_DEBUG_TINDATA
            sw2.Close();
            fileStream2.Close();
#endif
            return allMeshPoints.Count;
        }

        private void DrawDem(ushort[,] hmap)
        {
            TerrainManager tm = TerrainManager.instance;

            //-------------------------------------
            // 地形の高さ設定
            //-------------------------------------
            int loop1, loop2, idx;
            // Z方向のループ
            for (loop1 = 0; loop1 < MESH_NUM; loop1++)
            {
                // X方向のループ
                for (loop2 = 0; loop2 < MESH_NUM; loop2++)
                {
                    idx = loop1 * MESH_NUM + loop2;
                    tm.RawHeights[idx] = hmap[loop2, loop1];
                }
            }

            //-------------------------------------
            // 画面再描画
            //-------------------------------------
            int num4 = 120;
            for (int k = 0; k < 9; k++)
            {
                for (int l = 0; l < 9; l++)
                {
                    TerrainModify.UpdateArea(l * num4, k * num4, (l + 1) * num4, (k + 1) * num4, heights: true, surface: true, zones: true);
                }
            }
        }

        private ushort[,] ConvertHeight()
        {
            //-------------------------------------
            // 高さ範囲からベースとなる値を判断
            //-------------------------------------
            // 戻り値
            ushort[,] rtnHeightMap = new ushort[MESH_NUM, MESH_NUM];
            // 基準となるZ座標
            int baseZ = 0;
            // 比率（高さは1024mまでしか表現できないが、超える場合には「1pix辺り1m」から変更が必要となる）
            double res = 1.0;
            // 高さの差分
            double sabun = (areaMax.z - areaMin.z);
            // 水面高さ
            int seaLevel = (int)IniFileData.Instance.demSeaLevel;

            Logger.Log("[ConvertHeight] 高さの範囲： " + areaMin.z + " " + areaMax.z);
            Logger.Log("[ConvertHeight] 高さの差分： " + sabun);

            // 差分調整
            // ※小数点の切捨て、切り上げがあるので、2mバッファを持たせる
            sabun += 2;
            // ※河川を掘れるようにバッファを持たせる
            sabun += seaLevel;

            Logger.Log("[ConvertHeight] 差分調整後（切捨て切上の2m、河川バッファ）： " + sabun);

            // 表現可能な範囲内の場合
            if (sabun < MAX_HEIGHT)
            {
                // 最小値が０以上、最大値が範囲以内
                if (areaMin.z >= 0 && (areaMax.z + seaLevel) < MAX_HEIGHT)
                {
                    // 河川用の高さを加算した位置を基準位置とする（baseZを減算して標高値とするので、河川高さ分、加算された値が基準位置となる）
                    baseZ = seaLevel * -1;
                    Logger.Log("[ConvertHeight] 範囲内。0m基準とする");
                }
                else
                {
                    // 最小値を基準とする
                    baseZ = (int)Math.Floor(areaMin.z) - seaLevel;
                    Logger.Log("[ConvertHeight] 範囲内。最小値を基準とする");
                }
            }
            else
            {
                // 最小値を基準とする
                baseZ = (int)Math.Floor(areaMin.z) - seaLevel;
                // 比率を算出
                res = (double)MAX_HEIGHT / sabun;
            }

            //-------------------------------------
            // 地形の高さを、16byteの数値に変換
            //-------------------------------------
            int loop1, loop2;
            ushort tmpZ;
            // 高さをushort範囲内で表現するための比率
            float oneMetreBytes = ushort.MaxValue / (ushort)TerrainManager.TERRAIN_HEIGHT;

            Logger.Log("[ConvertHeight] ushort変換の比率 ： " + oneMetreBytes);

            // Z方向のループ
            for (loop1 = 0; loop1 < MESH_NUM; loop1++)
            {
                // X方向のループ
                for (loop2 = 0; loop2 < MESH_NUM; loop2++)
                {
                    // 標高設定されていない
                    if (heighMesh[loop2, loop1] == null)
                    {
                        // 標高を０にする
                        rtnHeightMap[loop2, loop1] = 0;
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[ConvertHeight] 標高変換 ： " + loop2 + ", " + loop1 + ", " + null + " -> " + rtnHeightMap[loop2, loop1]);
                        }
                    }
                    // 生成元のTINの面積が広すぎる場合
                    else if (heighMesh[loop2, loop1].isTinOverArea)
                    {
                        // 標高を０にする
                        rtnHeightMap[loop2, loop1] = 0;
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[ConvertHeight] 標高変換 ： " + loop2 + ", " + loop1 + ", " + null + " -> " + rtnHeightMap[loop2, loop1]);
                        }
                    }
                    else
                    {
                        // 標高値をushort型に変換
                        int tmp = (int)Math.Floor((heighMesh[loop2, loop1].pos.z - baseZ) * res * oneMetreBytes);
                        if (tmp < 0) tmp = 0;
                        tmpZ = (ushort)tmp;
                        rtnHeightMap[loop2, loop1] = tmpZ;
                        if (IniFileData.Instance.logOut)
                        {
                            Logger.Log("[ConvertHeight] 標高変換 ： " + loop2 + ", " + loop1 + ", " + null + " -> " + rtnHeightMap[loop2, loop1]);
                        }
                    }
                }
            }

            return rtnHeightMap;
        }


        /// <summary>
        /// DEMに河川の深さを反映する
        /// </summary>
        private void SetWaterWayHeight()
        {
            // 何度も標高減算処理を行わないよう、セルに対するフラグ管理を行う
            bool[] meshChkFlags = new bool[(TerrainManager.RAW_RESOLUTION + 1) * (TerrainManager.RAW_RESOLUTION + 1)];
            for (int i = 0; i < meshChkFlags.Length; i++) meshChkFlags[i] = false;

            //-------------------------------------
            // 河川を読み込む
            //-------------------------------------
            List<GmlWaterwayData> waterWayList = GmlWaterwayData.ReadXML();

            // 河川の数でループ処理
            Vector3 baseVec = new Vector3(MapExtent.MAX_AREA_SIZE / 2, MapExtent.MAX_AREA_SIZE / 2, 0);
            foreach (var segment in waterWayList)
            {
                //--------------------------------
                // ローカル変数定義
                //--------------------------------
                int iXstep, iYstep, iXstart, iYstart, iXend, iYend;
                int xidx, yidx, iXnum, iYnum;
                int iCellSize;
                bool bRet, bRet2;
                int idx;

                // グリッドサイズ
                iCellSize = (int)TerrainManager.RAW_CELL_SIZE;

                // Game画面中央ではなく、左上原点に変更
                Vector3 areaMin = segment.areaMin + baseVec;
                Vector3 areaMax = segment.areaMax + baseVec;

                // XY方向の始終点 (グリッドサイズで丸める)
                iXstart = (int)Math.Floor(areaMin.x / iCellSize) * iCellSize;
                iYstart = (int)Math.Floor(areaMin.y / iCellSize) * iCellSize;
                iXend = (int)Math.Ceiling(areaMax.x / iCellSize) * iCellSize;
                iYend = (int)Math.Ceiling(areaMax.y / iCellSize) * iCellSize;
                // グリッド数
                iXnum = (iXend - iXstart) / iCellSize;
                iYnum = (iYend - iYstart) / iCellSize;

                // 始終点をGame画面中央の座標系に戻す
                iXstart -= (int)baseVec.x;
                iYstart -= (int)baseVec.y;
                iXend -= (int)baseVec.x;
                iYend -= (int)baseVec.y;

                //----------------------------------------
                // ポリゴン範囲内の矩形数分ループ処理
                //----------------------------------------
                List<Vector3> tmpPoints = new List<Vector3>();
                for (iYstep = iYstart; iYstep <= iYend; iYstep += iCellSize)
                {
                    for (iXstep = iXstart; iXstep <= iXend; iXstep += iCellSize)
                    {
                        // 矩形範囲生成
                        tmpPoints.Clear();
                        tmpPoints.Add(new Vector3(iXstep, iYstep, 0));
                        tmpPoints.Add(new Vector3(iXstep + iCellSize, iYstep, 0));
                        tmpPoints.Add(new Vector3(iXstep + iCellSize, iYstep + iCellSize, 0));
                        tmpPoints.Add(new Vector3(iXstep, iYstep + iCellSize, 0));

                        // 内外判定
                        bRet = CommonFunc.checkPolyInPoly(tmpPoints, segment.points);
                        if (bRet)
                        {
                            bRet2 = false;
                            // ポリゴン内側の場合、穴あきの内部になっていないか判定する
                            foreach (InteriorPoints pos in segment.interiorList)
                            {
                                bRet2 = CommonFunc.checkPolyInPoly(tmpPoints, pos.points);
                                if (bRet2)
                                {
                                    if (IniFileData.Instance.logOut)
                                    {
                                        Logger.Log("穴の内側");
                                    }
                                    break;
                                }
                            }
                            if (bRet2)
                            {
                                // 穴の内側なのでスルーする
                                continue;
                            }

                            //--------------------------------------
                            // Index番号の取得
                            //--------------------------------------
                            // 該当する番号のセルを算出（CitiesSkylinesの座標系であるため、tmpPにMaxを加算することにより、左上原点にしている）
                            xidx = (int)Math.Floor((double)((iXstep + (iCellSize / 2)) + (MapExtent.MAX_AREA_SIZE / 2)) / iCellSize);
                            yidx = (int)Math.Floor((double)((iYstep + (iCellSize / 2)) + (MapExtent.MAX_AREA_SIZE / 2)) / iCellSize);
                            // 内外判定
                            if (xidx > TerrainManager.RAW_RESOLUTION || yidx > TerrainManager.RAW_RESOLUTION ||
                                xidx < 0 || yidx < 0)
                            {
                                if (IniFileData.Instance.logOut)
                                {
                                    Logger.Log("画面範囲外なのでスルー: [" + xidx + "," + yidx + "]");
                                }
                                // 画面範囲外
                                continue;
                            }

                            // 河川高さ導入済みの場合
                            idx = yidx * (TerrainManager.RAW_RESOLUTION + 1) + xidx;
                            if (meshChkFlags[idx])
                            {
                                if (IniFileData.Instance.logOut)
                                {
                                    Logger.Log("[Skip] 二重導入を防ぐ: [" + xidx + "," + yidx + "]");
                                }
                                continue;
                            }
                            // 二重導入され内容フラグを立てる
                            meshChkFlags[idx] = true;

                            //--------------------------------------
                            // 河川の高さを反映
                            //--------------------------------------
                            if (heighMesh[xidx, yidx] != null)
                            {
                                if (IniFileData.Instance.logOut)
                                {
                                    Logger.Log("河川の深さ反映: [" + xidx + "," + yidx + "]");
                                }
                                MeshData mdata = heighMesh[xidx, yidx];
                                mdata.pos.z -= (float)IniFileData.Instance.demWaterAreaDownHeight;
                                mdata.isTinOverArea = false;
                                heighMesh[xidx, yidx] = mdata;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 引数の三角形を用いて、数値地図用のメッシュに標高値を付与する
        /// </summary>
        /// <param name="points">三角形ポリゴンの頂点（４点）</param>
        private void SetTriangleHeight(List<Vector3> points, List<MeshData> meshPoints)
        {
            //--------------------------------
            // ローカル変数定義
            //--------------------------------
            double xstart, xend, ystart, yend, z;
            Vector3 tmpP;
            int iMinX, iMinY, iMaxX, iMaxY, iModX, iModY;
            int iXstep, iYstep, iXstart, iYstart, iXend, iYend, iIntX, iIntY;
            int xidx, yidx;
            bool isTinOverArea = false;

            //----------------------------------------
            // メッシュの最大最小範囲、間隔
            //----------------------------------------
            iMinX = iMinY = MapExtent.MAX_AREA_SIZE / -2;
            iMaxX = iMaxY = MapExtent.MAX_AREA_SIZE / 2;
            iIntX = MESH_SIZE; // グリッドサイズ
            iIntY = MESH_SIZE; // グリッドサイズ

            //----------------------------------------
            // 三角形の最大最小範囲
            //----------------------------------------
            ystart = yend = points[0].y;
            if (points[1].y > yend) yend = points[1].y;
            if (points[1].y < ystart) ystart = points[1].y;
            if (points[2].y > yend) yend = points[2].y;
            if (points[2].y < ystart) ystart = points[2].y;
            xstart = xend = points[0].x;
            if (points[1].x > xend) xend = points[1].x;
            if (points[1].x < xstart) xstart = points[1].x;
            if (points[2].x > xend) xend = points[2].x;
            if (points[2].x < xstart) xstart = points[2].x;

            //----------------------------------------
            // 面積判定
            //----------------------------------------
            double menseki = ((xend - xstart) * (yend - ystart)) / 2;
            if (menseki > IniFileData.Instance.demFilterAreaSize)
            {
                // 面積が設定値より大きい
                isTinOverArea = true;
            }

            //----------------------------------------
            // 三角形の範囲をメッシュ基準に丸める（Y方向）
            //----------------------------------------
            iYstart = (int)Math.Floor(ystart);  // 三角形の最小位置をM単位とし、cmは切捨て
            iYend = (int)Math.Ceiling(yend);    // 三角形の最大位置をM単位とし、cmは切り上げ
                                                // メッシュ範囲外の場合
            if (iYend < iMinY || iYstart > iMaxY) return;
            // グリッドサイズの間隔となるよう丸める（始点）
            iModY = (iYstart - iMinY) % iIntY;
            iYstart -= iModY;
            // グリッドサイズの間隔となるよう丸める（終点）
            iModY = (iYend - iMinY) % iIntY;
            if (iModY > 0) iYend -= iModY;
            // 終点をグリッド１つ分余裕を持たせる
            iYend += iIntY;

            //----------------------------------------
            // 三角形の範囲をメッシュ基準に丸める（X方向）
            //----------------------------------------
            iXstart = (int)Math.Floor(xstart);
            iXend = (int)Math.Ceiling(xend);
            // メッシュ範囲外の場合
            if (iXend < iMinX || iXstart > iMaxX) return;
            // グリッドサイズの間隔となるよう丸める（始点）
            iModX = (iXstart - iMinX) % iIntX;
            iXstart -= iModX;
            // グリッドサイズの間隔となるよう丸める（終点）
            iModX = (iXend - iMinX) % iIntX;
            if (iModX > 0) iXend = iXend - iModX;
            // 終点をグリッド１つ分余裕を持たせる
            iXend += iIntX;

            //----------------------------------------
            // グリッドの基準点が三角形の範囲内となる物の、高さを算出する
            //----------------------------------------
            int f1, f2, f3;
            // 三角形のＹ方向のグリッド数分、ループ処理
            for (iYstep = iYstart; iYstep <= iYend; iYstep += iIntY)
            {
                // 範囲外のグリッドはスキップ
                if (iYstep > iMaxY || iYstep < iMinY) continue;

                // 三角形のＸ方向のグリッド数分、ループ処理
                for (iXstep = iXstart; iXstep <= iXend; iXstep += iIntX)
                {
                    // 範囲外のグリッドはスキップ
                    if (iXstep > iMaxX || iXstep < iMinX) continue;

                    // 対象メッシュの中心点
                    tmpP.x = iXstep + (iIntX / 2);
                    tmpP.y = iYstep + (iIntY / 2);
                    tmpP.z = 0;
                    // グリッドの基準点が三角形の範囲内に含まれているか判定 (左右の判定値を逆転させる)
                    f1 = CommonFunc.Check_CrossPos(tmpP, points[0], points[1]) * -1;
                    f2 = CommonFunc.Check_CrossPos(tmpP, points[1], points[2]) * -1;
                    f3 = CommonFunc.Check_CrossPos(tmpP, points[2], points[0]) * -1;
                    // 範囲外の場合はスキップ
                    if ((f1 * f2) < 0 || (f2 * f3) < 0 || (f3 * f1) < 0 || (f1 == 0 && f2 == 0 && f3 == 0)) continue;

                    // 各頂点から基準点の高さを算出する
                    z = CalcZ(tmpP, points[0], points[1], points[2]);

                    //--------------------------------------
                    // メッシュに設定
                    //--------------------------------------
                    // 該当する番号のセルを算出（CitiesSkylinesの座標系であるため、tmpPにMaxを加算することにより、左上原点にしている）
                    xidx = (int)Math.Floor((tmpP.x + iMaxX) / iIntX);
                    yidx = (int)Math.Floor((tmpP.y + iMaxY) / iIntY);

                    meshPoints.Add(new MeshData(xidx, yidx, (float)z, isTinOverArea));
/*
                    // 該当メッシュに標高設定済みで、今回の方が大きい場合はスキップ
                    if (this.heighMesh[xidx, yidx] != null && this.heighMesh[xidx, yidx].z > z) continue;
                    // 標高値の設定
                    this.heighMesh[xidx, yidx] = new MeshData(z);
                    // 標高値の最大最小を設定
                    if (this.areaMax.z < z) this.areaMax.z = (float)z;
                    if (this.areaMin.z > z) this.areaMin.z = (float)z;
                    Logger.Log("[SetTriangleHeight] 標高算出 ： " + xidx + ", " + yidx + ", " + z);
                    Logger.Log("[SetTriangleHeight] 標高算出１ ： " + xidx + ", " + yidx + ", " + z);
*/
                }
            }
        }

        /// <summary>
        /// TIN内のx,yの高さを求める
        /// </summary>
        /// <param name="meshVec">TIN内のＸＹ座標</param>
        /// <param name="tri1">三角形の１点目</param>
        /// <param name="tri2">三角形の２点目</param>
        /// <param name="tri3">三角形の３点目</param>
        /// <returns>算出した高さ</returns>
        private double CalcZ(Vector3 vec, Vector3 tri1, Vector3 tri2, Vector3 tri3)
        {
            Vector3 norm;
            double d;
            double ret;

            norm = CalcNormal(tri1, tri2, tri3);
            d = -(norm.x * tri1.x + norm.y * tri1.y + norm.z * tri1.z);
            ret = (norm.x * vec.x + norm.y * vec.y + d) / norm.z * -1;

            return ret;
        }

        /// <summary>
        /// 法線ベクトルを計算する
        /// </summary>
        /// <param name="tri1">三角形の１点目</param>
        /// <param name="tri2">三角形の２点目</param>
        /// <param name="tri3">三角形の３点目</param>
        /// <returns>生成した法線ベクトル</returns>
        private Vector3 CalcNormal(Vector3 tri1, Vector3 tri2, Vector3 tri3)
        {
            Vector3 vec1, vec2;
            Vector3 rtnVec;
            float len;

            vec1.x = tri1.x - tri2.x;
            vec1.y = tri1.y - tri2.y;
            vec1.z = tri1.z - tri2.z;
            vec2.x = tri2.x - tri3.x;
            vec2.y = tri2.y - tri3.y;
            vec2.z = tri2.z - tri3.z;

            rtnVec.x = vec1.y * vec2.z - vec1.z * vec2.y;
            rtnVec.y = vec1.z * vec2.x - vec1.x * vec2.z;
            rtnVec.z = vec1.x * vec2.y - vec1.y * vec2.x;

            len = (float)Math.Sqrt(rtnVec.x * rtnVec.x + rtnVec.y * rtnVec.y + rtnVec.z * rtnVec.z);
            if (len == 0) len = 1.0f;

            rtnVec.x /= len;
            rtnVec.y /= len;
            rtnVec.z /= len;

            return rtnVec;
        }

    }
}
