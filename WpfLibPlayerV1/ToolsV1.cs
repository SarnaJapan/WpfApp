using System.Collections.Generic;
using System.Linq;

namespace WpfLibPlayerV1
{
    /// <summary>
    /// 各種処理
    /// </summary>
    internal static class ToolsV1
    {
        /// <summary>
        /// 反転リスト取得
        /// </summary>
        /// <param name="color">石色</param>
        /// <param name="data">盤面</param>
        /// <param name="pos">置石位置</param>
        /// <returns>反転リスト</returns>
        public static List<int> GetFlip(int color, int[] data, int pos)
        {
            var res = new HashSet<int>();
            if (pos < 0 || pos >= data.Length || data[pos] != Common.EMPTY)
            {
                return res.ToList();
            }
            int enemy = color * -1;
            int basex = pos % Common.SIZE;
            int basey = pos / Common.SIZE;

            // 周囲８方向を確認
            void judge(int dy, int dx)
            {
                int x = basex;
                int y = basey;
                while (true)
                {
                    // 隣が盤外の場合は終了
                    if (y + dy < 0 || y + dy >= Common.SIZE || x + dx < 0 || x + dx >= Common.SIZE)
                    {
                        break;
                    }
                    // 隣が相手の場合は隣に移動して再確認
                    else if (data[(y + dy) * Common.SIZE + (x + dx)] == enemy)
                    {
                        x += dx;
                        y += dy;
                    }
                    // 初期位置以外で隣が自分の場合は反転可能
                    else if (data[(y + dy) * Common.SIZE + (x + dx)] == color && (y * Common.SIZE + x) != pos)
                    {
                        // 初期位置から確認位置までを反転位置リストに追加
                        for (int k = 0; (basey + dy * k != y + dy) || (basex + dx * k != x + dx); k++)
                        {
                            res.Add((basey + dy * k) * Common.SIZE + (basex + dx * k));
                        }
                        break;
                    }
                    // 挟めないため終了
                    else
                    {
                        break;
                    }
                }
            }
            judge(-1, -1);
            judge(-1, 0);
            judge(-1, 1);
            judge(0, -1);
            judge(0, 1);
            judge(1, -1);
            judge(1, 0);
            judge(1, 1);

            return res.ToList();
        }

        /// <summary>
        /// 置石可否確認
        /// </summary>
        /// <param name="color">石色</param>
        /// <param name="data">盤面</param>
        /// <returns>置石可否</returns>
        public static bool CheckNext(int color, int[] data)
        {
            bool res = false;

            for (int i = 0; i < data.Length; i++)
            {
                if (GetFlip(color, data, i).Count > 0)
                {
                    res = true;
                    break;
                }
            }

            return res;
        }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="data">盤面</param>
        public static void InitData(int[] data)
        {
            int p1 = Common.SIZE / 2 - 1;
            int p2 = Common.SIZE / 2;

            for (int i = 0; i < data.Length; i++) { data[i] = Common.EMPTY; }
            data[p1 * Common.SIZE + p1] = Common.WHITE;
            data[p1 * Common.SIZE + p2] = Common.BLACK;
            data[p2 * Common.SIZE + p1] = Common.BLACK;
            data[p2 * Common.SIZE + p2] = Common.WHITE;
        }

        /// <summary>
        /// 反転
        /// </summary>
        /// <param name="color">石色</param>
        /// <param name="data">盤面</param>
        /// <param name="flip">反転リスト</param>
        public static void FlipData(int color, int[] data, List<int> flip)
        {
            foreach (int s in flip)
            {
                data[s] = color;
            }
        }

        /// <summary>
        /// ランダム位置取得
        /// </summary>
        /// <param name="color">石色</param>
        /// <param name="data">盤面</param>
        /// <returns>ランダム位置</returns>
        public static int GetRand(int color, int[] data)
        {
            var d = new List<int>();

            for (int i = 0; i < data.Length; i++)
            {
                if (GetFlip(color, data, i).Count > 0)
                {
                    d.Add(i);
                }
            }

            return (d.Count == 0) ? -1 : d[Common.Rand(d.Count)];
        }

        /// <summary>
        /// 開放度計算
        /// </summary>
        /// <param name="data">盤面</param>
        /// <param name="flip">反転リスト</param>
        /// <returns>開放度</returns>
        public static int CountOpen(int[] data, List<int> flip)
        {
            int res = 0;

            // 全ての反転位置を確認
            foreach (var item in flip)
            {
                // 周囲８方向を確認
                void count(int dy, int dx)
                {
                    int x = item % Common.SIZE;
                    int y = item / Common.SIZE;
                    // 隣が盤内の場合
                    if (y + dy >= 0 && y + dy < Common.SIZE && x + dx >= 0 && x + dx < Common.SIZE)
                    {
                        // 空白数追加
                        if (data[(y + dy) * Common.SIZE + (x + dx)] == Common.EMPTY)
                        {
                            res++;
                        }
                    }
                }
                count(-1, -1);
                count(-1, 0);
                count(-1, 1);
                count(0, -1);
                count(0, 1);
                count(1, -1);
                count(1, 0);
                count(1, 1);
            }

            return res;
        }
    }

}
