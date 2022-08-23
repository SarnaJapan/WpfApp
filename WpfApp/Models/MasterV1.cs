using System.Collections.Generic;
using System.Linq;
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
        /// コンストラクタ：プレイヤーの登録
        /// </summary>
        public MasterV1()
        {
            var playerlistv1 = new List<IOthelloPlayerV1>() { new PlayerNullV1(), new PlayerRandV1(), new PlayerMaxCountV1(), new PlayerMinOpenV1(),
                new PlayerMCV1(), };
            // デフォルトプレイヤー登録
            foreach (var item in playerlistv1)
            {
                PlayerMap.Add(item.Name, item);
            }

            // プラグインプレイヤー登録
            foreach (var item in Common.GetPluginList<IOthelloPlayerV1>())
            {
                PlayerMap.Add(item.Name + Common.PLUGIN_SUF, item);
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
            SetTitle(Common.TITLE_PLAYER);
        }

        /// <summary>
        /// 位置選択
        /// </summary>
        /// <param name="pos">クリック位置</param>
        public void GameSelectPos(int pos)
        {
            if (Turn != Common.EMPTY)
            {
                // 合法手があるなら
                if (ToolsV1.CheckNext(Turn, data))
                {
                    // 戦略が対人または計算不可ならクリック位置を選択
                    var p = (Turn == Common.BLACK) ? PlayerB.Calc(Turn, data) : PlayerW.Calc(Turn, data);
                    if (p < 0)
                    {
                        p = pos;
                    }
                    // 合法手なら反転してターン終了
                    var flip = ToolsV1.GetFlip(Turn, data, p);
                    if (flip.Count > 0)
                    {
                        ToolsV1.FlipData(Turn, data, flip);
                        GameEndTurn(false);
                    }
                }
                // 合法手がないならターン終了
                else
                {
                    GameEndTurn(true);
                }
            }
        }

        /// <summary>
        /// ターン終了
        /// </summary>
        /// <param name="pass">パス状態</param>
        private void GameEndTurn(bool pass)
        {
            // ターン交替
            Turn *= -1;
            SetBack(false);
            SetData();
            SetInfo(false);
            SetTitle(Common.TITLE_WAIT);

            // 合法手がないならパス
            if (!ToolsV1.CheckNext(Turn, data))
            {
                SetTitle(Common.TITLE_PASS);
                // 前回ターンがパスならゲーム終了
                if (pass)
                {
                    SetTitle(Common.TITLE_RESULT);
                    Turn = Common.EMPTY;
                }
            }

            // 自動進行
            if (AutomaticMove)
            {
                GameSelectPos(-1);
            }
        }

        /// <summary>
        /// <see cref="Title"/>
        /// </summary>
        private string title = "";
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
        /// タイトル設定
        /// </summary>
        /// <param name="type">タイトル種別</param>
        /// 各種状況をタイトルとしてまとめて通知する
        public void SetTitle(int type)
        {
            if (Turn != Common.EMPTY)
            {
                switch (type)
                {
                    case Common.TITLE_PLAYER:
                        var sb = PlayerB.Name;
                        var sw = PlayerW.Name;
                        Title = $"●：{sb}／○：{sw}";
                        break;
                    case Common.TITLE_RESULT:
                        var cb = data.Count(i => i == Common.BLACK);
                        var cw = data.Count(i => i == Common.WHITE);
                        Title = $"●：{cb}／○：{cw}";
                        break;
                    case Common.TITLE_WAIT:
                        Title = (Turn == Common.BLACK) ? "●：待機中…" : "○：待機中…";
                        break;
                    case Common.TITLE_PASS:
                        Title = (Turn == Common.BLACK) ? "●：パス" : "○：パス";
                        break;
                    default:
                        break;
                }
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
        /// 盤面をそのまま通知する
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
        /// 合法手を背景色情報用の配列として通知する
        public void SetBack(bool forced)
        {
            if (ShowCandidate)
            {
                for (int i = 0; i < back.Length; i++)
                {
                    back[i] = ToolsV1.GetFlip(Turn, data, i).Count > 0;
                }
                Back = back;
            }
            else if (forced)
            {
                // 候補表示なしに変更時は初期化
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
        public void SetInfo(bool forced)
        {
            if (forced)
            {
                // 設定変更時は初期化
                for (int i = 0; i < Common.SIZE * Common.SIZE; i++)
                {
                    info[8 * i + 0] = "White";
                    info[8 * i + 1] = i.ToString();
                    info[8 * i + 2] = "White";
                    info[8 * i + 3] = "";
                    info[8 * i + 4] = "White";
                    info[8 * i + 5] = "";
                    info[8 * i + 6] = "White";
                    info[8 * i + 7] = "";
                }
                Info = info;
            }
            var score = PlayerE.Score(Turn, data);
            if (score?.Length == Common.SIZE * Common.SIZE)
            {
                var order = score.OrderByDescending(n => n);
                var top3 = order.ElementAtOrDefault(2);
                for (int i = 0; i < Common.SIZE * Common.SIZE; i++)
                {
                    info[8 * i + 6] = (score[i] >= top3) ? "Red" : "Blue";
                    info[8 * i + 7] = double.IsNaN(score[i]) ? "" : score[i].ToString("F3");
                }
                Info = info;
            }
        }
    }

}
