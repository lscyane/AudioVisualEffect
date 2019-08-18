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
    public partial class FFT_Raw : VisualControlBase
    {
        public FFT_Raw()
        {
            InitializeComponent();

        }


        public override void Render(Complex[] fft, float[] wav)
        {
            for (int j = 0; j < fft.Length; j++)
            {
                //複素数の大きさを計算
                double diagonal = Math.Sqrt(fft[j].X * fft[j].X + fft[j].Y * fft[j].Y);

                // diagonalは0～1なのでHeightを掛けて 8倍の補正
                DxLibDLL.DX.DrawLine(
                    (int)(j * WindowWidth / fft.Length),
                    (int)(WindowHeight),
                    (int)(j * WindowWidth / fft.Length),
                    (int)(WindowHeight - diagonal * WindowHeight * 8),
                    DxLibDLL.DX.GetColor(255, 255, 255));

            }
        }
    }
}
