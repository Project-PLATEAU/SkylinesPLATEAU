//----------------------------------------------------------------------------
// CommonBL.cs
//
// ■概要
//      座標系の変換機能を提供するクラス
// 
// ■改版履歴
//      Ver00.03.00     2020.03.31      G.Arakawa@Cmind     新規作成
//
//----------------------------------------------------------------------------
using System;

namespace SkylinesPlateau
{
    //--------------------------
    // 座標変換機能を提供するクラス
    //--------------------------
    public class CommonBL
    {
        //--------------------------
        // 固定値
        //--------------------------
        private readonly double[] gPsi = { 0.0, 33.0, 33.0, 36.0, 33.0, 36.0, 36.0, 36.0, 36.0, 36.0, 40.0, 44.0, 44.0, 44.0, 26.0, 26.0, 26.0, 26.0, 20.0, 26.0 };
        private readonly double[] gLmd = { 0.0, 129.5, 131.0, 132.1666666666, 133.5, 134.3333333333, 136.0, 137.1666666666, 138.5, 139.8333333333, 140.8333333333, 140.25, 142.25, 144.25, 142.0, 127.5, 124.0, 131.0, 136.0, 154.0 };
        private readonly double[] CC = { 123.0, 129.0, 135.0, 141.0, 147.0, 153.0 };
        private readonly double[] C_L = { 1.005037306048555E0, 5.0478492403E-3, 1.0563786831E-5, 2.0633322E-8, 3.8853E-11, 7.0E-14 };
        private const double JA = 6377397.155;          // 日本測地系
        private const double JFS = 1.0 / 299.152813;    // 日本測地系
        private const double WA = 6378137.0;            // 世界測地系
        private const double WFS = 1.0 / 298.257222101; // 世界測地系
        private const double M0 = 0.9999;
        private const double QQ = 9.996E-1;
        private const double R2 = 6.377397155E6;
        private const double E1 = 6.674372231315E-3;
        private const double E2 = 6.719218798677E-3;
        private const double Y0 = 5.0E5;
        private const double EE = 0.006674372231315;
        private const double B0 = 1.005037306045577;
        private const double B1 = 0.002511273240647;
        private const double B2 = 0.000003678785849;
        private const double B3 = 0.000000007380969;
        private const double B4 = 0.000000000016832;
        private const double B5 = 0.000000000000041;

        // 平面直角座標系
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_START
//        public static readonly string[] SYS_NAME_LIST = { "平面直角座標　１系", "平面直角座標　２系", "平面直角座標　３系", "平面直角座標　４系", "平面直角座標　５系",
//            "平面直角座標　６系", "平面直角座標　７系", "平面直角座標　８系", "平面直角座標　９系", "平面直角座標１０系", "平面直角座標１１系", "平面直角座標１２系",
//            "平面直角座標１３系", "平面直角座標１４系", "平面直角座標１５系", "平面直角座標１６系", "平面直角座標１７系", "平面直角座標１８系", "平面直角座標１９系" };
        public static readonly string[] SYS_NAME_LIST = { 
            "01: 長崎, 鹿児島(南西部)",
            "02: 福岡, 佐賀, 熊本, 大分, 宮崎, 鹿児島(北東部)",
            "03: 山口, 島根, 広島",
            "04: 香川, 愛媛, 徳島, 高知",
            "05: 兵庫, 鳥取, 岡山",
            "06: 京都, 大阪, 福井, 滋賀, 三重, 奈良, 和歌山",
            "07: 石川, 富山, 岐阜, 愛知",
            "08: 新潟, 長野, 山梨, 静岡",
            "09: 東京(本州), 福島, 栃木, 茨城, 埼玉, 千葉, 群馬, 神奈川",
            "10: 青森, 秋田, 山形, 岩手, 宮城", 
            "11: 北海道(西部)", 
            "12: 北海道(中央部)",
            "13: 北海道(東部)", 
            "14: 諸島(東京南部)", 
            "15: 沖縄", 
            "16: 諸島(沖縄西部)", 
            "17: 諸島(沖縄東部)", 
            "18: 小笠原諸島", 
            "19: 南鳥島" };
// 2023.08.18 G.Arakawa@cmind [2023年度の改修対応] UPD_END

