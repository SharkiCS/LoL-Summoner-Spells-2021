using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using RiotSharp.Endpoints.SpectatorEndpoint;

using static LoopUtilities;

namespace LoL_Summoner_Spells
{
    public partial class InGame : Window
    {
        private readonly string summonerName;
        private readonly string KEY;
        private readonly string region;

        private List<int> spellCooldown = new List<int>();
        private List<Label> labelList = new List<Label>();

        private readonly int sizeChampImage;
        private readonly int sizeSpellImage;
        private readonly int space;

        public InGame(string _summonerName, string _region, string _KEY, double _iconSize, double _spacingSize)
        {
            InitializeComponent();
            summonerName = _summonerName;
            KEY = _KEY;
            region = _region;

            sizeChampImage = (int)_iconSize;
            sizeSpellImage = (int)_iconSize / 2;
            space = (int)_spacingSize;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Width = (sizeChampImage * 5) + (space * 5) + 20;
            this.Height = (sizeChampImage + sizeSpellImage) + sizeSpellImage + 200;
            this.Top = 0;
            this.Left = (SystemParameters.WorkArea.Width - this.Width) / 2;
            CreateControls(summonerName);
        }

        private void CreateImageChampions(List<string> championName)
        {
            List<Image> championImage = new List<Image>();
            Repeat(5, () => championImage.Add(new Image()));

            for (int i = 0, px = 0; i < championImage.Count; px += sizeChampImage + space, i++)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(championName[i], UriKind.Absolute);
                bitmap.EndInit();

                championImage[i].Source = bitmap;
                championImage[i].Width = sizeChampImage;
                championImage[i].Height = sizeChampImage;
                championImage[i].Stretch = Stretch.Fill;
                championImage[i].HorizontalAlignment = HorizontalAlignment.Left;
                championImage[i].VerticalAlignment = VerticalAlignment.Top;

                championImage[i].Margin = new Thickness(10 + px, 10, 0, 0);
                championsGrid.Children.Add(championImage[i]);
            }
        }


        private void CreateImageSpells(List<string> spellName)
        {
            List<Image> spellImage = new List<Image>();
            Repeat(10, () => spellImage.Add(new Image()));

            for (int i = 0, px = 0; i < spellImage.Count; px += sizeSpellImage, i++)
            {
                if (i % 2 == 0 && i != 0) { px += space; }

                spellImage[i].Tag = "Picture" + i.ToString();

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(spellName[i], UriKind.Absolute);
                bitmap.EndInit();

                spellImage[i].Source = bitmap;
                spellImage[i].Width = sizeSpellImage;
                spellImage[i].Height = sizeSpellImage;
                spellImage[i].Stretch = Stretch.Fill;
                spellImage[i].HorizontalAlignment = HorizontalAlignment.Left;
                spellImage[i].VerticalAlignment = VerticalAlignment.Top;

                spellImage[i].MouseLeftButtonDown += ClickHandler;

                spellImage[i].Margin = new Thickness(10 + px, 10 + sizeChampImage, 0, 0);
                championsGrid.Children.Add(spellImage[i]);
            }
        }

        private void CreateLabels()
        {
            Repeat(10, () => labelList.Add(new Label()));

            for (int i = 0, px = 0; i < labelList.Count; px += sizeSpellImage, i++)
            {
                if (i % 2 == 0 && i != 0) { px += space; }

                labelList[i].Visibility = Visibility.Hidden;
                labelList[i].Content = "360";

                labelList[i].HorizontalContentAlignment = HorizontalAlignment.Center;
                labelList[i].HorizontalAlignment = HorizontalAlignment.Left;
                labelList[i].VerticalAlignment = VerticalAlignment.Top;

                labelList[i].Foreground = Brushes.White;
                labelList[i].FontSize = 12;
                labelList[i].FontWeight = FontWeights.Bold;
                labelList[i].Width = sizeSpellImage;
                labelList[i].Height = sizeSpellImage;

                var parent = VisualTreeHelper.GetParent(labelList[i]) as Grid;
                if (parent != null)
                    parent.Children.Remove(labelList[i]);

                championsGrid.Children.Add(labelList[i]);

                labelList[i].Margin = new Thickness(10 + px, 10 + sizeChampImage + sizeSpellImage, 0, 0);
            }
        }


        private void CreateControls(string summonerName)
        {
            GameApi api = new GameApi(summonerName, region, KEY);
            IEnumerable<CurrentGameParticipant> enemyTeam = api.GetEnemyTeam();

            List<string> championUri = api.GetChampionsUri(enemyTeam);
            List<string> spellListUri = api.GetSpellUriList(enemyTeam);

            spellCooldown = api.GetSpellCD(enemyTeam);

            CreateImageChampions(championUri);
            CreateImageSpells(spellListUri);
            CreateLabels();
        }

        private Task ImageClicked(int i) => new Task(() => SpellTimer(spellCooldown[i], labelList[i]));

        private void ClickHandler(object sender, EventArgs e)
        {
            Image pic = (Image)sender;

            for (int i = 0; i < 10; i++)
                if ((string)pic.Tag == $"Picture{i}" && !labelList[i].IsVisible)
                    ImageClicked(i).Start();
        }

        static void SpellTimer(int time, Label label)
        {
            Action updateLabel = () =>
            {
                label.Content = time.ToString();
                label.Visibility = Visibility.Visible;
            };

            Application.Current.Dispatcher.BeginInvoke(updateLabel);

            while (time != 0)
            {
                Thread.Sleep(1000);
                time -= 1;

                Action updateLabel2 = () =>
                {
                    label.Content = time.ToString();
                };

                Application.Current.Dispatcher.BeginInvoke(updateLabel2);
            }

            Action hideLabel = () =>
            {
                label.Visibility = Visibility.Hidden;
            };

            Application.Current.Dispatcher.BeginInvoke(hideLabel);
        }
    }
}
