using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace kringloopKleding
{
    /// <summary>
    /// window for registering afhalingen
    /// <para>
    /// default window
    /// </para>
    /// </summary>
    public partial class wAfhaling : Window
    {

        //preparations

        private readonly kringloopAfhalingDataContext db = new kringloopAfhalingDataContext();
        private bool saved = true;
        private readonly List<int> checkedGezinsleden = new List<int>();
        /// <summary>
        /// constructor
        /// </summary>
        public wAfhaling()
        {
            InitializeComponent();
            cmbPlaats.ItemsSource = db.woonplaatsens.Select(x => x.woonplaats).Prepend("");
        }

        //updaters

        /// <summary>
        /// updates opmerking textblock and calls <see cref="UpdateTable(int)">UpdateTable(gezin_id)</see>
        /// </summary>
        /// <param name="gezin_id">id of gezin to update elements to</param>
        private void UpdateAll(gezin family)
        {
            btnAfhaling.IsEnabled = true;
            opmerking.Text = family.opmerking == "" ? ""
                             : "opmerking over de familie " + family.achternaam + ": " + Environment.NewLine + family.opmerking;
            UpdateTable(family.id);
        }
        /// <summary>
        /// refreshes datagrid
        /// </summary>
        private void UpdateTable()
        {
            //theres probably a better way
            registreren row = (registreren)dgFamilymember.Items.GetItemAt(0);

            IQueryable<registreren> query = from d in db.registrerens
                                            where d.gezin_id == row.gezin_id
                                            select d;

            dgFamilymember.ItemsSource = query;
        }
        /// <summary>
        /// inserts gezinsleden from passed gezin into datagrid
        /// </summary>
        /// <param name="gezin">gezin whose gezinsleden are displayed</param>
        private void UpdateTable(int gezin_id)
        {
            checkedGezinsleden.Clear();
            IQueryable<registreren> gezinsleden = from gezinslid in db.registrerens
                                                  where gezinslid.gezin_id == gezin_id
                                                  select gezinslid;

            if (!gezinsleden.Any())
            {
                Functions.CustomMsgbox("Geen gezinsleden gevonden.", "Maak gezinsleden aan.");
                return;
            }

            dgFamilymember.ItemsSource = gezinsleden;
        }

        //searching

        /// <summary>
        /// query database for families<br/><br/>
        /// 
        /// calls <see cref="SearchCardnumber()">Searchfamily()</see> (if cardnumber is not empty)
        /// <br/>or<br/>
        /// <see cref="SearchMembers()">SearchMembers()</see>
        /// </summary>
        private void SearchDatabase()
        {
            if (txtCard.Text != "")
            {
                SearchCardnumber();
            }
            else
            {
                SearchMembers();
            }
        }
        /// <summary>
        /// queries database for family based on cardnumber<br/>
        /// Then calls <see cref="PerformUpdates(int)">PerformUpdates(family.id)</see> on the found gezin
        /// </summary>
        private void SearchCardnumber()
        {
            gezin family = db.gezins.Where(x => x.kringloopKaartnummer == txtCard.Text).SingleOrDefault();

            if (family == null)
            {
                CardNotFound();
                return;
            }
            if (!family.actief)
            {
                Functions.CustomMsgbox("Gezin is niet actief", "Maak het gezin actief in het klantenbeheer menu");
                return;
            }
            UpdateAll(family);
        }
        /// <summary>
        /// Queries database for gezinsleden based on inputted info<br/>
        /// Then calls <see cref="GezinChoice(List{int})">Functions.GezinChoice(List{gezin.id})</see> on the found gezinsleden<br/>
        /// lastly calls <see cref="PerformUpdates(int)">PerformUpdates(gezin.id)</see> on the gezin selected in <see cref="GezinChoice(List{int})">GezinChoice</see>
        /// </summary>
        private void SearchMembers()
        {
            HashSet<int> Query = (from gezinslid in db.gezinslids
                                  join gezin in db.gezins on gezinslid.gezin_id equals gezin.id
                                  where txtLastname.Text.IsNullOrEmpty() || gezin.achternaam.Contains(txtLastname.Text)
                                  where txtFirstname.Text.IsNullOrEmpty() || gezinslid.voornaam.Contains(txtFirstname.Text)
                                  where txtDOB.Text.IsNullOrEmpty() || gezinslid.geboortejaar.ToString() == txtDOB.Text
                                  where cmbPlaats.Text.IsNullOrEmpty() || gezin.woonplaats == cmbPlaats.Text
                                  select gezinslid.gezin_id).ToHashSet();

            gezin family;

            switch (Query.Count())
            {
                case 0:
                    Functions.CustomMsgbox("Geen gezinsleden gevonden");
                    return;

                case 1:
                    family = db.gezins.Where(x => x.id == Query.First()).First();
                    break;
                // Count > 1
                default:
                    family = GezinChoice(Query);
                    break;

            }

            if (family == null) return;

            if (!family.actief)
            {
                Functions.CustomMsgbox("Gezin is niet actief", "Maak het gezin actief in het klantenbeheer menu");
                return;
            }

            UpdateAll(family);
        }

        //afhaling

        /// <summary>
        /// adds checked gezinsleden to database<br/><br/>
        /// event handler for clicking the opslaan button
        /// </summary>
        private void btnAfhaling_Click(object sender, RoutedEventArgs e)
        {
            if (!checkedGezinsleden.Any())
            {
                Functions.CustomMsgbox("Geen Gezinsleden geselecteerd");
                return;
            }
            foreach (int id in checkedGezinsleden)
            {
                if (CheckOnCooldown(id))
                {
                    continue;
                }
                AddAfhalingToDatabase(id);
            }
            checkedGezinsleden.Clear();
            saved = true;

            UpdateTable();
        }
        /// <summary>
        /// inserts a pickup for passed familymember
        /// </summary>
        /// <param name="familyMember_id">id which belongs to the gezinslid that the pickup is registered for</param>
        private void AddAfhalingToDatabase(int familyMember_id)
        {
            db.afhalings.InsertOnSubmit(new afhaling()
            {
                datum = DateTime.Now,
                gezinslid_id = familyMember_id,
            });
            db.SubmitChanges();
        }

        //checks

        /// <summary>
        /// Checks if a gezinslid has collected a pickup less than a month ago.<br/><br/>
        /// 
        /// used in <see cref="btnAfhaling_Click(object, RoutedEventArgs)">btnAfhaling_Click()</see> to double check cooldown
        /// </summary>
        /// <param name="id">id of gezinslid to check</param>
        /// <returns>
        /// False if last pickup wasnt any afhalingen this month, true if there was any afhaling was this month
        /// </returns>
        private bool CheckOnCooldown(int id)
        {
            IQueryable<DateTime> PickupQuery = from afhaling in db.afhalings
                                               where afhaling.gezinslid_id == id
                                               where afhaling.datum.Month == DateTime.Now.Month
                                               select afhaling.datum;

            return PickupQuery.Any();
        }
        /// <summary>
        /// checks if the window can safely be exited without losing data.
        /// </summary>
        /// <returns>True if saved or save warning is ignored, else returns false.</returns>
        private bool IsSaved()
        {
            return saved || !Functions.SaveReminder();
        }
        /// <summary>
        /// only allow numbers in txtcard and txtDOB.
        /// </summary>
        private void AllowNumbersOnly(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        //misc

        /// <summary>
        /// switches between windows.
        /// </summary>
        /// <param name="sender">used for determining destination window</param>
        private void SwitchWindow(object sender, RoutedEventArgs e)
        {
            if (!IsSaved())
            {
                return;
            }
            Functions.SwitchWindow(sender, this);
        }
        /// <summary>
        /// creates a messagebox where user can select a gezin from found gezinnen
        /// <para>
        /// gets called from the afhaling window when database contains multiple matching gezinnen
        /// </para>
        /// </summary>
        /// <returns>the gezin that is chosen, returns 0 when no gezin is selected</returns>
        public gezin GezinChoice(HashSet<int> gezinnen)
        {
            GezinChoice gezinChoice = new GezinChoice(gezinnen);
            gezinChoice.ShowDialog();
            return gezinChoice.choice;
        }
        /// <summary>
        /// Notifies the user that the inputted cardnumber does not exist<br/><br/>
        /// redirects to klantenbeheer if yes is clicked<br/>
        /// called in afhaling menu when a cardnumber is used to search<br/>
        /// uses <see cref="Functions.ChoiceCustom(string, string, string)"/>
        /// </summary>
        public void CardNotFound()
        {
            if (Functions.ChoiceCustom("kaartnummer niet gevonden" + Environment.NewLine + "Wil je een nieuw gezin aanmaken?", "Ja", "Nee"))
            {
                wKlant klant = new wKlant();
                klant.txtCard.Text = this.txtCard.Text;
                klant.Show();
                Close();
            }
        }

        //simple event handlers

        /// <summary>
        /// checks for empty textboxes then runs <see cref="SearchDatabase">SearchDatabase()</see><br/><br/>
        /// event handler for clicking the search button
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (txtCard.Text == "" && txtFirstname.Text == "" && txtLastname.Text == "" && txtDOB.Text == "" && cmbPlaats.Text == "")
            {
                Functions.EmptyTextBoxes();
                return;
            }
            SearchDatabase();
        }
        /// <summary>
        /// changes saved status when any text is changed
        /// </summary>
        private void Textbox_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            saved = false;
        }
        /// <summary>
        /// adds checked gezinslid to chechedGezinsleden
        /// </summary>
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            registreren selectedGezinslid = (registreren)dgFamilymember.SelectedItem;
            // if .Remove() fails it returns false
            if (!checkedGezinsleden.Remove(selectedGezinslid.id))
            {
                checkedGezinsleden.Add(selectedGezinslid.id);
            }
        }
        /// <summary>
        /// changes saved status when cmbPlaats is changed
        /// </summary>
        private void Combobox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            saved = false;
        }
        /// <summary>
        /// shortcut for searching
        /// </summary>
        private void Textbox_EnterPressed(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            SearchDatabase();

            e.Handled = true;
        }
        /// <summary>
        /// autocompletes woonplaats on lost focus
        /// </summary>
        private void cmbPlaats_LostFocus(object sender, RoutedEventArgs e)
        {
            if (cmbPlaats.Text == "") return;
            cmbPlaats.Text = db.woonplaatsens.Select(x => x.woonplaats).Where(x => x.StartsWith(cmbPlaats.Text)).FirstOrDefault() ?? "";
        }
        /// <summary>
        /// always opens combobox on click
        /// </summary>
        private void cmbPlaats_PreviewMouseUp(object sender, RoutedEventArgs e)
        {
            cmbPlaats.IsDropDownOpen = true;
        }
    }
}
