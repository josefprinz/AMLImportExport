#region copyright
	// Copyright (c) inpro Josef Prinz 2018-2021
	// author: Josef Prinz
	// date:  2021-1-18
	// license: See license.txt in this project
#endregion

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using SystemDataModel.Model_A;

namespace ImportExport.Export
{
	/// <summary>
	/// The implementation of this AutomationML exporter is based on the design pattern 'Export with
	/// externalization'. The design pattern is characterized by the fact that the assignment logic
	/// of which role classes are assigned to a generated AutomationML object is externaly defined
	/// by SystemUnitClasses where each SystemUnitClass represents a source data model class.
	/// </summary>
	internal static class AMLModelAExporterWithExternalization
	{
		#region Internal Fields

		/// <summary>
		/// The error messages, created from the exporter.
		/// </summary>
		internal static readonly List<string> ErrorMessages = new List<string>();

		#endregion Internal Fields

		#region Private Fields

		/// <summary>
		/// The SystemUnit class dictionary is used to map a source object class to an AutomationML
		/// system unit class
		/// </summary>
		private static readonly Dictionary<string, SystemUnitFamilyType> _systemUnitClassDictionary = new Dictionary<string, SystemUnitFamilyType>();

		#endregion Private Fields

		#region Internal Methods

		/// <summary>
		/// Generates an AutomationML Document for the provided project. Only a few query
		/// definitions and a single execution statement are needed to generate the complete
		/// instance hierarchy for the hierarchical system data model. The 'externalization' design
		/// pattern you see in the method "PrepareSystemUnitClassDictionary" where the Class
		/// mappings between SystemUnitClasses and internal source model classes are defined. The
		/// associations are used to create InternalElements by class instantiation which is done in
		/// the "GenerateInternalElementWithInstantiation" methods.
		/// </summary>
		/// <param name="project">The project to be exported.</param>
		/// <returns>The generated AutomationML document.</returns>
		internal static CAEXDocument Generate(Project project)
		{
			// first, clear the error messages
			ErrorMessages.Clear();

			// prepares the output document
			CAEXDocument outputDocument = PrepareOutputDocument(project, out SourceDocumentInformationType sourceMetaData);

			// check for errors
			if (ErrorMessages.Count > 0 || outputDocument == null)
			{
				return null;
			}

			// prepares the systemunit class mapping table
			PrepareSystemUnitClassDictionary(outputDocument);

			// sets the instance hierarchy containing the generated internal elements.
			InstanceHierarchyType ih = outputDocument.CAEXFile.InstanceHierarchy[0];

			// defining the queries with projections. The projections generate InternalElements with Instantiation
			var systemQuery = from system in project.Elements
							  select new { source = system, ie = system.GenerateInternalElementWithInstantiation(ih, sourceMetaData) };

			var lineQuery = from system in systemQuery
							from line in system.source.Elements
							select new { source = line, ie = line.GenerateInternalElementWithInstantiation(system.ie, sourceMetaData) };

			var stationQuery = from line in lineQuery
							   from Station station in line.source.Elements
							   select new { source = station, ie = station.GenerateInternalElementWithInstantiation(line.ie, sourceMetaData) };

			var robotQuery = from station in stationQuery
							 from Robot robot in station.source.Elements
							 select new { source = robot, ie = robot.GenerateInternalElementWithInstantiation(station.ie, sourceMetaData) };

			// materialization of queries and their projections
			robotQuery.ToList();

			// return result
			return outputDocument;
		}

		#endregion Internal Methods

		#region Private Methods

		/// <summary>
		/// Externalized generation method for instance generation for a source object and the
		/// associated SystemUnitClass
		/// </summary>
		/// <param name="sourceObject">source data object</param>
		/// <param name="parent">parent in Instancehierarchy</param>
		/// <param name="sourceMetaData">source data information</param>
		/// <returns>the generated InternalElement</returns>
		private static InternalElementType GenerateInternalElementWithInstantiation(this SystemClassBase sourceObject, IInternalElementContainer parent, SourceDocumentInformationType sourceMetaData)
		{
			string key = sourceObject.GetType().Name;
			InternalElementType internalElement = null;

			// class name and system unit class name are different a mapping table could be used to
			// define the mappings
			if (key == nameof(ProductionLine))
			{
				key = "Line";
			}

			if (_systemUnitClassDictionary.TryGetValue(key, out SystemUnitFamilyType systemUnitClass))
			{
				// class instantiation
				internalElement = systemUnitClass.CreateClassInstance();
				internalElement.Name = sourceObject.Name;

				// add source information
				SourceObjectInformationType sourceObjectInformation = internalElement.SourceObjectInformation.Append();
				sourceObjectInformation.OriginID = sourceMetaData.OriginID;
				sourceObjectInformation.SourceObjID = sourceObject.ID;

				// build hierarchy relation
				parent.InternalElement.Insert(internalElement);
			}
			return internalElement;
		}

