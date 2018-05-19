using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media;
using TraceGenie.Client;
using TraceGenie.Client.Models;
using Microsoft.Win32;

namespace TraceGenie.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<TraceGenieEntry> _activeEntries;
        IEnumerable<TraceGenieEntry> _filteredEntries;

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
            MainTabControl.SelectedIndex = 1;
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
            SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = $"POSTCODE - EXPORT"; // Default file name
            dlg.DefaultExt = ".csv"; // Default file extension
            dlg.Filter = "Csv documents (.csv)|*.csv"; // Filter files by extension

            // Show save file dialog box
            bool? result = dlg.ShowDialog();

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
            MainProgressBar.Visibility = Visibility.Visible;
            try
            {
                var postcodes = PostcodeTextBox.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                _activeEntries = new List<TraceGenieEntry>();

                for (int i = 0; i < postcodes.Length; i++)
                {
                    double procent = (100d / postcodes.Length) * i;
                    MainProgressBar.Value = procent;
                    StatusLabel.Text = $"Ładuję adresy z {postcodes[i]}. {(int)procent}% zrobione";
                    _activeEntries.AddRange(await _client.SearchForAddresses(postcodes[i]));

                }

                //_activeEntries = Helper.GetFakeActiveEntries();
                AdresyDataGrid.ItemsSource = _activeEntries;
                ActivateSaveButtons();
                StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(0, 192, 0));
                StatusLabel.Text = $"Załadowano wszystkie adresy";

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
                MainProgressBar.Visibility = Visibility.Hidden;
            }
        }

        private void FilterLoose(object sender, RoutedEventArgs e)
        {
            if (_activeEntries != null)
            {
                try
                {
                    var polskieImiona = File.ReadAllLines(ListPolskichImion).Select(x => x.ToLower());
                    _filteredEntries = _activeEntries.Where(x => polskieImiona.Any(x.FullName.ToLower().Contains));
                    AdresyDataGrid.ItemsSource = _filteredEntries;
                    StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(0, 192, 0));
                    StatusLabel.Text = $"Przefiltrowano listę wg. polskich imion";
                }
                catch (Exception ex)
                {
                    StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(192, 0, 0));
                    StatusLabel.Text = ex.Message;
                }
            }
            else
            {
                StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(192, 0, 0));
                StatusLabel.Text = "Najpierw załaduj adresy";
            }
        }
        private void FilterExact(object sender, RoutedEventArgs e)
        {
            if (_activeEntries != null)
            {
                try
                {
                    var polskieImiona = File.ReadAllLines(ListPolskichImion).Select(x => x.ToLower());
                     _filteredEntries = _activeEntries.Where(x => polskieImiona.Any(x.FullName.ToLower().StartsWith));

                    AdresyDataGrid.ItemsSource = _filteredEntries;

                    StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(0, 192, 0));
                    StatusLabel.Text = $"Przefiltrowano listę wg. polskich imion";
                }
                catch (Exception ex)
                {
                    StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(192, 0, 0));
                    StatusLabel.Text = ex.Message;
                }
            }
            else
            {
                StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(192, 0, 0));
                StatusLabel.Text = "Najpierw załaduj adresy";
            }
        }

        private void SaveFilteredCsv(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = $"POSTCODE - EXPORT"; // Default file name
            dlg.DefaultExt = ".csv"; // Default file extension
            dlg.Filter = "Csv documents (.csv)|*.csv"; // Filter files by extension

            // Show save file dialog box
            bool? result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                File.WriteAllLines(filename, _filteredEntries.Select(x => x.ToString()).ToArray());
                StatusLabel.Text = $"Adresy zapisane w pliku {filename}";
            }
            else
            {
                StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                StatusLabel.Text = $"Błąd zapisu.";
            }

        }

        public string ListPolskichImion
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.Combine(Path.GetDirectoryName(path), "lista_polskich_imion.txt");
            }
        }
    }
}
