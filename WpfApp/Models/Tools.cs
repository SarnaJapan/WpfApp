using System.Threading;
using WpfLib.OthelloInterface;

namespace WpfApp.Models
{
    /// <summary>
    /// 各種処理
    /// </summary>
    internal static class Tools
    {
        /// <summary>
        /// bit位置->ulong値変換
        /// </summary>
        /// <param name="pos">bit位置</param>
        /// <returns>ulong値</returns>
        public static ulong Pos2Bit(int pos)
        {
            return 0x8000000000000000 >> pos;
        }

        /// <summary>
        /// ulong値->bit位置変換
        /// </summary>
        /// <param name="bit">ulong値</param>
        /// <returns>bit位置</returns>
        public static int Bit2Pos(ulong bit)
        {
            int[] TABLE = {
                63,62, 4,61, 3,23, 9,60, 2,31,14,22, 8,44,28,59, 1,11,33,30,13,51,49,21, 7,47,36,43,27,40,19,58,
                 0, 5,24,10,32,15,45,29,12,34,52,50,48,37,41,20, 6,25,16,46,35,53,38,42,26,17,54,39,18,55,56,57,
            };

            if (bit == 0)
            {
                return -1;
            }
            bit &= (ulong)-(long)bit;

            return TABLE[(int)((bit * 0x03F566ED27179461) >> 58)];
        }

        /// <summary>
        /// ビット数計算
        /// </summary>
        /// <param name="t">対象</param>
        /// <returns>ビット数</returns>
        public static int BitCount(ulong t)
        {
            ulong r = (t & 0x5555555555555555) + ((t >> 1) & 0x5555555555555555);
            r = (r & 0x3333333333333333) + ((r >> 2) & 0x3333333333333333);
            r = (r & 0x0f0f0f0f0f0f0f0f) + ((r >> 4) & 0x0f0f0f0f0f0f0f0f);
            r = (r & 0x00ff00ff00ff00ff) + ((r >> 8) & 0x00ff00ff00ff00ff);
            r = (r & 0x0000ffff0000ffff) + ((r >> 16) & 0x0000ffff0000ffff);
            r = (r & 0x00000000ffffffff) + ((r >> 32) & 0x00000000ffffffff);

            return (int)r;
        }

