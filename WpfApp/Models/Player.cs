using OthelloInterface;

using System.Linq;

namespace WpfApp.Models
{
    /// <summary>
    /// 対人戦略プレイヤー
    /// </summary>
    /// 常に計算不可を返す。対戦選択肢対象外。
    internal class PlayerNull : IOthelloPlayer
    {
        public string Name { get; set; } = "（なし）";
        public string Version => string.Format(Common.VERSION_FORMAT, 2, 0, 0, Common.OPTION_NOMATCH);
        public ulong Calc(ulong p, ulong o) => 0;
        public double[] Score(ulong p, ulong o) => null;
    }

    /// <summary>
    /// ランダム戦略プレイヤー
    /// </summary>
    /// 置石可能位置をランダムで返す。評価選択肢対象外。
    internal class PlayerRand : IOthelloPlayer
    {
        public string Name { get; set; } = "ランダム";
        public string Version => string.Format(Common.VERSION_FORMAT, 2, 0, 1, Common.OPTION_NOEVAL);
        public ulong Calc(ulong p, ulong o) => Tools.GetRand(p, o);
        public double[] Score(ulong p, ulong o) => null;
    }

    /// <summary>
    /// 最大取得数戦略プレイヤー
    /// </summary>
    /// 最大取得数となる置石位置を計算。
    internal class PlayerMaxCount : IOthelloPlayer
    {
        public string Name { get; set; } = "最大取得数";
        public string Version => string.Format(Common.VERSION_FORMAT, 2, 0, 1, "");
        public ulong Calc(ulong p, ulong o)
        {
            var d = Score(p, o);
            var r = d.Where(n => !double.IsNaN(n));
            return r.Any() ? Tools.Pos2Bit(System.Array.IndexOf(d, r.Max())) : 0;
        }
        public double[] Score(ulong p, ulong o)
        {
            var res = new double[64];
            var lm = Tools.LegalMove(p, o);
            ulong p_, o_, s;
            for (int i = 0; i < 64; i++)
            {
                s = Tools.Pos2Bit(i);
                if ((lm & s) != 0)
                {
                    p_ = p;
                    o_ = o;
                    // 一時反転
                    Tools.Flip(ref p_, ref o_, s);
                    // 自分の石をカウント
                    res[i] = Tools.BitCount(p_);
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
    internal class PlayerMinOpen : IOthelloPlayer
    {
        public string Name { get; set; } = "最小開放度";
        public string Version => string.Format(Common.VERSION_FORMAT, 2, 0, 1, "");
        public ulong Calc(ulong p, ulong o)
        {
            var d = Score(p, o);
            var r = d.Where(n => !double.IsNaN(n));
            return r.Any() ? Tools.Pos2Bit(System.Array.IndexOf(d, r.Min())) : 0;
        }
        public double[] Score(ulong p, ulong o)
        {
            var res = new double[64];
            var lm = Tools.LegalMove(p, o);
            var e = ~(p | o);
            ulong p_, o_, s;
            for (int i = 0; i < 64; i++)
            {
                s = Tools.Pos2Bit(i);
                if ((lm & s) != 0)
                {
                    p_ = p;
                    o_ = o;
                    // 一時反転
                    Tools.Flip(ref p_, ref o_, s);
                    // 反転位置周囲の空白をカウント
                    res[i] = Tools.CountOpen(p_ & o, e);
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
    internal class PlayerMC : IOthelloPlayer
    {
        public string Name { get; set; } = "モンテカルロ";
        public string Version => string.Format(Common.VERSION_FORMAT, 2, 0, 1, "");
        public ulong Calc(ulong p, ulong o)
        {
            var d = Score(p, o);
            var r = d.Where(n => !double.IsNaN(n));
            return r.Any() ? Tools.Pos2Bit(System.Array.IndexOf(d, r.Max())) : 0;
        }
        public double[] Score(ulong p, ulong o) => ToolsMC.Compute(p, o, Count);

        public int Count { get; set; } = ToolsMC.DEFAULT_COUNT;
    }

    /// <summary>
    /// モンテカルロ木探索戦略プレイヤー
    /// </summary>
    /// モンテカルロ木探索法で置石位置を計算。
    internal class PlayerMCTS : IOthelloPlayer
    {
        public string Name { get; set; } = "モンテカルロ木探索";
        public string Version => string.Format(Common.VERSION_FORMAT, 2, 0, 1, "");
        public ulong Calc(ulong p, ulong o)
        {
            var d = Score(p, o);
            var r = d.Where(n => !double.IsNaN(n));
            return r.Any() ? Tools.Pos2Bit(System.Array.IndexOf(d, r.Max())) : 0;
        }
        public double[] Score(ulong p, ulong o) => ToolsMC.Compute(p, o, Count, Param);

        public int Count { get; set; } = ToolsMC.DEFAULT_COUNT;
        public double Param { get; set; } = ToolsMC.DEFAULT_PARAM;
    }

    /// <summary>
    /// Accord DBN戦略プレイヤー
    /// </summary>
    /// Accord DBNで置石位置を計算。
    internal class PlayerAccord : IOthelloPlayer
    {
        public string Name { get; set; } = "Accord DBN";
        public string Version => string.Format(Common.VERSION_FORMAT, 2, 0, 0, "");
        public ulong Calc(ulong p, ulong o)
        {
            // Scoreは盤面全体が対象のため合法手抽出が必要
            var d = Score(p, o);
            var lm = Tools.LegalMove(p, o);
            for (int i = 0; i < 64; i++)
            {
                if ((lm & Tools.Pos2Bit(i)) == 0)
                {
                    d[i] = double.NaN;
                }
            }
            var r = d.Where(n => !double.IsNaN(n));
            return (r.Count() == 0) ? 0 : Tools.Pos2Bit(System.Array.IndexOf(d, r.Max()));
        }
        public double[] Score(ulong p, ulong o) => ToolsMLAccord.Compute(p, o);
    }

    /// <summary>
    /// Kelp MLP戦略プレイヤー
    /// </summary>
    /// Kelp MLPで置石位置を計算。
    internal class PlayerKelp : IOthelloPlayer
    {
        public string Name { get; set; } = "Kelp MLP";
        public string Version => string.Format(Common.VERSION_FORMAT, 2, 0, 0, "");
        public ulong Calc(ulong p, ulong o)
        {
            // Scoreは盤面全体が対象のため合法手抽出が必要
            var d = Score(p, o);
            var lm = Tools.LegalMove(p, o);
            for (int i = 0; i < 64; i++)
            {
                if ((lm & Tools.Pos2Bit(i)) == 0)
                {
                    d[i] = double.NaN;
                }
            }
            var r = d.Where(n => !double.IsNaN(n));
            return (r.Count() == 0) ? 0 : Tools.Pos2Bit(System.Array.IndexOf(d, r.Max()));
        }
        public double[] Score(ulong p, ulong o) => ToolsMLKelp.Compute(p, o, Param);

        public int Param { get; set; } = 0;
    }

}
