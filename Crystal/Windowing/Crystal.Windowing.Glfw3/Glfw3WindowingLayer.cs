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

using Catalyst.Attributes.Threading;
using Catalyst.Threading;
using Crystal.Windowing.Model;
using Monitor = Silk.NET.GLFW.Monitor;

namespace Crystal.Windowing.Glfw3 {

    /// <summary>
    /// An implementation of <see cref="ICrystalWindowingLayer"/> using the Glfw3 windowing library.
    /// </summary>
    public sealed partial class Glfw3WindowingLayer : ICrystalWindowingLayer {

        /// <inheritdoc/>
        public IReadOnlyList<ICrystalDisplay> RequestDisplays() {
            if (!ThreadDelegateDispatcher.IsMainThreadCaptured) throw new RequiresMainThreadException(nameof(GlfwDisplay), nameof(RequestDisplays));
            if (!ThreadDelegateDispatcher.MainThreadDispatcher.Execute(_cachedFunctionGetDisplaysUnsafe, out GlfwDisplay[] displays)) {
                throw new WindowException("Failed to get the primary display on the main thread!");
            }
            return displays.OfType<ICrystalDisplay>().ToList();
        }

        [CachedDelegate]
        private static unsafe GlfwDisplay[] GetDisplaysUnsafe() {
            using Glfw3 glfw = Glfw3.GetInstance();
            Monitor** pMonitors = glfw.Api.GetMonitors(out int count);
            if (pMonitors == null || count <= 0) return [];
            GlfwDisplay[] displays = new GlfwDisplay[count];
            for (int i = 0; i < count; i++) {
                Monitor* pMonitor = pMonitors[i];
                displays[i] = GlfwDisplay.FromMonitor(glfw, pMonitor);
            }
            return displays;
        }

        /// <inheritdoc/>
        public ICrystalDisplay? RequestPrimaryDisplay() {
            if (!ThreadDelegateDispatcher.IsMainThreadCaptured) throw new RequiresMainThreadException(nameof(GlfwDisplay), nameof(RequestPrimaryDisplay));
            if (!ThreadDelegateDispatcher.MainThreadDispatcher.Execute(_cachedFunctionGetPrimaryDisplayUnsafe, out GlfwDisplay? display)) {
                throw new WindowException("Failed to get the primary display on the main thread!");
            }
            return display;
        }
        
        [CachedDelegate]
        private static unsafe GlfwDisplay? GetPrimaryDisplayUnsafe() {
            using Glfw3 glfw = Glfw3.GetInstance();
            Monitor* pPrimaryMonitor = glfw.Api.GetPrimaryMonitor();
            return pPrimaryMonitor != null ? GlfwDisplay.FromMonitor(glfw, pPrimaryMonitor) : null;
        }

    }

}