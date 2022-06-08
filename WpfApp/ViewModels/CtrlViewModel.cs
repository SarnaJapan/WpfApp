// メイン処理の版数。MainViewModel.cs と一致させること。
// #define MODE_V1

using OthelloInterface;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using WpfApp.Models;

namespace WpfApp.ViewModels
{
    /// <summary>
    /// 設定ダイアログ
    /// </summary>
    public class CtrlViewModel : NotificationObject
    {
        #region ダイアログ表示対応

        /// <summary>
        /// 表示終了処理
        /// </summary>
        private readonly System.Action CloseAction;

        /// <summary>
        /// ゲームマスター：設定対象モデルへの参照を起動時に設定
        /// </summary>
#if MODE_V1
        private readonly MasterV1 Master;
#else
        private readonly Master Master;
#endif

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="closeAction">表示終了処理</param>
        /// <param name="master">設定対象ゲームマスター</param>
#if MODE_V1
        public CtrlViewModel(System.Action closeAction, MasterV1 master)
#else
        public CtrlViewModel(System.Action closeAction, Master master)
#endif
        {
            CloseAction = closeAction;
            Master = master;

            // 評価プレイヤー選択肢設定
            EvalMap = Master.PlayerMap.Where(p => !p.Value.Version.EndsWith(Common.OPTION_NOEVAL));

            // 対戦プレイヤー選択肢設定
#if MODE_V1
            // 旧版（ゲームマスターに登録済みのプレイヤーリストを取得）
            MatchMapV1 = Master.PlayerMap.Where(p => !p.Value.Version.EndsWith(Common.OPTION_NOMATCH));
            // BitBoard版（メイン処理の版数と異なるため新規プレイヤーリストを取得）
            var gm = new Master();
            MatchMap = gm.PlayerMap.Where(p => !p.Value.Version.EndsWith(Common.OPTION_NOMATCH));
#else
            // 旧版（メイン処理の版数と異なるため新規プレイヤーリストを取得）
            var gmv1 = new MasterV1();
            MatchMapV1 = gmv1.PlayerMap.Where(p => !p.Value.Version.EndsWith(Common.OPTION_NOMATCH));
            // BitBoard版（ゲームマスターに登録済みのプレイヤーリストを取得）
            MatchMap = Master.PlayerMap.Where(p => !p.Value.Version.EndsWith(Common.OPTION_NOMATCH));
#endif
            // 登録済先頭プレイヤー選択
            MatchPBV1 = MatchMapV1.First().Value;
            MatchPWV1 = MatchMapV1.First().Value;
            MatchPB = MatchMap.First().Value;
            MatchPW = MatchMap.First().Value;

            // プレイヤー情報リスト設定
            PlayerInfoList = new ObservableCollection<PlayerInfo>();
            foreach (var item in MatchMapV1)
            {
                PlayerInfoList.Add(new PlayerInfo { Name = item.Key, Version = item.Value.Version });
            }
            foreach (var item in MatchMap)
            {
                PlayerInfoList.Add(new PlayerInfo { Name = item.Key, Version = item.Value.Version });
            }
        }

        /// <summary>
        /// <see cref="CloseCommand"/>
        /// </summary>
        private DelegateCommand closeCommand;
        /// <summary>
        /// 表示終了コマンド
        /// </summary>
        public DelegateCommand CloseCommand => closeCommand ?? (closeCommand = new DelegateCommand(_ =>
        {
            CloseAction();
        },
        _ =>
        {
            return true;
        }));

        /// <summary>
        /// 終了処理
        /// </summary>
        public void Closing()
        {
            // 設定変更反映
            Master.SetBack(true);
            Master.SetInfo(true);
            Master.SetTitle(Common.TITLE_PLAYER);
            // 起動中タスクキャンセル
            CancelCommand.Execute(null);
        }

        #endregion

        #region プロパティ

        /// <summary>
        /// <see cref="Master.PlayerMap"/>
        /// </summary>
#if MODE_V1
        public Dictionary<string, IOthelloPlayerV1> PlayerMap
#else
        public Dictionary<string, IOthelloPlayer> PlayerMap
#endif
        {
            get => Master.PlayerMap;
            set => Master.PlayerMap = value;
        }

        /// <summary>
        /// <see cref="Master.PlayerB"/>
        /// </summary>
#if MODE_V1
        public IOthelloPlayerV1 PlayerB
#else
        public IOthelloPlayer PlayerB
#endif
        {
            get => Master.PlayerB;
            set => Master.PlayerB = value;
        }

