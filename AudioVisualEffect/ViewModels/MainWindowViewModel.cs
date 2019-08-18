using NAudio.CoreAudioApi;
using NAudio.Dsp;
using NAudio.Wave;
using Prism.Commands;
using Prism.Events;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AudioVisualEffect.ViewModels
{
    class MainWindowViewModel : BindableBase, GongSolutions.Wpf.DragDrop.IDropTarget
    {
        //==========================================================
        // UI関係
        //==========================================================
        public string AudioFPS
        {
            get { return this._AudioFPS; }
            set { base.SetProperty(ref this._AudioFPS, value); }
        }
        private string _AudioFPS;

        private Models.FpsTicker fpsTicker = new Models.FpsTicker();

        private bool IsPlay
        {
            get { return ((this.wavPlayer != null) && (this.wavPlayer.PlaybackState == PlaybackState.Playing)); }
        }

        private DelegateCommand _MusicPlay;
        public DelegateCommand MusicPlay => this._MusicPlay ?? (this._MusicPlay = new DelegateCommand(MusicPlayAction));
        private void MusicPlayAction()
        {
            if (this.IsPlay == true)
            {
                this.wavPlayer.Stop();
                //this.sw.Stop();

                // FPS計測
                this.fpsTicker.Stop();

                this.StatusMessage = "停止";
            }
            else if (this.wavData != null)
            {
                //this.sw.Reset();
                //this.sw.Start();
                this.wavPlayer.Play();
                this.wavPosition = 0;

                // FPS計測
                this.fpsTicker.Start();

                this.StatusMessage = "再生開始";
            }
        }

        private string _StatusMessage;
        public string StatusMessage
        {
            get { return this._StatusMessage; }
            set { base.SetProperty(ref this._StatusMessage, value); }
        }


        //==========================================================
        // デバイス関係
        //==========================================================
        IWavePlayer wavPlayer;
        byte[] wavData;
        int wavPosition;
        BufferedWaveProvider bufferedWaveProvider;




        //==========================================================
        // IDropTarget インターフェースの実装
        //==========================================================
        public void DragOver(GongSolutions.Wpf.DragDrop.IDropInfo dropInfo)
        {
            var dragFileList = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();
            dropInfo.Effects = dragFileList.Any(dat =>
            {
                var extension = Path.GetExtension(dat);
                return extension != null && extension == ".wav" && IsPlay == false;
            }) ? DragDropEffects.Copy : DragDropEffects.None;
        }


        public void Drop(GongSolutions.Wpf.DragDrop.IDropInfo dropInfo)
        {
            var dragFileList = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();
            dropInfo.Effects = dragFileList.Any(dat =>
            {
                var extension = Path.GetExtension(dat);
                return extension != null && extension == ".wav";
            }) ? DragDropEffects.Copy : DragDropEffects.None;

            if (dragFileList.Count() >= 1)
            {
                LoadWavFile(dragFileList.ToArray()[0]);
            }
        }



        public MainWindowViewModel()
        {
            // TODO サンプルレートは自動検出、ビット深度は16のみ、モノラルチャンネルのみ
            this.bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(44100, 16, 1));

            //ボリューム調整をするために上のBufferedWaveProviderをデコレータっぽく包む
            var wavProvider = new VolumeWaveProvider16(bufferedWaveProvider);
            wavProvider.Volume = 0.1f;

            //再生デバイスと出力先を設定
            var mmDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            //
            wavPlayer = new WasapiOut(mmDevice, AudioClientShareMode.Shared, false, 99);
            //出力に入力を接続して再生開始
            wavPlayer.Init(wavProvider);




            // FPS計測
            this.fpsTicker.FpsUpdate += FpsTicker_FpsUpdate;
        }


        private DelegateCommand _LoadedCommand;
        public DelegateCommand LoadedCommand => this._LoadedCommand ?? (this._LoadedCommand = new DelegateCommand(LoadedAction));
        void LoadedAction()
        {
        }

        void LoadWavFile(string wavFilePath)
        { 
            // エラー処理
            if (File.Exists(wavFilePath) == false)
            {
                Console.WriteLine("Target sound files were not found. Wav file or MP3 file is needed for this program.");
                Console.WriteLine($"expected wav file: {wavFilePath}");
                Console.WriteLine($"expected mp3 file: {wavFilePath}");
                Console.WriteLine("(note: ONE file is enough, two files is not needed)");
                return;
            }
            // データ読み込み
            this.wavData = File.ReadAllBytes(wavFilePath);
            //若干効率が悪いが、ヘッダのバイト数を確実に割り出して取り除く
            using (var r = new WaveFileReader(wavFilePath))
            {
                int headerLength = (int)(this.wavData.Length - r.Length);
                this.wavData = this.wavData.Skip(headerLength).ToArray();
            }

            this.StatusMessage = wavFilePath + " を読み込みました";
        }


        private DelegateCommand _ClosingCommand;
        public DelegateCommand ClosingCommand => this._ClosingCommand ?? (this._ClosingCommand = new DelegateCommand(ClosingAction));
        void ClosingAction()
        {
            // 再生中なら停止させる
            if (this.IsPlay)
            {
                MusicPlayAction();
            }
        }


        //Stopwatch sw = new Stopwatch();
        //long old_tick = 0;

        /// <summary>
        /// Timer.Tickが発生したときのイベントハンドラ
        /// </summary>
        /// <param name="sender">イベント送信元</param>
        /// <param name="e">イベント引数</param>
        public void timer_Tick(object sender, EventArgs e)
        {
            if (IsPlay)
            {
                //long tick = sw.ElapsedTicks;
                //long elapsed_tick = tick - old_tick;
                //long elapsed_ms = elapsed_tick * 1000 / Stopwatch.Frequency;
                //double elapsed_sample = this.bufferedWaveProvider.WaveFormat.SampleRate / 1000.0 * elapsed_ms;

                // 88200 / 60 = 1470 byte (735 samples)
                int bytePerFrame = (int)(this.bufferedWaveProvider.WaveFormat.AverageBytesPerSecond / 60.0);
                int samplePerFrame = bytePerFrame / (this.bufferedWaveProvider.WaveFormat.BlockAlign);

                // 最後までいったら提訴ボタンを強制
                if ((this.wavPosition + bytePerFrame * 4) > this.wavData.Length)
                {
                    this.MusicPlayAction();
                    return;
                }

                // 別タスクでFFTを実行
                Task.Run(() =>
                {
                    int fft_len = samplePerFrame;
                    for (int i = 1; i < 32; i++)
                    {
                        fft_len >>= 1;
                        if (fft_len == 1)
                        {
                            fft_len <<= i;
                            break;
                        }
                    }

                    Complex[] buffer = new Complex[fft_len];
                    float[] raw_dat = new float[fft_len];

                    // サンプリング開始地点
                    int sample_base = this.wavPosition + bytePerFrame - (fft_len * this.bufferedWaveProvider.WaveFormat.BlockAlign);
                    if (sample_base < 0)
                    {
                        sample_base = 0;
                    }
                    else if (sample_base > (this.wavData.Length - (fft_len * this.bufferedWaveProvider.WaveFormat.BlockAlign)))
                    {
                        sample_base = this.wavPosition - (fft_len * this.bufferedWaveProvider.WaveFormat.BlockAlign);
                    }

                    // ハミング窓をかける
                    for (int i = 0; i < fft_len; i++)
                    {
                        raw_dat[i] = BitConverter.ToInt16(this.wavData, sample_base + i * this.bufferedWaveProvider.WaveFormat.BlockAlign) / 32768f;
                        buffer[i].X = (float)(raw_dat[i] * FastFourierTransform.HammingWindow(i, fft_len));
                        buffer[i].Y = 0.0f;
                    }
                    // FFT
                    int m = (int)Math.Log(fft_len, 2.0);
                    FastFourierTransform.FFT(true, m, buffer);
                    // 後ろ半分のデータを取り除く
                    buffer = buffer.Take(buffer.Length / 2).ToArray();

                    return (raw_dat, buffer);

                }).ContinueWith(x =>
                {
                    this.fpsTicker.FrameUpdate();

                    // View への送信
                    Messenger.Instance.GetEvent<PubSubEvent<(float[], Complex[])>>().Publish(x.Result);
                });


                // 再生バッファに追加
                if (this.bufferedWaveProvider.BufferedBytes < (bytePerFrame))
                {
                    this.bufferedWaveProvider.AddSamples(this.wavData, this.wavPosition, bytePerFrame * 4);
                    this.wavPosition += bytePerFrame * 4;
                }
                else if (this.bufferedWaveProvider.BufferedBytes < (bytePerFrame * 2))
                {
                    this.bufferedWaveProvider.AddSamples(this.wavData, this.wavPosition, bytePerFrame * 2);
                    this.wavPosition += bytePerFrame * 2;
                }
            }
        }








        private void FpsTicker_FpsUpdate(object sender, EventArgs e)
        {
            this.AudioFPS = this.fpsTicker.ToString(); ;
        }







    }

    class Messenger : EventAggregator
    {
        public static Messenger Instance { get; } = new Messenger();
    }
}
