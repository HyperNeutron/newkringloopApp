using System.Windows;

namespace kringloopKleding
{
    /// <summary>
    /// yes/no choice
    /// </summary>
    public partial class ChoiceCustom : Window
    {
        public ChoiceCustom(string text, string btn1, string btn2)
        {
            InitializeComponent();
            lblTitle.Text = text;
            btnJa.Content = btn1;
            btnNee.Content = btn2;
        }
        /// <summary>
        /// cancel switching windows
        /// </summary>
        private void btnJa_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        /// <summary>
        /// IGNORE warning
        /// </summary>
        private void btnNee_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
