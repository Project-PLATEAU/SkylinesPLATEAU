using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
using System.Reflection;
using UnityEngine;

using UIUtils = SkylinesPlateau.common.UIUtils;
using ColossalFramework.PlatformServices;

namespace SkylinesPlateau
{
	public static class SettingsUI
	{
		private const string FILE_NAME = "SkylinesPLATEAU";
		private const string DIALOG_TITLE = "地形、高速道路読込";
		private const string DIALOG_MSG_FOLDER = "3D都市モデルフォルダ";
		private const string DIALOG_MSG_CENTER = "中心座標";
		private const string DIALOG_MSG_SYS = "座標系系番号";
		private const string DIALOG_MSG_DEM_SEALEVEL = "地盤レベル";
		private const string DIALOG_MSG_DEM_SEALEVEL_HOSOKU = "地形読み込み時に基準となるゲーム上の高さ";
		private const string DIALOG_MSG_WATER_DOWNHEIGHT = "水面オフセット";
		private const string DIALOG_MSG_WATER_DOWNHEIGHT_HOSOKU = "地形と水部のレベル差。設定値分、水部を下げる";
		private const string DIALOG_MSG_DEM_FILTER = "TIN読込最大面積";
		private const string DIALOG_MSG_DEM_FILTER_HOSOKU = "最大面積を超える三角形ポリゴン（TIN）は標高0mとする";
		private const string DIALOG_MSG_AREASIZE = "読み込み範囲";
		private const string DIALOG_MSG_AREASIZE_HOSOKU = "地物読込を行う中心からの距離";
		private const string DIALOG_MSG_ROAD_FILTER = "三角道路ポリゴン除外面積";
		private const string DIALOG_MSG_ROAD_FILTER_HOSOKU = "狭小な道路ポリゴンを除外するための閾値";
		private const string DIALOG_MSG_SUPPORT = "サポート";
		private const string DIALOG_MSG_SUPPORT_BTN = "SkylinesPLATEAUのマニュアルを開く";

		private static UIFolderDialog _folderDialog = null;
		private static UITextField _folderPathField;
		private static UITextField _centerField;
		private static UIDropDown _systemDropdown;
		private static UITextField _demSeaLevelField;
		private static UITextField _waterDownHeightField;
		private static UITextField _demFilterField;
		private static UITextField _areaSizeField;
		private static UITextField _roadFilterField;

		static SettingsUI()
		{
			if (GameSettings.FindSettingsFileByName(FILE_NAME) == null)
			{
				GameSettings.AddSettingsFile(new SettingsFile
				{
					fileName = FILE_NAME
				});
			}
		}

		public static UIPanel Panel(this UIHelperBase helper)
		{
			return (helper as UIHelper).self as UIPanel;
		}

		public static void UpdateOptionSetting()
		{
			if (_folderPathField != null)
			{
				_folderPathField.text = IniFileData.Instance.inputFolderPath;
			}
			if (_centerField != null)
			{
				_centerField.text = IniFileData.Instance.center;
			}
			if (_systemDropdown != null)
			{
				_systemDropdown.selectedIndex = IniFileData.Instance.isystem;
			}
			if (_demSeaLevelField != null)
			{
				_demSeaLevelField.text = IniFileData.Instance.demSeaLevel.ToString("F1");
			}
			if (_waterDownHeightField != null)
			{
				_waterDownHeightField.text = IniFileData.Instance.demWaterAreaDownHeight.ToString("F1");
			}
			if (_demFilterField != null)
			{
				_demFilterField.text = IniFileData.Instance.demFilterAreaSize.ToString("F0");
			}
			if (_areaSizeField != null)
			{
				_areaSizeField.text = IniFileData.Instance.areaSize;
			}
			if (_roadFilterField != null)
			{
				_roadFilterField.text = IniFileData.Instance.roadFilterAreaSize.ToString("F0");
			}
		}

