using System.Windows;

namespace WpfApp.Utils
{
    /// <summary>
    /// ダイアログ表示ビヘイビア
    /// </summary>
    public static class DialogBehavior
    {
        /// <summary>
        /// テンプレート
        /// </summary>
        public static readonly DependencyProperty TemplateProperty =
        DependencyProperty.RegisterAttached(
        "Template",
        typeof(DataTemplate),
        typeof(DialogBehavior),
        new PropertyMetadata(null));

        /// <summary>
        /// コンテンツ
        /// </summary>
        public static readonly DependencyProperty ContentProperty =
        DependencyProperty.RegisterAttached(
        "Content",
        typeof(object),
        typeof(DialogBehavior),
        new PropertyMetadata(null, OnPropertyChanged));

        /// <summary>
        /// タイトル
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
        DependencyProperty.RegisterAttached(
        "Title",
        typeof(string),
        typeof(DialogBehavior),
        new PropertyMetadata(""));

        /// <summary>
        /// 終了コマンド
        /// </summary>
        public static readonly DependencyProperty CloseCommandProperty =
        DependencyProperty.RegisterAttached(
        "CloseCommand",
        typeof(DelegateCommand),
        typeof(DialogBehavior),
        new PropertyMetadata(null));

        /// <summary>
        /// ダイアログ
        /// </summary>
        private static readonly DependencyProperty DialogProperty =
        DependencyProperty.RegisterAttached(
        "Dialog",
        typeof(Window),
        typeof(DialogBehavior));

        /// <summary>
        /// テンプレート取得
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>テンプレート</returns>
        public static DataTemplate GetTemplate(DependencyObject obj) => (DataTemplate)obj.GetValue(TemplateProperty);

        /// <summary>
        /// テンプレート設定
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value">テンプレート</param>
        public static void SetTemplate(DependencyObject obj, DataTemplate value) => obj.SetValue(TemplateProperty, value);

        /// <summary>
        /// コンテンツ取得
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>コンテンツ</returns>
        public static object GetContent(DependencyObject obj) => obj.GetValue(ContentProperty);

        /// <summary>
        /// コンテンツ設定
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value">コンテンツ</param>
        public static void SetContent(DependencyObject obj, object value) => obj.SetValue(ContentProperty, value);

        /// <summary>
        /// タイトル取得
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>タイトル</returns>
        public static string GetTitle(DependencyObject obj) => (string)obj.GetValue(TitleProperty);

        /// <summary>
        /// タイトル設定
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value">タイトル</param>
        public static void SetTitle(DependencyObject obj, string value) => obj.SetValue(TitleProperty, value);

        /// <summary>
        /// 終了コマンド取得
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>終了コマンド</returns>
        public static DelegateCommand GetCloseCommand(DependencyObject obj) => (DelegateCommand)obj.GetValue(CloseCommandProperty);

        /// <summary>
        /// 終了コマンド設定
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value">終了コマンド</param>
        public static void SetCloseCommand(DependencyObject obj, DelegateCommand value) => obj.SetValue(CloseCommandProperty, value);

        /// <summary>
        /// ダイアログ取得
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>ダイアログ</returns>
        private static Window GetDialog(DependencyObject obj) => (Window)obj.GetValue(DialogProperty);

        /// <summary>
        /// ダイアログ設定
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value">ダイアログ</param>
        private static void SetDialog(DependencyObject obj, Window value) => obj.SetValue(DialogProperty, value);

        /// <summary>
        /// プロパティ変更処理
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        private static void OnPropertyChanged(object obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is FrameworkElement element)
            {
                if (GetTemplate(element) != null)
                {
                    if (GetDialog(element) != null)
                    {
                        CloseDialog(element);
                    }
                    if (GetContent(element) != null)
                    {
                        OpenDialog(element);
                    }
                }
            }
        }

        /// <summary>
        /// ダイアログ表示
        /// </summary>
        /// <param name="element"></param>
        private static void OpenDialog(FrameworkElement element)
        {
            var dialog = new Window
            {
                WindowStyle = WindowStyle.SingleBorderWindow,
                ResizeMode = ResizeMode.NoResize,
                SizeToContent = SizeToContent.WidthAndHeight,
                ContentTemplate = GetTemplate(element),
                Content = GetContent(element),
                Title = GetTitle(element),
            };
            var cmd = GetCloseCommand(element);
            dialog.Closed += (s, e) =>
            {
                if (cmd != null)
                {
                    if (cmd.CanExecute(dialog.Content))
                    {
                        cmd.Execute(dialog.Content);
                    }
                }
            };
            SetDialog(element, dialog);
            dialog.ShowDialog();
        }

        /// <summary>
        /// ダイアログ終了
        /// </summary>
        /// <param name="element"></param>
        private static void CloseDialog(FrameworkElement element)
        {
            GetDialog(element).Close();
            SetDialog(element, null);
        }
    }

    /// <summary>
    /// ウィンドウ終了ビヘイビア
    /// </summary>
    public static class CloseWindowBehavior
    {
        /// <summary>
        /// 終了プロパティ
        /// </summary>
        public static readonly DependencyProperty CloseProperty =
        DependencyProperty.RegisterAttached(
        "Close",
        typeof(bool),
        typeof(CloseWindowBehavior),
        new PropertyMetadata(false, OnPropertyChanged));

        /// <summary>
        /// 終了プロパティ設定
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>終了プロパティ</returns>
        public static bool GetClose(DependencyObject obj) => (bool)obj.GetValue(CloseProperty);

        /// <summary>
        /// 終了プロパティ取得
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value">終了プロパティ</param>
        public static void SetClose(DependencyObject obj, bool value) => obj.SetValue(CloseProperty, value);

        /// <summary>
        /// プロパティ変更処理
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        private static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (!(obj is Window win))
            {
                win = Window.GetWindow(obj);
            }
            if (GetClose(obj))
            {
                win.Close();
            }
        }
    }

}
