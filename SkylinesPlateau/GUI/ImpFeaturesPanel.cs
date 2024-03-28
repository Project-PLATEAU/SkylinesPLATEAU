using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using SkylinesPlateau.common;
using UIUtils = SkylinesPlateau.common.UIUtils;
using System.IO;

namespace SkylinesPlateau
{
    class ImpFeaturesPanel : UIPanel
	{
		//----------------------------------------------
		// 固定値
		//----------------------------------------------
		// ダイアログの各種名称
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
		private const string DIALOG_TITLE = "地物読込";
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END
		private const string DIALOG_MSG_BTN_OK = "インポート";
		private const string DIALOG_MSG_BTN_CANSEL = "キャンセル";
		private const string DIALOG_MSG_IMPTYPE = "インポートする地物を選択";
//		private const string DIALOG_MSG_IMPTYPE_1 = "河川";
		private const string DIALOG_MSG_IMPTYPE_2 = "道路";
		private const string DIALOG_MSG_IMPTYPE_3 = "線路";
		private const string DIALOG_MSG_IMPTYPE_4 = "一般建物";
		private const string DIALOG_MSG_IMPTYPE_5 = "主要建物";
		private const string DIALOG_MSG_IMPTYPE_6 = "区域区分";
//		private const string DIALOG_MSG_IMPTYPE_7 = "土地利用";
		private const string DIALOG_MSG_CENTER = "中心座標";
		private const string DIALOG_MSG_AREA = "読み込み範囲 (Km)";
		private const string DIALOG_MSG_AREA_HOSOKU = "*1タイルはおよそ2km x 2kmです";
//		private const string DIALOG_MSG_ZONE = "区画用途が判定できない場合";
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
//		private const string DIALOG_MSG_SYS = "座標系番号";
		private const string DIALOG_MSG_SYS = "座標系系番号";
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
		private const string DIALOG_MSG_FOLDER = "3D都市モデルフォルダ";
		private const string DIALOG_MSG_CENTER_HOSOKU = "10進法緯度経度、カンマ区切りで入力";
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END
		// ダイアログサイズ
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
//		private const int DIALOG_SIZE_W = 350;
//		private const int DIALOG_SIZE_H = 540;
		private const int DIALOG_SIZE_W = 500;
		private const int DIALOG_SIZE_H = 560;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END
		// 各項目間のスペース
		private const int DIALOG_SPACE_H1 = 20;
		// 同一項目間のスペース
		private const int DIALOG_SPACE_H2 = 10;
//        private static readonly string[] ZONE_DROPDOWN = { "読み込まない", "低密度住宅", "高密度住宅", "低密度商業", "高密度商業", "産業", "オフィス" };
		// 平面直角座標系
		private static readonly string[] SYS_DROPDOWN = CommonBL.SYS_NAME_LIST;

		//----------------------------------------------
		// メンバ変数
		//----------------------------------------------
		private UITextureAtlas _ingameAtlas;
		private UIDragHandle _dragHandle;
		private UILabel _lblTitle;
		private UIButton _btnClose;
		private UILabel _lblFitureTitle;
//		private UICheckBox _chkImpWaterway;
		private UICheckBox _chkImpRoad;
		private UICheckBox _chkImpRail;
		private UICheckBox _chkImpBuilding;
		private UICheckBox _chkImpUniqueBuilding;
		private UICheckBox _chkImpZone;
//		private UICheckBox _chkImpArea;
		private UILabel _lblCenterTitle;
		private UITextField _txtCenter;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] DEL_START
//		private UICheckBox _chkUseAreaSize;
//		private UITextField _txtAreaSize;
//		private UILabel _lblUseAreaMsg;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] DEL_END
//		private UILabel _lblZoneTitle;
//		private UIDropDown _dropZone;
		private UIButton _btnOk;
		private UIButton _btnCansel;
		private UILabel _lblSystemTitle;
		private UIDropDown _dropSystem;
		private bool _initialized = false;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
		private UILabel _lblFolderTitle;
		private UITextField _txtFolder;
		private UILabel _lblCenterMsg;
		private UIFolderDialog _folderDialog = null;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END

