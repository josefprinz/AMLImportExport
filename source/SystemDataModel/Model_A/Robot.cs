#region copyright
	// Copyright (c) inpro Josef Prinz 2018-2021
	// author: Josef Prinz
	// date:  2021-1-18
	// license: See license.txt in this project
#endregion

using System;
using System.Runtime.Serialization;

namespace SystemDataModel.Model_A
{
    /// <summary>
    /// Robot class
    /// </summary>
    [DataContract]
    public class Robot : SystemClassBase
    {
        #region Private Fields

        private static readonly Random Rand = new Random();
        private static int _counter;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// load property
        /// </summary>
        [DataMember(Order = 3)]
        public double Load { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// example data generator
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Robot Generate(int count)
        {
            Robot robot = new Robot();
            GenerateID(robot, ref _counter);

            robot.Load = Rand.Next(50, 500);
            return robot;
        }

        #endregion Public Methods
    }
}