using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OxyPlot.Avalonia
{
    public class RenderCache
    {
        /// <summary>
        /// The images in use
        /// </summary>
        private readonly HashSet<OxyImage> imagesInUse = new HashSet<OxyImage>();

        /// <summary>
        /// The image cache
        /// </summary>
        private readonly Dictionary<OxyImage, IBitmap> imageCache = new Dictionary<OxyImage, IBitmap>();

        /// <summary>
        /// The brush cache.
        /// </summary>
        private readonly Dictionary<OxyColor, IBrush> brushCache = new Dictionary<OxyColor, IBrush>();

        /// <summary>
        /// Gets the cached brush.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>The brush.</returns>
        public IBrush GetCachedBrush(OxyColor color)
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
        public IBitmap GetImageSource(OxyImage image)
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
    }
}
