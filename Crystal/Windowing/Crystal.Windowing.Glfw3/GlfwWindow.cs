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
using Catalyst.Mathematics;
using Catalyst.Threading;
using Crystal.Windowing.Glfw3.NativeHandlers;
using Silk.NET.GLFW;
using System.Runtime.InteropServices;

namespace Crystal.Windowing.Glfw3 {

    /// <summary>
    /// An implementation of <see cref="ICrystalWindow"/> which uses the Glfw3 windowing library.
    /// </summary>
    public class GlfwWindow : ICrystalWindow {
        
        /// <summary>
        /// The minimum allowed poll rate in milliseconds.
        /// </summary>
        /// <remarks>
        /// In passive polling mode, to prevent lockups of the
        /// main thread if the window is not responding, waiting
        /// for events will time out after the minimum poll rate
        /// in milliseconds.
        /// </remarks>
        public const int MINIMUM_POLL_RATE = 3000;

        /// <inheritdoc/>
        public event ICrystalWindow.WindowErroredEventHandler? Errored;

        /// <inheritdoc/>
        public event ICrystalWindow.WindowEventHandler? Created;

        /// <inheritdoc/>
        public event ICrystalWindow.WindowEventHandler? Repositioned;

        /// <inheritdoc/>
        public event ICrystalWindow.WindowEventHandler? Resized;

        /// <inheritdoc/>
        public event ICrystalWindow.WindowEventHandler? Refresh;

        /// <inheritdoc/>
        public event ICrystalWindow.WindowEventHandler? Redraw;

        /// <inheritdoc/>
        public event ICrystalWindow.WindowEventHandler? Focused;

        /// <inheritdoc/>
        public event ICrystalWindow.WindowEventHandler? Unfocused;

        /// <inheritdoc/>
        public event ICrystalWindow.WindowEventHandler? Minimized;

        /// <inheritdoc/>
        public event ICrystalWindow.WindowEventHandler? Maximized;

        /// <inheritdoc/>
        public event ICrystalWindow.WindowEventHandler? Restored;

        /// <inheritdoc/>
        public event ICrystalWindow.WindowEventHandler? Shown;

        /// <inheritdoc/>
        public event ICrystalWindow.WindowEventHandler? Hidden;

        /// <inheritdoc/>
        public event ICrystalWindow.WindowClosingEventHandler? Closing;

        /// <inheritdoc/>
        public event ICrystalWindow.WindowEventHandler? Closed;

        /// <summary>
        /// The Glfw3 API instance which is managing the window.
        /// </summary>
        public Glfw3 Glfw { get; private set; }

        /// <summary>
        /// The internal Glfw3 window handle.
        /// </summary>
        public nint GlfwHandle { get; private set; }

        /// <summary>
        /// A generated log ID for the window instance.
        /// </summary>
        protected string LogId => $"{nameof(GlfwWindow)} {(NativeHandle.Length > 0 ? $"0x{NativeHandle[0]:X}" : "0x????")}";

        /// <summary>
        /// Internal reference for <see cref="NativeHandle"/>.
        /// </summary>
        protected nint[] _nativeHandle;

        /// <inheritdoc/>
        public virtual nint[] NativeHandle => _nativeHandle;

        /// <summary>
        /// Internal reference for <see cref="PollRate"/>.
        /// </summary>
        protected volatile ushort _pollRate;

        /// <inheritdoc/>
        public virtual ushort PollRate {
            get => _pollRate;
            set {
                ObjectDisposedException.ThrowIf(_disposed, this);
                _pollRate = value;
                if (ThreadDelegateDispatcher.MainThreadDispatcher != null) {
                    // Simply enqueuing a no-op will cause the main thread dispatcher to wake up
                    // and re-evaluate the poll rate on the next iteration.
                    ThreadDelegateDispatcher.MainThreadDispatcher.Execute(() => {
                        // ... 
                    });
                }
            }
        }

        /// <summary>
        /// Internal reference for <see cref="Display"/>.
        /// </summary>
        protected volatile ICrystalDisplay? _display;

        /// <inheritdoc/>
        public virtual ICrystalDisplay? Display => _display;

        /// <summary>
        /// Internal reference for <see cref="Title"/>.
        /// </summary>
        protected volatile string _title;

        /// <inheritdoc/>
        public virtual unsafe string Title {
            get => _title;
            set {
                ObjectDisposedException.ThrowIf(_disposed, this);
                if (!ThreadDelegateDispatcher.IsMainThreadCaptured) throw new RequiresMainThreadException(nameof(GlfwWindow), nameof(Title));
                ThreadDelegateDispatcher.MainThreadDispatcher.Execute(() => {
                    Glfw.Api.SetWindowTitle((WindowHandle*) GlfwHandle, value);
                    _title = value;
                    Glfw3.DebugContext.Log(LogLevel.Verbose, $"Set title to \"{value}\"");
                });
            }
        }

        /// <summary>
        /// Internal reference for <see cref="X"/>.
        /// </summary>
        protected double _x;

        /// <inheritdoc/>
        public virtual double X {
            get => Volatile.Read(ref _x);
            set => SetPosition(value, Y);
        }

        /// <summary>
        /// Internal reference for <see cref="Y"/>.
        /// </summary>
        protected double _y;

        /// <inheritdoc/>
        public virtual double Y {
            get => Volatile.Read(ref _y);
            set => SetPosition(X, value);
        }

        /// <summary>
        /// Internal reference for <see cref="MinimumWidth"/>.
        /// </summary>
        protected volatile uint _minimumWidth;

        /// <inheritdoc/>
        public virtual uint MinimumWidth {
            get => _minimumWidth;
            set => SetSizeLimits(value, MinimumHeight, MaximumWidth, MaximumHeight);
        }

        /// <summary>
        /// Internal reference for <see cref="Width"/>.
        /// </summary>
        protected volatile uint _width;

        /// <inheritdoc/>
        public virtual uint Width {
            get => _width;
            set => SetSize(value, Height);
        }

