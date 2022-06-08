using OthelloInterface;

using System.Collections.Generic;
using System.Linq;
using Utils;

namespace WpfApp.Models
{
    /// <summary>
    /// ゲームマスター：プレイヤーとゲーム進行を管理する
    /// </summary>
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
        /// コンストラクタ：プレイヤーを登録する
        /// </summary>
        public Master()
        {
            var playerlist = new List<IOthelloPlayer>() { new PlayerNull(), new PlayerRand(), new PlayerMaxCount(), new PlayerMinOpen(),
                new PlayerMC(), new PlayerMCTS(), new PlayerAccord(), new PlayerKelp(),
                new PlayerKelp() { Name = "Kelp 00", Param = 0 }, new PlayerKelp() { Name = "Kelp 01", Param = 1 },
                new PlayerKelp() { Name = "Kelp 02", Param = 2 }, new PlayerKelp() { Name = "Kelp 03", Param = 3 }, };
            // デフォルトプレイヤー登録
            foreach (var item in playerlist)
            {
                PlayerMap.Add(item.Name, item);
            }

            // プラグインプレイヤー登録
            foreach (var item in Common.GetPluginList<IOthelloPlayer>())
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
            BP = Common.BB_BLACK;
            BO = Common.BB_WHITE;
            SetBack(false);
            SetData();
            // 最新位置表示削除
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
            if (Turn != Common.NULL)
            {
                // 合法手があるなら
                var lm = Tools.LegalMove(BP, BO);
                if (lm != 0)
                {
                    // 戦略が対人または計算不可ならクリック位置を選択
                    var sp = (Turn == Common.BLACK) ? PlayerB.Calc(BP, BO) : PlayerW.Calc(BP, BO);
                    if (sp == 0)
                    {
                        sp = Tools.Pos2Bit(pos);
                    }
                    // 合法手なら反転してターン終了
                    if ((sp & lm) != 0)
                    {
                        Tools.Flip(ref BP, ref BO, sp);
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
            (BO, BP) = (BP, BO);
            SetBack(false);
            SetData();
            SetInfo(false);
            SetTitle(Common.TITLE_WAIT);

            // 合法手がないならパス
            if (Tools.LegalMove(BP, BO) == 0)
            {
                SetTitle(Common.TITLE_PASS);
                // 前回ターンがパスならゲーム終了
                if (pass)
                {
                    SetTitle(Common.TITLE_RESULT);
                    Turn = Common.NULL;
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
        public void SetTitle(int type)
        {
            if (Turn != Common.NULL)
            {
                switch (type)
                {
                    case Common.TITLE_PLAYER:
                        var sb = PlayerB.Name;
                        var sw = PlayerW.Name;
                        Title = $"●：{sb}／○：{sw}";
                        break;
                    case Common.TITLE_RESULT:
                        var cb = (Turn == Common.BLACK) ? Tools.BitCount(BP) : Tools.BitCount(BO);
                        var cw = (Turn == Common.BLACK) ? Tools.BitCount(BO) : Tools.BitCount(BP);
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
                // 候補表示なしに変更時は初期化
                for (int i = 0; i < back.Length; i++) { back[i] = false; }
                Back = back;
            }
        }

        /// <summary>
        /// <see cref="Info"/>
        /// </summary>
        private string[] info = new string[64 * 8];
        /// <summary>
        /// 情報
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
        /// 情報設定
        /// </summary>
        /// <param name="forced">強制設定</param>
        public void SetInfo(bool forced)
        {
            if (forced)
            {
                // 評価値なしに変更時は初期化
                for (int i = 0; i < 64; i++)
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
            var score = PlayerE.Score(BP, BO);
            if (score?.Length == 64)
            {
                var order = score.OrderByDescending(n => n);
                var top3 = order.ElementAtOrDefault(2);
                for (int i = 0; i < 64; i++)
                {
                    info[8 * i + 6] = (score[i] >= top3) ? "Red" : "Blue";
                    info[8 * i + 7] = double.IsNaN(score[i]) ? "" : score[i].ToString("F3");
                }
                Info = info;
            }
        }
    }

}
