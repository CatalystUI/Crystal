// -------------------------------------------------------------------------------------------------
// CatalystUI - Cross-Platform UI Library
// Copyright (c) 2025 FireController#1847. All rights reserved.
// 
// This file is part of CatalystUI and is provided as part of an early-access release.
// Unauthorized commercial use, distribution, or modification is strictly prohibited.
// 
// This software is not open source and is not publicly licensed.
// For full terms, see the LICENSE and NOTICE files in the project root.
// -------------------------------------------------------------------------------------------------

using Catalyst.Mathematics.Geometry;

namespace Crystal.Models.CrystalWindow.Windowing {
    
    /// <summary>
    /// Represents an I/O device which provides an end-user some form
    /// of visual output in the form of pixels in a rectangular viewing
    /// area.
    /// </summary>
    public interface ICrystalDisplay {
        
        /// <summary>
        /// Gets the device descriptor of the display.
        /// </summary>
        /// <value>The display's device descriptor.</value>
        public string Descriptor { get; }
        
        /// <summary>
        /// Gets the manufacturer of the display.
        /// </summary>
        /// <value>The display's manufacturer, or <see langword="null"/> if one could not be determined.</value>
        public string? Manufacturer { get; }
        
        /// <summary>
        /// Gets the refresh rate of the display in hertz (Hz).
        /// </summary>
        /// <value>The display's refresh rate.</value>
        public double RefreshRate { get; }
        
        /// <summary>
        /// Gets the display's horizontal position relative to the
        /// primary display in pixels.
        /// </summary>
        /// <remarks>
        /// The horizontal position will always be reported as
        /// the distance from the primary display's left edge.
        /// Negative values indicate the display is to the left
        /// of the primary display, while positive values indicate
        /// the display is to the right of the primary display.
        /// </remarks>
        /// <value>The display's horizontal position in pixels.</value>
        public double X { get; }
        
        /// <summary>
        /// Gets the display's vertical position relative to the
        /// primary display in pixels.
        /// </summary>
        /// <remarks>
        /// The vertical position will always be reported as
        /// the distance from the primary display's top edge.
        /// Negative values indicate the display is above the
        /// primary display, while positive values indicate
        /// the display is below the primary display.
        /// </remarks>
        /// <value>The display's vertical position in pixels.</value>
        public double Y { get; }
        
        /// <summary>
        /// Gets the physical width of the display in pixels.
        /// </summary>
        /// <value>The display's width in pixels.</value>
        public uint Width { get; }
        
        /// <summary>
        /// Gets the physical height of the display in pixels.
        /// </summary>
        /// <value>The display's height in pixels.</value>
        public uint Height { get; }
        
        /// <summary>
        /// Gets the display's rotation in degrees.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A display's rotation is a more precise value of
        /// describing the display's orientation. If the
        /// rotation cannot be determined, then the display's
        /// rotation is reported as the <see cref="Orientation"/>
        /// underlying rotational value.
        /// </para>
        /// <para>
        /// Positive values rotate the display clockwise,
        /// whereas negative values rotate the display counter-clockwise.
        /// </para>
        /// </remarks>
        /// <value>The display's rotation in degrees.</value>
        public Angle Rotation { get; }
        
        /// <summary>
        /// Gets the orientation of the display.
        /// </summary>
        /// <value>The display's orientation.</value>
        public DisplayOrientation Orientation { get; }
        
        /// <summary>
        /// Gets the number of pixels per inch of the display.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Also known as the dots per inch (DPI) of the display,
        /// the default value is <c>96</c> PPI (pixels per inch),
        /// which is equivalent to <c>3/4</c> of a point in typography,
        /// which is defined as <c>1/72</c> of an inch.
        /// </para>
        /// <para>
        /// The PPI will always be reported as the
        /// physical pixel density of the display,
        /// and scaling should be performed
        /// by using the <see cref="ScalingFactor"/>
        /// property instead.
        /// </para>
        /// <para>
        /// In the context of the CatalystUI framework,
        /// the PPI and DPI are considered equivalent
        /// values, with both referring to the same
        /// context of pixel density for a display.
        /// </para>
        /// <para>
        /// For more information on the differences between
        /// PPI and DPI, see the following resources:
        /// <br/><br/>
        /// <see href="https://en.wikipedia.org/wiki/Pixel_density">Wikipedia - Pixel Density</see>
        /// <br/>
        /// <see href="https://en.wikipedia.org/wiki/Dots_per_inch">Wikipedia - Dots Per Inch</see>
        /// </para>
        /// </remarks>
        /// <value>The display's pixel density in pixels per inch.</value>
        public double PixelsPerInch { get; }
        
        /// <summary>
        /// Gets the scaling factor of the display.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A scaling factor is used to determine the
        /// display's logical size in pixels, and
        /// is represented as a decimal percentage.
        /// The value of this percentage
        /// is set by the user and reported by
        /// the user's system.
        /// </para>
        /// <para>
        /// Larger values indicate a smaller logical
        /// display size, which results in multiple
        /// physical pixels being used to represent
        /// one logical pixel. As a consequence,
        /// the perceived size of the display
        /// is increased.
        /// </para>
        /// </remarks>
        /// <example>
        /// A scaling factor of <c>1.25</c>
        /// indicates the display's physical
        /// size should be scaled by <c>125%</c>.
        /// If the reported physical width of the display
        /// is <c>1920</c> pixels, then to calculate
        /// the logical width, you would
        /// perform <c>1920 / 1.25</c> to get
        /// the value of <c>1536</c>.
        /// </example>
        /// <value>The display's scaling factor as a decimal percentage.</value>
        public double ScalingFactor { get; }
        
    }
    
}