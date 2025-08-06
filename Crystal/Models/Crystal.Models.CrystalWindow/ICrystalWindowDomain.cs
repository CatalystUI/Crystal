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

using Catalyst.Domains;
using Crystal.Model;

namespace Crystal.Models.CrystalWindow {
    
    /// <summary>
    /// Represents a domain for windows within the Crystal framework.
    /// </summary>
    /// <remarks>
    /// Windows are primarily used for displaying visual content and managing user interactions,
    /// which is why this domain extends both <see cref="ICrystalDomain"/> and <see cref="IVisualDomain"/>.
    /// Specific implementations may consider further domain-specific behaviors or properties,
    /// and would define an extended domain interface for more specialized functionality so
    /// they can be queried using the <see cref="Catalyst.ModelRegistry"/>.
    /// </remarks>
    public interface ICrystalWindowDomain : ICrystalDomain, IVisualDomain {
        
        // ...
        
    }
    
}