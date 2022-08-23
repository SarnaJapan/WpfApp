using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WpfLib.OthelloInterface;

namespace WpfApp.Models
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

            // 周囲８方向を確認
            bool judge(int p, int d)
            {
                // 隣が盤外の場合は終了
                if ((p + d < 0) || (p + d >= Common.SIZE * Common.SIZE) || (p % Common.SIZE == 0 && (d + 1) % Common.SIZE == 0) || ((p + 1) % Common.SIZE == 0 && (d - 1) % Common.SIZE == 0))
                {
                    return false;
                }
                // 隣が相手の場合は隣に移動して再帰確認
                else if (data[p + d] == enemy)
                {
                    // 反転可能なら移動経路も反転可能
                    if (judge(p + d, d))
                    {
                        res.Add(p);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                // 初期位置以外で隣が自分の場合は反転可能
                else if (data[p + d] == color && p != pos)
                {
                    res.Add(p);
                    return true;
                }
                // 挟めないため終了
                else
                {
                    return false;
                }
            }
            judge(pos, -Common.SIZE - 1);
            judge(pos, -Common.SIZE);
            judge(pos, -Common.SIZE + 1);
            judge(pos, -1);
            judge(pos, 1);
            judge(pos, Common.SIZE - 1);
            judge(pos, Common.SIZE);
            judge(pos, Common.SIZE + 1);

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

            return (d.Count == 0) ? -1 : d[Common.R.Next(d.Count)];
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
            foreach (var p in flip)
            {
                // 周囲８方向を確認
                void count(int d)
                {
                    // 隣が盤外の場合は終了
                    if ((p + d < 0) || (p + d >= Common.SIZE * Common.SIZE) || (p % Common.SIZE == 0 && (d + 1) % Common.SIZE == 0) || ((p + 1) % Common.SIZE == 0 && (d - 1) % Common.SIZE == 0))
                    {
                        return;
                    }
                    // 空白数追加
                    if (data[p + d] == Common.EMPTY)
                    {
                        res++;
                    }
                }
                count(-Common.SIZE - 1);
                count(-Common.SIZE);
                count(-Common.SIZE + 1);
                count(-1);
                count(1);
                count(Common.SIZE - 1);
                count(Common.SIZE);
                count(Common.SIZE + 1);
            }

            return res;
        }

        /// <summary>
        /// 対戦処理
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="token"></param>
        /// <param name="pp">プレイヤー：自分</param>
        /// <param name="po">プレイヤー：相手</param>
        /// <param name="count">対戦回数</param>
        /// <returns>処理時間</returns>
        public static string MatchStatistics(System.IProgress<string> progress, CancellationToken token, IOthelloPlayerV1 pp, IOthelloPlayerV1 po, int count)
        {
            int[] res = { 0, 0, 0, };
            var data = new int[Common.SIZE * Common.SIZE];

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            try
            {
                for (int i = 0; i < count && !token.IsCancellationRequested; i++)
                {
                    InitData(data);
                    var r = Playout(pp, po, Common.BLACK, data);
                    res[(r > 0) ? 1 : (r < 0) ? 2 : 0]++;

                    progress.Report($"{i}");
                }
            }
            catch (System.ArgumentOutOfRangeException ex)
            {
                return ex.Message;
            }
            sw.Stop();

            return $"{res[1]}/{res[0]}/{res[2]} @ {sw.ElapsedMilliseconds} ms";
        }

        /// <summary>
        /// プレイアウト
        /// </summary>
        /// <param name="pp">プレイヤー：自分</param>
        /// <param name="po">プレイヤー：相手</param>
        /// <param name="cp">石色</param>
        /// <param name="data">盤面</param>
        /// <exception cref="System.ArgumentOutOfRangeException">計算結果矛盾</exception>
        /// <returns>石差</returns>
        public static int Playout(IOthelloPlayerV1 pp, IOthelloPlayerV1 po, int cp, int[] data)
        {
            int co = cp * -1;
            int sp, so;
            do
            {
                sp = pp.Calc(cp, data);
                if (sp != -1)
                {
                    var fp = GetFlip(cp, data, sp);
                    if (fp.Count == 0)
                    {
                        throw new System.ArgumentOutOfRangeException(pp.ToString());
                    }
                    FlipData(cp, data, fp);
                }
                so = po.Calc(co, data);
                if (so != -1)
                {
                    var fo = GetFlip(co, data, so);
                    if (fo.Count == 0)
                    {
                        throw new System.ArgumentOutOfRangeException(po.ToString());
                    }
                    FlipData(co, data, fo);
                }
            } while (sp != -1 || so != -1);

            return data.Sum() * cp;
        }
    }

}
