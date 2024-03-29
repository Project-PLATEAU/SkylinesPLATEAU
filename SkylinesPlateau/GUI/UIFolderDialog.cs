using UnityEngine;
using ColossalFramework.UI;
using System.Collections.Generic;
using System.IO;
using System;
using UIUtils = SkylinesPlateau.common.UIUtils;
using ICities;

namespace SkylinesPlateau
{
    class UIFolderDialog //: GUIWindow //, IGameObject
    {
        protected enum SortingType
        {
            Name,
            Timestamp,
            Extension
        }

        private const float DIALOG_WIDTH = 600f;
        private const float DIALOG_HEIGHT = 500f;

        private UIPanel m_backPanel;
        private UIPanel m_DialogPanel;
        private UITextField m_PathTextField;
        private UIListBox m_FolderListBox;
        private UIButton m_ApplyButton;
        private UITextField m_NameTextField;

        private UIDragHandle _dragHandle;

        private UITextureAtlas m_Atlas;

        private DirectoryInfo[] m_folderList;
        public string m_FolderPath = "";

        public OnButtonClicked onOkButtonCallback = null;
        public OnButtonClicked onCancelButtonCallback = null;
        private bool m_drawBackFolder = true;
        private bool visible = false;
        public bool Visible
        {
            get {
                return visible;
            }
            set
            {
                // 背景パネルが存在していない場合
                if (m_backPanel == null)
                {
                    return;
                }

                if (value)
                {
                    m_PathTextField.text = m_FolderPath;
                    FolderReload();
                    m_backPanel.Show();
                }
                else
                {
                    m_backPanel.Hide();
                }
                visible = value;
            }
        }

        public UIFolderDialog()
//            : base("Debug console", new Rect(16f, 16f, 512f, 256f))
        {
        }

        private void Start()
        {
            try
            {
            }
            catch (Exception ex)
            {
                Debug.Log((object)("[SkylinesCityGml] UIFolderDialog:Start -> Exception: " + ex.Message));
            }
        }

        private string GetFolderName(string path)
        {
            string rtnStr = "";
            string checkStr = path;

            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    rtnStr = Path.GetFileName(path);
                    // \ or / 区切りで終わっている場合を考慮する
                    if (string.IsNullOrEmpty(rtnStr))
                    {
                        rtnStr = Path.GetDirectoryName(path);
                    }
                    if (string.IsNullOrEmpty(rtnStr))
                    {
                        rtnStr = "";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log((object)("[SkylinesCityGml] GetFolderName -> Exception: " + ex));
                rtnStr = "";
            }

            return rtnStr;
        }

        // 表示状態にする
        public void drawDialog(UIComponent parentView)
        {

            //----------------------------------
            // テクスチャ取得
            //----------------------------------
            UITextureAtlas[] array = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
            m_Atlas = null;
            for (int i = 0; i < array.Length; i++)
            {
                if (((UnityEngine.Object)array[i]).name == "Ingame")
                {
                    m_Atlas = array[i];
//                    break;
                }
            }

            if (m_Atlas == null)
            {
                m_Atlas = UIView.GetAView().defaultAtlas;
            }

            //----------------------------------
            // 画面全体の背景パネル
            //----------------------------------
            m_backPanel = parentView.AddUIComponent(typeof(UIPanel)) as UIPanel;
            m_backPanel.atlas = m_Atlas;
            m_backPanel.width = parentView.size.x;
            m_backPanel.height = parentView.size.y;
            m_backPanel.relativePosition = new Vector3(0, 0);

            //----------------------------------
            // ダイアログの背景パネル
            //----------------------------------
            m_DialogPanel = m_backPanel.AddUIComponent(typeof(UIPanel)) as UIPanel;
            m_DialogPanel.atlas = m_Atlas;
            m_DialogPanel.width = DIALOG_WIDTH;
            m_DialogPanel.height = DIALOG_HEIGHT;
            m_DialogPanel.relativePosition = new Vector3(Mathf.Floor(((float)parentView.size.x - m_DialogPanel.width) / 2f), Mathf.Floor(((float)parentView.size.y - m_DialogPanel.height) / 2f));
//            m_backPanel.backgroundSprite = "GenericPanel";
            m_DialogPanel.backgroundSprite = "MenuPanel2";
            m_DialogPanel.Focus();

            _dragHandle = UIUtils.CreateMenuPanelDragHandle(m_DialogPanel);

            // フォルダ選択用のダイアログ生成
            ShowFolderDialog();

            // ダイアログ検索
            FolderReload();
        }

