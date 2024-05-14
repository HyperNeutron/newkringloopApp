using System.Windows;

namespace kringloopKleding
{
    /// <summary>
    /// message box that displays up to 2 strings
    /// </summary>
    public partial class MessageCustom : Window
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="message2"></param>
        public MessageCustom(string message, string message2 = "")
        {
            InitializeComponent();
            txtTop.Content = message;
            txtBottom.Content = message2;
        }
        /// <summary>
        /// closes messagebox :O
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
