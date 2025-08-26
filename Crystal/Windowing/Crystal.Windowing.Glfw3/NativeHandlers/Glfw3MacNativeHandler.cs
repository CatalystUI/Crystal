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

using Catalyst.Native;
using Catalyst.Supplementary.Model.Systems;
using System.Runtime.InteropServices;
using System.Text;
using Monitor = Silk.NET.GLFW.Monitor;

namespace Crystal.Windowing.Glfw3.NativeHandlers {
    
    /// <summary>
    /// An Apple Mac-based implementation of <see cref="IGlfw3NativeHandler{TLayerLow}"/>
    /// </summary>
    public sealed unsafe class Glfw3MacNativeHandler : IGlfw3NativeHandler<IMacSystemLayer> {
        
        /// <inheritdoc/>
        public double GetDisplayRotation(Glfw3 glfw, Monitor* pMonitor) {
            int displayId = MacGlfwImports.glfwGetCocoaMonitor(glfw, pMonitor);
            return MacImports.GetDisplayRotation(displayId);
        }
        
        /// <inheritdoc/>
        public string GetDisplayDescriptor(Glfw3 glfw, Monitor* pMonitor) {
            int displayId = MacGlfwImports.glfwGetCocoaMonitor(glfw, pMonitor);
            byte[] edid = MacImports.GetDisplayEDID(displayId);
            string descriptor = EdidHelper.GetMonitorDescriptorFromEdid(ref edid);
            if (string.IsNullOrEmpty(descriptor)) throw new NativeException("Failed to retrieve monitor descriptor from EDID.");
            return descriptor;
        }
        
        /// <inheritdoc/>
        public string GetDisplayManufacturer(Glfw3 glfw, Monitor* pMonitor) {
            int displayId = MacGlfwImports.glfwGetCocoaMonitor(glfw, pMonitor);
            byte[] edid = MacImports.GetDisplayEDID(displayId);
            string manufacturer = EdidHelper.GetMonitorManufacturerCodeFromEdid(ref edid);
            if (string.IsNullOrEmpty(manufacturer)) throw new NativeException("Failed to retrieve monitor manufacturer from EDID.");
            return manufacturer;
        }
        
    }
    
    // ReSharper disable InconsistentNaming
    internal static unsafe class MacGlfwImports {
        
        /// <summary>
        /// Returns the MacOS displayID for the specified monitor.
        /// </summary>
        /// <param name="pMonitor">The monitor to get the screen for.</param>
        /// <returns>The display ID of the monitor.</returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetCocoaMonitor(Monitor* pMonitor);
        
        private static GetCocoaMonitor? _glfwGetCocoaMonitor = null;
        
        public static int glfwGetCocoaMonitor(Glfw3 glfw, Monitor* pMonitor) {
            if (_glfwGetCocoaMonitor == null) {
                if (glfw.Api.Context.TryGetProcAddress("glfwGetCocoaMonitor", out nint procAddress)) {
                    _glfwGetCocoaMonitor = Marshal.GetDelegateForFunctionPointer<GetCocoaMonitor>(procAddress);
                } else {
                    throw new NativeException("glfwGetCocoaMonitor is not supported on this platform.");
                }
            }
            return _glfwGetCocoaMonitor(pMonitor);
        }
        
    }
    // ReSharper restore InconsistentNaming
    
    // ReSharper disable InconsistentNaming
    internal static unsafe class MacImports {
        
        private static readonly nint _CoreGraphics;
        private static readonly nint _IOKit;
        private static readonly nint _CoreFoundation;
        
        private static readonly delegate* unmanaged[Cdecl]<int, double> _CGDisplayRotation;
        private static readonly delegate* unmanaged[Cdecl]<int, int> _CGDisplayVendorNumber;
        private static readonly delegate* unmanaged[Cdecl]<int, int> _CGDisplayModelNumber;
        
        private static readonly delegate* unmanaged[Cdecl]<nint, nint, nint*, nint> _IOServiceGetMatchingServices;
        private static readonly delegate* unmanaged[Cdecl]<sbyte*, nint> _IOServiceMatching;
        private static readonly delegate* unmanaged[Cdecl]<nint, nint> _IOIteratorNext;
        private static readonly delegate* unmanaged[Cdecl]<nint, void> _IOObjectRelease;
        private static readonly delegate* unmanaged[Cdecl]<nint, nint, nint, uint, nint> _IORegistryEntryCreateCFProperty;
        
