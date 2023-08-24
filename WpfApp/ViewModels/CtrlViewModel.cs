// メイン処理の版数。MainViewModel.cs と一致させること。
#define MODE_V1

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WpfApp.Models;
using WpfApp.Utils;
using WpfLib.OthelloInterface;

#if MODE_V1
using GameMaster = WpfApp.Models.MasterV1;
using IGamePlayer = WpfLib.OthelloInterface.IOthelloPlayerV1;
#else
using GameMaster = WpfApp.Models.Master;
using IGamePlayer = WpfLib.OthelloInterface.IOthelloPlayer;
#endif

namespace WpfApp.ViewModels
{
    /// <summary>
    /// 設定ダイアログ
    /// </summary>
    public class CtrlViewModel : NotificationObject
    {
        #region 設定ダイアログ

        /// <summary>
        /// ゲームマスター
        /// </summary>
        /// 設定対象として呼出時に参照先を指定する
        private readonly GameMaster Master;

        /// <summary>
        /// コンストラクタ：プレイヤー関連選択肢の設定
        /// </summary>
        /// <param name="master">設定対象ゲームマスター</param>
        /// - プレイヤーリストはゲームマスターから取得
        /// - 評価用と対戦用の選択肢はバージョンオプションでフィルター
        public CtrlViewModel(GameMaster master)
        {
            Master = master;

            // 評価プレイヤー選択肢設定
            EvalMap = Master.PlayerMap.Where(p => !Common.IsRandom(p.Value.Version));

            // 対戦プレイヤー選択肢設定
#if MODE_V1
            // 旧版（ゲームマスターに登録済みのプレイヤーリストを取得）
            MatchMapV1 = Master.PlayerMap.Where(p => !Common.IsManual(p.Value.Version));
            // BitBoard版（メイン処理の版数と異なるため新規プレイヤーリストを取得）
            var gm = new Master();
            MatchMap = gm.PlayerMap.Where(p => !Common.IsManual(p.Value.Version));
#else
            // 旧版（メイン処理の版数と異なるため新規プレイヤーリストを取得）
            var gmv1 = new MasterV1();
            MatchMapV1 = gmv1.PlayerMap.Where(p => !Common.IsManual(p.Value.Version));
            // BitBoard版（ゲームマスターに登録済みのプレイヤーリストを取得）
            MatchMap = Master.PlayerMap.Where(p => !Common.IsManual(p.Value.Version));
#endif
            // 登録済先頭プレイヤー選択
            MatchPBV1 = MatchMapV1.First().Value;
            MatchPWV1 = MatchMapV1.First().Value;
            MatchPB = MatchMap.First().Value;
            MatchPW = MatchMap.First().Value;
        }

        #endregion

        #region ダイアログ表示対応

        /// <summary>
        /// <see cref="CloseWindow"/>
        /// </summary>
        private bool closeWindow = false;
        /// <summary>
        /// ダイアログ終了フラグ
        /// </summary>
        public bool CloseWindow
        {
            get => closeWindow;
            set
            {
                closeWindow = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// <see cref="CloseCommand"/>
        /// </summary>
        private DelegateCommand closeCommand;
        /// <summary>
        /// ダイアログ終了コマンド
        /// </summary>
        public DelegateCommand CloseCommand => closeCommand ?? (closeCommand = new DelegateCommand(_ =>
        {
            CloseWindow = true;
        },
        _ =>
        {
            return true;
        }));

        #endregion

        #region プロパティ

        /// <summary>
        /// <see cref="GameMaster.PlayerMap"/>
        /// </summary>
        public Dictionary<string, IGamePlayer> PlayerMap
        {
            get => Master.PlayerMap;
            set => Master.PlayerMap = value;
        }

        /// <summary>
        /// <see cref="GameMaster.PlayerB"/>
        /// </summary>
        public IGamePlayer PlayerB
        {
            get => Master.PlayerB;
            set => Master.PlayerB = value;
        }

        /// <summary>
        /// <see cref="GameMaster.PlayerW"/>
        /// </summary>
        public IGamePlayer PlayerW
        {
            get => Master.PlayerW;
            set => Master.PlayerW = value;
        }

        /// <summary>
        /// 評価用プレイヤーマップ
        /// </summary>
        public static IEnumerable<KeyValuePair<string, IGamePlayer>> EvalMap { get; set; }

        /// <summary>
        /// <see cref="GameMaster.PlayerE"/>
        /// </summary>
        public IGamePlayer PlayerE
        {
            get => Master.PlayerE;
            set => Master.PlayerE = value;
        }

        /// <summary>
        /// <see cref="GameMaster.ShowCandidate"/>
        /// </summary>
        public bool ShowCandidate
        {
            get => Master.ShowCandidate;
            set => Master.ShowCandidate = value;
        }

        /// <summary>
        /// <see cref="GameMaster.AutomaticMove"/>
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
                if (value != modeV1)
                {
                    modeV1 = value;
                    OnPropertyChanged();
                }
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

        #endregion

        #region コマンド

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
                return func(progress, CancelTokenSource.Token);
            });
            Status = CancelTokenSource.IsCancellationRequested ? $"中断 [{res}]" : $"終了 [{res}]";
            CancelTokenSource.Dispose();
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
                return func(progress, CancelTokenSource.Token, param);
            });
            Status = CancelTokenSource.IsCancellationRequested ? $"中断 [{res}]" : $"終了 [{res}]";
            CancelTokenSource.Dispose();
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
                return func(progress, CancelTokenSource.Token, pb, pw, count);
            });
            Status = CancelTokenSource.IsCancellationRequested ? $"中断 [{res}]" : $"終了 [{res}]";
            CancelTokenSource.Dispose();
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
                return func(progress, CancelTokenSource.Token, pb, pw, count, param);
            });
            Status = CancelTokenSource.IsCancellationRequested ? $"中断 [{res}]" : $"終了 [{res}]";
            CancelTokenSource.Dispose();
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
        public DelegateCommand TestCommand => testCommand ?? (testCommand = new DelegateCommand(_ =>
        {
            AsyncCall(Common.TestCommand);
        },
        _ =>
        {
            return !IsBusy;
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

}