        /// <summary>
        /// Internal reference for <see cref="MaximumWidth"/>.
        /// </summary>
        protected volatile uint _maximumWidth;

        /// <inheritdoc/>
        public virtual uint MaximumWidth {
            get => _maximumWidth;
            set => SetSizeLimits(MinimumWidth, MinimumHeight, value, MaximumHeight);
        }

        /// <summary>
        /// Internal reference for <see cref="MinimumHeight"/>.
        /// </summary>
        protected volatile uint _minimumHeight;

        /// <inheritdoc/>
        public virtual uint MinimumHeight {
            get => _minimumHeight;
            set => SetSizeLimits(MinimumWidth, value, MaximumWidth, MaximumHeight);
        }

        /// <summary>
        /// Internal reference for <see cref="Height"/>.
        /// </summary>
        protected volatile uint _height;

        /// <inheritdoc/>
        public virtual uint Height {
            get => _height;
            set => SetSize(Width, value);
        }

        /// <summary>
        /// Internal reference for <see cref="MaximumHeight"/>.
        /// </summary>
        protected volatile uint _maximumHeight;

        /// <inheritdoc/>
        public virtual uint MaximumHeight {
            get => _maximumHeight;
            set => SetSizeLimits(MinimumWidth, MinimumHeight, MaximumWidth, value);
        }

        /// <summary>
        /// Internal reference for <see cref="FullscreenMode"/>.
        /// </summary>
        protected volatile WindowFullscreenMode _fullscreenMode;

        /// <inheritdoc/>
        public virtual WindowFullscreenMode FullscreenMode {
            get => _fullscreenMode;
            set => SetFullscreen(value);
        }

        /// <summary>
        /// Internal reference for <see cref="IsResizable"/>.
        /// </summary>
        protected volatile bool _isResizable;

        /// <inheritdoc/>
        public virtual bool IsResizable => _isResizable;

        /// <summary>
        /// Internal reference for <see cref="IsDecorated"/>.
        /// </summary>
        protected volatile bool _isDecorated;

        /// <inheritdoc/>
        public virtual bool IsDecorated => _isDecorated;

        /// <summary>
        /// Internal reference for <see cref="IsFocused"/>.
        /// </summary>
        protected volatile bool _isFocused;

        /// <inheritdoc/>
        public virtual bool IsFocused => _isFocused;

        /// <inheritdoc/>
        public virtual bool IsUnfocused => !_isFocused;

        /// <summary>
        /// Internal reference for <see cref="IsMinimized"/>.
        /// </summary>
        protected volatile bool _isMinimized;

        /// <inheritdoc/>
        public virtual bool IsMinimized => _isMinimized;

        /// <summary>
        /// Internal reference for <see cref="IsMaximized"/>.
        /// </summary>
        protected volatile bool _isMaximized;

        /// <inheritdoc/>
        public virtual bool IsMaximized => _isMaximized;

        /// <summary>
        /// Internal reference for <see cref="IsVisible"/>.
        /// </summary>
        protected volatile bool _isVisible;

        /// <inheritdoc/>
        public virtual bool IsVisible => _isVisible;

        /// <inheritdoc/>
        public virtual bool IsHidden => !_isVisible;
        
        /// <inheritdoc/>
        public virtual bool IsClosed => _disposed;
        
        /// <summary>
        /// A handle used to reset the poll wait for active polling.
        /// </summary>
        protected readonly ManualResetEvent _resetPollEventHandle;
        
        /// <summary>
        /// The state of the reset poll event handle.
        /// </summary>
        protected bool _resetPollEventHandleState;
        
        /// <summary>
        /// Cached delegate reference to the passive wait for events method.
        /// </summary>
        protected readonly ThreadDelegateDispatcher.DispatcherEventHandler _preExecuteHandler;
        
        /// <summary>
        /// Cached delegate reference to the pre-execute handler for the main thread dispatcher.
        /// </summary>
        protected readonly ThreadDelegateDispatcher.DispatcherQueueEventHandler _delegateEnqueuedHandler;

        /// <summary>
        /// Cached delegate reference to the error callback.
        /// </summary>
        protected readonly GlfwCallbacks.ErrorCallback _errorCallback;

        /// <summary>
        /// Cached delegate reference to the window close callback.
        /// </summary>
        protected readonly GlfwCallbacks.WindowCloseCallback _windowCloseCallback;

        /// <summary>
        /// Cached delegate reference to the window position callback.
        /// </summary>
        protected readonly GlfwCallbacks.WindowRefreshCallback _windowRefreshCallback;

        /// <summary>
        /// Cached delegate reference to the framebuffer size callback.
        /// </summary>
        protected readonly GlfwCallbacks.FramebufferSizeCallback _framebufferSizeCallback;

        /// <summary>
        /// Cached delegate reference to the window size callback.
        /// </summary>
        protected readonly GlfwCallbacks.WindowSizeCallback _windowSizeCallback;

        /// <summary>
        /// Cached delegate reference to the window content scale callback.
        /// </summary>
        protected readonly GlfwCallbacks.WindowPosCallback _windowPosCallback;

        /// <summary>
        /// Cached delegate reference to the window maximize callback.
        /// </summary>
        protected readonly GlfwCallbacks.WindowMaximizeCallback _windowMaximizeCallback;

        /// <summary>
        /// Cached delegate reference to the window minimize (iconify) callback.
        /// </summary>
        protected readonly GlfwCallbacks.WindowIconifyCallback _windowIconifyCallback;

        /// <summary>
        /// Cached delegate reference to the window focus callback.
        /// </summary>
        protected readonly GlfwCallbacks.WindowFocusCallback _windowFocusCallback;

        /// <summary>
        /// Cached delegate reference to the monitor callback.
        /// </summary>
        protected readonly GlfwCallbacks.MonitorCallback _monitorCallback;

        /// <summary>
        /// Used to store the previous monitor callback for GLFW, if any.
        /// </summary>
        protected GlfwCallbacks.MonitorCallback? _previousMonitorCallback;

