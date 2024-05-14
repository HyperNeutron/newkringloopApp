using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace kringloopKleding
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        readonly kringloopAfhalingDataContext db = new kringloopAfhalingDataContext();
        /// <summary>
        /// subscribes crashhandler, checks for database and opens starting window
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledCrashHandler);
            CheckForDatabase();
            Task.Delay(2000).Wait();
            wAfhaling start = new wAfhaling();
            start.Show();
        }
        /// <summary>
        /// Checks if database exists, creates database if not
        /// </summary>
        private void CheckForDatabase()
        {
            if (db.DatabaseExists())
            {
                return;
            }
            CreateDatabase();
        }

        /// <summary>
        /// creates database from script.sql
        /// </summary>
        private void CreateDatabase()
        {
            try
            {
                Directory.CreateDirectory("C:/ProgramData/KringloopKleding");
                string script = File.ReadAllText(AppContext.BaseDirectory + "script.sql");
                Server server = new Server(new ServerConnection(new SqlConnection("Data Source=(localdb)\\mssqllocaldb;Integrated Security = True")));
                _ = server.ConnectionContext.ExecuteNonQuery(script);
            }
            catch (Exception ex)
            {
                if (ex is ConnectionFailureException)
                {
                    Functions.CustomMsgbox("Geen Database Server Geïnstalleerd");
                    MessageBox.Show(ex.ToString());
                }
            }
        }
        /// <summary>
        /// handles unhandled crashes     
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void UnhandledCrashHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Clipboard.SetText(e.ExceptionObject.ToString());
            MessageBox.Show(e.ExceptionObject.ToString(), "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            Current.Shutdown();
        }
    }
}
