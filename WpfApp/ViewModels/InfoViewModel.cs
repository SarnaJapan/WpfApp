using WpfApp.Utils;

namespace WpfApp.ViewModels
{
    /// <summary>
    /// 情報ダイアログ
    /// </summary>
    public class InfoViewModel : NotificationObject
    {
        #region ダイアログ表示対応

        /// <summary>
        /// <see cref="CloseWindow"/>
        /// </summary>
        private bool closeWindow = false;
        /// <summary>
        /// ダイアログ終了フラグ
        /// </summary>
        public bool CloseWindow
        {
            get => closeWindow;
            set
            {
                closeWindow = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// <see cref="CloseCommand"/>
        /// </summary>
        private DelegateCommand closeCommand;
        /// <summary>
        /// ダイアログ終了コマンド
        /// </summary>
        public DelegateCommand CloseCommand => closeCommand ?? (closeCommand = new DelegateCommand(_ =>
        {
            CloseWindow = true;
        },
        _ =>
        {
            return true;
        }));

        #endregion
    }

}
