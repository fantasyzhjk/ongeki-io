using GameOverlay.Drawing;

using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using DXLinearGradientBrush = SharpDX.Direct2D1.LinearGradientBrush;
using System;
using System.Text;

namespace MU3Input
{
    public class LinearGradientBrush : IDisposable, IBrush
    {
        private DXLinearGradientBrush _brush;

        private bool disposedValue;

        public Brush Brush
        {
            get
            {
                return _brush;
            }
            set
            {
                _brush = (DXLinearGradientBrush)value;
            }
        }

        public LinearGradientBrush(RenderTarget renderTarget, RawVector2 startPoint, RawVector2 endPoint, RawColor4 startColor, RawColor4 endColor)
        {
            if (renderTarget == null)
            {
                throw new ArgumentNullException("renderTarget");
            }

            var linearGradientBrushProperties = new LinearGradientBrushProperties()
            {
                StartPoint = startPoint,
                EndPoint = endPoint
            };
            var gradientStops = new GradientStop[]
            {
                new GradientStop()
                {
                    Color = startColor,
                    Position = 0f
                },
                new GradientStop()
                {
                    Color = endColor,
                    Position = 1f
                }
            };
            var gradientStopCollection = new GradientStopCollection(renderTarget, gradientStops);
            _brush = new DXLinearGradientBrush(renderTarget, linearGradientBrushProperties, gradientStopCollection);
        }

        ~LinearGradientBrush()
        {
            Dispose(disposing: false);
        }

        public override bool Equals(object obj)
        {
            LinearGradientBrush solidBrush = obj as LinearGradientBrush;
            if (solidBrush != null)
            {
                return solidBrush._brush.NativePointer == _brush.NativePointer;
            }

            return false;
        }

        public bool Equals(LinearGradientBrush value)
        {
            if (value != null)
            {
                return value._brush.NativePointer == _brush.NativePointer;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCodes(_brush.NativePointer.GetHashCode());
        }

        public static int HashCodes(params int[] hashCodes)
        {
            if (hashCodes == null)
            {
                throw new ArgumentNullException("hashCodes");
            }

            if (hashCodes.Length == 0)
            {
                throw new ArgumentOutOfRangeException("hashCodes");
            }

            int num = 17;
            foreach (int num2 in hashCodes)
            {
                num = num * 23 + num2;
            }

            return num;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (_brush != null)
                {
                    _brush.Dispose();
                    _brush = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public static implicit operator DXLinearGradientBrush(LinearGradientBrush brush)
        {
            return brush._brush;
        }

        public static bool Equals(LinearGradientBrush left, LinearGradientBrush right)
        {
            return left?.Equals(right) ?? false;
        }
    }
}

