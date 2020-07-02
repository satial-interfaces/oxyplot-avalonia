using System;
using System.Collections.Generic;
using System.Text;

namespace OxyPlot.Avalonia
{
    /// <summary>
    /// Measures text.
    /// </summary>
    public interface ITextMeasurer
    {
        /// <summary>
        /// Determines basic font metrics for the given font.
        /// </summary>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">The font size in 1/96ths of an inch.</param>
        /// <param name="fontWeight">The font weight.</param>
        /// <returns>The font metrics.</returns>
        FontMetrics GetFontMetrics(string fontFamily, double fontSize, double fontWeight);

        /// <summary>
        /// Measures the width of one line of text.
        /// </summary>
        /// <param name="text">The single line of text to measure.</param>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">The font size in 1/96ths of an inch.</param>
        /// <param name="fontWeight">The font weight.</param>
        /// <returns>The width in pixels of the text.</returns>
        double MeasureTextWidth(string text, string fontFamily, double fontSize, double fontWeight);
    }
}