		/// <summary>
		/// Externalized generation method for instance generation for a source object <see cref="Station"/>
		/// </summary>
		/// <param name="station">source data object</param>
		/// <param name="parent">parent in Instancehierarchy</param>
		/// <param name="sourceMetaData">source data information</param>
		/// <returns>the generated InternalElement</returns>
		private static InternalElementType GenerateInternalElementWithInstantiation(this Station station, InternalElementType parent, SourceDocumentInformationType sourceMetaData)
		{
			// call the generic instantiation method
			InternalElementType ie = ((SystemClassBase)station).GenerateInternalElementWithInstantiation(parent, sourceMetaData);

			// add attribute values
			if (ie != null)
			{
				AttributeType at = ie.Attribute["Area"];

				at.Attribute["Length"].SetDouble(station.Area.Item1);
				at.Attribute["Width"].SetDouble(station.Area.Item2);
			}

			return ie;
		}

		/// <summary>
		/// Externalized generation method for instance generation for a source object <see cref="Robot"/>
		/// </summary>
		/// <param name="robot">source data object</param>
		/// <param name="parent">parent in Instancehierarchy</param>
		/// <param name="sourceMetaData">source data information</param>
		/// <returns>the generated InternalElement</returns>
		private static InternalElementType GenerateInternalElementWithInstantiation
			(this Robot robot, InternalElementType parent, SourceDocumentInformationType sourceMetaData)
		{
			// call the generic instantiation method
			InternalElementType ie = ((SystemClassBase)robot).GenerateInternalElementWithInstantiation(parent, sourceMetaData);

			// add attribute values
			if (ie != null)
			{
				ie.Attribute["Load"].SetDouble(robot.Load);
			}

			return ie;
		}

		/// <summary>
		/// Prepares the output document.
		/// </summary>
		/// <param name="project">The project.</param>
		/// <param name="sourceMetaData">The source meta data.</param>
		/// <returns></returns>
		private static CAEXDocument PrepareOutputDocument(Project project, out SourceDocumentInformationType sourceMetaData)
		{
			// Construction of the file name for the AutomationML file containing the class
			// libraries SystemUnitClasses, RoleClasses, InterfaceClasses and AttributeTypes
			string dir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string modelFile = Path.Combine(dir, "SystemDataModel/AMLFiles/Model_A.aml");
			CAEXDocument classDocument = CAEXDocument.LoadFromFile(modelFile);

			// Creates an empty AutomationML document for the output of the generated AutomationML
			// objects and initializes the CAEXDocument with the role class libraries.
			CAEXDocument outPutDocument = CAEXDocument.New_CAEXDocument();

			// create some Meta data defining the origin of the source data
			sourceMetaData = outPutDocument.CAEXFile.SourceDocumentInformation.Append();
			sourceMetaData.OriginProjectID = project.ID;
			sourceMetaData.OriginProjectTitle = project.Name;
			sourceMetaData.OriginID = "Exporter Model A";
			sourceMetaData.OriginName = "AMLModelAExporterWithExternalization";

			// imports all libraries from the class document to the output document
			foreach (CAEXObject library in classDocument.CAEXFile)
			{
				switch (library)
				{
					case AttributeTypeLibType aLib:
						outPutDocument.CAEXFile.Import_AttributeTypeLib(aLib);
						break;

					case InterfaceClassLibType iLib:
						outPutDocument.CAEXFile.Import_InterfaceClassLibHierarchy(iLib);
						break;

					case RoleClassLibType rLib:
						outPutDocument.CAEXFile.Import_RoleClassLibHierarchy(rLib);
						break;

					case SystemUnitClassLibType sLib:
						outPutDocument.CAEXFile.Import_SystemUnitClassLibHierarchy(sLib);
						break;

					default:
						break;
				}
			}

			// assure, that all needed role classes are imported PrepareRoleClassDictionary (outPutDocument);

			// add the instance hierarchy
			outPutDocument.CAEXFile.InstanceHierarchy.Append(project.Name);

			return outPutDocument;
		}

		/// <summary>
		/// Prepares the SystemUnitClass dictionary. The SystemUnitClass Dictionary contains
		/// mappings between the classes of the source data model and the externaly modeled
		/// SystemUnitClasses, which are used to create the AutomationML objects by class instanciation.
		///
		/// In this method, the externalized design pattern is implemented.
		/// </summary>
		/// <param name="document">
		/// The AutomationML document which contains the SystemUnitClass library.
		/// </param>
		private static void PrepareSystemUnitClassDictionary(CAEXDocument document)
		{
			_systemUnitClassDictionary.Clear();
			foreach (SystemUnitClassLibType systemUnitClassLib in document.CAEXFile.SystemUnitClassLib)
			{
				foreach (SystemUnitFamilyType systemUnitClass in systemUnitClassLib.Descendants<SystemUnitFamilyType>())
				{
					// the class name and the type name of a source object should match
					_systemUnitClassDictionary.Add(systemUnitClass.Name, systemUnitClass);
				}
			}
		}

		#endregion Private Methods
	}
}