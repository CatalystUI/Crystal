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

using Catalyst.Mathematics;
using System.Diagnostics.CodeAnalysis;

namespace Crystal.Windowing {

    /// <summary>
    /// Represents a window icon in the CatalystUI framework.
    /// </summary>
    public readonly record struct WindowIcon {

        /// <summary>
        /// Gets the width of the icon in pixels.
        /// </summary>
        /// <value>The width of the icon.</value>
        public required uint Width { get; init; }

        /// <summary>
        /// Gets the height of the icon in pixels.
        /// </summary>
        /// <value>The height of the icon.</value>
        public required uint Height { get; init; }

        /// <summary>
        /// Gets a read-only collection of pixel data for the icon.
        /// </summary>
        /// <remarks>
        /// The pixel data is expected to be a list of
        /// <see cref="Vector4{T}"/> values where each
        /// vector represents a pixel's RGBA color.
        /// </remarks>
        /// <value>A collection of pixel data represented as <see cref="Vector4{T}"/> values.</value>
        public required IReadOnlyCollection<Vector4<byte>> Pixels { get; init; }

        /// <summary>
        /// Constructs a new <see cref="WindowIcon"/>.
        /// </summary>
        /// <param name="width">The width of the icon in pixels.</param>
        /// <param name="height">The height of the icon in pixels.</param>
        /// <param name="pixels">A read-only collection of pixel data for the icon.</param>
        [SetsRequiredMembers]
        public WindowIcon(uint width, uint height, IReadOnlyCollection<Vector4<byte>> pixels) {
            Width = width;
            Height = height;
            Pixels = pixels ?? throw new ArgumentNullException(nameof(pixels), "Pixel data cannot be null.");
        }

        /// <summary>
        /// Creates an array of icon instances from the application manifest resources.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The resulting assembly path will be in the format: <c>{location}.{filename}</c>,
        /// and as such the provided filename must contain at least one <c>%</c> placeholder
        /// for the size of the icon and include the associated file extension (e.g., ".bmp", ".png").
        /// </para>
        /// <para>
        /// For example, if icons were being searched for in the CatalystUI.Examples.BasicWindow
        /// project, the provided location might be <i>Catalyst.Examples.BasicWindow.Resources.Icons</i>,
        /// and the provided filename might be <i>icon_%dx%d.bmp</i>. This would result in the
        /// method searching for a resource named <i>Catalyst.Examples.BasicWindow.Resources.Icons.icon_16x16.bmp</i>.
        /// </para>
        /// <para>
        /// The provided icon function will be called for each icon resource found,
        /// and the associated resources' stream will be passed to it. This
        /// provides flexibility in how the icon data is processed, such as
        /// utilizing the Catalyst Arcane library to decode BMP files,
        /// or using other image processing libraries to decode special
        /// file types such as JPEG or WEBP icons.
        /// </para>
        /// </remarks>
        /// <param name="location">The location of the application manifest.</param>
        /// <param name="filename">The filename template for the icon resources, which must contain a "%" placeholder.</param>
        /// <param name="sizes">An array of sizes for the icons to be created.</param>
        /// <param name="iconFunc">A function that takes a <see cref="Stream"/> and returns a <see cref="WindowIcon"/> containing the icon data.</param>
        /// <returns>An array of <see cref="WindowIcon"/> instances created from the specified resources.</returns>
        /// <typeparam name="T">The type of the assembly containing the application manifest resources.</typeparam>
        public static WindowIcon[] FromApplicationManifest<T>(string location, string filename, int[] sizes, Func<Stream, WindowIcon> iconFunc) {
            string template = $"{location}.{filename}";
            WindowIcon[] icons = new WindowIcon[sizes.Length];
            for (int i = 0; i < sizes.Length; i++) {
                int size = sizes[i];
                string resourceName = template.Replace("%", size.ToString());
                Stream? stream = typeof(T).Assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException($"Resource not found: {resourceName}");
                icons[i] = iconFunc(stream);
            }
            return icons;
        }

    }

}