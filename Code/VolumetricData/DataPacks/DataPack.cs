// <copyright file="DataPack.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons.Translation;

    /// <summary>
    /// Calculation data pack - provides parameters for calculating building populations for given services and (optional) subservices.
    /// </summary>
    internal class DataPack
    {
        // Data version.
        private readonly DataVersion _version;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataPack"/> class.
        /// </summary>
        /// <param name="version">Datapack version.</param>
        internal DataPack(DataVersion version)
        {
            _version = version;
        }

        /// <summary>
        /// Data pack calculation version.
        /// </summary>
        internal enum DataVersion
        {
            /// <summary>
            /// Vanilla calculations.
            /// </summary>
            Vanilla = 0,

            /// <summary>
            /// Legacy calculations.
            /// </summary>
            Legacy,

            /// <summary>
            /// Volumentric version one.
            /// </summary>
            One,

            /// <summary>
            /// Custom calculation version one.
            /// </summary>
            CustomOne,

            /// <summary>
            /// Override calculation version one.
            /// </summary>
            OverrideOne,
        }

        /// <summary>
        /// Gets the data pack's version.
        /// </summary>
        internal DataVersion Version => _version;

        /// <summary>
        /// Gets or sets the data pack's raw name.
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// Gets or sets the data pack's name translation key.
        /// </summary>
        internal string NameKey { get; set; }

        /// <summary>
        /// Gets or sets the data pack's description translation key.
        /// </summary>
        internal string DescriptionKey { get; set; }

        /// <summary>
        /// Gets the pack's display name, in current language if available.
        /// </summary>
        internal string DisplayName => !string.IsNullOrEmpty(NameKey) ? Translations.Translate(NameKey) : Name;

        /// <summary>
        /// Gets the pack's description, in current language if available.
        /// </summary>
        internal string Description => !string.IsNullOrEmpty(DescriptionKey) ? Translations.Translate(DescriptionKey) : string.Empty;
    }
}