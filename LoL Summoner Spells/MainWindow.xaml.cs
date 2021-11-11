using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Threading;
using System.Management;
using Newtonsoft.Json.Linq;

using RiotSharp;
using RiotSharp.Misc;
using System.Diagnostics;

namespace LoL_Summoner_Spells
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static LCUEndpoints api = new LCUEndpoints();
        private static RiotApi riotApi;

        private string summonerName;
        private string region;
        private string KEY;

        private bool gameIsRunning = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private ManagementEventWatcher EventWatcher(string process)
        {
            string query = String.Format(@"
                SELECT * 
                FROM __InstanceOperationEvent 
                WITHIN {0} 
                WHERE TargetInstance ISA 'Win32_Process' 
                AND TargetInstance.Name = '{1}'", "1", process
            );

            string scope = @"\\.\root\CIMV2";
            return new ManagementEventWatcher(scope, query);
        }

        // TODO: Change process name.
        private void LCUWatcher()
        {
            ManagementEventWatcher LCUWatcher = EventWatcher("LeagueClient.exe");

            LCUWatcher.EventArrived += new EventArrivedEventHandler(OnEventArrived_Lcu);
            LCUWatcher.Start();
        }


        private void GameWatcher()
        {
            ManagementEventWatcher GAMEWatcher = EventWatcher("League of Legends.exe");

            GAMEWatcher.EventArrived += new EventArrivedEventHandler(OnEventArrived_Game);
            GAMEWatcher.Start();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LCUWatcher();
        }

        private bool CheckAPIKEY(string key)
        {
            try
            {
                riotApi = RiotApi.GetDevelopmentInstance(key);
                riotApi.Status.GetShardStatusAsync(
                    region: (Region)Enum.Parse(typeof(Region), region)
                )
                .GetAwaiter()
                .GetResult();

                return true;
            }
            catch (RiotSharpException) { return false; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (gameIsRunning)
                MessageBox.Show(
                    messageBoxText: "Program is running",
                    caption: "Already active :)",
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Information
                );

            else if (summonerName != null)
            {
                KEY = Key.Text;

                if (!CheckAPIKEY(KEY))
                    MessageBox.Show(
                        messageBoxText: "Your Riot api key isn't valid",
                        caption: "Invalid key.",
                        button: MessageBoxButton.OK,
                        icon: MessageBoxImage.Error
                    );
                else
                {
                    GameWatcher();
                    Status.Text = "Running";
                    Status.Foreground = Brushes.GreenYellow;
                    gameIsRunning = true;
                }
            }
            else
                MessageBox.Show(
                    messageBoxText: "Couldn't recive information about your Summoner Name, please try restarting the program.",
                    caption: "Couldn't recive information",
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error
                );
        }

        private void OnEventArrived_Game(object sender, EventArrivedEventArgs e)
        {
            // If process is open. 
            if (e.NewEvent.ClassPath.ClassName.Contains("InstanceCreationEvent") && !(summonerName is null))
            {

                Action showWindow = () =>
                {
                    new InGame(summonerName, region, KEY, SliderIconSize.Value, SliderSpacingSize.Value).ShowDialog();
                };

                Dispatcher.BeginInvoke(showWindow);
            }


            // If process is closed
            if (e.NewEvent.ClassPath.ClassName.Contains("InstanceDeletionEvent"))
            {
                Action closeWindow = () =>
                {
                    Window window = Application.Current.Windows.Cast<Window>()
                    .Where(w => w.Title == "InGame")
                    .SingleOrDefault();

                    if (window != null)
                        window.Close();
                };

                Dispatcher.BeginInvoke(closeWindow);
            }
        }


        // Detect when the LCU process is running/closed/modificated.
        private void OnEventArrived_Lcu(object sender, EventArrivedEventArgs e)
        {
            String className = e.NewEvent.ClassPath.ClassName;
            switch (className)
            {
                // If process is closed. 
                case "__InstanceDeletionEvent":
                    defaultValue();
                    break;

                // If process is open
                case "__InstanceCreationEvent":
                    if (summonerName == null)
                        getSummonerName();
                    break;

                // If process is modify
                case "__InstanceModificationEvent":
                    if (summonerName == null)
                        getSummonerName();
                    break;
            }
        }

        private void defaultValue()
        {
            summonerName = null;
            Dispatcher.Invoke(() =>
            {
                SummonerName.Text = "Unknown";
                SummonerName.Foreground = Brushes.Red;

                Status.Text = "Stopped";
                Status.Foreground = Brushes.Red;

                gameIsRunning = false;
            });
        }


        private async void getSummonerName()
        {
            if (summonerName != null)
                Thread.CurrentThread.Abort();

            try
            {
                summonerName = JObject.Parse(await api.getSession())["displayName"].ToString();

                Dispatcher.Invoke(() =>
                {
                    SummonerName.Text = summonerName;
                    SummonerName.Foreground = Brushes.GreenYellow;

                    region = api.Region;
                });
            }
            catch (Exception) { }
        }

        private void TextBlock_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://developer.riotgames.com/",
            });
        }

        private void Image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://twitter.com/Uitwaaien___",
            });
        }
    }
}
