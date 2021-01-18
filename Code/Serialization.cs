﻿using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ICities;
using ColossalFramework;
using ColossalFramework.IO;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Handles savegame data saving and loading.
    /// </summary>
    public class Serializer : SerializableDataExtensionBase
    {
        // Unique data ID.
        private readonly string dataID = "RealPop2";
        internal const int CurrentDataVersion = 1;


        /// <summary>
        /// Serializes data to the savegame.
        /// Called by the game on save.
        /// </summary>
        public override void OnSaveData()
        {
            base.OnSaveData();


            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();

                // Serialise savegame settings.
                DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, CurrentDataVersion, new RealPopSerializer());

                // Write to savegame.
                serializableDataManager.SaveData(dataID, stream.ToArray());

                Logging.Message("wrote ", stream.Length.ToString());
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
                    BinaryFormatter formatter = new BinaryFormatter();

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
            if (!ModSettings.isRealPop2Save)
            {
                // No - we need to work out if this is a new game, or an existing load.
                if ((LoadMode)Singleton<SimulationManager>.instance.m_metaData.m_updateMode == LoadMode.NewGame)
                {
                    Logging.KeyMessage("new game detected");
                    // New game - set this game's legacy save settings to the new game defaults, and set the savegame flag.
                    ModSettings.ThisSaveLegacy = ModSettings.newSaveLegacy;
                    ModSettings.isRealPop2Save = true;
                }
            }
        }
    }


    /// <summary>
    ///  Savegame (de)serialisation for settings.
    /// </summary>
    public class RealPopSerializer : IDataContainer
    {
        /// <summary>
        /// Serialise to savegame.
        /// </summary>
        /// <param name="serializer">Data serializer</param>
        public void Serialize(DataSerializer serializer)
        {
            Logging.Message("writing data to save file");

            // Write data version.
            serializer.WriteInt32(Serializer.CurrentDataVersion);

            // Write 'using legacy' flag.
            serializer.WriteBool(ModSettings.ThisSaveLegacy);
        }


        /// <summary>
        /// Deseralise from savegame.
        /// </summary>
        /// <param name="serializer">Data serializer</param>
        public void Deserialize(DataSerializer serializer)
        {
            Logging.Message("deserializing data from save file");

            try
            {
                // Read data version.
                int dataVersion = serializer.ReadInt32();

                // Make sure we have a matching data version.
                if (dataVersion == Serializer.CurrentDataVersion)
                {

                    Logging.Message("read data version ", dataVersion.ToString());

                    // Read 'using legacy' flag.
                    ModSettings.ThisSaveLegacy = serializer.ReadBool();

                    // Record that we've successfully deserialized savegame data.
                    ModSettings.isRealPop2Save = true;
                }
            }
            catch
            {
                // Don't care if nothing read; assume no settings.
                Logging.Message("error deserializing data");
            }
        }


        /// <summary>
        /// Performs post-serialization data management.  Nothing to do here (yet).
        /// </summary>
        /// <param name="serializer">Data serializer</param>
        public void AfterDeserialize(DataSerializer serializer)
        {
        }
    }
}