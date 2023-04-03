using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using UnityEngine;

namespace SkylinesPlateau
{
    class GmlUtil
    {
        //-------------------------------------
        // メンバ変数
        //-------------------------------------
        private string _fileName;
        XPathNavigator _xml = null;
        private XmlNamespaceManager _xmlnsManager = null;

        //-------------------------------------
        // メソッド
        //-------------------------------------
        public GmlUtil(string fileName)
        {
            _fileName = fileName;
            OpenXML(fileName);
        }
        /// <summary>
        /// XMLファイルのオープン
        /// </summary>
        public bool OpenXML(string fileName)
        {
            //-------------------------------------
            // ファイル読み込み
            //-------------------------------------
            Logger.Log("ファイルの読み込み開始：" + fileName);
            try
            {
                XPathDocument xPathDoc = new XPathDocument(fileName);
                _xml = xPathDoc.CreateNavigator();
                _xmlnsManager = new XmlNamespaceManager(_xml.NameTable);
                _xmlnsManager.AddNamespace("grp", "http://www.opengis.net/citygml/cityobjectgroup/2.0");
                _xmlnsManager.AddNamespace("core", "http://www.opengis.net/citygml/2.0");
                _xmlnsManager.AddNamespace("bldg", "http://www.opengis.net/citygml/building/2.0");
                _xmlnsManager.AddNamespace("smil20", "http://www.w3.org/2001/SMIL20/");
                _xmlnsManager.AddNamespace("pbase", "http://www.opengis.net/citygml/profiles/base/2.0");
                _xmlnsManager.AddNamespace("smil20lang", "http://www.w3.org/2001/SMIL20/Language");
                _xmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                _xmlnsManager.AddNamespace("xAL", "urn:oasis:names:tc:ciq:xsdschema:xAL:2.0");
                _xmlnsManager.AddNamespace("luse", "http://www.opengis.net/citygml/landuse/2.0");
                _xmlnsManager.AddNamespace("app", "http://www.opengis.net/citygml/appearance/2.0");
                _xmlnsManager.AddNamespace("gen", "http://www.opengis.net/citygml/generics/2.0");
                _xmlnsManager.AddNamespace("dem", "http://www.opengis.net/citygml/relief/2.0");
                _xmlnsManager.AddNamespace("tex", "http://www.opengis.net/citygml/texturedsurface/2.0");
                _xmlnsManager.AddNamespace("xlink", "http://www.w3.org/1999/xlink");
                _xmlnsManager.AddNamespace("tun", "http://www.opengis.net/citygml/tunnel/2.0");
                _xmlnsManager.AddNamespace("sch", "http://www.ascc.net/xml/schematron");
                _xmlnsManager.AddNamespace("veg", "http://www.opengis.net/citygml/vegetation/2.0");
                _xmlnsManager.AddNamespace("frn", "http://www.opengis.net/citygml/cityfurniture/2.0");
                _xmlnsManager.AddNamespace("gml", "http://www.opengis.net/gml");
                _xmlnsManager.AddNamespace("tran", "http://www.opengis.net/citygml/transportation/2.0");
                _xmlnsManager.AddNamespace("wtr", "http://www.opengis.net/citygml/waterbody/2.0");
                _xmlnsManager.AddNamespace("brid", "http://www.opengis.net/citygml/bridge/2.0");
                _xmlnsManager.AddNamespace("uro", "https://www.geospatial.jp/iur/uro/2.0");
                _xmlnsManager.AddNamespace("urf", "https://www.geospatial.jp/iur/urf/2.0");
            }
            catch (Exception ex)
            {
                Logger.Log("xmlファイルの解析に失敗しました。：" + ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 範囲チェック
        /// </summary>
        public bool CheckXmlArea()
        {
            if (_xml == null)
            {
                return false;
            }

            bool bRet;
            Vector3 areaMax = new Vector3(0, 0, 0);
            Vector3 areaMin = new Vector3(0, 0, 0);

            // 入力ファイルの範囲（最大値）
            bRet = GetTagData(null, "core:CityModel/gml:boundedBy/gml:Envelope/gml:upperCorner", out areaMax);
            if (!bRet)
            {
                // タグが無いので範囲チェックしない
                return true;
            }

            // 入力ファイルの範囲（最小値）
            bRet = GetTagData(null, "core:CityModel/gml:boundedBy/gml:Envelope/gml:lowerCorner", out areaMin);
            if (!bRet)
            {
                // タグが無いので範囲チェックしない
                return true;
            }

            // 読み込み対象の範囲かチェック
            int iret = CommonFunc.checkAreaInArea( MapExtent.Instance.readAreaMax, MapExtent.Instance.readAreaMin, areaMax, areaMin);
            if (iret == 0)
            {
                Logger.Log("読み込み範囲外");
                Logger.Log("読み込み範囲 : (" + MapExtent.Instance.readAreaMin.x + "," + MapExtent.Instance.readAreaMin.y + "," + MapExtent.Instance.readAreaMin.y + ") - (" + MapExtent.Instance.readAreaMax.x + "," + MapExtent.Instance.readAreaMax.y + "," + MapExtent.Instance.readAreaMax.y + ")");
                Logger.Log("ＧＭＬ範囲　 : (" + areaMin.x + "," + areaMin.y + "," + areaMin.y + ") - (" + areaMax.x + "," + areaMax.y + "," + areaMax.y + ")");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 指定フォルダのXMLファイルを解析して読み込む
        /// </summary>
        public XPathNodeIterator GetXmlNodeList(XPathNavigator node, string xpath)
        {
            try
            {
                object obj;

                XPathExpression query;
                if (node == null)
                {
                    query = _xml.Compile(xpath);
                    query.SetContext(_xmlnsManager);
                    obj = _xml.Select(query);
                }
                else
                {
                    query = node.Compile(xpath);
                    query.SetContext(_xmlnsManager);
                    obj = node.Select(query);
                }

                if (obj is XPathNodeIterator)
                {
                    return (XPathNodeIterator)obj;
                }
            }
            catch (System.Xml.XmlException Ex)
            {
                Logger.Log("TAGの解析に失敗 : " + xpath);
                Logger.Log(Ex.Message);
                return null;
            }
            return null;
        }

        /// <summary>
        /// タグ読み込み（String）
        /// </summary>
        public bool GetTagData(XPathNavigator node, string xpath, out string rtnData)
        {
            //-----------------------------
            // XMLのパース
            //-----------------------------
            object obj;
            rtnData = "";

            try
            {
                XPathExpression query;
                if (node == null)
                {
                    query = _xml.Compile(xpath);
                    query.SetContext(_xmlnsManager);
                    obj = _xml.Select(query);
                }
                else
                {
                    query = node.Compile(xpath);
                    query.SetContext(_xmlnsManager);
                    obj = node.Select(query);
                }
                if (obj == null)
                {
                    Logger.Log("TAGが存在しない : " + xpath);
                    return false;
                }

                // タグデータ取得
                if (obj is XPathNodeIterator)
                {
                    XPathNodeIterator ite = (XPathNodeIterator)obj;
                    foreach (XPathNavigator nav in ite)
                    {
                        rtnData = nav.Value;
                        return true;
                    }
                }
                else
                {
                    rtnData = ((XPathNavigator)obj).Value;
                    return true;
                }
            }
            catch (System.Xml.XmlException Ex)
            {
                Logger.Log("TAGの解析に失敗 : " + xpath);
                Logger.Log(Ex.Message);
                return false;
            }

            return false;
        }

        /// <summary>
        /// タグ読み込み（Int）
        /// </summary>
        public bool GetTagData(XPathNavigator node, string xpath, out int rtnData)
        {
            rtnData = -1;

            // タグ読み込み
            string TagStr = "";
            bool bRet = GetTagData(node, xpath, out TagStr);
            if (!bRet)
            {
                return false;
            }
            // データ型変換
            int.TryParse(TagStr, out rtnData);

            return true;
        }

        /// <summary>
        /// タグ読み込み（Double）
        /// </summary>
        public bool GetTagData(XPathNavigator node, string xpath, out double rtnData)
        {
            rtnData = -1;

            // タグ読み込み
            string TagStr = "";
            bool bRet = GetTagData(node, xpath, out TagStr);
            if (!bRet)
            {
                return false;
            }
            // データ型変換
            double.TryParse(TagStr, out rtnData);

            return true;
        }

        /// <summary>
        /// タグ読み込み（Vector3）
        /// </summary>
        public bool GetTagData(XPathNavigator node, string xpath, out Vector3 rtnData)
        {
            rtnData.x = 0;
            rtnData.y = 0;
            rtnData.z = 0;

            // タグ読み込み
            string TagStr = "";
            bool bRet = GetTagData(node, xpath, out TagStr);
            if (!bRet)
            {
                return false;
            }

            // Vector3に変換
            string[] splitStr = TagStr.Split(' ');
            if (splitStr.Length == 3)
            {
                double lat, lon, x, y, z;
                double.TryParse(splitStr[0], out lat);
                double.TryParse(splitStr[1], out lon);
                double.TryParse(splitStr[2], out z);
                // Webメルカトルに変換
//                CommonBL.latlng2merc(lat, lon, out x, out y);
                // 平面直角座標に変換
                CommonBL.Instance.bl2xy(lat, lon, out x, out y);
                // 保持
                rtnData.x = (float)x;
                rtnData.y = (float)y;
                rtnData.z = (float)z;
            }
            else
            {
                Logger.Log("TAGの形式不正 : " + xpath + " (" + TagStr + ")");
                return false;
            }

            return true;
        }

        /// <summary>
        /// タグ読み込み（指定パスのXMLをDicに展開。Dicに指定Keyが含まれていればrtnDataを取得）
        /// </summary>
        public bool GetTagDataGML(string path, string key, Dictionary<string, Dictionary<string, string>> gmlDic, out string rtnData)
        {
            rtnData = "";
            if (path == "") return false;
            if (key == "") return false;

            Dictionary<string, string> rtnDic = new Dictionary<string, string>();
            string xmlPath, data1, data2;

            //-----------------------------------------------
            // 指定されたPathのXMLファイルを読み込む
            //-----------------------------------------------
            xmlPath = path;
            // 相対パスの場合
            if (!Path.IsPathRooted(path))
            {
                // Gmlファイルのパスを取得
//                Uri gmlPath = new Uri(Path.GetDirectoryName(_fileName));
                Uri gmlPath = new Uri(_fileName);
                // 相対パスからフルパスに変換する
                xmlPath = new Uri(gmlPath, path).LocalPath;
            }

            //-----------------------------------------------
            // XMLを読み込んでいない場合
            //-----------------------------------------------
            if (!gmlDic.ContainsKey(xmlPath))
            {
                // ファイルが無ければ終了
                if (!File.Exists(xmlPath))
                {
                    gmlDic[xmlPath] = rtnDic;
                    return false;
                }

                // XMLファイルのオープン
                GmlUtil gml2 = new GmlUtil(xmlPath);
                // dictionaryEntryタグを抽出
                XPathNodeIterator nodeList3 = gml2.GetXmlNodeList(null, "gml:Dictionary/gml:dictionaryEntry");
                // dictionaryEntryrタグでループ
                foreach (XPathNavigator nav3 in nodeList3)
                {
                    gml2.GetTagData(nav3, "gml:Definition/gml:name", out data1);
                    gml2.GetTagData(nav3, "gml:Definition/gml:description", out data2);
                    // 戻り値の配列に追加
                    rtnDic[data1] = data2;
                }
                if (nodeList3.Count == 0)
                {
                    return false;
                }
                // Dictionaryで保持
                gmlDic[xmlPath] = rtnDic;
            }

            //-----------------------------------------------
            // データがある場合に取得する
            //-----------------------------------------------
            gmlDic[xmlPath].TryGetValue(key, out rtnData);

            return true;
        }


        /// <summary>
        /// <gml:posList>に格納された頂点を、Vector3型の配列に展開する。
        /// </summary>
        /// <param name="str">「gml:posList」タグのValue値（半角スペース区切りの展開対象文字列）</param>
        /// <param name="areaCheckMode">範囲外頂点の扱い方（0:範囲外の頂点があれば除外、1:全頂点が範囲外であれば除外、2:除外しない）</param>
        /// <returns></returns>
        static public List<Vector3> ConvertStringToListVec(string str, int areaCheckMode=0)
        {
            List<Vector3> rtnData = new List<Vector3>();

            // List<Vector3>に変換
            string[] splitStr = str.Split(' ');

            int cnt = 0;
            for (int loop_cnt = 0; loop_cnt < splitStr.Length; loop_cnt += 3)
            {
                // X,Y,Zの3種が含まれている場合のみ取得
                if (loop_cnt + 2 >= splitStr.Length)
                {
                    break;
                }

                //----------------------------------
                // 頂点取得
                //----------------------------------
                double x, y, z;
                double lat, lon;
                // 数値に変換
                double.TryParse(splitStr[loop_cnt + 0], out lat);   // 経度,緯度で格納されている
                double.TryParse(splitStr[loop_cnt + 1], out lon);
                double.TryParse(splitStr[loop_cnt + 2], out z);

                // Webメルカトルに変換
//                CommonBL.latlng2merc(lat, lon, out x, out y);
                // 平面直角座標に変換
                CommonBL.Instance.bl2xy(lat, lon, out x, out y);
                // 画面中央を(0,0)とした座標系に変換
                x = x - MapExtent.Instance.centerX;
                y = y - MapExtent.Instance.centerY;
                // 画面範囲に対応するよう倍率を調整
                x = x * MapExtent.Instance.areaScaleX;
                y = y * MapExtent.Instance.areaScaleY;
                // 範囲チェックするモードの場合
                if (areaCheckMode == 0 || areaCheckMode == 1)
                {
                    // 範囲外の頂点の場合
                    if (Math.Abs(x) > MapExtent.Instance.importDist || Math.Abs(y) > MapExtent.Instance.importDist)
                    {
                        cnt++;
                    }
                }

                // １点でも範囲外の頂点があればスルーするモードの場合
                if (areaCheckMode == 0 && cnt > 0)
                {
                    // 範囲外を含むオブジェクトは表示対象外とする
                    rtnData.Clear();
                    return rtnData;
                }
                // 頂点を追加
                rtnData.Add(new Vector3((float)x, (float)y, (float)z));
            }

            // 全点範囲外ならスルーするモード
            if (areaCheckMode == 0 && cnt == rtnData.Count)
            {
                // 範囲外を含むオブジェクトは表示対象外とする
                rtnData.Clear();
            }

            return rtnData;
        }
    }
}
