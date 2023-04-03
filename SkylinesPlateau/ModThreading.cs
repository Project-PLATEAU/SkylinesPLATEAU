//----------------------------------------------------------------------------
// ModThreading.cs
//
// ■概要
//      Cities:Skylinesのゲーム画面上で動作するスレッドを管理するクラス
// 
//
//----------------------------------------------------------------------------
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace SkylinesPlateau
{
    public class ModThreading : ThreadingExtensionBase
    {
        /// <summary>
        /// 多重操作を防止するためのフラグ (true : 実行中, false: 待機中)
        /// </summary>
        private bool _processed = false;

        /// <summary>
        /// 描画対象のView
        /// </summary>
        private UIView uiView;

        /// <summary>
        /// ボタン表示するViewクラス
        /// </summary>
        private ImpGUI gui;

        public void drawGUI()
        {
            if (uiView == null)
            {
                uiView = Object.FindObjectOfType<UIView>();
                gui = (ImpGUI)uiView.gameObject.GetComponent(typeof(ImpGUI));
            }
            if (gui == null)
            {
                gui = (uiView.gameObject.AddComponent(typeof(ImpGUI)) as ImpGUI);
            }
            if (gui.uiView == null)
            {
                gui.uiView = uiView;
            }
//            _ = base.managers.threading.simulationPaused;
        }

        //---------------------------------------------------
        // 画面更新のタイミングで動作するメソッド
        //---------------------------------------------------
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            //----------------------------------------
            // 画面上にボタン配置
            //----------------------------------------
            drawGUI();
        }
    }
}
