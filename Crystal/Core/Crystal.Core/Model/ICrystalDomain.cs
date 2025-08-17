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

namespace Crystal.Model {
    
    /// <summary>
    /// Represents the Crystal domain.
    /// </summary>
    /// <remarks>
    /// The Crystal domain is not intended to be used directly,
    /// but rather serves as a base to help categorize other
    /// extended domains which are specific to Crystal.
    /// <br/><br/>
    /// It indicates that anything within the domain
    /// is related to the Crystal-specific implementation
    /// of the CatalystUI model.
    /// <br/><br/>
    /// All extended interfaces should also extend one
    /// of the base domains from CatalystUI.
    /// </remarks>
    public interface ICrystalDomain : IDomain {
        
        // ...
        
    }
    
}