		public override void Start()
		{
			//----------------------------------
			// 画面に項目追加
			//----------------------------------
			try
			{
				//----------------------------------
				// テクスチャ取得
				//----------------------------------
				UITextureAtlas[] array = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
				_ingameAtlas = null;
				for (int i = 0; i < array.Length; i++)
				{
					if (((UnityEngine.Object)array[i]).name == "Ingame")
					{
						_ingameAtlas = array[i];
						break;
					}
				}
				if (_ingameAtlas == null)
				{
					_ingameAtlas = UIView.GetAView().defaultAtlas;
				}

				//----------------------------------
				// パネル生成
				//----------------------------------
				base.backgroundSprite = "MenuPanel2";
				base.isVisible = false;
				canFocus = true;
				isInteractive = true;
				base.width = DIALOG_SIZE_W;
				base.height = DIALOG_SIZE_H;
				base.relativePosition = new Vector3(Mathf.Floor(((float)GetUIView().fixedWidth - base.width) / 2f), Mathf.Floor(((float)GetUIView().fixedHeight - base.height) / 2f));
				_dragHandle = UIUtils.CreateMenuPanelDragHandle(this);

				//----------------------------------
				// タイトル
				//----------------------------------
				_lblTitle = UIUtils.CreateMenuPanelTitle(this, _ingameAtlas, DIALOG_TITLE);

				//----------------------------------
				// 閉じるボタン
				//----------------------------------
				_btnClose = UIUtils.CreateMenuPanelCloseButton(this, _ingameAtlas);


				// 描画開始の基準点
				float posX = 20f;
				float posY = 50f;

// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
#if false
				//----------------------------------
				// 地物選択の項目
				//----------------------------------
				// 項目名
				_lblFitureTitle = UIUtils.CreateLabel(this, "_lblFitureTitle", DIALOG_MSG_IMPTYPE);
				_lblFitureTitle.width = DIALOG_SIZE_W - 40f;
				_lblFitureTitle.relativePosition = new Vector3(posX, posY);
				posX += 20;
				posY += _lblFitureTitle.height + DIALOG_SPACE_H2;
				// 道路
				_chkImpRoad = createCheckBox(posX, posY, "_chkImpRoad", DIALOG_MSG_IMPTYPE_2, ImportSettingData.Instance.isImpRoad);
				_chkImpRoad.eventCheckChanged += delegate (UIComponent component, bool value) { ImportSettingData.Instance.isImpRoad = value; };
				posY += _chkImpRoad.height + DIALOG_SPACE_H2;
				// 線路
				_chkImpRail = createCheckBox(posX, posY, "_chkImpRail", DIALOG_MSG_IMPTYPE_3, ImportSettingData.Instance.isImpRail);
				_chkImpRail.eventCheckChanged += delegate (UIComponent component, bool value) { ImportSettingData.Instance.isImpRail = value; };
				posY += _chkImpRail.height + DIALOG_SPACE_H2;
				// 一般建築物
				_chkImpBuilding = createCheckBox(posX, posY, "_chkImpBuilding", DIALOG_MSG_IMPTYPE_4, ImportSettingData.Instance.isImpBuilding);
				_chkImpBuilding.eventCheckChanged += delegate (UIComponent component, bool value) { ImportSettingData.Instance.isImpBuilding = value; };
				posY += _chkImpBuilding.height + DIALOG_SPACE_H2;
				// 特定建築物
				_chkImpUniqueBuilding = createCheckBox(posX, posY, "_chkImpUniqueBuilding", DIALOG_MSG_IMPTYPE_5, ImportSettingData.Instance.isImpUniqueBuilding);
				_chkImpUniqueBuilding.eventCheckChanged += delegate (UIComponent component, bool value) { ImportSettingData.Instance.isImpUniqueBuilding = value; };
				posY += _chkImpUniqueBuilding.height + DIALOG_SPACE_H2;
				// 区域区分
				_chkImpZone = createCheckBox(posX, posY, "_chkImpZone", DIALOG_MSG_IMPTYPE_6, ImportSettingData.Instance.isImpZone);
				_chkImpZone.eventCheckChanged += delegate (UIComponent component, bool value) { ImportSettingData.Instance.isImpZone = value; };
				posY += _chkImpZone.height + DIALOG_SPACE_H2;
//				// 土地利用
//				_chkImpArea = createCheckBox(posX, posY, "_chkImpArea", DIALOG_MSG_IMPTYPE_7, ImportSettingData.Instance.isImpArea);
//				_chkImpArea.eventCheckChanged += delegate (UIComponent component, bool value) { ImportSettingData.Instance.isImpArea = value; };
//				posY += _chkImpArea.height + DIALOG_SPACE_H2;
//				// 河川
//				_chkImpWaterway = createCheckBox(posX, posY, "_chkImpWaterway", DIALOG_MSG_IMPTYPE_1, ImportSettingData.Instance.isImpWaterway);
//				_chkImpWaterway.eventCheckChanged += delegate (UIComponent component, bool value) { ImportSettingData.Instance.isImpWaterway = value; };
//				posY += _chkImpWaterway.height + DIALOG_SPACE_H2;
				posX -= 20;
				// 次の項目用に始点位置を調整
				posY += DIALOG_SPACE_H1;

				//----------------------------------
				// 中心位置指定の項目
				//----------------------------------
				// 項目名
				_lblCenterTitle = UIUtils.CreateLabel(this, "_lblCenterTitle", DIALOG_MSG_CENTER);
				_lblCenterTitle.width = DIALOG_SIZE_W - 40f;
				_lblCenterTitle.relativePosition = new Vector3(posX, posY);
				posY += _lblCenterTitle.height + DIALOG_SPACE_H2;
				// テキストフィールド
				_txtCenter = UIUtils.CreateTextField(this, "_txtCenter", _ingameAtlas, ImportSettingData.Instance.center);
				_txtCenter.width = _txtCenter.parent.width - 60f;
				_txtCenter.relativePosition = new Vector3(posX + 20f, posY);
				_txtCenter.eventTextSubmitted += delegate (UIComponent component, string value)
				{
					ImportSettingData.Instance.center = value;
				};
				posY += _txtCenter.height + DIALOG_SPACE_H1;

				//----------------------------------
				// 系番号ドロップダウン
				//----------------------------------
				_lblSystemTitle = UIUtils.CreateLabel(this, "_lblSystemTitle", DIALOG_MSG_SYS);
				_lblSystemTitle.relativePosition = new Vector3(posX, posY);
				posY += _lblSystemTitle.height + DIALOG_SPACE_H2;
				_dropSystem = UIUtils.CreateDropDown(this, "_dropSystem", _ingameAtlas);
				_dropSystem.items = SYS_DROPDOWN;
				_dropSystem.width = DIALOG_SIZE_W - 60f;
				_dropSystem.relativePosition = new Vector3(posX + 20f, posY);
				_dropSystem.selectedIndex = ImportSettingData.Instance.isystem;
				_dropSystem.eventSelectedIndexChanged += delegate (UIComponent component, int value)
				{
					ImportSettingData.Instance.isystem = value;
				};
				posY += _dropSystem.height + DIALOG_SPACE_H1;

				//----------------------------------
				// 中心位置からの範囲指定
				//----------------------------------
				// 項目名（チェックボックス）
				_chkUseAreaSize = createCheckBox(posX, posY, "_chkUseAreaSize", DIALOG_MSG_AREA, ImportSettingData.Instance.isUseAreaSize);
				_chkUseAreaSize.eventCheckChanged += delegate (UIComponent component, bool value)
				{
					ImportSettingData.Instance.isUseAreaSize = value;
					_txtAreaSize.isInteractive = value;
				};
				posY += _chkUseAreaSize.height + DIALOG_SPACE_H2;
				// テキストフィールド
				_txtAreaSize = UIUtils.CreateTextField(this, "_txtAreaSize", _ingameAtlas, ImportSettingData.Instance.areaSize);
				_txtAreaSize.width = DIALOG_SIZE_W - 60f;
				_txtAreaSize.relativePosition = new Vector3(posX + 20f, posY);
				_txtAreaSize.isInteractive = ImportSettingData.Instance.isUseAreaSize;
				_txtAreaSize.eventTextSubmitted += delegate (UIComponent component, string value)
				{
					ImportSettingData.Instance.areaSize = value;
				};
				posY += _txtAreaSize.height + DIALOG_SPACE_H2;
				// 補足メッセージ
				_lblUseAreaMsg = UIUtils.CreateLabel(this, "_lblUseAreaMsg", DIALOG_MSG_AREA_HOSOKU);
				_lblUseAreaMsg.width = DIALOG_SIZE_W - 40f;
				_lblUseAreaMsg.relativePosition = new Vector3(posX + 20f, posY);
				posY += _lblUseAreaMsg.height + DIALOG_SPACE_H1;

/*
				//----------------------------------
				// 区域ドロップダウン
				//----------------------------------
				_lblZoneTitle = UIUtils.CreateLabel(this, "_lblZoneTitle", DIALOG_MSG_ZONE);
				_lblZoneTitle.relativePosition = new Vector3(posX, posY);
				posY += _lblZoneTitle.height + DIALOG_SPACE_H2;
				_dropZone = UIUtils.CreateDropDown(this, "_dropZone", _ingameAtlas);
				_dropZone.items = ZONE_DROPDOWN;
				_dropZone.width = DIALOG_SIZE_W - 60f;
				_dropZone.relativePosition = new Vector3(posX + 20f, posY);
				_dropZone.selectedIndex = 0;
				_dropZone.eventSelectedIndexChanged += delegate (UIComponent component, int value)
				{
					ImportSettingData.Instance.zoneType = value;
				};
				posY += _dropZone.height + DIALOG_SPACE_H1;
*/
				//----------------------------------
				// OKボタン
				//----------------------------------
				_btnOk = UIUtils.CreateSpriteButton(this, "_btnOk", _ingameAtlas, "ButtonMenu");
				_btnOk.text = DIALOG_MSG_BTN_OK;
				_btnOk.height = 30f;
				_btnOk.width = _btnOk.parent.width / 2f - 20f;
				_btnOk.relativePosition = new Vector3(10f, posY);
				_btnOk.eventClicked += delegate (UIComponent component, UIMouseEventParameter eventParam)
				{
					if (!eventParam.used)
					{
						if (!ImportSettingData.Instance.isImpRoad && 
							!ImportSettingData.Instance.isImpRail && 
							!ImportSettingData.Instance.isImpBuilding && 
							!ImportSettingData.Instance.isImpUniqueBuilding && 
							!ImportSettingData.Instance.isImpZone)
						{
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("入力値が不正", "インポート対象を選択してください。", false);
							return;
						}
						if (ImportSettingData.Instance.center.Length == 0)
						{
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("入力値が不正", "中心位置を指定してください。", false);
							return;
						}
						if (ImportSettingData.Instance.isUseAreaSize &&
							ImportSettingData.Instance.areaSize.Length == 0)
						{
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("入力値が不正", "読み込み範囲を指定してください。", false);
							return;
						}
						string[] splitStr = ImportSettingData.Instance.center.Split(',');
						if (splitStr.Length != 2)
						{
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("入力値が不正", "中心位置は半角カンマ区切りで指定してください。", false);
							return;
						}
						// 一般建物インポート時に必死テーブルが存在しない場合を考慮
						if (ImportSettingData.Instance.isImpBuilding)
						{
							if (!File.Exists(BuildingSgTbl.TBL_FILE_USAGE))
							{
								ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
								panel.SetMessage("環境設定が不正", "建物判定用テーブルを参照できませんでした。\n" + BuildingSgTbl.TBL_FILE_USAGE, false);
								return;
							}
						}
						// 主要建物インポート時に必死テーブルが存在しない場合を考慮
						if (ImportSettingData.Instance.isImpUniqueBuilding)
						{
							if (!File.Exists(BuildingSgTbl.TBL_FILE_BUILDING_NAME) && 
								!File.Exists(BuildingSgTbl.TBL_FILE_BUILDING_ID) &&
								!File.Exists(BuildingSgTbl.TBL_FILE_DETAILED_USAGE) &&
								!File.Exists(BuildingSgTbl.TBL_FILE_ORG_USAGE2) && 
								!File.Exists(BuildingSgTbl.TBL_FILE_ORG_USAGE))
							{
								ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
								panel.SetMessage("環境設定が不正", "建物判定用テーブルを参照できませんでした。\n" + BuildingSgTbl.TBL_FILE_BUILDING_NAME, false);
								return;
							}
						}
						// 区域区画インポート時に必死テーブルが存在しない場合を考慮
						if (ImportSettingData.Instance.isImpZone)
						{
							if (!File.Exists(ZoneSgTbl.TBL_FILE))
							{
								ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
								panel.SetMessage("環境設定が不正", "区画テーブルを参照できませんでした。\n" + BuildingSgTbl.TBL_FILE_BUILDING_NAME, false);
								return;
							}
						}

						//----------------------------------
						// 設定項目を保存
						//----------------------------------
						ImportSettingData.Instance.Save();

						//----------------------------------
						// インポート処理
						//----------------------------------
// 2022.11.22 G.Arakawa@cmind [平面直角座標系が９系以外、正常に動作していない不具合対応] ADD_START
						// 座標系の設定
						CommonBL.Instance.iSystem = ImportSettingData.Instance.isystem + 1;
// 2022.11.22 G.Arakawa@cmind [平面直角座標系が９系以外、正常に動作していない不具合対応] ADD_END

						MapExtent.Instance.SetMapExtent();
						int impAllNum = 0;
						int impNum = 0;
						string msg = "";
//						// 河川
//						if (ImportSettingData.Instance.isImpWaterway)
//						{
//							impNum = GmlWaterwayData.Import();
//							msg += ("河川： " + impNum + "件\n");
//							impAllNum += impNum;
//						}
						// 通常道路
						if (ImportSettingData.Instance.isImpRoad)
						{
							impNum = GmlRoadData.Import();
							msg += ("道路： " + impNum + "件\n");
							impAllNum += impNum;
						}
						// 線路
						if (ImportSettingData.Instance.isImpRail)
						{
							impNum = GmlRailData.Import();
							msg += ("線路： " + impNum + "件\n");
							impAllNum += impNum;
						}
						// 一般建築物, 特定建築物
						if (ImportSettingData.Instance.isImpBuilding ||
							ImportSettingData.Instance.isImpUniqueBuilding)
						{
							impNum = GmlBuildingData.Import();
							msg += ("建物, 区域： " + impNum + "件\n");
							impAllNum += impNum;
						}
						// 区域区分
						if (ImportSettingData.Instance.isImpZone)
						{
							impNum = GmlZoneData.Import();
							msg += ("区域区分： " + impNum + "件\n");
							impAllNum += impNum;
						}
//						// 土地利用
//						if (ImportSettingData.Instance.isImpArea)
//						{
//							impNum = GmlZoneData.Import();
//							msg += ("土地利用： " + impNum + "件\n");
//							impAllNum += impNum;
//						}

						//----------------------------------
						// 画面を閉じる
						//----------------------------------
						this.Hide();
						eventParam.Use();
						_initialized = false;

						//----------------------------------
						// ダイアログ表示
						//----------------------------------
						if (impAllNum == 0)
						{
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("インポート対象なし", "範囲内にデータはありませんでした。", false);
						}
						else
						{
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("インポート完了", msg, false);
						}
					}
				};
#endif
				//----------------------------------
				// 地物選択の項目
				//----------------------------------
				// 項目名
				_lblFitureTitle = UIUtils.CreateLabel(this, "_lblFitureTitle", DIALOG_MSG_IMPTYPE);
				_lblFitureTitle.width = DIALOG_SIZE_W - 40f;
				_lblFitureTitle.relativePosition = new Vector3(posX, posY);
				posX += 20;
				posY += _lblFitureTitle.height + DIALOG_SPACE_H2;
				// 道路
				_chkImpRoad = createCheckBox(posX, posY, "_chkImpRoad", DIALOG_MSG_IMPTYPE_2, IniFileData.Instance.isImpRoad);
				_chkImpRoad.eventCheckChanged += delegate (UIComponent component, bool value) { IniFileData.Instance.isImpRoad = value; };
				posY += _chkImpRoad.height + DIALOG_SPACE_H2;
				// 線路
				_chkImpRail = createCheckBox(posX, posY, "_chkImpRail", DIALOG_MSG_IMPTYPE_3, IniFileData.Instance.isImpRail);
				_chkImpRail.eventCheckChanged += delegate (UIComponent component, bool value) { IniFileData.Instance.isImpRail = value; };
				posY += _chkImpRail.height + DIALOG_SPACE_H2;
				// 一般建築物
				_chkImpBuilding = createCheckBox(posX, posY, "_chkImpBuilding", DIALOG_MSG_IMPTYPE_4, IniFileData.Instance.isImpBuilding);
				_chkImpBuilding.eventCheckChanged += delegate (UIComponent component, bool value) { IniFileData.Instance.isImpBuilding = value; };
				posY += _chkImpBuilding.height + DIALOG_SPACE_H2;
				// 特定建築物
				_chkImpUniqueBuilding = createCheckBox(posX, posY, "_chkImpUniqueBuilding", DIALOG_MSG_IMPTYPE_5, IniFileData.Instance.isImpUniqueBuilding);
				_chkImpUniqueBuilding.eventCheckChanged += delegate (UIComponent component, bool value) { IniFileData.Instance.isImpUniqueBuilding = value; };
				posY += _chkImpUniqueBuilding.height + DIALOG_SPACE_H2;
				// 区域区分
				_chkImpZone = createCheckBox(posX, posY, "_chkImpZone", DIALOG_MSG_IMPTYPE_6, IniFileData.Instance.isImpZone);
				_chkImpZone.eventCheckChanged += delegate (UIComponent component, bool value) { IniFileData.Instance.isImpZone = value; };
				posY += _chkImpZone.height + DIALOG_SPACE_H2;
				posX -= 20;
				// 次の項目用に始点位置を調整
				posY += DIALOG_SPACE_H1;
				
				//----------------------------------
				// 地形：３Ｄ都市モデルの項目
				//----------------------------------
				float btn_w = 30f;
				float btn_h = 22f;
				// 項目名
				_lblFolderTitle = UIUtils.CreateLabel(this, "_lblFolderTitle", DIALOG_MSG_FOLDER);
				_lblFolderTitle.width = DIALOG_SIZE_W - 40f;
				_lblFolderTitle.relativePosition = new Vector3(posX, posY);
				posY += _lblFolderTitle.height + DIALOG_SPACE_H2;
				// テキストフィールド
				_txtFolder = UIUtils.CreateTextField(this, "_txtFolder", _ingameAtlas, IniFileData.Instance.inputFolderPath);
				_txtFolder.width = _txtFolder.parent.width - 60f;
				_txtFolder.relativePosition = new Vector3(posX + 20f, posY);
				_txtFolder.eventTextSubmitted += delegate (UIComponent component, string value)
				{
					IniFileData.Instance.inputFolderPath = value;
				};
				_txtFolder.padding.right = (int)Math.Ceiling(btn_w) + 2;
				// フォルダ参照用ボタン
				UIButton folderButton = UIUtils.Instance.CreateImageButton(_txtFolder, "FolderIconButton", "folderIcon");
				folderButton.tooltip = "";
				folderButton.height = btn_h;
				folderButton.width = btn_w;
				folderButton.relativePosition = new Vector3(_txtFolder.width - (btn_w + 5.0f), (_txtFolder.height - btn_h) / 2.0f);
				folderButton.eventClicked += delegate (UIComponent component, UIMouseEventParameter eventParam)
				{
					if (_folderDialog != null)
					{
						_folderDialog.Visible = true;
						if (!_folderDialog.Visible)
						{
							_folderDialog = null;
						}
					}
					if (_folderDialog == null)
					{
						_folderDialog = new UIFolderDialog();
						_folderDialog.onOkButtonCallback = () => {
							IniFileData.Instance.inputFolderPath = _folderDialog.m_FolderPath;
							_txtFolder.text = _folderDialog.m_FolderPath;
						};
						_folderDialog.onCancelButtonCallback = () => {
						};
						_folderDialog.m_FolderPath = IniFileData.Instance.inputFolderPath;
						_folderDialog.drawDialog(this);
					}
					else
					{
						_folderDialog.m_FolderPath = IniFileData.Instance.inputFolderPath;
						_folderDialog.Visible = true;
					}
				};
				posY += _txtFolder.height + DIALOG_SPACE_H1;

				//----------------------------------
				// 中心位置指定の項目
				//----------------------------------
				// 項目名
				_lblCenterTitle = UIUtils.CreateLabel(this, "_lblCenterTitle", DIALOG_MSG_CENTER);
				_lblCenterTitle.width = DIALOG_SIZE_W - 40f;
				_lblCenterTitle.relativePosition = new Vector3(posX, posY);
				posY += _lblCenterTitle.height + DIALOG_SPACE_H2;
				// テキストフィールド
				_txtCenter = UIUtils.CreateTextField(this, "_txtCenter", _ingameAtlas, IniFileData.Instance.center);
				_txtCenter.width = _txtCenter.parent.width - 60f;
				_txtCenter.relativePosition = new Vector3(posX + 20f, posY);
				_txtCenter.eventTextSubmitted += delegate (UIComponent component, string value)
				{
					IniFileData.Instance.center = value;
				};
				posY += _txtCenter.height + DIALOG_SPACE_H2;
				// 補足メッセージ
				_lblCenterMsg = UIUtils.CreateLabel(this, "_lblCenterMsg", DIALOG_MSG_CENTER_HOSOKU);
				_lblCenterMsg.width = DIALOG_SIZE_W - 40f;
				_lblCenterMsg.relativePosition = new Vector3(posX + 20f, posY);
				posY += _lblCenterMsg.height + DIALOG_SPACE_H1;

				//----------------------------------
				// 系番号ドロップダウン
				//----------------------------------
				_lblSystemTitle = UIUtils.CreateLabel(this, "_lblSystemTitle", DIALOG_MSG_SYS);
				_lblSystemTitle.relativePosition = new Vector3(posX, posY);
				posY += _lblSystemTitle.height + DIALOG_SPACE_H2;
				_dropSystem = UIUtils.CreateDropDown(this, "_dropSystem", _ingameAtlas);
				_dropSystem.items = SYS_DROPDOWN;
				_dropSystem.width = DIALOG_SIZE_W - 60f;
				_dropSystem.relativePosition = new Vector3(posX + 20f, posY);
				_dropSystem.selectedIndex = IniFileData.Instance.isystem;
				_dropSystem.eventSelectedIndexChanged += delegate (UIComponent component, int value)
				{
					IniFileData.Instance.isystem = value;
				};
				posY += _dropSystem.height + DIALOG_SPACE_H1;

				//----------------------------------
				// OKボタン
				//----------------------------------
				_btnOk = UIUtils.CreateSpriteButton(this, "_btnOk", _ingameAtlas, "ButtonMenu");
				_btnOk.text = DIALOG_MSG_BTN_OK;
				_btnOk.height = 30f;
				_btnOk.width = _btnOk.parent.width / 2f - 20f;
				_btnOk.relativePosition = new Vector3(10f, posY);
				_btnOk.eventClicked += delegate (UIComponent component, UIMouseEventParameter eventParam)
				{
					if (!eventParam.used)
					{
						if (!IniFileData.Instance.isImpRoad &&
							!IniFileData.Instance.isImpRail &&
							!IniFileData.Instance.isImpBuilding &&
							!IniFileData.Instance.isImpUniqueBuilding &&
							!IniFileData.Instance.isImpZone)
						{
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("入力値が不正", "インポート対象を選択してください。", false);
							return;
						}
						if (IniFileData.Instance.center.Length == 0)
						{
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("入力値が不正", "中心位置を指定してください。", false);
							return;
						}
						if (IniFileData.Instance.inputFolderPath.Length == 0)
						{
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("入力値が不正", "3D都市モデルフォルダを指定してください。", false);
							return;
						}
						if (IniFileData.Instance.isUseAreaSize &&
							IniFileData.Instance.areaSize.Length == 0)
						{
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("入力値が不正", "読み込み範囲を指定してください。", false);
							return;
						}
						string[] splitStr = IniFileData.Instance.center.Split(',');
						if (splitStr.Length != 2)
						{
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("入力値が不正", "中心位置は半角カンマ区切りで指定してください。", false);
							return;
						}

						//----------------------------------
						// 設定項目を保存
						//----------------------------------
						IniFileData.Instance.Save();
						SettingsUI.UpdateOptionSetting();

						//----------------------------------
						// インポート処理
						//----------------------------------
// 2022.11.22 G.Arakawa@cmind [平面直角座標系が９系以外、正常に動作していない不具合対応] ADD_START
						// 座標系の設定
						CommonBL.Instance.iSystem = IniFileData.Instance.isystem + 1;
// 2022.11.22 G.Arakawa@cmind [平面直角座標系が９系以外、正常に動作していない不具合対応] ADD_END

						MapExtent.Instance.SetMapExtent();
						int impAllNum = 0;
						int impNum = 0;
						string msg = "";
						// 通常道路
						if (IniFileData.Instance.isImpRoad)
						{
							impNum = GmlRoadData.Import();
							msg += ("道路： " + impNum + "件\n");
							impAllNum += impNum;
						}
						// 線路
						if (IniFileData.Instance.isImpRail)
						{
							impNum = GmlRailData.Import();
							msg += ("線路： " + impNum + "件\n");
							impAllNum += impNum;
						}
						// 一般建築物, 特定建築物
						if (IniFileData.Instance.isImpBuilding ||
							IniFileData.Instance.isImpUniqueBuilding)
						{
							impNum = GmlBuildingData.Import();
							msg += ("建物, 区域： " + impNum + "件\n");
							impAllNum += impNum;
						}
						// 区域区分
						if (IniFileData.Instance.isImpZone)
						{
							impNum = GmlZoneData.Import();
							msg += ("区域区分： " + impNum + "件\n");
							impAllNum += impNum;
						}

						//----------------------------------
						// 画面を閉じる
						//----------------------------------
						this.Hide();
						eventParam.Use();
						_initialized = false;

						//----------------------------------
						// ダイアログ表示
						//----------------------------------
						if (impAllNum == 0)
						{
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("インポート対象なし", "範囲内にデータはありませんでした。", false);
						}
						else
						{
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("インポート完了", msg, false);
						}
					}
				};
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END

				//----------------------------------
				// Cancelボタン
				//----------------------------------
				_btnCansel = UIUtils.CreateSpriteButton(this, "_btnCansel", _ingameAtlas, "ButtonMenu");
				_btnCansel.text = DIALOG_MSG_BTN_CANSEL;
				_btnCansel.height = 30f;
				_btnCansel.width = _btnOk.width;
				_btnCansel.relativePosition = new Vector3(10f + _btnOk.width + 10f, posY);
				_btnCansel.eventClicked += delegate (UIComponent component, UIMouseEventParameter eventParam)
				{
					if (!eventParam.used)
					{
						// 画面を閉じる
						this.Hide();
						eventParam.Use();
						_initialized = false;
					}
				};
			}
			catch (Exception ex)
			{
				Debug.Log((object)("[SkylinesCityGml] ImpFeaturesPanel:Start -> Exception: " + ex.Message));
			}

		}

