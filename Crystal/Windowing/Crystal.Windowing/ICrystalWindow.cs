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
    /// Represents a logical device which is a subsection of a display
    /// by providing a bounded area for receiving user-input and providing
    /// user-output as determined by the device's system.
    /// </summary>
    /// <remarks>
    /// A window as provided by Crystal represents a <i>visual</i>
    /// form of interaction with the user's system. This differs
    /// from the CatalystUI definition which is a more abstract
    /// and represents <i>any</i> view into the user's system.
    /// Crystal's windowing system is designed to cover the
    /// most common use-cases for windowing across platforms,
    /// but may not cover every edge-case or platform-specific
    /// feature. For more advanced or specialized windowing needs,
    /// consider using platform-specific APIs, libraries, or
    /// providing custom <see cref="Catalyst.Layers.IWindowLayer{TDomain}"/>
    /// implementations.
    /// </remarks>
    public interface ICrystalWindow : IDisposable {
        
        /// <summary>
        /// The default value for <see cref="Width"/>.
        /// </summary>
        public const uint DEFAULT_WIDTH = 800;
        
        /// <summary>
        /// The default value for <see cref="Height"/>.
        /// </summary>
        public const uint DEFAULT_HEIGHT = 450;
        
        /// <summary>
        /// The default value for <see cref="Title"/>.
        /// </summary>
        public const string DEFAULT_TITLE = "Crystal Window";
        
        /// <summary>
        /// Delegate for <see cref="ICrystalWindow"/> events that don't pass any event arguments.
        /// </summary>
        /// <param name="window">The window that raised the event.</param>
        public delegate void WindowEventHandler(ICrystalWindow window);
        
        /// <summary>
        /// Delegate for the <see cref="Errored"/> event.
        /// </summary>
        /// <param name="window">The window that raised the event.</param>
        /// <param name="exception">The exception that occurred.</param>
        public delegate void WindowErroredEventHandler(ICrystalWindow window, WindowException exception);
        
        /// <summary>
        /// Delegate for the <see cref="Closing"/> event.
        /// </summary>
        /// <param name="window">The window that raised the event.</param>
        /// <returns><see langword="true"/> to cancel the close operation; otherwise, <see langword="false"/>.</returns>
        public delegate bool WindowClosingEventHandler(ICrystalWindow window);
        
        /// <summary>
        /// Occurs when an exception is thrown during the window's execution.
        /// </summary>
        /// <remarks>
        /// Raised when an unhandled or internal exception occurs within the window’s lifecycle or processing pipeline.
        /// This event allows listeners to log diagnostics, handle recovery, or terminate gracefully.
        /// </remarks>
        public event WindowErroredEventHandler? Errored;
        
        ///// <summary>
        ///// Delegate for the <see cref="Interacted"/> event.
        ///// </summary>
        ///// <param name="window">The window that raised the event.</param>
        ///// <param name="interaction">The interaction that triggered the event.</param>
        // TODO: Re-Implement
        //public delegate void WindowInteractedEventHandler(ICrystalWindow window, IInteraction interaction);

        /// <summary>
        /// Occurs when the window has been created, before the first event is processed.
        /// </summary>
        /// <remarks>
        /// Raised once the window is created prior to the
        /// first event being processed to allow for additional
        /// initialization or setup.
        /// </remarks>
        public event WindowEventHandler? Created;

        /// <summary>
        /// Occurs when the window's position on the display has changed.
        /// </summary>
        /// <remarks>
        /// Raised when the window is moved by the user or system.
        /// </remarks>
        public event WindowEventHandler? Repositioned;

        /// <summary>
        /// Occurs when the window's size has changed.
        /// </summary>
        /// <remarks>
        /// Raised when the window is resized by the user or system.
        /// </remarks>
        public event WindowEventHandler? Resized;

        /// <summary>
        /// Occurs when the window's state has changed and may require an update.
        /// </summary>
        /// <remarks>
        /// Raised when one or more properties of the window, such as size,
        /// visibility, focus, or decorations, have changed in a way that
        /// could affect rendering or layout. Unlike <see cref="Redraw"/>,
        /// this event does not imply that the window's surface is invalid.
        /// </remarks>
        public event WindowEventHandler? Refresh;

        /// <summary>
        /// Occurs when the window's surface requires a full redraw.
        /// </summary>
        /// <remarks>
        /// Raised when the underlying frame-buffer is no longer valid
        /// and must be re-rendered. This typically occurs after display changes,
        /// window occlusion, or other external interference.
        /// </remarks>
        public event WindowEventHandler? Redraw;

        /// <summary>
        /// Occurs when the window gains input focus.
        /// </summary>
        /// <remarks>
        /// Raised when the window becomes the active target for user input.
        /// </remarks>
        public event WindowEventHandler? Focused;

        /// <summary>
        /// Occurs when the window loses input focus.
        /// </summary>
        /// <remarks>
        /// Raised when the window is no longer the active target for user input.
        /// </remarks>
        public event WindowEventHandler? Unfocused;

        /// <summary>
        /// Occurs when the window is minimized to the taskbar, dock, or a similar system feature.
        /// </summary>
        /// <remarks>
        /// Raised in response to user action or system behavior.
        /// </remarks>
        public event WindowEventHandler? Minimized;

        /// <summary>
        /// Occurs when the window is maximized to fill the available display area.
        /// </summary>
        /// <remarks>
        /// Raised in response to user action or system behavior.
        /// </remarks>
        public event WindowEventHandler? Maximized;

        /// <summary>
        /// Occurs when the window has been restored from a minimized or maximized state.
        /// </summary>
        /// <remarks>
        /// Raised after returning to the window's previous state.
        /// </remarks>
        public event WindowEventHandler? Restored;

        /// <summary>
        /// Occurs when the window becomes visible on the display.
        /// </summary>
        /// <remarks>
        /// Raised after the window is shown and is ready for user interaction.
        /// </remarks>
        public event WindowEventHandler? Shown;

        /// <summary>
        /// Occurs when the window is hidden from the display.
        /// </summary>
        /// <remarks>
        /// Raised after the window becomes invisible due to user action or system behavior.
        /// </remarks>
        public event WindowEventHandler? Hidden;

        /// <summary>
        /// Occurs when the system or user has requested the window to close.
        /// </summary>
        /// <remarks>
        /// Raised when the user or system attempts to close the window.
        /// Can be used to perform cleanup or to cancel the close operation.
        /// Canceling may not be supported on all platforms, and some close
        /// requests may be ignored due to certain closing conditions.
        /// </remarks>
        public event WindowClosingEventHandler? Closing;

        /// <summary>
        /// Occurs when the window has been closed and all resources have been released.
        /// </summary>
        /// <remarks>
        /// Raised after the final shutdown of the window and no further interaction is possible.
        /// </remarks>
        public event WindowEventHandler? Closed;
        
        /// <summary>
        /// Gets the system's native handle for the window.
        /// </summary>
        /// <remarks>
        /// The handle's format depends on the underlying platform (e.g., <c>HWND</c> on Windows).
        /// Depending on the platform, more than one handle type may be available
        /// for things such as the display server or graphics context.
        /// The 0th index is always expected to be the primary handle for the window.
        /// </remarks>
        public nint[] NativeHandle { get; }

        /// <summary>
        /// Gets or sets the rate at which the window should be polled for native events.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The polling rate can be passive or active depending on the value. Passive
        /// polling will use reactive event handling to respond to events, whereas active
        /// polling will continuously check for events at the specified rate in milliseconds (ms).
        /// </para>
        /// <para>
        /// Since some platforms require windowing operations to be performed on
        /// the main thread, setting a high polling rate may lead to increased CPU usage
        /// and reduced application performance, since polling frequency may interfere
        /// with other main-thread operations. It is recommended to use passive polling
        /// unless a specific use-case requires active polling, such as real-time applications
        /// or video games.
        /// </para>
        /// </remarks>
        /// <value>
        /// <list type="bullet">
        ///     <item><c>0</c> represents passive polling.</item>
        ///     <item><c>>=1</c> represents the rate at which the window will be polled in milliseconds (ms).</item>
        ///     <item><c><see cref="ushort.MaxValue"/></c> represents an unlimited polling rate.</item>
        /// </list>
        /// </value>
        public ushort PollRate { get; set; }

        /// <summary>
        /// Gets the display on which the window is currently shown.
        /// </summary>
        /// <value>The display, or <c>null</c> if the display could not be determined.</value>
        /// <seealso cref="ICrystalDisplay"/>
        public ICrystalDisplay? Display { get; }

        /// <summary>
        /// Gets or sets the text displayed in the window's title bar.
        /// </summary>
        /// <remarks>
        /// The title typically appears in the window's title bar and
        /// can be used to indicate the current context, content, or
        /// purpose of the window. Not all platforms or window styles
        /// display or support a title bar.
        /// </remarks>
        /// <value>The window title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the horizontal position of the window relative to the current display.
        /// </summary>
        /// <remarks>
        /// The position is relative to the top-left origin of the primary display.
        /// Units are implementation-specific and may vary depending on the platform
        /// scaling or DPI settings, but generally represents pixels or screen units.
        /// </remarks>
        /// <value>The horizontal position.</value>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the vertical position of the window relative to the current display.
        /// </summary>
        /// <remarks>
        /// The position is relative to the top-left origin of the primary display.
        /// Units are implementation-specific and may vary depending on the platform
        /// scaling or DPI settings, but generally represents pixels or screen units.
        /// </remarks>
        /// <value>The vertical position.</value>
        public double Y { get; set; }

        /// <summary>
        /// Gets or sets the minimum allowable width of the window.
        /// </summary>
        /// <remarks>
        /// The minimum width defines the smallest size the window can be resized to.
        /// Units are implementation-specific and may vary depending on the platform
        /// scaling or DPI settings, but generally represents pixels or screen units.
        /// </remarks>
        /// <value>The minimum width.</value>
        public uint MinimumWidth { get; set; }

        /// <summary>
        /// Gets or sets the current width of the window.
        /// </summary>
        /// <remarks>
        /// Units are implementation-specific and may vary depending on the platform
        /// scaling or DPI settings, but generally represents pixels or screen units.
        /// </remarks>
        /// <value>The width.</value>
        public uint Width { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowable width of the window.
        /// </summary>
        /// <remarks>
        /// Units are implementation-specific and may vary depending on the platform
        /// scaling or DPI settings, but generally represents pixels or screen units.
        /// </remarks>
        /// <value>The maximum width.</value>
        public uint MaximumWidth { get; set; }

        /// <summary>
        /// Gets or sets the minimum allowable height of the window.
        /// </summary>
        /// <remarks>
        /// Units are implementation-specific and may vary depending on the platform
        /// scaling or DPI settings, but generally represents pixels or screen units.
        /// </remarks>
        /// <value>The minimum height.</value>
        public uint MinimumHeight { get; set; }

        /// <summary>
        /// Gets or sets the current height of the window.
        /// </summary>
        /// <remarks>
        /// Units are implementation-specific and may vary depending on the platform
        /// scaling or DPI settings, but generally represents pixels or screen units.
        /// </remarks>
        /// <value>The current height.</value>
        public uint Height { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowable height of the window.
        /// </summary>
        /// <remarks>
        /// Units are implementation-specific and may vary depending on the platform
        /// scaling or DPI settings, but generally represents pixels or screen units.
        /// </remarks>
        /// <value>The maximum height.</value>
        public uint MaximumHeight { get; set; }

        /// <summary>
        /// Gets or sets the current fullscreen mode of the window.
        /// </summary>
        /// <remarks>
        /// The fullscreen mode determines how the window is displayed on the screen.
        /// </remarks>
        /// <value>The current fullscreen mode.</value>
        public WindowFullscreenMode FullscreenMode { get; set; }

        /// <summary>
        /// Gets a value indicating whether the window can be resized by the user.
        /// </summary>
        /// <value><see langword="true"/> if the window is resizable; otherwise, <see langword="false"/>.</value>
        public bool IsResizable { get; }
        
        /// <summary>
        /// Gets a value indicating whether the window has standard decorations such as borders and title bar.
        /// </summary>
        /// <value><see langword="true"/> if the window is decorated; otherwise, <see langword="false"/>.</value>
        public bool IsDecorated { get; }
        
        /// <summary>
        /// Gets a value indicating whether the window currently has input focus.
        /// </summary>
        /// <value><see langword="true"/> if the window is focused; otherwise, <see langword="false"/>.</value>
        public bool IsFocused { get; }

        /// <summary>
        /// Gets a value indicating whether the window currently does not have input focus.
        /// </summary>
        /// <value><see langword="true"/> if the window is unfocused; otherwise, <see langword="false"/>.</value>
        public bool IsUnfocused { get; }

        /// <summary>
        /// Gets a value indicating whether the window is currently minimized.
        /// </summary>
        /// <value><see langword="true"/> if the window is minimized; otherwise, <see langword="false"/>.</value>
        public bool IsMinimized { get; }

        /// <summary>
        /// Gets a value indicating whether the window is currently maximized.
        /// </summary>
        /// <value><see langword="true"/> if the window is maximized; otherwise, <see langword="false"/>.</value>
        public bool IsMaximized { get; }

        /// <summary>
        /// Gets a value indicating whether the window is currently visible on the display.
        /// </summary>
        /// <value><see langword="true"/> if the window is visible; otherwise, <see langword="false"/>.</value>
        public bool IsVisible { get; }

        /// <summary>
        /// Gets a value indicating whether the window is currently hidden from the display.
        /// </summary>
        /// <value><see langword="true"/> if the window is hidden; otherwise, <see langword="false"/>.</value>
        public bool IsHidden { get; }
        
        /// <summary>
        /// Gets a value indicating whether the window has been closed and is no longer operational.
        /// </summary>
        /// <value><see langword="true"/> if the window is closed; otherwise, <see langword="false"/>.</value>
        public bool IsClosed { get; }
        
        /// <summary>
        /// Sets the window's position relative to the top-left corner of the primary display.
        /// </summary>
        /// <param name="x">The desired horizontal position of the window.</param>
        /// <param name="y">The desired vertical position of the window.</param>
        public void SetPosition(double x, double y);

        /// <summary>
        /// Sets the window's size.
        /// </summary>
        /// <param name="width">The desired width of the window.</param>
        /// <param name="height">The desired height of the window.</param>
        public void SetSize(uint width, uint height);

        /// <summary>
        /// Sets the window's size limits.
        /// </summary>
        /// <param name="minWidth">The desired minimum width of the window.</param>
        /// <param name="minHeight">The desired minimum height of the window.</param>
        /// <param name="maxWidth">The desired maximum width of the window.</param>
        /// <param name="maxHeight">The desired maximum height of the window.</param>
        public void SetSizeLimits(uint minWidth, uint minHeight, uint maxWidth, uint maxHeight);

        /// <summary>
        /// Sets the window's fullscreen mode.
        /// </summary>
        /// <param name="mode">The desired fullscreen mode.</param>
        /// <param name="display">Optional display to use for fullscreen mode. If <c>null</c>, the current display will be used.</param>
        /// <param name="width">Optional override width for the fullscreen resolution. Use <c>0</c> to use default value.</param>
        /// <param name="height">Optional override height for the fullscreen resolution. Use <c>0</c> to use the default value.</param>
        public void SetFullscreen(WindowFullscreenMode mode, ICrystalDisplay? display = null, uint width = 0, uint height = 0);

        /// <summary>
        /// Provides the window with a set of available icons to be displayed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the provided pixel data does not match the specified
        /// width and height, an exception will be thrown.
        /// </para>
        /// </remarks>
        /// <param name="icons">An array of <see cref="WindowIcon"/> instances representing the icon data.</param>
        public void SetIcons(params WindowIcon[] icons);

        /// <summary>
        /// Requests the window to be the active target for user input.
        /// </summary>
        public void RequestFocus();

        /// <summary>
        /// Requests the user's attention to the window (e.g., flashing taskbar).
        /// </summary>
        /// <remarks>
        /// Not all platforms will support this feature.
        /// </remarks>
        public void RequestAttention();

        /// <summary>
        /// Minimizes the window.
        /// </summary>
        public void Minimize();

        /// <summary>
        /// Maximizes the window to fill the available display space.
        /// </summary>
        public void Maximize();

        /// <summary>
        /// Restores the window to its previous state after being minimized or maximized.
        /// </summary>
        public void Restore();

        /// <summary>
        /// Shows the window on the display.
        /// </summary>
        public void Show();

        /// <summary>
        /// Hides the window from the display.
        /// </summary>
        public void Hide();

        /// <summary>
        /// Closes the window and begins releasing all associated resources.
        /// </summary>
        /// <remarks>
        /// Triggers the <see cref="Closing"/> event and,
        /// depending on the platform, may be canceled.
        /// </remarks>
        /// <seealso cref="Exit"/>
        public void Close();
        
        /// <summary>
        /// Forcefully exits the window's event loop and begins shutdown,
        /// bypassing the <see cref="Closing"/> event.
        /// </summary>
        /// <remarks>
        /// In almost all scenarios, it is preferable to
        /// use the <see cref="Close"/> method instead.
        /// </remarks>
        /// <seealso cref="Close"/>
        public void Exit();

        /// <summary>
        /// Waits for the window to finish processing
        /// its native event loop.
        /// </summary>
        /// <remarks>
        /// Blocks the calling thread until the window is closed.
        /// </remarks>
        public void Wait();
        
    }
    
}