//----------------------------------------------------------------------------
// CommonFunc.cs
//
// ■概要
//      ポリゴンの内外判定など、共通機能を提供するクラス
// 
// ■改版履歴
//      Ver00.03.00     2020.03.31      G.Arakawa@Cmind     新規作成
//
//----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkylinesPlateau
{
    public class CommonFunc
    {
        /// <summary>
        /// ラインとポリゴンの重なり判定
        /// </summary>
        static public bool checkLineInPoly
        (
            List<Vector3> in_line,  // [I] 入力ライン
            List<Vector3> in_poly   // [I] 入力ポリゴン
        )
        {
            // ラインの頂点とポリゴンとの内外判定
            foreach (Vector3 pos in in_line)
            {
                if (CommonFunc.checkPointInPoly(pos, in_poly))
                {
                    return true;
                }
            }
            // ラインの交差判定
            return CommonFunc.checkLineInLine(in_line, in_poly);
        }

        /// <summary>
        /// 矩形同士の内外判定処理（0:範囲外 1:[範囲１が範囲２の内側] 2:[範囲２が範囲１の内側] 3:重なり）
        /// </summary>
        static public int checkAreaInArea
        (
            Vector3 in_max1,    // [I] 入力矩形１
            Vector3 in_min1,    // [I] 入力矩形１ 
            Vector3 in_max2,    // [I] 入力矩形２
            Vector3 in_min2     // [I] 入力矩形２
        )
        {
            int xOver = 0;
            int yOver = 0;

            //-----------------------------------
            // Y方向の重なり判定
            //-----------------------------------
            // Y方向がエリアの内側にある場合
            if (in_min1.y >= in_min2.y && in_max1.y <= in_max2.y) yOver = 1;
            // Y方向がエリアの外側にある場合
            else if (in_min1.y <= in_min2.y && in_max1.y >= in_max2.y) yOver = 2;
            // Y方向の最小値のみがエリアの内側にある場合
            else if (in_min1.y >= in_min2.y && in_min1.y <= in_max2.y) yOver = 3;
            // Y方向の最大値のみがエリアの内側にある場合
            else if (in_max1.y >= in_min2.y && in_max1.y <= in_max2.y) yOver = 3;
            // 重なっていない場合
            else return 0;

            //-----------------------------------
            // X方向の重なり
            //-----------------------------------
            // X方向がエリアの内側にある場合
            if (in_min1.x >= in_min2.x && in_max1.x <= in_max2.x) xOver = 1;
            // X方向がエリアの外側にある場合
            else if (in_min1.x <= in_min2.x && in_max1.x >= in_max2.x) xOver = 2;
            // X方向の最小値のみがエリアの内側にある場合
            else if (in_min1.x >= in_min2.x && in_min1.x <= in_max2.x) xOver = 3;
            // X方向の最大値のみがエリアの内側にある場合
            else if (in_max1.x >= in_min2.x && in_max1.x <= in_max2.x) xOver = 3;
            // 重なっていない場合
            else return 0;

            // XYどちらの方向も重なり方が同じな場合
            if (xOver == yOver)
            {
                return xOver;
            }

            return 3;
        }

        /// <summary>
        /// ポリゴンの重なり判定
        /// </summary>
        static public bool checkPolyInPoly(List<Vector3> inPos1, List<Vector3> inPos2)
        {
            // ポリゴンの重なり判定
            if (checkLineInLine(inPos1, inPos2))
            {
                // 重なっている
                return true;
            }

            // 各頂点の内外判定
            for (int jx = 0; jx < inPos1.Count; jx++)
            {
                // Poly1の頂点が、Poly2に含まれているか判定
                if (checkPointInPoly(inPos1[jx], inPos2))
                {
                    // 含まれている
                    return true;
                }
            }

            // 各頂点の内外判定
            for (int jx = 0; jx < inPos2.Count; jx++)
            {
                // Poly2の頂点が、Poly1に含まれているか判定
                if (checkPointInPoly(inPos2[jx], inPos1))
                {
                    // 含まれている
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 頂点の内外判定
        /// </summary>
        static public bool checkPointInPoly(Vector3 pos1, List<Vector3> pos2)
        {
            //--------------------------------------
            // ローカル変数定義
            //--------------------------------------
            long iCrossNum, iLeftNum;
            double dCrossPosX;
            Vector3 max, min;

            //--------------------------------------
            // 内外判定処理
            //--------------------------------------
            iCrossNum = 0;
            iLeftNum = 0;
            // ポリゴンの頂点数分ループ
            for (int i = 0; i < pos2.Count; i++)
            {
                //--------------------------------------
                // 判定対象の直線の最大最小範囲を取得
                //--------------------------------------
                min = pos2[i];
                max = pos2[(i + 1) % pos2.Count];
                if (pos2[i].y > pos2[(i + 1) % pos2.Count].y)
                {
                    min = max;
                    max = pos2[i];
                }

                //--------------------------------------
                // 比較対象のラインか判定
                //--------------------------------------
                if (pos1.y >= min.y && pos1.y <= max.y)
                {
                    // 交差判定
                    dCrossPosX = (min.x * (max.y - pos1.y) + max.x * (pos1.y - min.y)) / (max.y - min.y);
                    iCrossNum++;
                    if (dCrossPosX < pos1.x)
                    {
                        iLeftNum++;
                    }
                }
            }

            //--------------------------------------
            // 判定結果を返却
            //--------------------------------------
            if (iCrossNum % 2 == 0 && iLeftNum % 2 == 1)
            {
                // 範囲内
                return true;
            }
            else
            {
                // 範囲外
                return false;
            }
        }

        /// <summary>
        /// 線分の交差判定
        /// </summary>
        static public bool checkLineInLine(List<Vector3> pos1, List<Vector3> pos2)
        {
            Vector3 xy11, xy12, xy21, xy22;

            for (int ix = 0; ix < pos1.Count - 1; ix++)
            {
                // 基本線分
                xy11.x = pos1[ix].x;
                xy11.y = pos1[ix].y;
                xy11.z = 0;
                xy12.x = pos1[ix + 1].x;
                xy12.y = pos1[ix + 1].y;
                xy12.z = 0;

                for (int iy = 0; iy < pos2.Count - 1; iy++)
                {
                    // 検査対象線分
                    xy21.x = pos2[iy].x;
                    xy21.y = pos2[iy].y;
                    xy21.z = 0;
                    xy22.x = pos2[iy + 1].x;
                    xy22.y = pos2[iy + 1].y;
                    xy22.z = 0;

                    if (Check_LineCross(xy11, xy12, xy21, xy22))
                    {
                        return true;
                    }
/*
                    if( ((xy11.x - xy12.x) * (xy21.y - xy11.y) + (xy11.y - xy12.y) * (xy11.x - xy21.x)) * 
                        ((xy11.x - xy12.x) * (xy22.y - xy11.y) + (xy11.y - xy12.y) * (xy11.x - xy22.x)) < 0.0)
                    {
                        if( ((xy21.x - xy22.x) * (xy11.y - xy21.y) + (xy21.y - xy22.y) * (xy21.x - xy11.x)) * 
                            ((xy21.x - xy22.x) * (xy12.y - xy21.y) + (xy21.y - xy22.y) * (xy21.x - xy12.x)) < 0.0)
                        {
                            return false;
                        }
                    }
*/
                }
            }
            return false;
        }

        /// <summary>
        /// 点が直線の左側にあるか右側にあるか調べる
        /// </summary>
        static public int Check_CrossPos(Vector3 a, Vector3 b, Vector3 c)
        {
            double v1x, v1y, v2x, v2y;
            int retval;

            v1x = b.x - a.x;
            v1y = b.y - a.y;
            v2x = c.x - a.x;
            v2y = c.y - a.y;
            if (v1x * v2y - v1y * v2x > 0.0)
            {
                retval = 1;												// 左側
            }
            else if (v1x * v2y - v1y * v2x < 0.0)
            {
                retval = -1;											// 右側
            }
            else
            {
                retval = 0;												// 同一直線上
            }

            return retval;
        }

        /// <summary>
        /// ２つの線分が交差しているか判定
        /// </summary>
        static public bool Check_LineCross
        (
            Vector3 xy11,	// [I] 辺１の頂点１
            Vector3 xy12,	// [I] 辺１の頂点２
            Vector3 xy21,	// [I] 辺２の頂点１
            Vector3 xy22	// [I] 辺２の頂点２
        )
        {
            // 線分の始点、終点がが直線に対して左側か右側か調べ、どちらも同じ側にあれば交わらない、
            // 線分同士で互いに同じ処理を繰り返せば、線分同士の交差をチェックできる。
            if (Check_CrossPos(xy11, xy12, xy21) != Check_CrossPos(xy11, xy12, xy22) &&
                Check_CrossPos(xy21, xy22, xy11) != Check_CrossPos(xy21, xy22, xy12))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 直線と線分が交差しているか判定
        /// ※直線は2点間を通る線、線分は2点間を結ぶ線
        /// http://www5d.biglobe.ne.jp/~tomoya03/shtml/algorithm/Intersection.htm
        /// </summary>
        static public bool Check_LineCross2
        (
            Vector3 xy11,	// [I] 辺１の頂点１（直線）
            Vector3 xy12,	// [I] 辺１の頂点２（直線）
            Vector3 xy21,	// [I] 辺２の頂点１（線分）
            Vector3 xy22	// [I] 辺２の頂点２（線分）
        )
        {
            // 直線に対して、線分の頂点が左側、右側それぞれに存在していれば交差する。
            if (Check_CrossPos(xy11, xy12, xy21) != Check_CrossPos(xy11, xy12, xy22))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// ポリゴンの頂点並び順を判定（ TRUE:時計回り FALSE:反時計回り[穴あき扱い] ）
        /// </summary>
        static public bool checkPolyL2R(List<Vector3> in_points)
        {
            int i;
            double s = 0;
            Vector3 vec1, vec2;

            for (i = 0; i < in_points.Count; i++)
            {
                vec1 = in_points[i];
                vec2 = in_points[(i + 1) % in_points.Count];
                // 方向判断
                s += ((vec1.x * vec2.y) - (vec1.y * vec2.x));
            }
            if (s < 0)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 矩形とポリゴンの内外判定
        /// </summary>
        static public bool ClipRect(List<Vector3> in_point1, List<Vector3> in_point2)
        {
            //----------------------
            // ローカル変数
            //----------------------
            long iCrossNum, iLeftNum;
            double dCrossPosX;
            double dX, dY;
            Vector3 max1, min1;

            //----------------------
            // 初期化
            //----------------------
            min1 = new Vector3();
            max1 = new Vector3();
            dX = min1.x + ((max1.x - min1.x) / 2.0);
            dY = min1.y + ((max1.y - min1.y) / 2.0);

            //----------------------
            // 頂点数分ループ処理
            //----------------------
            iCrossNum = 0;
            iLeftNum = 0;
            for (int i = 0; i < in_point1.Count; i++)
            {
                // 最大最小を設定
                min1 = in_point1[i];
                max1 = in_point1[(i + 1) % in_point1.Count];
                if (min1.y > max1.y)
                {
                    // 入れ替える
                    min1 = max1;
                    max1 = in_point1[i];
                }

                // ===== Detect Cross Line ===== //
                if (dY > min1.y && dY <= max1.y)
                {
                    // ===== Calculate Cross Position ===== //
                    dCrossPosX = (min1.x * (max1.y - dY) + max1.x * (dY - min1.y)) / (max1.y - min1.y);
                    iCrossNum++;
                    if (dCrossPosX < dX)
                    {
                        iLeftNum++;
                    }
                    for (int j = 0; j < 4; j++)
                    {
                        // 交差判定
                        if (Check_LineCross(max1, min1, in_point2[j], in_point2[(j + 1) % 4]))
                        {
                            // 範囲が交差しているため、重なっている
                            return true;
                        }
                    }
                }
            }

            // ===== Check Inner Polygon ===== //
            if (iCrossNum % 2 == 0 && iLeftNum % 2 == 1)
            {
                // 重なり
                return true;
            }
            else
            {
                // 重なっていない
                return false;
            }
        }

        /// <summary>
        /// 2点間の距離を算出
        /// </summary>
        static public double Dist2Point(Vector3 in_point1, Vector3 in_point2)
        {
            double dDist;
            // ===== ２点間の距離計算 ===== //
            dDist = Math.Sqrt(
                ((double)in_point1.x - (double)in_point2.x) *
                ((double)in_point1.x - (double)in_point2.x) +
                ((double)in_point1.y - (double)in_point2.y) *
                ((double)in_point1.y - (double)in_point2.y)
            );
            return dDist;
        }

        /// <summary>
        /// 点群の長さを算出する
        /// </summary>
        static public double Dist2PointList(List<Vector3> in_point1, out List<double> out_distList)
        {
            double dDist_all = 0;
            double dDist = 0;

            out_distList = new List<double>();
            for (var i = 0; i < in_point1.Count-1; i++)
            {
                dDist = Dist2Point(in_point1[i], in_point1[i + 1]);
                out_distList.Add(dDist);
                dDist_all += dDist;
            }

            return dDist_all;
        }

        /// <summary>
        /// 直線上の指定位置に頂点を生成
        /// </summary>
        static public Vector3 GetLinePos(Vector3 in_point1, Vector3 in_point2, double Dist)
        {
            double Distance1;           // 始終点の距離

            // 比較元の始点から終点までの距離
            Distance1 = Dist2Point(in_point1, in_point2);
            // 比較元の始点と終点の差分
            Vector3 vec = in_point2 - in_point1;
            // 比較元の始点から中心位置までの高さの増減量
            Vector3 add = (float)Dist * (vec / (float)Distance1);

            // 指定位置の座標
            return in_point1 + add;
        }

        /// <summary>
        /// 2点間の中心位置を取得
        /// </summary>
        static public Vector3 GetLineCenterPos(Vector3 in_point1, Vector3 in_point2)
        {
            return in_point1 + ((in_point2 - in_point1) / 2);
        }

        /// <summary>
        /// 頂点Pから線分に垂線をおろし交点Qを求める。
        /// 算出した頂点が線上ならtrue, 線上でなければfalseを返す。
        /// </summary>
        static public bool GetPerpendicularFootPoint(Vector3 in_point1, Vector3 in_point2, Vector3 in_p, out Vector3 out_q)
        {
            // 内積で位置を算出
            Vector3 ab = (in_point2 - in_point1).normalized;
            float k = Vector3.Dot(in_p - in_point1, ab);
            out_q = in_point1 + k * ab;

            // 線上判定
            if (k < 0.0 || k > 1.0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// ２点間の中心点から垂線を上げる
        /// http://20100718seko.blog130.fc2.com/blog-entry-153.html
        /// </summary>
        static public Vector3 GetLineCenterPerpendicularPoint(Vector3 in_point1, Vector3 in_point2)
        {
            // ２点間の中心位置を求める
            Vector3 center = CommonFunc.GetLineCenterPos(in_point1, in_point2);
            // centerを中心としたPos2の位置を算出
            Vector3 vec = in_point2 - center;
            // centerを中心にPos2を90度回転
            double angle = Math.PI * 90 / 180;
            double x = vec.x * Math.Cos(angle) - vec.y * Math.Sin(angle);
            double y = vec.x * Math.Sin(angle) + vec.y * Math.Cos(angle);
            // 回転後の座標位置を返却
            return new Vector3(center.x + (float)x, center.y + (float)y, 0.0f);
        }

        /// <summary>
        /// ２つの線分の交差判定
        ///  true:交差する, false:交差しない
        /// https://spphire9.wordpress.com/2013/01/14/%E7%9B%B4%E7%B7%9A%E3%81%A8%E7%B7%9A%E5%88%86%E3%81%AE%E5%BD%93%E3%81%9F%E3%82%8A%E5%88%A4%E5%AE%9A/
        /// https://sapphire-al2o3.github.io/js-demo/math/%E7%9B%B4%E7%B7%9A%E3%81%A8%E7%B7%9A%E5%88%86%E3%81%AE%E5%BD%93%E3%81%9F%E3%82%8A%E5%88%A4%E5%AE%9A/
        /// https://mf-atelier.sakura.ne.jp/mf-atelier2/a1/
        /// </summary>
        static public bool GetCrossPos
        (
            Vector3 in_point1,      // 線分1
            Vector3 in_point2,      // 線分1
            Vector3 in_point3,      // 線分2
            Vector3 in_point4,      // 線分2
            out Vector3 out_point   // 算出した交点
        )
        {
            // 初期化
            out_point.x = 0.0f;
            out_point.y = 0.0f;
            out_point.z = 0.0f;

            // 交差判定
            bool check = Check_LineCross(in_point1, in_point2, in_point3, in_point4);
            if (check)
            {
                double ksi, delta, ramda;
                ksi = (in_point4.y - in_point3.y) * (in_point4.x - in_point1.x) - (in_point4.x - in_point3.x) * (in_point4.y - in_point1.y);
                delta = (in_point2.x - in_point1.x) * (in_point4.y - in_point3.y) - (in_point2.y - in_point1.y) * (in_point4.x - in_point3.x);
                ramda = ksi / delta;
                out_point.x = (float)(in_point1.x + ramda * (in_point2.x - in_point1.x));
                out_point.y = (float)(in_point1.y + ramda * (in_point2.y - in_point1.y));
                out_point.z = (float)(in_point1.z + ramda * (in_point2.z - in_point1.z));
            }
            return check;
        }

        /// <summary>
        /// 直線と線分の交差判定
        ///  true:交差する, false:交差しない
        /// https://spphire9.wordpress.com/2013/01/14/%E7%9B%B4%E7%B7%9A%E3%81%A8%E7%B7%9A%E5%88%86%E3%81%AE%E5%BD%93%E3%81%9F%E3%82%8A%E5%88%A4%E5%AE%9A/
        /// https://sapphire-al2o3.github.io/js-demo/math/%E7%9B%B4%E7%B7%9A%E3%81%A8%E7%B7%9A%E5%88%86%E3%81%AE%E5%BD%93%E3%81%9F%E3%82%8A%E5%88%A4%E5%AE%9A/
        /// https://mf-atelier.sakura.ne.jp/mf-atelier2/a1/
        /// </summary>
        static public bool GetCrossPos2
        (
            Vector3 in_point1,      // 直線1
            Vector3 in_point2,      // 直線1
            Vector3 in_point3,      // 線分2
            Vector3 in_point4,      // 線分2
            out Vector3 out_point   // 算出した交点
        ){
            // 初期化
            out_point.x = 0.0f;
            out_point.y = 0.0f;
            out_point.z = 0.0f;

            // 交差判定
            bool check = Check_LineCross2(in_point1, in_point2, in_point3, in_point4);
            if (check)
            {
                double ksi, delta, ramda;
                ksi = (in_point4.y - in_point3.y) * (in_point4.x - in_point1.x) - (in_point4.x - in_point3.x) * (in_point4.y - in_point1.y);
                delta = (in_point2.x - in_point1.x) * (in_point4.y - in_point3.y) - (in_point2.y - in_point1.y) * (in_point4.x - in_point3.x);
                ramda = ksi / delta;
                out_point.x = (float)(in_point1.x + ramda * (in_point2.x - in_point1.x));
                out_point.y = (float)(in_point1.y + ramda * (in_point2.y - in_point1.y));
                out_point.z = (float)(in_point1.z + ramda * (in_point2.z - in_point1.z));
            }
            return check;
        }

        /// <summary>
        /// ベクトルから角度を取得
        /// https://hacchi-man.hatenablog.com/entry/2020/03/05/220000
        /// </summary>
        public static double GetAngle
        (
            Vector3 in_point1,      // 線分の始点
            Vector3 in_point2       // 線分の終点
        )
        {
            double dx, dy, t, a;
            int area;

            dx = in_point2.x - in_point1.x;
            dy = in_point2.y - in_point1.y;

            if (dx >= 0.0)
            {
                if (dy >= 0.0) { area = 0; }    /* 第１象限 */
                else { area = 3; t = dx; dx = -dy; dy = t; }    /* 第４象限 */
            }
            else
            {
                if (dy >= 0.0) { area = 1; t = dx; dx = dy; dy = -t; }  /* 第２象限 */
                else { area = 2; dx = -dx; dy = -dy; }  /* 第３象限 */
            }

            if (dy > dx) a = Math.PI / 2.0 - Math.Atan(dx / dy);                  /* ４５度以上 */
            else a = Math.Atan(dy / dx);                 /* ４５度以下 */

            return a + area * (Math.PI / 2.0);


            /*
                        // それぞれの線分を原点基準に調整
                        Vector3 vec1 = in_point2 - in_point1;
                        Vector3 vec2 = in_point3 - in_point1;
                        // 線分2を、線分1を基準とした位置に調整
                        Vector3 vec3 = vec2 - vec1;
                        // 角度算出
                        return Math.Abs(Mathf.Atan2(vec3.y, vec3.x) * Mathf.Rad2Deg);
            */
        }

        /// <summary>
        /// 線分の上下にバッファを付与し、その内側に頂点が入るか判定
        /// </summary>
        static public bool checkLineInPoint
        (
            Vector3 in_linPos1,     // [I] 入力線分１
            Vector3 in_linPos2,     // [I] 入力線分１ 
            Vector3 in_pos3,        // [I] 入力ポイント
            float in_areaSize       // [I] 線分に付与する範囲
        )
        {
            //-----------------------------------
            // ラインの周囲にバッファ付与
            //-----------------------------------
            List<Vector3> buffPos = new List<Vector3>();
            /*
                        // 単純に付与
                        buffPos.Add(new Vector3(in_linPos1.x - in_areaSize, in_linPos1.y - in_areaSize, 0.0f));
                        buffPos.Add(new Vector3(in_linPos1.x + in_areaSize, in_linPos1.y + in_areaSize, 0.0f));
                        buffPos.Add(new Vector3(in_linPos2.x - in_areaSize, in_linPos2.y - in_areaSize, 0.0f));
                        buffPos.Add(new Vector3(in_linPos2.x + in_areaSize, in_linPos2.y + in_areaSize, 0.0f));
            */

            Vector3 vec1, vec2;
            double angle, x, y;

            //--------------------------------------------------------------
            // ラインの前後には付与せず、ラインの上下にのみ付与する
            //--------------------------------------------------------------
            //------------------
            // １点目
            //------------------
            vec1 = CommonFunc.GetLinePos(in_linPos1, in_linPos2, in_areaSize);
            // in_linPos1を中心とした位置を算出
            vec2 = in_linPos1 - vec1;
            // in_linPos1を中心にvec1を-90度回転
            angle = Math.PI * -90 / 180;
            x = vec2.x * Math.Cos(angle) - vec2.y * Math.Sin(angle);
            y = vec2.x * Math.Sin(angle) + vec2.y * Math.Cos(angle);
            // 回転後の頂点
            buffPos.Add(new Vector3(in_linPos1.x + (float)x, in_linPos1.y + (float)y, 0.0f));
            //------------------
            // ２点目
            //------------------
            vec1 = CommonFunc.GetLinePos(in_linPos1, in_linPos2, in_areaSize);
            // in_linPos1を中心とした位置を算出
            vec2 = in_linPos1 - vec1;
            // in_linPos1を中心にvec1を90度回転
            angle = Math.PI * 90 / 180;
            x = vec2.x * Math.Cos(angle) - vec2.y * Math.Sin(angle);
            y = vec2.x * Math.Sin(angle) + vec2.y * Math.Cos(angle);
            // 回転後の頂点
            buffPos.Add(new Vector3(in_linPos1.x + (float)x, in_linPos1.y + (float)y, 0.0f));
            //------------------
            // ３点目
            //------------------
            vec1 = CommonFunc.GetLinePos(in_linPos2, in_linPos1, in_areaSize);
            // in_linPos2を中心とした位置を算出
            vec2 = in_linPos2 - vec1;
            // in_linPos2を中心にvec1を-90度回転
            angle = Math.PI * -90 / 180;
            x = vec2.x * Math.Cos(angle) - vec2.y * Math.Sin(angle);
            y = vec2.x * Math.Sin(angle) + vec2.y * Math.Cos(angle);
            // 回転後の頂点
            buffPos.Add(new Vector3(in_linPos2.x + (float)x, in_linPos2.y + (float)y, 0.0f));
            //------------------
            // ４点目
            //------------------
            vec1 = CommonFunc.GetLinePos(in_linPos2, in_linPos1, in_areaSize);
            // in_linPos2を中心とした位置を算出
            vec2 = in_linPos2 - vec1;
            // in_linPos2を中心にvec1を90度回転
            angle = Math.PI * 90 / 180;
            x = vec2.x * Math.Cos(angle) - vec2.y * Math.Sin(angle);
            y = vec2.x * Math.Sin(angle) + vec2.y * Math.Cos(angle);
            // 回転後の頂点
            buffPos.Add(new Vector3(in_linPos2.x + (float)x, in_linPos2.y + (float)y, 0.0f));


            //-----------------------------------
            // 内外判定
            //-----------------------------------
            if (checkPointInPoly(in_pos3, buffPos))
            {
                // 含まれている
                return true;
            }

            return false;
        }
    }
}
