using Microsoft.Win32;
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

        public string ListPolskichImion = "lista_polskich_imion.txt";

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
            StringBuilder builder = new StringBuilder();
            SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            foreach (var entry in _activeEntries)
            {
                builder.AppendLine(entry.ToString());
            }


            
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
                File.WriteAllLines(filename, _activeEntries.Select(x => x.ToString(SeparatorPol.Text)).ToArray());
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
                String[] postcodes = PostcodeTextBox.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                if (PostcodeTextBox.Text.Contains(","))
                {
                    postcodes = PostcodeTextBox.Text.Split(new string[] { "," }, StringSplitOptions.None);
                }
                else
                {
                    postcodes = PostcodeTextBox.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                }

                _activeEntries = new List<TraceGenieEntry>();



                for (int i = 0; i < postcodes.Length; i++)
                {
                    double procent = (100d / postcodes.Length) * i;
                    MainProgressBar.Value = procent;
                    StatusLabel.Text = $"Ładuję adresy z {postcodes[i]}. {(int)procent}% zrobione";
                    this.Title = $"JW PL TraceGenie - {(int)procent}% zrobione";
                    if (Lata.SelectedIndex == 1)
                    {
                        _activeEntries.AddRange(await _client.SearchForAddressesSingleYear(postcodes[i], "2018"));
                    }
                    else if (Lata.SelectedIndex == 2)
                    {
                        _activeEntries.AddRange(await _client.SearchForAddressesSingleYear(postcodes[i], "2017"));
                    }
                    else
                    {
                        _activeEntries.AddRange(await _client.SearchForAddresses(postcodes[i]));
                    }

                }
                this.Title = $"JW PL TraceGenie";
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (_activeEntries != null)
            {
                try
                {
                    var polskieImiona = File.ReadAllLines(ListPolskichImion).Select(x => $"{x.ToLower()} ");
                    var filteredEntries = _activeEntries.Where(x => polskieImiona.Any(x.FullName.ToLower().Contains));
                    AdresyDataGrid.ItemsSource = filteredEntries;
                    _activeEntries = filteredEntries.ToList();
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
    }
}
