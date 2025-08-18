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

using Catalyst.Layers;

namespace Crystal.Windowing.Model {
    
    /// <summary>
    /// Represents the window layer API for Crystal-based windowing implementations.
    /// </summary>
    public interface ICrystalWindowingLayer : IWindowLayer<ICrystalWindowingDomain> {
        
        /// <summary>
        /// Queries the system for a list of all available displays.
        /// </summary>
        /// <returns>A read-only list of displays.</returns>
        public IReadOnlyList<ICrystalDisplay> RequestDisplays();
        
        /// <summary>
        /// Queries the system for the primary display.
        /// </summary>
        /// <returns>The primary display, or <see langword="null"/> if no primary display is available.</returns>
        public ICrystalDisplay? RequestPrimaryDisplay();
        
    }
    
}