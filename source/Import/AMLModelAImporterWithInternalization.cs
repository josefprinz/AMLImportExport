#region copyright
	// Copyright (c) inpro Josef Prinz 2018-2021
	// author: Josef Prinz
	// date:  2021-1-18
	// license: See license.txt in this project
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Aml.Engine.CAEX;
using Aml.Engine.Services.TreeTraversal;

using SystemDataModel.Model_A;

namespace ImportExport.Import
{
    /// <summary>
    /// The implementation of this AutomationML importer is based on the design pattern 'Import with
    /// internalization'. The design pattern is characterized by the fact that the interpreter logic
    /// for the imported InternalElements and their assigned role classes is internally defined in
    /// the importer. The importer can associate role classes of InternalElements to a class in the
    /// internal data model.
    /// </summary>
    internal static partial class AMLModelAImporterWithInternalization
    {
        #region Internal Fields

        /// <summary>
        /// The unknown and not imported elements
        /// </summary>
        internal static readonly List<InternalElementType> UnknownElements = new List<InternalElementType>();

        /// <summary>
        /// The list of roles, unknown by the importer
        /// </summary>
        internal static readonly List<string> UnknownRoles = new List<string>();

        #endregion Internal Fields

        #region Internal Methods

        /// <summary>
        /// Imports the provided AutomationML document: The implementation is based on the design
        /// pattern 'import with internalization'.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>The imported project.</returns>
        internal static Project Import(CAEXDocument document)
        {
            Project importedProject = new Project();

            // List of interpreted and generated objects from the importer
            var objectList = new List<GeneratedObject<InternalElementType, SystemClassBase>>();

            // Prepare the list, containing the not interpretable roles
            UnknownRoles.Clear();
            UnknownElements.Clear();

            // Define a service for postorder tree traversal. The topology is build from the bottom
            // to the top
            var treeTraversalService = TreeTraversalService.Register();

            // be aware, that multiple instance hierarchies could exist
            foreach (var instanceHierarchy in document.CAEXFile.InstanceHierarchy)
            {
                // interpretation will first use the leaf nodes (robots)
                foreach (var internalElement in treeTraversalService.DepthFirstPostOrder(instanceHierarchy).OfType<InternalElementType>())
                {
                    // internalized role interpretation retur´ns assignable system model class
                    var types = RoleInterpreter(internalElement);
                    if (types == Type.EmptyTypes || types.Length <= 0)
                    {
                        continue;
                    }
                    if (types[0] == typeof(Robot))
                    {
                        // generate and add a new Robot-Object
                        objectList.Add(GenerateRobot(internalElement));
                    }
                    else if (types[0] == typeof(Station))
                    {
                        // generate a new Station-Object and add so far generated robot objects to
                        // the station
                        GenerateObjectWithElements<Robot>(internalElement, GenerateStation, objectList);
                    }
                    else
                    {
                        // check the last created child to decide which object type should be created
                        var lastElement = objectList.LastOrDefault();
                        if (lastElement == null)
                        {
                            continue;
                        }
                        if (lastElement.SystemObject is Station)
                        {
                            // generate a new ProductionLine-Object and add so far generated Station-Objects
                            GenerateObjectWithElements<Station>(internalElement, GenerateLine, objectList);
                        }
                        else if (lastElement.SystemObject is ProductionLine)
                        {
                            // generate a new ManufacturingSystem-Object and add so far generated ProductionLine-Objects
                            GenerateObjectWithElements<ProductionLine>(internalElement, GenerateSystem, objectList);
                        }
                    }
                }

                importedProject.Elements.AddRange(objectList.Select(obj => obj.SystemObject));
                importedProject.Name = instanceHierarchy.Name;
            }
            return importedProject;
        }

        #endregion Internal Methods

        #region Private Methods

        /// <summary>
        /// Generiert ein ProductionLine-Objekt
        /// </summary>
        /// <param name="internalElement"></param>
        /// <returns></returns>
        private static GeneratedObject<InternalElementType, SystemClassBase> GenerateLine(InternalElementType internalElement)
        {
            return new GeneratedObject<InternalElementType, SystemClassBase>(internalElement, new ProductionLine() { Name = internalElement.Name });
        }

        /// <summary>
        /// Generates a system object and assigns to the generated system object already generated
        /// child objects of the provided AllowedContentType.
        /// </summary>
        /// <typeparam name="AllowedContentType">Allowed type of child objects</typeparam>
        /// <param name="internalElement">The read-in InternalElement</param>
        /// <param name="generator">The generation method</param>
        /// <param name="objectList">The list of so far generated objects</param>
        private static void GenerateObjectWithElements<AllowedContentType>(InternalElementType internalElement,
            Func<InternalElementType, GeneratedObject<InternalElementType, SystemClassBase>> generator,
            List<GeneratedObject<InternalElementType, SystemClassBase>> objectList)
        {
            var generatedObject = generator(internalElement);
            if (objectList.Any(g => g.SystemObject is AllowedContentType))
            {
                generatedObject.SystemObject.Elements.AddRange
                    (
                        from gen in objectList
                        where gen.SystemObject is AllowedContentType
                        select gen.SystemObject
                    );

                // Removing the assigned children from the generation list
                objectList.RemoveAll(gen => gen.SystemObject is AllowedContentType);
            }
            // Adding the generated object to the generation list
            objectList.Add(generatedObject);
        }

