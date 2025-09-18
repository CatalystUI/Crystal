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

namespace Crystal.Profiling {
    
    /// <summary>
    /// Profiling program for CatalystUI or CatalystUI-based applications.
    /// </summary>
    /// <remarks>
    /// For usage information on this tool, please refer to the README.md file
    /// in the project root or the official documentation.
    /// </remarks>
    public static class Program {
        
        /// <summary>
        /// Main entry point for the profiling tool.
        /// </summary>
        /// <remarks>
        /// Do not modify this method. Please refer to the official documentation for usage instructions.
        /// </remarks>
        /// <param name="args">The command-line arguments.</param>
        public static void Main(string[] args) {
#if PROFILE_EXISTS
            Profile.Entry(args);
#endif
        }
        
    }
    
}