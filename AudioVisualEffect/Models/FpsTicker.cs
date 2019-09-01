using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace AudioVisualEffect.Models
{
    /// <summary>
    /// フレーム計測
    /// </summary>
    public class FpsTicker
    {
        public double Interval { get; set; } = 1.0;
        public event EventHandler FpsUpdate;

        Stopwatch sw = new Stopwatch();
        private double ElpasedSec { get { return sw.ElapsedTicks / (double)Stopwatch.Frequency; } }
        private double oldElpasedSec;
        private long FrameCount;
        private double Fps;



        /// <summary>
        /// 開始
        /// </summary>
        public void Start()
        {
            this.FrameCount = 0;
            this.oldElpasedSec = this.ElpasedSec;
            this.sw.Start();
        }


        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            this.sw.Stop();
        }


        /// <summary>
        /// フレーム更新
        /// </summary>
        /// <remarks> 描画毎に呼び出すこと </remarks>
        public void FrameUpdate()
        {
            double elpasedSec = this.ElpasedSec;

            this.FrameCount++;

            if (elpasedSec - this.oldElpasedSec > this.Interval)
            {
                this.Fps = this.FrameCount / (elpasedSec - this.oldElpasedSec);
                this.FrameCount = 0;
                this.oldElpasedSec = elpasedSec;

                this.FpsUpdate?.Invoke(this.Fps, EventArgs.Empty);
            }
        }
    }
}