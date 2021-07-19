#region copyright
	// Copyright (c) inpro Josef Prinz 2018-2021
	// author: Josef Prinz
	// date:  2021-1-18
	// license: See license.txt in this project
#endregion

using Aml.Engine.CAEX;
using SystemDataModel.Model_A;

namespace ImportExport.Import
{
    /// <summary>
    /// Helper class to save the generated association between the imported AutomationML object and
    /// the generated system object during import
    /// </summary>
    /// <typeparam name="T1">The type of the AutomationML object</typeparam>
    /// <typeparam name="T2">The type of the System data object.</typeparam>
    internal class GeneratedObject<T1, T2> where T1 : CAEXObject where T2: SystemClassBase
    {
        #region Public Constructors

        public GeneratedObject(T1 caexObject, T2 systemObject)
        {
            CaexObject = caexObject;
            SystemObject = systemObject;
        }

        #endregion Public Constructors

        #region Public Properties

        public T1 CaexObject { get; set; }

        public T2 SystemObject { get; set; }

        #endregion Public Properties
    }
}