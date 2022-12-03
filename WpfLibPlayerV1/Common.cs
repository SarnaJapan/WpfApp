namespace WpfLibPlayerV1
{
    /// <summary>
    /// 共通処理
    /// </summary>
    internal static class Common
    {
        /// <summary>
        /// 盤サイズ
        /// </summary>
        public const int SIZE = 8;

        /// @name 石色
        /// @{
        public const int EMPTY = 0;
        public const int BLACK = 1;
        public const int WHITE = -1;
        /// @}

        /// @name バージョン情報
        /// @{

        /// <summary>
        /// 表示フォーマット
        /// </summary>
        /// @note (Major Version).(Minor Version).(Build Version)(Option)
        public const string VERSION_FORMAT = "{0:D}.{1:D}.{2:D}{3}";

        /// <summary>
        /// オプション：評価選択肢対象外
        /// </summary>
        /// 評価値が存在しないランダム戦略プレイヤー用オプション。設定時は評価選択肢の対象外となる。
        public const string OPTION_NOEVAL = "/NoEval";

        /// @}

        /// <summary>
        /// 乱数ジェネレータ
        /// </summary>
        private static readonly System.Random R = new System.Random();

        /// <summary>
        /// 乱数ジェネレータ用ロックオブジェクト
        /// </summary>
        private static readonly object RandLock = new object();

        /// <summary>
        /// 乱数取得
        /// </summary>
        /// <param name="max">最大値</param>
        /// <returns>0以上max未満の乱数</returns>
        public static int Rand(int max)
        {
            int res = 0;

            lock (RandLock)
            {
                res = R.Next(max);
            }

            return res;
        }
    }

}
