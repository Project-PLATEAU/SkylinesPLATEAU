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
    class ImpMapPanel : UIPanel
	{
		//----------------------------------------------
		// 固定値
		//----------------------------------------------
		// 四隅範囲の座標値
		private const string MAPAREA_PATH = @"Files/SkylinesPlateau/ImportMapArea";
		private const string MAPAREA_NAME = @"ImportMapArea";
		// ダイアログの各種名称
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
//		private const string DIALOG_TITLE = "地形読込";
//		private const string DIALOG_MSG_BTN_OK = "地形読込";
		private const string DIALOG_TITLE = "地形、高速道路読込";
		private const string DIALOG_MSG_BTN_OK = "インポート";
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END
		private const string DIALOG_MSG_BTN_CANSEL = "キャンセル";
		private const string DIALOG_MSG_CENTER = "中心座標";
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
//		private const string DIALOG_MSG_SYS = "座標系番号";
		private const string DIALOG_MSG_SYS = "座標系系番号";
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
		private const string DIALOG_MSG_IMPTYPE = "インポートする地物を選択";
		private const string DIALOG_MSG_IMPTYPE_1 = "地形";
		private const string DIALOG_MSG_IMPTYPE_2 = "高速道路";
		private const string DIALOG_MSG_CENTER_HOSOKU = "10進法緯度経度、カンマ区切りで入力";
		private const string DIALOG_MSG_FOLDER = "3D都市モデルフォルダ";
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END
		// ダイアログサイズ
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
//		private const int DIALOG_SIZE_W = 350;
//		private const int DIALOG_SIZE_H = 280;
		private const int DIALOG_SIZE_W = 500;
		private const int DIALOG_SIZE_H = 490;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END
		// 各項目間のスペース
		private const int DIALOG_SPACE_H1 = 20;
		// 同一項目間のスペース
		private const int DIALOG_SPACE_H2 = 10;
		// 平面直角座標系
		private static readonly string[] SYS_DROPDOWN = CommonBL.SYS_NAME_LIST;

		//----------------------------------------------
		// メンバ変数
		//----------------------------------------------
		private UITextureAtlas _ingameAtlas;
		private UIDragHandle _dragHandle;
		private UILabel _lblTitle;
		private UIButton _btnClose;
		private UILabel _lblCenterTitle;
		private UITextField _txtCenter;
		private UIButton _btnOk;
		private UIButton _btnCansel;
		private UIPanel _innerTxtPanel;
		private UILabel _lblSystemTitle;
		private UIPanel _innerDropPanel;
		private UIDropDown _dropSystem;
		private bool _initialized = false;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
		private UILabel _lblFitureTitle;
		private UICheckBox _chkImpMap;
		private UICheckBox _chkImpHighway;
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

// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
				//----------------------------------
				// 地物選択の項目
				//----------------------------------
				// 項目名
				_lblFitureTitle = UIUtils.CreateLabel(this, "_lblFitureTitle", DIALOG_MSG_IMPTYPE);
				_lblFitureTitle.width = DIALOG_SIZE_W - 40f;
				_lblFitureTitle.relativePosition = new Vector3(posX, posY);
				posX += 20;
				posY += _lblFitureTitle.height + DIALOG_SPACE_H2;
				// 地形
				_chkImpMap = createCheckBox(posX, posY, "_chkImpMap", DIALOG_MSG_IMPTYPE_1, IniFileData.Instance.isImpMap);
				_chkImpMap.eventCheckChanged += delegate (UIComponent component, bool value) { IniFileData.Instance.isImpMap = value; };
				posY += _chkImpMap.height + DIALOG_SPACE_H2;
				// 高速道路
				_chkImpHighway = createCheckBox(posX, posY, "_chkImpHighway", DIALOG_MSG_IMPTYPE_2, IniFileData.Instance.isImpHighway);
				_chkImpHighway.eventCheckChanged += delegate (UIComponent component, bool value) { IniFileData.Instance.isImpHighway = value; };
				posY += _chkImpHighway.height + DIALOG_SPACE_H2;
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
							Debug.Log("OKボタン：" + _folderDialog.m_FolderPath);
							IniFileData.Instance.inputFolderPath = _folderDialog.m_FolderPath;
							_txtFolder.text = _folderDialog.m_FolderPath;
						};
						_folderDialog.onCancelButtonCallback = () => {
						};

						_folderDialog.m_FolderPath = IniFileData.Instance.inputFolderPath;
						Debug.Log("ダイアログ表示１：" + _folderDialog.m_FolderPath);
						_folderDialog.drawDialog(this);
					}
					else
					{
						_folderDialog.m_FolderPath = IniFileData.Instance.inputFolderPath;
						Debug.Log("ダイアログ表示２：" + _folderDialog.m_FolderPath);
						_folderDialog.Visible = true;
					}
				};
				posY += _txtFolder.height + DIALOG_SPACE_H1;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END

				//----------------------------------
				// 中心位置指定の項目
				//----------------------------------
				// 項目名
				_lblCenterTitle = UIUtils.CreateLabel(this, "_lblCenterTitle1", DIALOG_MSG_CENTER);
				_lblCenterTitle.width = DIALOG_SIZE_W - 40f;
				_lblCenterTitle.relativePosition = new Vector3(posX, posY);
				posY += _lblCenterTitle.height + DIALOG_SPACE_H2;
				// テキストフィールド
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
//				_txtCenter = UIUtils.CreateTextField(this, "_txtCenter1", _ingameAtlas, ImportSettingData.Instance.center);
				_txtCenter = UIUtils.CreateTextField(this, "_txtCenter1", _ingameAtlas, IniFileData.Instance.center);
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END
				_txtCenter.width = _txtCenter.parent.width - 60f;
				_txtCenter.relativePosition = new Vector3(posX + 20f, posY);
				_txtCenter.eventTextSubmitted += delegate (UIComponent component, string value)
				{
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
//					ImportSettingData.Instance.center = value;
					IniFileData.Instance.center = value;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END
				};
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
//				posY += _txtCenter.height + DIALOG_SPACE_H1;
				posY += _txtCenter.height + DIALOG_SPACE_H2;
				// 補足メッセージ
				_lblCenterMsg = UIUtils.CreateLabel(this, "_lblCenterMsg", DIALOG_MSG_CENTER_HOSOKU);
				_lblCenterMsg.width = DIALOG_SIZE_W - 40f;
				_lblCenterMsg.relativePosition = new Vector3(posX + 20f, posY);
				posY += _lblCenterMsg.height + DIALOG_SPACE_H1;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END


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
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
//				_dropSystem.selectedIndex = ImportSettingData.Instance.isystem;
				_dropSystem.selectedIndex = IniFileData.Instance.isystem;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END
				_dropSystem.eventSelectedIndexChanged += delegate (UIComponent component, int value)
				{
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
//					ImportSettingData.Instance.isystem = value;
					IniFileData.Instance.isystem = value;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END
				};
				posY += _dropSystem.height + DIALOG_SPACE_H1;

				//----------------------------------
				// OKボタン
				//----------------------------------
				_btnOk = UIUtils.CreateSpriteButton(this, "_btnOk1", _ingameAtlas, "ButtonMenu");
				_btnOk.text = DIALOG_MSG_BTN_OK;
				_btnOk.height = 30f;
				_btnOk.width = _btnOk.parent.width / 2f - 20f;
				_btnOk.relativePosition = new Vector3(10f, posY);
				_btnOk.eventClicked += delegate (UIComponent component, UIMouseEventParameter eventParam)
				{
					if (!eventParam.used)
					{
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
						if (!IniFileData.Instance.isImpMap &&
							!IniFileData.Instance.isImpHighway)
						{
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("入力値が不正", "インポート対象を選択してください。", false);
							return;
						}
						if (IniFileData.Instance.inputFolderPath.Length == 0)
						{
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("入力値が不正", "3D都市モデルフォルダを指定してください。", false);
							return;
						}
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END

// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
//						if (ImportSettingData.Instance.center.Length == 0)
						if (IniFileData.Instance.center.Length == 0)
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END
						{
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("入力値が不正", "中心位置を指定してください。", false);
							return;
						}
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
//						string[] splitStr = ImportSettingData.Instance.center.Split(',');
						string[] splitStr = IniFileData.Instance.center.Split(',');
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END
						if (splitStr.Length != 2)
						{
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("入力値が不正", "中心位置は半角カンマ区切りで指定してください。", false);
							return;
						}

						//----------------------------------
						// 設定項目を保存
						//----------------------------------
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
//						ImportSettingData.Instance.Save();
						IniFileData.Instance.Save();
						SettingsUI.UpdateOptionSetting();
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END

						//----------------------------
						// 画面を閉じる
						//----------------------------
						this.Hide();
						eventParam.Use();
						_initialized = false;

// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
#if false
						//----------------------------------
						// インポート処理
						//----------------------------------
// 2022.11.22 G.Arakawa@cmind [平面直角座標系が９系以外、正常に動作していない不具合対応] ADD_START
						// 座標系の設定
						CommonBL.Instance.iSystem = ImportSettingData.Instance.isystem + 1;
// 2022.11.22 G.Arakawa@cmind [平面直角座標系が９系以外、正常に動作していない不具合対応] ADD_END
						// 読み込み範囲の設定
						MapExtent.Instance.SetMapExtent(MapExtent.AREA_SIZE_9);
						int impNum = GmlDemData.Import();

						//----------------------------
						// 終了処理
						//----------------------------
						// ダイアログ表示
						if (impNum == 0)
						{
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("インポート対象なし", "範囲内にデータはありませんでした。", false);
						}
						else
						{

							//-------------------------------------
							// 生成範囲をテキスト出力
							//-------------------------------------
							if (!Directory.Exists(MAPAREA_PATH))
							{
								// フォルダを生成
								Directory.CreateDirectory(MAPAREA_PATH);
							}
							// 現在日時
							DateTime dt = DateTime.Now;
							String dateStr = dt.ToString($"{dt:yyyyMMddHHmmss}");
							// ファイル出力
							String name = MAPAREA_PATH + "/" + MAPAREA_NAME + "_" + dateStr + ".txt";
							FileStream fileStream = File.Create(name);
							StreamWriter sw = new StreamWriter(fileStream);

							int isystem = ImportSettingData.Instance.isystem + 1;
							double minX = MapExtent.Instance.centerY - MapExtent.MAX_AREA_SIZE / 2; // 内部では描画範囲を回転させるため、ＸＹを逆に保持 
							double minY = MapExtent.Instance.centerX - MapExtent.MAX_AREA_SIZE / 2; //
							double maxX = MapExtent.Instance.centerY + MapExtent.MAX_AREA_SIZE / 2; //
							double maxY = MapExtent.Instance.centerX + MapExtent.MAX_AREA_SIZE / 2; //
							sw.WriteLine("■中心座標");
							sw.WriteLine(" " + ImportSettingData.Instance.center);
							sw.WriteLine("■緯度経度の範囲");
							double lat, lon;
							// 最小最小 (x,yは反転させているので、指定変数も逆転させている)
							CommonBL.Instance.ido_keido(minY, minX, out lat, out lon);
							sw.WriteLine(" " + lat + "," + lon);
							// 最小最大 (x,yは反転させているので、指定変数も逆転させている)
							CommonBL.Instance.ido_keido(minY, maxX, out lat, out lon);
							sw.WriteLine(" " + lat + "," + lon);
							// 最大最大 (x,yは反転させているので、指定変数も逆転させている)
							CommonBL.Instance.ido_keido(maxY, maxX, out lat, out lon);
							sw.WriteLine(" " + lat + "," + lon);
							// 最大最小 (x,yは反転させているので、指定変数も逆転させている)
							CommonBL.Instance.ido_keido(maxY, minX, out lat, out lon);
							sw.WriteLine(" " + lat + "," + lon);

							sw.WriteLine("■平面直角座標 " + isystem + "系 の範囲");
							sw.WriteLine(" " + maxX + "," + maxY);
							sw.WriteLine(" " + minX + "," + minY);
							sw.Close();
							fileStream.Close();



							//-------------------------------------
							// ダイアログ表示
							//-------------------------------------
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
								panel.SetMessage("インポート完了", "ＤＥＭの取込完了", false);
						}
#endif
						//----------------------------------
						// インポート処理
						//----------------------------------
						// 座標系の設定
						CommonBL.Instance.iSystem = IniFileData.Instance.isystem + 1;
						// 読み込み範囲の設定
						MapExtent.Instance.SetMapExtent(MapExtent.AREA_SIZE_9);

						int impAllNum = 0;
						int impNum = 0;
						string msg = "";
						// 地形
						if (IniFileData.Instance.isImpMap)
						{
							impNum = GmlDemData.Import();
							msg += ("ＤＥＭの取込完了\n");
							impAllNum += impNum;
						}
						// 高速道路
						if (IniFileData.Instance.isImpHighway)
						{
							impNum = GmlRoadData.ImportHighway();
							msg += ("高速道路： " + impNum + "件\n");
							impAllNum += impNum;
						}

						//----------------------------
						// 終了処理
						//----------------------------
						// ダイアログ表示
						if (impAllNum == 0)
						{
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("インポート対象なし", "範囲内にデータはありませんでした。", false);
						}
						else
						{
							if (IniFileData.Instance.isImpMap)
							{
								//-------------------------------------
								// 生成範囲をテキスト出力
								//-------------------------------------
								if (!Directory.Exists(MAPAREA_PATH))
								{
									// フォルダを生成
									Directory.CreateDirectory(MAPAREA_PATH);
								}
								// 現在日時
								DateTime dt = DateTime.Now;
								String dateStr = dt.ToString($"{dt:yyyyMMddHHmmss}");
								// ファイル出力
								String name = MAPAREA_PATH + "/" + MAPAREA_NAME + "_" + dateStr + ".txt";
								FileStream fileStream = File.Create(name);
								StreamWriter sw = new StreamWriter(fileStream);

								int isystem = IniFileData.Instance.isystem + 1;
								double minX = MapExtent.Instance.centerY - MapExtent.MAX_AREA_SIZE / 2; // 内部では描画範囲を回転させるため、ＸＹを逆に保持 
								double minY = MapExtent.Instance.centerX - MapExtent.MAX_AREA_SIZE / 2; //
								double maxX = MapExtent.Instance.centerY + MapExtent.MAX_AREA_SIZE / 2; //
								double maxY = MapExtent.Instance.centerX + MapExtent.MAX_AREA_SIZE / 2; //
								sw.WriteLine("■中心座標");
								sw.WriteLine(" " + IniFileData.Instance.center);
								sw.WriteLine("■緯度経度の範囲");
								double lat, lon;
								// 最小最小 (x,yは反転させているので、指定変数も逆転させている)
								CommonBL.Instance.ido_keido(minY, minX, out lat, out lon);
								sw.WriteLine(" " + lat + "," + lon);
								// 最小最大 (x,yは反転させているので、指定変数も逆転させている)
								CommonBL.Instance.ido_keido(minY, maxX, out lat, out lon);
								sw.WriteLine(" " + lat + "," + lon);
								// 最大最大 (x,yは反転させているので、指定変数も逆転させている)
								CommonBL.Instance.ido_keido(maxY, maxX, out lat, out lon);
								sw.WriteLine(" " + lat + "," + lon);
								// 最大最小 (x,yは反転させているので、指定変数も逆転させている)
								CommonBL.Instance.ido_keido(maxY, minX, out lat, out lon);
								sw.WriteLine(" " + lat + "," + lon);

								sw.WriteLine("■平面直角座標 " + isystem + "系 の範囲");
								sw.WriteLine(" " + maxX + "," + maxY);
								sw.WriteLine(" " + minX + "," + minY);
								sw.Close();
								fileStream.Close();
							}

							//-------------------------------------
							// ダイアログ表示
							//-------------------------------------
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("インポート完了", msg, false);
						}
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END
					}
				};
				//----------------------------------
				// Cancelボタン
				//----------------------------------
				_btnCansel = UIUtils.CreateSpriteButton(this, "_btnCansel1", _ingameAtlas, "ButtonMenu");
				_btnCansel.text = DIALOG_MSG_BTN_CANSEL;
				_btnCansel.height = 30f;
				_btnCansel.width = _btnOk.width;
				_btnCansel.relativePosition = new Vector3(10f + _btnOk.width + 10f, posY);
				_btnCansel.eventClicked += delegate (UIComponent component, UIMouseEventParameter eventParam)
				{
					if (!eventParam.used)
					{
						this.Hide();
						eventParam.Use();
						_initialized = false;
					}
				};
			}
			catch (Exception ex)
			{
				Debug.Log((object)("[SkylinesCityGml] ImpMapPanel:Start -> Exception: " + ex.Message));
			}

		}

		// 表示状態にする
		public void drawDialog()
		{
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
//			// 表示内容を最新化
//			_txtCenter.text = ImportSettingData.Instance.center;
//			_dropSystem.selectedIndex = ImportSettingData.Instance.isystem;
			// 表示内容を最新化
			_chkImpMap.isChecked = IniFileData.Instance.isImpMap;
			_chkImpHighway.isChecked = IniFileData.Instance.isImpHighway;
			_txtCenter.text = IniFileData.Instance.center;
			_dropSystem.selectedIndex = IniFileData.Instance.isystem;
			_chkImpMap.isChecked = IniFileData.Instance.isImpMap;
			_chkImpHighway.isChecked = IniFileData.Instance.isImpHighway;
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
//					_txtCenter.text = ImportSettingData.Instance.center;
//					_dropSystem.selectedIndex = ImportSettingData.Instance.isystem;
					_chkImpMap.isChecked = IniFileData.Instance.isImpMap;
					_chkImpHighway.isChecked = IniFileData.Instance.isImpHighway;
					_txtCenter.text = IniFileData.Instance.center;
					_dropSystem.selectedIndex = IniFileData.Instance.isystem;
					_chkImpMap.isChecked = IniFileData.Instance.isImpMap;
					_chkImpHighway.isChecked = IniFileData.Instance.isImpHighway;
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
		/*
		public override void OnDestroy()
		{
			base.OnDestroy();
			try
			{
				DestroyGameObject(_dragHandle);
				DestroyGameObject(_lblCenterTitle);
				DestroyGameObject(_lblTitle);
				DestroyGameObject(_btnOk);
				DestroyGameObject(_btnCansel);
				DestroyGameObject(_btnClose);
				DestroyGameObject(_innerTxtPanel);
				DestroyGameObject(_lblSystemTitle);
				DestroyGameObject(_dropSystem);
			}
			catch (Exception ex)
			{
				Debug.Log((object)("[SkylinesCityGml] ImpMapPanel:OnDestroy -> Exception: " + ex.Message));
			}
		}
		*/

		private void DestroyGameObject(UIComponent component)
		{
			if (component != null)
			{
                UnityEngine.Object.Destroy(component.gameObject);
			}
		}

// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
		private UICheckBox createCheckBox(float posX, float posY, string itemName, string title, bool isUse)
		{
			UICheckBox chkBox;
			chkBox = UIUtils.CreateCheckBox(this, itemName, _ingameAtlas, title, state: isUse);
			chkBox.width = chkBox.parent.width - posX * 2;
			chkBox.relativePosition = new Vector3(posX, posY);
			return chkBox;
		}
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END
	}
}
