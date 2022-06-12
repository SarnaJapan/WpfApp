using WpfApp.Utils;

namespace WpfApp.ViewModels
{
    /// <summary>
    /// 情報ダイアログ
    /// </summary>
    public class InfoViewModel
    {
        #region ダイアログ表示対応

        /// <summary>
        /// ダイアログ終了処理
        /// </summary>
        private readonly System.Action CloseAction;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="closeAction">ダイアログ終了処理</param>
        public InfoViewModel(System.Action closeAction)
        {
            // 呼出元で定義した処理を登録する
            CloseAction = closeAction;
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
            CloseAction();
        },
        _ =>
        {
            return true;
        }));

        /// <summary>
        /// 終了処理
        /// </summary>
        public void Closing()
        {
            // 終了時にDialogBehaviorから呼び出される
        }

        #endregion
    }

}
