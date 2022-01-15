using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

using GameOverlay.Drawing;
using GameOverlay.Windows;

using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace MU3Input
{
    public class Overlay : IDisposable
    {
        private readonly GraphicsWindow _window;

        private readonly Dictionary<string, IBrush> _brushes;

        public bool Visible { get => _window.IsVisible; set => _window.IsVisible = value; }

        public Overlay(int x,int y,int width,int height)
        {
            _brushes = new Dictionary<string, IBrush>();

            var gfx = new Graphics()
            {
                PerPrimitiveAntiAliasing = false,
                TextAntiAliasing = false
            };

            _window = new GraphicsWindow(gfx)
            {
                FPS = 60,
                IsTopmost = true,
                IsVisible = true,
            };
            SetSize(x, y, width, height);

            _window.DestroyGraphics += _window_DestroyGraphics;
            _window.DrawGraphics += _window_DrawGraphics;
            _window.SetupGraphics += _window_SetupGraphics;
        }

        private void _window_SetupGraphics(object sender, SetupGraphicsEventArgs e)
        {
            var gfx = e.Graphics;
            if (e.RecreateResources)
            {
                foreach (var pair in _brushes)
                {
                    if (pair.Value is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
            _brushes["background"] = gfx.CreateSolidBrush(0, 0, 0, 0);
            _brushes["red"] = gfx.CreateSolidBrush(0xFF, 0x45, 0x5B);
            _brushes["green"] = gfx.CreateSolidBrush(0x45, 0xFF, 0x75);
            _brushes["blue"] = gfx.CreateSolidBrush(0x45, 0x89, 0xFF);
            _brushes["yellow"] = gfx.CreateSolidBrush(0xFF, 0xD5, 0x45);
            _brushes["cyan"] = gfx.CreateSolidBrush(0x45, 0xF8, 0xFF);
            _brushes["purple"] = gfx.CreateSolidBrush(0x8B, 0x45, 0xFF);
            _brushes["blank"] = gfx.CreateSolidBrush(0xDD, 0xDD, 0xDD);
            _brushes["white"] = gfx.CreateSolidBrush(0xFF, 0xFF, 0xFF);

            if (e.RecreateResources) return;

            return;
        }

        private void _window_DestroyGraphics(object sender, DestroyGraphicsEventArgs e)
        {
            foreach (var pair in _brushes)
            {
                if (pair.Value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
        public static extern IntPtr GetForegroundWindow();
        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowRect")]
        public static extern int GetWindowRect(IntPtr hwnd, ref System.Drawing.Rectangle lpRect);
        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowText")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int maxCount);
        StringBuilder sb = new StringBuilder();
        private void _window_DrawGraphics(object sender, DrawGraphicsEventArgs e)
        {
            if (!Visible) return;
            IntPtr handle = GetForegroundWindow();
            GetWindowText(handle, sb, 16);
            if (sb.ToString() == "Otoge")
            {
                _window.PlaceAbove(handle);
            }
            var gfx = e.Graphics;
            GenRects(gfx.Width, gfx.Height);
            gfx.ClearScene((SolidBrush)_brushes["background"]);
            gfx.FillRectangle(_brushes["red"], buttons[0]);
            gfx.FillRectangle(_brushes["green"], buttons[1]);
            gfx.FillRectangle(_brushes["blue"], buttons[2]);
            gfx.FillRectangle(_brushes["red"], buttons[3]);
            gfx.FillRectangle(_brushes["green"], buttons[4]);
            gfx.FillRectangle(_brushes["blue"], buttons[5]);
        }
        const float PanelMarginCoef = 0.5f;
        const float LRSpacingCoef = 0.5f;
        const float ButtonSpacingCoef = 0.25f;
        Rectangle[] buttons = new Rectangle[6];
        /// <summary>
        /// 计算各个元素的位置和大小
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void GenRects(int width, int height)
        {
            float buttonWidth = (width / (PanelMarginCoef * 2 + LRSpacingCoef * 1 + ButtonSpacingCoef * 4 + 6));
            float panelMargin = buttonWidth * PanelMarginCoef;
            float lrSpacing = buttonWidth * LRSpacingCoef;
            float buttonSpacing = buttonWidth * ButtonSpacingCoef;
            // Left 1
            buttons[0] = new Rectangle(panelMargin - buttonSpacing * 0.5f, 0, panelMargin + buttonWidth + buttonSpacing * 0.5f, height);
            // Left 2
            buttons[1] = new Rectangle(buttons[0].Right, 0, buttons[0].Right + buttonWidth + buttonSpacing, height);
            // Left 3
            buttons[2] = new Rectangle(buttons[1].Right, 0, buttons[1].Right + buttonWidth + buttonSpacing, height);

            //-------------------
            // Right 1
            buttons[3] = new Rectangle(buttons[2].Right + lrSpacing - buttonSpacing, 0, buttons[2].Right + lrSpacing +buttonWidth, height);
            // Right 2
            buttons[4] = new Rectangle(buttons[3].Right, 0, buttons[3].Right + buttonWidth + buttonSpacing, height);
            // Right 3
            buttons[5] = new Rectangle(buttons[4].Right, 0, buttons[4].Right + buttonWidth + buttonSpacing, height);

        }

        public void Run()
        {
            _window.Create();
            _window.Join();
        }

        public void SetSize(int x, int y, int width, int height)
        {
            _window.X = x - width / 2;
            _window.Y = y - height;
            _window.Width = width;
            _window.Height = height;
            _window.Graphics.Width = width;
            _window.Graphics.Height = height;
        }

        ~Overlay()
        {
            Dispose(false);
        }

        #region IDisposable Support
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                _window.Dispose();

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}