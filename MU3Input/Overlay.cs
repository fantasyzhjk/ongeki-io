using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly Colors[] _colors = new Colors[6];

        private bool _inRhythmGame = false;

        public bool Visible { get => _window.IsVisible; set => _window.IsVisible = value; }

        public Overlay(int x, int y, int width, int height)
        {
            _brushes = new Dictionary<string, IBrush>();

            var gfx = new Graphics()
            {
                PerPrimitiveAntiAliasing = false,
                TextAntiAliasing = false
            };

            _window = new GraphicsWindow(gfx)
            {
                Title = "Ongeki IO Debug",
                FPS = 60,
                IsTopmost = true,
                IsVisible = true,
            };
            SetSize(x, y, width, height);

            _window.DestroyGraphics += _window_DestroyGraphics;
            _window.DrawGraphics += _window_DrawGraphics;
            _window.SetupGraphics += _window_SetupGraphics;
        }

        public void SetLed(uint data)
        {
            _colors[0] = (Colors)((data >> 23 & 1) << 2 | (data >> 19 & 1) << 1 | (data >> 22 & 1) << 0);
            _colors[1] = (Colors)((data >> 20 & 1) << 2 | (data >> 21 & 1) << 1 | (data >> 18 & 1) << 0);
            _colors[2] = (Colors)((data >> 17 & 1) << 2 | (data >> 16 & 1) << 1 | (data >> 15 & 1) << 0);
            _colors[3] = (Colors)((data >> 14 & 1) << 2 | (data >> 13 & 1) << 1 | (data >> 12 & 1) << 0);
            _colors[4] = (Colors)((data >> 11 & 1) << 2 | (data >> 10 & 1) << 1 | (data >> 9 & 1) << 0);
            _colors[5] = (Colors)((data >> 8 & 1) << 2 | (data >> 7 & 1) << 1 | (data >> 6 & 1) << 0);
            if (_colors.Count(c => c == Colors.Red) == 2 && _colors.Count(c => c == Colors.Green) == 2 && _colors.Count(c => c == Colors.Blue) == 2)
            {
                _inRhythmGame = true;
            }
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
            _brushes["background"] = gfx.CreateSolidBrush(0x0, 0x0, 0x0, 0x0);
            _brushes[Colors.Red.ToString().ToLower()] = gfx.CreateSolidBrush(0xFF, 0x45, 0x5B);
            _brushes[Colors.Green.ToString().ToLower()] = gfx.CreateSolidBrush(0x45, 0xFF, 0x75);
            _brushes[Colors.Blue.ToString().ToLower()] = gfx.CreateSolidBrush(0x45, 0x89, 0xFF);
            _brushes[Colors.Yellow.ToString().ToLower()] = gfx.CreateSolidBrush(0xFF, 0xD5, 0x45);
            _brushes[Colors.Cyan.ToString().ToLower()] = gfx.CreateSolidBrush(0x45, 0xF8, 0xFF);
            _brushes[Colors.Purple.ToString().ToLower()] = gfx.CreateSolidBrush(0x8B, 0x45, 0xFF);
            _brushes[Colors.Blank.ToString().ToLower()] = gfx.CreateSolidBrush(0xDD, 0xDD, 0xDD);
            _brushes[Colors.White.ToString().ToLower()] = gfx.CreateSolidBrush(0xFF, 0xFF, 0xFF);

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
            string windowText = sb.ToString();
            var gfx = e.Graphics;
            GenRects(gfx.Width, gfx.Height);
            gfx.ClearScene((SolidBrush)_brushes["background"]);
            if (windowText == "Otoge")
            {
                if (_inRhythmGame)
                {
                    _window.PlaceAbove(handle);
                }
            }
            if (windowText != "Otoge" && windowText != "Ongeki IO Debug")
            {
                return;
            }
            gfx.FillRectangle(_brushes[_colors[0].ToString().ToLower()], buttons[0]);
            gfx.FillRectangle(_brushes[_colors[1].ToString().ToLower()], buttons[1]);
            gfx.FillRectangle(_brushes[_colors[2].ToString().ToLower()], buttons[2]);
            gfx.FillRectangle(_brushes[_colors[3].ToString().ToLower()], buttons[3]);
            gfx.FillRectangle(_brushes[_colors[4].ToString().ToLower()], buttons[4]);
            gfx.FillRectangle(_brushes[_colors[5].ToString().ToLower()], buttons[5]);
            gfx.DrawRectangle(_brushes[Colors.White.ToString().ToLower()], buttons[0], 1);
            gfx.DrawRectangle(_brushes[Colors.White.ToString().ToLower()], buttons[1], 1);
            gfx.DrawRectangle(_brushes[Colors.White.ToString().ToLower()], buttons[2], 1);
            gfx.DrawRectangle(_brushes[Colors.White.ToString().ToLower()], buttons[3], 1);
            gfx.DrawRectangle(_brushes[Colors.White.ToString().ToLower()], buttons[4], 1);
            gfx.DrawRectangle(_brushes[Colors.White.ToString().ToLower()], buttons[5], 1);
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
            buttons[3] = new Rectangle(buttons[2].Right + lrSpacing - buttonSpacing, 0, buttons[2].Right + lrSpacing + buttonWidth, height);
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
    public enum Colors
    {
        Red = 0b100,
        Green = 0b010,
        Blue = 0b001,
        Yellow = 0b110,
        Cyan = 0b011,
        Purple = 0b101,
        White = 0b111,
        Blank = 0b000,
    }
}