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
using NAudio.Dsp;

namespace AudioVisualEffect.Views
{
    /// <summary>
    /// RawWave.xaml の相互作用ロジック
    /// </summary>
    public partial class FFT_dB : VisualControlBase
    {
        public FFT_dB()
        {
            InitializeComponent();

        }


        public override void Render(Complex[] fft, float[] wav)
        {
            for (int j = 0; j < fft.Length; j++)
            {
                //複素数の大きさを計算
                double diagonal = Math.Sqrt(fft[j].X * fft[j].X + fft[j].Y * fft[j].Y);

                // FFT レベル対数
                // -128dBを底に 2倍の補正
                double homu = 20 * Math.Log(diagonal);

                DxLibDLL.DX.DrawLine(
                    (int)(j * WindowWidth / fft.Length),
                    (int)(WindowHeight),
                    (int)(j * WindowWidth / fft.Length),
                    (int)(WindowHeight - ((homu + 128) * 2)),
                    DxLibDLL.DX.GetColor(255, 255, 255));
            }
        }
    }
}
