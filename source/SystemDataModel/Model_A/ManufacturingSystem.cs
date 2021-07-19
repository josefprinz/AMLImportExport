#region copyright
	// Copyright (c) inpro Josef Prinz 2018-2021
	// author: Josef Prinz
	// date:  2021-1-18
	// license: See license.txt in this project
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SystemDataModel.Model_A
{
    /// <summary>
    /// ManufacturingSystem class
    /// </summary>
    [DataContract]
    public class ManufacturingSystem : SystemClassBase
    {
        #region Private Fields

        private static readonly Random r = new Random(1171);
        private static int _counter;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Elements of a Manufacturing System. Overriding allows attribution with own
        /// DataMember Name
        /// </summary>
        [DataMember(Name = "Line", Order = 3)]
        public override List<SystemClassBase> Elements => base.Elements;

        /// <summary>
        /// TypeCast for element enumeration
        /// </summary>
        public IEnumerable<ProductionLine> Lines => Elements.OfType<ProductionLine>();

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Example data generator
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static ManufacturingSystem Generate(int count)
        {
            ManufacturingSystem system = new ManufacturingSystem();
            GenerateID(system, ref _counter);

            for (int i = 0; i < count; i++)
            {
                ProductionLine line = ProductionLine.Generate(r.Next(1, ProjectDataGeneratorSettings.MaxStation));
                system.Elements.Add(line);
                line.Name = "Line_" + (i + 1);
            }
            return system;
        }

        #endregion Public Methods
    }
}