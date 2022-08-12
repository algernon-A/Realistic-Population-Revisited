// <copyright file="GoodsUtils.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Static utilities class for handling goods production and consumption.
    /// </summary>
    public static class GoodsUtils
    {
        /// <summary>
        /// Default sales multiplier.
        /// </summary>
        internal const int DefaultSalesMult = 60;

        /// <summary>
        /// Default inventory order threshold.
        /// </summary>
        internal const int DefaultInventory = 48000;

        /// <summary>
        /// Minimum inventory order threshold.
        /// </summary>
        internal const int MinInventory = 8000;

        /// <summary>
        /// Maximum inventory order threshold.
        /// </summary>
        internal const int MaxInventory = 57000;

        // Internal multipliers.
        private static int s_lowComMult = DefaultSalesMult;
        private static int s_highComMult = DefaultSalesMult;
        private static int s_ecoComMult = DefaultSalesMult;
        private static int s_touristMult = DefaultSalesMult;
        private static int s_leisureMult = DefaultSalesMult;

        // Internal inventory caps;
        private static int s_lowComInv = DefaultInventory;
        private static int s_highComInv = DefaultInventory;
        private static int s_ecoComInv = DefaultInventory;
        private static int s_touristInv = DefaultInventory;
        private static int s_leisureInv = DefaultInventory;

        /// <summary>
        /// Gets the current commercial sales multiplier for the given commercial building.
        /// </summary>
        /// <param name="building">Specified building.</param>
        /// <returns>Current sales multiplier as integer pecentage.</returns>
        public static int GetComMult(ref Building building) => GetComMult(building.Info.GetSubService());

        /// <summary>
        /// Gets the current inventory cap for the given commercial building.
        /// </summary>
        /// <param name="building">Specified building.</param>
        /// <returns>Current sales multiplier as integer pecentage.</returns>
        public static int GetInventoryCap(ref Building building) => GetInventoryCap(building.Info.GetSubService());

        /// <summary>
        /// Gets the current inventory cap for the given commercial building ai.
        /// </summary>
        /// <param name="ai">CommercialBuildingAI instance.</param>
        /// <returns>Current sales multiplier as integer pecentage.</returns>
        public static int GetInventoryCap(CommercialBuildingAI ai) => GetInventoryCap(ai.m_info.GetSubService());

        /// <summary>
        /// Gets the current commercial sales multiplier for the given commercial subservice.
        /// </summary>
        /// <param name="subService">Specified subservice.</param>
        /// <returns>Current sales multiplier as integer pecentage.</returns>
        internal static int GetComMult(ItemClass.SubService subService)
        {
            switch (subService)
            {
                case ItemClass.SubService.CommercialHigh:
                    return s_highComMult;
                case ItemClass.SubService.CommercialEco:
                    return s_ecoComMult;
                case ItemClass.SubService.CommercialLeisure:
                    return s_leisureMult;
                case ItemClass.SubService.CommercialTourist:
                    return s_touristMult;
                default:
                    return s_lowComMult;
            }
        }

        /// <summary>
        /// Returns the current inventory cap for the given commercial subservice.
        /// </summary>
        /// <param name="subService">Specified subservice.</param>
        /// <returns>Current inventory cap.</returns>
        internal static int GetInventoryCap(ItemClass.SubService subService)
        {
            switch (subService)
            {
                case ItemClass.SubService.CommercialHigh:
                    return s_highComInv;
                case ItemClass.SubService.CommercialEco:
                    return s_ecoComInv;
                case ItemClass.SubService.CommercialLeisure:
                    return s_leisureInv;
                case ItemClass.SubService.CommercialTourist:
                    return s_touristInv;
                default:
                    return s_lowComInv;
            }
        }

        /// <summary>
        /// Sets the current commercial sales multiplier for the given commercial subservice.
        /// </summary>
        /// <param name="subService">Sub-service to set.</param>
        /// <param name="value">Value to set.</param>
        internal static void SetComMult(ItemClass.SubService subService, int value)
        {
            int cleanValue = Mathf.Clamp(value, 0, 100);

            switch (subService)
            {
                case ItemClass.SubService.CommercialLow:
                    s_lowComMult = cleanValue;
                    break;
                case ItemClass.SubService.CommercialHigh:
                    s_highComMult = cleanValue;
                    break;
                case ItemClass.SubService.CommercialEco:
                    s_ecoComMult = cleanValue;
                    break;
                case ItemClass.SubService.CommercialLeisure:
                    s_leisureMult = cleanValue;
                    break;
                case ItemClass.SubService.CommercialTourist:
                    s_touristMult = cleanValue;
                    break;
            }
        }

        /// <summary>
        /// Sets the current inventory cap for the given commercial subservice.
        /// </summary>
        /// <param name="subService">Sub-service to set.</param>
        /// <param name="value">Value to set.</param>
        internal static void SetInventoryCap(ItemClass.SubService subService, int value)
        {
            int cleanValue = Mathf.Clamp(value, MinInventory, MaxInventory);

            switch (subService)
            {
                case ItemClass.SubService.CommercialLow:
                    s_lowComInv = cleanValue;
                    break;
                case ItemClass.SubService.CommercialHigh:
                    s_highComInv = cleanValue;
                    break;
                case ItemClass.SubService.CommercialEco:
                    s_ecoComInv = cleanValue;
                    break;
                case ItemClass.SubService.CommercialLeisure:
                    s_leisureInv = cleanValue;
                    break;
                case ItemClass.SubService.CommercialTourist:
                    s_touristInv = cleanValue;
                    break;
            }
        }

        /// <summary>
        /// Serializes the current sales multiplier settings ready for XML.
        /// </summary>
        /// <returns>New list of sub-service entries ready for serialization.</returns>
        internal static List<Configuration.SubServiceValue> SerializeSalesMults()
        {
            return new List<Configuration.SubServiceValue>
            {
                new Configuration.SubServiceValue
                {
                    SubService = ItemClass.SubService.CommercialLow,
                    Value = s_lowComMult,
                },
                new Configuration.SubServiceValue
                {
                    SubService = ItemClass.SubService.CommercialHigh,
                    Value = s_highComMult,
                },
                new Configuration.SubServiceValue
                {
                    SubService = ItemClass.SubService.CommercialEco,
                    Value = s_ecoComMult,
                },
                new Configuration.SubServiceValue
                {
                    SubService = ItemClass.SubService.CommercialLeisure,
                    Value = s_leisureMult,
                },
                new Configuration.SubServiceValue
                {
                    SubService = ItemClass.SubService.CommercialTourist,
                    Value = s_touristMult,
                },
            };
        }

        /// <summary>
        /// Serializes the current inventory demand caps ready for XML.
        /// </summary>
        /// <returns>New list of sub-service entries ready for serialization.</returns>
        internal static List<Configuration.SubServiceValue> SerializeInvCaps()
        {
            return new List<Configuration.SubServiceValue>
            {
                new Configuration.SubServiceValue
                {
                    SubService = ItemClass.SubService.CommercialLow,
                    Value = s_lowComInv,
                },
                new Configuration.SubServiceValue
                {
                    SubService = ItemClass.SubService.CommercialHigh,
                    Value = s_highComInv,
                },
                new Configuration.SubServiceValue
                {
                    SubService = ItemClass.SubService.CommercialEco,
                    Value = s_ecoComInv,
                },
                new Configuration.SubServiceValue
                {
                    SubService = ItemClass.SubService.CommercialLeisure,
                    Value = s_leisureInv,
                },
                new Configuration.SubServiceValue
                {
                    SubService = ItemClass.SubService.CommercialTourist,
                    Value = s_touristInv,
                },
            };
        }

        /// <summary>
        /// Deserializes XML sub-service entries for sales multipliers.
        /// </summary>
        /// <param name="entries">List of sub-service entries to deserialize.</param>
        internal static void DeserializeSalesMults(List<Configuration.SubServiceValue> entries)
        {
            foreach (Configuration.SubServiceValue entry in entries)
            {
                SetComMult(entry.SubService, entry.Value);
            }
        }

        /// <summary>
        /// Deserializes XML sub-service entries for inventory demand caps.
        /// </summary>
        /// <param name="entries">List of sub-service entries to deserialize.</param>
        internal static void DeserializeInvCaps(List<Configuration.SubServiceValue> entries)
        {
            foreach (Configuration.SubServiceValue entry in entries)
            {
                SetInventoryCap(entry.SubService, entry.Value);
            }
        }
    }
}
