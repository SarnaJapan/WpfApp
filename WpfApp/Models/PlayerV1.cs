using OthelloInterface;

using System.Collections.Generic;
using System.Linq;

namespace WpfApp.Models
{
    /// <summary>
    /// デフォルトプレイヤー関連処理
    /// </summary>
    internal static class PlayerV1
    {
        /// <summary>
        /// デフォルトプレイヤーリスト取得
        /// </summary>
        /// <returns>新規デフォルトプレイヤーリスト</returns>
        public static List<IOthelloPlayerV1> GetPlayerList()
        {
            return new List<IOthelloPlayerV1>
            {
                new PlayerNullV1(),
                new PlayerRandV1(),
                new PlayerMaxCountV1(),
                new PlayerMinOpenV1(),
                new PlayerMCV1(),
            };
        }
    }

    /// <summary>
    /// 対人戦略プレイヤー
    /// </summary>
    /// 常に計算不可を返す。対戦選択肢対象外。
    internal class PlayerNullV1 : IOthelloPlayerV1
    {
        public string Name => "（なし）";
        public string Version => string.Format(Common.VERSION_FORMAT, 1, 0, 0, Common.OPTION_NOMATCH);
        public int Calc(int color, int[] data) => -1;
        public double[] Score(int color, int[] data) => null;
    }

    /// <summary>
    /// ランダム戦略プレイヤー
    /// </summary>
    /// 置石可能位置をランダムで返す。評価選択肢対象外。
    internal class PlayerRandV1 : IOthelloPlayerV1
    {
        public string Name => "ランダム";
        public string Version => string.Format(Common.VERSION_FORMAT, 1, 0, 1, Common.OPTION_NOEVAL);
        public int Calc(int color, int[] data) => ToolsV1.GetRand(color, data);
        public double[] Score(int color, int[] data) => null;
    }

    /// <summary>
    /// 最大取得数戦略プレイヤー
    /// </summary>
    /// 最大取得数となる置石位置を計算。
    internal class PlayerMaxCountV1 : IOthelloPlayerV1
    {
        public string Name => "最大取得数";
        public string Version => string.Format(Common.VERSION_FORMAT, 1, 0, 1, "");
        public int Calc(int color, int[] data)
        {
            var d = Score(color, data);
            var r = d.Where(n => !double.IsNaN(n));
            return r.Any() ? System.Array.IndexOf(d, r.Max()) : -1;
        }
        public double[] Score(int color, int[] data)
        {
            var res = new double[data.Length];
            for (int i = 0; i < res.Length; i++)
            {
                var count = ToolsV1.GetFlip(color, data, i).Count;
                if (count > 0)
                {
                    res[i] = count;
                }
                else
                {
                    res[i] = double.NaN;
                }
            }
            return res;
        }
    }

    /// <summary>
    /// 最小開放度戦略プレイヤー
    /// </summary>
    /// 最小開放度となる置石位置を計算。
    internal class PlayerMinOpenV1 : IOthelloPlayerV1
    {
        public string Name => "最小開放度";
        public string Version => string.Format(Common.VERSION_FORMAT, 1, 0, 1, "");
        public int Calc(int color, int[] data)
        {
            var d = Score(color, data);
            var r = d.Where(n => !double.IsNaN(n));
            return r.Any() ? System.Array.IndexOf(d, r.Min()) : -1;
        }
        public double[] Score(int color, int[] data)
        {
            var res = new double[data.Length];
            for (int i = 0; i < res.Length; i++)
            {
                var flip = ToolsV1.GetFlip(color, data, i);
                if (flip.Count > 0)
                {
                    // 置石位置はカウント対象外
                    flip.Remove(i);
                    // 反転位置周囲の空白をカウント
                    res[i] = ToolsV1.CountOpen(data, flip);
                }
                else
                {
                    res[i] = double.NaN;
                }
            }
            return res;
        }
    }

    /// <summary>
    /// 原始モンテカルロ戦略プレイヤー
    /// </summary>
    /// 原始モンテカルロ法で置石位置を計算。
    internal class PlayerMCV1 : IOthelloPlayerV1
    {
        public string Name => "モンテカルロ";
        public string Version => string.Format(Common.VERSION_FORMAT, 1, 0, 1, "");
        public int Calc(int color, int[] data)
        {
            var d = Score(color, data);
            var r = d.Where(n => !double.IsNaN(n));
            return r.Any() ? System.Array.IndexOf(d, r.Max()) : -1;
        }
        public double[] Score(int color, int[] data) => ToolsV1MC.Compute(color, data, Count);

        public int Count { get; set; } = ToolsV1MC.DEFAULT_COUNT;
    }

}
