using System.Collections.Generic;
using System.Linq;

namespace WpfLibPlayer
{
    /// <summary>
    /// モンテカルロ関連処理
    /// </summary>
    internal static class ToolsMC
    {
        /// <summary>
        /// デフォルトプレイアウト回数
        /// </summary>
        public static int DEFAULT_COUNT = 1024;

        /// <summary>
        /// デフォルトMCTSパラメータ
        /// </summary>
        public static double DEFAULT_PARAM = 0.3;

        #region 原始モンテカルロ

        /// <summary>
        /// 評価配列計算
        /// </summary>
        /// <param name="p">自分</param>
        /// <param name="o">相手</param>
        /// <param name="count">プレイアウト回数</param>
        /// <returns>評価配列</returns>
        public static double[] Compute(ulong p, ulong o, int count)
        {
            var res = Enumerable.Repeat(double.NaN, 64).ToArray();

            // 合法手取得
            var lm = Tools.LegalMove(p, o);

            // 試行数計算
            var pc = (lm == 0) ? 0 : (count / Tools.BitCount(lm) + 1);

            // プレイアウト
            ulong p_, o_, s;
            int r;
            while (lm != 0)
            {
                // 最下位ビット取得
                s = lm & (ulong)-(long)lm;
                // 最下位ビット削除
                lm ^= s;
                // 一時反転
                p_ = p;
                o_ = o;
                Tools.Flip(ref p_, ref o_, s);
                // 勝率計算
                r = 0;
                for (int j = 0; j < pc; j++)
                {
                    // 相手ターンの結果
                    if (PlayoutRand(o_, p_) < 0)
                    {
                        r++;
                    }
                }
                res[Tools.Bit2Pos(s)] = (double)r / pc;
            }

            return res;
        }

        #endregion

        #region モンテカルロ木探索

        /// <summary>
        /// 評価配列計算
        /// </summary>
        /// <param name="p">自分</param>
        /// <param name="o">相手</param>
        /// <param name="count">プレイアウト回数</param>
        /// <param name="param">MCTSパラメータ</param>
        /// <returns>評価配列</returns>
        public static double[] Compute(ulong p, ulong o, int count, double param)
        {
            var res = Enumerable.Repeat(double.NaN, 64).ToArray();

            // 現状の盤面をルートノードとして試行
            TreeNode tn = new TreeNode(-1, 0, p, o, param);
            for (int i = 0; i < count; i++)
            {
                tn.Action();
            }
            // 子ノードの試行数を取得
            foreach (TreeNode item in tn.children)
            {
                // ルートノード除外。パスノード評価対応。
                if (item.from >= 0 && item.from < 64)
                {
                    res[item.from] = (double)item.visit / count;
                }
            }

            return res;
        }

        /// <summary>
        /// 探索木
        /// </summary>
        private class TreeNode
        {
            // 子ノード
            public List<TreeNode> children = new List<TreeNode>();
            // 試行数
            public int visit = 0;
            // 評価値
            private double value = 0.0;

            // 親選択肢
            public int from;
            // ターン
            private readonly int turn;
            // 自分
            private readonly ulong p;
            // 相手
            private readonly ulong o;
            // MCTSパラメータ
            private readonly double param;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="from">親選択肢</param>
            /// <param name="turn">ターン</param>
            /// <param name="p">自分</param>
            /// <param name="o">相手</param>
            /// <param name="param">MCTSパラメータ</param>
            public TreeNode(int from, int turn, ulong p, ulong o, double param)
            {
                this.from = from;
                this.turn = turn;
                this.p = p;
                this.o = o;
                this.param = param;
            }

