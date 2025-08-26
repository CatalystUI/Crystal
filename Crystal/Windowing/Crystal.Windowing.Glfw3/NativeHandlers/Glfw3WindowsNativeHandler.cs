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
using Microsoft.Win32;
using System.Runtime.InteropServices;
using Monitor = Silk.NET.GLFW.Monitor;

namespace Crystal.Windowing.Glfw3.NativeHandlers {
    
    /// <summary>
    /// A Microsoft Windows-based implementation of <see cref="IGlfw3NativeHandler{TLayerLow}"/>.
    /// </summary>
    public sealed unsafe class Glfw3WindowsNativeHandler : IGlfw3NativeHandler<IWindowsSystemLayer> {
        
        /// <inheritdoc/>
        public double GetDisplayRotation(Glfw3 glfw, Monitor* pMonitor) {
            nint deviceName = WindowsGlfwImports.glfwGetWin32Monitor(glfw, pMonitor);
            string? str = Marshal.PtrToStringUTF8(deviceName);
            if (str == null) throw new NativeException("Failed to retrieve device name from monitor.");
            return WindowsImports.GetDisplayRotation(str);
        }
        
        /// <inheritdoc/>
        public string GetDisplayDescriptor(Glfw3 glfw, Monitor* pMonitor) {
            nint deviceName = WindowsGlfwImports.glfwGetWin32Monitor(glfw, pMonitor);
            string? str = Marshal.PtrToStringUTF8(deviceName);
            if (str == null) throw new NativeException("Failed to retrieve device name from monitor.");
            List<byte[]> edids = WindowsImports.GetDisplayEdid(str);
            if (edids.Count == 0) throw new NativeException("No EDID data found for monitor.");
            string descriptor = string.Empty;
            for (int i = 0; i < edids.Count; i++) {
                byte[] edid = edids[i];
                descriptor = EdidHelper.GetMonitorDescriptorFromEdid(ref edid);
                if (!string.IsNullOrEmpty(descriptor)) break;
            }
            if (string.IsNullOrEmpty(descriptor)) throw new NativeException("No monitor descriptor found in EDID data.");
            return descriptor;
        }
        
        /// <inheritdoc/>
        public string GetDisplayManufacturer(Glfw3 glfw, Monitor* pMonitor) {
            nint deviceName = WindowsGlfwImports.glfwGetWin32Monitor(glfw, pMonitor);
            string? str = Marshal.PtrToStringUTF8(deviceName);
            if (str == null) throw new NativeException("Failed to retrieve device name from monitor.");
            List<byte[]> edids = WindowsImports.GetDisplayEdid(str);
            if (edids.Count == 0) throw new NativeException("No EDID data found for monitor.");
            string manufacturer = string.Empty;
            for (int i = 0; i < edids.Count; i++) {
                byte[] edid = edids[i];
                manufacturer = EdidHelper.GetMonitorManufacturerCodeFromEdid(ref edid);
                if (!string.IsNullOrEmpty(manufacturer)) break;
            }
            if (string.IsNullOrEmpty(manufacturer)) throw new NativeException("No monitor manufacturer found in EDID data.");
            return manufacturer;
        }
        
    }
    
    // ReSharper disable InconsistentNaming
    internal static unsafe class WindowsGlfwImports {
        
        /// <summary>
        /// Returns the Win32 device name for the specified monitor.
        /// </summary>
        /// <param name="pMonitor">The monitor to get the device name for.</param>
        /// <returns>The device name of the monitor.</returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate nint GetWin32Monitor(Monitor* pMonitor);
        
        private static GetWin32Monitor? _glfwGetWin32Monitor;
        
        public static nint glfwGetWin32Monitor(Glfw3 glfw, Monitor* pMonitor) {
            if (_glfwGetWin32Monitor == null) {
                if (glfw.Api.Context.TryGetProcAddress("glfwGetWin32Monitor", out nint procAddress)) {
                    _glfwGetWin32Monitor = Marshal.GetDelegateForFunctionPointer<GetWin32Monitor>(procAddress);
                } else {
                    throw new PlatformNotSupportedException("glfwGetWin32Monitor is not supported on this platform.");
                }
            }
            return _glfwGetWin32Monitor(pMonitor);
        }
        
    }
    // ReSharper restore InconsistentNaming
    
    // ReSharper disable InconsistentNaming
    internal static unsafe class WindowsImports {
        
        private static readonly nint _user32;
        private static readonly nint _gdi32;
        
