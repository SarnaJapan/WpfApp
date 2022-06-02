using OthelloInterface;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace WpfApp.Models
{
    /// <summary>
    /// 棋譜関連処理
    /// </summary>
    internal static class ToolsKF
    {
        /// <summary>
        /// 生成棋譜ファイル
        /// </summary>
        private static readonly string FILE_VS = "log_vs.dat";

        /// <summary>
        /// 変換棋譜ファイル
        /// </summary>
        private static readonly string FILE_WT = "log_wt.dat";

        /// <summary>
        /// 正規化棋譜ファイル
        /// </summary>
        private static readonly string FILE_OK = "log_ok.dat";

        /// <summary>
        /// エラー棋譜ファイル
        /// </summary>
        private static readonly string FILE_NG = "log_ng.dat";

        /// <summary>
        /// 棋譜生成処理
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="token"></param>
        /// <param name="pp">プレイヤー：自分</param>
        /// <param name="po">プレイヤー：相手</param>
        /// <param name="count">対戦回数</param>
        /// <param name="path"></param>
        /// <returns>処理数</returns>
        public static string CreateLog(System.IProgress<string> progress, CancellationToken token, IOthelloPlayer pp, IOthelloPlayer po, int count, string path)
        {
            var list = new List<string>();
            int[] log;
            for (int i = 0; i < count && !token.IsCancellationRequested; i++)
            {
                log = PlayoutLog(pp, po);
                if (log.Length > 0)
                {
                    list.Add(string.Join(",", log));
                }
                log = PlayoutLog(po, pp);
                if (log.Length > 0)
                {
                    list.Add(string.Join(",", log));
                }

                progress.Report($"{i}");
            }
            try
            {
                Common.SaveLogList(Path.Combine(path, FILE_VS), list);
            }
            catch (System.Exception ex)
            {
                return ex.Message;
            }

            return $"{list.Count / 2}";
        }

        /// <summary>
        /// 棋譜変換処理
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="token"></param>
        /// <param name="param">[string path]</param>
        /// <returns>処理数</returns>
        public static string ConvertLog(System.IProgress<string> progress, CancellationToken token, object[] param)
        {
            // パラメータ確認
            string path;
            if (param.Length == 1)
            {
                path = (string)param[0];
            }
            else
            {
                return "Invalid parameter.";
            }

            int[] res = { 0, 0, 0, };
            int[] log;
            try
            {
                var list = new List<string>();
                var wtb = Directory.GetFiles(Path.Combine(path, "wtb"), "*.wtb");
                if (wtb.Length == 0)
                {
                    return "File not found.";
                }
                foreach (var file in wtb)
                {
                    foreach (var item in LoadWTB(file))
                    {
                        // 変換（11=A1～88=H8）
                        for (int i = 0; i < item.Length; i++)
                        {
                            item[i] = (item[i] / 10 - 1) * 8 + item[i] % 10 - 1;
                        }
                        // 保存（PASS=-1）
                        log = Norm(item, out int r);
                        if (log.Length > 0)
                        {
                            list.Add(string.Join(",", log));
                            res[(r > 0) ? 1 : (r < 0) ? 2 : 0]++;
                        }
                    }

                    progress.Report($"{res[1]}/{res[0]}/{res[2]}");
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                }
                Common.SaveLogList(Path.Combine(path, FILE_WT), list);
            }
            catch (System.Exception ex)
            {
                return ex.Message;
            }

            return $"{res[1]}/{res[0]}/{res[2]}";
        }

        /// <summary>
        /// 棋譜確認処理
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="token"></param>
        /// <param name="param">[string path]</param>
        /// <returns>処理数</returns>
        public static string CheckLog(System.IProgress<string> progress, CancellationToken token, object[] param)
        {
            // パラメータ確認
            string path;
            try
            {
                path = (string)param[0];
            }
            catch (System.Exception)
            {
                return "Invalid parameter.";
            }

            int[] res = { 0, 0, 0, };
            try
            {
                var list = new List<string>();
                var err = new List<string>();
                var tmp = LoadKF(Path.Combine(path, FILE_VS));
                if (tmp.Count == 0)
                {
                    return "File not found.";
                }
                foreach (var item in tmp)
                {
                    var log = Norm(item, out int r);
                    if (log.Length > 0)
                    {
                        log = (log[0] == 26) ? Conv(log, 1) : (log[0] == 44) ? Conv(log, 2) : (log[0] == 19) ? Conv(log, 3) : log;
                        list.Add(string.Join(",", log));
                        res[(r > 0) ? 1 : (r < 0) ? 2 : 0]++;
                    }
                    else
                    {
                        err.Add(string.Join(",", item));
                    }

                    if (list.Count % 100 == 0)
                    {
                        progress.Report($"{res[1]}/{res[0]}/{res[2]}");
                    }
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                }
                Common.SaveLogList(Path.Combine(path, FILE_OK), list.Distinct().ToList());
                Common.SaveLogList(Path.Combine(path, FILE_NG), err.Distinct().ToList());
            }
            catch (System.Exception ex)
            {
                return ex.Message;
            }

            return $"{res[1]}/{res[0]}/{res[2]}";
        }

        /// <summary>
        /// 棋譜取得プレイアウト
        /// </summary>
        /// <param name="pp">プレイヤー：自分</param>
        /// <param name="po">プレイヤー：相手</param>
        /// <returns>正規化棋譜</returns>
        private static int[] PlayoutLog(IOthelloPlayer pp, IOthelloPlayer po)
        {
            var res = new List<int>();
            ulong p = Common.BB_BLACK;
            ulong o = Common.BB_WHITE;

            ulong lp, lo, sp, so;
            do
            {
                lp = Tools.LegalMove(p, o);
                if (lp != 0)
                {
                    sp = pp.Calc(p, o);
                    if ((sp & lp) == 0)
                    {
                        res.Clear();
                        break;
                    }
                    Tools.Flip(ref p, ref o, sp);
                    res.Add(Tools.Bit2Pos(sp));
                }
                else
                {
                    res.Add(-1);
                }
                lo = Tools.LegalMove(o, p);
                if (lo != 0)
                {
                    so = po.Calc(o, p);
                    if ((so & lo) == 0)
                    {
                        res.Clear();
                        break;
                    }
                    Tools.Flip(ref o, ref p, so);
                    res.Add(Tools.Bit2Pos(so));
                }
                else
                {
                    res.Add(-1);
                }
            } while (lp != 0 || lo != 0);

            while (res.Count > 0 && res.Last() == -1)
            {
                res.RemoveAt(res.Count - 1);
            }

            return res.ToArray();
        }

        /// <summary>
        /// 棋譜相似変換
        /// </summary>
        /// <param name="log">棋譜</param>
        /// <param name="type">相似種別(0-7)</param>
        /// <returns>結果</returns>
        internal static int[] Conv(int[] log, int type)
        {
            int[,] T = new int[8, 64] {
                // 回転
                { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, },
                { 63, 62, 61, 60, 59, 58, 57, 56, 55, 54, 53, 52, 51, 50, 49, 48, 47, 46, 45, 44, 43, 42, 41, 40, 39, 38, 37, 36, 35, 34, 33, 32, 31, 30, 29, 28, 27, 26, 25, 24, 23, 22, 21, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, },
                { 0, 8, 16, 24, 32, 40, 48, 56, 1, 9, 17, 25, 33, 41, 49, 57, 2, 10, 18, 26, 34, 42, 50, 58, 3, 11, 19, 27, 35, 43, 51, 59, 4, 12, 20, 28, 36, 44, 52, 60, 5, 13, 21, 29, 37, 45, 53, 61, 6, 14, 22, 30, 38, 46, 54, 62, 7, 15, 23, 31, 39, 47, 55, 63, },
                { 63, 55, 47, 39, 31, 23, 15, 7, 62, 54, 46, 38, 30, 22, 14, 6, 61, 53, 45, 37, 29, 21, 13, 5, 60, 52, 44, 36, 28, 20, 12, 4, 59, 51, 43, 35, 27, 19, 11, 3, 58, 50, 42, 34, 26, 18, 10, 2, 57, 49, 41, 33, 25, 17, 9, 1, 56, 48, 40, 32, 24, 16, 8, 0, },
                // 反転
                { 7, 6, 5, 4, 3, 2, 1, 0, 15, 14, 13, 12, 11, 10, 9, 8, 23, 22, 21, 20, 19, 18, 17, 16, 31, 30, 29, 28, 27, 26, 25, 24, 39, 38, 37, 36, 35, 34, 33, 32, 47, 46, 45, 44, 43, 42, 41, 40, 55, 54, 53, 52, 51, 50, 49, 48, 63, 62, 61, 60, 59, 58, 57, 56, },
                { 56, 57, 58, 59, 60, 61, 62, 63, 48, 49, 50, 51, 52, 53, 54, 55, 40, 41, 42, 43, 44, 45, 46, 47, 32, 33, 34, 35, 36, 37, 38, 39, 24, 25, 26, 27, 28, 29, 30, 31,16, 17, 18, 19, 20, 21, 22, 23, 8,  9, 10, 11, 12, 13, 14, 15, 0, 1, 2, 3, 4, 5, 6, 7, },
                { 7, 15, 23, 31, 39, 47, 55, 63, 6, 14, 22, 30, 38, 46, 54, 62, 5, 13, 21, 29, 37, 45, 53, 61, 4, 12, 20, 28, 36, 44, 52, 60, 3, 11, 19, 27, 35, 43, 51, 59, 2, 10, 18, 26, 34, 42, 50, 58, 1, 9, 17, 25, 33, 41, 49, 57, 0, 8, 16, 24, 32, 40, 48, 56, },
                { 56, 48, 40, 32, 24, 16, 8, 0, 57, 49, 41, 33, 25, 17, 9, 1, 58, 50, 42, 34, 26, 18, 10, 2, 59, 51, 43, 35, 27, 19, 11, 3, 60, 52, 44, 36, 28, 20, 12, 4, 61, 53, 45, 37, 29, 21, 13, 5, 62, 54, 46, 38, 30, 22, 14, 6, 63, 55, 47, 39, 31, 23, 15, 7, },
            };
            var res = Enumerable.Repeat(-1, log.Length).ToArray();

            if (type >= 0 && type < 8)
            {
                for (int i = 0; i < res.Length; i++)
                {
                    res[i] = (log[i] >= 0 && log[i] < 64) ? T[type, log[i]] : -1;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("type=" + type + "\n" + string.Join(",", log));
            }

            return res;
        }

        /// <summary>
        /// 棋譜正規化
        /// </summary>
        /// <param name="log">棋譜</param>
        /// <param name="diff">石差</param>
        /// <returns>結果</returns>
        internal static int[] Norm(int[] log, out int diff)
        {
            var res = new List<int>();
            ulong p = Common.BB_BLACK;
            ulong o = Common.BB_WHITE;

            ulong lx, sx;
            for (int i = 0; i < log.Length; i++)
            {
                if (log[i] >= 0 && log[i] < 64)
                {
                    lx = Tools.LegalMove(p, o);
                    if (lx != 0)
                    {
                        sx = Tools.Pos2Bit(log[i]);
                        if ((sx & lx) != 0)
                        {
                            Tools.Flip(ref p, ref o, sx);
                            res.Add(log[i]);
                        }
                        else
                        {
                            // 矛盾棋譜
                            break;
                        }
                    }
                    else
                    {
                        // 省略パス
                        res.Add(-1);
                        (o, p) = (p, o);
                        lx = Tools.LegalMove(p, o);
                        if (lx != 0)
                        {
                            sx = Tools.Pos2Bit(log[i]);
                            if ((sx & lx) != 0)
                            {
                                Tools.Flip(ref p, ref o, sx);
                                res.Add(log[i]);
                            }
                            else
                            {
                                // 矛盾棋譜
                                break;
                            }
                        }
                        else
                        {
                            // 終了
                            break;
                        }
                    }
                }
                else
                {
                    lx = Tools.LegalMove(p, o);
                    if (lx == 0)
                    {
                        // 有効パス
                        res.Add(-1);
                    }
                    else
                    {
                        // 矛盾パス
                        break;
                    }
                }
                (o, p) = (p, o);
            }

            if (Tools.LegalMove(p, o) == 0 && Tools.LegalMove(o, p) == 0)
            {
                while (res.Count > 0 && res.Last() == -1)
                {
                    res.RemoveAt(res.Count - 1);
                }
                int b = (res.Count % 2 == 0) ? Tools.BitCount(p) : Tools.BitCount(o);
                int w = (res.Count % 2 == 0) ? Tools.BitCount(o) : Tools.BitCount(p);
                diff = b - w;
            }
            else
            {
                res.Clear();
                diff = 0;
            }

            return res.ToArray();
        }

        /// <summary>
        /// 棋譜読み込み
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <returns>棋譜リスト</returns>
        private static List<int[]> LoadWTB(string path)
        {
            int fileSize;
            byte[] buf;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                fileSize = (int)fs.Length;
                buf = new byte[fileSize];
                int remainSize = fileSize;
                int bufPos = 0;
                int readSize;
                while (remainSize > 0)
                {
                    readSize = fs.Read(buf, bufPos, System.Math.Min(4096, remainSize));
                    bufPos += readSize;
                    remainSize -= readSize;
                }
            }

            const int HEAD = 16;
            //  1: ファイルの作られた世紀
            //  1: ファイルの作られた年
            //  1: ファイルの作られた月
            //  1: ファイルの作られた日
            //  4: 記録されている試合の数
            //  2: 記録されている大会または対戦者名の数
            //  2: 対戦の行われた年
            //  4: 予備
            const int UNIT = 68;
            //  2: 大会名のインデックス
            //  2: 黒番の対戦者のインデックス
            //  2: 白番の対戦者のインデックス
            //  1: 対戦結果（黒石の数）
            //  1: 理論スコア（黒石の数）
            // 60: 棋譜（11=A1～88=H8）
            var res = new List<int[]>();

            for (int i = 0; HEAD + UNIT * i < fileSize; i++)
            {
                var record = new int[60];
                for (int j = 0; j < record.Length; j++)
                {
                    record[j] = buf[HEAD + UNIT * i + 8 + j];
                }
                res.Add(record);
            }

            return res;
        }

        /// <summary>
        /// 棋譜リスト取得
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <returns>棋譜リスト</returns>
        internal static List<int[]> LoadKF(string path)
        {
            var res = new List<int[]>();

            foreach (var item in Common.LoadLogList(path))
            {
                try
                {
                    var pos = new List<int>();
                    foreach (var buf in item.Split(','))
                    {
                        pos.Add(int.Parse(buf));
                    }
                    if (pos.Count > 0)
                    {
                        res.Add(pos.ToArray());
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(item + "\n" + ex.ToString());
                }
            }

            return res;
        }

        /// <summary>
        /// 盤面->入力データ変換
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p">自分</param>
        /// <param name="o">相手</param>
        /// <returns>変換結果</returns>
        internal static T[] ConvInputData<T>(ulong p, ulong o)
        {
            T[] res = new T[128];
            ulong t;
            switch (res)
            {
                case int[] xI:
                    for (int i = 0; i < 64; i++)
                    {
                        t = Tools.Pos2Bit(i);
                        if ((p & t) != 0)
                        {
                            xI[i * 2] = 1;
                        }
                        if ((o & t) != 0)
                        {
                            xI[i * 2 + 1] = 1;
                        }
                    }
                    break;
                case float[] xF:
                    for (int i = 0; i < 64; i++)
                    {
                        t = Tools.Pos2Bit(i);
                        if ((p & t) != 0)
                        {
                            xF[i * 2] = 1.0f;
                        }
                        if ((o & t) != 0)
                        {
                            xF[i * 2 + 1] = 1.0f;
                        }
                    }
                    break;
                case double[] xD:
                    for (int i = 0; i < 64; i++)
                    {
                        t = Tools.Pos2Bit(i);
                        if ((p & t) != 0)
                        {
                            xD[i * 2] = 1.0;
                        }
                        if ((o & t) != 0)
                        {
                            xD[i * 2 + 1] = 1.0;
                        }
                    }
                    break;
            }
            return res;
        }

        /// <summary>
        /// 位置->出力データ変換
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pos">位置</param>
        /// <returns>変換結果</returns>
        internal static T[] ConvOutputData<T>(int pos)
        {
            T[] res = new T[64];
            if (pos >= 0 && pos < 64)
            {
                switch (res)
                {
                    case int[] xI:
                        xI[pos] = 1;
                        break;
                    case float[] xF:
                        xF[pos] = 1.0f;
                        break;
                    case double[] xD:
                        xD[pos] = 1.0;
                        break;
                }
            }
            return res;
        }
    }

    /// <summary>
    /// データ
    /// </summary>
    internal class KFData
    {
        /// <summary>
        /// 棋譜
        /// </summary>
        public int[] Record { get; }

        /// <summary>
        /// 石差
        /// </summary>
        public int Result { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="record">棋譜</param>
        /// <param name="result">石差</param>
        public KFData(int[] record, int result)
        {
            Record = new int[record.Length];
            System.Array.Copy(record, Record, record.Length);
            Result = result;
        }
    }

    /// <summary>
    /// データローダ
    /// </summary>
    internal class KFDataLoader
    {
        /// <summary>
        /// 正規化棋譜リスト
        /// </summary>
        public List<KFData> KFList { get; } = new List<KFData>();

        /// <summary>
        /// 次回変換開始位置
        /// </summary>
        private int Pos = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path">棋譜ファイルパス</param>
        public KFDataLoader(string path)
        {
            var temp = ToolsKF.LoadKF(path).OrderBy(param => System.Guid.NewGuid());
            foreach (var log in temp)
            {
                var res = ToolsKF.Norm(log, out int diff);
                if (res.Length > 0)
                {
                    // 正規化棋譜のみ登録
                    KFList.Add(new KFData(res, diff));
                }
                else
                {
                    // 正規化棋譜以外は対象外
                    System.Diagnostics.Debug.WriteLine("E00:" + string.Join(",", log));
                }
            }
        }

        /// <summary>
        /// 棋譜データ変換
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="count">要求棋譜数</param>
        /// <param name="id">入力データリスト</param>
        /// <param name="od">出力データリスト</param>
        /// <returns>データ数</returns>
        public int GetConvData8<T>(int count, List<T[]> id, List<int> od)
        {
            if (KFList.Count == 0)
            {
                return 0;
            }
            for (int i = 0; i < count && Pos < KFList.Count; i++)
            {
                // 回転
                ConvData(KFList[Pos].Record, KFList[Pos].Result, Common.BB_BLACK, Common.BB_WHITE, id, od);
                ConvData(ToolsKF.Conv(KFList[Pos].Record, 1), KFList[Pos].Result, Common.BB_BLACK, Common.BB_WHITE, id, od);
                ConvData(ToolsKF.Conv(KFList[Pos].Record, 2), KFList[Pos].Result, Common.BB_BLACK, Common.BB_WHITE, id, od);
                ConvData(ToolsKF.Conv(KFList[Pos].Record, 3), KFList[Pos].Result, Common.BB_BLACK, Common.BB_WHITE, id, od);
                // 反転
                ConvData(ToolsKF.Conv(KFList[Pos].Record, 4), KFList[Pos].Result, Common.BB_WHITE, Common.BB_BLACK, id, od);
                ConvData(ToolsKF.Conv(KFList[Pos].Record, 5), KFList[Pos].Result, Common.BB_WHITE, Common.BB_BLACK, id, od);
                ConvData(ToolsKF.Conv(KFList[Pos].Record, 6), KFList[Pos].Result, Common.BB_WHITE, Common.BB_BLACK, id, od);
                ConvData(ToolsKF.Conv(KFList[Pos].Record, 7), KFList[Pos].Result, Common.BB_WHITE, Common.BB_BLACK, id, od);
                Pos++;
            }
            Pos %= KFList.Count;
            return od.Count;
        }

        /// <summary>
        /// データ変換
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="record">棋譜</param>
        /// <param name="result">石差</param>
        /// <param name="p">初期値：黒</param>
        /// <param name="o">初期値：白</param>
        /// <param name="id">入力データリスト</param>
        /// <param name="od">出力データリスト</param>
        private void ConvData<T>(int[] record, int result, ulong p, ulong o, List<T[]> id, List<int> od)
        {
            ulong lm, s;
            for (int i = 0; i < record.Length; i++)
            {
                // 有効なら
                if (record[i] > -1)
                {
                    lm = Tools.LegalMove(p, o);
                    s = Tools.Pos2Bit(record[i]);
                    if ((s & lm) != 0)
                    {
                        // 先手が勝なら偶数手のデータを先手が負なら奇数手のデータを登録
                        if ((result >= 0 && i % 2 == 0) || (result <= 0 && i % 2 == 1))
                        {
                            id.Add(ToolsKF.ConvInputData<T>(p, o));
                            od.Add(record[i]);
                        }
                        // 反転
                        Tools.Flip(ref p, ref o, s);
                    }
                    else
                    {
                        break;
                    }
                }
                (o, p) = (p, o);
            }
            if (Tools.LegalMove(p, o) != 0 || Tools.LegalMove(o, p) != 0)
            {
                // 正規化棋譜以外は対象外
                System.Diagnostics.Debug.WriteLine("E01:" + string.Join(",", record));
                id.Clear();
                od.Clear();
            }
        }
    }

}
