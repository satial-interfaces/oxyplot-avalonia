using Avalonia.Collections;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Transactions;
using Avalonia;
using System.Runtime.InteropServices.WindowsRuntime;

namespace OxyPlot.Avalonia
{
    public class ImmediateModeRenderContext : RenderContextBase, ITextMeasurer
    {

        public ImmediateModeRenderContext(DrawingContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            RenderCache = new RenderCache();
            TextArranger = new TextArranger(this, new SimpleTextTrimmer());
        }

        private DrawingContext Context { get; }
        private RenderCache RenderCache { get; }
        private TextArranger TextArranger { get; }
        private DrawingContext.PushedState? CurrentClip = null;

        public override void DrawLine(IList<ScreenPoint> points, OxyColor stroke, double thickness, double[] dashArray, LineJoin lineJoin, bool aliased)
        {
            if (points.Count < 2 || thickness <= 0 || stroke.IsInvisible())
            {
                return;
            }

            var pen = new Pen(RenderCache.GetCachedBrush(stroke), thickness, ToPointDashArray(dashArray, 0.0), PenLineCap.Square, ToPointLineJoin(lineJoin)); // TODO: miter limit
            var geometry = ToPath(points, false);

            Context.DrawGeometry(null, pen, geometry);
        }

        public override void DrawPolygon(IList<ScreenPoint> points, OxyColor fill, OxyColor stroke, double thickness, double[] dashArray, LineJoin lineJoin, bool aliased)
        {
            if (points.Count < 2 || ((thickness <= 0 || stroke.IsInvisible()) && fill.IsInvisible()))
            {
                return;
            }

            var pen = new Pen(RenderCache.GetCachedBrush(stroke), thickness, ToPointDashArray(dashArray, 0.0), PenLineCap.Square, ToPointLineJoin(lineJoin)); // TODO: miter limit
            var brush = RenderCache.GetCachedBrush(fill);
            var geometry = ToPath(points, true);

            Context.DrawGeometry(brush, pen, geometry);
        }

        public override void DrawImage(OxyImage source, double srcX, double srcY, double srcWidth, double srcHeight, double destX, double destY, double destWidth, double destHeight, double opacity, bool interpolate)
        {
            if (destWidth <= 0 || destHeight <= 0 || srcWidth <= 0 || srcHeight <= 0)
            {
                return;
            }

            var bitmapChain = GetImageSource(source);
            var dest = new Rect(destX, destY, destWidth, destHeight);
                var src = new Rect(srcX, srcY, srcWidth, srcHeight);
            var interpolationMode = interpolate
                ? global::Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.LowQuality
                : global::Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.Default;

            Context.DrawImage(bitmapChain, src, dest, interpolationMode);
        }

        protected static Geometry ToPath(IList<ScreenPoint> points, bool closed)
        {
            var geometry = new PathGeometry();

            using (var path = geometry.Open())
            {
                bool first = true;

                foreach (var p in points)
                {
                    if (IsDefined(p))
                    {
                        if (first)
                        {
                            path.BeginFigure(ToPoint(p), closed);
                            first = false;
                        }
                        else
                        {
                            path.LineTo(ToPoint(p));
                        }
                    }
                    else if (!first)
                    {
                        path.EndFigure(closed);
                        first = true;
                    }
                }

                if (!first)
                {
                    path.EndFigure(closed);
                }
            }

            return geometry;
        }

        protected static Typeface GetTypeFace(string fontFamily, double fontWeight)
        {
            return new Typeface(fontFamily ?? "{$Default}", GetFontWeight(fontWeight), FontStyle.Normal);
        }

        public override void DrawText(ScreenPoint p, string text, OxyColor fill, string fontFamily, double fontSize, double fontWeight, double rotation, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, OxySize? maxSize)
        {
            if (text == null || !fill.IsVisible())
            {
                return;
            }

            var ft = new FormattedText();
            ft.Text = text;
            ft.FontSize = fontSize;
            ft.Typeface = GetTypeFace(fontFamily, fontWeight);

            this.TextArranger.ArrangeText(p, text, fontFamily, fontSize, fontWeight, rotation, horizontalAlignment, verticalAlignment, maxSize, OxyPlot.HorizontalAlignment.Left, TextVerticalAlignment.Top, out var lines, out var linePositions);

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var linePosition = ToPoint(linePositions[i]);

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // translate and rotate into position
                using (var translate = Context.PushPreTransform(Matrix.CreateTranslation(linePosition.X, linePosition.Y)))
                using (var rotate = Context.PushPreTransform(Matrix.CreateRotation(rotation / 180 * Math.PI)))
                {
                    //var size = MeasureText(line, fontFamily, fontSize, fontWeight);
                    //Context.DrawRectangle(new Pen(GetCachedBrush(OxyColors.Red), 1), new Rect(0, 0, size.Width, size.Height));
                    Context.DrawText(RenderCache.GetCachedBrush(fill), new Point(0, 0), ft);
                }
            }
        }

        public FontMetrics GetFontMetrics(string fontFamily, double fontSize, double fontWeight)
        {
            var tf = GetTypeFace(fontFamily, fontWeight);
            var gtf = tf.GlyphTypeface;

            var ascent = fontSize * MilliPointsToNominalResolution(Math.Abs(gtf.Ascent));
            var descent = fontSize * MilliPointsToNominalResolution(Math.Abs(gtf.Descent));
            var leading = fontSize * MilliPointsToNominalResolution(Math.Abs(gtf.LineGap));

            return new FontMetrics(ascent, descent, leading);
        }

        /// <summary>
        /// Converts millipoints (thousanths of 1/72nds of an inch) to pixels at 96 dots per inch.
        /// </summary>
        /// <param name="milliPoints">The number of milliPoints.</param>
        /// <returns>Pixels at the nominal resolution of 96 dots per inch. </returns>
        private static double MilliPointsToNominalResolution(int milliPoints)
        {
            return milliPoints * (0.5 / 1000); // this 0.5 makes no sense
        }

        public double MeasureTextWidth(string text, string fontFamily, double fontSize, double fontWeight)
        {
            return MeasureTextNative(text, fontFamily, fontSize, fontWeight).Width;
        }
        private static OxySize MeasureTextNative(string text, string fontFamily, double fontSize, double fontWeight)
        {
            var ft = new FormattedText();
            ft.Text = text;
            ft.FontSize = fontSize;
            ft.Typeface = GetTypeFace(fontFamily, fontWeight);
            return new OxySize(ft.Bounds.Width, ft.Bounds.Height);
        }

        public override OxySize MeasureText(string text, string fontFamily, double fontSize, double fontWeight)
        {
            return TextArranger.MeasureText(text, fontFamily, fontSize, fontWeight);
        }

        public override bool SetClip(OxyRect rect)
        {
            CurrentClip?.Dispose();
            CurrentClip = Context.PushClip(ToRect(rect));
            return true;
        }

        public override void ResetClip()
        {
            CurrentClip?.Dispose();
            CurrentClip = null;
        }
    }
}
