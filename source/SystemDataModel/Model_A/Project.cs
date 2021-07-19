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
    /// Project class
    /// </summary>
    [DataContract]
    public class Project : SystemClassBase
    {
        #region Private Fields

        private static readonly Random r = new Random(7);
        private static int _counter;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Elements of a project
        /// </summary>
        [DataMember(Name = "ManufacturingSystem", Order = 3)]
        public override List<SystemClassBase> Elements => base.Elements;

        /// <summary>
        /// element enumeration
        /// </summary>
        public IEnumerable<ManufacturingSystem> Systems => Elements.OfType<ManufacturingSystem>();

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// example data generator
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Project Generate(string name)
        {
            Project project = new Project();

            _counter++;
            project.ID = "PRJ-" + _counter;
            project.Name = name;

            ManufacturingSystem system = ManufacturingSystem.Generate(r.Next(1, ProjectDataGeneratorSettings.MaxLine));
            system.Name = "Assembly";

            project.Elements.Add(system);
            return project;
        }

        #endregion Public Methods
    }
}