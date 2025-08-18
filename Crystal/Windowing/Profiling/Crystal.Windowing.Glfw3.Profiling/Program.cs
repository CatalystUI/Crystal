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

namespace Catalyst.Profiling {
    
    /// <summary>
    /// A basic program used to general debugging and profiling of CatalystUI
    /// and its associated libraries or tools.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The executable project references all associated libraries via their
    /// project references to allow for debugging of the internals. It also
    /// references a class called <c>Profile</c>, and fires a method with the
    /// signature of <c>static void Entry()</c>.
    /// </para>
    /// <para>
    /// <b>To use the profiling project</b>, copy the <c>Profile.template.cs</c>
    /// file, and rename it to <c>Profile.cs</c>. Also rename the class
    /// from <c>ProfileTemplate</c> to <c>Profile</c>. Do not delete
    /// the template class, as that is committed to the repository.
    /// You may also rename the <c>catdebug.template.ini</c> file to
    /// <c>catdebug.ini</c> to enable debugging features.
    /// </para>
    /// <para>
    /// After creating the <c>Profile.cs</c> file, sometimes you
    /// may need to reload the project in your IDE to ensure
    /// the conditional compilation symbol is defined.
    /// </para>
    /// <para>
    /// The profiling project is designed to be a simple way to profile
    /// sections of Catalyst's code, test performance, and/or generically
    /// test features of the library without needing to commit a new
    /// testing project to the repository. It only commits
    /// the necessary files to the repository, such as
    /// the csproj file, the Program file, and the template file,
    /// with special provisions made in the `.gitignore` to ensure
    /// no other files are committed.
    /// </para>
    /// <para>
    /// Please avoid modifications to the project file itself as
    /// much as possible to avoid git conflicts. If modification
    /// is unavoidable, ensure the changes do not get committed.
    /// </para>
    /// </remarks>
    public static class Program {
        
        /// <summary>
        /// The main entry point for the profiling program.
        /// </summary>
        public static void Main() {
#if PROFILE_EXISTS
            Profile.Entry();
#endif
        }
        
    }
    
}