        private static readonly delegate* unmanaged[Stdcall]<nint, MONITORINFO*, bool> _GetMonitorInfoW;
        private static readonly delegate* unmanaged[Stdcall]<nint, RECT*, nint, nint, bool> _EnumDisplayMonitors;
        private static readonly delegate* unmanaged[Stdcall]<nint, uint, ref DEVMODEW, bool> _EnumDisplaySettingsW;
        private static readonly delegate* unmanaged[Stdcall]<nint, uint, ref DISPLAY_DEVICEW, uint, bool> _EnumDisplayDevicesW;
        
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal unsafe delegate bool EnumMonitorsProc(nint hMonitor, nint hdc, RECT* rect, nint data);
        
        static WindowsImports() {
            _user32 = NativeLibrary.Load("user32.dll");
            _gdi32 = NativeLibrary.Load("gdi32.dll");
            
            _GetMonitorInfoW = (delegate* unmanaged[Stdcall]<nint, MONITORINFO*, bool>)
                NativeLibrary.GetExport(_user32, "GetMonitorInfoW");
            _EnumDisplayMonitors = (delegate* unmanaged[Stdcall]<nint, RECT*, nint, nint, bool>)
                NativeLibrary.GetExport(_user32, "EnumDisplayMonitors");
            _EnumDisplaySettingsW = (delegate* unmanaged[Stdcall]<nint, uint, ref DEVMODEW, bool>)
                NativeLibrary.GetExport(_user32, "EnumDisplaySettingsW");
            _EnumDisplayDevicesW = (delegate* unmanaged[Stdcall]<nint, uint, ref DISPLAY_DEVICEW, uint, bool>)
                NativeLibrary.GetExport(_user32, "EnumDisplayDevicesW");
        }
        
        /// <summary>
        /// Gets the rotation of the display for the specified device name.
        /// </summary>
        /// <param name="deviceName">The name of the device.</param>
        /// <returns>The rotation of the display in degrees.</returns>
        public static double GetDisplayRotation(string deviceName) {
            DEVMODEW devmode = GetDEVMODEW(deviceName);
            const uint DM_DISPLAYORIENTATION = 0x80;
            if ((devmode.dmFields & DM_DISPLAYORIENTATION) != 0) {
                return devmode.dmDisplayOrientation switch {
                    0 => 0,    // DMDO_DEFAULT
                    1 => 90,   // DMDO_90
                    2 => 180,  // DMDO_180
                    3 => 270,  // DMDO_270
                    _ => throw new NativeException($"Unknown display orientation: {devmode.dmDisplayOrientation}")
                };
            } else {
                throw new NativeException($"Display orientation not set in DEVMODEW for device: {deviceName}");
            }
        }
        
        /// <summary>
        /// Gets the EDID (Extended Display Identification Data) for the specified display device.
        /// </summary>
        /// <param name="deviceName">The name of the display device.</param>
        /// <returns>A list of byte arrays representing the EDID data for the display.</returns>
        public static List<byte[]> GetDisplayEdid(string deviceName) {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) throw new PlatformNotSupportedException("EDID registry access is Windows-only.");
            DISPLAY_DEVICEW device = WindowsImports.GetDISPLAY_DEVICEW(deviceName);
            string deviceID = device.DeviceID.ToString().TrimEnd('\0');
            string hwid = deviceID.Split('\\')[1]; // Extract the hardware ID part after the first backslash
            List<byte[]> found = [ ];
            using RegistryKey? displayKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\DISPLAY");
            if (displayKey == null) throw new NativeException("Failed to open display registry key.");
            string[] displaySubKeyNames = displayKey.GetSubKeyNames();
            for (int i = 0; i < displaySubKeyNames.Length; i++) {
                // Check against the hwid
                string hwidKeyName = displaySubKeyNames[i];
                if (string.IsNullOrEmpty(hwidKeyName)) continue;
                if (!hwidKeyName.Contains(hwid, StringComparison.OrdinalIgnoreCase)) continue;
                
                // If it matches, pull the EDID data
                using RegistryKey? hwidKey = displayKey.OpenSubKey(hwidKeyName);
                if (hwidKey == null) continue;
                string[] hwidSubKeyNames = hwidKey.GetSubKeyNames();
                for (int j = 0; j < hwidSubKeyNames.Length; j++) {
                    using RegistryKey? instanceKey = hwidKey.OpenSubKey(hwidSubKeyNames[j]);
                    using RegistryKey? deviceParamsKey = instanceKey?.OpenSubKey("Device Parameters");
                    if (deviceParamsKey?.GetValue("EDID") is byte[] { Length: >= 128 } edid) {
                        found.Add(edid);
                    }
                }
            }
            return found;
        }
        
