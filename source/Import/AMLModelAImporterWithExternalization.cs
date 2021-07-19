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
using Aml.Engine.Services.TreeTraversal;
using SystemDataModel.Model_A;

namespace ImportExport.Import
{
    /// <summary>
    /// The implementation of this AutomationML importer is based on the design pattern 'Import with
    /// externalization'. The design pattern is characterized by the fact that the interpreter logic
    /// for the imported InternalElements and their assigned role classes is externaly defined by
    /// SystemUnitClasses where each SystemUnitClass upports a specific role and can be mapped to a
    /// class in the internal data model.
    /// </summary>
    internal static class AMLModelAImporterWithExternalization
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

        #region Private Fields

        /// <summary>
        /// table to access a systemUnitclass which supports a specific
        /// </summary>
        private static CAEXDocument _externalClassModelDocument;

        #endregion Private Fields

        #region Internal Methods

        /// <summary>
        /// Imports the provided AutomationML document: The implementation is based on the design
        /// pattern 'import with externalization'.
        /// </summary>
        /// <param name="document">The dcument.</param>
        /// <returns>The imported project.</returns>
        internal static Project Import(CAEXDocument document)
        {
            Project importedProject = new Project();

            // loads the external class model document
            LoadExternalClassModelDocument();

            // List of interpreted and generated objects from the importer
            var objectList = new List<SystemClassBase>();

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
                    var supportedClasses = RoleInterpreter(internalElement);
                    if (supportedClasses == null)
                    {
                        continue;
                    }

                    GenerateData(internalElement, supportedClasses, objectList);
                }