        private void ShowFolderDialog()
        {
            try
            {
                //----------------------------------------------------
                // フォルダ選択ダイアログのカスタムUIを作成
                //----------------------------------------------------
                float offsetX = 10.0f;
                float offsetY = 0.0f;
                float space_w = 10.0f;
                float space_h = 10.0f;

                float button_w = 100.0f;
                float button_h = 30.0f;
                float button_w2 = 70.0f;
                float button_h2 = 70.0f;

                //-------------------------
                // タイトルラベル
                //-------------------------
                offsetY += space_w;
                UILabel label = UIUtils.CreateMenuPanelTitle(m_DialogPanel, m_Atlas, "フォルダを選択してください");
//                UILabel label = m_DialogPanel.AddUIComponent<UILabel>();
//                label.text = "フォルダを選択してください";
//                label.relativePosition = new Vector3(offsetX, offsetY);
                offsetY += label.height;
                offsetY += space_h;

                //-------------------------
                // 選択中のパス
                //-------------------------
                UITextField baseTextField = UIUtils.GetOptionTemplate_TextField();
                m_PathTextField = UIUtils.CreateTextField(m_DialogPanel, "m_PathTextField", m_FolderPath, baseTextField);
                m_PathTextField.relativePosition = new Vector3(offsetX, offsetY);
                m_PathTextField.width = DIALOG_WIDTH - (space_w * 2);
                m_PathTextField.height = 30f;
                m_PathTextField.eventTextChanged += delegate (UIComponent c, string sel)
                {
                    // フォルダ選択処理
                    Debug.Log("フォルダパスが変更されました");
                    m_FolderPath = m_PathTextField.text;
                    Debug.Log("フォルダ移動：" + m_FolderPath);
                    // 再描画
                    FolderReload();
                };
                m_PathTextField.eventTextSubmitted += delegate (UIComponent c, string sel)
                {
                    // フォルダ選択処理
                    Debug.Log("フォルダパスが変更されました");
                    m_FolderPath = m_PathTextField.text;
                    Debug.Log("フォルダ移動：" + m_FolderPath);
                    // 再描画
                    FolderReload();
                };
                offsetY += m_PathTextField.height;
                offsetY += space_h;

                //----------------------------------------------------------------------------
                // ショートカットボタン群
                //----------------------------------------------------------------------------
                float bk_offsetY = offsetY;
                //-------------------------
                // 上のフォルダ
                //-------------------------
#if false
                UIButton backButton = CreateShortcutButton(m_DialogPanel, "上のフォルダ", "folderDialog_folderBack", offsetX, offsetY, button_w2, button_h2, () =>
                {
                    Debug.Log("選択：上のフォルダ");
                    // フォルダパスを設定
                    string parentFolderPath = Path.GetDirectoryName(m_FolderPath);
                    if (!string.IsNullOrEmpty(parentFolderPath))
                    {
                        //--------------------------
                        // １つ上のフォルダを追加
                        //--------------------------
                        DirectoryInfo dic = new DirectoryInfo(parentFolderPath);
                        try
                        {
                            // アクセス可能かチェック
                            dic.GetDirectories();
                            // パスを設定
                            m_FolderPath = dic.FullName;
                        }
                        catch (Exception ex)
                        {
                            Debug.Log("アクセス不可 : " + parentFolderPath);
                            Debug.Log("アクセス不可 : " + dic);
                        }
                    }
                    Debug.Log("フォルダ移動：" + m_FolderPath);
                    // リストを再描画する
                    FolderReload();
                });
                offsetY += button_w2;
                offsetY += space_h;
#endif
                //-------------------------
                // デスクトップ
                //-------------------------
                UIButton desktopButton = CreateShortcutButton(m_DialogPanel, "デスクトップ", "folderDialog_desktop", offsetX, offsetY, button_w2, button_h2, () =>
                {
                    Debug.Log("選択：デスクトップ");
                    // フォルダパスを設定
                    m_FolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    Debug.Log("フォルダ移動：" + m_FolderPath);
                    // リストを再描画する
                    FolderReload();
                });
                offsetY += button_w2;
                offsetY += space_h;
                //-------------------------
                // マイドキュメント
                //-------------------------
                UIButton mydocButton = CreateShortcutButton(m_DialogPanel, "ドキュメント", "folderDialog_folder", offsetX, offsetY, button_w2, button_h2, () =>
                {
                    Debug.Log("選択：マイドキュメント");
                    // フォルダパスを設定
                    m_FolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    Debug.Log("フォルダ移動：" + m_FolderPath);
                    // リストを再描画する
                    FolderReload();
                });
                offsetY += button_w2;
                offsetY += space_h;

                //-------------------------
                // ドライブ
                //-------------------------
                UIButton pcButton = CreateShortcutButton(m_DialogPanel, "ドライブ", "folderDialog_pc", offsetX, offsetY, button_w2, button_h2, () =>
                {
                    Debug.Log("選択：PC");
                    // フォルダパスを設定
                    m_FolderPath = "";
                    Debug.Log("フォルダ移動：" + m_FolderPath);
                    // リストを再描画する
                    FolderReload();
                });
                offsetY += button_w2;
                offsetY += space_h;

                offsetY = bk_offsetY;


                //-------------------------
                // フォルダリスト
                //-------------------------
                offsetX = button_w2 + space_w * 2;
                m_FolderListBox = m_DialogPanel.AddUIComponent<UIListBox>();
                m_FolderListBox.atlas = m_Atlas;
                m_FolderListBox.text = "";

                m_FolderListBox.normalBgSprite = "OptionsDropboxListbox";
//                m_FolderListBox.hoveredBgSprite = "OptionsDropboxListboxHovered";
//                m_FolderListBox.focusedBgSprite = "OptionsDropboxListboxFocused";
                m_FolderListBox.disabledBgSprite = "OptionsDropboxListboxDisabled";

                m_FolderListBox.itemHighlight = "ListItemHighlight";
                m_FolderListBox.itemHover = "ListItemHover";

                m_FolderListBox.normalFgSprite = "OptionsDropboxListbox";
//                m_FolderListBox.hoveredFgSprite = "OptionsDropboxListboxHovered";
//                m_FolderListBox.focusedFgSprite = "OptionsDropboxListboxFocused";
                m_FolderListBox.disabledFgSprite = "OptionsDropboxListboxDisabled";

                m_FolderListBox.itemPadding = new RectOffset(10, 10, 5, 0);
                m_FolderListBox.listPadding = new RectOffset(10, 10, 10, 10);
                //                m_FolderListBox.color = new Color32(255, 1, 1, 255);
                //                m_FolderListBox.colorizeSprites = true;

//                m_FolderListBox.itemHeight = 20;

                m_FolderListBox.relativePosition = new Vector3(offsetX, offsetY);
                m_FolderListBox.width = DIALOG_WIDTH - ( offsetX + space_w );
                m_FolderListBox.height = DIALOG_HEIGHT - ( offsetY + (space_h * 2) + button_h );
                m_FolderListBox.eventSelectedIndexChanged += OnListingSelectionChanged;
                offsetY += m_FolderListBox.height;
                offsetY += space_h;

                //-------------------------
                // 選択中のフォルダ名
                //-------------------------
                m_NameTextField = UIUtils.CreateTextField(m_DialogPanel, "m_NameTextField", GetFolderName(m_FolderPath), baseTextField);
                m_NameTextField.relativePosition = new Vector3(offsetX, offsetY);
                m_NameTextField.width = DIALOG_WIDTH - (offsetX + (space_w + button_w) * 2 + space_w);
                m_NameTextField.height = button_h;
                m_NameTextField.verticalAlignment = UIVerticalAlignment.Middle;
                m_NameTextField.readOnly = true;
                m_NameTextField.eventClick += (component, eventParam) => {};
//                m_NameTextField.enabled = false;

                //-------------------------
                // 選択ボタン
                //-------------------------
                offsetX += m_NameTextField.width;
                offsetX += space_w;
                m_ApplyButton = m_DialogPanel.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsButtonTemplate")) as UIButton;
                m_ApplyButton.text = "選択";
                m_ApplyButton.textScale = 0.75f;
                m_ApplyButton.autoSize = false;
                m_ApplyButton.textHorizontalAlignment = UIHorizontalAlignment.Center;
                m_ApplyButton.relativePosition = new Vector3(offsetX, offsetY);
                m_ApplyButton.width = button_w;
                m_ApplyButton.height = button_h;
                m_ApplyButton.eventClick += (component, eventParam) =>
                {
                    // フォルダ選択処理
                    Debug.Log("フォルダ選択：選択ボタン押下");
                    if (onOkButtonCallback != null)
                    {
                        onOkButtonCallback();
                    }
                    // ダイアログを閉じる
                    this.Visible = false;
                };

                //-------------------------
                // キャンセルボタン
                //-------------------------
                offsetX += button_w;
                offsetX += space_w;
                UIButton cancelButton = m_DialogPanel.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsButtonTemplate")) as UIButton;
                cancelButton.text = "キャンセル";
                cancelButton.textScale = 0.75f;
                cancelButton.autoSize = false;
                cancelButton.textHorizontalAlignment = UIHorizontalAlignment.Center;
                cancelButton.relativePosition = new Vector3(offsetX, offsetY);
                cancelButton.width = button_w;
                cancelButton.height = button_h;
                cancelButton.eventClick += (component, eventParam) =>
                {
                    Debug.Log("フォルダ選択：キャンセルボタン押下");
                    if (onCancelButtonCallback != null)
                    {
                        onCancelButtonCallback();
                    }
                    // ダイアログを閉じる
                    this.Visible = false;
                };
                offsetY += cancelButton.height;
                offsetY += space_h;

                //-------------------------
                // ダイアログの高さを調整
                //-------------------------
                m_DialogPanel.height = offsetY;
                m_DialogPanel.Focus();
            }
            catch (Exception ex)
            {
                Debug.Log((object)("[SkylinesCityGml] UIFolderDialog:ShowFolderDialog -> Exception: " + ex.Message));
            }
        }


