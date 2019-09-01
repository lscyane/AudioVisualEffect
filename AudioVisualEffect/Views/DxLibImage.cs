using DxLibDLL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace AudioVisualEffect.Views
{
    class DxLibImage : D3DImage
    {
        public event EventHandler Reset;
        public event EventHandler Update;
        public event EventHandler Render;
        public event EventHandler FpsUpdate;

        public int BufferWidth { get; set; } = 960;
        public int BufferHeight { get; set; } = 540;

            
        DX.SetRestoreGraphCallbackCallback RestoreGraphCallback;        // デバイスリセット時のイベント
        private Models.FpsTicker fpsTicker = new Models.FpsTicker();    // 描画FPSの計測


        /// <summary>
        /// DxLibの初期化
        /// </summary>
        public void Init_DxLib()
        {
            if (DX.DxLib_IsInit() == DX.TRUE)
            {
                return;
            }

            HwndSource hwnd = new HwndSource(0, 0, 0, 0, 0, "DxLib", IntPtr.Zero);

            DX.SetUserWindow(hwnd.Handle);                              // 描画ウィンドウの設定
            DX.SetGraphMode(this.BufferWidth, this.BufferHeight, 32);   // グラフィックモードの設定
            DX.SetAlwaysRunFlag(DX.TRUE);                               // 非アクティブ時の処理設定
            DX.SetDrawScreen(DX.DX_SCREEN_BACK);                        // 描画先設定
            DX.SetUseFPUPreserveFlag(DX.TRUE);                          // FPUの精度
            DX.SetWaitVSyncFlag(DX.FALSE);                              // VSync同期
            DX.SetOutApplicationLogValidFlag(DX.FALSE);                 // ログ出力
            DX.SetDoubleStartValidFlag(DX.TRUE);                        // 多重起動許可の設定
            DX.SetUseIMEFlag(DX.FALSE);                                 // IMEの状態
            DX.SetUseDirect3DVersion(DX.DX_DIRECT3D_9EX);               // Direct3D 11 が使用できる環境でも Direct3D 9 を使用する
            DX.SetBackgroundColor(0, 0, 0);
            if (DX.DxLib_Init() == -1)
            {
                throw new Exception("Dxlib_Init");
            }

            // GC対策(不要?)
            DX.SetUseGraphBaseDataBackup(DX.FALSE);             // 自分ででデバイスロスト時の処理を行う
            this.RestoreGraphCallback = RestoreGraph;
            DX.SetRestoreGraphCallback(RestoreGraphCallback);   // デバイスリセット時のイベントを設定

            // メインループ
            ComponentDispatcher.ThreadIdle += ComponentDispatcher_ThreadIdle;

            // 描画ループ
            CompositionTarget.Rendering += CompositionTarget_Rendering;
            base.IsFrontBufferAvailableChanged += d3dImage_IsFrontBufferAvailableChanged;
            this.SetBackBuffer();

            this.fpsTicker.FpsUpdate += FpsUpdate;
            this.fpsTicker.Start();
        }


        /// <summary>
        /// デバイスリセット発生
        /// </summary>
        void RestoreGraph()
        {
            this.Reset?.Invoke(null, EventArgs.Empty);   // デバイスロスト発生
        }


        /// <summary>
        /// バックバッファの設定
        /// </summary>
        void SetBackBuffer()
        {
            Lock();
            base.SetBackBuffer(D3DResourceType.IDirect3DSurface9, DX.GetUseDirect3D9BackBufferSurface());
            Unlock();
        }


        /// <summary>
        /// メインループ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ComponentDispatcher_ThreadIdle(object sender, EventArgs e)
        {
            this.fpsTicker.FrameUpdate();
            this.Update?.Invoke(null, EventArgs.Empty); // プログラム更新処理要求イベント

            DX.ClearDrawScreen();
            this.Render?.Invoke(null, EventArgs.Empty); // 描画処理要求イベント
            DX.ScreenFlip();
        }


        /// <summary>
        /// 表示の更新(ディスプレイのリフレッシュレート毎の処理？)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            try
            {
                if (base.IsFrontBufferAvailable)
                {
                    base.Lock();
                    base.SetBackBuffer(D3DResourceType.IDirect3DSurface9, DX.GetUseDirect3D9BackBufferSurface());
                    base.AddDirtyRect(new Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));
                    base.Unlock();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }


        /// <summary>
        /// フロントバッファの更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void d3dImage_IsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.fpsTicker.Stop();
            if (base.IsFrontBufferAvailable)
            {
                CompositionTarget.Rendering += CompositionTarget_Rendering;
            }
            else
            {
                CompositionTarget.Rendering -= CompositionTarget_Rendering;
            }
            this.fpsTicker.Start();
        }



        /// <summary>
        /// DxLibの開放
        /// </summary>
        public void End_DxLib()
        {
            if (DX.DxLib_IsInit() != DX.TRUE)
            {
                return;
            }

            this.fpsTicker.FpsUpdate -= FpsUpdate;
            this.fpsTicker.Stop();

            DX.DxLib_End();
        }
    }
}
