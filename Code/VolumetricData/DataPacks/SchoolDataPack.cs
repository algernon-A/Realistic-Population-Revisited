// <copyright file="SchoolDataPack.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    /// <summary>
    /// School calculation data pack - provides parameters for calculating building populations.
    /// </summary>
    internal class SchoolDataPack : DataPack
    {
        // School level.
        private ItemClass.Level _level;

        // Employment calculation arrays.
        private int[] _baseWorkers;
        private int[] _perWorker;

        // Cost and maintenance fields.
        private int _baseCost;
        private int _costPer;
        private int _baseMaint;
        private int _maintPer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchoolDataPack"/> class.
        /// </summary>
        /// <param name="version">Datapack version.</param>
        /// <param name="level">School level.</param>
        /// <param name="baseCost">Base construction cost.</param>
        /// <param name="costPer">Additional construction cost per student.</param>
        /// <param name="baseMaint">Base maintenance cost.</param>
        /// <param name="maintPer">Additional maintenance cost per student.</param>
        /// <param name="baseWorkers">Base workers array.</param>
        /// <param name="perWorker">Students-per-additional-worker array.</param>
        internal SchoolDataPack(DataVersion version, ItemClass.Level level, int baseCost, int costPer, int baseMaint, int maintPer, int[] baseWorkers, int[] perWorker)
            : base(version)
        {
            _level = level;
            _baseCost = baseCost;
            _costPer = costPer;
            _baseMaint = baseMaint;
            _maintPer = maintPer;
            _baseWorkers = baseWorkers;
            _perWorker = perWorker;
        }

        /// <summary>
        /// Gets the base construction cost.
        /// </summary>
        internal int BaseCost => _baseCost;

        /// <summary>
        /// Gets the additional construction cost per student.
        /// </summary>
        internal int CostPer => _costPer;

        /// <summary>
        /// Gets the base maintenance cost.
        /// </summary>
        internal int BaseMaint => _baseMaint;

        /// <summary>
        /// Gets the additional maintenance cost per student.
        /// </summary>
        internal int MaintPer => _maintPer;

        /// <summary>
        /// Gets the base workers array.
        /// </summary>
        internal int[] BaseWorkers => _baseWorkers;

        /// <summary>
        /// Gets the Students-per-additional-worker array.
        /// </summary>
        internal int[] PerWorker => _perWorker;

        /// <summary>
        /// Gets the school level that this pack is applicable to.
        /// </summary>
        internal ItemClass.Level Level => _level;
    }
}