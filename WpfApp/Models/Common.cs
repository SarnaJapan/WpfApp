using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace WpfApp.Models
{
    /// <summary>
    /// 共通処理
    /// </summary>
    internal static class Common
    {
        #region 個別

        /// <summary>
        /// 盤サイズ
        /// </summary>
        /// @note 旧版のみ変更可能。BitBoard版は8のみ有効。ビューと一致させること。
        public const int SIZE = 8;

        /// @name 石色
        ///@{
        public const int NULL = 0;
        public const int BLACK = 1;
        public const int WHITE = -1;
        ///@}

        /// <summary>
        /// タイトル
        /// </summary>
        public const string TITLE = "WPF Othello";

        /// @name タイトル種別
        ///@{
        public const int TITLE_PLAYER = 0;
        public const int TITLE_RESULT = 1;
        public const int TITLE_WAIT = 2;
        public const int TITLE_PASS = 3;
        ///@}

        /// <summary>
        /// BitBoard版初期値：黒
        /// </summary>
        public const ulong BB_BLACK = 0x0000000810000000;

        /// <summary>
        /// BitBoard版初期値：白
        /// </summary>
        public const ulong BB_WHITE = 0x0000001008000000;

        /// @name バージョン情報
        ///@{

        /// <summary>
        /// 表示フォーマット
        /// </summary>
        /// @note (Major Version).(Minor Version).(Build Version)(Option)
        public const string VERSION_FORMAT = "{0:D}.{1:D}.{2:D}{3}";

        /// <summary>
        /// オプション：対戦選択肢対象外
        /// </summary>
        public const string OPTION_NOMATCH = "/NoMatch";

        /// <summary>
        /// オプション：評価選択肢対象外
        /// </summary>
        public const string OPTION_NOEVAL = "/NoEval";

        ///@}

        /// <summary>
        /// 疑似乱数ジェネレータ
        /// </summary>
        public static System.Random R = new System.Random();

        /// @name コマンド種別
        ///@{
        public const string TEST_CREATE = "棋譜生成(->log_vs)";
        public const string TEST_CONVERT = "棋譜変換(->log_wt)";
        public const string TEST_CHECK = "棋譜確認(->log_ok)";
        public const string TEST_SEARCH = "MCTSパラメータ探索";
        public const string TEST_TRAIN_NW = "Train Network (Accord)";
        public const string TEST_REINFORCE = "Reinforce (Accord)";
        public const string TEST_TRAIN_NW_KELP = "Train Network (Kelp)";
        public const string TEST_REINFORCE_KELP = "Reinforce (Kelp)";
        // @todo 実験用のため修正予定
        public const string TEST_TRAIN_NW_KELP0 = "Train Network 00 (Kelp)";
        public const string TEST_TRAIN_NW_KELP1 = "Train Network 01 (Kelp)";
        public const string TEST_TRAIN_NW_KELP2 = "Train Network 02 (Kelp)";
        public const string TEST_TRAIN_NW_KELP3 = "Train Network 03 (Kelp)";
        // @todo 実験用のため修正予定
        ///@}

        /// <summary>
        /// 実験処理
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="token"></param>
        /// <param name="param"></param>
        /// <returns>処理時間</returns>
        public static string TestCommandBase(System.IProgress<string> progress, CancellationToken token, object[] param)
        {
            // パラメータ確認
            int timeout;
            try
            {
                timeout = int.Parse((string)param[0]);
            }
            catch (System.Exception)
            {
                return "Invalid parameter.";
            }

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < 100 && !token.IsCancellationRequested; i++)
            {
                Thread.Sleep(timeout);
                progress.Report($"{i}");
            }
            sw.Stop();

            return $"{sw.ElapsedMilliseconds} ms";
        }

        /// <summary>
        /// 実験処理
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="token"></param>
        /// <returns>処理時間</returns>
        public static string TestCommand(System.IProgress<string> progress, CancellationToken token)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < 100 && !token.IsCancellationRequested; i++)
            {
                Thread.Sleep(100);
                progress.Report($"{i}");
            }
            sw.Stop();

            return $"{sw.ElapsedMilliseconds} ms";
        }

        #endregion

        #region 汎用

        /// <summary>
        /// ログディレクトリ
        /// </summary>
        public const string LOG_DIR = "log";

        /// <summary>
        /// アプリケーション用パス取得
        /// </summary>
        /// <param name="subdir">サブディレクトリ</param>
        /// <returns>アプリケーション用パス</returns>
        public static string GetAppPath(string subdir)
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", subdir);
        }

        /// <summary>
        /// プラグイン名接尾辞
        /// </summary>
        public const string PLUGIN_SUF = " *";

        /// <summary>
        /// プラグインディレクトリ
        /// </summary>
        private const string PLUGIN_DIR = "plugin";

        /// <summary>
        /// プラグインリスト取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>プラグインリスト</returns>
        public static List<T> GetPluginList<T>()
        {
            var list = new List<T>();

            var path = GetAppPath(PLUGIN_DIR);
            if (Directory.Exists(path))
            {
                foreach (var dll in Directory.GetFiles(path, "*.dll"))
                {
                    try
                    {
                        var asm = Assembly.LoadFile(dll);
                        foreach (var type in asm.GetTypes())
                        {
                            if (type.IsClass && type.IsPublic && !type.IsAbstract && !type.IsInterface)
                            {
                                if (System.Activator.CreateInstance(type) is T plugin)
                                {
                                    list.Add(plugin);
                                }
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(dll + "\n" + ex.ToString());
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 文字列読み込み
        /// </summary>
        /// <param name="path">パス</param>
        /// <returns>文字列リスト</returns>
        public static List<string> LoadLogList(string path)
        {
            var res = new List<string>();

            StreamReader sr = null;
            try
            {
                sr = new StreamReader(path, Encoding.ASCII);
                while (sr.Peek() >= 0)
                {
                    var l = sr.ReadLine();
                    if (l != null)
                    {
                        res.Add(l);
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            finally
            {
                sr?.Close();
            }

            return res;
        }

        /// <summary>
        /// 文字列書き込み
        /// </summary>
        /// <param name="path">パス</param>
        /// <param name="log">文字列リスト</param>
        /// <returns>書き込み結果</returns>
        public static bool SaveLogList(string path, List<string> log)
        {
            StreamWriter sw = null;
            try
            {
                if (log.Count > 0)
                {
                    sw = new StreamWriter(path, true, Encoding.ASCII);
                    sw.Write(string.Join("\n", log) + "\n");
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return false;
            }
            finally
            {
                sw?.Close();
            }

            return true;
        }

        #endregion
    }

}
