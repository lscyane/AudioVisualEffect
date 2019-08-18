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
    public partial class Wave_Radial : VisualControlBase
    {
        public Wave_Radial()
        {
            InitializeComponent();
        }


        public override void Render(Complex[] fft, float[] wav)
        {
            int radius = (int)this.Radius.Value;            // 半径
            int level_mag = (int)this.LevelMag.Value;       // レベル倍率
            bool absolute = this.Absolute.IsChecked.Value;
            bool inverse = this.Inverse.IsChecked.Value;

            for (int j = 0; j < wav.Length; j++)
            {
                float dat;
                if (absolute)
                {
                    dat = Math.Abs(wav[j]);
                }
                else
                {
                    dat = wav[j];
                }

                if (inverse)
                {
                    dat *= -1;
                }

                double rad = 2 * Math.PI / wav.Length * j;

                double sin_base = radius * Math.Sin(rad);
                double cos_base = radius * Math.Cos(rad);

                double sin = (radius + dat * level_mag) * Math.Sin(rad);
                double cos = (radius + dat * level_mag) * Math.Cos(rad);

                DxLibDLL.DX.DrawLine(
                    (int)(WindowWidth / 2 + sin_base),
                    (int)(WindowHeight / 2 + cos_base),
                    (int)(WindowWidth / 2 + sin),
                    (int)(WindowHeight / 2 + cos),
                    DxLibDLL.DX.GetColor(255, 255, 255));
            }
        }
    }
}
