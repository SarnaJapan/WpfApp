// メイン処理の版数。CtrlViewModel.cs と一致させること。
// #define MODE_V1

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using WpfApp.Models;
using WpfApp.Utils;

#if MODE_V1
using GameMaster = WpfApp.Models.MasterV1;
#else
using GameMaster = WpfApp.Models.Master;
#endif

namespace WpfApp.ViewModels
{
    /// <summary>
    /// 石
    /// </summary>
    public class Stone : NotificationObject
    {
        #region 石

        /// <summary>
        /// 行位置
        /// </summary>
        public int RowIndex { get; set; }

        /// <summary>
        /// 列位置
        /// </summary>
        public int ColumnIndex { get; set; }

        /// <summary>
        /// <see cref="BackColor"/>
        /// </summary>
        private string backColor = "";
        /// <summary>
        /// 背景色
        /// </summary>
        public string BackColor
        {
            get => backColor;
            set
            {
                if (value != backColor)
                {
                    backColor = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// <see cref="Color"/>
        /// </summary>
        private string color = "";
        /// <summary>
        /// 石色
        /// </summary>
        public string Color
        {
            get => color;
            set
            {
                if (value != color)
                {
                    color = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// <see cref="LastColor"/>
        /// </summary>
        private string lastColor = "";
        /// <summary>
        /// 更新色
        /// </summary>
        public string LastColor
        {
            get => lastColor;
            set
            {
                if (value != lastColor)
                {
                    lastColor = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// <see cref="this[int id]"/>
        /// </summary>
        /// @note インデクサは[(文字色,文字列),情報数]の構成。ビューと一致させること。
        private readonly string[] info = new string[] { "White", "", "White", "", "White", "", "White", "", };
        /// <summary>
        /// インデクサ
        /// </summary>
        public string this[int id]
        {
            get => info[id];
            set
            {
                if (value != info[id])
                {
                    info[id] = value;
                    OnPropertyChanged(Binding.IndexerName);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 盤
    /// </summary>
    public class Board : NotificationObject
    {
        #region 盤

        /// <summary>
        /// ゲームマスター
        /// </summary>
        /// メインモデルとして起動時に生成する
        private readonly GameMaster Master = new GameMaster();

        /// <summary>
        /// 石データ
        /// </summary>
        public Stone[] Data { get; set; } = new Stone[Common.SIZE * Common.SIZE];

        /// <summary>
        /// コンストラクタ：ゲームマスターと石データの初期設定
        /// </summary>
        /// - 変更通知イベントハンドラ登録
        ///   - 各データをモデル用からビュー用に変換する処理を登録する
        /// - 関連ビューモデル生成
        ///   - 盤(@ref Board)が所有する石(@ref Stone)を生成する
        public Board()
        {
            Master.PropertyChanged += BackPropertyChanged;
            Master.PropertyChanged += DataPropertyChanged;
            Master.PropertyChanged += InfoPropertyChanged;
            Master.PropertyChanged += StatusPropertyChanged;

            for (int i = 0; i < Common.SIZE; i++)
            {
                for (int j = 0; j < Common.SIZE; j++)
                {
                    Data[i * Common.SIZE + j] = new Stone { RowIndex = i, ColumnIndex = j, BackColor = "Green", Color = "Transparent", LastColor = "Transparent", };
                }
            }
        }

        #endregion

        #region プロパティ

        /// <summary>
        /// <see cref="Title"/>
        /// </summary>
        private string title = Common.TITLE;
        /// <summary>
        /// タイトル
        /// </summary>
        public string Title
        {
            get => title;
            set
            {
                if (value != title)
                {
                    title = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// <see cref="Status"/>
        /// </summary>
        /// @note ステータスは[(ステータス,ツールチップ),領域数]の構成。ビューと一致させること。
        private ObservableCollection<string> status = new ObservableCollection<string>() { "", "", "", "", "", "", };
        /// <summary>
        /// ステータス
        /// </summary>
        public ObservableCollection<string> Status
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
        /// <see cref="IsBusy"/>
        /// </summary>
        private bool isBusy = false;
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
                    // 開始と選択の同時起動を抑止
                    StartCommand.RaiseCanExecuteChanged();
                    SelectCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// <see cref="InfoViewModel"/>
        /// </summary>
        private InfoViewModel infoViewModel;
        /// <summary>
        /// 情報ダイアログ
        /// </summary>
        public InfoViewModel InfoViewModel
        {
            get => infoViewModel;
            set
            {
                if (infoViewModel != value)
                {
                    infoViewModel = value;
                    OnPropertyChanged(nameof(InfoViewModel));
                }
            }
        }

        /// <summary>
        ///  <see cref="CtrlViewModel" />
        /// </summary>
        private CtrlViewModel ctrlViewModel;
        /// <summary>
        /// 設定ダイアログ
        /// </summary>
        public CtrlViewModel CtrlViewModel
        {
            get => ctrlViewModel;
            set
            {
                if (ctrlViewModel != value)
                {
                    ctrlViewModel = value;
                    OnPropertyChanged(nameof(CtrlViewModel));
                }
            }
        }

        #endregion

        #region コマンド

        /// <summary>
        /// <see cref="OpenInfoCommand"/>
        /// </summary>
        private DelegateCommand openInfoCommand;
        /// <summary>
        /// 情報表示コマンド
        /// </summary>
        public DelegateCommand OpenInfoCommand => openInfoCommand ?? (openInfoCommand = new DelegateCommand(_ =>
        {
            InfoViewModel = new InfoViewModel();
        },
        _ =>
        {
            return true;
        }));

        /// <summary>
        /// <see cref="OpenCtrlCommand"/>
        /// </summary>
        private DelegateCommand openCtrlCommand;
        /// <summary>
        /// 設定表示コマンド
        /// </summary>
        public DelegateCommand OpenCtrlCommand => openCtrlCommand ?? (openCtrlCommand = new DelegateCommand(_ =>
        {
            CtrlViewModel = new CtrlViewModel(Master);
        },
        _ =>
        {
            return true;
        }));

        /// <summary>
        /// <see cref="OnCloseInfoCommand"/>
        /// </summary>
        private DelegateCommand onCloseInfoCommand;
        /// <summary>
        /// 情報終了時処理用コマンド
        /// </summary>
        public DelegateCommand OnCloseInfoCommand => onCloseInfoCommand ?? (onCloseInfoCommand = new DelegateCommand(_ =>
        {
        },
        _ =>
        {
            return true;
        }));

        /// <summary>
        /// <see cref="OnCloseCtrlCommand"/>
        /// </summary>
        private DelegateCommand onCloseCtrlCommand;
        /// <summary>
        /// 設定終了時処理用コマンド
        /// </summary>
        public DelegateCommand OnCloseCtrlCommand => onCloseCtrlCommand ?? (onCloseCtrlCommand = new DelegateCommand(_ =>
        {
            // 起動中タスクキャンセル
            CtrlViewModel?.CancelCommand.Execute(null);
            // 設定変更反映
            Master.SetBack(true);
            Master.SetInfo(true);
            Master.SetStatus(Common.STATUS_CTRL);
        },
        _ =>
        {
            return true;
        }));

        /// <summary>
        /// <see cref="StartCommand"/>
        /// </summary>
        private DelegateCommand startCommand;
        /// <summary>
        /// 開始コマンド
        /// </summary>
        public DelegateCommand StartCommand => startCommand ?? (startCommand = new DelegateCommand(_ =>
        {
            IsBusy = true;
            Master.GameStart();
            IsBusy = false;
        },
        _ =>
        {
            return !IsBusy;
        }));

        /// <summary>
        /// <see cref="SelectCommand"/>
        /// </summary>
        private DelegateCommand selectCommand;
        /// <summary>
        /// 選択コマンド
        /// </summary>
        public DelegateCommand SelectCommand => selectCommand ?? (selectCommand = new DelegateCommand(param =>
        {
            IsBusy = true;
            Master.GameSelectPos((int)param);
            IsBusy = false;
        },
        _ =>
        {
            return !IsBusy;
        }));

        #endregion

        #region 通知

        /// <summary>
        /// 背景色変更通知処理
        /// </summary>
        /// <param name="sender">送信元</param>
        /// <param name="e">イベント</param>
        /// 通知された合法手の配列を基に背景色を設定する
        private void BackPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!"Back".Equals(e.PropertyName))
            {
                return;
            }
            var p = sender as GameMaster;
            if (Data.Length != p?.Back.Length)
            {
                return;
            }
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i].BackColor = p.Back[i] ? "LimeGreen" : "Green";
            }
        }

        /// <summary>
        /// 石色変更通知処理
        /// </summary>
        /// <param name="sender">送信元</param>
        /// <param name="e">イベント</param>
        /// 通知された配列と既存の石色を基に石色と更新色を設定する
        private void DataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!"Data".Equals(e.PropertyName))
            {
                return;
            }
            var p = sender as GameMaster;
            if (Data.Length != p?.Data.Length)
            {
                return;
            }
            for (int i = 0; i < Data.Length; i++)
            {
                // 新規なら更新色を着手色に設定。反転なら更新色をダミーに設定。
                // 連続更新時に更新色が残り反転処理が重複するため消去すること。
                // 反転用データトリガ判定のため、石色->更新色の順序とすること。
                Data[i].LastColor = "Transparent";
                switch (p.Data[i])
                {
                    case Common.BLACK:
                        string b = (Data[i].Color == "Transparent") ? "Gray" : (Data[i].Color == "White") ? "Red" : "Black";
                        Data[i].Color = "Black";
                        Data[i].LastColor = b;
                        break;
                    case Common.WHITE:
                        string w = (Data[i].Color == "Transparent") ? "Gray" : (Data[i].Color == "Black") ? "Red" : "White";
                        Data[i].Color = "White";
                        Data[i].LastColor = w;
                        break;
                    default:
                        Data[i].Color = "Transparent";
                        Data[i].LastColor = "Transparent";
                        break;
                }
            }
        }

        /// <summary>
        /// 情報変更通知処理
        /// </summary>
        /// <param name="sender">送信元</param>
        /// <param name="e">イベント</param>
        /// 通知された各種情報用の配列をインデクサに変換する
        /// @note 配列は[((文字色,文字列),情報数),石数]の構成。ビューと一致させること。
        private void InfoPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!"Info".Equals(e.PropertyName))
            {
                return;
            }
            var p = sender as GameMaster;
            if (Data.Length * 8 != p?.Info.Length)
            {
                return;
            }
            for (int i = 0; i < Data.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Data[i][j] = p.Info[i * 8 + j];
                }
            }
        }

        /// <summary>
        /// ステータス変更通知処理
        /// </summary>
        /// <param name="sender">送信元</param>
        /// <param name="e">イベント</param>
        /// 通知されたステータスをビューのステータスに変換する
        /// @note 通知ステータス(@ref WpfApp.Models.GameStatus)とステータス文字列(@ref WpfApp.Models.Common::StatusString)から生成。ビューと一致させること。
        private void StatusPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!"Status".Equals(e.PropertyName))
            {
                return;
            }
            var p = sender as GameMaster;
            if (p == null || !Common.StatusString.ContainsKey(p.Status.StatusB) || !Common.StatusString.ContainsKey(p.Status.StatusW))
            {
                return;
            }
            var sb = Common.StatusString[p.Status.StatusB];
            var sw = Common.StatusString[p.Status.StatusW];
            status[0] = sb.Equals("") ? ("○：" + sw) : sw.Equals("") ? ("●：" + sb) : ("●：" + sb + "／○：" + sw);
            status[2] = "●：" + p.Status.CountB;
            status[3] = Master.PlayerB.Name;
            status[4] = "○：" + p.Status.CountW;
            status[5] = Master.PlayerW.Name;
            Status = status;
        }

        #endregion
    }

}
