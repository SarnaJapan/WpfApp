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
        #region 専用

        /// <summary>
        /// 盤サイズ
        /// </summary>
        /// @note 旧版のみ変更可能。BitBoard版は8のみ有効。ビューと一致させること。
        public const int SIZE = 8;

        /// @name 石色
        /// @{
        public const int EMPTY = 0;
        public const int BLACK = 1;
        public const int WHITE = -1;
        /// @}

        /// <summary>
        /// タイトル
        /// </summary>
        public const string TITLE = "WPF Othello";

        /// <summary>
        /// BitBoard版初期値：黒
        /// </summary>
        public const ulong BB_BLACK = 0x0000000810000000;

        /// <summary>
        /// BitBoard版初期値：白
        /// </summary>
        public const ulong BB_WHITE = 0x0000001008000000;

        /// @name バージョン情報
        /// @{

        /// <summary>
        /// 表示フォーマット
        /// </summary>
        /// @note (Major Version).(Minor Version).(Build Version)(Option)
        public const string VERSION_FORMAT = "{0:D}.{1:D}.{2:D}{3}";

        /// <summary>
        /// オプション：対戦選択肢対象外
        /// </summary>
        /// 常に計算不可を返す手動プレイヤー用オプション。設定時は自動対戦選択肢の対象外となる。
        public const string OPTION_NOMATCH = "/NoMatch";

        /// <summary>
        /// オプション：評価選択肢対象外
        /// </summary>
        /// 評価値が存在しないランダム戦略プレイヤー用オプション。設定時は評価選択肢の対象外となる。
        public const string OPTION_NOEVAL = "/NoEval";

        /// @}

        /// <summary>
        /// 疑似乱数ジェネレータ
        /// </summary>
        public static System.Random R = new System.Random();

        /// @name ステータス種別
        /// @{
        public const int STATUS_EMPTY = 0;  //!< ステータス：空白状態
        public const int STATUS_WIN = 1;    //!< ステータス：勝ち状態
        public const int STATUS_LOSE = 2;   //!< ステータス：負け状態
        public const int STATUS_DRAW = 3;   //!< ステータス：引き分け状態
        public const int STATUS_WAIT = 4;   //!< ステータス：ターン進行待ち状態（手動）
        public const int STATUS_NEXT = 5;   //!< ステータス／パラメータ：ターン進行待ち状態／通知
        public const int STATUS_CALC = 6;   //!< ステータス／パラメータ：計算開始状態／通知
        public const int STATUS_PASS = 7;   //!< ステータス／パラメータ：パス状態／通知
        public const int STATUS_RESULT = 8; //!< パラメータ：結果表示通知
        public const int STATUS_INIT = 9;   //!< パラメータ：初期化通知
        public const int STATUS_CTRL = 10;  //!< パラメータ：設定更新通知
        /// @}

        /// <summary>
        /// ステータス文字列
        /// </summary>
        /// ステータス(@ref WpfApp.ViewModels.Board::Status)に設定する文字列。通知ステータス(@ref WpfApp.Models.GameStatus)から変換。
        /// @sa STATUS_EMPTY,STATUS_WIN,STATUS_LOSE,STATUS_DRAW,STATUS_WAIT,  
        /// STATUS_NEXT,STATUS_CALC,STATUS_PASS,STATUS_RESULT,STATUS_INIT,STATUS_CTRL
        /// @par STATUS_INIT
        /// 初期化シーケンス
        /// @msc
        /// MainWindow, Board, Master, Player;
        /// MainWindow->Board [label="StartCommand@binding"];
        /// Board=>Master     [label="GameStart()"];
        /// Master=>Master    [label="SetStatus(STATUS_INIT)"];
        /// Board<<=Master    [label="StatusPropertyChanged(STATUS_NEXT/STATUS_WAIT)"];
        /// MainWindow<-Board [label="Status@binding(STATUS_NEXT/STATUS_WAIT)"];
        /// @endmsc
        /// @par STATUS_CTRL
        /// 設定更新シーケンス
        /// @msc
        /// MainWindow, Board, Master, Player;
        /// MainWindow->Board [label="OnCloseCtrlCommand@binding"];
        /// Board=>Master     [label="SetStatus(STATUS_CTRL)"];
        /// Board<<=Master    [label="StatusPropertyChanged(STATUS_NEXT/STATUS_WAIT)"];
        /// MainWindow<-Board [label="Status@binding(STATUS_NEXT/STATUS_WAIT)"];
        /// @endmsc
        /// @par STATUS_NEXT/STATUS_CALC/STATUS_PASS/STATUS_RESULT
        /// ステータス表示シーケンス
        /// @msc
        /// MainWindow, Board, Master, Player;
        /// MainWindow->Board [label="SelectCommand@binding"];
        /// Board=>Master     [label="GameSelectPos()"];
        /// Master=>Master    [label="SetStatus(STATUS_CALC)"];
        /// Board<<=Master    [label="StatusPropertyChanged(STATUS_CALC)"];
        /// MainWindow<-Board [label="Status@binding(STATUS_CALC)"];
        /// Master=>Player    [label="Calc()"];
        /// Master<<Player    [label="result"];
        /// Master=>Master    [label="SetStatus(STATUS_NEXT)"];
        /// Board<<=Master    [label="StatusPropertyChanged(STATUS_NEXT/STATUS_WAIT)"];
        /// MainWindow<-Board [label="Status@binding(STATUS_NEXT/STATUS_WAIT)"];
        /// ---               [label="pass"];
        /// Master=>Master    [label="SetStatus(STATUS_PASS)"];
        /// Board<<=Master    [label="StatusPropertyChanged(STATUS_PASS)"];
        /// MainWindow<-Board [label="Status@binding(STATUS_PASS)"];
        /// ---               [label="result"];
        /// Master=>Master    [label="SetStatus(STATUS_RESULT)"];
        /// Board<<=Master    [label="StatusPropertyChanged(STATUS_RESULT)"];
        /// MainWindow<-Board [label="Status@binding(STATUS_WIN/STATUS_LOSE/STATUS_DRAW)"];
        /// @endmsc
        public static Dictionary<int, string> StatusString = new Dictionary<int, string>()
        {
            {STATUS_EMPTY, ""},
            {STATUS_WIN,   "勝ち"},
            {STATUS_LOSE,  "負け"},
            {STATUS_DRAW,  "引き分け"},
            {STATUS_WAIT,  "選択待ち"},
            {STATUS_NEXT,  "次へ"},
            {STATUS_CALC,  "計算中…"},
            {STATUS_PASS,  "パス"},
        };

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

    /// <summary>
    /// 通知ステータス
    /// </summary>
    public class GameStatus
    {
        #region 通知ステータス

        /// <summary>
        /// 石数：黒
        /// </summary>
        public int CountB { get; set; } = 0;

        /// <summary>
        /// 石数：白
        /// </summary>
        public int CountW { get; set; } = 0;

        /// <summary>
        /// 手動戦略フラグ：黒
        /// </summary>
        public bool ManualB { get; set; } = true;

        /// <summary>
        /// 手動戦略フラグ：白
        /// </summary>
        public bool ManualW { get; set; } = true;

        /// <summary>
        /// ステータス種別：黒
        /// </summary>
        public int StatusB { get; set; } = Common.STATUS_EMPTY;

        /// <summary>
        /// ステータス種別：白
        /// </summary>
        public int StatusW { get; set; } = Common.STATUS_EMPTY;

        /// <summary>
        /// 履歴
        /// </summary>
        public List<int> Record { get; set; } = new List<int>();

        #endregion
    }

}
