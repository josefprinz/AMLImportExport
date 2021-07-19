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
    /// Station class
    /// </summary>
    [DataContract]
    public class Station : SystemClassBase
    {
        #region Private Fields

        private static readonly Random Rand = new Random();
        private static int _counter;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// elements of a station
        /// </summary>
        [DataMember(Name = "Robot", Order = 3)]
        public override List<SystemClassBase> Elements => base.Elements;

        /// <summary>
        /// area
        /// </summary>
        [DataMember(Order = 3)]
        public Tuple<double, double> Area { get; set; }

        /// <summary>
        /// element enumeration
        /// </summary>
        public IEnumerable<Robot> Robots => Elements.OfType<Robot>();

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// exampe data generator
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Station Generate(int count)
        {
            Station station = new Station();
            GenerateID(station, ref _counter);

            station.Area = new Tuple<double, double>(Rand.Next(100, 120), Rand.Next(100, 120));

            for (int i = 0; i < count; i++)
            {
                Robot robot = Robot.Generate(0);
                station.Elements.Add(robot);
                robot.Name = "Robot_" + (i + 1);
            }

            return station;
        }

        #endregion Public Methods
    }
}