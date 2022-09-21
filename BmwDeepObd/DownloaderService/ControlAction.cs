// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ControlAction.cs" company="Matthew Leibowitz">
//   Copyright (c) Matthew Leibowitz
//   This code is licensed under the Apache 2.0 License
//   http://www.apache.org/licenses/LICENSE-2.0.html
// </copyright>
// <summary>
//   The control action.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ExpansionDownloader.Service
{
    /// <summary>
    /// The control action.
    /// </summary>
    public enum ControlAction
    {
        /// <summary>
        /// This download is allowed to run.
        /// </summary>
        Run = 0, 

        /// <summary>
        /// This download must pause at the first opportunity.
        /// </summary>
        Paused = 1
    }
}