                importedProject.Elements.AddRange(objectList);
                importedProject.Name = instanceHierarchy.Name;
            }
            return importedProject;
        }

        /// <summary>
        /// Sets the attribute which is retrieved from an InternalElement to a generated system
        /// Object. Please note, that the attribute Name and the property name are required to be
        /// the same. If Name-differences exist, this could be solved by mapping objects.
        /// </summary>
        /// <param name="systemDataObject">The system data object.</param>
        /// <param name="attribute">The attribute.</param>
        internal static void SetAttribute(this SystemClassBase systemDataObject, AttributeType attribute)
        {
            switch (systemDataObject)
            {
                case Robot robot:
                    if (attribute.Name == nameof(Robot.Load))
                    {
                        robot.Load = attribute.GetDouble();
                    }
                    break;

                case Station station:
                    if (attribute.Name == nameof(Station.Area))
                    {
                        var length = attribute.Attribute["Length"]?.GetDouble();
                        var width = attribute.Attribute["Width"]?.GetDouble();
                        if (length.HasValue && width.HasValue )
                        station.Area = new System.Tuple<double, double>( length.Value, width.Value); 
                    }
                    break;
            }
        }

        #endregion Internal Methods

        #region Private Methods

        /// <summary>
        /// Creates a system data model object for the provided InternalElement, using the
        /// systemUnitClass as a generation template.
        /// </summary>
        /// <param name="systemUnitClass">The system unit class.</param>
        /// <param name="internalElement">The internal element.</param>
        /// <returns></returns>
        private static SystemClassBase CreateObject(this SystemUnitFamilyType systemUnitClass, InternalElementType internalElement)
        {
            SystemClassBase createdObject = null;

            // create the apporpriateobject type, based on the class name
            switch (systemUnitClass.Name)
            {
                case "Robot":
                    createdObject = new Robot();
                    break;

                case "Station":
                    createdObject = new Station();
                    break;

                case "Line":
                    createdObject = new ProductionLine();
                    break;

                case "ManufacturingSystem":
                    createdObject = new ManufacturingSystem();
                    break;
            }

            if (createdObject == null)
                return null;

            createdObject.Name = internalElement.Name;

            // the created object should get the same ID as before
            if (internalElement.SourceObjectInformation.Count > 0)
            {
                createdObject.ID = internalElement.SourceObjectInformation.FirstOrDefault().SourceObjID;
            }

            // Assigning the attributes:
            //
            // we want every attribute, defined in the system Unit Class to be transfered to the
            // generated object, including the inherited attributes.
            foreach (var attribute in systemUnitClass.GetInheritedAttributesAndDescendants())
            {
                // check, if the attribute has an occurrence in the internal element
                var ieAttribute = internalElement.Attribute[attribute.Name];
                if (ieAttribute != null)
                {
                    createdObject.SetAttribute(ieAttribute);
                }
            }

            return createdObject;
        }

        /// <summary>
        /// Gets the system unit classes, which directly support all the required roles
        /// </summary>
        /// <param name="roleReferences">The role references.</param>
        /// <returns></returns>
        private static IEnumerable<SystemUnitFamilyType> DirectSupportedClasses(CAEXSequence<RoleRequirementsType> roleReferences)
        {
            foreach (var systemUnitClassLib in _externalClassModelDocument.CAEXFile.SystemUnitClassLib)
            {
                foreach (var systemUnitClass in systemUnitClassLib.Descendants<SystemUnitFamilyType>())
                {
                    // a valid systemUnitClass shall support all role references
                    if (roleReferences.All(r => systemUnitClass.HasRoleClassReference(r.RefBaseRoleClassPath, false)))
                    {
                        yield return systemUnitClass;
                    }
                }
            }
        }

        /// <summary>
        /// Generates the system data object, based on the provided SystemUnitClasses, which can be
        /// used as generation templates. This implementation has an explicit generation method for
        /// each systemUnitClass, however it would be also possible, to implement the generation
        /// method using generics and type reflection only. The advantage would be, that no code
        /// changes are needed if the model changes.
        /// </summary>
        /// <param name="internalElement">The internal element.</param>
        /// <param name="supportedClasses">
        /// The supported system unit classes which define the generator templates.
        /// </param>
        /// <param name="generatedObjects">The list of generated objects.</param>
        private static void GenerateData(InternalElementType internalElement, List<SystemUnitFamilyType> supportedClasses, List<SystemClassBase> generatedObjects)
        {
            SystemClassBase generatedObject = null;

            // only one class defined, no ambiguities which item has to be created
            if (supportedClasses.Count == 1)
            {
                generatedObject = supportedClasses[0].CreateObject(internalElement);
            }

            // check what has been created at last to decide which system unit class should be next
            else
            {
                // if a station was last, the parent shoud be a Line
                if (generatedObjects.LastOrDefault() is Station)
                {
                    generatedObject = supportedClasses.FirstOrDefault(s => s.Name == "Line")?.CreateObject(internalElement);
                }
                else
                {
                    generatedObject = supportedClasses.FirstOrDefault(s => s.Name == "ManufacturingSystem")?.CreateObject(internalElement);
                }
            }

            if (generatedObject == null)
            {
                return;
            }

            // create the hierarchy
            if (generatedObjects.Count > 0)
            {
                var children = generatedObjects.Where(g => generatedObject.IsChild(g));

                if (children.Any())
                {
                    generatedObject.Elements.AddRange(children);
                    generatedObjects.RemoveAll(o => children.Contains(o));
                }
            }

            // add this object to the list
            generatedObjects.Add(generatedObject);
        }


        private static bool IsChild (this SystemClassBase parent, SystemClassBase child)
        {
            switch (parent)
            {
                case Robot _:
                    return false;

                case Station _:
                    return child is Robot;

                case ProductionLine _:
                    return child is Station;

                case ManufacturingSystem _:
                    return child is ProductionLine;

                default:
                 return false;

            }
        }

        /// <summary>
        /// Gets the SystemUnitClasses which supports a generic role from the provided RoleReferences
        /// </summary>
        /// <param name="roleReferences">The role references.</param>
        /// <returns></returns>
        private static IEnumerable<SystemUnitFamilyType> GenericSupportedClasses(CAEXSequence<RoleRequirementsType> roleReferences)
        {
            foreach (var systemUnitClassLib in _externalClassModelDocument.CAEXFile.SystemUnitClassLib)
            {
                foreach (var systemUnitClass in systemUnitClassLib.Descendants<SystemUnitFamilyType>())
                {
                    // if no class is found check for a more generic supported role class,
                    // assigned to a system Unit class
                    if (roleReferences.All(r => systemUnitClass.HasGenericRoleClassReference(r)))
                    {
                        yield return systemUnitClass;
                    }
                }
            }
        }

        /// <summary>
        /// Loads the external class model document.
        /// </summary>
        private static void LoadExternalClassModelDocument()
        {
            // Construction of the file name for the AutomationML file containing the class
            // libraries SystemUnitClasses, RoleClasses, InterfaceClasses and AttributeTypes
            string dir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string modelFile = Path.Combine(dir, "SystemDataModel/AMLFiles/Model_A_Import.aml");
            _externalClassModelDocument = CAEXDocument.LoadFromFile(modelFile);
        }

        /// <summary>
        /// Roles interpretation function
        ///
        /// In this method, the externalized impoter design pattern is implemented. The
        /// interpretation function uses external knowledgem because the interpretable role classes
        /// are derived from an externally defined system data model and the supporting role classes
        /// of the SystemUnitClasses in that model. If roles are not known the according elements
        /// are skipped and the unknown roles are recorded.
        /// </summary>
        /// <param name="internalElement">InternalElement object to be interpreted</param>
        /// <returns>the list of supported system unit family types</returns>
        private static List<SystemUnitFamilyType> RoleInterpreter(InternalElementType internalElement)
        {
            // interpretation without roles is not possible. These elements are bot recorded.
            if (internalElement.RoleRequirements.Count == 0)
            {
                UnknownElements.Add(internalElement);
                return null;
            }

            // check, which systemUnitClasses supports the referenced role, if more than one role is
            // defined, the systemUnitClass shall suport all roles

            var systemUnitClasses = SystemUnitClassesWithSupportedRole(internalElement.RoleRequirements);
            if (!systemUnitClasses.Any())
            {
                UnknownElements.Add(internalElement);
                return null;
            }

            return systemUnitClasses.ToList();
        }

        /// <summary>
        /// Gets the SystemUnit classes from the ExternalClassModelDocument which supports all the
        /// provided supported roles. Please note, that also classes are returned, which supports a
        /// base role from a provided supported role, if no class with 'full' support exists.
        /// </summary>
        /// <param name="roleReferences">The role references.</param>
        /// <returns>The system unit class with supported roles</returns>
        private static IEnumerable<SystemUnitFamilyType> SystemUnitClassesWithSupportedRole(CAEXSequence<RoleRequirementsType> roleReferences)
        {
            var supportedClasses = DirectSupportedClasses(roleReferences);
            if (!supportedClasses.Any())
                supportedClasses = GenericSupportedClasses(roleReferences);

            return supportedClasses;
        }

        #endregion Private Methods
    }
}