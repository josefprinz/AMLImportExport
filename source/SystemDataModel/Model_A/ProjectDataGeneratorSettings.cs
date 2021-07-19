#region copyright
	// Copyright (c) inpro Josef Prinz 2018-2021
	// author: Josef Prinz
	// date:  2021-1-18
	// license: See license.txt in this project
#endregion

namespace SystemDataModel.Model_A
{
    /// <summary>
    /// configuration of data generator
    /// </summary>
    public class ProjectDataGeneratorSettings
    {
        #region Public Properties

        /// <summary>
        /// Upper limit for the generation of ProductionLine objects
        /// </summary>
        public static int MaxLine { get; set; }

        /// <summary>
        /// Upper limit for the generation of robot objects
        /// </summary>
        public static int MaxRobot { get; set; }

        /// <summary>
        /// Upper limit for the generation of station objects
        /// </summary>
        public static int MaxStation { get; set; }

        /// <summary>
        /// Projectname
        /// </summary>
        public string ProjectName { get; set; }

        #endregion Public Properties
    }
}