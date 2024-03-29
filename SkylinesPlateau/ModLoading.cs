//----------------------------------------------------------------------------
// ModLoading.cs
//
// ■概要
//      MOD読み込み時の動作を管理するクラス
// 
//
//----------------------------------------------------------------------------
using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System.IO;
using UnityEngine;

namespace SkylinesPlateau
{
    public class ModLoading : ILoadingExtension
    {
        private GameObject _objImpGui = null;
        private GameObject _objImpMapPanel = null;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] DEL_START
//        private GameObject _objImpHighwayPanel = null;
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] DEL_END
        private GameObject _objImpFeaturesPanel = null;

        // called when level loading begins
        public void OnCreated(ILoading loading)
        {
        }

        // called when level is loaded
        public void OnLevelLoaded(LoadMode mode)
        {
            UIView uiView = Object.FindObjectOfType<UIView>();
            if (uiView != null)
            {
                if (_objImpMapPanel == null)
                {
                    _objImpMapPanel = (GameObject)(object)new GameObject("ImpMapPanel");
                    _objImpMapPanel.transform.parent = uiView.transform;
                    _objImpMapPanel.AddComponent<ImpMapPanel>();
                }
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] DEL_START
/*
                if (_objImpHighwayPanel == null)
                {
                    _objImpHighwayPanel = (GameObject)(object)new GameObject("ImpHighwayPanel");
                    _objImpHighwayPanel.transform.parent = uiView.transform;
                    _objImpHighwayPanel.AddComponent<ImpHighwayPanel>();
                }
*/
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] DEL_END
                if (_objImpFeaturesPanel == null)
                {
                    _objImpFeaturesPanel = (GameObject)(object)new GameObject("ImpFeaturesPanel");
                    _objImpFeaturesPanel.transform.parent = uiView.transform;
                    _objImpFeaturesPanel.AddComponent<ImpFeaturesPanel>();
                }
                if (_objImpGui == null)
                {
                    _objImpGui = (GameObject)(object)new GameObject("SkylinesCityGml_GUI");
                    _objImpGui.transform.parent = uiView.transform;
                    _objImpGui.AddComponent<ImpGUI>();
                }
            }
        }

        // called when unloading begins
        public void OnLevelUnloading()
        {
//            if (_objImpGui != null) { Object.Destroy(_objImpGui); _objImpGui }
//            if (_objImpMapPanel != null) { Object.Destroy(_objImpMapPanel); }
//            if (_objImpHighwayPanel != null) { Object.Destroy(_objImpHighwayPanel); }
//            if (_objImpFeaturesPanel != null) { Object.Destroy(_objImpFeaturesPanel); }
        }

        // called when unloading finished
        public void OnReleased()
        {
        }
    }
}
