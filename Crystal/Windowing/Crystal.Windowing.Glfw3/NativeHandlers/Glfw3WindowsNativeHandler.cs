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

using Catalyst.Supplementary.Model.Systems;
using Monitor = Silk.NET.GLFW.Monitor;

namespace Crystal.Windowing.Glfw3.NativeHandlers {
    
    /// <summary>
    /// A Microsoft Windows-based implementation of <see cref="IGlfw3NativeHandler{TLayerLow}"/>.
    /// </summary>
    public sealed unsafe class Glfw3WindowsNativeHandler : IGlfw3NativeHandler<IWindowsSystemLayer> {
        
        /// <inheritdoc/>
        public double GetDisplayRotation(Glfw3 glfw, Monitor* pMonitor) {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        public string GetDisplayDescriptor(Glfw3 glfw, Monitor* pMonitor) {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        public string GetDisplayManufacturer(Glfw3 glfw, Monitor* pMonitor) {
            throw new NotImplementedException();
        }
        
    }
    
}