        //--------------------------
        // ローカル変数
        //--------------------------
        private static CommonBL instance;
        public static CommonBL Instance => instance ?? (instance = new CommonBL());

        private double A = WA;
        private double FS = WFS;
        public int iSystem = 9;

        /// <summary>
        /// 度単位の数値をラジアン単位に変換
        /// </summary>
        /// <param name="deg">度単位の数値</param>
        /// <returns>ラジアン単位の数値</returns>
        public static double deg2rad(double deg)
        {
            return deg / 180.0 * Math.PI;
        }

        /// <summary>
        /// 緯度経度をWEBメルカトルに変換
        /// </summary>
        /// <param name="lat">緯度</param>
        /// <param name="lon">経度</param>
        /// <param name="x">X座標(WEBメルカトル)</param>
        /// <param name="y">Y座標(WEBメルカトル)</param>
        public static void latlng2merc(double lat, double lon, out double x, out double y)
        {
            y = Math.Log(Math.Tan(Math.PI / 4.0 + deg2rad(lat) / 2.0)) * WA;
            x = deg2rad(lon) * WA;
            
        }

        // Webメルカトル図法から経度に変換
        public static double XToLon(double x)
        {
            return (x / (WA * Math.PI)) * 180.0;
        }
        // Webメルカトル図法から緯度に変換
        public static double YToLat(double y)
        {
            // 引数チェック
            double lat = (y / (6378137.0 * Math.PI)) * 180;
            lat = 180.0 / Math.PI * (2.0 * Math.Atan(Math.Exp(lat * Math.PI / 180.0)) - Math.PI / 2.0);
            return lat;
        }
        // 経度からWebメルカトル図法に変換
        public static double LonToX(double lon)
        {
            // 参考サイト：http://qiita.com/kochizufan/items/bf880c8d2b25385d4efe
            return WA * lon * Math.PI / 180.0;
        }
        // 緯度からWebメルカトル図法に変換
        public static double LatToY(double lat) {
            // 参考サイト：http://qiita.com/kochizufan/items/bf880c8d2b25385d4efe
            return WA * Math.Log(Math.Tan(Math.PI / 360.0 * (90.0 + lat)));
        }


