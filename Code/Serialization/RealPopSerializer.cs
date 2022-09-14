// <copyright file="RealPopSerializer.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons;
    using AlgernonCommons.XML;
    using ColossalFramework.IO;

    /// <summary>
    ///  Savegame (de)serialisation for settings.
    /// </summary>
    public class RealPopSerializer : IDataContainer
    {
        /// <summary>
        /// Serialize to savegame.
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void Serialize(DataSerializer serializer)
        {
            Logging.Message("writing data to save file");

            // Write data version.
            serializer.WriteInt32(Serializer.CurrentDataVersion);

            // Write 'using legacy' flags.
            serializer.WriteUInt8((byte)ModSettings.ThisSaveDefaultRes);
            serializer.WriteUInt8((byte)ModSettings.ThisSaveDefaultCom);
            serializer.WriteUInt8((byte)ModSettings.ThisSaveDefaultInd);
            serializer.WriteUInt8((byte)ModSettings.ThisSaveDefaultOff);

            // Write embedded config file.
            // serializer.WriteByteArray(XMLFileUtils.SerializeBinary<XMLSettingsFile>());
        }

        /// <summary>
        /// Deseralize from savegame.
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void Deserialize(DataSerializer serializer)
        {
            Logging.Message("deserializing data from save file");

            try
            {
                // Read data version.
                int dataVersion = serializer.ReadInt32();
                Logging.Message("read data version ", dataVersion);

                // Make sure we have a matching data version.
                if (dataVersion >= 7)
                {
                    // Replaces 'using legacy' bool flags.
                    ModSettings.ThisSaveDefaultRes = (DefaultMode)serializer.ReadUInt8();
                    ModSettings.ThisSaveDefaultCom = (DefaultMode)serializer.ReadUInt8();
                    ModSettings.ThisSaveDefaultInd = (DefaultMode)serializer.ReadUInt8();
                    ModSettings.ThisSaveDefaultOff = (DefaultMode)serializer.ReadUInt8();

                    // Record that we've successfully deserialized savegame data.
                    ModSettings.IsRealPop2Save = true;
                }

                if (dataVersion == 3 || dataVersion == 5 || dataVersion == 6)
                {
                    // Versions where industrial and extractor workplace legacy settings are combined.

                    // Read 'using legacy' flags for residential and workplace buildings, in order.
                    ModSettings.ThisSaveLegacyRes = serializer.ReadBool();
                    ModSettings.ThisSaveLegacyCom = serializer.ReadBool();
                    ModSettings.ThisSaveLegacyInd = serializer.ReadBool();
                    ModSettings.ThisSaveLegacyOff = serializer.ReadBool();

                    // Record that we've successfully deserialized savegame data.
                    ModSettings.IsRealPop2Save = true;
                }
                else if (dataVersion == 4)
                {
                    // Legacy data version which had separate industrial and extractor defaults.

                    // Read 'using legacy' flags for residential and workplace buildings, in order.
                    ModSettings.ThisSaveLegacyRes = serializer.ReadBool();
                    ModSettings.ThisSaveLegacyCom = serializer.ReadBool();
                    ModSettings.ThisSaveLegacyInd = serializer.ReadBool();
                    serializer.ReadBool();
                    ModSettings.ThisSaveLegacyOff = serializer.ReadBool();

                    // Record that we've successfully deserialized savegame data.
                    ModSettings.IsRealPop2Save = true;
                }
                else if (dataVersion == 2)
                {
                    // Legacy data version with residential and workplace legacy settings.

                    // Read 'using legacy' flags.
                    ModSettings.ThisSaveLegacyRes = serializer.ReadBool();
                    bool thisSaveLegacyWrk = serializer.ReadBool();
                    ModSettings.ThisSaveLegacyCom = thisSaveLegacyWrk;
                    ModSettings.ThisSaveLegacyInd = thisSaveLegacyWrk;
                    ModSettings.ThisSaveLegacyOff = thisSaveLegacyWrk;

                    // Record that we've successfully deserialized savegame data.
                    ModSettings.IsRealPop2Save = true;
                }
                else if (dataVersion == 1)
                {
                    // Legacy data version with combined legacy settings.

                    // Read 'using legacy' flag.
                    bool thisSaveLegacy = serializer.ReadBool();
                    ModSettings.ThisSaveLegacyRes = thisSaveLegacy;
                    ModSettings.ThisSaveLegacyCom = thisSaveLegacy;
                    ModSettings.ThisSaveLegacyInd = thisSaveLegacy;
                    ModSettings.ThisSaveLegacyOff = thisSaveLegacy;

                    // Record that we've successfully deserialized savegame data.
                    ModSettings.IsRealPop2Save = true;
                }

                /*
                if (dataVersion == 8)
                {
                    // Read embedded config file.
                    XMLFileUtils.DeserializeBinary<XMLSettingsFile>(serializer.ReadByteArray());
                }
                */
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
        /// <param name="serializer">Data serializer.</param>
        public void AfterDeserialize(DataSerializer serializer)
        {
        }
    }
}