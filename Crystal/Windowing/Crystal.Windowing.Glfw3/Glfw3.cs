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
using Catalyst.Native;
using Catalyst.Threading;
using Silk.NET.GLFW;

namespace Crystal.Windowing.Glfw3 {
    
    /// <summary>
    /// Native API wrapper for the Glfw3 library.
    /// </summary>
    public sealed partial class Glfw3 : INativeApi<Glfw3, Glfw> {
        
        /// <summary>
        /// Gets the GLFW 3 API instance.
        /// </summary>
        private static Glfw? _api;
        
        /// <summary>
        /// The number of counts for requests to the GLFW 3 API.
        /// </summary>
        private static ushort _referenceCount;
        
        /// <summary>
        /// A static lock used to ensure thread-safe access to the GLFW 3 API.
        /// </summary>
        private static readonly Lock _staticLock;
        
        /// <summary>
        /// A flag indicating whether the object has been disposed of.
        /// </summary>
        private bool _disposed;
        
        /// <summary>
        /// A lock used to ensure thread-safe access to the object.
        /// </summary>
        private readonly Lock _lock;
        
        /// <summary>
        /// Gets the GLFW 3 API instance.
        /// </summary>
        /// <value>The GLFW 3 API instance.</value>
        public Glfw Api => _api!; // Non-nullable because it is initialized in the static constructor
        
        /// <summary>
        /// Static constructor for <see cref="Glfw3"/>.
        /// </summary>
        static Glfw3() {
            _referenceCount = 0;
            _staticLock = new();
        }
        
        /// <summary>
        /// Constructs a new <see cref="Glfw3"/>.
        /// </summary>
        private Glfw3() {
            // Fields
            _disposed = false;
            _lock = new();
        }
        
        /// <summary>
        /// Disposes of the <see cref="Glfw3"/>.
        /// </summary>
        /// <exclude/>
        ~Glfw3() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }
        
        /// <summary>
        /// Requests the GLFW 3 API.
        /// </summary>
        /// <returns>The requested <see cref="Glfw3"/> instance.</returns>
        public static Glfw3 GetInstance() {
            lock (_staticLock) {
                if (_referenceCount == 0) Initialize();
                _referenceCount++;
                return new();
            }
        }
        
        /// <summary>
        /// Initializes the GLFW 3 API.
        /// </summary>
        private static void Initialize() {
            if (!ThreadDelegateDispatcher.IsMainThreadCaptured) throw new RequiresMainThreadException(nameof(Glfw3), nameof(GetInstance));
            if (!ThreadDelegateDispatcher.MainThreadDispatcher.Execute(_cachedActionInitializeUnsafe, true)) {
                throw new TypeInitializationException(nameof(Glfw3), new InvalidOperationException("Failed to initialize the GLFW 3 API on the main thread!"));
            }
        }
        
        [CachedDelegate]
        private static void InitializeUnsafe() {
            if (_api != null) throw new InvalidOperationException("The GLFW 3 API has already been initialized.");
            _api = Glfw.GetApi();
            if (!_api.Init()) throw new WindowException("Failed to initialize the GLFW 3 API!");
        }
        
        /// <summary>
        /// Terminates the GLFW 3 API.
        /// </summary>
        private static void Terminate() {
            if (!ThreadDelegateDispatcher.IsMainThreadCaptured) throw new RequiresMainThreadException(nameof(Glfw3), nameof(GetInstance));
            if (!ThreadDelegateDispatcher.MainThreadDispatcher.Execute(_cachedActionTerminateUnsafe, true)) {
                throw new TypeInitializationException(nameof(Glfw3), new InvalidOperationException("Failed to terminate the GLFW 3 API on the main thread!"));
            }
        }
        
        [CachedDelegate]
        private static void TerminateUnsafe() {
            _api?.Terminate();
            _api?.Dispose();
            _api = null;
        }
        
        /// <summary>
        /// Disposes of the <see cref="Glfw3"/>.
        /// </summary>
        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        
        /// <param name="disposing"><see langword="false"/> if disposal is being performed by the garbage collector, otherwise <see langword="true"/></param>
        /// <inheritdoc cref="Dispose()"/>
        private void Dispose(bool disposing) {
            _lock.Enter();
            try {
                if (_disposed) return;
                
                // Dispose managed state (managed objects)
                if (disposing) {
                    // ...
                }
                
                // Dispose unmanaged state (unmanaged objects)
                lock (_staticLock) {
                    _referenceCount--;
                    if (_referenceCount == 0) Terminate();
                }
                
                // Indicate disposal completion
                _disposed = true;
            } finally {
                _lock.Exit();
            }
        }
        
    }
    
}