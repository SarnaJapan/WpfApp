using System.Collections.Generic;
using System.Linq;

namespace WpfLibPlayerV1
{
    /// <summary>
    /// モンテカルロ関連処理
    /// </summary>
    internal static class ToolsV1MC
    {
        /// <summary>
        /// デフォルトプレイアウト回数
        /// </summary>
        public static int DEFAULT_COUNT = 64;

        #region 原始モンテカルロ

        /// <summary>
        /// 評価配列計算
        /// </summary>
        /// <param name="cp">石色</param>
        /// <param name="data">盤面</param>
        /// <param name="count">プレイアウト回数</param>
        /// <returns>評価配列</returns>
        public static double[] Compute(int cp, int[] data, int count)
        {
            var res = Enumerable.Repeat(double.NaN, data.Length).ToArray();

            // 合法手取得
            var lmlist = new List<int>[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                lmlist[i] = ToolsV1.GetFlip(cp, data, i);
            }

            // 試行数計算
            int lmc = lmlist.Count(n => n.Count > 0);
            var poc = (lmc == 0) ? 0 : (count / lmc + 1);

            // プレイアウト
            int co = cp * -1;
            int r;
            var tmp = new int[lmlist.Length];
            for (int i = 0; i < lmlist.Length; i++)
            {
                if (lmlist[i].Count > 0)
                {
                    // 勝率計算
                    r = 0;
                    for (int j = 0; j < poc; j++)
                    {
                        // 一時反転
                        System.Array.Copy(data, tmp, data.Length);
                        ToolsV1.FlipData(cp, tmp, lmlist[i]);
                        // 相手ターンの結果
                        if (PlayoutRand(co, tmp) < 0)
                        {
                            r++;
                        }
                    }
                    res[i] = (double)r / poc;
                }
            }

            return res;
        }

        #endregion

        #region 共通

        /// <summary>
        /// ランダムプレイアウト
        /// </summary>
        /// <param name="cp">石色</param>
        /// <param name="data">盤面</param>
        /// <returns>石差</returns>
        private static int PlayoutRand(int cp, int[] data)
        {
            int co = cp * -1;
            int sp, so;
            do
            {
                sp = ToolsV1.GetRand(cp, data);
                if (sp != -1)
                {
                    var fp = ToolsV1.GetFlip(cp, data, sp);
                    ToolsV1.FlipData(cp, data, fp);
                }
                so = ToolsV1.GetRand(co, data);
                if (so != -1)
                {
                    var fo = ToolsV1.GetFlip(co, data, so);
                    ToolsV1.FlipData(co, data, fo);
                }
            } while (sp != -1 || so != -1);

            return data.Sum() * cp;
        }

        #endregion
    }

}
