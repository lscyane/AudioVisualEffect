using NAudio.Dsp;
using Prism.Events;
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

namespace AudioVisualEffect.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewModels.MainWindowViewModel main_vm = new ViewModels.MainWindowViewModel();
        (float[], Complex[]) fft_data;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = main_vm;

            ViewModels.Messenger.Instance.GetEvent<PubSubEvent<(float[], Complex[])>>().Subscribe(HammingWindowReceive);

            // TODO仮 どこかで紐付けしたい
            VisualControlBase.WindowWidth = 960;
            VisualControlBase.WindowHeight = 540;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // DxLibの初期化
            dxImage.Init_DxLib();
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            // DxLibの開放
            dxImage.End_DxLib();
        }


        private void dxImage_Reset(object sender, EventArgs e)
        {
            // デバイスリセット
        }


        private void dxImage_Render(object sender, EventArgs e)
        {
            main_vm.timer_Tick(null, EventArgs.Empty);

            float[] raw_dat = this.fft_data.Item1;
            Complex[] fft_dat = this.fft_data.Item2;
            VisualControlBase vcb = this.VisualControl.Content as VisualControlBase;

            if ((raw_dat != null) && (fft_dat != null) && (vcb != null)) 
            {
                vcb.Render(fft_dat, raw_dat);
            }
        }


        private void HammingWindowReceive((float[], Complex[]) obj)
        {
            this.fft_data = obj;
        }


        private void dxImage_FpsUpdate(object sender, EventArgs e)
        {
            var ea = e as FpsUpdataEventArgs;
            if (ea != null) {
                this.Status_GFPS.Content = ea.Fps;
            }
        }


        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = e.Source as RadioButton;
            if (rb != null)
            {
                VisualControlBase visual;
                switch (rb.Content)
                {
                    case "FFT Raw":         visual = new FFT_Raw(); break;      // FFT 生値
                    case "FFT dB Level":    visual = new FFT_dB(); break;       // FFT レベル対数
                    case "Wave Raw":        visual = new Wave_Raw(); break;     // 波形生値
                    case "Wave Round":      visual = new Wave_Round(); break;   // 円形波形
                    case "Wave Radial":     visual = new Wave_Radial(); break;  // 放射波形
                    default:                visual = null; break;
                }

                VisualControl.Content = visual;
            }
        }
    }
}
