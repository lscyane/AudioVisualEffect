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
    public partial class Wave_Raw : VisualControlBase
    {
        public Wave_Raw()
        {
            InitializeComponent();
        }


        public override void Render(Complex[] fft, float[] wav)
        {
            int level_mag = (int)this.LevelMag.Value;       // レベル倍率

            for (int j = 0; j < wav.Length - 1; j++)
            {
                DxLibDLL.DX.DrawLine(
                (int)(j * WindowWidth / wav.Length),
                (int)((WindowHeight / 2) - wav[j] * level_mag),
                (int)((j + 1) * WindowWidth / wav.Length),
                (int)((WindowHeight / 2) - wav[j + 1] * level_mag),
                DxLibDLL.DX.GetColor(255, 255, 255));
            }

        }
    }
}
