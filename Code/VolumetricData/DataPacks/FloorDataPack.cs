// <copyright file="FloorDataPack.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    /// <summary>
    /// Floor calculation data pack - provides parameters for calculating building floors.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Internal fields passed by ref")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Internal fields passed by ref")]
    internal class FloorDataPack : DataPack
    {
        /// <summary>
        /// Height per floor, in metres.
        /// </summary>
        internal float m_floorHeight;

        /// <summary>
        /// Minimum height required for any floor to exist, in metres.
        /// </summary>
        internal float m_firstFloorMin;

        /// <summary>
        /// Height by which the first floor is extended up to (if building height is available), in metres.
        /// </summary>
        internal float m_firstFloorExtra;

        /// <summary>
        /// Whether the first floor should be excluded from calculations (e.g. for foyers/lobbies).
        /// </summary>
        internal bool m_firstFloorEmpty;

        /// <summary>
        /// Initializes a new instance of the <see cref="FloorDataPack"/> class.
        /// </summary>
        /// <param name="version">Datapack version.</param>
        internal FloorDataPack(DataVersion version)
            : base(version)
        {
        }
    }
}