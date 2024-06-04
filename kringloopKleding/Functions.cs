using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace kringloopKleding
{
    /// <summary>
    /// class containing functions
    /// <para>
    /// containts mostly messageboxes
    /// </para>
    /// </summary>
    internal static class Functions
    {
        /// <summary>
        /// messagebox about missing required fields
        /// <para>call when required fields are missing</para>
        /// </summary>
        public static void EmptyTextBoxes()
        {
            CustomMsgbox("Een of meerdere vakje(s) zijn leeggelaten.");
        }
        /// <summary>
        /// yes/no choice messagebox
        /// </summary>
        /// <param name="text">message text</param>
        /// <param name="btn1">left button text</param>
        /// <param name="btn2">right button text</param>
        /// <returns>btn1 (left) will return true and btn2 (right) will return false</returns>
        public static bool ChoiceCustom(string text, string btn1, string btn2)
        {
            ChoiceCustom choicec = new ChoiceCustom(text, btn1, btn2);
            return (bool)choicec.ShowDialog();
        }
        /// <summary>
        /// Reminds the user to save (can be ignored)
        /// </summary>
        /// <returns>False when niet opslaan is clicked, otherwise returns true.</returns>
        public static bool SaveReminder()
        {
            return ChoiceCustom("Vergeet niet om op te slaan!", "Oke", "Niet opslaan");
        }
        /// <summary>
        /// Messagebox with 2 parameters to configure text
        /// </summary>
        /// <param name="message">Top text (required)</param>
        /// <param name="message2">Bottom text</param>
        public static void CustomMsgbox(string message, string message2 = "")
        {
            MessageCustom messageC = new MessageCustom(message, message2);
            _ = messageC.ShowDialog();
        }
        /// <summary>
        /// switches windows <br/><br/>
        /// mainly called from the menu select<br/>
        /// sender name is used for determining the destination window
        /// </summary>
        /// <param name="sender">button that sent the event</param>
        /// <param name="main">previous window</param>
        public static void SwitchWindow(object sender, Window main)
        {
            FrameworkElement button = (FrameworkElement)sender;
            string targetWindow = "kringloopKleding.w" + button.Name;
            Type type = Assembly.GetExecutingAssembly().GetType(targetWindow);
            object newWindow = Activator.CreateInstance(type);
            //copy state
            Window window = (Window)newWindow;
            window.Height = main.Height;
            window.Width = main.Width;
            window.Show();
            window.Left = main.Left;
            window.Top = main.Top;
            window.WindowState = main.WindowState;
            main.Close();
        }
    }
    /// <summary>
    /// converts a date to false if more than a month ago<br/><br/>
    /// used for afhaling table to determine if an afhaling can be made
    /// </summary>
    [ValueConversion(typeof(DateTime), typeof(bool))]
    public class DatumCheck : IValueConverter
    {
        /// <summary>
        /// checks if date is more than 1 month ago
        /// </summary>
        /// <param name="value">date</param>
        /// <returns>false if date is more than 1 month ago, else returns true</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }
            return ((DateTime)value).Month == DateTime.Now.Month;
        }
        /// <summary>
        /// not implemented
        /// </summary>
        /// <returns>throws an NotImplementedException</returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