        // 現在設定されている座標系から、世界測地系の緯度経度に変換する
        public void ido_keido(double posX, double posY, out double dpIdo, out double dpKeido)
        {
            // 戻り値を設定
            dpIdo = 0.0;
            dpKeido = 0.0;

            // (平面直角の場合、内部ではXYを逆転させている。描画範囲を回転させたいので）
            double x = posY;
            double y = posX;

            double psi1 = psi(x);
            double t1 = Math.Tan(psi1);
            double eta2 = ED * ED * Math.Cos(psi1) * Math.Cos(psi1);

            //-----------------------------------
            // 緯度を算出
            //-----------------------------------
            double tmp1 = 1.0 + eta2;
            double tmp2 = 5.0 + 3.0 * Math.Pow(t1, 2) + 6.0 * eta2 - 6.0 * Math.Pow(t1, 2) * eta2 - 3.0 * Math.Pow(eta2, 2) - 9.0 * Math.Pow(t1, 2) * Math.Pow(eta2, 2);
            double tmp3 = 61.0 + 90.0 * Math.Pow(t1, 2) + 45.0 * Math.Pow(t1, 4) + 107.0 * eta2 - 162.0 * Math.Pow(t1, 2) * eta2 - 45.0 * Math.Pow(t1, 4) * eta2;
            double tmp4 = 1385.0 + 3633.0 * Math.Pow(t1, 2) + 4095.0 * Math.Pow(t1, 4) + 1575.0 * Math.Pow(t1, 6);

            dpIdo = psi1 - 1.0 / 2.0 * 1.0 / Math.Pow(N(psi1), 2) * t1 * tmp1 * Math.Pow((y / M0), 2)
                         + 1.0 / 24.0 * 1.0 / Math.Pow(N(psi1), 4) * t1 * tmp2 * Math.Pow((y / M0), 4)
                         - 1.0 / 720.0 * 1.0 / Math.Pow(N(psi1), 6) * t1 * tmp3 * Math.Pow((y / M0), 6)
                         + 1.0 / 40320.0 * 1.0 / Math.Pow(N(psi1), 8) * t1 * tmp4 * Math.Pow((y / M0), 8);
            dpIdo = dpIdo * 180.0 / Math.PI;

            //-----------------------------------
            // 経度を算出
            //-----------------------------------
            double lmd0 = gLmd[iSystem] * Math.PI / 180.0;
            double tmp5 = 1.0 + 2.0 * Math.Pow(t1, 2) + eta2;
            double tmp6 = 5.0 + 28.0 * Math.Pow(t1, 2) + 24.0 * Math.Pow(t1, 4) + 6.0 * eta2 + 8.0 * Math.Pow(t1, 2) * eta2;
            double tmp7 = 61.0 + 662.0 * Math.Pow(t1, 2) + 1320.0 * Math.Pow(t1, 4) + 720.0 * Math.Pow(t1, 6);
            dpKeido = lmd0 + 1.0 / (N(psi1) * Math.Cos(psi1)) * (y / M0)
                           - 1.0 / (6.0 * Math.Pow(N(psi1), 3) * Math.Cos(psi1)) * tmp5 * Math.Pow((y / M0), 3)
                           + 1.0 / (120.0 * Math.Pow(N(psi1), 5) * Math.Cos(psi1)) * tmp6 * Math.Pow((y / M0), 5)
                           - 1.0 / (5040.0 * Math.Pow(N(psi1), 7) * Math.Cos(psi1)) * tmp7 * Math.Pow((y / M0), 7);
            dpKeido = dpKeido * 180.0 / Math.PI;
        }

        // 世界測地系の緯度経度から、現在設定されている座標系に変換する
        public void bl2xy(double ido, double keido, out double posX, out double posY)
        {
            posX = 0;
            posY = 0;

            double psi0 = gPsi[iSystem] * Math.PI / 180.0;
            double lmd0 = gLmd[iSystem] * Math.PI / 180.0;
            double psi1 = ido * Math.PI / 180.0;
            double lmd1 = keido * Math.PI / 180.0;
            double dlmd = lmd1 - lmd0;
            double t1 = Math.Tan(psi1);
            double eta2 = ED * ED * Math.Cos(psi1) * Math.Cos(psi1);
            double S0 = ss(psi0);
            double S1 = ss(psi1);

            //-----------------------------------
            // X座標を算出
            //-----------------------------------
            double tmp1 = 5.0 - Math.Pow(t1, 2) + 9.0 * eta2 + 4.0 * Math.Pow(eta2, 2);
            double tmp2 = -61.0 + 58.0 * Math.Pow(t1, 2) - Math.Pow(t1, 4) - 270.0 * eta2 + 330.0 * Math.Pow(t1, 2) * eta2;
            double tmp3 = -1385.0 + 3111.0 * Math.Pow(t1, 2) - 543.0 * Math.Pow(t1, 4) + Math.Pow(t1, 6);
            posX = ((S1 - S0)
                  + 1.0 / 2.0 * N(psi1) * Math.Pow(Math.Cos(psi1), 2) * t1 * Math.Pow(dlmd, 2)
                  + 1.0 / 24.0 * N(psi1) * Math.Pow(Math.Cos(psi1), 4) * t1 * tmp1 * Math.Pow(dlmd, 4)
                  - 1.0 / 720.0 * N(psi1) * Math.Pow(Math.Cos(psi1), 6) * t1 * tmp2 * Math.Pow(dlmd, 6)
                  - 1.0 / 40320.0 * N(psi1) * Math.Pow(Math.Cos(psi1), 8) * t1 * tmp3 * Math.Pow(dlmd, 8)) * M0;

            //-----------------------------------
            // Y座標を算出
            //-----------------------------------
            double tmp4 = -1.0 + Math.Pow(t1, 2) - Math.Pow(eta2, 2);
            double tmp5 = -5.0 + 18.0 * Math.Pow(t1, 2) - Math.Pow(t1, 4) - 14.0 * Math.Pow(eta2, 2) + 58.0 * Math.Pow(t1, 2) * eta2;
            double tmp6 = -61.0 + 479.0 * Math.Pow(t1, 2) - 179.0 * Math.Pow(t1, 4) + Math.Pow(t1, 6);
            posY = (N(psi1) * Math.Cos(psi1) * dlmd
                  - 1.0 / 6.0 * N(psi1) * Math.Pow(Math.Cos(psi1), 3) * tmp4 * Math.Pow(dlmd, 3)
                  - 1.0 / 120.0 * N(psi1) * Math.Pow(Math.Cos(psi1), 5) * tmp5 * Math.Pow(dlmd, 5)
                  - 1.0 / 5040.0 * N(psi1) * Math.Pow(Math.Cos(psi1), 7) * tmp6 * Math.Pow(dlmd, 7)) * M0;

            // (平面直角の場合、内部ではXYを逆転させている。描画範囲を回転させたいので）
            double tmp = posX;
            posX = posY;
            posY = tmp;
        }

