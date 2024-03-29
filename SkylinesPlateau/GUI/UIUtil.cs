using ColossalFramework.UI;
using System;
using System.Collections.Generic;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
using System.IO;
using System.Reflection;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END
using System.Linq;
using System.Text;
using UnityEngine;

namespace SkylinesPlateau.common
{
	public class UIUtils
	{
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
        //-------------------------------------
        // メンバ変数
        //-------------------------------------
		private UITextureAtlas m_citiesPlateauAtlas;
		private static UIUtils instance;
		public static UIUtils Instance => instance ?? (instance = new UIUtils());
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END

		public static UIFont GetUIFont(string name)
		{
			UIFont[] array = Resources.FindObjectsOfTypeAll<UIFont>();
			UIFont[] array2 = array;
			foreach (UIFont uIFont in array2)
			{
				if (((UnityEngine.Object)uIFont).name.CompareTo(name) == 0)
				{
					return uIFont;
				}
			}
			return null;
		}

		public static UIPanel CreatePanel(UIComponent parent, string name)
		{
			UIPanel uIPanel = parent.AddUIComponent<UIPanel>();
			((UnityEngine.Object)uIPanel).name = name;
			return uIPanel;
		}

		public static UIScrollablePanel CreateScrollablePanel(UIComponent parent, string name)
		{
			UIScrollablePanel uIScrollablePanel = parent.AddUIComponent<UIScrollablePanel>();
			((UnityEngine.Object)uIScrollablePanel).name = name;
			return uIScrollablePanel;
		}

		public static UIScrollbar CreateScrollbar(UIComponent parent, string name)
		{
			UIScrollbar uIScrollbar = parent.AddUIComponent<UIScrollbar>();
			((UnityEngine.Object)uIScrollbar).name = name;
			return uIScrollbar;
		}

		public static UISlicedSprite CreateSlicedSprite(UIComponent parent, string name)
		{
			UISlicedSprite uISlicedSprite = parent.AddUIComponent<UISlicedSprite>();
			((UnityEngine.Object)uISlicedSprite).name = name;
			return uISlicedSprite;
		}

		public static UIDragHandle CreateDragHandle(UIComponent parent, string name)
		{
			UIDragHandle uIDragHandle = parent.AddUIComponent<UIDragHandle>();
			((UnityEngine.Object)uIDragHandle).name = name;
			uIDragHandle.target = parent;
			return uIDragHandle;
		}

		public static UICheckBox CreateButtonCheckBox(UIComponent parent, string name, UITextureAtlas atlas, string text, bool state)
		{
			UICheckBox uICheckBox = parent.AddUIComponent<UICheckBox>();
			((UnityEngine.Object)uICheckBox).name = name;
			uICheckBox.size = new Vector2(36f, 36f);
			UIButton button = uICheckBox.AddUIComponent<UIButton>();
			button.atlas = atlas;
			button.text = text;
			button.textHorizontalAlignment = UIHorizontalAlignment.Center;
			button.textVerticalAlignment = UIVerticalAlignment.Middle;
			button.relativePosition = new Vector3(0f, 0f);
			button.normalBgSprite = "OptionBase";
			button.hoveredBgSprite = "OptionBaseHovered";
			button.pressedBgSprite = "OptionBasePressed";
			button.disabledBgSprite = "OptionBaseDisabled";
			uICheckBox.isChecked = state;
			if (state)
			{
				button.normalBgSprite = "OptionBaseFocused";
			}
			uICheckBox.eventCheckChanged += delegate (UIComponent component, bool value)
			{
				if (value)
				{
					button.normalBgSprite = "OptionBaseFocused";
				}
				else
				{
					button.normalBgSprite = "OptionBase";
				}
			};
			return uICheckBox;
		}

		public static UIButton CreateSpriteButton(UIComponent parent, string name, UITextureAtlas atlas, string spriteName)
		{
			UIButton uIButton = parent.AddUIComponent<UIButton>();
			((UnityEngine.Object)uIButton).name = name;
			uIButton.atlas = atlas;
			uIButton.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
			uIButton.normalFgSprite = spriteName;
			uIButton.hoveredFgSprite = spriteName + "Hovered";
			uIButton.pressedFgSprite = spriteName + "Pressed";
			uIButton.disabledFgSprite = spriteName + "Disabled";
			return uIButton;
		}