        /// <summary>
        /// creates a Robot - Object from an imported InternalElement and assigns properties if found
        /// </summary>
        /// <param name="internalElement"></param>
        /// <returns>the generated object</returns>
        private static GeneratedObject<InternalElementType, SystemClassBase> GenerateRobot(InternalElementType internalElement)
        {
            var robot = new Robot() { Name = internalElement.Name };

            // this is more or less a guess, that elements with an assigned robot role class should
            // define this attribute. if the attribute is missing, no load is defined
            robot.Load = internalElement.Attribute["Load"]?.GetDouble() ?? 0;

            return new GeneratedObject<InternalElementType, SystemClassBase>(internalElement, robot);
        }

        /// <summary>
        /// creates a station - Object from an imported InternalElement and assigns properties if found
        /// </summary>
        /// <param name="internalElement"></param>
        /// <returns></returns>
        private static GeneratedObject<InternalElementType, SystemClassBase> GenerateStation(InternalElementType internalElement)
        {
            var station = new Station() { Name = internalElement.Name };

            var att = internalElement.Attribute["Area"];
            if (att == null)
            {
                return new GeneratedObject<InternalElementType, SystemClassBase>(internalElement, station);
            }
            var length = att.Attribute["Length"]?.GetDouble() ?? 0;
            var width = att.Attribute["Width"]?.GetDouble() ?? 0;

            station.Area = new Tuple<double, double>(length, width);

            return new GeneratedObject<InternalElementType, SystemClassBase>(internalElement, station);
        }

        /// <summary>
        /// creates a ManufacturingSystem - Object from an imported InternalElement
        /// </summary>
        /// <param name="internalElement"></param>
        /// <returns></returns>
        private static GeneratedObject<InternalElementType, SystemClassBase> GenerateSystem(InternalElementType internalElement)
        {
            return new GeneratedObject<InternalElementType, SystemClassBase>(internalElement, new ManufacturingSystem() { Name = internalElement.Name });
        }

        /// <summary>
        /// Roles interpretation function
        ///
        /// In this method, the internalized impoter design pattern is implemented. The
        /// interpretation function uses internal knowledgem because the interpretable role classes
        /// are all explicit specified in this method. If roles are not known the according elements
        /// are skipped and the unknown roles are recorded.
        /// </summary>
        /// <param name="internalElement">InternalElement object to be interpreted</param>
        /// <returns>array of possible system data types to be generated for this internal Element</returns>
        private static Type[] RoleInterpreter(InternalElementType internalElement)
        {
            // interpretation without roles is not possible. These elements are bot recorded.
            if (internalElement.RoleRequirements.Count == 0)
            {
                UnknownElements.Add(internalElement);
                return Type.EmptyTypes;
            }

            // check, which role assignments are defined

            // please note the second parameter 'regardInheritance' in the method call, which is set
            // to true. this ensures, that a possible more specific class, used by an excporter cam
            // still be interpreted, even if the importer has no knowledge about this class but only
            // knows a more generic base class.
            if (internalElement.HasRoleClassReference(
                roleReference: @"AutomationMLDMIRoleClassLib/DiscManufacturingEquipment/Robot",
                regardInheritance: true))
            {
                // Role is interpreted as 'Robot'
                return new Type[] { typeof(Robot) };
            }

            if (internalElement.HasRoleClassReference(
                roleReference: @"AutomationMLExtendedRoleClassLib/WorkCell",
                regardInheritance: false))
            {
                // Role is interpreted as 'Station'
                return new Type[] { typeof(Station) };
            }

            if (internalElement.HasRoleClassReference(
                roleReference: @"AutomationMLBaseRoleClassLib/AutomationMLBaseRole/Structure/ResourceStructure",
                regardInheritance: false))
            {
                // Role is ambiguously interpreted as 'ManufacturingSystem' or 'ProductionLine
                return new Type[] { typeof(ManufacturingSystem), typeof(ProductionLine) };
            }

            // other internal elements are not known and will not be imported these roles can be add
            // to the errorlist
            foreach (var role in internalElement.RoleRequirements)
            {
                if (!UnknownRoles.Contains(role.RoleReference))
                    UnknownRoles.Add(role.RoleReference);
            }
            UnknownElements.Add(internalElement);
            return Type.EmptyTypes;
        }

        #endregion Private Methods
    }
}