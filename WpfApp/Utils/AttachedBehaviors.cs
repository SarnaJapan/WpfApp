using System.ComponentModel;
using System.Windows;

namespace Utils
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
        new PropertyMetadata(null, OnPropertyChanged));

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
        new PropertyMetadata("", OnPropertyChanged));

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
        /// タイトル取得
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value">タイトル</param>
        public static void SetTitle(DependencyObject obj, string value) => obj.SetValue(TitleProperty, value);

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
            dialog.Closing += DialogClosing;
            SetDialog(element, dialog);
            dialog.ShowDialog();
        }

        /// <summary>
        /// ダイアログ表示終了
        /// </summary>
        /// <param name="element"></param>
        private static void CloseDialog(FrameworkElement element)
        {
            GetDialog(element).Close();
            SetDialog(element, null);
        }

        /// <summary>
        /// ダイアログ終了処理
        /// </summary>
        /// <param name="sender">送信元</param>
        /// <param name="e">イベント</param>
        private static void DialogClosing(object sender, CancelEventArgs e)
        {
            if (sender is Window element)
            {
                element.Content.GetType().GetMethod("Closing")?.Invoke(element.Content, null);
            }
        }
    }

}