        /// <summary>
        /// Used to store the previous fullscreen mode for the window.
        /// </summary>
        protected WindowFullscreenMode _previousFullscreenMode;

        /// <summary>
        /// Used to store the previous window position for restoring.
        /// </summary>
        protected (double, double) _restorePos;

        /// <summary>
        /// Used to store the previous window size for restoring.
        /// </summary>
        protected (uint, uint) _restoreSize;

        /// <summary>
        /// Used to store the previous window decorated state for restoring.
        /// </summary>
        protected bool _restoreDecorated;

        /// <summary>
        /// Used to cache the current list of known displays.
        /// </summary>
        protected GlfwDisplay[] _cachedDisplays;

        /// <summary>
        /// Used to cache the primary display.
        /// </summary>
        protected GlfwDisplay? _cachedPrimaryDisplay;

        /// <summary>
        /// Used to prevent window refreshing from corrupting window resizing events.
        /// </summary>
        protected volatile bool _pendingResize;

        /// <summary>
        /// The amount of time left waiting for pending resize.
        /// </summary>
        protected volatile int _resizeTimeout;

        /// <summary>
        /// A flag indicating whether the object has been disposed of.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// A lock used to ensure thread-safe access to the object.
        /// </summary>
        private readonly ReaderWriterLockSlim _lock;

        /// <summary>
        /// Constructs a new <see cref="GlfwWindow"/>.
        /// </summary>
        /// <param name="width">The initial width of the window.</param>
        /// <param name="height">The initial height of the window.</param>
        /// <param name="title">The initial title of the window.</param>
        /// <param name="hidden">Indicates whether the window should be initially hidden.</param>
        /// <param name="resizable">Indicates whether the window should be resizable.</param>
        /// <param name="decorated">Indicates whether the window should have decorations.</param>
        /// <param name="fullscreen">The initial fullscreen mode of the window.</param>
        /// <param name="pollRate">The poll rate of the window.</param>
        /// <param name="icons">An array of icons to use for the window.</param>
        /// <param name="initializedHandler">Optional handler to be invoked once the window is initialized, but prior to creation.</param>
        public unsafe GlfwWindow(
            uint width = ICrystalWindow.DEFAULT_WIDTH,
            uint height = ICrystalWindow.DEFAULT_HEIGHT,
            string title = ICrystalWindow.DEFAULT_TITLE,
            bool hidden = false,
            bool resizable = true,
            bool decorated = true,
            WindowFullscreenMode fullscreen = WindowFullscreenMode.Fullscreen,
            ushort pollRate = 0,
            WindowIcon[]? icons = null,
            ICrystalWindow.WindowEventHandler? initializedHandler = null) {
            // Fields
            _nativeHandle = [];
            _pollRate = pollRate;
            _display = null;
            _title = title;
            _x = 0;
            _y = 0;
            _minimumWidth = uint.MinValue;
            _width = width;
            _maximumWidth = uint.MaxValue;
            _minimumHeight = uint.MinValue;
            _height = height;
            _maximumHeight = uint.MaxValue;
            _fullscreenMode = fullscreen;
            _isResizable = resizable;
            _isDecorated = decorated;
            _isFocused = false;
            _isMinimized = false;
            _isMaximized = false;
            _isVisible = !hidden;
            _resetPollEventHandle = new(false);
            _resetPollEventHandleState = false;
            _preExecuteHandler = HandlePreExecute;
            _delegateEnqueuedHandler = HandleDelegateEnqueued;
            _errorCallback = HandleError;
            _windowCloseCallback = HandleWindowClose;
            _windowRefreshCallback = HandleWindowRefresh;
            _framebufferSizeCallback = HandleFramebufferSize;
            _windowSizeCallback = HandleWindowSize;
            _windowPosCallback = HandleWindowPos;
            _windowMaximizeCallback = HandleWindowMaximize;
            _windowIconifyCallback = HandleWindowIconify;
            _windowFocusCallback = HandleWindowFocus;
            _monitorCallback = HandleMonitorCallback;
            _previousMonitorCallback = null;
            _previousFullscreenMode = fullscreen;
            _restorePos = (0, 0);
            _restoreSize = (0, 0);
            _restoreDecorated = decorated;
            _cachedDisplays = [];
            _cachedPrimaryDisplay = null;
            _pendingResize = false;
            _resizeTimeout = 0;
            _disposed = false;
            _lock = new(LockRecursionPolicy.SupportsRecursion);

            // Properties
            Glfw = null!;
            GlfwHandle = 0;

            // Wait for other processes if they are initializing
            bool newMutex;
            using Mutex mutex = new(true, "Global\\CatalystUI_Glfw3_Lock", out newMutex);
            if (!newMutex) {
                if (!mutex.WaitOne(ThreadDelegateDispatcher.LockoutTimeout)) {
                    throw new WindowException("Failed to acquire mutex lock for Glfw3 window initialization.");
                }
            }
            try {
                // Perform window initialization
                if (!ThreadDelegateDispatcher.IsMainThreadCaptured) throw new RequiresMainThreadException(nameof(GlfwWindow), "constructor");
                if (!ThreadDelegateDispatcher.MainThreadDispatcher.Execute(() => {
                    // Get a Glfw3 API instance
                    Glfw3.DebugContext.Log(LogLevel.Verbose, "Requesting new Glfw3 instance for window creation...");
                    Glfw = Glfw3.GetInstance();
                    Glfw glfw = Glfw.Api;
                    glfw.SetErrorCallback(_errorCallback);
                    Glfw3.DebugContext.Log(LogLevel.Verbose, "Done.");

                    // Now assign the cached displays to avoid creating then destroying the API a bunch
                    _cachedDisplays = Glfw3WindowingLayer._cachedFunctionGetDisplaysUnsafe();
                    _cachedPrimaryDisplay = Glfw3WindowingLayer._cachedFunctionGetPrimaryDisplayUnsafe();

                    // Specify no client api
                    glfw.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);
                    Glfw3.DebugContext.Log(LogLevel.Verbose, "Set Glfw3 ClientApi hint to NoApi.");

                    // TODO: Initial visibility on Wayland compat
                    glfw.WindowHint(WindowHintBool.Visible, !hidden);
                    Glfw3.DebugContext.Log(LogLevel.Verbose, $"Set Glfw3 Visible hint to {!hidden}.");

                    // Resizable
                    glfw.WindowHint(WindowHintBool.Resizable, resizable);
                    Glfw3.DebugContext.Log(LogLevel.Verbose, $"Set Glfw3 Resizable hint to {resizable}.");

                    // Decorations
                    glfw.WindowHint(WindowHintBool.Decorated, decorated);
                    Glfw3.DebugContext.Log(LogLevel.Verbose, $"Set Glfw3 Decorated hint to {decorated}.");

                    // Initialized
                    initializedHandler?.Invoke(this);

                    // TODO: Handle decorated and resizable

                    // Create the window
                    Glfw3.DebugContext.Log(LogLevel.Verbose, "Creating Glfw3 window...");
                    WindowHandle* handle = glfw.CreateWindow((int) width, (int) height, title, null, null);
                    GlfwHandle = (nint) handle;
                    Glfw3.DebugContext.Log(LogLevel.Verbose, $"Created Glfw3 window with handle 0x{GlfwHandle:X}");
                    if (handle == null || GlfwHandle == 0) throw new WindowException("Failed to create the window!");

                    // Get the native handle(s)
                    IGlfw3NativeHandler<ISystemLayer<IDomain>>? nativeHandler;
                    try {
                        nativeHandler = ModelRegistry.RequestConnector<IGlfw3NativeHandler<ISystemLayer<IDomain>>>();
                        Glfw3.DebugContext.Log(LogLevel.Verbose, $"Found Glfw3 native handler: {nativeHandler}");
                    } catch {
                        nativeHandler = null;
                        Glfw3.DebugContext.Log(LogLevel.Warning, "No native handler found for Glfw3. Native windowing functionality will be limited, and renderer compatibility may be null.");
                    }
                    if (nativeHandler != null) {
                        _nativeHandle = [
                            nativeHandler.GetNativeHandle(Glfw, handle)
                        ];
                    }

                    // TODO: Wait for Wayland

                    // Attach events
                    glfw.SetWindowCloseCallback(handle, _windowCloseCallback);
                    glfw.SetWindowRefreshCallback(handle, _windowRefreshCallback);
                    glfw.SetFramebufferSizeCallback(handle, _framebufferSizeCallback);
                    glfw.SetWindowSizeCallback(handle, _windowSizeCallback);
                    glfw.SetWindowPosCallback(handle, _windowPosCallback);
                    glfw.SetWindowMaximizeCallback(handle, _windowMaximizeCallback);
                    glfw.SetWindowIconifyCallback(handle, _windowIconifyCallback);
                    glfw.SetWindowFocusCallback(handle, _windowFocusCallback);
                    _previousMonitorCallback = glfw.SetMonitorCallback(_monitorCallback);

                    // Update initial window size limits
                    SetSizeLimits(_minimumWidth, _minimumHeight, _maximumWidth, _maximumHeight);

                    // Update initial fullscreen mode
                    SetFullscreen(_fullscreenMode);

                    // Set window icons
                    if (icons is { Length: > 0 }) SetIcons(icons);

                    // Initial properties refresh
                    RefreshProperties();

                    // Fire created
                    OnCreated();
                    
                    // Attach threading events
                    ThreadDelegateDispatcher.MainThreadDispatcher.PreExecute += _preExecuteHandler;
                    ThreadDelegateDispatcher.MainThreadDispatcher.DelegateEnqueued += _delegateEnqueuedHandler;
                }, wait: true, timeout: Timeout.Infinite)) {
                    throw new WindowException("Failed to initialize the window on the main thread.");
                }
            } finally {
                // Release the mutex
                mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Disposes of the <see cref="GlfwWindow"/>.
        /// </summary>
        ~GlfwWindow() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <inheritdoc/>
        public virtual unsafe void SetPosition(double x, double y) {
            // TODO: Add Wayland linux check
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!ThreadDelegateDispatcher.IsMainThreadCaptured) throw new RequiresMainThreadException(nameof(GlfwWindow), nameof(SetPosition));
            ThreadDelegateDispatcher.MainThreadDispatcher.Execute(() => {
                Glfw.Api.SetWindowPos((WindowHandle*) GlfwHandle, (int) x, (int) y);
                Glfw3.DebugContext.Log(LogLevel.Debug, $"Set position to {x}, {y}");
            });
        }

