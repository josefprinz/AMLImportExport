#region copyright
	// Copyright (c) inpro Josef Prinz 2018-2021
	// author: Josef Prinz
	// date:  2021-1-18
	// license: See license.txt in this project
#endregion

using System;
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
    /// internalization'. The design pattern is characterized by the fact that the assignment logic
    /// of which role classes are assigned to a generated AutomationML object is completely realized
    /// within this exporter.
    /// </summary>
    internal static class AMLModelAExporterWithInternalization
    {
        #region Internal Fields

        /// <summary>
        /// The error messages, created from the exporter.
        /// </summary>
        internal static readonly List<string> ErrorMessages = new List<string>();

        #endregion Internal Fields

        #region Private Fields

        /// <summary>
        /// The role class dictionary used to map a source object class to an AutomationML role class
        /// </summary>
        private static readonly Dictionary<Type, RoleFamilyType> _roleClassDictionary = new Dictionary<Type, RoleFamilyType>();

        #endregion Private Fields              

        #region Internal Methods

        /// <summary>
        /// Generates an AutomationML Document for the provided project. Only a few query
        /// definitions and a single execution statement are needed to generate the complete
        /// instance hierarchy for the hierarchical system data model. The 'internalization' design
        /// pattern you see in the method "PrepareRoleClassDictionary" methods where the RoleClass
        /// associations are defined. The associations are then created in each
        /// "GenerateInternalElement" method.
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

            // sets the instance hierarchy containing the generated internal elements.
            InstanceHierarchyType ih = outputDocument.CAEXFile.InstanceHierarchy[0];

            // create the queries and projections to InternalElements
            var systemQuery = from ManufacturingSystem system in project.Elements
                              select new { source = system, ie = system.GenerateInternalElement(ih, sourceMetaData) };

            var lineQuery = from system in systemQuery
                            from ProductionLine line in system.source.Elements
                            select new { source = line, ie = line.GenerateInternalElement(system.ie, sourceMetaData) };

            var stationQuery = from line in lineQuery
                               from Station station in line.source.Elements
                               select new { source = station, ie = station.GenerateInternalElement(line.ie, sourceMetaData) };

            var robotQuery = from station in stationQuery
                             from Robot robot in station.source.Elements
                             select new { source = robot, ie = robot.GenerateInternalElement(station.ie, sourceMetaData) };

            // materialization of queries and their projections
            robotQuery.ToList();

            // return result
            return outputDocument;
        }

        #endregion Internal Methods

        #region Private Methods

        private static void AddRoleToClassDictionary(CAEXDocument amlDocument, string roleClassPath, Type sourceObjectClass)
        {
            // try to get the role class from the aml document
            CAEXObject amlClass = amlDocument.FindByPath(roleClassPath, resolveAlias: true);

            if (amlClass is RoleFamilyType roleClass)
            {
                _roleClassDictionary[sourceObjectClass] = roleClass;
            }
            else
            {
                NotifyError($"{roleClassPath} not found in AutomationML document.");
            }
        }

        /// <summary>
        /// Assigns optional meta data to the generated internal Element
        /// </summary>
        /// <param name="internalElement">The internal element.</param>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="sourceMetaData">The source meta data.</param>
        /// <param name="setChangeMode">if set to <c>true</c> [set change mode].</param>
        /// <param name="assignSoid">if set to <c>true</c> [assign soid].</param>
        private static void AssignMetaData(this InternalElementType internalElement, SystemClassBase sourceObject, SourceDocumentInformationType sourceMetaData, bool setChangeMode, bool assignSoid)
        {
            if (setChangeMode)
            {
                internalElement.ChangeMode = ChangeMode.Create;
            }

            if (assignSoid)
            {
                internalElement.AssignSoid(sourceObject, sourceMetaData);
            }
        }

        /// <summary>
        /// Assigns the source object id to the generated internal Element.
        /// </summary>
        /// <param name="internalElment">The internal elment.</param>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="sourceMetaData">The source system meta data.</param>
        private static void AssignSoid(this InternalElementType internalElment, SystemClassBase sourceObject, SourceDocumentInformationType sourceMetaData)
        {
            SourceObjectInformationType objectInfo = internalElment.SourceObjectInformation.Append();
            objectInfo.OriginID = sourceMetaData.OriginID;
            objectInfo.SourceObjID = sourceObject.ID;
        }

        /// <summary>
        /// Internalized generation method for an object of class <see cref="ManufacturingSystem"/>
        /// </summary>
        /// <param name="system">Source instance</param>
        /// <param name="parent">Parent object in the instance hierarchy</param>
        /// <param name="sourceMetaData">Information about the data source</param>
        /// <param name="setChangeMode">Optional generation of change information</param>
        /// <param name="assignSoid">
        /// Optional generation of an ID mapping between IE GUID and system object ID
        /// </param>
        /// <returns>the generated InternalElement</returns>
        private static InternalElementType GenerateInternalElement(this ManufacturingSystem system, InstanceHierarchyType parent, SourceDocumentInformationType sourceMetaData, bool setChangeMode = false, bool assignSoid = false)
        {
            InternalElementType internalElement = parent.InternalElement.Append(system.Name);

            // alternatively a direct Assignment without existence check can be done 
            // internalElement.AddRoleClassReference(@"AutomationMLBaseRoleClassLib/AutomationMLBaseRole/Structure/ResourceStructure");

            // assignment of existing roleclass to the InternalElement
            internalElement.RoleRequirements.Append().RoleClass = _roleClassDictionary[typeof(ManufacturingSystem)];
            internalElement.AssignMetaData(system, sourceMetaData, setChangeMode, assignSoid);

            return internalElement;
        }

        /// <summary>
        /// Internalized generation method for an object of class <see cref="ProductionLine"/>
        /// </summary>
        /// <param name="line">Source instance</param>
        /// <param name="parent">Parent object in the instance hierarchy</param>
        /// <param name="sourceMetaData">Information about the data source</param>
        /// <param name="setChangeMode">Optional generation of change information</param>
        /// <param name="assignSoid">
        /// Optional generation of an ID mapping between IE GUID and system object ID
        /// </param>
        /// <returns>the generated InternalElement</returns>
        private static InternalElementType GenerateInternalElement(this ProductionLine line, InternalElementType parent, SourceDocumentInformationType sourceMetaData, bool setChangeMode = false, bool assignSoid = false)
        {
            InternalElementType internalElement = parent.AddNewInternalElement(line.Name);

            // alternatively a direct Assignment without existence check can be done
            // internalElement.AddRoleClassReference(@"AutomationMLBaseRoleClassLib/AutomationMLBaseRole/Structure/ResourceStructure");

            // assignment of existing roleclass to the InternalElement
            internalElement.RoleRequirements.Append().RoleClass = _roleClassDictionary[typeof(ProductionLine)];

            internalElement.AssignMetaData(line, sourceMetaData, setChangeMode, assignSoid);
            return internalElement;
        }

        /// <summary>
        /// Internalisierte Generierungsmethode für ein Objekt der Klasse <see cref="Station"/>
        /// </summary>
        /// <param name="station">Datenquelle</param>
        /// <param name="parent">Parent object in the instance hierarchy</param>
        /// <param name="sourceMetaData">Information about the data source</param>
        /// <param name="setChangeMode">Optional generation of change information</param>
        /// <param name="assignSoid">
        /// Optional generation of an ID mapping between IE GUID and system object ID
        /// </param>
        /// <returns>the generated InternalElement</returns>
        private static InternalElementType GenerateInternalElement(this Station station, InternalElementType parent, SourceDocumentInformationType sourceMetaData, bool setChangeMode = false, bool assignSoid = false)
        {
            InternalElementType internalElement = parent.AddNewInternalElement(station.Name);

            // alternatively a direct Assignment without existence check can be done
            // internalElement.AddRoleClassReference( @"AutomationMLExtendedRoleClassLib/WorkCell");

            // assignment of existing roleclass to the InternalElement
            internalElement.RoleRequirements.Append().RoleClass = _roleClassDictionary[typeof(Station)];

            internalElement.AssignMetaData(station, sourceMetaData, setChangeMode, assignSoid);

            AttributeType at = internalElement.Attribute.Append("Area");
            at.SetAttributeValue("Length", attValue: station.Area.Item1, defaultValue: 0, description: "", attUnit: "m");
            at.SetAttributeValue("Width", attValue: station.Area.Item2, defaultValue: 0, description: "", attUnit: "m");
            return internalElement;
        }

        /// <summary>
        /// Internalisierte Generierungsmethode für ein Objekt der Klasse <see cref="Robot"/>
        /// </summary>
        /// <param name="robot">Datenquelle</param>
        /// <param name="parent">Parent object in the instance hierarchy</param>
        /// <param name="sourceMetaData">Information about the data source</param>
        /// <param name="setChangeMode">Optional generation of change information</param>
        /// <param name="assignSoid">
        /// Optional generation of an ID mapping between IE GUID and system object ID
        /// </param>
        /// <returns>the generated InternalElement</returns>
        private static InternalElementType GenerateInternalElement(this Robot robot, InternalElementType parent,
                    SourceDocumentInformationType sourceMetaData, bool setChangeMode = false, bool assignSoid = false)
        {
            InternalElementType internalElement = parent.AddNewInternalElement(robot.Name);

            // alternatively a direct Assignment without existence check can be done 
            // internalElement.AddRoleClassReference(@"UserDefinedRoleClassLib/MyRobot");

            // assignment of existing roleclass to the InternalElement
            internalElement.RoleRequirements.Append().RoleClass = _roleClassDictionary[typeof(Robot)];

            internalElement.AssignMetaData(robot, sourceMetaData, setChangeMode, assignSoid);
            internalElement.SetAttributeValue("Load", robot.Load, defaultValue: 0, "", attUnit: "kg");

            return internalElement;
        }

        /// <summary>
        /// Notifies the error.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        private static void NotifyError(string errorMessage)
        {
            ErrorMessages.Add(errorMessage);
        }

        /// <summary>
        /// Prepares the output document. Imports the required AutomationML libraries and sets the
        /// source document information.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="sourceMetaData">The source meta data.</param>
        /// <returns></returns>
        private static CAEXDocument PrepareOutputDocument(Project project, out SourceDocumentInformationType sourceMetaData)
        {
            // Construction of the file name for the AutomationML file containing the class
            // libraries RoleClasses, InterfaceClasses and AttributeTypes
            string dir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string modelFile = Path.Combine(dir, "SystemDataModel/AMLFiles/Model_A_Classes.aml");
            CAEXDocument classDocument = CAEXDocument.LoadFromFile(modelFile);

            // Creates an empty AutomationML document for the output of the generated AutomationML
            // objects and initializes the CAEXDocument with the role class libraries.
            CAEXDocument outPutDocument = CAEXDocument.New_CAEXDocument();

            // create some Meta data defining the origin of the source data
            sourceMetaData = outPutDocument.CAEXFile.SourceDocumentInformation.Append();
            sourceMetaData.OriginProjectID = project.ID;
            sourceMetaData.OriginProjectTitle = project.Name;
            sourceMetaData.OriginID = "Exporter Model A";
            sourceMetaData.OriginName = "AMLModelAExporterWithInternalization";

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

                    default:
                        break;
                }
            }

            // assure, that all needed role classes are imported
            PrepareRoleClassDictionary(outPutDocument);

            // add the instance hierarchy
            outPutDocument.CAEXFile.InstanceHierarchy.Append(project.Name);

            return outPutDocument;
        }

        /// <summary>
        /// Prepares the role class dictionary. It is assured, that only existing role classes from
        /// the document are used for role class assignments to exported source objects.
        ///
        /// In this method, the internalized design pattern is implemented. The role classes to be
        /// assigned to an object are defined internally here. The role classes are identified by
        /// their fully qualified CAEX Path.
        /// </summary>
        /// <param name="amlDocument">The aml document.</param>
        private static void PrepareRoleClassDictionary(CAEXDocument amlDocument)
        {
            _roleClassDictionary.Clear();
            AddRoleToClassDictionary(amlDocument, @"AutomationMLBaseRoleClassLib/AutomationMLBaseRole/Structure/ResourceStructure", typeof(ManufacturingSystem));
            AddRoleToClassDictionary(amlDocument, @"AutomationMLBaseRoleClassLib/AutomationMLBaseRole/Structure/ResourceStructure", typeof(ProductionLine));
            AddRoleToClassDictionary(amlDocument, @"AutomationMLExtendedRoleClassLib/WorkCell", typeof(Station));
            AddRoleToClassDictionary(amlDocument, @"UserDefinedRoleClassLib/MyRobot", typeof(Robot));
        }

        #endregion Private Methods
    }
}