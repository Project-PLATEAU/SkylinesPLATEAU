using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UIUtils = SkylinesPlateau.common.UIUtils;

// 参考サイト
// https://skylines.paradoxwikis.com/UI_Framework

namespace SkylinesPlateau
{
	internal class ImpGUI : MonoBehaviour
	{
		//----------------------------------------------
		// 固定値
		//----------------------------------------------
		// ボタン名称
		private const string BTN_TITLE_MAPCREATE = "地形読込";
		private const string BTN_TITLE_IMPORT_HW = "高速道路読込";
		private const string BTN_TITLE_IMPORT = "地物読込";
		
		// ボタンサイズ
		private const int BTN_SIZE_W = 150;
		private const int BTN_SIZE_H = 30;
		// ダイアログの各種名称
		private const string DIALOG_TITLE = "地物読込";
		private const string DIALOG_MSG_BTN_OK_1 = "インポート";
		private const string DIALOG_MSG_BTN_OK_2 = "高さマップを作成";
		private const string DIALOG_MSG_BTN_CANSEL = "キャンセル";
		private const string DIALOG_MSG_IMPTYPE = "インポートする地物を選択";
		private const string DIALOG_MSG_IMPTYPE_1 = "河川";
		private const string DIALOG_MSG_IMPTYPE_2 = "道路";
		private const string DIALOG_MSG_IMPTYPE_3 = "線路";
		private const string DIALOG_MSG_IMPTYPE_4 = "一般建築物";
		private const string DIALOG_MSG_IMPTYPE_5 = "特定建築物";
		private const string DIALOG_MSG_IMPTYPE_6 = "区域区分";
		private const string DIALOG_MSG_IMPTYPE_7 = "土地利用";
		private const string DIALOG_MSG_CENTER = "中心座標";
		private const string DIALOG_MSG_AREA = "読み込み範囲指定 (Km)";
		private const string DIALOG_MSG_AREA_HOSOKU = "*1タイルはおよそ2km x 2kmです";
		// ダイアログサイズ
		private const int DIALOG_SIZE_W = 300;
		private const int DIALOG_SIZE_H = 500;
		// ダイアログに表示するメッセージの高さ
		private const int DIALOG_MSG_W = DIALOG_SIZE_W - 20;
		private const int DIALOG_MSG_H = 25;
		// 各項目間のスペース
		private const int DIALOG_SPACE_H1 = 10;
		// 同一項目間のスペース
		private const int DIALOG_SPACE_H2 = 2;

		/// <summary>
		/// インポートモード
		/// </summary>
		public enum GML_IMPORT_MODE
		{
			/// <summary>
			/// 高さ地図
			/// </summary>
			mapCreate = 0,
			/// <summary>
			/// 高速道路取込
			/// </summary>
			highway = 1,
			/// <summary>
			/// 地物取込
			/// </summary>
			features = 2
		}

		//----------------------------------------------
		// メンバ変数
		//----------------------------------------------
		public UIView uiView;
		private UITextureAtlas _ingameAtlas;
		private UIButton _btnImpMapCreate;
		private UIButton _btnImpHighway;
		private UIButton _btnImpFeatures;
		private ImpMapPanel _impMapPanel;
		private ImpHighwayPanel _impHighwayPanel;
		private ImpFeaturesPanel _impFeaturesPanel;

		private GameObject _objBtnImpMapCreate;
		private GameObject _objBtnImpHighway;
		private GameObject _objBtnImpFeatures;

