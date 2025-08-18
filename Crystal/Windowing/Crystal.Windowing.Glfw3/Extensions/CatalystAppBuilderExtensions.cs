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

using Crystal.Windowing.Glfw3;
using Catalyst.Threading;

// ReSharper disable once CheckNamespace
namespace Catalyst.Builders.Extensions {
    
    /// <summary>
    /// Builder extensions for the <see cref="CatalystAppBuilder"/>.
    /// </summary>
    public static class CatalystAppBuilderExtensions {
        
        /// <summary>
        /// Adds the Crystal-based Glfw3 windowing module to the <see cref="CatalystAppBuilder"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The Crystal-based Glfw3 windowing module adds the following to your CatalystUI application:
        /// <list type="bullet">
        ///     <item><see cref="Glfw3WindowingLayer"/></item>
        /// </list>
        /// Click on any of the above links to learn more about each component.
        /// </para>
        /// </remarks>
        /// <param name="builder">The <see cref="CatalystAppBuilder"/> to add the module to.</param>
        /// <returns>The <see cref="CatalystAppBuilder"/> with the Glfw3 windowing module added.</returns>
        public static CatalystAppBuilder AddGlfw3WindowingModule(this CatalystAppBuilder builder) {
            Glfw3WindowingLayer glfw3WindowingLayer = new();
            ModelRegistry.RegisterLayer(glfw3WindowingLayer);
            return builder;
        }
        
    }
    
}