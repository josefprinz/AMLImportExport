#region copyright
	// Copyright (c) inpro Josef Prinz 2018-2021
	// author: Josef Prinz
	// date:  2021-1-18
	// license: See license.txt in this project
#endregion

using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace SystemDataModel.Model_A
{
    /// <summary>
    /// baseclass used for all data classes of the system data model
    /// </summary>
    [DataContract]
    public abstract class SystemClassBase
    {
        #region Private Fields

        private List<SystemClassBase> _elements;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// sub elements of a class
        /// </summary>
        public virtual List<SystemClassBase> Elements => _elements ?? (_elements = new List<SystemClassBase>());

        /// <summary>
        /// ID-property
        /// </summary>
        [DataMember(Order = 2)]
        public string ID { get; set; }

        /// <summary>
        /// Name property
        /// </summary>
        [DataMember(Order = 1)]
        public string Name { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// ID Generator
        /// </summary>
        /// <param name="element"></param>
        /// <param name="index"></param>
        public static void GenerateID(SystemClassBase element, ref int index)
        {
            char c = element.GetType().Name[0];
            index++;
            element.ID = c + index.ToString(CultureInfo.InvariantCulture);
        }

        #endregion Public Methods
    }
}