            /// <summary>
            /// 試行
            /// </summary>
            public void Action()
            {
                var visited = new List<TreeNode>();

                // 既存ノード選択
                TreeNode cur = this;
                visited.Add(cur);
                while (cur?.children.Count > 0)
                {
                    cur = cur.Select();
                    if (cur != null)
                    {
                        visited.Add(cur);
                    }
                }

                // 末端ノード展開
                TreeNode add = null;
                cur?.Expand();
                if (cur?.children.Count > 0)
                {
                    add = cur.Select();
                    if (add != null)
                    {
                        visited.Add(add);
                    }
                }

                // 評価値計算
                var delta = Simulate(add);

                // 評価値更新
                foreach (TreeNode item in visited)
                {
                    item.Update(delta);
                }
            }

            /// <summary>
            /// ノード選択
            /// </summary>
            /// <returns>選択ノード</returns>
            private TreeNode Select()
            {
                TreeNode selected = null;
                double max = double.MinValue;
                double v, uct;
                foreach (TreeNode item in children)
                {
                    if (item.visit == 0)
                    {
                        // 未探索ノード優先
                        uct = double.MaxValue;
                    }
                    else
                    {
                        // 自分ターンが基準。相手ターンは補数。
                        v = (item.turn % 2 == 1) ? item.value : (item.visit - item.value);
                        // UCB1計算
                        uct = (v / item.visit) + param * System.Math.Sqrt(2 * System.Math.Log(visit) / item.visit);
                    }
                    // 最大値ノード保存
                    if (uct > max)
                    {
                        selected = item;
                        max = uct;
                    }
                }
                return selected;
            }

            /// <summary>
            /// ノード展開
            /// </summary>
            private void Expand()
            {
                // 自分がパスならターン交替
                var lm = Tools.LegalMove(p, o);
                if (lm == 0)
                {
                    // 相手がパス以外なら同一親選択肢で子ノード追加
                    lm = Tools.LegalMove(o, p);
                    if (lm != 0)
                    {
                        children.Add(new TreeNode(from, turn + 1, o, p, param));
                        lm = 0;
                    }
                }
                // 各合法手展開
                ulong p_, o_, s;
                while (lm != 0)
                {
                    // 最下位ビット取得
                    s = lm & (ulong)-(long)lm;
                    // 最下位ビット削除
                    lm ^= s;
                    // 一時反転
                    p_ = p;
                    o_ = o;
                    Tools.Flip(ref p_, ref o_, s);
                    // 子ノード追加
                    children.Add(new TreeNode(Tools.Bit2Pos(s), turn + 1, o_, p_, param));
                }
            }

            /// <summary>
            /// 評価値計算
            /// </summary>
            /// <param name="node">対象ノード</param>
            /// <returns>評価値</returns>
            private double Simulate(TreeNode node)
            {
                int r;
                if (node == null)
                {
                    // 終局なら現ノードの石差
                    r = Tools.BitCount(p) - Tools.BitCount(o);
                    if (turn % 2 == 0)
                    {
                        r *= -1; // 今が相手ターン
                    }
                }
                else
                {
                    // 終局以外ならプレイアウトの結果
                    r = PlayoutRand(node.p, node.o);
                    if (node.turn % 2 == 1)
                    {
                        r *= -1; // 次が自分ターン
                    }
                }
                return (r > 0) ? 1.0 : (r == 0) ? 0.5 : 0.0;
            }

            /// <summary>
            /// 評価値更新
            /// </summary>
            /// <param name="delta">評価値</param>
            private void Update(double delta)
            {
                visit++;
                value += delta;
            }
        }

        #endregion

        #region 共通

        /// <summary>
        /// ランダムプレイアウト
        /// </summary>
        /// <param name="p">BitBoard：自分</param>
        /// <param name="o">BitBoard：相手</param>
        /// <returns>石差</returns>
        private static int PlayoutRand(ulong p, ulong o)
        {
            ulong sp, so;
            do
            {
                sp = Tools.GetRand(p, o);
                if (sp != 0)
                {
                    Tools.Flip(ref p, ref o, sp);
                }
                so = Tools.GetRand(o, p);
                if (so != 0)
                {
                    Tools.Flip(ref o, ref p, so);
                }
            } while (sp != 0 || so != 0);

            return Tools.BitCount(p) - Tools.BitCount(o);
        }

        #endregion
    }

}
