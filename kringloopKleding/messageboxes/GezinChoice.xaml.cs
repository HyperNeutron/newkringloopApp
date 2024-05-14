using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace kringloopKleding
{
    /// <summary>
    /// card not active
    /// </summary>
    public partial class GezinChoice : Window
    {
        readonly kringloopAfhalingDataContext db = new kringloopAfhalingDataContext();
        public gezin choice;
        public GezinChoice(HashSet<int> gezinnen)
        {
            InitializeComponent();
            IEnumerable<int> maxgezinnen = gezinnen.Take(1750);
            dgGezin.ItemsSource = from gezin in db.gezins
                                  where maxgezinnen.Contains(gezin.id)
                                  select gezin;
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            choice = (gezin)dgGezin.SelectedItem;
            Close();
        }

        private void dgGezin_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            select.IsEnabled = dgGezin.SelectedItem != null;
        }
    }
}