        private UIButton CreateShortcutButton(UIComponent parent, string name, string iconName, float x, float y, float w, float h, OnButtonClicked eventCallback)
        {
            float label_h = 20f;

            //-------------------------
            // ボタン配置
            //-------------------------
            UIButton button = UIUtils.Instance.CreateImageButton(parent, name, iconName);
            button.tooltip = "";
            button.width = w;
            button.height = h - label_h;
            button.relativePosition = new Vector3(x, y);
            button.horizontalAlignment = UIHorizontalAlignment.Center;
            button.eventClicked += delegate (UIComponent component, UIMouseEventParameter eventParam)
            {
                if (eventCallback != null)
                {
                    eventCallback();
                }
            };

            //-------------------------
            // ラベル配置
            //-------------------------
            UILabel label = parent.AddUIComponent<UILabel>();
            label.autoSize = false;
            label.text = name;
            label.width = w;
            label.height = label_h;
            label.textScale = 0.75f;
            label.textAlignment = UIHorizontalAlignment.Center;
            label.relativePosition = new Vector3(x, y + (h - label.height));

            return button;
        }

        private void OnListingSelectionChanged(UIComponent comp, int sel)
        {
            if (sel <= -1)
            {
                return;
            }

            Debug.Log("フォルダ選択：移動");
            Debug.Log("Clicked item: " + m_FolderListBox.items[sel]);
            if (m_drawBackFolder && sel == 0)
            {
                // １つ上のフォルダ
                string parentFolderPath = Path.GetDirectoryName(m_FolderPath);
                if (parentFolderPath != null && parentFolderPath.Length > 0)
                {
                    m_FolderPath = parentFolderPath;
                    Debug.Log("上のパス：" + m_FolderPath);
                }
                else
                {
                    m_FolderPath = "";
                    Debug.Log("最上位パス：" + m_FolderPath);
                }
            }
            else
            {
                int selectIdx = sel;
                if (m_drawBackFolder)
                {
                    // リストボックスには「上フォルダ」あり。
                    // 内部のディレクトリ一覧にｈ「上フォルダ」なし。
                    selectIdx--;
                }
                m_FolderPath = m_folderList[selectIdx].FullName;
                Debug.Log("フォルダ移動：" + m_FolderPath);
            }
            // リストを再描画する
            FolderReload();
        }
        private void SetDriveList()
        {
            //----------------------------------------------
            // 表示用のリスト生成
            //----------------------------------------------
            List<string> list = new List<string>();
            List<DirectoryInfo> list2 = new List<DirectoryInfo>();

            try
            {
                //--------------------------
                // ドライブ一覧を追加
                //--------------------------
                string[] driveList = Directory.GetLogicalDrives();
                foreach (string driveName in driveList)
                {
                    DirectoryInfo dic2 = new DirectoryInfo(driveName);
                    if (dic2.Exists)
                    {
                        list.Add(dic2.Name);
                        list2.Add(dic2);
                    }
                    else
                    {
                        Debug.Log("フォルダが無い : " + driveName);
                        Debug.Log("フォルダが無い : " + dic2);
                        Debug.Log("フォルダが無い : " + dic2.FullName);
                    }
                }
                // メンバに設定
                m_folderList = list2.ToArray();
                m_FolderListBox.items = list.ToArray();
            }
            catch (Exception ex)
            {
                Debug.Log("SetDriveList() exception : " + ex);
            }
        }

