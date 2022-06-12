// メイン処理の版数。CtrlViewModel.cs と一致させること。
// #define MODE_V1

using System.ComponentModel;
using System.Windows.Data;
using WpfApp.Models;
using WpfApp.Utils;

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
        /// 最新位置色
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
        /// @note [(文字色＆文字列),(情報数)]のインデクサを定義する。ビューと一致させること。
        private readonly string[] info = new string[] { "White", "", "White", "", "White", "", "White", "", };
        /// <summary>
        /// 情報用インデクサ
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
#if MODE_V1
        private readonly MasterV1 Master;
#else
        private readonly Master Master;
#endif

        /// <summary>
        /// 石データ
        /// </summary>
        public Stone[] Data { get; set; } = new Stone[Common.SIZE * Common.SIZE];

        /// <summary>
        /// ゲームマスターと石データの初期設定
        /// </summary>
        public Board()
        {
            // ゲームマスター生成：モデルインスタンスを保持する
#if MODE_V1
            Master = new MasterV1();
#else
            Master = new Master();
#endif
            Master.PropertyChanged += BackPropertyChanged;
            Master.PropertyChanged += DataPropertyChanged;
            Master.PropertyChanged += InfoPropertyChanged;
            Master.PropertyChanged += TitlePropertyChanged;

            for (int i = 0; i < Common.SIZE; i++)
            {
                for (int j = 0; j < Common.SIZE; j++)
                {
                    Data[i * Common.SIZE + j] = new Stone { RowIndex = i, ColumnIndex = j, Color = "Transparent", BackColor = "Green", LastColor = "Transparent", };
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
                    // 情報表示、設定表示、開始、選択の同時起動を抑止
                    InfoCommand.RaiseCanExecuteChanged();
                    CtrlCommand.RaiseCanExecuteChanged();
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
        /// 情報ダイアログ表示
        /// </summary>
        private void OpenInfoDialog() => InfoViewModel = new InfoViewModel(CloseInfoDialog);

        /// <summary>
        /// 情報ダイアログ表示終了
        /// </summary>
        private void CloseInfoDialog() => InfoViewModel = null;

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

        /// <summary>
        /// 設定ダイアログ表示
        /// </summary>
        private void OpenCtrlDialog() => CtrlViewModel = new CtrlViewModel(CloseCtrlDialog, Master);

        /// <summary>
        /// 設定ダイアログ表示終了
        /// </summary>
        private void CloseCtrlDialog() => CtrlViewModel = null;

        #endregion

        #region コマンド

        /// <summary>
        /// <see cref="InfoCommand"/>
        /// </summary>
        private DelegateCommand infoCommand;
        /// <summary>
        /// 情報表示コマンド
        /// </summary>
        public DelegateCommand InfoCommand => infoCommand ?? (infoCommand = new DelegateCommand(_ =>
        {
            IsBusy = true;
            OpenInfoDialog();
            IsBusy = false;
        },
        _ =>
        {
            return !IsBusy;
        }));

        /// <summary>
        /// <see cref="CtrlCommand"/>
        /// </summary>
        private DelegateCommand ctrlCommand;
        /// <summary>
        /// 設定表示コマンド
        /// </summary>
        public DelegateCommand CtrlCommand => ctrlCommand ?? (ctrlCommand = new DelegateCommand(_ =>
        {
            IsBusy = true;
            OpenCtrlDialog();
            IsBusy = false;
        },
        _ =>
        {
            return !IsBusy;
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
        private void BackPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!"Back".Equals(e.PropertyName))
            {
                return;
            }
#if MODE_V1
            var p = sender as MasterV1;
#else
            var p = sender as Master;
#endif
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
        private void DataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!"Data".Equals(e.PropertyName))
            {
                return;
            }
#if MODE_V1
            var p = sender as MasterV1;
#else
            var p = sender as Master;
#endif
            if (Data.Length != p?.Data.Length)
            {
                return;
            }
            for (int i = 0; i < Data.Length; i++)
            {
                switch (p.Data[i])
                {
                    case Common.BLACK:
                        Data[i].LastColor = (Data[i].Color == "Transparent") ? "Gray" : "Transparent";
                        Data[i].Color = "Black";
                        break;
                    case Common.WHITE:
                        Data[i].LastColor = (Data[i].Color == "Transparent") ? "Gray" : "Transparent";
                        Data[i].Color = "White";
                        break;
                    default:
                        Data[i].LastColor = "Transparent";
                        Data[i].Color = "Transparent";
                        break;
                }
            }
        }

        /// <summary>
        /// 情報変更通知処理
        /// </summary>
        /// <param name="sender">送信元</param>
        /// <param name="e">イベント</param>
        /// @note [(文字色＆文字列),(情報数),(石数)]のパラメータを変換する。ビューと一致させること。
        private void InfoPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!"Info".Equals(e.PropertyName))
            {
                return;
            }
#if MODE_V1
            var p = sender as MasterV1;
#else
            var p = sender as Master;
#endif
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
        /// タイトル変更通知処理
        /// </summary>
        /// <param name="sender">送信元</param>
        /// <param name="e">イベント</param>
        private void TitlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!"Title".Equals(e.PropertyName))
            {
                return;
            }
#if MODE_V1
            var p = sender as MasterV1;
#else
            var p = sender as Master;
#endif
            Title = p?.Title ?? "";
        }

        #endregion
    }

}
