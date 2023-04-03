using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using SkylinesPlateau.common;
using UIUtils = SkylinesPlateau.common.UIUtils;

namespace SkylinesPlateau
{
    class ImpHighwayPanel : UIPanel
	{
		//----------------------------------------------
		// 固定値
		//----------------------------------------------
		// ダイアログの各種名称
		private const string DIALOG_TITLE = "高速道路読込";
		private const string DIALOG_MSG_BTN_OK = "インポート";
		private const string DIALOG_MSG_BTN_CANSEL = "キャンセル";
		private const string DIALOG_MSG_CENTER = "中心座標";
		private const string DIALOG_MSG_SYS = "座標系番号";
		// ダイアログサイズ
		private const int DIALOG_SIZE_W = 350;
		private const int DIALOG_SIZE_H = 280;
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
		private UIDropDown _dropSystem;
		private bool _initialized = false;

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

				//----------------------------------
				// 中心位置指定の項目
				//----------------------------------
				// 項目名
				_lblCenterTitle = UIUtils.CreateLabel(this, "_lblCenterTitle2", DIALOG_MSG_CENTER);
				_lblCenterTitle.width = DIALOG_SIZE_W - 40f;
				_lblCenterTitle.relativePosition = new Vector3(posX, posY);
				posY += _lblCenterTitle.height + DIALOG_SPACE_H2;

				Logger.Log("中心位置：" + ImportSettingData.Instance.center);

				// テキストフィールド
				_txtCenter = UIUtils.CreateTextField(this, "_txtCenter2", _ingameAtlas, ImportSettingData.Instance.center);
				_txtCenter.text = ImportSettingData.Instance.center;
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
				// OKボタン
				//----------------------------------
				_btnOk = UIUtils.CreateSpriteButton(this, "_btnOk2", _ingameAtlas, "ButtonMenu");
				_btnOk.text = DIALOG_MSG_BTN_OK;
				_btnOk.height = 30f;
				_btnOk.width = _btnOk.parent.width / 2f - 20f;
				_btnOk.relativePosition = new Vector3(10f, posY);
				_btnOk.eventClicked += delegate (UIComponent component, UIMouseEventParameter eventParam)
				{
					if (!eventParam.used)
					{
						if (ImportSettingData.Instance.center.Length == 0)
						{
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("入力値が不正", "中心位置を指定してください。", false);
							return;
						}
						string[] splitStr = ImportSettingData.Instance.center.Split(',');
						if (splitStr.Length != 2)
						{
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("入力値が不正", "中心位置は半角カンマ区切りで指定してください。", false);
							return;
						}

						//----------------------------
						// 設定項目を保存
						//----------------------------
						ImportSettingData.Instance.Save();

						//----------------------------
						// 画面を閉じる
						//----------------------------
						this.Hide();
						eventParam.Use();
						_initialized = false;

						//----------------------------
						// インポート処理
						//----------------------------
						// 読み込み範囲の設定
						MapExtent.Instance.SetMapExtent(MapExtent.AREA_SIZE_9);
						// 高速道路のインポート処理
						int impNum = GmlRoadData.ImportHighway();

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
							ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
							panel.SetMessage("インポート完了", "高速道路： " + impNum + "件\n", false);
						}
					}
				};
				//----------------------------------
				// Cancelボタン
				//----------------------------------
				_btnCansel = UIUtils.CreateSpriteButton(this, "_btnCansel2", _ingameAtlas, "ButtonMenu");
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
				Debug.Log((object)("[SkylinesCityGml] ImpHighwayPanel:Start -> Exception: " + ex.Message));
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
				Debug.Log((object)("[SkylinesCityGml] ImpHighwayPanel:OnDestroy -> Exception: " + ex.Message));
			}
		}
		*/

		// 表示状態にする
		public void drawDialog()
		{
			// 表示内容を最新化
			_txtCenter.text = ImportSettingData.Instance.center;
			_dropSystem.selectedIndex = ImportSettingData.Instance.isystem;
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
					_txtCenter.text = ImportSettingData.Instance.center;
					_dropSystem.selectedIndex = ImportSettingData.Instance.isystem;
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
	}
}