        private static readonly delegate* unmanaged[Cdecl]<nint, sbyte*, uint, nint> _CFStringCreateWithCString;
        private static readonly delegate* unmanaged[Cdecl]<nint, int> _CFDataGetLength;
        private static readonly delegate* unmanaged[Cdecl]<nint, CFRange, byte*, void> _CFDataGetBytes;
        private static readonly delegate* unmanaged[Cdecl]<nint, int, int*, int> _CFNumberGetValue;
        private static readonly delegate* unmanaged[Cdecl]<nint, void> _CFRelease;
        
        
        static MacImports() {
            _CoreGraphics = NativeLibrary.Load("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics");
            _IOKit = NativeLibrary.Load("/System/Library/Frameworks/IOKit.framework/IOKit");
            _CoreFoundation= NativeLibrary.Load("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation");

            _CGDisplayRotation = (delegate* unmanaged[Cdecl]<int, double>)
                NativeLibrary.GetExport(_CoreGraphics, "CGDisplayRotation");
            _CGDisplayVendorNumber = (delegate* unmanaged[Cdecl]<int, int>)
                NativeLibrary.GetExport(_CoreGraphics, "CGDisplayVendorNumber");
            _CGDisplayModelNumber = (delegate* unmanaged[Cdecl]<int, int>)
                NativeLibrary.GetExport(_CoreGraphics, "CGDisplayModelNumber");

            _IOServiceGetMatchingServices = (delegate* unmanaged[Cdecl]<nint, nint, nint*, nint>)
                NativeLibrary.GetExport(_IOKit, "IOServiceGetMatchingServices");
            _IOServiceMatching = (delegate* unmanaged[Cdecl]<sbyte*, nint>)
                NativeLibrary.GetExport(_IOKit, "IOServiceMatching");
            _IOIteratorNext = (delegate* unmanaged[Cdecl]<nint, nint>)
                NativeLibrary.GetExport(_IOKit, "IOIteratorNext");
            _IOObjectRelease = (delegate* unmanaged[Cdecl]<nint, void>)
                NativeLibrary.GetExport(_IOKit, "IOObjectRelease");
            _IORegistryEntryCreateCFProperty = (delegate* unmanaged[Cdecl]<nint, nint, nint, uint, nint>)
                NativeLibrary.GetExport(_IOKit, "IORegistryEntryCreateCFProperty");

            _CFStringCreateWithCString = (delegate* unmanaged[Cdecl]<nint, sbyte*, uint, nint>)
                NativeLibrary.GetExport(_CoreFoundation, "CFStringCreateWithCString");
            _CFDataGetLength = (delegate* unmanaged[Cdecl]<nint, int>) 
                NativeLibrary.GetExport(_CoreFoundation, "CFDataGetLength");
            _CFDataGetBytes = (delegate* unmanaged[Cdecl]<nint, CFRange, byte*, void>)
                NativeLibrary.GetExport(_CoreFoundation, "CFDataGetBytes");
            _CFNumberGetValue = (delegate* unmanaged[Cdecl]<nint, int, int*, int>)
                NativeLibrary.GetExport(_CoreFoundation, "CFNumberGetValue");
            _CFRelease = (delegate* unmanaged[Cdecl]<nint, void>)
                NativeLibrary.GetExport(_CoreFoundation, "CFRelease");
        }
        
        /// <summary>
        /// Gets the rotation of the display with the specified ID.
        /// </summary>
        /// <param name="displayId">The ID of the display.</param>
        /// <returns>The rotation of the display in degrees.</returns>
        public static double GetDisplayRotation(int displayId) {
            return _CGDisplayRotation(displayId);
        }
        
