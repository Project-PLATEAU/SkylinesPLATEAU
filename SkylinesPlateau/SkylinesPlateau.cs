//----------------------------------------------------------------------------
// SkylinesCityGml.cs
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
            get { return "SkylinesPlateau"; }
        }

        // MODの説明文
        public string Description
        {
            get { return "Plateauオープンデータを参照し、Cities: Skylines上に実在都市を再現するModです。"; }
        }
    }
}
