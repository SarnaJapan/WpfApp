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
    public class Master : NotificationObject
    {
        /// <summary>
        /// プレイヤーマップ：選択可能プレイヤー
        /// </summary>
        public Dictionary<string, IOthelloPlayer> PlayerMap = new Dictionary<string, IOthelloPlayer>();

        /// <summary>
        /// ゲームプレイヤー：黒
        /// </summary>
        public IOthelloPlayer PlayerB;

        /// <summary>
        /// ゲームプレイヤー：白
        /// </summary>
        public IOthelloPlayer PlayerW;

        /// <summary>
        /// 評価用プレイヤー
        /// </summary>
        public IOthelloPlayer PlayerE;

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
        /// BitBoard：自分
        /// </summary>
        private ulong BP;

        /// <summary>
        /// BitBoard：相手
        /// </summary>
        private ulong BO;

        /// <summary>
        /// 一時保存フラグ
        /// </summary>
        private bool SaveFlag;

        /// <summary>
        /// BitBoard：自分（一時保存）
        /// </summary>
        private ulong SaveBP;

        /// <summary>
        /// BitBoard：相手（一時保存）
        /// </summary>
        private ulong SaveBO;

        /// <summary>
        /// 計算結果取得タスク
        /// </summary>
        private Task<ulong> CalcTask;

        /// <summary>
        /// 評価値取得タスク
        /// </summary>
        private Task<double[]> ScoreTask;

        /// <summary>
        /// コンストラクタ：プレイヤーの登録
        /// </summary>
        public Master()
        {
            // デフォルトプレイヤー登録
            var playerlist = new List<IOthelloPlayer>() { new PlayerNull(), new PlayerRand(), new PlayerMaxCount(), new PlayerMinOpen(),
                new PlayerMC(), new PlayerMCTS(), };
            foreach (var item in playerlist)
            {
                PlayerMap.Add(item.Name, item);
            }

            // プラグインプレイヤー登録
            foreach (var item in Common.GetPluginList<IOthelloPlayer>())
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
            BP = Common.BB_BLACK;
            BO = Common.BB_WHITE;
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
                    var lm = Tools.LegalMove(BP, BO);
                    if (lm != 0)
                    {
                        SetStatus(Common.STATUS_CALC);
                        // 戦略が手動または計算不可ならクリック位置を選択
                        CalcTask = (Turn == Common.BLACK) ? Task.Run(() => PlayerB.Calc(BP, BO)) : Task.Run(() => PlayerW.Calc(BP, BO));
                        var sp = await CalcTask;
                        if (sp == 0)
                        {
                            sp = Tools.Pos2Bit(pos);
                        }
                        // 合法手なら反転してターン終了
                        if ((sp & lm) != 0)
                        {
                            Tools.Flip(ref BP, ref BO, sp);
                            GameEndTurn(sp);
                        }
                        CalcTask = null;
                    }
                    else
                    {
                        // ターン終了
                        GameEndTurn(0);
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
        private void GameEndTurn(ulong pos)
        {
            // ターン交替
            Turn *= -1;
            var temp = BP;
            BP = BO;
            BO = temp;
            SetBack(false);
            SetData();
            SetInfo(false);
            SetStatus(Common.STATUS_NEXT, pos);

            // 合法手がないなら
            if (Tools.LegalMove(BP, BO) == 0)
            {
                // 前回ターンがパスならゲーム終了
                if (pos == 0)
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
        private int[] data = new int[64];
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
        /// 盤面をBitBoardから配列に変換して通知する
        private void SetData()
        {
            switch (Turn)
            {
                case Common.BLACK:
                    Tools.Bit2Array(BP, BO, data);
                    Data = data;
                    break;
                case Common.WHITE:
                    Tools.Bit2Array(BO, BP, data);
                    Data = data;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// <see cref="Back"/>
        /// </summary>
        private bool[] back = new bool[64];
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
                var lm = Tools.LegalMove(BP, BO);
                for (int i = 0; i < back.Length; i++)
                {
                    back[i] = (lm & Tools.Pos2Bit(i)) != 0;
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
        private string[] info = new string[64 * 8];
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
                for (int i = 0; i < 64; i++)
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
            SaveBP = BP;
            SaveBO = BO;
            // 処理完了済みなら
            if (ScoreTask == null)
            {
                double[] score = null;
                // 最新情報が有効なら
                while (SaveFlag)
                {
                    SaveFlag = false;
                    // 最新情報の評価値を取得
                    ScoreTask = Task.Run(() => PlayerE.Score(SaveBP, SaveBO));
                    score = await ScoreTask;
                }
                if (score?.Length == 64)
                {
                    // インデックスと評価値を設定
                    var order = score.OrderByDescending(n => n);
                    var top3 = order.ElementAtOrDefault(2);
                    for (int i = 0; i < 64; i++)
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
        public void SetStatus(int type, ulong pos = 0)
        {
            if (Turn != Common.EMPTY)
            {
                switch (type)
                {
                    case Common.STATUS_INIT:
                        // 履歴初期化
                        status.Record.Clear();
                        status.CountB = (Turn == Common.BLACK) ? Tools.BitCount(BP) : Tools.BitCount(BO);
                        status.CountW = (Turn == Common.WHITE) ? Tools.BitCount(BP) : Tools.BitCount(BO);
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
                        status.Record.Add(Tools.Bit2Pos(pos));
                        status.CountB = (Turn == Common.BLACK) ? Tools.BitCount(BP) : Tools.BitCount(BO);
                        status.CountW = (Turn == Common.WHITE) ? Tools.BitCount(BP) : Tools.BitCount(BO);
                        // 戦略種別を基にステータス文字列を設定
                        status.StatusB = (Turn == Common.BLACK) ? (Common.IsManual(PlayerB.Version) ? Common.STATUS_WAIT : Common.STATUS_NEXT) : Common.STATUS_EMPTY;
                        status.StatusW = (Turn == Common.WHITE) ? (Common.IsManual(PlayerW.Version) ? Common.STATUS_WAIT : Common.STATUS_NEXT) : Common.STATUS_EMPTY;
                        break;
                    case Common.STATUS_RESULT:
                        // 結果を基にステータス文字列を設定
                        status.CountB = (Turn == Common.BLACK) ? Tools.BitCount(BP) : Tools.BitCount(BO);
                        status.CountW = (Turn == Common.WHITE) ? Tools.BitCount(BP) : Tools.BitCount(BO);
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
