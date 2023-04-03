using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SkylinesPlateau
{
    public class GmlCodeListData
    {
        public string key;
        public string value;

        public GmlCodeListData(string data1, string data2)
        {
        	key = data1;
        	value = data2;
        }

        /// <summary>
        /// コードリストから該当するKey値の設定値を取得する
        /// </summary>
        static public string FindData(Dictionary<string, List<GmlCodeListData>> codeListMap, string fileName, string keyStr)
        {
        	string rtnStr = "";
            List<GmlCodeListData> list = null;
        	
        	//----------------------------------------------
        	// GMLファイルが読み込み済みか判定
        	//----------------------------------------------
        	// 読み込み済みの場合
            if (codeListMap.ContainsKey(fileName))
            {
            	list = codeListMap[fileName];
            }
        	// 読み込み済みではない場合
        	else
        	{
        		// GMLファイルを読み込む
        		GmlUtil gml = GmlUtil(fileName);
        		list = GmlUtil.readData();
        		// 読み込みデータを保持
        		codeListMap[fileName] = list;
        	}
        	
        	//----------------------------------------------
        	// リスト内に該当データが存在するか判定
        	//----------------------------------------------
        	var match = codeListMap[fileName].FindAll(item =>
        	{
        		if(item.key == keyStr)
        		{
        			return true;
        		}
        		return false;
        	}
        	// 発見できた場合
        	if(match.count > 0)
        	{
        		return match[0].value;
        	}
        	return "";
        }
    }
}
