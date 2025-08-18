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

using Catalyst.Connectors;
using Catalyst.Domains;
using Catalyst.Layers;
using Monitor = Silk.NET.GLFW.Monitor;

namespace Crystal.Windowing.Glfw3.NativeHandlers {
    
    /// <summary>
    /// Represents the low-level access needed for various Glfw3 functionality.
    /// </summary>
    /// <inheritdoc cref="INativeHandler{TLayerHigh, TLayerLow}"/>
    public unsafe interface IGlfw3NativeHandler<out TLayerLow> : INativeHandler<Glfw3WindowingLayer, TLayerLow> where TLayerLow : ISystemLayer<IDomain> {
        
        /// <summary>
        /// Gets the rotation of the specified display in degrees.
        /// </summary>
        /// <param name="glfw">The GLFW instance.</param>
        /// <param name="pMonitor">The GLFW monitor to get the rotation for.</param>
        /// <returns>The rotation of the display in degrees.</returns>
        public double GetDisplayRotation(Glfw3 glfw, Monitor* pMonitor);
        
        /// <summary>
        /// Gets the display descriptor from the EDID for the specified monitor.
        /// </summary>
        /// <param name="glfw">The GLFW instance.</param>
        /// <param name="pMonitor">The GLFW monitor to get the descriptor for.</param>
        /// <returns>The descriptor string.</returns>
        public string GetDisplayDescriptor(Glfw3 glfw, Monitor* pMonitor);
        
        /// <summary>
        /// Gets the display manufacturer from the EDID for the specified monitor.
        /// </summary>
        /// <param name="glfw">The GLFW instance.</param>
        /// <param name="pMonitor">The GLFW monitor to get the manufacturer for.</param>
        /// <returns>The manufacturer string.</returns>
        public string GetDisplayManufacturer(Glfw3 glfw, Monitor* pMonitor);
        
    }
    
}