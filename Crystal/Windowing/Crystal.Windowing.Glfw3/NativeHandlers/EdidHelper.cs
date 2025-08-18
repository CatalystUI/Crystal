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

using System.Text;

namespace Crystal.Windowing.Glfw3.NativeHandlers {
    
    // TODO: Maybe make this an Arcane thing? Seems reasonable enough.
    /// <summary>
    /// EDID utilities for working with Extended Display Identification Data (EDID).
    /// </summary>
    public static class EdidHelper {
        
        /// <summary>
        /// Gets the monitor descriptor from an EDID byte array.
        /// </summary>
        /// <param name="edid">The EDID byte array.</param>
        /// <returns>The monitor descriptor string, or an empty string if not found.</returns>
        public static string GetMonitorDescriptorFromEdid(ref byte[] edid) {
            // Descriptor blocks from offset 0x36 to 0x7F
            for (int i = 0x36; i <= 0x6C; i += 18) {
                // Check for the monitor name tag (0xFC)
                if (edid[i] == 0x00 &&
                    edid[i + 1] == 0x00 &&
                    edid[i + 2] == 0x00 &&
                    (edid[i + 3] == 0xFC || (edid[i + 3] == 0x00 && edid[i + 4] == 0xFC))) {
                    // Fetch the name data (stored in i+5 to i+17, or 13 bytes).
                    byte[] nameBytes = edid[(i + 5)..(i + 18)];
                    string name = Encoding.ASCII.GetString(nameBytes).Trim();
                    return name;
                }
            }
            return string.Empty;
        }
        
        /// <summary>
        /// Gets the 3-character manufacturer EISA ID from the EDID byte array.
        /// </summary>
        /// <param name="edid">The EDID byte array.</param>
        /// <returns>The manufacturer code (e.g., "CFL"), or an empty string if one could not be determined.</returns>
        public static string GetMonitorManufacturerCodeFromEdid(ref byte[] edid) {
            // Must be at least 10 bytes long to contain the manufacturer ID
            if (edid.Length < 0x0A) return string.Empty;
            
            // The manufacturer ID is packed into bytes 0x08 and 0x09
            ushort raw = (ushort) ((edid[0x08] << 8) | edid[0x09]);
            
            // Extract 3 5-bit character codes
            char c1 = (char) (((raw >> 10) & 0x1F) + 'A' - 1);
            char c2 = (char) (((raw >> 5) & 0x1F) + 'A' - 1);
            char c3 = (char) ((raw & 0x1F) + 'A' - 1);
            
            // Sanity check: all must be in 'A'..'Z'
            if (c1 is < 'A' or > 'Z' || c2 is < 'A' or > 'Z' || c3 is < 'A' or > 'Z')
                return string.Empty;
            
            return new string([ c1, c2, c3 ]);
        }
        
    }
    
}