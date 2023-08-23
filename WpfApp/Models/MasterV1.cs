using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WpfApp.Utils;
using WpfLib.OthelloInterface;

namespace WpfApp.Models
{
    /// <summary>
    /// ゲームマスター
    /// </summary>
    /// プレイヤーとゲーム進行を管理する
    public class MasterV1 : NotificationObject
    {
        /// <summary>
        /// プレイヤーマップ：選択可能プレイヤー
        /// </summary>
        public Dictionary<string, IOthelloPlayerV1> PlayerMap = new Dictionary<string, IOthelloPlayerV1>();

        /// <summary>
        /// ゲームプレイヤー：黒
        /// </summary>
        public IOthelloPlayerV1 PlayerB;

        /// <summary>
        /// ゲームプレイヤー：白
        /// </summary>
        public IOthelloPlayerV1 PlayerW;

        /// <summary>
        /// 評価用プレイヤー
        /// </summary>
        public IOthelloPlayerV1 PlayerE;

        /// <summary>
        /// 自動進行
        /// </summary>
        public bool AutomaticMove = false;

        /// <summary>
        /// 候補表示
        /// </summary>
        public bool ShowCandidate = false;

        /// <summary>
        /// 対戦状態
        /// </summary>
        private int Turn;

        /// <summary>
        /// 一時保存フラグ
        /// </summary>
        private bool SaveFlag;

        /// <summary>
        /// 対戦状態（一時保存）
        /// </summary>
        private int SaveTurn;

        /// <summary>
        /// 石色（一時保存）
        /// </summary>
        private readonly int[] SaveData = new int[Common.SIZE * Common.SIZE];

        /// <summary>
        /// 計算結果取得タスク
        /// </summary>
        private Task<int> CalcTask;

        /// <summary>
        /// 評価値取得タスク
        /// </summary>
        private Task<double[]> ScoreTask;

        /// <summary>
        /// コンストラクタ：プレイヤーの登録
        /// </summary>
        public MasterV1()
        {
            // デフォルトプレイヤー登録
            var playerlistv1 = new List<IOthelloPlayerV1>() { new PlayerNullV1(), new PlayerRandV1(), new PlayerMaxCountV1(), new PlayerMinOpenV1(),
                new PlayerMCV1(), };
            foreach (var item in playerlistv1)
            {
                PlayerMap.Add(item.Name, item);
            }

            // プラグインプレイヤー登録
            foreach (var item in Common.GetPluginList<IOthelloPlayerV1>())
            {
                PlayerMap.Add(item.Name + " *", item);
            }

            // 登録済先頭プレイヤー選択
            PlayerB = PlayerMap.First().Value;
            PlayerW = PlayerMap.First().Value;
            PlayerE = PlayerMap.First().Value;
        }

        /// <summary>
        /// ゲーム開始
        /// </summary>
        public void GameStart()
        {
            Turn = Common.BLACK;
            ToolsV1.InitData(data);
            SetBack(false);
            SetData();
            // 更新色表示削除
            SetData();
            SetInfo(false);
            SetStatus(Common.STATUS_INIT);
        }

        /// <summary>
        /// 位置選択
        /// </summary>
        /// <param name="pos">クリック位置</param>
        /// @note Calc()を非同期化。処理完了待機中の要求は破棄するが自動進行時なら遅延実行。
        public async void GameSelectPos(int pos)
        {
            if (Turn != Common.EMPTY)
            {
                // 処理完了済みなら
                if (CalcTask == null)
                {
                    // 合法手があるなら
                    if (ToolsV1.CheckNext(Turn, data))
                    {
                        SetStatus(Common.STATUS_CALC);
                        // 戦略が手動または計算不可ならクリック位置を選択
                        CalcTask = (Turn == Common.BLACK) ? Task.Run(() => PlayerB.Calc(Turn, data)) : Task.Run(() => PlayerW.Calc(Turn, data));
                        var p = await CalcTask;
                        if (p < 0)
                        {
                            p = pos;
                        }
                        // 合法手なら反転してターン終了
                        var flip = ToolsV1.GetFlip(Turn, data, p);
                        if (flip.Count > 0)
                        {
                            ToolsV1.FlipData(Turn, data, flip);
                            GameEndTurn(p);
                        }
                        CalcTask = null;
                    }
                    else
                    {
                        // ターン終了
                        GameEndTurn(-1);
                    }
                }
                else
                {
                    // 自動進行時なら遅延実行
                    if (pos == -1)
                    {
                        await Task.Delay(10);
                        GameSelectPos(-1);
                    }
                }
            }
        }

