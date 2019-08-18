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
    public class FpsUpdataEventArgs : EventArgs
    {
        public string Fps;
    }



    class DxLibImage : D3DImage
    {
        public event EventHandler Reset;
        public event EventHandler Update;
        public event EventHandler Render;
        public event EventHandler FpsUpdate;

        public int BufferWidth { get; set; } = 960;
        public int BufferHeight { get; set; } = 540;


        DX.SetRestoreGraphCallbackCallback RestoreGraphCallback;
        private Models.FpsTicker fpsTicker = new Models.FpsTicker();


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
            DX.SetAlwaysRunFlag(DX.TRUE);                               // 非アクティブ時も処理続行
            DX.SetDrawScreen(DX.DX_SCREEN_BACK);                        // 描画先をバックバッファへ設定
            DX.SetUseFPUPreserveFlag(DX.TRUE);                          // FPUの精度を落とさない
            DX.SetWaitVSyncFlag(DX.FALSE);                              // VSync同期を無効
            DX.SetOutApplicationLogValidFlag(DX.FALSE);                 // ログ出力停止
            DX.SetDoubleStartValidFlag(DX.TRUE);                        // 多重起動を許可
            DX.SetUseIMEFlag(DX.TRUE);                                  // IMEを有効
            DX.SetUseDirect3DVersion(DX.DX_DIRECT3D_9EX);               // Direct3D 11 が使用できる環境でも Direct3D 9 を使用する
            DX.SetBackgroundColor(0, 0, 0);
            if (DX.DxLib_Init() == -1)
            {
                throw new Exception("Dxlib_Init");
            }

            // GC対策(不要?)
            DX.SetUseGraphBaseDataBackup(DX.FALSE);             // 自分ででデバイスロスト時の処理を行う
            RestoreGraphCallback = RestoreGraph;
            DX.SetRestoreGraphCallback(RestoreGraphCallback);   // デバイスリセット時のイベントを設定

            // メインループ
            ComponentDispatcher.ThreadIdle += ComponentDispatcher_ThreadIdle;

            // 描画ループ
            CompositionTarget.Rendering += CompositionTarget_Rendering;
            IsFrontBufferAvailableChanged += d3dImage_IsFrontBufferAvailableChanged;
            SetBackBuffer();

            this.fpsTicker.FpsUpdate += OnFpsUpdate;
            this.fpsTicker.Start();
        }


        /// <summary>
        /// デバイスリセット発生
        /// </summary>
        void RestoreGraph()
        {
            Reset?.Invoke(null, EventArgs.Empty);   // デバイスロスト発生
        }


        /// <summary>
        /// バックバッファの設定
        /// </summary>
        void SetBackBuffer()
        {
            Lock();
            SetBackBuffer(D3DResourceType.IDirect3DSurface9, DX.GetUseDirect3D9BackBufferSurface());
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
            this.Update?.Invoke(null, EventArgs.Empty); //  プログラム更新処理要求イベント

            DX.ClearDrawScreen();
            this.Render?.Invoke(null, EventArgs.Empty); // 描画処理要求イベント
            DX.ScreenFlip();
        }


        /// <summary>
        /// 表示の更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            try
            {
                if (IsFrontBufferAvailable)
                {
                    Lock();
                    SetBackBuffer(D3DResourceType.IDirect3DSurface9, DX.GetUseDirect3D9BackBufferSurface());
                    AddDirtyRect(new Int32Rect(0, 0, PixelWidth, PixelHeight));
                    Unlock();
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
            if (IsFrontBufferAvailable)
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
        /// FPS更新イベント
        /// </summary>
        public virtual void OnFpsUpdate(object sender, EventArgs e)
        {
            var ea = new FpsUpdataEventArgs();
            ea.Fps = this.fpsTicker.Fps.ToString();
            this.FpsUpdate?.Invoke(null, ea);
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

            this.fpsTicker.FpsUpdate -= OnFpsUpdate;
            this.fpsTicker.Stop();

            DX.DxLib_End();
        }
    }
}
