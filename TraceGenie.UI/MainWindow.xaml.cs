using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using TraceGenie.Client;
using TraceGenie.Client.Models;

namespace TraceGenie.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<TraceGenieEntry> _activeEntries;
        public MainWindow()
        {
            InitializeComponent();
        }

        TraceGenieClient _client;

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_client != null)
            {
                _client.Dispose();
            }
            _client = new TraceGenieClient();
            try
            {
                var result = await _client.Login(UsernameTextBox.Text, PasswordTextBox.Password);
                if (result)
                {
                    StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(0, 192, 0));
                    StatusLabel.Text = "Zalogowano poprawnie.";
                    LoginButton.IsEnabled = false;
                    DeactivateLoginOption();
                    ActivateSearchButtons();
                }
                else
                {
                    StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(192, 0, 0));
                    StatusLabel.Text = "Błąd logowania. Sprawdź nazwę użytkownika i hasło";
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(192, 0, 0));
                StatusLabel.Text = $"Błąd logowania. Szczegóły: {ex.Message}";
            }
        }

        private void DeactivateLoginOption()
        {
            UsernameTextBox.IsEnabled = false;
            PasswordTextBox.IsEnabled = false;
            LoginButton.IsEnabled = false;
        }

        private void ActivateSearchButtons()
        {
            PostcodeTextBox.IsEnabled = true;
            PostcodeLabel.IsEnabled = true;
            SzukajAdresowButton.IsEnabled = true;
        }


        private void ActivateSaveButtons()
        {
            ZapiszCSVButton.IsEnabled = true;
        }

        private void ZapiszCSVButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var entry in _activeEntries)
            {
                builder.AppendLine(entry.ToString());
            }


            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = $"POSTCODE - EXPORT"; // Default file name
            dlg.DefaultExt = ".csv"; // Default file extension
            dlg.Filter = "Csv documents (.csv)|*.csv"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                File.WriteAllLines(filename, _activeEntries.Select(x => x.ToString()).ToArray());
                StatusLabel.Text = $"Adresy zapisane w pliku {filename}";
            }
            else
            {

                StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                StatusLabel.Text = $"Błąd zapisu.";
            }

        }

        private async void SzukajAdresowButton_Click(object sender, RoutedEventArgs e)
        {
            PostcodeTextBox.IsEnabled = false;
            SzukajAdresowButton.IsEnabled = false;
            try
            {
                //live
                var result = await _client.Search(PostcodeTextBox.Text);

                //for testing purpose
                //string fileContent = File.ReadAllText("sampleEntries.html");
                //_client = new TraceGenieClient();
                //List<TraceGenieEntry> result = _client.ConvertToTraceGenieEntries(fileContent);

                _activeEntries = result;
                //just for flickering and scroll reset
                AdresyDataGrid.ItemsSource = new List<TraceGenieEntry>();
                AdresyDataGrid.ItemsSource = result;
                ActivateSaveButtons();
                StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(0, 192, 0));
                StatusLabel.Text = $"Załadowano adresy z {PostcodeTextBox.Text}";

            }
            catch (Exception ex)
            {
                StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(192, 0, 0));
                StatusLabel.Text = ex.Message;
            }
            finally
            {
                PostcodeTextBox.IsEnabled = true;
                SzukajAdresowButton.IsEnabled = true;
            }
        }
    }
}