		// 表示状態にする
		public void drawDialog()
		{
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
#if false
			// 表示内容を最新化
//			_chkImpWaterway.isChecked = ImportSettingData.Instance.isImpWaterway;
			_chkImpRoad.isChecked = ImportSettingData.Instance.isImpRoad;
			_chkImpRail.isChecked = ImportSettingData.Instance.isImpRail;
			_chkImpBuilding.isChecked = ImportSettingData.Instance.isImpBuilding;
			_chkImpUniqueBuilding.isChecked = ImportSettingData.Instance.isImpUniqueBuilding;
			_chkImpZone.isChecked = ImportSettingData.Instance.isImpZone;
//			_chkImpArea.isChecked = ImportSettingData.Instance.isImpArea;
			_chkUseAreaSize.isChecked = ImportSettingData.Instance.isUseAreaSize;
			_txtCenter.text = ImportSettingData.Instance.center;
			_txtAreaSize.text = ImportSettingData.Instance.areaSize;
//			_dropZone.selectedIndex = ImportSettingData.Instance.zoneType;
			_dropSystem.selectedIndex = ImportSettingData.Instance.isystem;
#endif
			// 表示内容を最新化
			_chkImpRoad.isChecked = IniFileData.Instance.isImpRoad;
			_chkImpRail.isChecked = IniFileData.Instance.isImpRail;
			_chkImpBuilding.isChecked = IniFileData.Instance.isImpBuilding;
			_chkImpUniqueBuilding.isChecked = IniFileData.Instance.isImpUniqueBuilding;
			_chkImpZone.isChecked = IniFileData.Instance.isImpZone;
			_txtCenter.text = IniFileData.Instance.center;
			_dropSystem.selectedIndex = IniFileData.Instance.isystem;
			_txtFolder.text = IniFileData.Instance.inputFolderPath;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END
			// 表示状態にｓる
			this.isVisible = true;
		}