        private void SetFolderList(string path)
        {
            //----------------------------------------------
            // 表示用のリスト生成
            //----------------------------------------------
            List<string> list = new List<string>();
            List<DirectoryInfo> list2 = new List<DirectoryInfo>();

            try
            {
                //-----------------------
                // 指定フォルダを検索
                //-----------------------
                DirectoryInfo directoryInfo = new DirectoryInfo(m_FolderPath);
                if (!directoryInfo.Exists)
                {
                    Debug.Log("フォルダなし");
                    // リストを初期化
                    m_folderList = new DirectoryInfo[] { };
                    m_FolderListBox.items = new string[] { };
                    return;
                }
                DirectoryInfo[] array = null;
                try
                {
                    array = directoryInfo.GetDirectories();
                    Sort(array, SortingType.Name);
                }
                catch (Exception ex)
                {
                    Debug.Log("An exception occured " + ex);
                    // リストを初期化
                    m_folderList = new DirectoryInfo[] { };
                    m_FolderListBox.items = new string[] { };
                    return;
                }

                //---------------------------------
                // 上のフォルダに移動可能かチェック
                //---------------------------------
                m_drawBackFolder = false;
                string parentFolderPath = Path.GetDirectoryName(m_FolderPath);
                if (parentFolderPath != null && parentFolderPath.Length > 0)
                {
                    //--------------------------
                    // １つ上のフォルダを追加
                    //--------------------------
                    DirectoryInfo dic = new DirectoryInfo(parentFolderPath);
                    try
                    {
                        // アクセス可能かチェック
                        dic.GetDirectories();
                        // 上のフォルダの表示あり
                        list.Add("上のフォルダへ移動");
                        m_drawBackFolder = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("上フォルダ、アクセス不可 : " + parentFolderPath);
                        Debug.Log("上フォルダ、アクセス不可 : " + dic);
                    }
                }
                else
                {
                    // 上のフォルダが最上位フォルダ
                    list.Add("上のフォルダへ移動");
                    m_drawBackFolder = true;
                }

                //--------------------------
                // 通常フォルダを追加
                //--------------------------
                foreach (DirectoryInfo dirInfo in array)
                {
                    DirectoryInfo dic2 = new DirectoryInfo(dirInfo.FullName);
                    try
                    {
                        // アクセス可能かチェック
                        dic2.GetDirectories();

                        // リストに追加
                        list.Add(dic2.Name);
                        list2.Add(dic2);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("アクセス不可 : " + dirInfo.FullName);
                        Debug.Log("アクセス不可 : " + dic2);
                    }
                }

                // メンバに設定
                m_folderList = list2.ToArray();
                m_FolderListBox.items = list.ToArray();
            }
            catch (Exception ex)
            {
                Debug.Log("SetFolderList() exception : " + ex);
            }
        }


