using System.Globalization;
using System.Windows.Data;

namespace Utils
{
    /// <summary>
    /// 座標変換処理
    /// </summary>
    /// @note 変換処理は @ref RowSize に依存
    public class PositionConverter : IMultiValueConverter
    {
        /// <summary>
        /// 行サイズ
        /// </summary>
        public int RowSize { get; set; } = 0;

        /// <summary>
        /// [ColumnIndex,RowIndex]を[Position]に変換
        /// </summary>
        /// <param name="values">[ColumnIndex,RowIndex]</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>[Position]</returns>
        public object Convert(object[] values, System.Type targetType, object parameter, CultureInfo culture)
        {
            int col, row;
            try
            {
                col = (int)values[0];
                row = (int)values[1];
            }
            catch (System.Exception)
            {
                throw;
            }
            return (row * RowSize) + col;
        }

        /// <summary>
        /// （未実装）
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetTypes"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object[] ConvertBack(object value, System.Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// BoolToVisibility反転変換処理
    /// </summary>
    /// @note 変換処理は BooleanToVisibilityConverter の反転
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// trueをHiddenにfalseをVisibleに変換
        /// </summary>
        /// <param name="value">[true|false]</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>[Hidden|Visible]</returns>
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            bool val;
            try
            {
                val = (bool)value;
            }
            catch (System.Exception)
            {
                throw;
            }
            if (targetType != typeof(System.Windows.Visibility))
            {
                throw new System.InvalidOperationException();
            }
            return val ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// （未実装）
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

}
