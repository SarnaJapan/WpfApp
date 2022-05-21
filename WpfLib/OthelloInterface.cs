namespace OthelloInterface
{
    /// <summary>
    /// オセロプレイヤーインターフェース（旧版）
    /// </summary>
    public interface IOthelloPlayerV1
    {
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// バージョン
        /// </summary>
        string Version { get; }

        /// <summary>
        /// 計算結果取得
        /// </summary>
        /// <param name="color">石色</param>
        /// <param name="data">盤面</param>
        /// <returns>計算結果(>=0)／計算不可(=-1)</returns>
        int Calc(int color, int[] data);

        /// <summary>
        /// 評価値取得
        /// </summary>
        /// <param name="color">石色</param>
        /// <param name="data">盤面</param>
        /// <returns>評価値(double)／初期値(NaN)／未対応(null)</returns>
        double[] Score(int color, int[] data);
    }

    /// <summary>
    /// オセロプレイヤーインターフェース（BitBoard版）
    /// </summary>
    public interface IOthelloPlayer
    {
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// バージョン
        /// </summary>
        string Version { get; }

        /// <summary>
        /// 計算結果取得
        /// </summary>
        /// <param name="p">BitBoard：自分</param>
        /// <param name="o">BitBoard：相手</param>
        /// <returns>計算結果(>0)／計算不可(=0)</returns>
        ulong Calc(ulong p, ulong o);

        /// <summary>
        /// 評価値取得
        /// </summary>
        /// <param name="p">BitBoard：自分</param>
        /// <param name="o">BitBoard：相手</param>
        /// <returns>評価値(double)／初期値(NaN)／未対応(null)</returns>
        double[] Score(ulong p, ulong o);
    }

}
