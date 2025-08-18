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
using Catalyst.Debugging;
using Catalyst.Domains;
using Catalyst.Layers;
using Catalyst.Mathematics.Geometry;
using Catalyst.Supplementary.Model.Systems;
using Crystal.Windowing.Glfw3.NativeHandlers;
using Silk.NET.GLFW;
using Monitor = Silk.NET.GLFW.Monitor;

namespace Crystal.Windowing.Glfw3 {
    
    /// <summary>
    /// An implementation of <see cref="ICrystalDisplay"/> which uses the Glfw3 windowing library.
    /// </summary>
    public readonly record struct GlfwDisplay : ICrystalDisplay {
        
        /// <inheritdoc/>
        public required string Descriptor { get; init; }
        
        /// <inheritdoc/>
        public required string? Manufacturer { get; init; }
        
        /// <inheritdoc/>
        public required double RefreshRate { get; init; }
        
        /// <inheritdoc/>
        public required double X { get; init; }
        
        /// <inheritdoc/>
        public required double Y { get; init; }
        
        /// <inheritdoc/>
        public required uint Width { get; init; }
        
        /// <inheritdoc/>
        public required uint Height { get; init; }
        
        /// <inheritdoc/>
        public required Angle Rotation { get; init; }
        
        /// <inheritdoc/>
        public required DisplayOrientation Orientation { get; init; }
        
        /// <inheritdoc/>
        public required double PixelsPerInch { get; init; }
        
        /// <inheritdoc/>
        public required double ScalingFactor { get; init; }
        
        /// <summary>
        /// Constructs a <see cref="GlfwDisplay"/> from a Glfw <see cref="Monitor"/> pointer.
        /// </summary>
        /// <param name="glfw">The Glfw3 api instance.</param>
        /// <param name="pMonitor">The pointer to the Glfw monitor.</param>
        /// <returns>A new instance of <see cref="GlfwDisplay"/>.</returns>
        public static unsafe GlfwDisplay FromMonitor(Glfw3 glfw, in Monitor* pMonitor) {
            // Parse video mode
            VideoMode* pVideoMode = glfw.Api.GetVideoMode(pMonitor);
            if (pVideoMode == null) throw new WindowException("Failed to get the video mode on the monitor!");
            Glfw3.DebugContext.Log(LogLevel.Verbose, $"Parsed video mode: {pVideoMode->Width}x{pVideoMode->Height} @ {pVideoMode->RefreshRate}Hz");
            
            // Get position and scaling
            glfw.Api.GetMonitorPos(pMonitor, out int x, out int y);
            glfw.Api.GetMonitorContentScale(pMonitor, out float xScale, out float yScale);
            double scale = (xScale + yScale) / 2.0; // Average scaling factor
            Glfw3.DebugContext.Log(LogLevel.Verbose, $"Parsed monitor position: ({x}, {y}), Scaling factor: {scale}");
            
            // Get the PPI (Pixels Per Inch)
            glfw.Api.GetMonitorPhysicalSize(pMonitor, out int physicalWidth, out int physicalHeight);
            double physicalWidthInInches = physicalWidth / 25.4; // Convert mm to inches
            double physicalHeightInInches = physicalHeight / 25.4; // Convert mm to inches
            double ppiX = physicalWidthInInches > 0 ? pVideoMode->Width / physicalWidthInInches : 0;
            double ppiY = physicalHeightInInches > 0 ? pVideoMode->Height / physicalHeightInInches : 0;
            double ppi = (ppiX + ppiY) / 2.0; // Average PPI
            Glfw3.DebugContext.Log(LogLevel.Verbose, $"Parsed monitor physical size: {physicalWidth}mm x {physicalHeight}mm, PPI: {ppi}");
            
            // Check if we have a native handler for this system
            IGlfw3NativeHandler<ISystemLayer<IDomain>>? nativeHandler;
            try {
                nativeHandler = ModelRegistry.RequestConnector<IGlfw3NativeHandler<ISystemLayer<IDomain>>>();
                Glfw3.DebugContext.Log(LogLevel.Verbose, $"Found Glfw3 native handler: {nativeHandler}");
            } catch {
                nativeHandler = null;
                Glfw3.DebugContext.Log(LogLevel.Warning, "No native handler found for Glfw3. Some QOL features may not be available.");
            }
            
            // Get native details
            Angle rotation;
            DisplayOrientation orientation;
            try {
                if (nativeHandler != null) {
                    rotation = Angle.FromDegrees(nativeHandler.GetDisplayRotation(glfw, pMonitor));
                    orientation = rotation.ToOrientation();
                } else {
                    throw new PlatformNotSupportedException();
                }
            } catch {
                // Use physical size to determine orientation if rotation is not available
                if (physicalWidth >= physicalHeight) {
                    rotation = Angle.FromDegrees(0);
                    orientation = DisplayOrientation.Landscape;
                } else {
                    rotation = Angle.FromDegrees(90);
                    orientation = DisplayOrientation.Portrait;
                }
            }
            Glfw3.DebugContext.Log(LogLevel.Verbose, $"Parsed monitor rotation: {rotation}, Orientation: {orientation}");
            
            // Get EDID descriptor and manufacturer
            string descriptor;
            try {
                if (nativeHandler != null) {
                    descriptor = nativeHandler.GetDisplayDescriptor(glfw, pMonitor);
                } else {
                    throw new PlatformNotSupportedException();
                }
            } catch {
                descriptor = glfw.Api.GetMonitorName(pMonitor); // Fallback to GLFW monitor name if EDID is not available
            }
            Glfw3.DebugContext.Log(LogLevel.Verbose, $"Parsed monitor descriptor: {descriptor}");
            string? manufacturer;
            try {
                if (nativeHandler != null) {
                    manufacturer = nativeHandler.GetDisplayManufacturer(glfw, pMonitor);
                } else {
                    throw new PlatformNotSupportedException();
                }
            } catch {
                manufacturer = null; // Fallback to null if manufacturer is not available
            }
            Glfw3.DebugContext.Log(LogLevel.Verbose, $"Parsed monitor manufacturer: {manufacturer ?? "Unknown"}");
            
            // Construct and return
            return new() {
                Descriptor = descriptor,
                Manufacturer = manufacturer,
                RefreshRate = (uint) pVideoMode->RefreshRate,
                X = x,
                Y = y,
                Width = (uint) pVideoMode->Width,
                Height = (uint) pVideoMode->Height,
                Rotation = rotation,
                Orientation = orientation,
                PixelsPerInch = ppi,
                ScalingFactor = scale
            };
        }
        
    }
    
}