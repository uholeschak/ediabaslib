// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DownloadServiceRequirement.cs" company="Matthew Leibowitz">
//   Copyright (c) Matthew Leibowitz
//   This code is licensed under the Apache 2.0 License
//   http://www.apache.org/licenses/LICENSE-2.0.html
// </copyright>
// <summary>
//   The download service requirement.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ExpansionDownloader
{
    /// <summary>
    /// The download service requirement.
    /// </summary>
    public enum DownloadServiceRequirement
    {
        /// <summary>
        /// The download required.
        /// </summary>
        DownloadRequired = 2, 

        /// <summary>
        /// The lvl check required.
        /// </summary>
        LvlCheckRequired = 1, 

        /// <summary>
        /// The no download required.
        /// </summary>
        NoDownloadRequired = 0
    }
}