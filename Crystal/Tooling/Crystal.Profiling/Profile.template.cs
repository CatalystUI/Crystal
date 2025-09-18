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

using Catalyst;
using Catalyst.Builders;
using Catalyst.Builders.Extensions;
using Catalyst.Debugging;

namespace Crystal.Profiling {
    
    /// <summary>
    /// A template for profiling CatalystUI or a CatalystUI-based application.
    /// </summary>
    public static class ProfileTemplate {
        
        /// <summary>
        /// The debug context for profiling operations.
        /// </summary>
        private static DebugContext _debug;
        
        /// <summary>
        /// Static constructor to initialize the debug context.
        /// </summary>
        static ProfileTemplate() {
            _debug = null!;
        }
        
        /// <summary>
        /// Acts as the main entry point for the profiling code.
        /// </summary>
        public static void Entry(string[] args) {
            new CatalystAppBuilder()
#if DEBUG
                .UseCatalystDebug()
#endif
                .Build(Run);
        }
        
        /// <summary>
        /// Runs the profiling code.
        /// </summary>
        public static void Run(CatalystApp app) {
            _debug = CatalystDebug.ForContext("Profiling");
            _debug.LogInfo("Hello, world!");
        }
        
    }
    
}