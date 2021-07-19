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
    /// ProductionLine class
    /// </summary>
    [DataContract]
    public class ProductionLine : SystemClassBase
    {
        #region Private Fields

        private static readonly Random r = new Random(13);
        private static int _counter;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Elements of a Production line. Overriding allows attribution with own
        /// DataMember Name
        /// </summary>
        [DataMember(Name = "Station", Order = 3)]
        public override List<SystemClassBase> Elements => base.Elements;

        /// <summary>
        /// Typecast for elements enumeration
        /// </summary>
        public IEnumerable<Station> Stations => Elements.OfType<Station>();

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// example data generator
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static ProductionLine Generate(int count)
        {
            ProductionLine line = new ProductionLine();
            GenerateID(line, ref _counter);

            for (int i = 0; i < count; i++)
            {
                Station station = Station.Generate(r.Next(1, ProjectDataGeneratorSettings.MaxRobot));
                line.Elements.Add(station);
                station.Name = "Station_" + (i + 1);
            }

            return line;
        }

        #endregion Public Methods
    }
}