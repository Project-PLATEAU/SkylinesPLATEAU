//----------------------------------------------------------------------------
// Logger.cs
//
// ■概要
//      ログ出力機能を提供するクラス
// 
// ■改版履歴
//      Ver00.03.00     2020.03.31      G.Arakawa@Cmind     新規作成
//
//----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SkylinesPlateau
{
	public static class Logger
	{
		private static string Format(object o)
		{
			if (o != null)
			{
				string text;
				if ((text = (o as string)) != null)
				{
					string text2 = text;
                    string titleStr = (Attribute.GetCustomAttribute(System.Reflection.Assembly.GetExecutingAssembly(), typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute).Title;
					return "[ " + titleStr + " " + DateTime.Now.ToString("s") + " ] " + text2;
				}
				Exception ex;
				if ((ex = (o as Exception)) != null)
				{
					Exception ex2 = ex;
					return "Exception({" + ex2.GetType() + "}): {" + ex2.Message + "}";
				}
			}
			return Format("object({" + o.GetType() + "}): {" + o + "}");
		}

		public static void Log(object o)
		{
#if TOOL_DEBUG
			System.Diagnostics.Debug.WriteLine(o);
#else
			Debug.Log((object)Format(o));
#endif
		}

		public static void Log(object o, UnityEngine.Object context)
		{
			Debug.Log((object)Format(o), context);
		}

		public static void Warning(object o)
		{
			Debug.LogWarning((object)Format(o));
		}

		public static void Warning(object o, UnityEngine.Object context)
		{
			Debug.LogWarning((object)Format(o), context);
		}

		public static void Error(object o)
		{
			Debug.LogError((object)Format(o));
		}

		public static void Error(object o, UnityEngine.Object context)
		{
			Debug.LogError((object)Format(o), context);
		}

		public static void ErrorIf(bool condition, object o)
		{
		}

		public static void ErrorIf(bool condition, object o, UnityEngine.Object context)
		{
		}

		public static void Log(string str, List<Vector3> data)
		{
			string logStr = "";
			foreach(Vector3 vec in data)
			{
				if (logStr != "") logStr += " ";
//				logStr += "" + vec.x + " " + vec.y + " " + vec.z + "";
				logStr += "" + vec.x + " " + vec.y;
			}

#if TOOL_DEBUG
			System.Diagnostics.Debug.WriteLine(str + logStr);
#else
			Debug.Log((object)Format(str + logStr));
#endif
		}
	}
}
