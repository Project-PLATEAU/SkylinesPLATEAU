using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SkylinesPlateau.common
{
	public class UIUtils
	{
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
			uIButton.normalBgSprite = "OptionBase";
			uIButton.hoveredBgSprite = "OptionBaseHovered";
			uIButton.pressedBgSprite = "OptionBasePressed";
			uIButton.disabledBgSprite = "OptionBaseDisabled";
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
	}
}
