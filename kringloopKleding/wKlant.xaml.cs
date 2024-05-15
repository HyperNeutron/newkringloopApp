using kringloopKleding.messageboxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace kringloopKleding
{

    /// <summary>
    /// klantenbeheer window
    /// </summary>
    public partial class wKlant : Window
    {

        //preparation

        private readonly kringloopAfhalingDataContext db = new kringloopAfhalingDataContext();
        private gezinslid selectedGezinslid;
        private gezin selectedGezin;
        private bool saved = true;

        /// <summary>
        /// constructor
        /// </summary>
        public wKlant()
        {
            InitializeComponent();
            PrepareComboboxes();
        }
        /// <summary>
        /// provides comboboxes with values
        /// </summary>
        private void PrepareComboboxes()
        {
            IQueryable<string> referers = from verwijzer in db.verwijzers
                                          where verwijzer.actief
                                          select verwijzer.verwijzer;

            txtResidence.ItemsSource = db.woonplaatsens.Select(x => x.woonplaats);
            txtReferer.ItemsSource = referers;
        }

        //updaters

        /// <summary>
        /// updates selectedgezin, textboxes, gezinslid datagrid and doorverwijzertext
        /// </summary>
        private void UpdateAll()
        {
            btnChange.IsEnabled = btnDoorverwijzen.IsEnabled = true;
            UpdateDoorverwijzerText();
            FillTextboxes(selectedGezin);
            UpdateTables(selectedGezin);
        }
        /// <summary>
        /// fills textboxes with data from a passed gezin.
        /// </summary>
        /// <param name="gezin">gezin to fill data from.</param>
        private void FillTextboxes(gezin gezin)
        {
            txtCard.Text = gezin.kringloopKaartnummer;
            txtLastname.Text = gezin.achternaam;
            txtResidence.Text = gezin.woonplaats;
            txtReferer.Text = gezin.verwijzer;
            txtComment.Text = gezin.opmerking;
        }
        /// <summary>
        /// Fills textboxes with data from a gezinslid and their gezin<br/>
        /// see <see cref="FillTextboxes(gezin)">FillTextboxes(gezin)</see>
        /// </summary>
        /// <param name="gezinslid">gezinslid whose data is displayed</param>
        private void FillTextboxes(gezinslid gezinslid)
        {
            txtFirstName.Text = gezinslid.voornaam;
            txtbirthYear.Text = gezinslid.geboortejaar.ToString();
            txtGezinslidComment.Text = gezinslid.opmerking;

            FillTextboxes(selectedGezin);
        }
        /// <summary>
        /// updates the member datagrid bases on a gezin
        /// </summary>
        /// <param name="gezin">the gezin whose members should be displayed</param>
        private void UpdateTables(gezin gezin)
        {
            IQueryable<gezinslid> gezinsleden = from gezinslid in db.gezinslids
                                                where gezinslid.gezin_id == gezin.id
                                                select gezinslid;

            dgGezinslid.ItemsSource = gezinsleden;
        }
        /// <summary>
        /// updates both datagrids based on a query
        /// </summary>
        /// <param name="gezinnen">the query with gezinnen which will be displayed</param>
        private void UpdateTables(IQueryable<gezin> gezinnen)
        {
            dgGezin.ItemsSource = gezinnen;
            UpdateTables(gezinnen.First());
            UpdateSelectedGezin(gezinnen.First());
        }
        /// <summary>
        /// updates txtdoorverwezen 
        /// </summary>
        private void UpdateDoorverwijzerText()
        {
            txtDoorverwezen.Inlines.Clear();
            IQueryable<string> verwezen = db.doorverwezens.Where(x => x.gezin_id == selectedGezin.id)
                                                          .Select(x => x.naar);
            //prevents null reference
            if (!verwezen.Any()) return;
            //prevents lazy evaluation
            var verwijzers = verwezen.ToArray();

            if (selectedGezin != null && verwezen.Any())
            {
                txtDoorverwezen.Inlines.Add("De familie ");
                txtDoorverwezen.Inlines.Add(new Run(selectedGezin.achternaam) { FontWeight = FontWeights.Bold });
                txtDoorverwezen.Inlines.Add(" is doorverwezen naar ");
                string opsomming = string.Join(", ", verwijzers, 0, verwijzers.Count() - 1) + (verwijzers.Count() > 1 ? " en " + verwijzers.Last() : "");
                txtDoorverwezen.Inlines.Add(opsomming);
            }
        }
        /// <summary>
        /// sets selected gezin to the selected gezin in the datagrid
        /// </summary>
        private void UpdateSelectedGezin()
        {
            selectedGezin = (gezin)dgGezin.SelectedItem ?? selectedGezin;
        }
        /// <summary>
        /// updates selectedgezin to passed gezin
        /// </summary>
        /// <param name="gezin">gezin to assign selectedgezin</param>
        private void UpdateSelectedGezin(gezin gezin)
        {
            selectedGezin = gezin;
        }
        /// <summary>
        /// kaartnummer => gezin => selectedgezin
        /// </summary>
        /// <param name="kaartnummer"></param>
        private void UpdateSelectedGezin(string kaartnummer)
        {
            selectedGezin = db.gezins.Where(x => x.kringloopKaartnummer == kaartnummer).SingleOrDefault();
        }

        //searching

        /// <summary>
        /// Search database based on cardnumber or lastname and display in tables
        /// </summary>
        private void Search()
        {
            IQueryable<gezin> gezinQuery = from gezin in db.gezins
                                           where gezin.kringloopKaartnummer == txtCard.Text
                                           select gezin;

            IQueryable<gezin> achternaamQuery = from gezin in db.gezins
                                                where gezin.achternaam.Contains(txtLastname.Text)
                                                select gezin;

            if (!gezinQuery.Any() && !achternaamQuery.Any())
            {
                Functions.CustomMsgbox("Geen gezinnen gevonden.");
                return;
            }
            if (!gezinQuery.Any())
            {
                gezinQuery = achternaamQuery;
            }
            dgGezin.ItemsSource = gezinQuery;
            UpdateSelectedGezin(gezinQuery.First());
            UpdateAll();
            //prevents editing gezinsleden that are not visible
            selectedGezinslid = null;
            TextBoxReset();
        }

        //adding

        /// <summary>
        /// event for when the add gezin button is clicked
        /// </summary>
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateGezin()) return;

            AddGezinToDatabase();

            if (ValidateGezinslid()) AddGezinslidToDatabase();

            // dit sux
            dgGezin.ItemsSource = new List<gezin> { selectedGezin };
            UpdateAll();
            TextBoxReset();
            saved = true;
        }
        /// <summary>
        /// event handler for "gezinslid toevoegen" button.
        /// </summary>
        private void BtnGezinslid_Click(object sender, RoutedEventArgs e)
        {
            UpdateSelectedGezin(txtCard.Text);

            if (!ValidateGezinslid()) return;

            AddGezinslidToDatabase();
            UpdateTables(db.gezins.Where(x => x.id == selectedGezin.id));
            TextBoxReset();
            saved = true;
        }
        /// <summary>
        /// adds gezinslid to database
        /// </summary>
        private void AddGezinslidToDatabase()
        {
            db.gezinslids.InsertOnSubmit(new gezinslid()
            {
                voornaam = txtFirstName.Text,
                geboortejaar = int.Parse(txtbirthYear.Text),
                actief = true,
                opmerking = txtGezinslidComment.Text,
                gezin_id = selectedGezin.id,
            });

            db.SubmitChanges();
        }
        /// <summary>
        /// adds new gezin to database
        /// </summary>
        private void AddGezinToDatabase()
        {
            gezin newGezin = new gezin()
            {
                kringloopKaartnummer = txtCard.Text,
                achternaam = txtLastname.Text,
                woonplaats = txtResidence.Text,
                actief = true,
                // prevent foreign key error
                verwijzer = txtReferer.Text,
                opmerking = txtComment.Text
            };

            saved = true;
            db.gezins.InsertOnSubmit(newGezin);
            db.SubmitChanges();
            UpdateSelectedGezin(newGezin);
        }

        //changing

        /// <summary>
        /// event when change button is pressed
        /// </summary>
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateChange()) return;
            ChangeEntry();
        }
        /// <summary>
        /// edit gezin and if possible gezinsleden
        /// </summary>
        private void ChangeEntry()
        {
            if (selectedGezinslid != null && txtFirstName.Text != "" && txtbirthYear.Text != "")
            {
                selectedGezinslid.voornaam = txtFirstName.Text;
                selectedGezinslid.geboortejaar = int.Parse(txtbirthYear.Text);
                selectedGezinslid.opmerking = txtGezinslidComment.Text;
            }

            //changing values
            selectedGezin.achternaam = txtLastname.Text;
            selectedGezin.woonplaats = txtResidence.Text;
            selectedGezin.kringloopKaartnummer = txtCard.Text;
            selectedGezin.verwijzer = txtReferer.Text;
            selectedGezin.opmerking = txtComment.Text;

            db.SubmitChanges();
            UpdateTables(selectedGezin);
            saved = true;
        }
        /// <summary>
        /// Changes active status in database when the checkbox is clicked
        /// </summary>
        private void CheckBoxMember_Clicked(object sender, RoutedEventArgs e)
        {
            selectedGezinslid = (gezinslid)dgGezinslid.SelectedItem ?? selectedGezinslid;

            if (selectedGezinslid == null) return;

            string reden = db.inactiefs.Where(x => x.gezinslid_id == selectedGezinslid.id).Select(x => x.reden).SingleOrDefault();
            //if reden and actief get desynced SHOULDNT HAPPEN
            if (reden == null != selectedGezinslid.actief)
            {
                Functions.CustomMsgbox("actief gezinslid met reden of inactief gezinslid zonder reden", "gezinslid wordt hersteld");
                selectedGezinslid.actief = !selectedGezinslid.actief;
                return;
            }

            CheckBox checkBox = (CheckBox)sender;
            checkBox.BorderBrush = Brushes.Red;
            if (selectedGezinslid.actief)
            {
                string keuze = RedenKiezen();

                if (keuze == null)
                {
                    checkBox.IsChecked = true;
                    return;
                }
                db.inactiefs.InsertOnSubmit(new inactief
                {
                    gezinslid_id = selectedGezinslid.id,
                    datum = DateTime.Now,
                    reden = keuze
                });
            }
            else
            {
                if (!Functions.ChoiceCustom("Het gezinslid is inactief wegens: " + reden + Environment.NewLine + "wil je dit gezinslid actief maken?", "Ja", "Annuleren"))
                {
                    checkBox.IsChecked = false;
                    return;
                }
                db.inactiefs.DeleteAllOnSubmit(db.inactiefs.Where(x => x.gezinslid_id == selectedGezinslid.id));
            }
            selectedGezinslid.actief = !selectedGezinslid.actief;
            db.SubmitChanges();
            saved = true;
        }

        // deleting 

        /// <summary>
        /// deletes selectedgezin <br/><br/>
        /// 
        /// only gezinnen without gezinsleden can be deleted
        /// </summary>
        private void delete_Click(object sender, RoutedEventArgs e)
        {
            if (!Functions.ChoiceCustom("weet je zeker dat je \"" + selectedGezin.achternaam + "\" wilt verwijderen", "Ja", "Annuleren")) return;

            db.gezins.DeleteOnSubmit(selectedGezin);
            db.SubmitChanges();
            dgGezin.ItemsSource = null;
        }
        /// <summary>
        /// deletes selectedgezinslid<br/><br/>
        /// 
        /// only gezinsleden without afhalingen can be deleted
        /// </summary>
        private void deleteGezinslid_Click(object sender, RoutedEventArgs e)
        {
            if (!Functions.ChoiceCustom("weet je zeker dat je \"" + selectedGezinslid.voornaam + "\" wilt verwijderen", "Ja", "Annuleren")) return;

            db.gezinslids.DeleteOnSubmit(selectedGezinslid);
            db.SubmitChanges();
            UpdateTables(selectedGezin);
        }

        //checks

        /// <summary>
        /// check if gezin can be made
        /// </summary>
        /// <returns>true if possible, else false</returns>
        private bool ValidateGezin()
        {
            if (txtCard.Text == "" || txtLastname.Text == "" || txtResidence.Text == "" || txtReferer.Text == "")
            {
                Functions.EmptyTextBoxes();
                return false;
            }
            if (txtCard.Text.Length != 6)
            {
                Functions.CustomMsgbox("Het kaartnummer is te kort.", "Het kaartnummer moet 6 nummers lang zijn.");
                return false;
            }
            if (CheckDuplicate())
            {
                Functions.CustomMsgbox("Het ingevulde kaartnummer is al gebruikt.");
                return false;
            }
            return true;
        }
        /// <summary>
        /// check if gezinslid can be made
        /// </summary>
        /// <returns>true if possible, false if not</returns>
        private bool ValidateGezinslid()
        {
            if (txtFirstName.Text == "" || txtbirthYear.Text == "")
            {
                Functions.CustomMsgbox("Kan geen gezinslid aanmaken", "Het voornaam en geboortjejaar veld moet ingevuld zijn");
                return false;
            }
            if (selectedGezin == null)
            {
                Functions.CustomMsgbox("Geen gezin gevonden", "Controleer het kaartnummer");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Checks if its possible to change data
        /// </summary>
        /// <returns>true if changes are possible, else returns false</returns>
        private bool ValidateChange()
        {
            //return if any textbox is empty
            if (txtCard.Text == "" || txtLastname.Text == "" || txtResidence.Text == "")
            {
                Functions.EmptyTextBoxes();
                return false;
            }
            if (txtCard.Text != selectedGezin.kringloopKaartnummer && CheckDuplicate())
            {
                Functions.CustomMsgbox("Het ingevulde kaartnummer is al gebruikt.");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Checks if changes have been saved
        /// </summary>
        /// <returns>true when saved, otherwise returns false</returns>
        private bool CheckSaved()
        {
            return saved || !Functions.SaveReminder();
        }
        /// <summary>
        /// checks if txtCard is already used
        /// </summary>
        /// <returns>true if duplicate, otherwise returns false</returns>
        private bool CheckDuplicate()
        {
            return db.gezins.Any(x => x.kringloopKaartnummer == txtCard.Text);
        }
        /// <summary>
        /// allow numbers only for use in textbox
        /// </summary>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        //misc

        /// <summary>
        /// messagebox for modifying doorverwijzingen
        /// <para>called from klantenbeheer when doorverwijzen is clicked</para>
        /// </summary>
        /// <param name="gezin_id">id of gezin to modify doorverwijzingen</param>
        public void Doorverwijzen(int gezin_id)
        {
            DoorverwijzerKiezer doorverwijzen = new DoorverwijzerKiezer(gezin_id);
            _ = doorverwijzen.ShowDialog();
            Dictionary<string, bool> verwijzers = doorverwijzen.modifiedDoorverwijzingen;

            foreach (KeyValuePair<string, bool> verwijzer in verwijzers)
            {
                if (verwijzer.Value)
                {
                    db.doorverwezens.InsertOnSubmit(new doorverwezen()
                    {
                        gezin_id = selectedGezin.id,
                        datum = DateTime.Now,
                        naar = verwijzer.Key
                    });
                }
                else
                {
                    doorverwezen removed = db.doorverwezens.Where(x => x.gezin_id == selectedGezin.id && x.naar == verwijzer.Key).Single();
                    db.doorverwezens.DeleteOnSubmit(removed);
                }
                db.SubmitChanges();
            }
        }
        /// <summary>
        /// messagebox for selecting a reason for setting a gezinslid status to inactief 
        /// <para>
        /// called when gezinslid actief status is set to false in klantenbeheer
        /// </para>
        /// </summary>
        /// <returns>string reden that is chosen</returns>
        public string RedenKiezen()
        {
            RedenKiezer redenkiezen = new RedenKiezer();
            _ = redenkiezen.ShowDialog();
            return redenkiezen.choice;
        }
        /// <summary>
        /// reset gezinslid associated textboxes
        /// </summary>
        public void TextBoxReset()
        {
            txtFirstName.Text = txtbirthYear.Text = "";
        }

        //simple event handlers

        /// <summary>
        /// search button event handler
        /// </summary>
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (txtCard.Text == "" && txtLastname.Text == "")
            {
                Functions.EmptyTextBoxes();
                return;
            }
            Search();
        }
        /// <summary>
        /// switches between windows
        /// </summary>
        private void SwitchWindow(object sender, RoutedEventArgs e)
        {
            if (!CheckSaved())
            {
                return;
            }
            //voor ⓘ knoppen
            if (((FrameworkElement)sender).Name == "")
            {
                ((FrameworkElement)sender).Name = "Woonplaats";
            }
            Functions.SwitchWindow(sender, this);
        }
        /// <summary>
        /// calls <see cref="UpdateSelectedGezin()"/><br/>
        /// then updates labels, textboxes en tables
        /// </summary>
        private void DgGezin_Clicked(object sender, MouseButtonEventArgs e)
        {
            UpdateSelectedGezin();
            UpdateAll();
        }
        /// <summary>
        /// Fills in the txtboxes with the data that was clicked on
        /// </summary>
        private void DgGezinslid_Clicked(object sender, MouseButtonEventArgs e)
        {
            if (dgGezinslid.SelectedItem == null) return;
            selectedGezinslid = (gezinslid)dgGezinslid.SelectedItem ?? selectedGezinslid;
            FillTextboxes(selectedGezinslid);
        }
        /// <summary>
        /// Changes active status in database when the checkbox is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            UpdateSelectedGezin();
            CheckBox checkBox = (CheckBox)sender;
            checkBox.BorderBrush = Brushes.Red;
            selectedGezin.actief = !selectedGezin.actief;
            db.SubmitChanges();
            saved = true;
        }
        /// <summary>
        /// Changes saved status to false when any textbox is changed
        /// </summary>
        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            saved = false;
        }
        /// <summary>
        /// changes saved status when verwijzer or woonplaats combobox is changed
        /// </summary>
        private void ComboBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            saved = false;
        }
        /// <summary>
        /// 
        /// </summary>
        private void btnDoorverwijzen_Click(object sender, RoutedEventArgs e)
        {
            Doorverwijzen(selectedGezin.id);
            UpdateDoorverwijzerText();
            saved = true;
        }
        /// <summary>
        /// calls search() (all it do)
        /// </summary>
        private void Textbox_EnterPressed(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            Search();
            e.Handled = true;
        }
        /// <summary>
        /// event handler for opening context menu<br/><br/>
        /// 
        /// stops gezinsleden with afhalingen being deleted
        /// </summary>
        private void DataGridRowGezinslid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            selectedGezinslid = (gezinslid)((DataGridCellsPresenter)e.Source).Item;
            e.Handled = selectedGezinslid == null || db.afhalings.Any(x => x.gezinslid_id == selectedGezinslid.id) || db.inactiefs.Any(x => x.gezinslid_id == selectedGezinslid.id);
        }
        /// <summary>
        /// event handler for opening context menu<br/><br/>
        /// 
        /// stops gezinnen with gezinsleden from being deleted
        /// </summary>
        private void DataGridRowGezin_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            selectedGezin = (gezin)((DataGridCellsPresenter)e.Source).Item;
            e.Handled = selectedGezin == null || db.gezinslids.Any(x => x.gezin_id == selectedGezin.id);
        }
    }
}