		private void Start()
		{
// 道路のアセット一覧
#if false
			//----------------------------------
			// 使用可能な建物アセットを取得しログ出力
			//----------------------------------
			var map = new Dictionary<NetInfo, int>();
			Logger.Log("[TEST] 道路の全件数 : " + PrefabCollection<NetInfo>.LoadedCount());
			for (uint i = 0; i < PrefabCollection<NetInfo>.LoadedCount(); i++)
			{
				// 建物読み込み
				var prefab = PrefabCollection<NetInfo>.GetLoaded(i);
				if (prefab == null)
				{
					continue;
				}
				Logger.Log("[TEST] NetInfo [" + i + "] : " + prefab.name);

				if (!prefab.name.EndsWith("_Data"))
				{
					continue;
				}
				map[prefab] = 0;
			}
#endif
// 建物のアセット一覧
#if true
			//----------------------------------
			// 使用可能な建物アセットを取得しログ出力
			//----------------------------------
			var map = new Dictionary<BuildingInfo, int>();
			Logger.Log("[TEST] 建物の全件数 : " + PrefabCollection<BuildingInfo>.LoadedCount());
			for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount(); i++)
			{
				// 建物読み込み
				var prefab = PrefabCollection<BuildingInfo>.GetLoaded(i);
				if (prefab == null)
				{
					continue;
				}
				Logger.Log("[TEST] BuildingInfo [" + i + "] : " + prefab.name);

				if (!prefab.name.EndsWith("_Data"))
				{
					continue;
				}
				map[prefab] = 0;
			}

			string assetName = "Cemetery";
			BuildingInfo bi = null;
			// アセットが対応しているか判定
			if (PrefabCollection<BuildingInfo>.LoadedExists(assetName))
			{
				// アセットを用いる
				bi = PrefabCollection<BuildingInfo>.FindLoaded(assetName);
			}
			// 対応しているアセットがない場合
			if (bi == null)
			{
				Logger.Log("×　assetなし：" + assetName);
			}
			else
			{
				Logger.Log("〇　assetあり：" + assetName);
			}
#endif

			//----------------------------------
			// 設定ファイルの読み込み
			//----------------------------------
			ImportSettingData.Instance.Load();

			//----------------------------------
			// テクスチャ取得
			//----------------------------------
			UITextureAtlas[] array = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
			for (int i = 0; i < array.Length; i++)
			{
				if (((UnityEngine.Object)array[i]).name == "Ingame")
				{
					_ingameAtlas = array[i];
					break;
				}
			}
			_ingameAtlas = UIView.GetAView().defaultAtlas;

