using LINQtoCSV;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace kringloopKleding
{
    /// <summary>
    /// rapportage window
    /// </summary>
    public partial class wRapportage : Window
    {
        private bool updating;
        private static readonly kringloopAfhalingDataContext db = new kringloopAfhalingDataContext() { CommandTimeout = 120 };

        /// <summary>
        /// constructor
        /// </summary>
        public wRapportage()
        {
            InitializeComponent();
            PrepareComboboxes();
            SetTotalGezinnen();
            Task.Run(() => UpdateInactief());
        }
        /// <summary>
        /// generates algemeen table
        /// </summary>
        private void GenerateTabelAlgemeen()
        {
            var permonth = from pm in db.perMaands
                           where cmbMaand.SelectedIndex <= 0 || pm.maand == cmbMaand.SelectedIndex
                           where cmbJaar.Text.IsNullOrEmpty() || pm.jaar.ToString() == cmbJaar.Text
                           where cmbPlaats.Text.IsNullOrEmpty() || pm.woonplaats == cmbPlaats.Text
                           orderby pm.jaar, pm.maand
                           select new
                           {
                               pm.jaar,
                               maand = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(pm.maand),
                               pm.woonplaats,
                               aantal__gezinnen = pm.gezinnen,
                               aantal__mensen = pm.aantal
                           };

            tabel.ItemsSource = permonth;
            titel.Text = "Algemeen";
        }
        /// <summary>
        /// generates opleeftijd table
        /// </summary>
        private void GenerateTabelLeeftijd()
        {
            var perAge = from pa in db.opLeeftijds
                         where cmbMaand.SelectedIndex <= 0 || pa.maand == cmbMaand.SelectedIndex
                         where cmbJaar.Text.IsNullOrEmpty() || pa.jaar.ToString() == cmbJaar.Text
                         where cmbPlaats.Text.IsNullOrEmpty() || pa.woonplaats == cmbPlaats.Text
                         orderby pa.jaar, pa.maand, pa.leeftijdsgroep
                         select new
                         {
                             pa.jaar,
                             maand = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(pa.maand),
                             pa.woonplaats,
                             leeftijd = pa.leeftijdsgroep,
                             aantal__mensen = pa.aantal
                         };
            tabel.ItemsSource = perAge;
            titel.Text = "Afhalingen op leeftijd";
        }

        private void GenerateTabelDoorverwijzer()
        {
            var doorverwijsd = from z in db.doorverwezens
                               where cmbMaand.SelectedIndex <= 0 || z.datum.Month == cmbMaand.SelectedIndex
                               where cmbJaar.Text.IsNullOrEmpty() || z.datum.Year.ToString() == cmbJaar.Text
                               orderby z.datum.Year, z.datum.Month
                               group z by new { z.datum.Year, z.datum.Month, z.naar } into g
                               select new
                               {
                                   jaar = g.Key.Year,
                                   maand = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                                   doorverwezen__naar = g.Key.naar,
                                   aantal__mensen = g.Count()
                               };

            tabel.ItemsSource = doorverwijsd;
            titel.Text = "Doorverwijzingen";
        }
        private void GenerateTabelInactief()
        {
            var inactiefq = from i in db.inactiefs
                            where cmbMaand.SelectedIndex <= 0 || i.datum.Month == cmbMaand.SelectedIndex
                            where cmbJaar.Text.IsNullOrEmpty() || i.datum.Year.ToString() == cmbJaar.Text
                            orderby i.datum.Year, i.datum.Month
                            group i by new { i.datum.Year, i.datum.Month, i.reden } into g
                            select new
                            {
                                jaar = g.Key.Year,
                                maand = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                                g.Key.reden,
                                aantal__mensen = g.Count()
                            };

            tabel.ItemsSource = inactiefq;
            titel.Text = "Mensen inactief geworden";
        }

        private void UpdateInactief()
        {
            updating = true;
            Console.WriteLine("update: begin");
            IQueryable<gezinslid> maxgezinslid = from a in db.afhalings
                                                 join gl in db.gezinslids on a.gezinslid_id equals gl.id
                                                 where a.datum.AddYears(1) <= DateTime.Now
                                                 where gl.actief
                                                 select gl;

            Console.WriteLine("update: done query");

            foreach (gezinslid i in maxgezinslid)
            {
                Console.WriteLine("Update: processing " + i.voornaam);

                i.actief = false;
                inactief gezinslid = new inactief()
                {
                    gezinslid_id = i.id,
                    datum = DateTime.Now,
                    reden = "1 jaar inactief"
                };
                db.inactiefs.InsertOnSubmit(gezinslid);
                db.SubmitChanges();
                Console.WriteLine("update: inactief " + i.voornaam);
            }
            Console.WriteLine("update: done");
            updating = false;
        }

        /// <summary>
        /// provides values to comboboxes
        /// </summary>
        private void PrepareComboboxes()
        {
            cmbJaar.ItemsSource = (from g in db.afhalings
                                   select Convert.ToDateTime(g.datum).Year).Distinct().OrderBy(x => x);

            cmbPlaats.ItemsSource = db.woonplaatsens.Select(x => x.woonplaats).Prepend("");

            cmbMaand.ItemsSource = CultureInfo.CurrentCulture.DateTimeFormat.MonthGenitiveNames.Take(12).Prepend("");
        }
        /// <summary>
        /// writes query results to csv file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="query"></param>
        private void SaveQuerytoCSV(dynamic query, string filename)
        {
            SaveFileDialog saveDialog = new SaveFileDialog()
            {
                FileName = filename + DateTime.Now.ToString("dd_MMM_yyyy") + ".csv",
                Filter = "alle bestanden (*.*)| *.*",
                RestoreDirectory = true,
            };

            if (saveDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            string path = saveDialog.FileName;
            CsvFileDescription outputFileDesc = new CsvFileDescription
            {
                SeparatorChar = ',',
                FirstLineHasColumnNames = true,
                FileCultureName = "nl-NL"
            };
            CsvContext cc = new CsvContext();
            cc.Write(query, path, outputFileDesc);
            saveDialog.Dispose();
        }
        /// <summary>
        /// Algemeen table save button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveQuerytoCSV(tabel.ItemsSource, "Rapportage_" + cmbSoort.Text + "_");
        }
        /// <summary>
        /// switches windows
        /// </summary>
        private void SwitchWindow(object sender, RoutedEventArgs e)
        {
            Functions.SwitchWindow(sender, this);
        }
        /// <summary>
        /// algemeen button click event handler
        /// </summary>
        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            _ = GenerateTables();
        }

        private async Task GenerateTables()
        {
            while (updating == true)
            {
                await Task.Delay(10);
                Console.WriteLine("Generate: waiting");
            }
            Console.WriteLine("generate: start table query");
            switch (cmbSoort.Text)
            {
                case "Algemeen":
                    GenerateTabelAlgemeen();
                    break;
                case "Op leeftijd":
                    GenerateTabelLeeftijd();
                    break;
                case "Doorverwijzen":
                    GenerateTabelDoorverwijzer();
                    break;
                case "Inactief":
                    GenerateTabelInactief();
                    break;
                default:
                    Console.WriteLine("generate: spellings fout");
                    return;
            }
            opslaan.IsEnabled = true;
            Console.WriteLine("generate: done");
        }

        /// <summary>
        /// reset filters button click event handler
        /// </summary>
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            cmbJaar.Text = cmbPlaats.Text = cmbMaand.Text = "";
        }
        /// <summary>
        /// sets total gezinnen text to the total gezinnen :o
        /// </summary>
        private void SetTotalGezinnen()
        {
            totaal.Text = db.gezins.Count().ToString();
        }
        private void forgot(object sender, RoutedEventArgs e)
        {
            Functions.CustomMsgbox("het wachtwoord is password");
        }
        private void credit(object sender, RoutedEventArgs e)
        {
            Functions.CustomMsgbox("Kringloopkleding gemaakt door", "github.com/foxydepiraat - eerste versie" + Environment.NewLine + "Nigel Koremans - huidige versie");
        }
        private void git(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/HyperNeutron/newkringloopApp/");
        }
    }
}