        //-----------------------------------
        // ReadOnly
        //-----------------------------------
        private double FL { get { return 1 / FS; }}
        private double B { get { return A * (1.0 - FS); }}
        private double C { get { return A / (1.0 - FS); }}
        private double E { get { return Math.Sqrt(2.0 * FS - FS * FS); }}
        private double ED { get { return Math.Sqrt(E * E / (1.0 - E * E)); }}
        private double P { get { return 180.0 / Math.PI; }}
        private double AEE { get { return A * (1.0 - EE); }}
        private double EEE { get { return EE / (1.0 - EE); }}
        private double V(double PSI) { return Math.Sqrt(1.0 + (ED * ED) * Math.Pow(Math.Cos(PSI), 2)); }
        private double W(double PSI) { return Math.Sqrt(1.0 + (E * E * -1.0) * Math.Pow(Math.Sin(PSI), 2));}
        private double M(double PSI) { return C / (Math.Pow(V(PSI), 3)); }
        private double N(double PSI) { return C / V(PSI); }
        private double R(double PSI) { return C / (Math.Pow(V(PSI), 2)); }

        private double psi(double dX)
        {
            int i = 0;
            double dPsi_N, dPsi_N1;

            dPsi_N = gPsi[iSystem] * Math.PI / 180.0;
            dPsi_N1 = psi_NtoN1(dPsi_N, dX);
            i += 1;

            while (Math.Abs(dPsi_N1 - dPsi_N) > 2.0 / 60.0 / 60.0 * Math.Pow(10, -5))
            {
                dPsi_N = dPsi_N1;
                dPsi_N1 = psi_NtoN1(dPsi_N, dX);
                i += 1;
            }

            return dPsi_N1;
        }

        private double psi_NtoN1(double psi_n, double posX)
        {
            double dSpsi = ss(psi_n);
            double dM = ss((gPsi[iSystem] * Math.PI / 180.0)) + posX / M0;
            double dTmp1 = dSpsi - dM;
            double dTmp2 = 1 - E * E * Math.Sin(psi_n) * Math.Sin(psi_n);
            double dTmp3 = 1 - E * E;
            double psi_n1 = psi_n + 2 * dTmp1 * Math.Pow(dTmp2, 3 / 2) / (3 * Math.Pow(E, 2) * dTmp1 * Math.Sin(psi_n) * Math.Cos(psi_n) * Math.Pow(dTmp2, 1 / 2) - 2 * A * dTmp3);
            return psi_n1;
        }

