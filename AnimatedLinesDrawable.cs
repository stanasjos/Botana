using System;
using System.Collections.Generic;
using Microsoft.Maui;                  // IDispatcher / Application
using Microsoft.Maui.Dispatching;      // IDispatcherTimer
using Microsoft.Maui.Graphics;         // ICanvas, Colors, SizeF, Color

namespace Botana
{
    /// Лёгкий эффект: хаотичные линии + «звёздочки»
    public sealed class AnimatedLinesDrawable : IDrawable, IDisposable
    {
        private readonly Random _rnd = new();
        private readonly List<Line> _lines = new();
        private readonly List<Star> _stars = new();
        private readonly IDispatcherTimer _timer;

        private GraphicsView? _host;
        private bool _started;
        private float _w, _h;

        private readonly Color _glowColor;

        public event EventHandler? Invalidated;

        public AnimatedLinesDrawable(Color glowColor, int lineCount = 3, double fps = 30)
        {
            _glowColor = glowColor;

            for (int i = 0; i < Math.Max(1, lineCount); i++)
                _lines.Add(new Line());

            _timer = Application.Current.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(1000.0 / Clamp(fps, 5, 120));
            _timer.Tick += Tick;
        }

        public void Start(GraphicsView host)
        {
            _host = host;
            if (_started) return;
            _started = true;
            _timer.Start();
        }

        public void Stop()
        {
            if (!_started) return;
            _started = false;
            _timer.Stop();
            _lines.ForEach(l => l.Life = 0);
            _stars.Clear();
            Invalidated?.Invoke(this, EventArgs.Empty);
        }

        public void Burst(int count = 12)
        {
            if (_w <= 0 || _h <= 0) return;

            for (int i = 0; i < count; i++)
            {
                _stars.Add(new Star
                {
                    X = (float)(_rnd.NextDouble() * _w),
                    Y = (float)(_rnd.NextDouble() * _h),
                    R = (float)(1.5 + _rnd.NextDouble() * 2.5),
                    VX = (float)((_rnd.NextDouble() - 0.5) * 50),
                    VY = (float)((_rnd.NextDouble() - 0.5) * 50),
                    Life = 1f,
                    MaxLife = 1f
                });
            }

            Invalidated?.Invoke(this, EventArgs.Empty);
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            _w = dirtyRect.Width;
            _h = dirtyRect.Height;

            if (_lines[0].MaxLife <= 0)
                foreach (var l in _lines) ResetLine(l);

            // лёгкое свечение и линии
            canvas.SaveState();
            canvas.StrokeSize = 2f;
            canvas.SetShadow(new SizeF(0, 0), 12f, _glowColor.WithAlpha(0.25f)); // <— правильная сигнатура

            foreach (var l in _lines)
            {
                var a = Clamp(l.Life / Math.Max(0.001f, l.MaxLife), 0, 1);
                canvas.StrokeColor = _glowColor.WithAlpha(0.25f + 0.55f * a);
                canvas.DrawLine(l.X1, l.Y1, l.X2, l.Y2);
            }
            canvas.RestoreState();

            // «звёздочки»
            foreach (var s in _stars)
            {
                var a = Clamp(s.Life / Math.Max(0.001f, s.MaxLife), 0, 1);
                canvas.FillColor = _glowColor.WithAlpha(0.10f + 0.40f * a);
                canvas.FillCircle(s.X, s.Y, s.R);
            }
        }

        private void Tick(object? sender, EventArgs e)
        {
            const float dt = 1f / 60f;

            foreach (var l in _lines)
            {
                l.Life += dt;
                l.X1 += l.VX * dt; l.Y1 += l.VY * dt;
                l.X2 += l.VX * dt; l.Y2 += l.VY * dt;

                if (l.Life >= l.MaxLife) ResetLine(l);
            }

            for (int i = _stars.Count - 1; i >= 0; i--)
            {
                var s = _stars[i];
                s.Life -= dt;
                s.X += s.VX * dt;
                s.Y += s.VY * dt;
                if (s.Life <= 0) _stars.RemoveAt(i);
            }

            Invalidated?.Invoke(this, EventArgs.Empty);
            _host?.Invalidate(); // перерисовать GraphicsView
        }

        private void ResetLine(Line l)
        {
            l.MaxLife = (float)(0.8 + _rnd.NextDouble() * 1.6);
            l.Life = 0f;

            float x = (float)(_rnd.NextDouble() * _w);
            float y = (float)(_rnd.NextDouble() * _h);
            float len = (float)(20 + _rnd.NextDouble() * 60);
            float ang = (float)(_rnd.NextDouble() * Math.PI * 2);

            l.X1 = x; l.Y1 = y;
            l.X2 = x + (float)Math.Cos(ang) * len;
            l.Y2 = y + (float)Math.Sin(ang) * len;

            float spd = (float)(10 + _rnd.NextDouble() * 50);
            l.VX = (float)Math.Cos(ang) * spd;
            l.VY = (float)Math.Sin(ang) * spd;
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Tick -= Tick;
        }

        private static float Clamp(double v, double min, double max)
            => (float)Math.Max(min, Math.Min(max, v));

        private sealed class Line
        {
            public float X1, Y1, X2, Y2;
            public float VX, VY;
            public float Life, MaxLife;
        }

        private sealed class Star
        {
            public float X, Y, VX, VY, R;
            public float Life, MaxLife;
        }
    }
}