using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

        private bool userIsDraggingSlider = false;

        MediaPlayer _player = new MediaPlayer();
        DispatcherTimer _timer = new DispatcherTimer();

        string a = null;

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
                a = GetName(filename);
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
                PlayList.Items.Remove(PlayList.SelectedItem);
            }
        }
    }
}
