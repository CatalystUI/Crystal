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
    /// Represents an error that occurs during windowing operations within the Crystal framework.
    /// </summary>
    /// <inheritdoc/>
    public class WindowException : Exception {
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowException"/> class.
        /// </summary>
        public WindowException() : base() {
            // ...
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowException"/> class
        /// with a specified error message.
        /// </summary>
        public WindowException(string message) : base(message) {
            // ...
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowException"/> class
        /// with a specified error message and a reference to the inner exception
        /// that is the cause of this exception.
        /// </summary>
        public WindowException(string message, Exception innerException) : base(message, innerException) {
            // ...
        }
        
    }
    
}