        internal static MONITORINFOEXW GetMonitorInfo(IntPtr hMonitor) {
            MONITORINFOEXW info = new();
            info.monitorInfo.cbSize = (uint) Marshal.SizeOf<MONITORINFOEXW>();
            if (!_GetMonitorInfoW(hMonitor, (MONITORINFO*)&info)) {
                throw new NativeException($"Failed to retrieve monitor info for HMONITOR: {hMonitor}");
            }
            return info;
        }
        
        internal static nint GetHMONITOR(string deviceName) {
            nint? result = null;
            int backslash = deviceName.IndexOf('\\', @"\\.\DISPLAY".Length);
            string trimmed = backslash >= 0 ? deviceName[..backslash] : deviceName;
            EnumMonitorsProc proc = (hMonitor, hdcMonitor, lprcMonitor, dwData) => {
                try {
                    MONITORINFOEXW info = GetMonitorInfo(hMonitor);
                    string currentDevice = new string(info.szDevice).TrimEnd('\0');
                    if (string.Equals(currentDevice, trimmed, StringComparison.OrdinalIgnoreCase)) {
                        result = hMonitor;
                        return false; // stop enumeration
                    }
                } catch {
                    // ...
                }
                return true; // continue enumeration
            };
            GCHandle handle = GCHandle.Alloc(proc);
            try {
                nint pProc = Marshal.GetFunctionPointerForDelegate(proc);
                _EnumDisplayMonitors(nint.Zero, null, pProc, nint.Zero);
            } finally {
                handle.Free();
            }
            if (result == null) throw new NativeException("Failed to retrieve HMONITOR for device: " + new string(deviceName));
            return result.Value;
        }
        
        internal static DEVMODEW GetDEVMODEW(string deviceName) {
            DEVMODEW devmode = new() {
                dmSize = (ushort)Marshal.SizeOf<DEVMODEW>()
            };
            int backslash = deviceName.IndexOf('\\', @"\\.\DISPLAY".Length);
            string trimmed = backslash >= 0 ? deviceName[..backslash] : deviceName;
            nint lpDeviceName = Marshal.StringToHGlobalUni(trimmed);
            try {
                if (!_EnumDisplaySettingsW(lpDeviceName, unchecked((uint) -1), ref devmode)) {
                    throw new NativeException("Failed to retrieve DEVMODEW for device: " + deviceName);
                }
            } finally {
                if (lpDeviceName != nint.Zero) Marshal.FreeHGlobal(lpDeviceName);
            }
            return devmode;
        }
        
        internal static DISPLAY_DEVICEW GetDISPLAY_DEVICEW(string deviceName) {
            nint hMonitor = GetHMONITOR(deviceName);
            MONITORINFOEXW info = GetMonitorInfo(hMonitor);
            DISPLAY_DEVICEW device = new() {
                cb = (uint)Marshal.SizeOf<DISPLAY_DEVICEW>()
            };
            nint lpAdapterName = Marshal.StringToHGlobalUni(new string(info.szDevice));
            try {
                if (!_EnumDisplayDevicesW(lpAdapterName, 0, ref device, 0)) {
                    throw new NativeException("Failed to retrieve DISPLAY_DEVICEW for device: " + deviceName);
                }
            } finally {
                if (lpAdapterName != nint.Zero) Marshal.FreeHGlobal(lpAdapterName);
            }
            return device;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MONITORINFO {
            public uint cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal unsafe struct MONITORINFOEXW {
            public MONITORINFO monitorInfo;
            public fixed char szDevice[32];
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct DEVMODEW {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public ushort dmSpecVersion;
            public ushort dmDriverVersion;
            public ushort dmSize;
            public ushort dmDriverExtra;
            public uint dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public uint dmDisplayOrientation;
            public uint dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public ushort dmLogPixels;
            public uint dmBitsPerPel;
            public uint dmPelsWidth;
            public uint dmPelsHeight;
            public uint dmDisplayFlags;
            public uint dmDisplayFrequency;
            public uint dmICMMethod;
            public uint dmICMIntent;
            public uint dmMediaType;
            public uint dmDitherType;
            public uint dmReserved1;
            public uint dmReserved2;
            public uint dmPanningWidth;
            public uint dmPanningHeight;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct DISPLAY_DEVICEW {
            public uint cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public uint StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }
        
    }
    // ReSharper enable InconsistentNaming
    
}