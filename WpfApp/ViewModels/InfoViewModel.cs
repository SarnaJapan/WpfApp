using WpfApp.Utils;

namespace WpfApp.ViewModels
{
    /// <summary>
    /// 情報ダイアログ
    /// </summary>
    /// @par [Communication Diagram]
    /// @dot
    /// digraph {
    ///   rankdir=LR;
    ///   subgraph cluster0 {
    ///     label=DialogBehavior
    ///     node00 [
    ///       shape=record
    ///       label="Title |Template |Content"]
    ///     node01 [
    ///       shape=record
    ///       label="DialogClosing()"]
    ///   }
    ///   subgraph cluster1 {
    ///     label=MainVeiw
    ///     node10 [
    ///       shape=record
    ///       label="DialogBehavior.Title |DialogBehavior.Template |DialogBehavior.Content"]
    ///     node11 [
    ///       shape=record
    ///       label="menu"]
    ///   }
    ///   subgraph cluster2 {
    ///     label=MainViewModel
    ///     node20 [
    ///       shape=record
    ///       label="openDialogCommand"]
    ///     node21 [
    ///       shape=record
    ///       label="OpenDialog() |CloseDialog()"]
    ///   }
    ///   node1->node2
    ///   node21->node2 [label="null"]
    ///   node20->node2 [label="set"]
    ///   node0->node1 [label = "set"]
    ///   node10->node2 [label = "binding"] 
    ///   
    ///   subgraph cluster3 {
    ///     label=DialogVeiw
    ///     node30 [
    ///       shape=record
    ///       label="menu"]
    ///   }
    ///   subgraph cluster4 {
    ///     label=DialogViewModel
    ///     node40 [
    ///       shape=record
    ///       label="DialogViewModel"]
    ///     node41 [
    ///       shape = record
    ///       label = "OpenDialogCommand"]
    ///     node42 [
    ///       shape = record
    ///       label = "CloseDialog() |OpenDialog()"]
    ///   }
    /// }
    /// @enddot
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