        /// <summary>
        /// <see cref="Master.PlayerW"/>
        /// </summary>
#if MODE_V1
        public IOthelloPlayerV1 PlayerW
#else
        public IOthelloPlayer PlayerW
#endif
        {
            get => Master.PlayerW;
            set => Master.PlayerW = value;
        }

        /// <summary>
        /// 評価用プレイヤーマップ
        /// </summary>
#if MODE_V1
        public static IEnumerable<KeyValuePair<string, IOthelloPlayerV1>> EvalMap { get; set; }
#else
        public static IEnumerable<KeyValuePair<string, IOthelloPlayer>> EvalMap { get; set; }
#endif

        /// <summary>
        /// <see cref="Master.PlayerE"/>
        /// </summary>
#if MODE_V1
        public IOthelloPlayerV1 PlayerE
#else
        public IOthelloPlayer PlayerE
#endif
        {
            get => Master.PlayerE;
            set => Master.PlayerE = value;
        }

        /// <summary>
        /// <see cref="Master.ShowCandidate"/>
        /// </summary>
        public bool ShowCandidate
        {
            get => Master.ShowCandidate;
            set => Master.ShowCandidate = value;
        }

        /// <summary>
        /// <see cref="Master.AutomaticMove"/>
        /// </summary>
        public bool AutomaticMove
        {
            get => Master.AutomaticMove;
            set => Master.AutomaticMove = value;
        }

        /// <summary>
        /// 対戦用プレイヤーマップ
        /// </summary>
        public static IEnumerable<KeyValuePair<string, IOthelloPlayer>> MatchMap { get; set; }

        /// <summary>
        /// 対戦用プレイヤー：黒
        /// </summary>
        public IOthelloPlayer MatchPB { get; set; }

        /// <summary>
        /// 対戦用プレイヤー：白
        /// </summary>
        public IOthelloPlayer MatchPW { get; set; }

        /// <summary>
        /// 対戦用プレイヤーマップ旧版
        /// </summary>
        public static IEnumerable<KeyValuePair<string, IOthelloPlayerV1>> MatchMapV1 { get; set; }

        /// <summary>
        /// 対戦用プレイヤー旧版：黒
        /// </summary>
        public IOthelloPlayerV1 MatchPBV1 { get; set; }

        /// <summary>
        /// 対戦用プレイヤー旧版：白
        /// </summary>
        public IOthelloPlayerV1 MatchPWV1 { get; set; }

