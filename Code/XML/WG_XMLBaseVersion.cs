// <copyright file="WG_XMLBaseVersion.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard) and Whitefang Greytail. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System.Xml;

    /// <summary>
    /// WG legacy settings file base class.
    /// </summary>
    public abstract class WG_XMLBaseVersion
    {
        /// <summary>
        /// Read XML document.
        /// </summary>
        /// <param name="doc">Document to read.</param>
        public abstract void ReadXML(XmlDocument doc);

        /// <summary>
        /// Write XML document.
        /// </summary>
        /// <param name="fullPathFileName">Destination pathfile.</param>
        /// <returns>True if write was successful, false otherwise.</returns>
        public abstract bool WriteXML(string fullPathFileName);
    }
}