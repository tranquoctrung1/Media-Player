using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace _1760190_1760221_1760222_Media_Player_Project
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        List<string> _playList = new List<string>();
        private bool userIsDraggingSlider = false;
        int _indexPlayList = 0;

        MediaPlayer _player = new MediaPlayer();
        DispatcherTimer _timer = new DispatcherTimer();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void BrowserButton_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();
            if(screen.ShowDialog() == true)
            {
                var filename = screen.FileName;
                _player.Open(new Uri(filename));
            }

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += timer_Tick;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((_player.Source != null) && (_player.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                TimeStatus.Content = String.Format("{0} / {1}", _player.Position.ToString(@"mm\:ss"), 
                    _player.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                TimeLine.Minimum = 0;
                TimeLine.Maximum = _player.NaturalDuration.TimeSpan.TotalSeconds;
                TimeLine.Value = _player.Position.TotalSeconds;
            }
            else
                TimeStatus.Content = "No file selected...";
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            _player.Play();
            _timer.Start();
            _player.MediaEnded += _player_MediaEnded;
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            _player.Pause();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _player.Stop();
        }

        string GetName(string name)
        {
            string[] tokens = name.Split(new string[] { "\\" }, StringSplitOptions.None);

            return tokens[tokens.Length-1];
        }

        private void AddPlayList_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();
            screen.Multiselect = true;

            if (screen.ShowDialog() == true)
            {
                var filenames = screen.FileNames;

                foreach(var filename in filenames)
                {
                    PlayList.Items.Add(GetName(filename));
                    _playList.Add(filename);
                }
            }

            PlayList.SelectionMode = SelectionMode.Multiple;
        }

        private void TimeLine_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void TimeLine_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            _player.Position = TimeSpan.FromSeconds(TimeLine.Value);
        }

        private void TimeLine_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TimeStatus.Content = String.Format("{0} / {1}", TimeSpan.FromSeconds(TimeLine.Value).ToString(@"mm\:ss"),
                    _player.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {

            for (int i = PlayList.SelectedItems.Count - 1; i >= 0; i--)
            {
                _playList.RemoveAt(int.Parse(PlayList.SelectedIndex.ToString()));
                PlayList.Items.Remove(PlayList.SelectedItem);
            }
        }

        private void PlayPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (PlayList.SelectedIndex == -1)
            {

                _player.Open(new Uri(_playList[_indexPlayList]));
                _player.Play();
            }
            else
            {

                _indexPlayList = int.Parse(PlayList.SelectedIndex.ToString());
                _player.Open(new Uri(_playList[_indexPlayList]));
                _player.Play();
            }
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += timer_Tick;
            _timer.Start();
            _player.MediaEnded += _player_MediaEnded;
        }

        private void _player_MediaEnded(object sender, EventArgs e)
        {
            if(randomRadioButton.IsChecked == true)
            {
                Random random = new Random();
                _indexPlayList = random.Next(1, _playList.Count) - 1;

                _player.Open(new Uri(_playList[_indexPlayList]));
                _player.Play();
            }
            else
            {
                _indexPlayList++;
                if(_indexPlayList == _playList.Count)
                {
                    if (infinityRadioButton.IsChecked == true)
                    {
                        _indexPlayList = 0;

                        _player.Open(new Uri(_playList[_indexPlayList]));
                        _player.Play();
                    }
                    else
                    {
                        _player.Stop();
                    }
                }
                else
                {
                    _player.Open(new Uri(_playList[_indexPlayList]));
                    _player.Play();
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();
            if (screen.ShowDialog() == true)
            {
                var filename = screen.FileName;
                var doc = new XmlDocument();
                doc.Load(filename);

                foreach(string song in _playList)
                {
                    var newNode = doc.CreateNode("element", "entry", "");
                    ((XmlElement)newNode)
                        .SetAttribute("source", song);

                    doc.DocumentElement.AppendChild(newNode);
                }

                doc.Save(filename);

                using (StreamWriter sw = new StreamWriter("names.txt"))
                {

                    foreach (string s in names)
                    {
                        sw.WriteLine(s);
                    }
                }

                MessageBox.Show("Saved..");

            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            _playList.Clear();
            PlayList.Items.Clear();

            var screen = new OpenFileDialog();
            if (screen.ShowDialog() == true)
            {
                var filename = screen.FileName;
                var doc = new XmlDocument();
                doc.Load(filename);

                var childs = doc.DocumentElement.ChildNodes;

                foreach(XmlNode child in childs)
                {
                    _playList.Add(child.Attributes["source"].Value);
                    PlayList.Items.Add(GetName(child.Attributes["source"].Value));
                }
            }

            MessageBox.Show("Exported...");
        }
    }
}
