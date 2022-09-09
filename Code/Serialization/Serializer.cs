// <copyright file="Serializer.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using AlgernonCommons;
    using ColossalFramework;
    using ColossalFramework.IO;
    using ICities;

    /// <summary>
    /// Handles savegame data saving and loading.
    /// </summary>
    public class Serializer : SerializableDataExtensionBase
    {
        /// <summary>
        /// Current savegame data version.
        /// </summary>
        internal const int CurrentDataVersion = 7;

        // Unique data ID.
        private readonly string dataID = "RealisticPopulation";

        /// <summary>
        /// Serializes data to the savegame.
        /// Called by the game on save.
        /// </summary>
        public override void OnSaveData()
        {
            base.OnSaveData();
            using (MemoryStream stream = new MemoryStream())
            {
                // Serialise savegame settings.
                DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, CurrentDataVersion, new RealPopSerializer());

                // Write to savegame.
                serializableDataManager.SaveData(dataID, stream.ToArray());

                Logging.Message("wrote ", stream.Length);
            }
        }

        /// <summary>
        /// Deserializes data from a savegame (or initialises new data structures when none available).
        /// Called by the game on load (including a new game).
        /// </summary>
        public override void OnLoadData()
        {
            Logging.Message("reading data from save file");
            base.OnLoadData();

            // Read data from savegame.
            byte[] data = serializableDataManager.LoadData(dataID);

            // Check to see if anything was read.
            if (data != null && data.Length != 0)
            {
                // Data was read - go ahead and deserialise.
                using (MemoryStream stream = new MemoryStream(data))
                {
                    // Deserialise savegame settings.
                    DataSerializer.Deserialize<RealPopSerializer>(stream, DataSerializer.Mode.Memory);
                }
            }
            else
            {
                // No data read.
                Logging.Message("no data read");
            }

            // Were we able to deserialize data?
            if (!ModSettings.IsRealPop2Save)
            {
                // No - we need to work out if this is a new game, or an existing load.
                if ((LoadMode)Singleton<SimulationManager>.instance.m_metaData.m_updateMode == LoadMode.NewGame)
                {
                    Logging.KeyMessage("new game detected");

                    // New game - set this game's legacy save settings to the new game defaults, and set the savegame flag.
                    ModSettings.ThisSaveDefaultRes = ModSettings.NewSaveDefaultRes;
                    ModSettings.ThisSaveDefaultCom = ModSettings.NewSaveDefaultCom;
                    ModSettings.ThisSaveDefaultInd = ModSettings.NewSaveDefaultInd;
                    ModSettings.ThisSaveDefaultOff = ModSettings.NewSaveDefaultOff;
                    ModSettings.IsRealPop2Save = true;
                }
            }
        }
    }
}