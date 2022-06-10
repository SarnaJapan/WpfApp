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
        /// @note 旧版のみ変更可能。BitBoard版は8のみ有効。ビューと一致させること。
        public const int SIZE = 8;

        /// @name 石色
        ///@{
        public const int NULL = 0;
        public const int BLACK = 1;
        public const int WHITE = -1;
        ///@}

        /// @name バージョン情報
        ///@{

        /// <summary>
        /// 表示フォーマット
        /// </summary>
        /// @note (Major Version).(Minor Version).(Build Version)(Option)
        public const string VERSION_FORMAT = "{0:D}.{1:D}.{2:D}{3}";

        /// <summary>
        /// オプション：評価選択肢対象外
        /// </summary>
        public const string OPTION_NOEVAL = "/NoEval";

        ///@}

        /// <summary>
        /// 疑似乱数ジェネレータ
        /// </summary>
        public static System.Random R = new System.Random();
    }

}