        /// <summary>
        /// Array->BitBoard変換
        /// </summary>
        /// <param name="data">配列</param>
        /// <param name="p">自分</param>
        /// <param name="o">相手</param>
        /// <returns>結果</returns>
        public static bool Array2Bit(int[] data, ref ulong p, ref ulong o)
        {
            if (data.Length != 64)
            {
                return false;
            }
            p = 0;
            o = 0;
            for (int i = 0; i < 64; i++)
            {
                switch (data[i])
                {
                    case Common.BLACK:
                        p |= Pos2Bit(i);
                        break;
                    case Common.WHITE:
                        o |= Pos2Bit(i);
                        break;
                    default:
                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// BitBoard->Array変換
        /// </summary>
        /// <param name="p">自分</param>
        /// <param name="o">相手</param>
        /// <param name="data">配列</param>
        /// <returns>結果</returns>
        public static bool Bit2Array(ulong p, ulong o, int[] data)
        {
            if (data.Length != 64 || (p & o) != 0)
            {
                return false;
            }
            for (int i = 0; i < 64; i++)
            {
                var t = Pos2Bit(i);
                data[i] = Common.EMPTY;
                if ((p & t) != 0)
                {
                    data[i] = Common.BLACK;
                }
                if ((o & t) != 0)
                {
                    data[i] = Common.WHITE;
                }
            }

            return true;
        }

        /// <summary>
        /// 移動
        /// </summary>
        /// <param name="t">対象</param>
        /// <param name="d">方向(0-7)</param>
        /// <returns>結果</returns>
        private static ulong Transfer(ulong t, int d)
        {
            switch (d)
            {
                case 0: //上
                    return (t << 8) & 0xffffffffffffff00;
                case 1: //右上
                    return (t << 7) & 0x7f7f7f7f7f7f7f00;
                case 2: //右
                    return (t >> 1) & 0x7f7f7f7f7f7f7f7f;
                case 3: //右下
                    return (t >> 9) & 0x007f7f7f7f7f7f7f;
                case 4: //下
                    return (t >> 8) & 0x00ffffffffffffff;
                case 5: //左下
                    return (t >> 7) & 0x00fefefefefefefe;
                case 6: //左
                    return (t << 1) & 0xfefefefefefefefe;
                case 7: //左上
                    return (t << 9) & 0xfefefefefefefe00;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// 反転
        /// </summary>
        /// <param name="p">自分</param>
        /// <param name="o">相手</param>
        /// <param name="s">選択位置</param>
        public static void Flip(ref ulong p, ref ulong o, ulong s)
        {
            ulong r = 0;
            for (int i = 0; i < 8; i++)
            {
                ulong temp = 0;
                ulong mask = Transfer(s, i);
                // 隣が相手なら
                while ((mask != 0) && ((mask & o) != 0))
                {
                    // 反転候補
                    temp |= mask;
                    mask = Transfer(mask, i);
                }
                // その隣が自分なら
                if ((mask & p) != 0)
                {
                    // 反転可能
                    r |= temp;
                }
            }
            p ^= s | r;
            o ^= r;
        }

        /// <summary>
        /// 合法手生成
        /// </summary>
        /// <param name="p">自分</param>
        /// <param name="o">相手</param>
        /// <returns>合法手</returns>
        public static ulong LegalMove(ulong p, ulong o)
        {
            const ulong LR = 0x7e7e7e7e7e7e7e7e;
            const ulong UD = 0x00ffffffffffff00;
            const ulong DI = 0x007e7e7e7e7e7e00;
            ulong e = ~(p | o);
            ulong r = 0;

            ulong v = o & LR; // 左
            ulong t = v & (p << 1);
            t |= v & (t << 1); // 隣が相手なら
            t |= v & (t << 1); // その隣も相手なら
            t |= v & (t << 1);
            t |= v & (t << 1);
            t |= v & (t << 1); // 最大６地点
            r |= e & (t << 1); // その隣が空白なら合法手

            v = o & LR; // 右
            t = v & (p >> 1);
            t |= v & (t >> 1);
            t |= v & (t >> 1);
            t |= v & (t >> 1);
            t |= v & (t >> 1);
            t |= v & (t >> 1);
            r |= e & (t >> 1);

            v = o & UD; // 上
            t = v & (p << 8);
            t |= v & (t << 8);
            t |= v & (t << 8);
            t |= v & (t << 8);
            t |= v & (t << 8);
            t |= v & (t << 8);
            r |= e & (t << 8);

            v = o & UD; // 下
            t = v & (p >> 8);
            t |= v & (t >> 8);
            t |= v & (t >> 8);
            t |= v & (t >> 8);
            t |= v & (t >> 8);
            t |= v & (t >> 8);
            r |= e & (t >> 8);

            v = o & DI; // 右上
            t = v & (p << 7);
            t |= v & (t << 7);
            t |= v & (t << 7);
            t |= v & (t << 7);
            t |= v & (t << 7);
            t |= v & (t << 7);
            r |= e & (t << 7);

            v = o & DI; // 左下
            t = v & (p >> 7);
            t |= v & (t >> 7);
            t |= v & (t >> 7);
            t |= v & (t >> 7);
            t |= v & (t >> 7);
            t |= v & (t >> 7);
            r |= e & (t >> 7);

            v = o & DI; // 左上
            t = v & (p << 9);
            t |= v & (t << 9);
            t |= v & (t << 9);
            t |= v & (t << 9);
            t |= v & (t << 9);
            t |= v & (t << 9);
            r |= e & (t << 9);

            v = o & DI; // 右下
            t = v & (p >> 9);
            t |= v & (t >> 9);
            t |= v & (t >> 9);
            t |= v & (t >> 9);
            t |= v & (t >> 9);
            t |= v & (t >> 9);
            r |= e & (t >> 9);

            return r;
        }

        /// <summary>
        /// ランダム位置取得
        /// </summary>
        /// <param name="p">自分</param>
        /// <param name="o">相手</param>
        /// <returns>ランダム位置</returns>
        public static ulong GetRand(ulong p, ulong o)
        {
            var lm = LegalMove(p, o);
            var s = Common.Rand(BitCount(lm));
            for (int i = 0; i < s; i++)
            {
                // 最下位ビット削除
                lm &= lm - 1;
            }
            // 最下位ビット取得
            lm &= (ulong)-(long)lm;

            return lm;
        }

        /// <summary>
        /// 開放度計算
        /// </summary>
        /// <param name="f">反転位置</param>
        /// <param name="e">空白位置</param>
        /// <returns>開放度</returns>
        public static int CountOpen(ulong f, ulong e)
        {
            int r = BitCount(Transfer(f, 0) & e);
            r += BitCount(Transfer(f, 1) & e);
            r += BitCount(Transfer(f, 2) & e);
            r += BitCount(Transfer(f, 3) & e);
            r += BitCount(Transfer(f, 4) & e);
            r += BitCount(Transfer(f, 5) & e);
            r += BitCount(Transfer(f, 6) & e);
            r += BitCount(Transfer(f, 7) & e);

            return r;
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
        public static string MatchStatistics(System.IProgress<string> progress, CancellationToken token, IOthelloPlayer pp, IOthelloPlayer po, int count)
        {
            int[] res = { 0, 0, 0, };

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            try
            {
                for (int i = 0; i < count && !token.IsCancellationRequested; i++)
                {
                    var r = Playout(pp, po, Common.BB_BLACK, Common.BB_WHITE);
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
        /// <param name="p">BitBoard：自分</param>
        /// <param name="o">BitBoard：相手</param>
        /// <exception cref="System.ArgumentOutOfRangeException">計算結果矛盾</exception>
        /// <returns>石差</returns>
        public static int Playout(IOthelloPlayer pp, IOthelloPlayer po, ulong p, ulong o)
        {
            ulong lp, lo, sp, so;
            do
            {
                lp = LegalMove(p, o);
                if (lp != 0)
                {
                    sp = pp.Calc(p, o);
                    if ((sp & lp) == 0)
                    {
                        throw new System.ArgumentOutOfRangeException(pp.ToString());
                    }
                    Flip(ref p, ref o, sp);
                }
                lo = LegalMove(o, p);
                if (lo != 0)
                {
                    so = po.Calc(o, p);
                    if ((so & lo) == 0)
                    {
                        throw new System.ArgumentOutOfRangeException(po.ToString());
                    }
                    Flip(ref o, ref p, so);
                }
            } while (lp != 0 || lo != 0);

            return BitCount(p) - BitCount(o);
        }
    }

}