        private double ss(double dPsi)
        {
            double dA, dB, dC, dD, dE, dF, dG, dH, dI;
            double[] aB = new double[10];

            dA = 1.0 + 3.0 / 4.0 * Math.Pow(E, 2) + 45.0 / 64.0 * Math.Pow(E, 4) + 175.0 / 256.0 * Math.Pow(E, 6) + 11025.0 / 16384.0 * Math.Pow(E, 8) + 43659.0 / 65536.0 * Math.Pow(E, 10) + 693693.0 / 1048576.0 * Math.Pow(E, 12) + 19324305.0 / 29360128.0 * Math.Pow(E, 14) + 4927697775.0 / 7516192768.0 * Math.Pow(E, 16);
            dB = 3.0 / 4.0 * Math.Pow(E, 2) + 15.0 / 16.0 * Math.Pow(E, 4) + 525.0 / 512.0 * Math.Pow(E, 6) + 2205.0 / 2048.0 * Math.Pow(E, 8) + 72765.0 / 65536.0 * Math.Pow(E, 10) + 297297.0 / 262144.0 * Math.Pow(E, 12) + 135270135.0 / 117440512.0 * Math.Pow(E, 14) + 547521975.0 / 469762048.0 * Math.Pow(E, 16);
            dC = 15.0 / 64.0 * Math.Pow(E, 4) + 105.0 / 256.0 * Math.Pow(E, 6) + 2205.0 / 4096.0 * Math.Pow(E, 8) + 10395.0 / 16384.0 * Math.Pow(E, 10) + 1486485.0 / 2097152.0 * Math.Pow(E, 12) + 45090045.0 / 58720256.0 * Math.Pow(E, 14) + 766530765.0 / 939524096.0 * Math.Pow(E, 16);
            dD = 35.0 / 512.0 * Math.Pow(E, 6) + 315.0 / 2048.0 * Math.Pow(E, 8) + 31185.0 / 131072.0 * Math.Pow(E, 10) + 165165.0 / 524288.0 * Math.Pow(E, 12) + 45090045.0 / 117440512.0 * Math.Pow(E, 14) + 209053845.0 / 469762048.0 * Math.Pow(E, 16);
            dE = 315.0 / 16384.0 * Math.Pow(E, 8) + 3465.0 / 65536.0 * Math.Pow(E, 10) + 99099.0 / 1048576.0 * Math.Pow(E, 12) + 4099095.0 / 29360128.0 * Math.Pow(E, 14) + 348423075.0 / 1879048192.0 * Math.Pow(E, 16);
            dF = 693.0 / 131072.0 * Math.Pow(E, 10) + 9009.0 / 524288.0 * Math.Pow(E, 12) + 4099095.0 / 117440512.0 * Math.Pow(E, 14) + 26801775.0 / 469762048.0 * Math.Pow(E, 16);
            dG = 3003.0 / 2097152.0 * Math.Pow(E, 12) + 315315.0 / 58720256.0 * Math.Pow(E, 14) + 11486475.0 / 939524096.0 * Math.Pow(E, 16);
            dH = 45045.0 / 117440512.0 * Math.Pow(E, 14) + 765765.0 / 469762048.0 * Math.Pow(E, 16);
            dI = 765765.0 / 7516192768.0 * Math.Pow(E, 16);

            aB[0] = 0.0;
            aB[1] = A * (1.0 - E * E) * dA;
            aB[2] = A * (1.0 - E * E) * (-dB / 2.0);
            aB[3] = A * (1.0 - E * E) * (dC / 4.0);
            aB[4] = A * (1.0 - E * E) * (-dD / 6.0);
            aB[5] = A * (1.0 - E * E) * (dE / 8.0);
            aB[6] = A * (1.0 - E * E) * (-dF / 10.0);
            aB[7] = A * (1.0 - E * E) * (dG / 12.0);
            aB[8] = A * (1.0 - E * E) * (-dH / 14.0);
            aB[9] = A * (1.0 - E * E) * (dI / 16.0);

            return aB[1] * dPsi + aB[2] * Math.Sin(2.0 * dPsi) + aB[3] * Math.Sin(4.0 * dPsi) + aB[4] * Math.Sin(6.0 * dPsi) + aB[5] * Math.Sin(8.0 * dPsi) + aB[6] * Math.Sin(10.0 * dPsi) + aB[7] * Math.Sin(12.0 * dPsi) + aB[8] * Math.Sin(14.0 * dPsi) + aB[9] * Math.Sin(16.0 * dPsi);
        }
    }
}
