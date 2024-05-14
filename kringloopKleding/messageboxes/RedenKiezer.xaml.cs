using System.Linq;
using System.Windows;

namespace kringloopKleding.messageboxes
{
    /// <summary>
    /// messagebox for selecting inactief reason<br/><br/>
    /// choice is stored in the choice field
    /// </summary>
    public partial class RedenKiezer : Window
    {
        readonly kringloopAfhalingDataContext db = new kringloopAfhalingDataContext();
        public string choice;
        /// <summary>
        /// default constructor
        /// </summary>
        public RedenKiezer()
        {
            InitializeComponent();
            dgredenen.ItemsSource = from r in db.redenens
                                    where r.reden != "1 jaar inactief"
                                    select r.reden;
        }
        /// <summary>
        /// sets choice and closes messagebox
        /// </summary>
        private void Select_Click(object sender, RoutedEventArgs e)
        {
            choice = (string)dgredenen.SelectedItem;
            Close();
        }
        /// <summary>
        /// enables select button when any row is clicked
        /// </summary>
        private void dgredenen_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            select.IsEnabled = dgredenen.SelectedItem != null;
        }
    }
}
