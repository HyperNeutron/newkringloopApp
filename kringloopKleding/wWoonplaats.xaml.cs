using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace kringloopKleding
{
    /// <summary>
    /// woonplaats and verwijzer window
    /// </summary>
    public partial class wWoonplaats : Window
    {
        private readonly kringloopAfhalingDataContext db = new kringloopAfhalingDataContext();
        private woonplaatsen selectedWoonplaats;
        private verwijzers selectedVerwijzer;
        private redenen selectedReden;
        private string selectedGegeven;
        /// <summary>
        /// constructorsaer
        /// </summary>
        public wWoonplaats()
        {
            InitializeComponent();
            InitializeTables();
            InitialiseComboBox();
        }

        /// <summary>
        /// adds values to provincie combobox
        /// </summary>
        private void InitialiseComboBox()
        {
            string[] provinces = { "Noord-Brabant", "Zeeland", "België", "Limburg", "Groningen", "Friesland", "Drenthe", "Overijssel", "Flevoland", "Gelderland", "Utrecht", "Noord-Holland", "Zuid-Holland" };
            txtProvincie.ItemsSource = provinces;
        }
        /// <summary>
        /// Supplies data to tables
        /// </summary>
        private void InitializeTables()
        {
            IQueryable<verwijzers> verwijzers = db.verwijzers.Select(x => x);
            IQueryable<woonplaatsen> woonplaatsens = db.woonplaatsens.Select(x => x);
            IQueryable<redenen> redenens = db.redenens.Select(x => x);

            dgVerwijzers.ItemsSource = verwijzers;
            dgWoonplaats.ItemsSource = woonplaatsens;
            dgReden.ItemsSource = redenens;
        }
        /// <summary>
        /// clear woonplaats textboxes
        /// </summary>
        private void TextboxReset()
        {
            text1.Text = text2.Text = password.Password = txtProvincie.Text = "";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (text1.Text == "" || text2.Text == "" && password.Password == "")
            {
                Functions.EmptyTextBoxes();
                return;
            }
            switch (selectedGegeven)
            {
                case "woonplaatsen":
                    if (db.woonplaatsens.Any(x => x.woonplaats == text1.Text))
                    {
                        Functions.CustomMsgbox("woonplaats bestaat al");
                        return;
                    }
                    SaveWoonplaats();
                    break;
                case "verwijzers":
                    if (db.verwijzers.Any(x => x.verwijzer == text1.Text))
                    {
                        Functions.CustomMsgbox("verwijzer bestaat al");
                        return;
                    }
                    SaveVerwijzer();
                    break;
                case "reden inactief":
                    if (db.redenens.Any(x => x.reden == text1.Text))
                    {
                        Functions.CustomMsgbox("reden bestaat al");
                        return;
                    }
                    SaveReden();
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// adds a new woonplaats to database
        /// </summary>
        private void SaveWoonplaats()
        {
            woonplaatsen newPlaats = new woonplaatsen()
            {
                woonplaats = text1.Text,
                gemeente = text2.Text,
                provincie = txtProvincie.Text,
            };

            db.woonplaatsens.InsertOnSubmit(newPlaats);
            db.SubmitChanges();
            TextboxReset();
            InitializeTables();
        }
        /// <summary>
        /// replaces woonplaats data with data from textboxes
        /// </summary>
        private void EditWoonplaats()
        {
            if (selectedWoonplaats == null || selectedWoonplaats.woonplaats != text1.Text && db.woonplaatsens.Any(x => x.woonplaats == text1.Text))
            {
                return;
            }

            selectedWoonplaats.woonplaats = text1.Text;
            selectedWoonplaats.gemeente = text2.Text;
            selectedWoonplaats.provincie = txtProvincie.Text;

            db.SubmitChanges();
            TextboxReset();
        }
        /// <summary>
        /// fills textbox with selecteditem data
        /// </summary>
        private void dgWoonplaats_Click(object sender, RoutedEventArgs e)
        {
            if ((selectedWoonplaats = (woonplaatsen)dgWoonplaats.SelectedItem) == null)
            {
                return;
            }
            text1.Text = selectedWoonplaats.woonplaats;
            text2.Text = selectedWoonplaats.gemeente;
            txtProvincie.Text = selectedWoonplaats.provincie;
        }
        /// <summary>
        /// searches database woonplaats
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (text1.Text == "")
            {
                Functions.EmptyTextBoxes();
                return;
            }
            switch (selectedGegeven)
            {
                case "woonplaatsen":
                    dgWoonplaats.ItemsSource = db.woonplaatsens.Where(x => x.woonplaats.Contains(text1.Text));
                    break;
                case "verwijzers":
                    dgVerwijzers.ItemsSource = db.verwijzers.Where(x => x.verwijzer.Contains(text1.Text));
                    break;
                case "reden inactief":
                    dgReden.ItemsSource = db.redenens.Where(x => x.reden.Contains(text1.Text));
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// adds a new verwijzer to database
        /// </summary>
        private void SaveVerwijzer()
        {
            if (!PasswordCorrect())
            {
                return;
            }
            verwijzers Verwijzer = new verwijzers
            {
                verwijzer = text1.Text,
                actief = true
            };

            db.verwijzers.InsertOnSubmit(Verwijzer);
            db.SubmitChanges();
            InitializeTables();
            TextboxReset();
        }
        /// <summary>
        /// add reden to database
        /// </summary>
        private void SaveReden()
        {
            if (!PasswordCorrect())
            {
                return;
            }
            redenen reden = new redenen
            {
                reden = text1.Text
            };

            db.redenens.InsertOnSubmit(reden);
            db.SubmitChanges();
            InitializeTables();
            TextboxReset();
        }

        /// <summary>
        /// checks if the password is correct
        /// </summary>
        /// <returns>true correct and false if not</returns>
        private bool PasswordCorrect()
        {
            if (Convert.ToBase64String(Encoding.UTF8.GetBytes(password.Password)) != "cGFzc3dvcmQ=")
            {
                password.Clear();
                Functions.CustomMsgbox("Het ingevulde wachtwoord is niet correct");
                return false;
            }
            return true;
        }
        /// <summary>
        /// replaces verwijzer data with data from textboxes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditVerwijzer()
        {
            if (selectedVerwijzer == null || !PasswordCorrect())
            {
                return;
            }

            selectedVerwijzer.verwijzer = text1.Text;

            db.SubmitChanges();
            TextboxReset();
        }
        /// <summary>
        /// fills textboxes with selecteditem data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VerwijzerDatagrid_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            selectedVerwijzer = (verwijzers)dgVerwijzers.SelectedItem;
            if (selectedVerwijzer == null) return;

            text1.Text = selectedVerwijzer.verwijzer;
        }
        /// <summary>
        /// Changes active status of a verwijzer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            selectedVerwijzer = (verwijzers)dgVerwijzers.SelectedItem;
            if (selectedVerwijzer == null) return;

            selectedVerwijzer.actief = !selectedVerwijzer.actief;
            db.SubmitChanges();
        }
        /// <summary>
        /// Switches windows :o
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwitchWindow(object sender, RoutedEventArgs e)
        {
            Functions.SwitchWindow(sender, this);
        }

        private void comboGegeven_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // cannot use comboGegeven.Text because it will not have the new value
            ComboBoxItem selectedItem = (ComboBoxItem)comboGegeven.SelectedItem;
            selectedGegeven = selectedItem.Content.ToString();
            TextboxReset();
        }
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (text1.Text == "" || (text2.Text == "" && password.Password == ""))
            {
                Functions.EmptyTextBoxes();
                return;
            }
            switch (selectedGegeven)
            {
                case "woonplaatsen":
                    EditWoonplaats();
                    break;
                case "verwijzers":
                    EditVerwijzer();
                    break;
                case "reden inactief":
                    EditReden();
                    break;
                default:
                    break;
            }
        }

        private void dgReden_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if ((selectedReden = (redenen)dgReden.SelectedItem) == null)
            {
                return;
            }

            text1.Text = selectedReden.reden;
        }
        private void EditReden()
        {
            if (selectedReden == null || !PasswordCorrect() || db.redenens.Any(x => x.reden == text1.Text))
            {
                return;
            }
            if (selectedReden.reden == "1 jaar inactief")
            {
                Functions.CustomMsgbox("Deze reden wordt door het systeem gebruikt", "kan reden niet aanpassen");
                return;
            }

            selectedReden.reden = text1.Text;
            db.SubmitChanges();
            TextboxReset();
        }
    }
}
