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
    public partial class Wave_Round : VisualControlBase
    {
        public Wave_Round()
        {
            InitializeComponent();
        }


        public override void Render(Complex[] fft, float[] wav)
        {
            int radius = (int)this.Radius.Value;            // 半径
            int level_mag = (int)this.LevelMag.Value;       // レベル倍率

            for (int j = 0; j < wav.Length - 1; j++)
            {
                double rad = 2 * Math.PI / wav.Length * j;
                double sin = (radius + wav[j] * level_mag) * Math.Sin(rad);
                double cos = (radius + wav[j] * level_mag) * Math.Cos(rad);

                rad = 2 * Math.PI / wav.Length * (j + 1);
                double sin_next = (radius + wav[j + 1] * level_mag) * Math.Sin(rad);
                double cos_next = (radius + wav[j + 1] * level_mag) * Math.Cos(rad);

                DxLibDLL.DX.DrawLine(
                    (int)(WindowWidth / 2 + sin),
                    (int)(WindowHeight / 2 + cos),
                    (int)(WindowWidth / 2 + sin_next),
                    (int)(WindowHeight / 2 + cos_next),
                    DxLibDLL.DX.GetColor(255, 255, 255));
            }

            // 始点と終点を繋ぐ
            {
                double rad = 0;
                double sin = (radius + wav[0] * level_mag) * Math.Sin(rad);
                double cos = (radius + wav[0] * level_mag) * Math.Cos(rad);

                rad = 2 * Math.PI / wav.Length * (wav.Length - 1);
                double sin_next = (radius + wav[wav.Length - 1] * level_mag) * Math.Sin(rad);
                double cos_next = (radius + wav[wav.Length - 1] * level_mag) * Math.Cos(rad);

                DxLibDLL.DX.DrawLine(
                    (int)(WindowWidth / 2 + sin),
                    (int)(WindowHeight / 2 + cos),
                    (int)(WindowWidth / 2 + sin_next),
                    (int)(WindowHeight / 2 + cos_next),
                    DxLibDLL.DX.GetColor(255, 255, 255));
            }

        }
    }
}
