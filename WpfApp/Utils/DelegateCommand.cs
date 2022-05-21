using System.Windows.Input;

namespace Utils
{
    /// <summary>
    /// ICommand 実装基底クラス
    /// </summary>
    public class DelegateCommand : ICommand
    {
        /// <summary>
        /// コマンド生成
        /// </summary>
        /// <param name="execute">Execute処理</param>
        /// <param name="canExecute">CanExecute処理</param>
        public DelegateCommand(System.Action<object> execute, System.Func<object, bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Execute処理
        /// </summary>
        private readonly System.Action<object> execute;

        /// <summary>
        /// コマンド実行
        /// </summary>
        /// <param name="parameter">パラメータ</param>
        public void Execute(object parameter) => execute.Invoke(parameter ?? "");

        /// <summary>
        /// CanExecute処理
        /// </summary>
        private readonly System.Func<object, bool> canExecute;

        /// <summary>
        /// コマンド実行可否確認
        /// </summary>
        /// <param name="parameter">パラメータ</param>
        /// <returns>確認結果</returns>
        public bool CanExecute(object parameter) => canExecute == null || canExecute(parameter ?? "");

        /// <summary>
        /// CanExecuteChangedイベント
        /// </summary>
        public event System.EventHandler CanExecuteChanged;

        /// <summary>
        /// コマンド実行可否変更通知
        /// </summary>
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, System.EventArgs.Empty);
    }

}