        /// <summary>
        /// Gets the EDID (Extended Display Identification Data) for the specified display.
        /// </summary>
        /// <param name="displayId">The ID of the display.</param>
        /// <returns>The EDID data as a byte array.</returns>
        public static byte[] GetDisplayEDID(int displayId) {
            int vendorId = _CGDisplayVendorNumber(displayId);
            int productId = _CGDisplayModelNumber(displayId);
            nint matchingDictionary = IOServiceMatching("IODisplayConnect");
            if (matchingDictionary == 0) throw new NativeException($"Failed to create matching dictionary for display {displayId}.");
            nint result = IOServiceGetMatchingServices(0, matchingDictionary, out nint iterator);
            if (result != 0) throw new NativeException($"Failed to get matching services for display {displayId}.");
            if (iterator == 0) throw new NativeException($"No matching services found for display {displayId}.");
            try {
                nint service;
                while ((service = _IOIteratorNext(iterator)) != 0) {
                    try {
                        int foundVendorId = 0;
                        int foundProductId = 0;
                        nint vendorKey = CFString("IODisplayVendorID");
                        nint productKey = CFString("IODisplayProductID");
                        nint edidKey = CFString("IODisplayEDID");
                        try {
                            nint cfVendor = _IORegistryEntryCreateCFProperty(service, vendorKey, 0, 0);
                            nint cfProduct = _IORegistryEntryCreateCFProperty(service, productKey, 0, 0);
                            nint cfEdid = _IORegistryEntryCreateCFProperty(service, edidKey, 0, 0);
                            try {
                                if (cfVendor != 0 && cfProduct != 0 && cfEdid != 0) {
                                    CFNumberGetValue(cfVendor, 9 /* kCFNumberSInt32Type */, out foundVendorId);
                                    CFNumberGetValue(cfProduct, 9 /* kCFNumberSInt32Type */, out foundProductId);
                                    if (foundVendorId == vendorId && foundProductId == productId) {
                                        int length = _CFDataGetLength(cfEdid);
                                        byte[] buffer = new byte[length];
                                        CFDataGetBytes(cfEdid, new() {
                                            Location = 0,
                                            Length = length
                                        }, buffer);
                                        return buffer;
                                    }
                                }
                            } finally {
                                if (cfVendor != 0) _CFRelease(cfVendor);
                                if (cfProduct != 0) _CFRelease(cfProduct);
                                if (cfEdid != 0) _CFRelease(cfEdid);
                            }
                        } finally {
                            if (vendorKey != 0) _CFRelease(vendorKey);
                            if (productKey != 0) _CFRelease(productKey);
                            if (edidKey != 0) _CFRelease(edidKey);
                        }
                    } finally {
                        _IOObjectRelease(service);
                    }
                }
                throw new NativeException($"EDID not found for display {displayId} with vendor ID {vendorId} and product ID {productId}.");
            } finally {
                if (iterator != 0) _IOObjectRelease(iterator);
            }
        }
        
        internal static nint IOServiceGetMatchingServices(nint masterPort, nint matching, out nint iterator) {
            nint it;
            nint result = _IOServiceGetMatchingServices(masterPort, matching, &it);
            iterator = it;
            return result;
        }
        
        internal static nint IOServiceMatching(string name) {
            // UTF-8 + null
            byte[] bytes = Encoding.UTF8.GetBytes(name);
            fixed (byte* p = bytes) {
                // stack-allocate a null-terminated buffer
                int len = bytes.Length;
                byte* tmp = stackalloc byte[len + 1];
                Buffer.MemoryCopy(p, tmp, len + 1, len);
                tmp[len] = 0;
                return _IOServiceMatching((sbyte*)tmp);
            }
        }
        
        // kCFStringEncodingUTF8 = 0x08000100
        internal static nint CFStringCreateWithCString(nint alloc, string str, uint encoding) {
            // Ensure null-terminated UTF-8
            byte[] utf8 = Encoding.UTF8.GetBytes(str);
            fixed (byte* pUtf8 = utf8) {
                int len = utf8.Length;
                byte* tmp = stackalloc byte[len + 1];
                Buffer.MemoryCopy(pUtf8, tmp, len + 1, len);
                tmp[len] = 0;
                return _CFStringCreateWithCString(alloc, (sbyte*)tmp, encoding);
            }
        }
        
        internal static nint CFString(string str) => CFStringCreateWithCString(0, str, 0x08000100u);
        
        internal static void CFDataGetBytes(nint cfData, CFRange range, byte[] buffer) {
            if (buffer is null) throw new ArgumentNullException(nameof(buffer));
            fixed (byte* p = buffer) {
                _CFDataGetBytes(cfData, range, p);
            }
        }
        
        internal static int CFNumberGetValue(nint number, int theType, out int value) {
            fixed (int* p = &value) {
                return _CFNumberGetValue(number, theType, p);
            }
        }
        
        [StructLayout(LayoutKind.Sequential)]
        internal struct CFRange {
            
            public int Location;
            public int Length;
            
        }
        
    }
    // ReSharper restore InconsistentNaming
    
}