		public override void Update()
		{
			base.Update();
			try
			{
				if (!_initialized)
				{
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
#if false
//					_chkImpWaterway.isChecked = ImportSettingData.Instance.isImpWaterway;
					_chkImpRoad.isChecked = ImportSettingData.Instance.isImpRoad;
					_chkImpRail.isChecked = ImportSettingData.Instance.isImpRail;
					_chkImpBuilding.isChecked = ImportSettingData.Instance.isImpBuilding;
					_chkImpUniqueBuilding.isChecked = ImportSettingData.Instance.isImpUniqueBuilding;
					_chkImpZone.isChecked = ImportSettingData.Instance.isImpZone;
//					_chkImpArea.isChecked = ImportSettingData.Instance.isImpArea;
					_chkUseAreaSize.isChecked = ImportSettingData.Instance.isUseAreaSize;
					_txtCenter.text = ImportSettingData.Instance.center;
					_txtAreaSize.text = ImportSettingData.Instance.areaSize;
//					_dropZone.selectedIndex = ImportSettingData.Instance.zoneType;
					_dropSystem.selectedIndex = ImportSettingData.Instance.isystem;
#endif
					_chkImpRoad.isChecked = IniFileData.Instance.isImpRoad;
					_chkImpRail.isChecked = IniFileData.Instance.isImpRail;
					_chkImpBuilding.isChecked = IniFileData.Instance.isImpBuilding;
					_chkImpUniqueBuilding.isChecked = IniFileData.Instance.isImpUniqueBuilding;
					_chkImpZone.isChecked = IniFileData.Instance.isImpZone;
					_txtCenter.text = IniFileData.Instance.center;
					_dropSystem.selectedIndex = IniFileData.Instance.isystem;
					_txtFolder.text = IniFileData.Instance.inputFolderPath;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END
					_initialized = true;
				}
			}
			catch (Exception ex)
			{
				Debug.Log((object)("ImpHighwayPanel:Update -> Exception: " + ex.Message));
			}
		}

		private void DestroyGameObject(UIComponent component)
		{
			if (component != null)
			{
                UnityEngine.Object.Destroy(component.gameObject);
			}
		}

		private UICheckBox createCheckBox(float posX, float posY, string itemName, string title, bool isUse)
		{
			UICheckBox chkBox;
			chkBox = UIUtils.CreateCheckBox(this, itemName, _ingameAtlas, title, state: isUse);
			chkBox.width = chkBox.parent.width - posX * 2;
			chkBox.relativePosition = new Vector3(posX, posY);
			return chkBox;
		}
	}
}
