//----------------------------------------------------------------------------
// SkylinesPlateau.cs
//
// ■概要
//      Cities:SkylinesのMOD画面に表示する内容を定義するクラス
// 
//
//----------------------------------------------------------------------------
using ICities;

namespace SkylinesPlateau
{
    public class SkylinesPlateau: IUserMod
	{
		// MOD名
		public string Name
        {
            get { return "SkylinesPLATEAU"; }
        }

        // MODの説明文
        public string Description
        {
            get { return "3D都市モデルを参照し、Cities: Skylines上に実在都市を再現するModです。"; }
        }

// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_START
        // MODの設定項目
        public void OnSettingsUI(UIHelper helper)
		{
			SettingsUI.OnSettingsUI(helper);
        }

		public void OnEnabled()
        {
            IniFileData.Instance.Load();
            AssetTbl.Instance.Load();
            ZoneSgTbl zoneTbl = new ZoneSgTbl();
            BuildingSgTbl build = new BuildingSgTbl();
            zoneTbl = null;
            build = null;
            SettingsUI.UpdateOptionSetting();
        }
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] ADD_END
    }
}
