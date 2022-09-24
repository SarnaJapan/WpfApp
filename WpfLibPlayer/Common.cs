﻿namespace WpfLibPlayer
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
        /// 疑似乱数ジェネレータ
        /// </summary>
        public static System.Random R = new System.Random();
    }

}