		public static void OnSettingsUI(UIHelper helper)
		{
			_folderPathField = AddTextfield2(helper, DIALOG_MSG_FOLDER, "", IniFileData.Instance.inputFolderPath, "", true,
				(value) => { },
				(value) => { 
					IniFileData.Instance.inputFolderPath = value;
					IniFileData.Instance.Save();
				},
//-------------------------------------------------------------------------------------------------------------------
				() => {
					try
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
								_folderPathField.text = _folderDialog.m_FolderPath;
								IniFileData.Instance.Save();
							};
							_folderDialog.onCancelButtonCallback = () => {
							};
							_folderDialog.m_FolderPath = IniFileData.Instance.inputFolderPath;
							_folderDialog.drawDialog(((UIComponent)helper.self).parent);
						}
						else
						{
							_folderDialog.m_FolderPath = IniFileData.Instance.inputFolderPath;
							_folderDialog.Visible = true;
						}
					}
					catch (Exception ex)
					{
						Debug.Log("Exception Error : " + ex.Message);
						Debug.Log("Exception Tracer: " + ex.StackTrace);
					}
				});
			//-------------------------------------------------------------------------------------------------------------------

			_centerField = AddTextfield2(helper, DIALOG_MSG_CENTER, "", IniFileData.Instance.center, "", false,
				(value) => { },
				(value) => { 
					IniFileData.Instance.center = value;
					IniFileData.Instance.Save();
				},
				null);

			_systemDropdown = AddDropdown2(helper, DIALOG_MSG_SYS, CommonBL.SYS_NAME_LIST, IniFileData.Instance.isystem,
				(index) => {
					IniFileData.Instance.isystem = index;
					IniFileData.Instance.Save();
				});

			_demSeaLevelField = AddTextfield2(helper, DIALOG_MSG_DEM_SEALEVEL, DIALOG_MSG_DEM_SEALEVEL_HOSOKU, IniFileData.Instance.demSeaLevel.ToString("F1"), "(m)", false,
				(value) => { },
				(value) => { 
					IniFileData.Instance.demSeaLevel = double.Parse(value);
					IniFileData.Instance.Save();
				},
				null);

			_waterDownHeightField = AddTextfield2(helper, DIALOG_MSG_WATER_DOWNHEIGHT, DIALOG_MSG_WATER_DOWNHEIGHT_HOSOKU, IniFileData.Instance.demWaterAreaDownHeight.ToString("F1"), "(m)", false,
				(value) => { },
				(value) => { 
					IniFileData.Instance.demWaterAreaDownHeight = double.Parse(value);
					IniFileData.Instance.Save();
				},
				null);

			_demFilterField = AddTextfield2(helper, DIALOG_MSG_DEM_FILTER, DIALOG_MSG_DEM_FILTER_HOSOKU, IniFileData.Instance.demFilterAreaSize.ToString("F0"), "(㎡)", false,
				(value) => { },
				(value) => { 
					IniFileData.Instance.demFilterAreaSize = double.Parse(value);
					IniFileData.Instance.Save();
				},
				null);

			_areaSizeField = AddTextfield2(helper, DIALOG_MSG_AREASIZE, DIALOG_MSG_AREASIZE_HOSOKU, IniFileData.Instance.areaSize, "(km)", false,
				(value) => { },
				(value) => { 
					IniFileData.Instance.areaSize = value;
					IniFileData.Instance.Save();
				},
				null);

			_roadFilterField = AddTextfield2(helper, DIALOG_MSG_ROAD_FILTER, DIALOG_MSG_ROAD_FILTER_HOSOKU, IniFileData.Instance.roadFilterAreaSize.ToString("F0"), "(㎡)", false,
				(value) => { },
				(value) => { 
					IniFileData.Instance.roadFilterAreaSize = double.Parse(value);
					IniFileData.Instance.Save();
				},
				null);

			// ヘルプの指定がある場合のみ表示
			if (!string.IsNullOrEmpty(IniFileData.Instance.helpUrl))
			{
				// スペース配置
				helper.AddSpace(50);

				// ボタン配置
				AddButton2(helper, DIALOG_MSG_SUPPORT, DIALOG_MSG_SUPPORT_BTN,
					() => {
						try
						{
							if (PlatformService.IsOverlayEnabled())
							{
								if (PlatformService.apiBackend == APIBackend.Steam)
								{
									PlatformService.ActivateGameOverlayToWebPage(IniFileData.Instance.helpUrl);
								}
								else
								{
									PlatformService.ActivateGameOverlayToStore(346791u, OverlayToStoreFlag.None);
								}
							}
						}
						catch (Exception ex)
						{
							Debug.Log("Exception Error : " + ex.Message);
							Debug.Log("Exception Tracer: " + ex.StackTrace);
						}
					});
			}

			// バージョン表示
			try
			{
				// バージョン取得
				System.Reflection.Assembly assembly = Assembly.GetExecutingAssembly();
				System.Reflection.AssemblyName asmName = assembly.GetName();
				System.Version version = asmName.Version;
				// バージョン表示
				AddLabel2(helper, "v" + version.ToString());
			}
			catch (Exception ex)
			{
				Debug.Log("[2023] バージョン取得に失敗");
				Debug.Log("Exception Error : " + ex.Message);
				Debug.Log("Exception Tracer: " + ex.StackTrace);
				Debug.Log("Exception : " + ex);
			}
		}


		private static UITextField AddTextfield2(UIHelper inHelper, string inTitle, string inSubTitle, string inText, string inUnitText, bool inDrawBtn, OnTextChanged eventChangedCallback, OnTextSubmitted eventSubmittedCallback, OnButtonClicked eventCallback)
		{
			UITextField rtnTextField = null;
			float lbl_h = 20f;
			float txt_h = 30f;
			float btn_w = 30f;
			float btn_h = 22f;

			float areaW = ((UIComponent)inHelper.self).width;

			float offset_x = 0f;
			float offset_y = 0f;
			float space_w = 30f;
			float space_h = 1f;

			UILabel baseLabel = UIUtils.GetOptionTemplate_TextLabel();
			UITextField baseTextField = UIUtils.GetOptionTemplate_TextField();

			//--------------------------------------------
			// 背景用のパネル
			//--------------------------------------------
			UIPanel panel_bg = ((UIComponent)inHelper.self).AddUIComponent<UIPanel>();
			panel_bg.width = areaW;
			panel_bg.height = 70;
			{
				//--------------------------------------------
				// 項目名ラベル
				//--------------------------------------------
				UILabel txtLabel = UIUtils.CreateLabel(panel_bg, "_txtLabel", inTitle, baseLabel);
				txtLabel.height = lbl_h;
				txtLabel.relativePosition = new Vector3(offset_x, offset_y);
				txtLabel.verticalAlignment = UIVerticalAlignment.Bottom;

				//--------------------------------------------
				// 項目名ラベル(SUB)
				//--------------------------------------------
				if (!string.IsNullOrEmpty(inSubTitle))
				{
					float bkSize = txtLabel.width + space_w;
					offset_x += bkSize;
					UILabel txtSubLabel = UIUtils.CreateLabel(panel_bg, "_txtSubLabel", inSubTitle, baseLabel);
					txtSubLabel.relativePosition = new Vector3(offset_x, offset_y + 2f);
					txtSubLabel.verticalAlignment = UIVerticalAlignment.Bottom;
					txtSubLabel.textScale = 0.75f;
					txtSubLabel.height = lbl_h;
					offset_x -= bkSize;
				}

				offset_y += (txtLabel.height + space_h);

				//--------------------------------------------
				// テキストフィールド
				//--------------------------------------------
				rtnTextField = UIUtils.CreateTextField(panel_bg, "_txtField", inText, baseTextField);
				rtnTextField.width = (areaW / 4) * 3;
				rtnTextField.height = txt_h;
				rtnTextField.relativePosition = new Vector3(offset_x, offset_y);
				//				txtField.color = Color.white;

				rtnTextField.eventTextChanged += delegate (UIComponent c, string sel)
				{
					eventChangedCallback(sel);
				};
				rtnTextField.eventTextSubmitted += delegate (UIComponent c, string sel)
				{
					if (eventSubmittedCallback != null)
					{
						eventSubmittedCallback(sel);
					}
				};

				//--------------------------------------------
				// ボタン
				//--------------------------------------------
				if (inDrawBtn)
				{
					try
					{
						UIButton folderButton = UIUtils.Instance.CreateImageButton(rtnTextField, "FolderIconButton", "folderIcon");
						folderButton.tooltip = "";
						folderButton.height = btn_h;
						folderButton.width = btn_w;
						folderButton.relativePosition = new Vector3(rtnTextField.width - (btn_w + 5.0f), (txt_h - btn_h) / 2.0f);
						folderButton.eventClicked += delegate (UIComponent component, UIMouseEventParameter eventParam)
						{
							eventCallback();
						};

						rtnTextField.padding.right = (int)Math.Ceiling(btn_w) + 5;
					}
					catch (Exception ex)
					{
						Debug.Log("Exception Error : " + ex.Message);
						Debug.Log("Exception Tracer: " + ex.StackTrace);
					}
				}

				//--------------------------------------------
				// 単位
				//--------------------------------------------
				if (!string.IsNullOrEmpty(inUnitText))
				{
					float bkSize = rtnTextField.width + 10f;
					offset_x += bkSize;
					UILabel unitLabel = UIUtils.CreateLabel(panel_bg, "_unitLabel", inUnitText, baseLabel);
					unitLabel.relativePosition = new Vector3(offset_x, offset_y);
					unitLabel.verticalAlignment = UIVerticalAlignment.Bottom;
					unitLabel.textScale = 1.3f;
					unitLabel.height = txt_h;
					offset_x -= bkSize;
				}

				offset_y += (rtnTextField.height);
			}
			panel_bg.height = offset_y + 10f;

			return rtnTextField;
		}

		private static UIDropDown AddDropdown2(UIHelper inHelper, string text, string[] options, int defaultSelection, OnDropdownSelectionChanged eventCallback)
		{
			if (eventCallback != null && !string.IsNullOrEmpty(text))
			{
				float areaW = ((UIComponent)inHelper.self).width;
				UIPanel uIPanel = ((UIComponent)inHelper.self).AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsDropdownTemplate")) as UIPanel;
				uIPanel.Find<UILabel>("Label").text = text;
				UIDropDown uIDropDown = uIPanel.Find<UIDropDown>("Dropdown");
				uIDropDown.items = options;
				uIDropDown.selectedIndex = defaultSelection;
				uIDropDown.width = (areaW / 4) * 3;
				uIDropDown.eventSelectedIndexChanged += delegate (UIComponent c, int sel)
				{
					eventCallback(sel);
				};
				return uIDropDown;
			}
			return null;
		}

		public static void AddButton2(UIHelper inHelper, string inTitle, string inBtnText, OnButtonClicked eventCallback)
		{
			float lbl_h = 20f;
			float btn_w = 400f;
			float btn_h = 40f;

			float areaW = ((UIComponent)inHelper.self).width;
			float offset_x = 0f;
			float offset_y = 0f;
			float space_h = 1f;

			UILabel baseLabel = UIUtils.GetOptionTemplate_TextLabel();
			GameObject txtTmplate = UnityEngine.Object.Instantiate(UITemplateManager.GetAsGameObject("OptionsButtonTemplate"));
			UIButton baseButton = txtTmplate.GetComponent<UIComponent>() as UIButton;

			//--------------------------------------------
			// 背景用のパネル
			//--------------------------------------------
			UIPanel panel_bg = ((UIComponent)inHelper.self).AddUIComponent<UIPanel>();
			panel_bg.width = areaW;
			panel_bg.height = 70;
			{
				//--------------------------------------------
				// 項目名ラベル
				//--------------------------------------------
				UILabel txtLabel = UIUtils.CreateLabel(panel_bg, "_txtLabel", inTitle, baseLabel);
				txtLabel.height = lbl_h;
				txtLabel.relativePosition = new Vector3(offset_x, offset_y);
				txtLabel.verticalAlignment = UIVerticalAlignment.Bottom;
				offset_y += (txtLabel.height + space_h);

				//--------------------------------------------
				// ボタン配置
				//--------------------------------------------
				UIButton txtButton = UIUtils.CreateButton(panel_bg, "_txtButton", baseButton.atlas, baseButton.normalFgSprite, baseButton);
				txtButton.width = btn_w;
				txtButton.height = btn_h;
				txtButton.relativePosition = new Vector3(offset_x + 10f, offset_y);
				txtButton.text = inBtnText;
				txtButton.eventClick += delegate
				{
					eventCallback();
				};
				offset_y += (txtButton.height);
			}
			panel_bg.height = offset_y + 10f;
		}

		public static void AddLabel2(UIHelper inHelper, string inTitle)
		{
			//--------------------------------------------
			// 項目名ラベル
			//--------------------------------------------
			UILabel baseLabel = UIUtils.GetOptionTemplate_TextLabel();
			UILabel txtLabel = UIUtils.CreateLabel((UIComponent)inHelper.self, "_txtLabel", inTitle, baseLabel);
			txtLabel.autoSize = false;
			txtLabel.width = ((UIComponent)inHelper.self).width-50;
			txtLabel.height = 20f;
			txtLabel.relativePosition = new Vector3(0, 0);
			txtLabel.verticalAlignment = UIVerticalAlignment.Bottom;
			txtLabel.textAlignment = UIHorizontalAlignment.Right;
		}
	}
}
