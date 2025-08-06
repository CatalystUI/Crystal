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
    /// A list of possible orientations for a display.
    /// </summary>
    /// <remarks>
    /// The value of each orientation corresponds to the clockwise rotation angle of the display.
    /// For example, <see cref="Portrait"/> is 90 degrees, and <see cref="LandscapeFlipped"/> is 180 degrees.
    /// These values align with the <see cref="ICrystalDisplay.Rotation"/> property.
    /// </remarks>
    public enum DisplayOrientation {
        
        /// <summary>
        /// The display is oriented in landscape mode.
        /// </summary>
        /// <value>The clockwise orientation of the display in degrees.</value>
        Landscape = 0,
        
        /// <summary>
        /// The display is oriented in portrait mode.
        /// </summary>
        /// <value>The clockwise orientation of the display in degrees.</value>
        Portrait = 90,
        
        /// <summary>
        /// The display is flipped and oriented in landscape mode.
        /// </summary>
        /// <value>The clockwise orientation of the display in degrees.</value>
        LandscapeFlipped = 180,
        
        /// <summary>
        /// The display is flipped and oriented in portrait mode.
        /// </summary>
        /// <value>The clockwise orientation of the display in degrees.</value>
        PortraitFlipped = 270
        
    }
    
    /// <summary>
    /// Extension methods for <see cref="DisplayOrientation"/> enum.
    /// </summary>
    public static class DisplayOrientationExtensions {
        
        /// <summary>
        /// Converts a rotational angle to a <see cref="DisplayOrientation"/>.
        /// </summary>
        /// <param name="rotation">The angle to convert, in degrees.</param>
        /// <returns>The corresponding <see cref="DisplayOrientation"/>.</returns>
        public static DisplayOrientation ToOrientation(this Angle rotation) {
            Angle shifted = Angle.FromRadians((rotation.Normalize().Radians + Math.PI / 4) % (2 * Math.PI));
            Quadrant quadrant = shifted.ToQuadrant();
            return quadrant switch {
                Quadrant.First => DisplayOrientation.Landscape,
                Quadrant.Second => DisplayOrientation.Portrait,
                Quadrant.Third => DisplayOrientation.LandscapeFlipped,
                Quadrant.Fourth => DisplayOrientation.PortraitFlipped,
                _ => throw new ArgumentOutOfRangeException(nameof(rotation), "Invalid rotation angle for display orientation.")
            };
        }
        
    }
    
}