		public static UIButton CreateButton(UIComponent parent, string name, UITextureAtlas atlas, string spriteName)
		{
			UIButton uIButton = parent.AddUIComponent<UIButton>();
			((UnityEngine.Object)uIButton).name = name;
			uIButton.atlas = atlas;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] DEL_START
//			uIButton.normalBgSprite = "OptionBase";
//			uIButton.hoveredBgSprite = "OptionBaseHovered";
//			uIButton.pressedBgSprite = "OptionBasePressed";
//			uIButton.disabledBgSprite = "OptionBaseDisabled";
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] DEL_END
			uIButton.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
			uIButton.normalFgSprite = spriteName;
			uIButton.hoveredFgSprite = spriteName;
			uIButton.pressedFgSprite = spriteName;
			uIButton.disabledFgSprite = spriteName;
			return uIButton;
		}

		public static UILabel CreateTitle(UIComponent parent, string name, string text)
		{
			UILabel uILabel = parent.AddUIComponent<UILabel>();
			uILabel.font = GetUIFont("OpenSans-Bold");
			((UnityEngine.Object)uILabel).name = name;
			uILabel.text = text;
			uILabel.autoSize = false;
			uILabel.height = 20f;
			uILabel.verticalAlignment = UIVerticalAlignment.Middle;
			uILabel.relativePosition = new Vector3(0f, 0f);
			return uILabel;
		}

		public static UILabel CreateLabel(UIComponent parent, string name, string text)
		{
			UILabel uILabel = parent.AddUIComponent<UILabel>();
			((UnityEngine.Object)uILabel).name = name;
			uILabel.font = GetUIFont("OpenSans-Regular");
			uILabel.textScale = 0.875f;
			uILabel.text = text;
			uILabel.autoSize = false;
			uILabel.height = 20f;
			uILabel.verticalAlignment = UIVerticalAlignment.Middle;
			uILabel.relativePosition = new Vector3(0f, 0f);
			return uILabel;
		}

		public static UITextField CreateTextField(UIComponent parent, string name, UITextureAtlas atlas, string text)
		{
			UITextField uITextField = parent.AddUIComponent<UITextField>();
			((UnityEngine.Object)uITextField).name = name;
			uITextField.atlas = atlas;
			uITextField.font = GetUIFont("OpenSans-Regular");
			uITextField.textScale = 0.875f;
			uITextField.height = 32f;
			uITextField.width = parent.width - 10f;
			uITextField.relativePosition = new Vector3(0f, 0f);
			uITextField.normalBgSprite = "OptionsDropboxListbox";
			uITextField.hoveredBgSprite = "OptionsDropboxListboxHovered";
			uITextField.focusedBgSprite = "OptionsDropboxListboxFocused";
			uITextField.disabledBgSprite = "OptionsDropboxListboxDisabled";
			uITextField.selectionSprite = "EmptySprite";
			uITextField.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
			uITextField.horizontalAlignment = UIHorizontalAlignment.Left;
			uITextField.verticalAlignment = UIVerticalAlignment.Middle;
			uITextField.padding = (RectOffset)(object)new RectOffset(10, 5, 10, 5);
			uITextField.builtinKeyNavigation = true;
			uITextField.text = text;
			return uITextField;
		}

		public static UIDropDown CreateDropDown(UIComponent parent, string name, UITextureAtlas atlas)
		{
			UIDropDown dropDown = parent.AddUIComponent<UIDropDown>();
			((UnityEngine.Object)dropDown).name = name;
			dropDown.atlas = atlas;
			dropDown.font = GetUIFont("OpenSans-Regular");
			dropDown.textScale = 0.875f;
			dropDown.height = 32f;
			dropDown.width = parent.width - 10f;
			dropDown.relativePosition = new Vector3(0f, 0f);
			dropDown.listBackground = "OptionsDropboxListbox";
			dropDown.listHeight = 200;
			dropDown.itemHeight = 24;
			dropDown.itemHover = "ListItemHover";
			dropDown.itemHighlight = "ListItemHighlight";
			dropDown.normalBgSprite = "OptionsDropbox";
			dropDown.hoveredBgSprite = "OptionsDropboxHovered";
			dropDown.focusedBgSprite = "OptionsDropboxFocused";
			dropDown.disabledBgSprite = "OptionsDropboxDisabled";
			dropDown.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
			dropDown.horizontalAlignment = UIHorizontalAlignment.Center;
			dropDown.verticalAlignment = UIVerticalAlignment.Middle;
			dropDown.itemPadding = (RectOffset)(object)new RectOffset(5, 5, 5, 5);
			dropDown.listPadding = (RectOffset)(object)new RectOffset(5, 5, 5, 5);
			dropDown.textFieldPadding = (RectOffset)(object)new RectOffset(10, 5, 10, 5);
			dropDown.popupColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			dropDown.popupTextColor = new Color32((byte)170, (byte)170, (byte)170, byte.MaxValue);
			UIButton button = dropDown.AddUIComponent<UIButton>();
			button.height = dropDown.height;
			button.width = dropDown.width;
			button.relativePosition = new Vector3(0f, 0f);
			dropDown.triggerButton = button;
			dropDown.eventSizeChanged += delegate (UIComponent component, Vector2 value)
			{
				dropDown.listWidth = (int)value.x;
				button.size = value;
			};
			return dropDown;
		}

		public static UIDropDown CreateDropDown2(UIComponent parent, string name, UITextureAtlas atlas)
		{
			UIDropDown dropDown = parent.AddUIComponent<UIDropDown>();
			((UnityEngine.Object)dropDown).name = name;
			dropDown.atlas = atlas;
			dropDown.font = GetUIFont("OpenSans-Regular");
			dropDown.textScale = 0.875f;
			dropDown.height = 32f;
			dropDown.width = parent.width - 10f;
			dropDown.relativePosition = new Vector3(0f, 0f);
//			dropDown.listBackground = "OptionsDropboxListbox";
			dropDown.listHeight = 200;
			dropDown.itemHeight = 24;
//			dropDown.itemHover = "ListItemHover";
//			dropDown.itemHighlight = "ListItemHighlight";
//			dropDown.normalBgSprite = "OptionsDropbox";
//			dropDown.hoveredBgSprite = "OptionsDropboxHovered";
//			dropDown.focusedBgSprite = "OptionsDropboxFocused";
//			dropDown.disabledBgSprite = "OptionsDropboxDisabled";
			dropDown.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
			dropDown.horizontalAlignment = UIHorizontalAlignment.Center;
			dropDown.verticalAlignment = UIVerticalAlignment.Middle;
			dropDown.itemPadding = (RectOffset)(object)new RectOffset(5, 5, 5, 5);
			dropDown.listPadding = (RectOffset)(object)new RectOffset(5, 5, 5, 5);
			dropDown.textFieldPadding = (RectOffset)(object)new RectOffset(10, 5, 10, 5);
			dropDown.popupColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			dropDown.popupTextColor = new Color32((byte)170, (byte)170, (byte)170, byte.MaxValue);
			UIButton button = dropDown.AddUIComponent<UIButton>();
			button.height = dropDown.height;
			button.width = dropDown.width;
			button.relativePosition = new Vector3(0f, 0f);
			dropDown.triggerButton = button;
			dropDown.eventSizeChanged += delegate (UIComponent component, Vector2 value)
			{
				dropDown.listWidth = (int)value.x;
				button.size = value;
			};
			return dropDown;
		}

		public static UICheckBox CreateCheckBox(UIComponent parent, string name, UITextureAtlas atlas, string text, bool state)
		{
			UICheckBox uICheckBox = parent.AddUIComponent<UICheckBox>();
			((UnityEngine.Object)uICheckBox).name = name;
			uICheckBox.height = 16f;
			uICheckBox.width = parent.width - 10f;

			UISprite uISprite = uICheckBox.AddUIComponent<UISprite>();
			uISprite.atlas = atlas;
//			uISprite.spriteName = "check-unchecked";
			uISprite.spriteName = "AchievementCheckedFalse";
			uISprite.size = new Vector2(16f, 16f);
			uISprite.relativePosition = Vector3.zero;

			UISprite uISprite2 = uICheckBox.AddUIComponent<UISprite>();
			uISprite2.atlas = atlas;
//			uISprite2.spriteName = "check-checked";
			uISprite2.spriteName = "AchievementCheckedTrue";
			uISprite2.size = new Vector2(16f, 16f);
			uISprite2.relativePosition = Vector3.zero;

			uICheckBox.label = uICheckBox.AddUIComponent<UILabel>();
			uICheckBox.label.font = GetUIFont("OpenSans-Regular");
			uICheckBox.label.textScale = 0.875f;
			uICheckBox.label.verticalAlignment = UIVerticalAlignment.Middle;
			uICheckBox.label.height = 30f;
			uICheckBox.label.relativePosition = new Vector3(25f, 2f);
			uICheckBox.label.text = text;
			uICheckBox.checkedBoxObject = uISprite2;
			uICheckBox.isChecked = state;
			return uICheckBox;
		}

		public static UISprite CreateDivider(UIComponent parent, string name, UITextureAtlas atlas)
		{
			UISprite uISprite = parent.AddUIComponent<UISprite>();
			((UnityEngine.Object)uISprite).name = name;
			uISprite.atlas = atlas;
			uISprite.spriteName = "ContentManagerItemBackground";
			uISprite.height = 15f;
			uISprite.width = parent.width;
			uISprite.relativePosition = new Vector3(0f, 0f);
			return uISprite;
		}

		public static UILabel CreateMenuPanelTitle(UIComponent parent, UITextureAtlas atlas, string title)
		{
			UILabel uILabel = parent.AddUIComponent<UILabel>();
			((UnityEngine.Object)uILabel).name = "Title";
			uILabel.atlas = atlas;
			uILabel.text = title;
			uILabel.textAlignment = UIHorizontalAlignment.Center;
			uILabel.relativePosition = new Vector3(parent.width / 2f - uILabel.width / 2f, 11f);
			return uILabel;
		}

		public static UIButton CreateMenuPanelCloseButton(UIComponent parent, UITextureAtlas atlas)
		{
			UIButton uIButton = parent.AddUIComponent<UIButton>();
			((UnityEngine.Object)uIButton).name = "CloseButton";
			uIButton.atlas = atlas;
			uIButton.relativePosition = new Vector3(parent.width - 37f, 2f);
			uIButton.normalBgSprite = "buttonclose";
			uIButton.hoveredBgSprite = "buttonclosehover";
			uIButton.pressedBgSprite = "buttonclosepressed";
			uIButton.eventClicked += delegate (UIComponent component, UIMouseEventParameter eventParam)
			{
				if (!eventParam.used)
				{
					parent.Hide();
					eventParam.Use();
				}
			};
			return uIButton;
		}

		public static UIDragHandle CreateMenuPanelDragHandle(UIComponent parent)
		{
			UIDragHandle uIDragHandle = parent.AddUIComponent<UIDragHandle>();
			((UnityEngine.Object)uIDragHandle).name = "DragHandle";
			uIDragHandle.width = parent.width - 40f;
			uIDragHandle.height = 40f;
			uIDragHandle.relativePosition = Vector3.zero;
			uIDragHandle.target = parent;
			return uIDragHandle;
		}

// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
		public static UILabel GetOptionTemplate_TextLabel()
		{

			GameObject txtTmplate = UnityEngine.Object.Instantiate(UITemplateManager.GetAsGameObject("OptionsTextfieldTemplate"));
			UIComponent txtTmplate_component = txtTmplate.GetComponent<UIComponent>();
			return txtTmplate_component.Find<UILabel>("Label");
		}
		public static UITextField GetOptionTemplate_TextField()
		{
			GameObject txtTmplate = UnityEngine.Object.Instantiate(UITemplateManager.GetAsGameObject("OptionsTextfieldTemplate"));
			UIComponent txtTmplate_component = txtTmplate.GetComponent<UIComponent>();
			return txtTmplate_component.Find<UITextField>("Text Field");
		}
		public static UIButton CreateButton(UIComponent parent, string name, UITextureAtlas atlas, string spriteName, UIButton baseButton)
		{
			UIButton uIButton = parent.AddUIComponent<UIButton>();
			((UnityEngine.Object)uIButton).name = name;
			uIButton.atlas = atlas;
			uIButton.font = GetUIFont(baseButton.font.name);
			uIButton.normalBgSprite = baseButton.normalBgSprite;
			uIButton.hoveredBgSprite = baseButton.hoveredBgSprite;
			uIButton.pressedBgSprite = baseButton.pressedBgSprite;
			uIButton.disabledBgSprite = baseButton.disabledBgSprite;
			uIButton.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
			uIButton.normalFgSprite = spriteName;
			uIButton.hoveredFgSprite = spriteName;
			uIButton.pressedFgSprite = spriteName;
			uIButton.disabledFgSprite = spriteName;
			return uIButton;
		}

		public static UILabel CreateLabel(UIComponent parent, string name, string text, UILabel baseLabel)
		{
			UILabel uILabel = parent.AddUIComponent<UILabel>();
			uILabel.name = name;
			uILabel.font = GetUIFont(baseLabel.font.name);
			uILabel.font.size = baseLabel.font.size;
			uILabel.text = text;
			uILabel.autoSize = baseLabel.autoSize;
			uILabel.verticalAlignment = baseLabel.verticalAlignment;
			return uILabel;
		}

		public static UITextField CreateTextField(UIComponent parent, string name, string text, UITextField baseTextField)
		{
			UITextField uITextField = parent.AddUIComponent<UITextField>();
			((UnityEngine.Object)uITextField).name = name;
			uITextField.atlas = baseTextField.atlas;
			uITextField.font = GetUIFont(baseTextField.font.name);
			uITextField.font.size = baseTextField.font.size;
			uITextField.textScale = baseTextField.textScale;
			uITextField.color = baseTextField.color;
			uITextField.normalBgSprite = baseTextField.normalBgSprite;
			uITextField.hoveredBgSprite = baseTextField.hoveredBgSprite;
			uITextField.focusedBgSprite = baseTextField.focusedBgSprite;
			uITextField.disabledBgSprite = baseTextField.disabledBgSprite;
			uITextField.selectionSprite = baseTextField.selectionSprite;
			uITextField.foregroundSpriteMode = baseTextField.foregroundSpriteMode;
			uITextField.horizontalAlignment = baseTextField.horizontalAlignment;
			uITextField.verticalAlignment = baseTextField.verticalAlignment;
			uITextField.padding = baseTextField.padding;
			uITextField.builtinKeyNavigation = baseTextField.builtinKeyNavigation;
			uITextField.bottomColor = baseTextField.bottomColor;
			uITextField.characterSpacing = baseTextField.characterSpacing;
			uITextField.processMarkup = baseTextField.processMarkup;
			uITextField.colorizeSprites = baseTextField.colorizeSprites;
			uITextField.textColor = baseTextField.textColor;
			uITextField.disabledTextColor = baseTextField.disabledTextColor;
			uITextField.useGradient = baseTextField.useGradient;
			uITextField.useOutline = baseTextField.useOutline;
			uITextField.outlineSize = baseTextField.outlineSize;
			uITextField.outlineColor = baseTextField.outlineColor;
			uITextField.useDropShadow = baseTextField.useDropShadow;
			uITextField.dropShadowColor = baseTextField.dropShadowColor;
			uITextField.dropShadowOffset = baseTextField.dropShadowOffset;
			uITextField.selectionBackgroundColor = baseTextField.selectionBackgroundColor;
			uITextField.selectionSprite = baseTextField.selectionSprite;
			uITextField.text = text;
			return uITextField;
		}

		public static void AddTexturesInAtlas(UITextureAtlas atlas, Texture2D[] newTextures, bool locked = false)
		{
			Texture2D[] array = new Texture2D[atlas.count + newTextures.Length];
			for (int i = 0; i < atlas.count; i++)
			{
				Texture2D val = atlas.sprites[i].texture;
				if (locked)
				{
					RenderTexture temporary = RenderTexture.GetTemporary(((Texture)val).width, ((Texture)val).height, 0);
					Graphics.Blit((Texture)val, temporary);
					RenderTexture active = RenderTexture.active;
					val = new Texture2D(((Texture)temporary).width, ((Texture)temporary).height);
					RenderTexture.active = (temporary);
					val.ReadPixels(new Rect(0f, 0f, (float)((Texture)temporary).width, (float)((Texture)temporary).height), 0, 0);
					val.Apply();
					RenderTexture.active = (active);
					RenderTexture.ReleaseTemporary(temporary);
				}
				array[i] = val;
				array[i].name = atlas.sprites[i].name;
			}
			for (int j = 0; j < newTextures.Length; j++)
			{
				array[atlas.count + j] = newTextures[j];
			}
			Rect[] array2 = atlas.texture.PackTextures(array, atlas.padding, 4096, false);
			atlas.sprites.Clear();
			for (int k = 0; k < array.Length; k++)
			{
				UITextureAtlas.SpriteInfo spriteInfo = atlas[(array[k]).name];
				atlas.sprites.Add(new UITextureAtlas.SpriteInfo
				{
					texture = array[k],
					name = array[k].name,
					border = (RectOffset)((spriteInfo != null) ? spriteInfo.border : (new RectOffset())),
					region = array2[k]
				});
			}
			atlas.RebuildIndexes();
		}
		public static UITextureAtlas GetAtlas(string name)
		{
			UITextureAtlas[] array = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].name == name)
				{
					return array[i];
				}
			}
			return UIView.GetAView().defaultAtlas;
		}

		public UIButton CreateImageButton(UIComponent parent, string name, string iconName)
		{
			if (m_citiesPlateauAtlas == null)
			{
				m_citiesPlateauAtlas = LoadResources();
			}
			UIButton rtnButton = UIUtils.CreateButton(parent, name, m_citiesPlateauAtlas, iconName);
			rtnButton.tooltip = "";

			return rtnButton;
		}

		private UITextureAtlas LoadResources()
		{
			try
			{
				UITextureAtlas rtnAtlas;
				string[] spriteNames = new string[4]
				{
						"folderIcon",
						"folderDialog_desktop",
						"folderDialog_pc",
						"folderDialog_folder"
				};
				rtnAtlas = CreateTextureAtlas("SkylinesPlateauAtlas", spriteNames, "SkylinesPlateau.res.icon.");
				UITextureAtlas atlas = GetAtlas("Ingame");
				Texture2D[] newTextures = (Texture2D[])new Texture2D[5]
				{
						atlas["OptionBase"].texture,
						atlas["OptionBaseFocused"].texture,
						atlas["OptionBaseHovered"].texture,
						atlas["OptionBasePressed"].texture,
						atlas["OptionBaseDisabled"].texture
				};
				AddTexturesInAtlas(rtnAtlas, newTextures);
				return rtnAtlas;
			}
			catch (Exception exception)
			{
				Debug.Log("LoadResources Exception Error : " + exception);
				Debug.Log("LoadResources Exception Error : " + exception.Message);
				Debug.Log("LoadResources Exception Error : " + exception.StackTrace);
				return null;
			}
		}
		private UITextureAtlas CreateTextureAtlas(string atlasName, string[] spriteNames, string assemblyPath)
		{
			int num = 1024;
			Texture2D val = new Texture2D(1, 1, (TextureFormat)5, false);
			Texture2D[] array = new Texture2D[spriteNames.Length];
			Rect[] array2 = new Rect[spriteNames.Length];
			for (int i = 0; i < spriteNames.Length; i++)
			{
				array[i] = LoadTextureFromAssembly(assemblyPath + spriteNames[i] + ".png");
			}
			array2 = val.PackTextures(array, 2, num);
			UITextureAtlas uITextureAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();
			Material val2 = UnityEngine.Object.Instantiate<Material>(UIView.GetAView().defaultAtlas.material);
			val2.mainTexture = val;
			uITextureAtlas.material = val2;
			(uITextureAtlas).name = atlasName;
			for (int j = 0; j < spriteNames.Length; j++)
			{
				UITextureAtlas.SpriteInfo item = new UITextureAtlas.SpriteInfo
				{
					name = spriteNames[j],
					texture = array[j],
					region = array2[j]
				};
				uITextureAtlas.AddSprite(item);
			}
			return uITextureAtlas;
		}
		private Texture2D LoadTextureFromAssembly(string path)
		{
			try
			{
				Assembly executingAssembly = Assembly.GetExecutingAssembly();
				using (Stream textureStream = executingAssembly.GetManifestResourceStream(path))
				{
					return LoadTextureFromStream(textureStream);
				}
			}
			catch (Exception exception)
			{
				Debug.Log("LoadTextureFromAssembly Exception Error : " + exception);
				Debug.Log("LoadTextureFromAssembly Exception Error : " + exception.Message);
				Debug.Log("LoadTextureFromAssembly Exception Error : " + exception.StackTrace);
				return null;
			}
		}
		private Texture2D LoadTextureFromStream(Stream textureStream)
		{
			byte[] array = new byte[textureStream.Length];
			textureStream.Read(array, 0, array.Length);
			textureStream.Close();
			Texture2D texture2D = new Texture2D(36, 36, TextureFormat.ARGB32, mipmap: true)
			{
				filterMode = FilterMode.Trilinear
			};
			texture2D.LoadImage(array);
			return texture2D;
		}
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END
	}
}
