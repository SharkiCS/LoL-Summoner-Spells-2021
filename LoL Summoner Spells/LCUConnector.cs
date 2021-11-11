using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Linq;

namespace LoL_Summoner_Spells
{
    class LCUConnector
    {
        private const string protocol = "https";
        private const string LHOST = "127.0.0.1";
        private const string login = "riot";

        private string LPORT;
        private readonly string lockfile;

        public string URL { get; }
        public string PASS { get; }
        public string LOGIN { get => login; }
        public string Region { get; }

        public LCUConnector()
        {
            try
            {
                System.Diagnostics.Process CMD = new System.Diagnostics.Process();
                CMD.StartInfo.FileName = "cmd.exe";
                CMD.StartInfo.Arguments = @"/c " + "wmic PROCESS WHERE name='LeagueClientUx.exe' GET commandline";
                CMD.StartInfo.UseShellExecute = false;
                CMD.StartInfo.CreateNoWindow = true;
                CMD.StartInfo.RedirectStandardOutput = true;
                CMD.Start();

                lockfile = CMD.StandardOutput.ReadToEnd();

                // Match pattern for Lockfile
                LPORT = new Regex(@"--app-port=([0-9]+)").Matches(lockfile)[0].Groups[1].ToString();
                PASS = new Regex(@"--remoting-auth-token=([\w-]+)").Matches(lockfile)[0].Groups[1].ToString();
                Region = new Regex(@"--region=([\w-]+)").Matches(lockfile)[0].Groups[1].ToString();

                Region = Region.ToLower();
                Region = Region.First().ToString().ToUpper() + String.Join("", Region.Skip(1));
                Console.WriteLine(Region);

                URL = protocol + "://" + LHOST + ":" + LPORT;

            }
            catch (Exception)
            {

                MessageBox.Show(
                    messageBoxText: "League of legends isn't running. Open League of legends before starting the program.",
                    caption: "Error",
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error
                );

                Application.Current.Shutdown();
            }
        }
    }
}