			//----------------------------------
			// ボタン表示
			//----------------------------------
			ItemClass.Availability ins_mode = Singleton<ToolManager>.instance.m_properties.m_mode;
			// マップエディターモードの場合
			if (ins_mode == ItemClass.Availability.MapEditor)
			{
				//----------------------------------------
				// 高さマップ作成ボタン
				//----------------------------------------
				// パネル
				if (_impMapPanel == null)
				{
					GameObject obj = GameObject.Find("ImpMapPanel");
					_impMapPanel = ((obj != null) ? obj.GetComponent<ImpMapPanel>() : null);
				}
				// ボタン
				_objBtnImpMapCreate = new GameObject("UIButton1");
				_objBtnImpMapCreate.transform.parent = uiView.transform;
				_objBtnImpMapCreate.AddComponent<UIButton>();
				_btnImpMapCreate = _objBtnImpMapCreate.GetComponent<UIButton>();
				_btnImpMapCreate.name = "_btnImpMapCreate";
				_btnImpMapCreate.atlas = _ingameAtlas;
				_btnImpMapCreate.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
				_btnImpMapCreate.normalFgSprite = "ButtonMenu";
				_btnImpMapCreate.hoveredFgSprite = "ButtonMenu" + "Hovered";
				_btnImpMapCreate.pressedFgSprite = "ButtonMenu" + "Pressed";
				_btnImpMapCreate.disabledFgSprite = "ButtonMenu" + "Disabled";
				_btnImpMapCreate.text = BTN_TITLE_MAPCREATE;
				_btnImpMapCreate.height = BTN_SIZE_H;
				_btnImpMapCreate.width = BTN_SIZE_W;
				_btnImpMapCreate.relativePosition = new Vector3(60, 10);
				_btnImpMapCreate.eventClicked += delegate (UIComponent component, UIMouseEventParameter eventParam)
				{
					if (!eventParam.used)
					{
						IniFileData.Instance.Load();
						ImportSettingData.Instance.Load();
						_impMapPanel.drawDialog();
						eventParam.Use();
					}
				};

				//----------------------------------------
				// 高速道路インポートボタン
				//----------------------------------------
				// パネル
				if (_impHighwayPanel == null)
				{
					GameObject obj = GameObject.Find("ImpHighwayPanel");
					_impHighwayPanel = ((obj != null) ? obj.GetComponent<ImpHighwayPanel>() : null);
				}
				// ボタン
				_objBtnImpHighway = new GameObject("UIButton2");
				_objBtnImpHighway.transform.parent = uiView.transform;
				_objBtnImpHighway.AddComponent<UIButton>();
				_btnImpHighway = _objBtnImpHighway.GetComponent<UIButton>();
				_btnImpHighway.name = "_btnImpFeatures";
				_btnImpHighway.atlas = _ingameAtlas;
				_btnImpHighway.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
				_btnImpHighway.normalFgSprite = "ButtonMenu";
				_btnImpHighway.hoveredFgSprite = "ButtonMenu" + "Hovered";
				_btnImpHighway.pressedFgSprite = "ButtonMenu" + "Pressed";
				_btnImpHighway.disabledFgSprite = "ButtonMenu" + "Disabled";
				_btnImpHighway.text = BTN_TITLE_IMPORT_HW;
				_btnImpHighway.height = BTN_SIZE_H;
				_btnImpHighway.width = BTN_SIZE_W;
				_btnImpHighway.relativePosition = new Vector3(70 + BTN_SIZE_W, 10);
				_btnImpHighway.eventClicked += delegate (UIComponent component, UIMouseEventParameter eventParam)
				{
					if (!eventParam.used)
					{
						IniFileData.Instance.Load();
						ImportSettingData.Instance.Load();
						_impHighwayPanel.drawDialog();
						eventParam.Use();
					}
				};
			}
			// ゲームモードの場合
			else if (ins_mode == ItemClass.Availability.Game)
			{
				//----------------------------------------
				// 高速道路インポートボタン
				//----------------------------------------
				// パネル
				if (_impFeaturesPanel == null)
				{
					GameObject obj = GameObject.Find("ImpFeaturesPanel");
					_impFeaturesPanel = ((obj != null) ? obj.GetComponent<ImpFeaturesPanel>() : null);
				}
				// ボタン
				_objBtnImpFeatures = new GameObject("UIButton3");
				_objBtnImpFeatures.transform.parent = uiView.transform;
				_objBtnImpFeatures.AddComponent<UIButton>();
				_btnImpFeatures = _objBtnImpFeatures.GetComponent<UIButton>();
				_btnImpFeatures.name = "_btnImpFeatures";
				_btnImpFeatures.atlas = _ingameAtlas;
				_btnImpFeatures.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
				_btnImpFeatures.normalFgSprite = "ButtonMenu";
				_btnImpFeatures.hoveredFgSprite = "ButtonMenu" + "Hovered";
				_btnImpFeatures.pressedFgSprite = "ButtonMenu" + "Pressed";
				_btnImpFeatures.disabledFgSprite = "ButtonMenu" + "Disabled";
				_btnImpFeatures.text = BTN_TITLE_IMPORT;
				_btnImpFeatures.height = BTN_SIZE_H;
				_btnImpFeatures.width = BTN_SIZE_W;
				_btnImpFeatures.relativePosition = new Vector3(60, 10);
//				_btnImpFeatures.relativePosition = new Vector3(80 + BTN_SIZE_W * 2, 10);
				_btnImpFeatures.eventClicked += delegate (UIComponent component, UIMouseEventParameter eventParam)
				{
					if (!eventParam.used)
					{
						IniFileData.Instance.Load();
						ImportSettingData.Instance.Load();
						_impFeaturesPanel.drawDialog();
						eventParam.Use();
					}
				};
			}
		}

		public void Update()
		{
		}
	}
}