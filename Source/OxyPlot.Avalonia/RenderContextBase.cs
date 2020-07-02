using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OxyPlot.Avalonia
{
    public abstract class RenderContextBase : OxyPlot.RenderContextBase
    {
        /// <summary>
        /// The images in use
        /// </summary>
        protected readonly HashSet<OxyImage> imagesInUse = new HashSet<OxyImage>();

        /// <summary>
        /// The image cache
        /// </summary>
        protected readonly Dictionary<OxyImage, IBitmap> imageCache = new Dictionary<OxyImage, IBitmap>();

        /// <summary>
        /// The brush cache.
        /// </summary>
        protected readonly Dictionary<OxyColor, IBrush> brushCache = new Dictionary<OxyColor, IBrush>();

        /// <summary>
        /// The canvas.
        /// </summary>
        protected readonly global::Avalonia.Controls.Canvas canvas;

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasRenderContext" /> class.
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        public RenderContextBase()
        {
            RendersToScreen = true;
        }

        /// <summary>
        /// Gets the font weight.
        /// </summary>
        /// <param name="fontWeight">The font weight value.</param>
        /// <returns>The font weight.</returns>
        protected static FontWeight GetFontWeight(double fontWeight)
        {
            return fontWeight > FontWeights.Normal ? FontWeight.Bold : FontWeight.Normal;
        }

        /// <summary>
        /// Gets the cached brush.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>The brush.</returns>
        protected IBrush GetCachedBrush(OxyColor color)
        {
            if (color.A == 0)
            {
                return null;
            }

            IBrush brush;
            if (!brushCache.TryGetValue(color, out brush))
            {
                brush = new SolidColorBrush(color.ToColor());
                brushCache.Add(color, brush);
            }

            return brush;
        }

        /// <summary>
        /// Gets the bitmap source.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>The bitmap source.</returns>
        protected IBitmap GetImageSource(OxyImage image)
        {
            if (image == null)
            {
                return null;
            }

            if (!imagesInUse.Contains(image))
            {
                imagesInUse.Add(image);
            }

            IBitmap src;
            if (imageCache.TryGetValue(image, out src))
            {
                return src;
            }

            using (var ms = new MemoryStream(image.GetData()))
            {
                var btm = new Bitmap(ms);
                imageCache.Add(image, btm);
                return btm;
            }
        }

        /// <summary>
        /// Converts a <see cref="ScreenPoint" /> to a <see cref="Point" />.
        /// </summary>
        /// <param name="pt">The screen point.</param>
        /// <returns>A <see cref="Point" />.</returns>
        protected static Point ToPoint(ScreenPoint pt)
        {
            return new Point(pt.X, pt.Y);
        }

        /// <summary>
        /// Converts a <see cref="ScreenPoint" /> to a pixel aligned<see cref="Point" />.
        /// </summary>
        /// <param name="pt">The screen point.</param>
        /// <returns>A pixel aligned <see cref="Point" />.</returns>
        protected static Point ToPixelAlignedPoint(ScreenPoint pt)
        {
            // adding 0.5 to get pixel boundary alignment, seems to work
            // http://weblogs.asp.net/mschwarz/archive/2008/01/04/silverlight-rectangles-paths-and-line-comparison.aspx
            // http://www.wynapse.com/Silverlight/Tutor/Silverlight_Rectangles_Paths_And_Lines_Comparison.aspx
            // TODO: issue 10221 - should consider line thickness and logical to physical size of pixels
            return new Point(0.5 + (int)pt.X, 0.5 + (int)pt.Y);
        }

        /// <summary>
        /// Converts an <see cref="OxyRect" /> to a <see cref="Rect" />.
        /// </summary>
        /// <param name="r">The rectangle.</param>
        /// <returns>A <see cref="Rect" />.</returns>
        protected static Rect ToRect(OxyRect r)
        {
            return new Rect(r.Left, r.Top, r.Width, r.Height);
        }

        /// <summary>
        /// Converts an <see cref="OxyRect" /> to a pixel aligned <see cref="Rect" />.
        /// </summary>
        /// <param name="r">The rectangle.</param>
        /// <returns>A pixel aligned<see cref="Rect" />.</returns>
        protected static Rect ToPixelAlignedRect(OxyRect r)
        {
            // TODO: similar changes as in ToPixelAlignedPoint
            var x = 0.5 + (int)r.Left;
            var y = 0.5 + (int)r.Top;
            var ri = 0.5 + (int)r.Right;
            var bo = 0.5 + (int)r.Bottom;
            return new Rect(x, y, ri - x, bo - y);
        }

        /// <summary>
        /// Converts a <see cref="ScreenPoint" /> to a <see cref="Point" />.
        /// </summary>
        /// <param name="pt">The screen point.</param>
        /// <param name="aliased">use pixel alignment conversion if set to <c>true</c>.</param>
        /// <returns>A <see cref="Point" />.</returns>
        protected static Point ToPoint(ScreenPoint pt, bool aliased)
        {
            return aliased ? ToPixelAlignedPoint(pt) : ToPoint(pt);
        }

        /// <summary>
        /// Creates a point collection from the specified points.
        /// </summary>
        /// <param name="points">The points to convert.</param>
        /// <param name="aliased">convert to pixel aligned points if set to <c>true</c>.</param>
        /// <returns>The point collection.</returns>
        protected static List<Point> ToPointCollection(IEnumerable<ScreenPoint> points, bool aliased)
        {
            return new List<Point>(aliased ? points.Select(ToPixelAlignedPoint) : points.Select(ToPoint));
        }

        protected static DashStyle ToPointDashArray(double[] dashArray, double dashOffset)
        {
            return new DashStyle(dashArray, dashOffset);
        }

        protected static PenLineJoin ToPointLineJoin(LineJoin lineJoin)
        {
            switch (lineJoin)
            {
                case LineJoin.Round:
                    return PenLineJoin.Round;
                case LineJoin.Bevel:
                    return PenLineJoin.Bevel;
                case LineJoin.Miter:
                    return PenLineJoin.Miter;
            }

            throw new ArgumentException(nameof(lineJoin));
        }

        protected static bool IsDefined(ScreenPoint point)
        {
            return !(double.IsNaN(point.X) || double.IsNaN(point.Y));
        }
    }
}
