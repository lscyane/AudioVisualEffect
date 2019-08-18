using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AudioVisualEffect.Views
{
    public class VisualControlBase : UserControl
    {
        public static double WindowWidth { get; set; }
        public static double WindowHeight { get; set; }



        public VisualControlBase() : base() {}

        public virtual void Render(Complex[] fft, float[] wav){}
    }
}