        private void FolderReload()
        {
            // テキストフィールドを差し替える
            m_PathTextField.text = m_FolderPath;
            m_NameTextField.text = GetFolderName(m_FolderPath);

            // 戻るボタンは非表示で初期化
            m_drawBackFolder = false;

            // パスの指定がない場合
            if (string.IsNullOrEmpty(m_FolderPath))
            {
                // ドライブ一覧を設定
                SetDriveList();
            }
            // パスの指定がない場合
            else
            {
                // フォルダ一覧を設定
                SetFolderList(m_FolderPath);
            }
        }

        protected void Sort(FileInfo[] files, SortingType type)
        {
            switch (type)
            {
                case SortingType.Timestamp:
                    Array.Sort(files, (FileInfo a, FileInfo b) => b.LastWriteTime.CompareTo(a.LastWriteTime));
                    break;
                case SortingType.Name:
                    Array.Sort(files, (FileInfo a, FileInfo b) => a.Name.CompareTo(b.Name));
                    break;
                case SortingType.Extension:
                    Array.Sort(files, (FileInfo a, FileInfo b) => a.Extension.CompareTo(b.Extension));
                    break;
            }
        }
        protected void Sort(DirectoryInfo[] files, SortingType type)
        {
            switch (type)
            {
                case SortingType.Timestamp:
                    Array.Sort(files, (DirectoryInfo a, DirectoryInfo b) => b.LastWriteTime.CompareTo(a.LastWriteTime));
                    break;
                case SortingType.Name:
                    Array.Sort(files, (DirectoryInfo a, DirectoryInfo b) => a.Name.CompareTo(b.Name));
                    break;
            }
        }
	}
}
