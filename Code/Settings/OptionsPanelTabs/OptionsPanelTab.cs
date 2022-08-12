// <copyright file="OptionsPanelTab.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using ColossalFramework.UI;

    /// <summary>
    /// Base class for options panel tabs.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Protected fields")]
    internal abstract class OptionsPanelTab
    {
        /// <summary>
        /// Setup status flag.
        /// </summary>
        protected bool m_isSetup = false;

        /// <summary>
        /// Current panel instance.
        /// </summary>
        protected UIPanel m_panel;

        /// <summary>
        /// Performs initial setup; called via event when tab is first selected.
        /// </summary>
        internal abstract void Setup();
    }
}