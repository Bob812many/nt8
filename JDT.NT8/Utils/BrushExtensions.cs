// ***********************************************************************
// Assembly         : JDT.NT8
// Author           : JasonnatorDayTrader
// Created          : 09-02-2020
//
// Last Modified By : JasonnatorDayTrader
// Last Modified On : 09-02-2020
// ***********************************************************************
// Created in support of my YouTube channel https://www.youtube.com/user/Jasonnator
// Code freely available at https://gitlab.com/jasonnatordaytrader/jdt.nt8	
// ***********************************************************************
using System;

namespace JDT.NT8.Utils
{
    public static class DrawingExtensions
    {
        /// <summary>
        /// Converts to color4.
        /// </summary>
        /// <param name="brush">The brush.</param>
        /// <returns>SharpDX.Color4.</returns>
        /// <exception cref="ArgumentNullException">brush</exception>
        public static SharpDX.Color4 ToColor4(this System.Windows.Media.SolidColorBrush brush)
        {
            if (brush == null)
            {
                throw new ArgumentNullException(nameof(brush));
            }

            return new SharpDX.Color4(brush.Color.R / 255.0f, brush.Color.G / 255.0f, brush.Color.B / 255.0f, 1.0f);
        }
    }
}
