using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;


namespace kringloopKleding.messageboxes
{
    /// <summary>
    /// Messagebox for modifying oorverwijzingen
    /// </summary>
    public partial class DoorverwijzerKiezer : Window
    {
        readonly kringloopAfhalingDataContext db = new kringloopAfhalingDataContext();
        private readonly Dictionary<string, bool> _modifiedDoorverwijzingen = new Dictionary<string, bool>();
        public Dictionary<string, bool> modifiedDoorverwijzingen = new Dictionary<string, bool>();
        /// <summary>
        /// contructor for doorverwijzerkiezer
        /// </summary>
        /// <param name="gezin_id">id of gezin to modify doorverwijzingen</param>
        public DoorverwijzerKiezer(int gezin_id)
        {
            InitializeComponent();

            IQueryable<string> doorverwezen = from d in db.doorverwezens
                                              where gezin_id == d.gezin_id
                                              select d.naar;

            var verwijzers = from v in db.verwijzers
                             select new
                             {
                                 v.verwijzer,
                                 actief = doorverwezen.Contains(v.verwijzer)
                             };

            dgVerwijzers.ItemsSource = verwijzers;
        }
        /// <summary>
        /// closes window, event handler for select button
        /// </summary>
        private void Select_Click(object sender, RoutedEventArgs e)
        {
            modifiedDoorverwijzingen = _modifiedDoorverwijzingen;
            Close();
        }
        /// <summary>
        /// adds/removes the verwijzer to the modified dictionary
        /// <para>event handler for table checkboxes</para>
        /// </summary>
        private void CheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            string clickedverwijzer = ((dynamic)dgVerwijzers.SelectedItem).verwijzer;
            //Remove() returns false if it fails to find the key
            if (!_modifiedDoorverwijzingen.Remove(clickedverwijzer))
            {
                _modifiedDoorverwijzingen.Add(clickedverwijzer, ((System.Windows.Controls.CheckBox)sender).IsChecked ?? false);
            }
            Console.WriteLine(string.Join(", ", _modifiedDoorverwijzingen));
            select.IsEnabled = true;
        }
    }
}