        /// <summary>
        /// ターン終了
        /// </summary>
        /// <param name="pos">置石位置</param>
        private void GameEndTurn(int pos)
        {
            // ターン交替
            Turn *= -1;
            SetBack(false);
            SetData();
            SetInfo(false);
            SetStatus(Common.STATUS_NEXT, pos);

            // 合法手がないなら
            if (!ToolsV1.CheckNext(Turn, data))
            {
                // 前回ターンがパスならゲーム終了
                if (pos < 0)
                {
                    SetStatus(Common.STATUS_RESULT);
                    Turn = Common.EMPTY;
                }
                // 前回ターンがパスでなければパス
                else
                {
                    SetStatus(Common.STATUS_PASS);
                }
            }

            // 自動進行
            if (AutomaticMove)
            {
                GameSelectPos(-1);
            }
        }

        /// <summary>
        /// <see cref="Data"/>
        /// </summary>
        private int[] data = new int[Common.SIZE * Common.SIZE];
        /// <summary>
        /// 石色
        /// </summary>
        public int[] Data
        {
            get => data;
            set
            {
                data = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 石色設定
        /// </summary>
        /// 盤面の配列をそのまま通知する
        private void SetData()
        {
            Data = data;
        }

        /// <summary>
        /// <see cref="Back"/>
        /// </summary>
        private bool[] back = new bool[Common.SIZE * Common.SIZE];
        /// <summary>
        /// 背景色
        /// </summary>
        public bool[] Back
        {
            get => back;
            set
            {
                back = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 背景色設定
        /// </summary>
        /// <param name="forced">強制設定</param>
        /// 合法手を背景色用の配列として通知する
        public void SetBack(bool forced)
        {
            if (ShowCandidate)
            {
                for (int i = 0; i < back.Length; i++)
                {
                    back[i] = ToolsV1.GetFlip(Turn, Data, i).Count > 0;
                }
                Back = back;
            }
            else if (forced)
            {
                // 候補表示なしに設定変更時は初期化
                for (int i = 0; i < back.Length; i++) { back[i] = false; }
                Back = back;
            }
        }

        /// <summary>
        /// <see cref="Info"/>
        /// </summary>
        private string[] info = new string[Common.SIZE * Common.SIZE * 8];
        /// <summary>
        /// 各種情報
        /// </summary>
        public string[] Info
        {
            get => info;
            set
            {
                info = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 各種情報設定
        /// </summary>
        /// <param name="forced">強制設定</param>
        /// 評価値を各種情報用の配列に変換して通知する
        /// @note Score()を非同期化。処理完了待機中の要求は破棄して最新情報のみ反映。
        public async void SetInfo(bool forced)
        {
            if (forced)
            {
                // 設定変更時は初期化
                for (int i = 0; i < Common.SIZE * Common.SIZE; i++)
                {
                    info[8 * i + 0] = "White";
                    info[8 * i + 1] = "";
                    info[8 * i + 2] = "White";
                    info[8 * i + 3] = "";
                    info[8 * i + 4] = "White";
                    info[8 * i + 5] = "";
                    info[8 * i + 6] = "White";
                    info[8 * i + 7] = "";
                }
                Info = info;
            }
            // 最新情報一時保存
            SaveFlag = true;
            SaveTurn = Turn;
            System.Array.Copy(Data, SaveData, Data.Length);
            // 処理完了済みなら
            if (ScoreTask == null)
            {
                double[] score = null;
                // 最新情報が有効なら
                while (SaveFlag)
                {
                    SaveFlag = false;
                    // 最新情報の評価値を取得
                    ScoreTask = Task.Run(() => PlayerE.Score(SaveTurn, SaveData));
                    score = await ScoreTask;
                }
                if (score?.Length == Common.SIZE * Common.SIZE)
                {
                    // インデックスと評価値を設定
                    var order = score.OrderByDescending(n => n);
                    var top3 = order.ElementAtOrDefault(2);
                    for (int i = 0; i < Common.SIZE * Common.SIZE; i++)
                    {
                        info[8 * i + 0] = "White";
                        info[8 * i + 1] = i.ToString();
                        info[8 * i + 6] = (score[i] >= top3) ? "Red" : "Blue";
                        info[8 * i + 7] = double.IsNaN(score[i]) ? "" : score[i].ToString("F3");
                    }
                    Info = info;
                }
                ScoreTask = null;
            }
        }

        /// <summary>
        /// <see cref="Status"/>
        /// </summary>
        private GameStatus status = new GameStatus();
        /// <summary>
        /// ステータス
        /// </summary>
        public GameStatus Status
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// ステータス設定
        /// </summary>
        /// <param name="type">ステータス種別</param>
        /// <param name="pos">置石位置（ターン終了時のみ）</param>
        /// ゲームの進行状況をステータスとしてまとめて通知する
        public void SetStatus(int type, int pos = -1)
        {
            if (Turn != Common.EMPTY)
            {
                switch (type)
                {
                    case Common.STATUS_INIT:
                        // 履歴初期化
                        status.Record.Clear();
                        status.CountB = Data.Count(i => i == Common.BLACK);
                        status.CountW = Data.Count(i => i == Common.WHITE);
                        // 戦略種別を基にステータス文字列を設定
                        status.StatusB = (Turn == Common.BLACK) ? (Common.IsManual(PlayerB.Version) ? Common.STATUS_WAIT : Common.STATUS_NEXT) : Common.STATUS_EMPTY;
                        status.StatusW = (Turn == Common.WHITE) ? (Common.IsManual(PlayerW.Version) ? Common.STATUS_WAIT : Common.STATUS_NEXT) : Common.STATUS_EMPTY;
                        break;
                    case Common.STATUS_CTRL:
                        // 戦略種別を基にステータス文字列を設定
                        status.StatusB = (Turn == Common.BLACK) ? (Common.IsManual(PlayerB.Version) ? Common.STATUS_WAIT : Common.STATUS_NEXT) : Common.STATUS_EMPTY;
                        status.StatusW = (Turn == Common.WHITE) ? (Common.IsManual(PlayerW.Version) ? Common.STATUS_WAIT : Common.STATUS_NEXT) : Common.STATUS_EMPTY;
                        break;
                    case Common.STATUS_NEXT:
                        // 履歴更新
                        status.Record.Add(pos);
                        status.CountB = Data.Count(i => i == Common.BLACK);
                        status.CountW = Data.Count(i => i == Common.WHITE);
                        // 戦略種別を基にステータス文字列を設定
                        status.StatusB = (Turn == Common.BLACK) ? (Common.IsManual(PlayerB.Version) ? Common.STATUS_WAIT : Common.STATUS_NEXT) : Common.STATUS_EMPTY;
                        status.StatusW = (Turn == Common.WHITE) ? (Common.IsManual(PlayerW.Version) ? Common.STATUS_WAIT : Common.STATUS_NEXT) : Common.STATUS_EMPTY;
                        break;
                    case Common.STATUS_RESULT:
                        // 結果を基にステータス文字列を設定
                        status.CountB = Data.Count(i => i == Common.BLACK);
                        status.CountW = Data.Count(i => i == Common.WHITE);
                        status.StatusB = (status.CountB > status.CountW) ? Common.STATUS_WIN : (status.CountB < status.CountW) ? Common.STATUS_LOSE : Common.STATUS_DRAW;
                        status.StatusW = (status.CountB < status.CountW) ? Common.STATUS_WIN : (status.CountB > status.CountW) ? Common.STATUS_LOSE : Common.STATUS_DRAW;
                        break;
                    case Common.STATUS_CALC:
                        // ステータス文字列のみ設定
                        status.StatusB = (Turn == Common.BLACK) ? (Common.IsManual(PlayerB.Version) ? status.StatusB : Common.STATUS_CALC) : Common.STATUS_EMPTY;
                        status.StatusW = (Turn == Common.WHITE) ? (Common.IsManual(PlayerW.Version) ? status.StatusW : Common.STATUS_CALC) : Common.STATUS_EMPTY;
                        break;
                    case Common.STATUS_PASS:
                        // ステータス文字列のみ設定
                        status.StatusB = (Turn == Common.BLACK) ? Common.STATUS_PASS : Common.STATUS_EMPTY;
                        status.StatusW = (Turn == Common.WHITE) ? Common.STATUS_PASS : Common.STATUS_EMPTY;
                        break;
                    default:
                        break;
                }
                Status = status;
            }
        }
    }

}
