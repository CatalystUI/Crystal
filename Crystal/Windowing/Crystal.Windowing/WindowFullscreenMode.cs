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

namespace Crystal.Windowing {
    
    /// <summary>
    /// The supported fullscreen modes for a window.
    /// </summary>
    public enum WindowFullscreenMode {
        
        /// <summary>
        /// The window is displayed in normal mode within the bounds of the system environment.
        /// </summary>
        Windowed,
        
        /// <summary>
        /// The window fills the entire screen without entering exclusive fullscreen mode.
        /// </summary>
        Borderless,
        
        /// <summary>
        /// The window enters exclusive fullscreen mode which may change the display resolution.
        /// </summary>
        Fullscreen
        
    }
    
}