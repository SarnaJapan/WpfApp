using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace Utils
{
    /// <summary>
    /// 入力範囲確認(Integer)
    /// </summary>
    public class IntegerValidationRule : ValidationRule
    {
        /// <summary>
        /// 最小値
        /// </summary>
        public int Min { get; set; } = 0;

        /// <summary>
        /// 最大値
        /// </summary>
        public int Max { get; set; } = 65535;

        /// <summary>
        /// 入力範囲確認(Integer)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int i;
            try
            {
                i = int.Parse((string)value);
            }
            catch (System.Exception)
            {
                return new ValidationResult(false, "Should be integer.");
            }
            if (i < Min || i > Max)
            {
                return new ValidationResult(false, $"Should be {Min} - {Max}.");
            }
            return ValidationResult.ValidResult;
        }
    }

    /// <summary>
    /// パス存在確認
    /// </summary>
    public class PathValidationRule : ValidationRule
    {
        /// <summary>
        /// パス存在確認
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!Directory.Exists((string)value))
            {
                return new ValidationResult(false, "Invalid path.");
            }
            return ValidationResult.ValidResult;
        }
    }

}
