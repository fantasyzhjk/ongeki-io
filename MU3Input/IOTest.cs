using System;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MU3Input
{
    public partial class IOTest : Form
    {
        private IO _io;
        private Overlay _overlay;

        private CheckBox[] _left;
        private CheckBox[] _right;

        public IOTest(IO io)
        {
            InitializeComponent();

            _left = new[] {
                lA,
                lB,
                lC,
                lS,
                lM,
            };

            _right = new[] {
                rA,
                rB,
                rC,
                rS,
                rM,
            };

            _io = io;

            textX.Text = Initialization.Overlay.X.ToString();
            textY.Text = Initialization.Overlay.Y.ToString();
            textWidth.Text = Initialization.Overlay.Width.ToString();
            textHeight.Text = Initialization.Overlay.Height.ToString();
        }

        public bool OverlayVisible
        {
            get => _overlay?.Visible ?? false;
            set
            {
                if (_overlay == null && int.TryParse(textX.Text, out int x) && int.TryParse(textY.Text, out int y) && int.TryParse(textWidth.Text, out int width) && int.TryParse(textHeight.Text, out int height))
                {
                    _overlay = new Overlay(x, y, width, height);
                    Task.Run(_overlay.Run);
                }
                if (_overlay == null) return;
                _overlay.Visible = value;
            }
        }

        public static byte[] StringToByteArray(string hex)
        {
            var numberChars = hex.Length;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        internal void UpdateData()
        {
            if (!Enabled && Handle == IntPtr.Zero) return;

            try
            {
                BeginInvoke(new Action(() =>
                {
                    lblStatus.Text = _io.IsConnected ? "Connected" : "Disconnected";

                    if (!_io.IsConnected) return;

                    for (var i = 0; i < 5; i++)
                    {
                        _left[i].Checked = Convert.ToBoolean(_io.Data.Buttons[i]);
                        _right[i].Checked = Convert.ToBoolean(_io.Data.Buttons[i + 5]);
                    }

                    trackBar1.Value = _io.Lever;

                    if (_io.Scan)
                    {
                        textAimiId.Text = BitConverter.ToString(_io.AimiId).Replace("-", "");
                    }
                }));
            }
            catch
            {
                // ignored
            }
        }

        public void SetColor(uint data)
        {
            try
            {
                BeginInvoke(new Action(() =>
                {
                    _left[0].BackColor = Color.FromArgb(
                        (int)((data >> 23) & 1) * 255,
                        (int)((data >> 19) & 1) * 255,
                        (int)((data >> 22) & 1) * 255
                    );
                    _left[1].BackColor = Color.FromArgb(
                        (int)((data >> 20) & 1) * 255,
                        (int)((data >> 21) & 1) * 255,
                        (int)((data >> 18) & 1) * 255
                    );
                    _left[2].BackColor = Color.FromArgb(
                        (int)((data >> 17) & 1) * 255,
                        (int)((data >> 16) & 1) * 255,
                        (int)((data >> 15) & 1) * 255
                    );
                    _right[0].BackColor = Color.FromArgb(
                        (int)((data >> 14) & 1) * 255,
                        (int)((data >> 13) & 1) * 255,
                        (int)((data >> 12) & 1) * 255
                    );
                    _right[1].BackColor = Color.FromArgb(
                        (int)((data >> 11) & 1) * 255,
                        (int)((data >> 10) & 1) * 255,
                        (int)((data >> 9) & 1) * 255
                    );
                    _right[2].BackColor = Color.FromArgb(
                        (int)((data >> 8) & 1) * 255,
                        (int)((data >> 7) & 1) * 255,
                        (int)((data >> 6) & 1) * 255
                    );
                }));
            }
            catch
            {
                // ignored
            }
        }

        private void ShowOverlay_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                OverlayVisible = checkBox.Checked;
                checkBox.Checked = OverlayVisible;
            }
        }

        private void textSize_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textWidth.Text, out int width) && int.TryParse(textHeight.Text, out int height) && int.TryParse(textX.Text, out int x) && int.TryParse(textY.Text, out int y))
            {
                _overlay?.SetSize(x, y, width, height);
                Initialization.Overlay.X = x;
                Initialization.Overlay.Y = y;
                Initialization.Overlay.Width = width;
                Initialization.Overlay.Height = height;
            }
        }
        private void textSize_MouseWheel(object sender, MouseEventArgs e)
        {
            if (sender is TextBox textBox && int.TryParse(textBox.Text, out int result))
            {
                int delta = e.Delta > 0 ? 1 : -1;
                textBox.Text = (result + delta).ToString();
            }
        }
    }
}
