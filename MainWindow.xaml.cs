using Microsoft.Win32;
using System;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CheckK2LowLatency
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ApplySystemTheme();
            _ = RunDiagnosticsAsync();
        }

        private void ApplySystemTheme()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                object registryValue = key?.GetValue("AppsUseLightTheme");

                bool isLightTheme = registryValue is int value && value == 1;

                if (isLightTheme)
                {
                    // Light Mode Colors
                    Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#F9F9F9");
                    Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#1A1A1A");

                    TxtHeader.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#005A9E");

                    CardSysInfo.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFF");
                    CardSysInfo.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#E5E5E5");

                    CardTelemetry.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFF");
                    CardTelemetry.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#E5E5E5");

                    CardNotes.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#F0F0F0");
                    CardNotes.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#E0E0E0");

                    TxtOsCaption.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#1A1A1A");
                    TxtOsBuild.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#555555");
                    TxtNotes.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#666666");
                }
                else
                {
                    SetDarkModeColors();
                }
            }
            catch (Exception)
            {
                SetDarkModeColors();
            }
        }

        private void SetDarkModeColors()
        {
            Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#1E1E1E");
            Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFF");

            TxtHeader.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#60CDFF");

            CardSysInfo.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#2B2B2B");
            CardSysInfo.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#333333");

            CardTelemetry.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#2B2B2B");
            CardTelemetry.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#333333");

            CardNotes.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#252525");
            CardNotes.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#333333");

            TxtOsCaption.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFF");
            TxtOsBuild.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#CCCCCC");
            TxtNotes.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#888888");
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await RunDiagnosticsAsync();
        }

        private async Task RunDiagnosticsAsync()
        {
            BtnRefresh.IsEnabled = false;
            BtnRefresh.Content = "Scanning WMI Telemetry...";
            TxtLastUpdated.Text = "Querying system telemetry...";

            SetStatus(TxtPresenceIcon, TxtPresence, "⏳", "#888888", "Checking feature presence...");
            SetStatus(TxtActiveIcon, TxtActive, "⏳", "#888888", "Checking activation status...");

            await Task.Delay(300);

            await Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    GetOperatingSystemInfo();
                    CheckK2Status();
                });
            });

            BtnRefresh.IsEnabled = true;
            BtnRefresh.Content = "Refresh Diagnostics";
            TxtLastUpdated.Text = $"Last checked: {DateTime.Now:h:mm:ss tt}";
        }

        private void GetOperatingSystemInfo()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(@"root\cimv2", "SELECT Caption, BuildNumber FROM Win32_OperatingSystem");
                foreach (ManagementObject os in searcher.Get())
                {
                    TxtOsCaption.Text = os["Caption"]?.ToString() ?? "Windows OS";
                    TxtOsBuild.Text = $"Build {os["BuildNumber"]?.ToString() ?? "Unknown"}";
                    break;
                }
            }
            catch (Exception ex)
            {
                TxtOsCaption.Text = "Failed to query OS info.";
                TxtOsBuild.Text = ex.Message;
            }
        }

        private void CheckK2Status()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(@"root\wmi", "SELECT * FROM MS_SystemInformation");
                using ManagementObjectCollection results = searcher.Get();

                ManagementObject k2Object = results.OfType<ManagementObject>().FirstOrDefault();

                if (k2Object == null)
                {
                    SetStatus(TxtPresenceIcon, TxtPresence, "✘", "#E03E3E", "K2 Low Latency Profile is NOT included in this Windows build.");
                    SetStatus(TxtActiveIcon, TxtActive, "✘", "#888888", "Cannot check activation because the feature does not exist on this build.");
                    return;
                }

                bool hasProperty = k2Object.Properties.OfType<PropertyData>()
                    .Any(p => p.Name.Equals("LowLatencyProfile", StringComparison.OrdinalIgnoreCase));

                if (hasProperty)
                {
                    SetStatus(TxtPresenceIcon, TxtPresence, "✔", "#107C41", "K2 Low Latency Profile is INCLUDED in this Windows build.");

                    bool isActive = Convert.ToBoolean(k2Object["LowLatencyProfile"]);
                    if (isActive)
                    {
                        SetStatus(TxtActiveIcon, TxtActive, "✔", "#107C41", "Low Latency Profile is ACTIVE and in use.");
                    }
                    else
                    {
                        SetStatus(TxtActiveIcon, TxtActive, "✘", "#D83B01", "Low Latency Profile exists but is NOT active.");
                    }
                }
                else
                {
                    SetStatus(TxtPresenceIcon, TxtPresence, "✘", "#E03E3E", "K2 Low Latency Profile is NOT included in this Windows build.");
                    SetStatus(TxtActiveIcon, TxtActive, "✘", "#888888", "Cannot check activation because the feature does not exist on this build.");
                }
            }
            catch (Exception)
            {
                SetStatus(TxtPresenceIcon, TxtPresence, "✘", "#E03E3E", "K2 Low Latency Profile is NOT included or unavailable.");
                SetStatus(TxtActiveIcon, TxtActive, "✘", "#888888", "WMI class root\\wmi:MS_SystemInformation could not be queried.");
            }
        }

        private void SetStatus(TextBlock icon, TextBlock label, string symbol, string hexColor, string text)
        {
            icon.Text = symbol;
            icon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom(hexColor);
            label.Text = text;
        }
    }
}