        /// <summary>
        /// <see cref="ModeV1"/>
        /// </summary>
        private bool modeV1 = false;
        /// <summary>
        /// 旧版戦略用フラグ
        /// </summary>
        public bool ModeV1
        {
            get => modeV1;
            set
            {
                modeV1 = value;
                OnPropertyChanged();
                // 共通コマンド実行可否更新（旧版戦略用フラグとコマンド種別に依存）
                TestCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// <see cref="MatchCount"/>
        /// </summary>
        private int matchCount = 100;
        /// <summary>
        /// 対戦回数
        /// </summary>
        public int MatchCount
        {
            get => matchCount;
            set
            {
                if (value != matchCount)
                {
                    matchCount = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// <see cref="Status"/>
        /// </summary>
        private string status = "";
        /// <summary>
        /// ステータス文字列
        /// </summary>
        public string Status
        {
            get => status;
            set
            {
                if (value != status)
                {
                    status = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// プレイヤー情報リスト
        /// </summary>
        public ObservableCollection<PlayerInfo> PlayerInfoList { get; set; }

        /// <summary>
        /// コマンド種別リスト
        /// </summary>
        public string[] TestList { get; } = new string[] {
            Common.TEST_CREATE, Common.TEST_CONVERT, Common.TEST_CHECK, Common.TEST_SEARCH,
            Common.TEST_TRAIN_NW, Common.TEST_REINFORCE,
            Common.TEST_TRAIN_NW_KELP, Common.TEST_REINFORCE_KELP,
            /// @todo 実験用のため修正予定
            Common.TEST_TRAIN_NW_KELP0, Common.TEST_TRAIN_NW_KELP1, Common.TEST_TRAIN_NW_KELP2, Common.TEST_TRAIN_NW_KELP3,
        };

        /// <summary>
        /// <see cref="TestType"/>
        /// </summary>
        private string testType = "";
        /// コマンド種別
        /// </summary>
        public string TestType
        {
            get => testType;
            set
            {
                testType = value;
                OnPropertyChanged();
                // 共通コマンド実行可否更新（旧版戦略用フラグとコマンド種別に依存）
                TestCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// ログディレクトリ
        /// </summary>
        /// public string LogDir { get; set; } = Common.GetAppPath(Common.LOG_DIR);
        /// @todo 実験用のため修正予定
        public string LogDir { get; set; } = @"D:\Prog\doc\WpfApp\log";

        #endregion

        #region コマンド

        /// <summary>
        /// <see cref="IsBusy"/>
        /// </summary>
        private bool isBusy;
        /// <summary>
        /// 処理中フラグ
        /// </summary>
        private bool IsBusy
        {
            get => isBusy;
            set
            {
                if (value != isBusy)
                {
                    isBusy = value;
                    // 共通コマンドと対戦コマンドの同時起動を抑止
                    TestCommand.RaiseCanExecuteChanged();
                    MatchCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// <see cref="CancelTokenSource"/>
        /// </summary>
        private CancellationTokenSource cancelTokenSource;
        /// <summary>
        /// キャンセルトークン
        /// </summary>
        private CancellationTokenSource CancelTokenSource
        {
            get => cancelTokenSource;
            set
            {
                cancelTokenSource = value;
                CancelCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// <see cref="CancelCommand"/>
        /// </summary>
        private DelegateCommand cancelCommand;
        /// <summary>
        /// キャンセルコマンド
        /// </summary>
        public DelegateCommand CancelCommand => cancelCommand ?? (cancelCommand = new DelegateCommand(_ =>
        {
            CancelTokenSource?.Cancel();
        },
        _ =>
        {
            return CancelTokenSource != null;
        }));

        /// <summary>
        /// 非同期実行
        /// </summary>
        /// <param name="func">処理</param>
        private async void AsyncCall(System.Func<System.IProgress<string>, CancellationToken, string> func)
        {
            IsBusy = true;
            CancelTokenSource = new CancellationTokenSource();
            Status = "開始";
            var progress = new System.Progress<string>(i =>
            {
                Status = $"実行中… [{i}]";
            });
            var res = await Task.Run(() =>
            {
                return func(progress, (CancelTokenSource != null) ? CancelTokenSource.Token : new CancellationToken(false));
            });
            Status = CancelTokenSource.IsCancellationRequested ? $"中断 [{res}]" : $"終了 [{res}]";
            CancelTokenSource = null;
            IsBusy = false;
        }

        /// <summary>
        /// 非同期実行
        /// </summary>
        /// <param name="func">処理</param>
        /// <param name="param">パラメータ</param>
        private async void AsyncCall(System.Func<System.IProgress<string>, CancellationToken, object[], string> func, object[] param)
        {
            IsBusy = true;
            CancelTokenSource = new CancellationTokenSource();
            Status = "開始";
            var progress = new System.Progress<string>(i =>
            {
                Status = $"実行中… [{i}]";
            });
            var res = await Task.Run(() =>
            {
                return func(progress, (CancelTokenSource != null) ? CancelTokenSource.Token : new CancellationToken(false), param);
            });
            Status = CancelTokenSource.IsCancellationRequested ? $"中断 [{res}]" : $"終了 [{res}]";
            CancelTokenSource = null;
            IsBusy = false;
        }

        /// <summary>
        /// 非同期対戦
        /// </summary>
        /// <param name="func">処理</param>
        /// <param name="pb">プレイヤー：黒</param>
        /// <param name="pw">プレイヤー：白</param>
        /// <param name="count">対戦回数</param>
        private async void AsyncCall<T>(System.Func<System.IProgress<string>, CancellationToken, T, T, int, string> func, T pb, T pw, int count)
        {
            IsBusy = true;
            CancelTokenSource = new CancellationTokenSource();
            Status = "開始";
            var progress = new System.Progress<string>(i =>
            {
                Status = $"対戦中… [{i}]";
            });
            var res = await Task.Run(() =>
            {
                return func(progress, (CancelTokenSource != null) ? CancelTokenSource.Token : new CancellationToken(false), pb, pw, count);
            });
            Status = CancelTokenSource.IsCancellationRequested ? $"中断 [{res}]" : $"終了 [{res}]";
            CancelTokenSource = null;
            IsBusy = false;
        }

        /// <summary>
        /// 非同期対戦
        /// </summary>
        /// <param name="func">処理</param>
        /// <param name="pb">プレイヤー：黒</param>
        /// <param name="pw">プレイヤー：白</param>
        /// <param name="count">対戦回数</param>
        /// <param name="param">パラメータ</param>
        private async void AsyncCall<T>(System.Func<System.IProgress<string>, CancellationToken, T, T, int, string, string> func, T pb, T pw, int count, string param)
        {
            IsBusy = true;
            CancelTokenSource = new CancellationTokenSource();
            Status = "開始";
            var progress = new System.Progress<string>(i =>
            {
                Status = $"対戦中… [{i}]";
            });
            var res = await Task.Run(() =>
            {
                return func(progress, (CancelTokenSource != null) ? CancelTokenSource.Token : new CancellationToken(false), pb, pw, count, param);
            });
            Status = CancelTokenSource.IsCancellationRequested ? $"中断 [{res}]" : $"終了 [{res}]";
            CancelTokenSource = null;
            IsBusy = false;
        }

        /// <summary>
        /// <see cref="TestCommand"/>
        /// </summary>
        private DelegateCommand testCommand;
        /// <summary>
        /// 共通コマンド
        /// </summary>
        public DelegateCommand TestCommand => testCommand ?? (testCommand = new DelegateCommand(param =>
        {
            if ("".Equals(param))
            {
                // パラメータ指定なし
                switch (TestType)
                {
                    case Common.TEST_CREATE:
                        AsyncCall(ToolsKF.CreateLog, MatchPB, MatchPW, MatchCount, LogDir);
                        break;
                    case Common.TEST_CONVERT:
                        AsyncCall(ToolsKF.ConvertLog, new object[] { LogDir });
                        break;
                    case Common.TEST_CHECK:
                        AsyncCall(ToolsKF.CheckLog, new object[] { LogDir });
                        break;
                    case Common.TEST_SEARCH:
                        AsyncCall(ToolsMC.SearchParam, new object[] { LogDir });
                        break;
                    case Common.TEST_TRAIN_NW:
                        AsyncCall(ToolsMLAccord.TrainNetwork, new object[] { LogDir, 0 });
                        break;
                    case Common.TEST_REINFORCE:
                        AsyncCall(ToolsMLAccord.ReinforceNetwork, new object[] { LogDir });
                        break;
                    case Common.TEST_TRAIN_NW_KELP:
                        AsyncCall(ToolsMLKelp.TrainNetwork, new object[] { LogDir, 0 });
                        break;
                    case Common.TEST_REINFORCE_KELP:
                        AsyncCall(ToolsMLKelp.ReinforceNetwork, new object[] { LogDir });
                        break;
                    /// @todo 実験用のため修正予定
                    case Common.TEST_TRAIN_NW_KELP0:
                        AsyncCall(ToolsMLKelp.TrainNetwork, new object[] { LogDir, 0 });
                        break;
                    case Common.TEST_TRAIN_NW_KELP1:
                        AsyncCall(ToolsMLKelp.TrainNetwork, new object[] { LogDir, 1 });
                        break;
                    case Common.TEST_TRAIN_NW_KELP2:
                        AsyncCall(ToolsMLKelp.TrainNetwork, new object[] { LogDir, 2 });
                        break;
                    case Common.TEST_TRAIN_NW_KELP3:
                        AsyncCall(ToolsMLKelp.TrainNetwork, new object[] { LogDir, 3 });
                        break;
                    /// @todo 実験用のため修正予定
                    default:
                        AsyncCall(Common.TestCommand);
                        break;
                }
            }
            else
            {
                // パラメーラ指定あり
                AsyncCall(Common.TestCommandBase, new object[] { param });
            }
        },
        _ =>
        {
            var res = !IsBusy;
            // 棋譜生成はBitBoard版のみ
            if (ModeV1 && TestType == Common.TEST_CREATE)
            {
                res = false;
            }
            return res;
        }));

        /// <summary>
        /// <see cref="MatchCommand"/>
        /// </summary>
        private DelegateCommand matchCommand;
        /// <summary>
        /// 対戦コマンド
        /// </summary>
        public DelegateCommand MatchCommand => matchCommand ?? (matchCommand = new DelegateCommand(_ =>
        {
            if (ModeV1)
            {
                AsyncCall(ToolsV1.MatchStatistics, MatchPBV1, MatchPWV1, MatchCount);
            }
            else
            {
                AsyncCall(Tools.MatchStatistics, MatchPB, MatchPW, MatchCount);
            }
        },
        _ =>
        {
            return !IsBusy;
        }));

        #endregion
    }

    /// <summary>
    /// プレイヤー情報
    /// </summary>
    public class PlayerInfo : NotificationObject
    {
        #region プレイヤー情報

        public string Version { get; set; } = "";
        public string Name { get; set; } = "";

        #endregion
    }

}