        /// <inheritdoc/>
        public virtual unsafe void SetSize(uint width, uint height) {
            // TODO: Add Wayland linux check
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!ThreadDelegateDispatcher.IsMainThreadCaptured) throw new RequiresMainThreadException(nameof(GlfwWindow), nameof(SetSize));
            if (width == 0) width = ICrystalWindow.DEFAULT_WIDTH;
            if (height == 0) height = ICrystalWindow.DEFAULT_HEIGHT;
            ThreadDelegateDispatcher.MainThreadDispatcher.Execute(() => {
                Glfw.Api.SetWindowSize((WindowHandle*) GlfwHandle, (int) width, (int) height);
                Glfw3.DebugContext.Log(LogLevel.Debug, $"Set size to {width}x{height}");
            });
        }

        /// <inheritdoc/>
        public virtual unsafe void SetSizeLimits(uint minWidth, uint minHeight, uint maxWidth, uint maxHeight) {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!ThreadDelegateDispatcher.IsMainThreadCaptured) throw new RequiresMainThreadException(nameof(GlfwWindow), nameof(SetSizeLimits));
            if (minWidth == 0) minWidth = uint.MinValue;
            if (minHeight == 0) minHeight = uint.MinValue;
            if (maxWidth == 0) maxWidth = uint.MaxValue;
            if (maxHeight == 0) maxHeight = uint.MaxValue;
            ThreadDelegateDispatcher.MainThreadDispatcher.Execute(() => {
                _minimumWidth = minWidth;
                _minimumHeight = minHeight;
                _maximumWidth = maxWidth;
                _maximumHeight = maxHeight;
                Glfw.Api.SetWindowSizeLimits((WindowHandle*) GlfwHandle,
                    _minimumWidth == uint.MinValue ? Silk.NET.GLFW.Glfw.DontCare : (int) minWidth,
                    _minimumHeight == uint.MinValue ? Silk.NET.GLFW.Glfw.DontCare : (int) minHeight,
                    _maximumWidth == uint.MaxValue ? Silk.NET.GLFW.Glfw.DontCare : (int) maxWidth,
                    _maximumHeight == uint.MaxValue ? Silk.NET.GLFW.Glfw.DontCare : (int) maxHeight
                );
                Glfw3.DebugContext.Log(LogLevel.Debug, $"Set size limits to {(_minimumWidth == uint.MinValue ? "Unlimited" : _minimumWidth)}x{(_minimumHeight == uint.MinValue ? "Unlimited" : _minimumHeight)} - {(_maximumWidth == uint.MaxValue ? "Unlimited" : _maximumWidth)}x{(_maximumHeight == uint.MaxValue ? "Unlimited" : _maximumHeight)}");
            });
        }

        /// <inheritdoc/>
        public virtual unsafe void SetFullscreen(WindowFullscreenMode mode, ICrystalDisplay? display = null, uint width = 0, uint height = 0) {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!ThreadDelegateDispatcher.IsMainThreadCaptured) throw new RequiresMainThreadException(nameof(GlfwWindow), nameof(SetFullscreen));
            if (_fullscreenMode == mode) return;
            ThreadDelegateDispatcher.MainThreadDispatcher.Execute(() => {
                if (display == null) {
                    if (_cachedPrimaryDisplay == null) throw new WindowException("Failed to fetch the primary display!");
                    display = _cachedPrimaryDisplay;
                }

                // Log the windowed position and size
                if (mode != WindowFullscreenMode.Windowed && _previousFullscreenMode == WindowFullscreenMode.Windowed) {
                    _restorePos = (X, Y);
                    _restoreSize = (_width, _height);
                    _restoreDecorated = Glfw.Api.GetWindowAttrib((WindowHandle*) GlfwHandle, WindowAttributeGetter.Decorated);
                }

                // Update the mode
                switch (mode) {
                    case WindowFullscreenMode.Windowed:
                        Glfw.Api.SetWindowMonitor((WindowHandle*) GlfwHandle, null, (int) _restorePos.Item1, (int) _restorePos.Item2, (int) _restoreSize.Item1, (int) _restoreSize.Item2, 0);
                        Glfw.Api.WindowHint(WindowHintBool.Decorated, _restoreDecorated);
                        SetPosition(_restorePos.Item1, _restorePos.Item2);
                        SetSize(width == 0 ? _restoreSize.Item1 : width, height == 0 ? _restoreSize.Item2 : height);
                        break;
                    case WindowFullscreenMode.Borderless:
                        // If we match existing monitor specs, GLFW will use borderless.
                        Glfw.Api.WindowHint(WindowHintBool.Decorated, false);
                        Glfw.Api.SetWindowMonitor((WindowHandle*) GlfwHandle, (Silk.NET.GLFW.Monitor*) ((GlfwDisplay) display).Monitor, 0, 0, (int) display.Width, (int) display.Height, (int) Math.Round(display.RefreshRate));
                        break;
                    case WindowFullscreenMode.Fullscreen:
                        // TODO: Handling custom resolutions? See https://github.com/glfw/glfw/issues/1904
                        Glfw.Api.SetWindowMonitor((WindowHandle*) GlfwHandle, (Silk.NET.GLFW.Monitor*) ((GlfwDisplay) display).Monitor, 0, 0, (int) display.Width, (int) display.Height, (int) Math.Round(display.RefreshRate));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                }

                // Request focus and update the fullscreen mode
                RequestFocus();
                _previousFullscreenMode = _fullscreenMode;
                _fullscreenMode = mode;

                Glfw3.DebugContext.Log(LogLevel.Debug, $"Set fullscreen mode to {mode} on display {display} with resolution {(width == 0 ? _width : width)}x{(height == 0 ? _height : height)}");
            });
        }

        /// <inheritdoc/>
        public virtual unsafe void SetIcons(params WindowIcon[] icons) {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!ThreadDelegateDispatcher.IsMainThreadCaptured) throw new RequiresMainThreadException(nameof(GlfwWindow), nameof(SetIcons));
            if (icons is not { Length: > 0 }) throw new ArgumentException("At least one icon must be provided.", nameof(icons));
            ThreadDelegateDispatcher.MainThreadDispatcher.Execute(() => {
                // Allocate handles and pointers for the icons
                List<GCHandle> iconHandles = new(icons.Length);
                List<nint> iconPointers = new(icons.Length);
                for (int i = 0; i < icons.Length; i++) {
                    WindowIcon icon = icons[i];

                    // Convert the icon to a byte array
                    Vector4<byte>[] pixelData = [.. icon.Pixels];
                    if (pixelData.Length != (icon.Width * icon.Height)) throw new ArgumentException($"The number of pixels does not match the specified width and height for icon index {i}.");

                    // Pin the pixel data in memory
                    GCHandle handle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
                    iconHandles.Add(handle);
                    nint pointer = handle.AddrOfPinnedObject();
                    iconPointers.Add(pointer);
                }

                // Convert the icon to an array of Silk.NET.GLFW.Image
                Image[] images = new Image[icons.Length];
                for (int i = 0; i < icons.Length; i++) {
                    WindowIcon icon = icons[i];
                    images[i] = new() {
                        Width = (int) icon.Width,
                        Height = (int) icon.Height,
                        Pixels = (byte*) iconPointers[i]
                    };
                }

                // Upload the icons to GLFW
                fixed (Image* imagesPtr = images) {
                    Glfw.Api.SetWindowIcon((WindowHandle*) GlfwHandle, icons.Length, imagesPtr);
                }

                // Release the pinned pixel data for each icon
                for (int i = 0; i < iconHandles.Count; i++) {
                    iconHandles[i].Free();
                }

                Glfw3.DebugContext.Log(LogLevel.Debug, $"Set {icons.Length} window icons");
            });
        }

        /// <inheritdoc/>
        public virtual unsafe void RequestFocus() {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!ThreadDelegateDispatcher.IsMainThreadCaptured) throw new RequiresMainThreadException(nameof(GlfwWindow), nameof(RequestFocus));
            ThreadDelegateDispatcher.MainThreadDispatcher.Execute(() => {
                Glfw.Api.FocusWindow((WindowHandle*) GlfwHandle);
                Glfw3.DebugContext.Log(LogLevel.Debug, "Requested focus");
            });
        }

        /// <inheritdoc/>
        public virtual unsafe void RequestAttention() {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!ThreadDelegateDispatcher.IsMainThreadCaptured) throw new RequiresMainThreadException(nameof(GlfwWindow), nameof(RequestAttention));
            ThreadDelegateDispatcher.MainThreadDispatcher.Execute(() => {
                Glfw.Api.RequestWindowAttention((WindowHandle*) GlfwHandle);
                Glfw3.DebugContext.Log(LogLevel.Debug, "Requested attention");
            });
        }

        /// <inheritdoc/>
        public virtual unsafe void Minimize() {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!ThreadDelegateDispatcher.IsMainThreadCaptured) throw new RequiresMainThreadException(nameof(GlfwWindow), nameof(Minimize));
            ThreadDelegateDispatcher.MainThreadDispatcher.Execute(() => {
                Glfw.Api.IconifyWindow((WindowHandle*) GlfwHandle);
                Glfw3.DebugContext.Log(LogLevel.Debug, "Minimized window");
            });
        }

        /// <inheritdoc/>
        public virtual unsafe void Maximize() {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!ThreadDelegateDispatcher.IsMainThreadCaptured) throw new RequiresMainThreadException(nameof(GlfwWindow), nameof(Maximize));
            ThreadDelegateDispatcher.MainThreadDispatcher.Execute(() => {
                Glfw.Api.MaximizeWindow((WindowHandle*) GlfwHandle);
                Glfw3.DebugContext.Log(LogLevel.Debug, "Maximized window");
            });
        }

        /// <inheritdoc/>
        public virtual unsafe void Restore() {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!ThreadDelegateDispatcher.IsMainThreadCaptured) throw new RequiresMainThreadException(nameof(GlfwWindow), nameof(Restore));
            ThreadDelegateDispatcher.MainThreadDispatcher.Execute(() => {
                Glfw.Api.RestoreWindow((WindowHandle*) GlfwHandle);
                OnRestored(); // call manually, Glfw doesn't trigger the callback for some reason
                Glfw3.DebugContext.Log(LogLevel.Debug, "Restored window");
            });
        }

        /// <inheritdoc/>
        public virtual unsafe void Show() {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!ThreadDelegateDispatcher.IsMainThreadCaptured) throw new RequiresMainThreadException(nameof(GlfwWindow), nameof(Show));
            ThreadDelegateDispatcher.MainThreadDispatcher.Execute(() => {
                Glfw.Api.ShowWindow((WindowHandle*) GlfwHandle);
                Glfw3.DebugContext.Log(LogLevel.Debug, "Shown window");
            });
        }

        /// <inheritdoc/>
        public virtual unsafe void Hide() {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!ThreadDelegateDispatcher.IsMainThreadCaptured) throw new RequiresMainThreadException(nameof(GlfwWindow), nameof(Hide));
            ThreadDelegateDispatcher.MainThreadDispatcher.Execute(() => {
                Glfw.Api.HideWindow((WindowHandle*) GlfwHandle);
                Glfw3.DebugContext.Log(LogLevel.Debug, "Hidden window");
            });
        }

        /// <inheritdoc/>
        public virtual unsafe void Close() {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!ThreadDelegateDispatcher.IsMainThreadCaptured) throw new RequiresMainThreadException(nameof(GlfwWindow), nameof(Close));
            ThreadDelegateDispatcher.MainThreadDispatcher.Execute(() => {
                Glfw.Api.SetWindowShouldClose((WindowHandle*) GlfwHandle, true);
                HandleWindowClose((WindowHandle*) GlfwHandle);
                Glfw3.DebugContext.Log(LogLevel.Debug, "Closed window");
            });
        }

        /// <inheritdoc/>
        public virtual unsafe void Exit() {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!ThreadDelegateDispatcher.IsMainThreadCaptured) throw new RequiresMainThreadException(nameof(GlfwWindow), nameof(Close));
            ThreadDelegateDispatcher.MainThreadDispatcher.Execute(() => {
                // Destroy the window
                Glfw.Api.DestroyWindow((WindowHandle*) GlfwHandle);
                GlfwHandle = 0;

                // Debug
                Glfw3.DebugContext.Log(LogLevel.Debug, $"Destroyed window {LogId}");

                // Fire closed
                OnClosed();
            });
        }

        /// <inheritdoc/>
        public virtual void Wait() {
            if (_disposed) return;
            ManualResetEvent reset = new(false);
            Closed += _ => {
                reset.Set();
            };
            reset.WaitOne();
        }

        /// <summary>
        /// Refreshes the underlying variables for the property of this window.
        /// </summary>
        private unsafe void RefreshProperties() {
            // TODO: Linux Wayland check
            // Pull position
            Glfw.Api.GetWindowPos((WindowHandle*) GlfwHandle, out int x, out int y);
            _x = x;
            _y = y;

            // Pull size
            Glfw.Api.GetWindowSize((WindowHandle*) GlfwHandle, out int width, out int height);
            _width = (uint) width;
            _height = (uint) height;

            // Pull state
            _isFocused = Glfw.Api.GetWindowAttrib((WindowHandle*) GlfwHandle, WindowAttributeGetter.Focused);
            _isMinimized = Glfw.Api.GetWindowAttrib((WindowHandle*) GlfwHandle, WindowAttributeGetter.Iconified);
            _isMaximized = Glfw.Api.GetWindowAttrib((WindowHandle*) GlfwHandle, WindowAttributeGetter.Maximized);
            _isVisible = Glfw.Api.GetWindowAttrib((WindowHandle*) GlfwHandle, WindowAttributeGetter.Visible);

            // Refresh the display
            RefreshDisplay();

            Glfw3.DebugContext.Log(LogLevel.Verbose, $"Refreshed on-request properties for {LogId}: Pos({_x}, {_y}), Size({_width}x{_height}), Focused({_isFocused}), Minimized({_isMinimized}), Maximized({_isMaximized}), Visible({_isVisible}), Display({_display})");
            // The rest of internal variables are state-driven and requested on-demand.

            // Fire refresh
            OnRefresh();
        }

        /// <summary>
        /// Refreshes the current display for this window.
        /// </summary>
        private void RefreshDisplay() {
            if (_cachedDisplays.Length == 0) return;

            // Determine which monitor has the most overlap
            double bestOverlap = 0.0f;
            GlfwDisplay? bestDisplay = null;
            for (int i = 0; i < _cachedDisplays.Length; i++) {
                double wx = _x;
                double wy = _y;
                uint ww = _width;
                uint wh = _height;
                double dx = _cachedDisplays[i].X;
                double dy = _cachedDisplays[i].Y;
                uint dw = _cachedDisplays[i].Width;
                uint dh = _cachedDisplays[i].Height;

                // Determine overlap
                double overlap =
                    (uint) Math.Max(0, Math.Min(wx + ww, dx + (int) dw) - Math.Max(wx, dx)) *
                    (double) Math.Max(0, Math.Min(wy + wh, dy + (int) dh) - Math.Max(wy, dy));

                // Determine best overlap
                if (overlap > bestOverlap) {
                    bestOverlap = overlap;
                    bestDisplay = _cachedDisplays[i];
                }
            }

            // Assign the best display
            _display = bestDisplay;
        }
        
        /// <inheritdoc cref="ThreadDelegateDispatcher.OnPreExecute"/>
        protected virtual void HandlePreExecute(ThreadDelegateDispatcher dispatcher) {
            if (PollRate == 0) {
                // Passive polling (only when events are enqueued)
                while (!_disposed && dispatcher.Enqueued == 0) {
                    Glfw.Api.WaitEventsTimeout(MINIMUM_POLL_RATE);
                }
            } else if (PollRate != ushort.MaxValue) {
                // Active polling (on X intervals of time)
                Glfw.Api.PollEvents();
                _resetPollEventHandle.WaitOne(PollRate);
                if (_resetPollEventHandleState) {
                    _resetPollEventHandle.Reset();
                    _resetPollEventHandleState = false;
                }
            } else {
                // Fastest polling (on every loop of the main thread)
                Glfw.Api.PollEvents();
            }
        }
        
        /// <inheritdoc cref="ThreadDelegateDispatcher.DelegateEnqueued"/>
        protected virtual void HandleDelegateEnqueued(ThreadDelegateDispatcher dispatcher, Delegate @delegate) {
            Glfw.Api.PostEmptyEvent(); // un-block the main thread
            _resetPollEventHandle.Set();
            _resetPollEventHandleState = true;
        }

        /// <inheritdoc cref="GlfwCallbacks.ErrorCallback"/>
        protected virtual void HandleError(ErrorCode error, string description) {
            OnErrored(new WindowException($"{error}: {description}"));
        }

        /// <inheritdoc cref="GlfwCallbacks.WindowCloseCallback"/>
        protected virtual unsafe void HandleWindowClose(WindowHandle* handle) {
            if (GlfwHandle != (nint) handle) return;
            bool shouldCancel = false;
            ThreadDelegateDispatcher.MainThreadDispatcher?.Execute(() => {
                shouldCancel = OnClosing();
            }, wait: true);
            if (shouldCancel) {
                Glfw.Api.SetWindowShouldClose(handle, false);
            } else {
                Dispose();
                OnClosed();
            }
        }

        /// <inheritdoc cref="GlfwCallbacks.WindowRefreshCallback"/>
        protected virtual unsafe void HandleWindowRefresh(WindowHandle* handle) {
            if (_pendingResize) return;
            RefreshProperties();
        }

        /// <inheritdoc cref="GlfwCallbacks.FramebufferSizeCallback"/>
        protected virtual unsafe void HandleFramebufferSize(WindowHandle* handle, int width, int height) {
            _resizeTimeout = 500;
            if (!_pendingResize) {
                _pendingResize = true;
                FramebufferResizeStabilizer();
            }
            RefreshProperties();
            OnRedraw();
        }

        /// <summary>
        /// Delays the frame-buffer size callback to allow for the window to stabilize.
        /// </summary>
        protected virtual void FramebufferResizeStabilizer() {
            // Delay resetting the value to allow for the window to stabilize
            // Thanks GLFW for the confusing results of refreshing and resizing :)
            new Thread(() => {
                while (_resizeTimeout > 0) {
                    Thread.Sleep(50);
                    int newTimeout = _resizeTimeout - 50;
                    Interlocked.Exchange(ref _resizeTimeout, newTimeout);
                }
                _pendingResize = false;
            }).Start();
        }

        /// <inheritdoc cref="GlfwCallbacks.WindowSizeCallback"/>
        protected virtual unsafe void HandleWindowSize(WindowHandle* handle, int width, int height) {
            OnResized();
        }

        /// <inheritdoc cref="GlfwCallbacks.WindowPosCallback"/>
        protected virtual unsafe void HandleWindowPos(WindowHandle* handle, int x, int y) {
            RefreshProperties();
            OnRepositioned();
        }

        /// <inheritdoc cref="GlfwCallbacks.WindowMaximizeCallback"/>
        protected virtual unsafe void HandleWindowMaximize(WindowHandle* handle, bool maximized) {
            RefreshProperties();
            if (maximized) OnMaximized();
        }

        /// <inheritdoc cref="GlfwCallbacks.WindowIconifyCallback"/>
        protected virtual unsafe void HandleWindowIconify(WindowHandle* handle, bool iconified) {
            RefreshProperties();
            if (iconified) OnMinimized();
        }

        /// <inheritdoc cref="GlfwCallbacks.WindowFocusCallback"/>
        protected virtual unsafe void HandleWindowFocus(WindowHandle* handle, bool focused) {
            RefreshProperties();
            if (focused) OnFocused();
            else OnUnfocused();
        }

        /// <inheritdoc cref="GlfwCallbacks.MonitorCallback"/>
        protected virtual unsafe void HandleMonitorCallback(Silk.NET.GLFW.Monitor* monitor, ConnectedState state) {
            // Propagate the event
            _previousMonitorCallback?.Invoke(monitor, state);

            // Update the cached displays
            _cachedDisplays = Glfw3WindowingLayer._cachedFunctionGetDisplaysUnsafe();
            _cachedPrimaryDisplay = Glfw3WindowingLayer._cachedFunctionGetPrimaryDisplayUnsafe();
        }

        /// <inheritdoc cref="Errored"/>
        protected virtual void OnErrored(WindowException exception) {
            Glfw3.DebugContext.Log(LogLevel.Error, $"Exception occurred in {LogId}", args: exception);
            Errored?.Invoke(this, exception);
        }

        /// <inheritdoc cref="Created"/>
        protected virtual void OnCreated() {
            Glfw3.DebugContext.Log(LogLevel.Info, $"Created new {LogId}");
            Created?.Invoke(this);
        }

        /// <inheritdoc cref="Repositioned"/>
        protected virtual void OnRepositioned() {
            Glfw3.DebugContext.Log(LogLevel.Verbose, $"Repositioned {LogId}");
            Repositioned?.Invoke(this);
        }

        /// <inheritdoc cref="Resized"/>
        protected virtual void OnResized() {
            Glfw3.DebugContext.Log(LogLevel.Verbose, $"Resized {LogId}");
            Resized?.Invoke(this);
        }

        /// <inheritdoc cref="Refresh"/>
        protected virtual void OnRefresh() {
            Refresh?.Invoke(this);
        }

        /// <inheritdoc cref="Redraw"/>
        protected virtual void OnRedraw() {
            Redraw?.Invoke(this);
        }

        /// <inheritdoc cref="Focused"/>
        protected virtual void OnFocused() {
            Glfw3.DebugContext.Log(LogLevel.Debug, $"Focused {LogId}");
            Focused?.Invoke(this);
        }

        /// <inheritdoc cref="Unfocused"/>
        protected virtual void OnUnfocused() {
            Glfw3.DebugContext.Log(LogLevel.Debug, $"Unfocused {LogId}");
            Unfocused?.Invoke(this);
        }

        /// <inheritdoc cref="Minimized"/>
        protected virtual void OnMinimized() {
            Glfw3.DebugContext.Log(LogLevel.Debug, $"Minimized {LogId}");
            Minimized?.Invoke(this);
        }

        /// <inheritdoc cref="Maximized"/>
        protected virtual void OnMaximized() {
            Glfw3.DebugContext.Log(LogLevel.Debug, $"Maximized {LogId}");
            Maximized?.Invoke(this);
        }

        /// <inheritdoc cref="Restored"/>
        protected virtual void OnRestored() {
            Glfw3.DebugContext.Log(LogLevel.Debug, $"Restored {LogId}");
            Restored?.Invoke(this);
        }

        /// <inheritdoc cref="Shown"/>
        protected virtual void OnShown() {
            Glfw3.DebugContext.Log(LogLevel.Debug, $"Shown {LogId}");
            Shown?.Invoke(this);
        }

        /// <inheritdoc cref="Hidden"/>
        protected virtual void OnHidden() {
            Glfw3.DebugContext.Log(LogLevel.Debug, $"Hidden {LogId}");
            Hidden?.Invoke(this);
        }

        /// <inheritdoc cref="Closing"/>
        protected virtual bool OnClosing() {
            Glfw3.DebugContext.Log(LogLevel.Debug, $"Closing {LogId}");
            Delegate[] delegates = Closing?.GetInvocationList() ?? [];
            bool result = delegates.Cast<ICrystalWindow.WindowClosingEventHandler>().Any(handler => handler(this));
            Glfw3.DebugContext.Log(LogLevel.Debug, result ? $"Closure of {LogId} cancelled by event handler" : $"Closure of {LogId} proceeding");
            return result;
        }

        /// <inheritdoc cref="Closed"/>
        protected virtual void OnClosed() {
            Glfw3.DebugContext.Log(LogLevel.Info, $"Closed {LogId}");
            Closed?.Invoke(this);
        }

        /// <summary>
        /// Disposes of the <see cref="GlfwWindow"/>.
        /// </summary>
        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <param name="disposing"><see langword="false"/> if disposal is being performed by the garbage collector, otherwise <see langword="true"/></param>
        /// <inheritdoc cref="Dispose()"/>
        private void Dispose(bool disposing) {
            _lock.EnterWriteLock();
            try {
                if (_disposed) return;

                // Dispose managed state (managed objects)
                if (disposing) {
                    // Detach threading events
                    ThreadDelegateDispatcher? dispatcher = ThreadDelegateDispatcher.MainThreadDispatcher;
                    if (dispatcher != null) {
                        dispatcher.PreExecute -= _preExecuteHandler;
                        dispatcher.DelegateEnqueued -= _delegateEnqueuedHandler;
                    }
                    
                    // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                    Glfw?.Api.PostEmptyEvent(); // always post to unblock the main thread
                    Glfw?.Dispose(); // dispose of our requested api instance
                }

                // Dispose unmanaged state (unmanaged objects)
                // ...

                // Indicate disposal completion
                _disposed = true;
            } finally {
                _lock.ExitWriteLock();
            }
        }

    }

}