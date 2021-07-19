#region copyright
	// Copyright (c) inpro Josef Prinz 2018-2021
	// author: Josef Prinz
	// date:  2021-1-18
	// license: See license.txt in this project
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Aml.Engine.CAEX;
using Aml.Toolkit.ViewModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ICSharpCode.AvalonEdit.Document;
using ImportExport.Controls;
using ImportExport.Export;
using Microsoft.Win32;
using Newtonsoft.Json;
using SystemDataModel.Model_A;

namespace ImportExport.ViewModel
{
    /// <summary>
    /// Viewmodel class bound to the main window.
    /// </summary>
    /// <seealso cref="GalaSoft.MvvmLight.ViewModelBase"/>
    public class MainViewModel : ViewModelBase
    {
        #region Private Fields

        /// <summary>
        /// <see cref="ExportModelADocument"/>
        /// </summary>
        private AMLTreeViewModel _aMLExportResult;

        /// <summary>
        /// <see cref="ExportModelADocumentExternalized"/>
        /// </summary>
        private AMLTreeViewModel _exportModelADocumentExternalized;

        /// <summary>
        /// <see cref="ExportModelAExternalizedCommand"/>
        /// </summary>
        private RelayCommand<object> _exportModelAExternalizedCommand;

        /// <summary>
        /// <see cref="ExportModelAInternalizedCommand"/>
        /// </summary>
        private RelayCommand<object> _exportModelAInternalizedCommand;

        /// <summary>
        /// <see cref="ExternalizedModelA"/>
        /// </summary>
        private AMLTreeViewModel _externalModelA;

        /// <summary>
        /// <see cref="HasUnknownRoles"/>
        /// </summary>
        private bool _hasUnknownRoles;

        /// <summary>
        /// <see cref="ImportedModelA"/>
        /// </summary>
        private TextDocument _importedModelA;

        /// <summary>
        /// <see cref="ImportedModelAExternalized"/>
        /// </summary>
        private TextDocument _importedModelAExternalized;

        private Project _importedProjectInternalized;

        private ImportError _importError = null;

        /// <summary>
        /// <see cref="ImportExternalizedCAEXDocumentTree"/>
        /// </summary>
        private AMLTreeViewModel _importExternalizedCAEXDocumentTree;

        /// <summary>
        /// <see cref="ImportInternalizedCAEXDocumentTree"/>
        /// </summary>
        private AMLTreeViewModel _importInternalizedCAEXDocumentTree;

        /// <summary>
        /// <see cref="ImportModelACommand"/>
        /// </summary>
        private RelayCommand<object> _importModelACommand;

        private CAEXDocument _inputDocumentExtern;
        private CAEXDocument _inputDocumentIntern;
        private CAEXDocument _modelADocument;

        /// <summary>
        /// <see cref="ModelADocument"/>
        /// </summary>
        private TextDocument _modelAInstances;

        private CAEXDocument _outPutDocumentExternalized;
        private CAEXDocument _outPutDocumentInternalized;
        private RelayCommand<object> _saveExportCommand;

        /// <summary>
        /// <see cref="UnknownRoles"/>
        /// </summary>
        private ObservableCollection<string> _unknownRoles;

        #endregion Private Fields

        #region Private Constructors

        /// <summary>
        /// This class is a singleton. Prevents a default instance of the <see
        /// cref="MainViewModel"/> class from being created.
        /// </summary>
        private MainViewModel()
        {
            LoadExternalizedModelA();
        }

        #endregion Private Constructors

        #region Public Properties

        /// <summary>
        /// Gets the static instance.
        /// </summary>
        public static MainViewModel Instance { get; } = new MainViewModel();

        public MainWindow AppWindow { get; internal set; }

        /// <summary>
        /// Gets and sets the AutomationML Tree view model, which represents the exported 'Model A'
        /// project data from <see cref="ModelAProject"/>.
        /// </summary>
        public AMLTreeViewModel ExportModelADocument
        {
            get => _aMLExportResult;
            set => Set(ref _aMLExportResult, value, nameof(ExportModelADocument));
        }

        /// <summary>
        /// Gets and sets the ExportModelADocumentExternalized
        /// </summary>
        public AMLTreeViewModel ExportModelADocumentExternalized
        {
            get => _exportModelADocumentExternalized;
            set => Set(ref _exportModelADocumentExternalized, value, nameof(ExportModelADocumentExternalized));
        }

        /// <summary>
        /// The ExportModelAExternalized Command which calls the method to export the <see
        /// cref="ModelAProject"/> to AutomationML using an Externalized Export Algoritm, which uses
        /// the <see cref="ExternalizedModelA"/>.
        /// </summary>
        public System.Windows.Input.ICommand ExportModelAExternalizedCommand => _exportModelAExternalizedCommand != null ? _exportModelAExternalizedCommand : (_exportModelAExternalizedCommand = new RelayCommand<object>(ExportModelAExternalizedCommandExecute, ExportModelAExternalizedCommandCanExecute));

        /// <summary>
        /// The ExportModelAInternalized Command which calls the method to export the <see
        /// cref="ModelAProject"/> to AutomationML using an Internalized Export Algoritm.
        /// </summary>
        public System.Windows.Input.ICommand ExportModelAInternalizedCommand => _exportModelAInternalizedCommand != null ? _exportModelAInternalizedCommand : (_exportModelAInternalizedCommand = new RelayCommand<object>(ExportModelAInternalizedCommandExecute, ExportModelAInternalizedCommandCanExecute));

        /// <summary>
        /// Gets and sets the AutomationML Tree view model, which represents the externalized 'Model A'.
        /// </summary>
        public AMLTreeViewModel ExternalizedModelA
        {
            get => _externalModelA;
            set => Set(ref _externalModelA, value, nameof(ExternalizedModelA));
        }

        private Project _importedProjectExternalized;

        /// <summary>
        /// Gets and sets the HasUnknownRoles
        /// </summary>
        public bool HasUnknownRoles
        {
            get => _hasUnknownRoles;

            set
            {
                if (Set(ref _hasUnknownRoles, value, nameof(HasUnknownRoles)))
                {
                    AppWindow.Dispatcher.Invoke(() =>
                    {
                        if (_importError == null)
                        {
                            _importError = new ImportError
                            {
                                DataContext = this,
                                Owner = AppWindow
                            };
                            _importError.Closed += (s, e) => _importError = null;
                            _importError.Show();
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Gets and sets the ImportedModelAExternalized serialized as a Text document
        /// </summary>
        public TextDocument ImportedModelAExternalized
        {
            get => _importedModelAExternalized;
            set { Set(ref _importedModelAExternalized, value, nameof(ImportedModelAExternalized)); }
        }

        /// <summary>
        /// Gets and sets the ImportedModelA serialized as a Text document
        /// </summary>
        public TextDocument ImportedModelAInternalized
        {
            get => _importedModelA;
            set { Set(ref _importedModelA, value, nameof(ImportedModelAInternalized)); }
        }

        /// <summary>
        /// Gets and sets the ImportExternalizedCAEXDocumentTree
        /// </summary>
        public AMLTreeViewModel ImportExternalizedCAEXDocumentTree
        {
            get => _importExternalizedCAEXDocumentTree;
            set { Set(ref _importExternalizedCAEXDocumentTree, value, nameof(ImportExternalizedCAEXDocumentTree)); }
        }

        /// <summary>
        /// Gets and sets the ImportInternalizedCAEXDocumentTree
        /// </summary>
        public AMLTreeViewModel ImportInternalizedCAEXDocumentTree
        {
            get => _importInternalizedCAEXDocumentTree;
            set { Set(ref _importInternalizedCAEXDocumentTree, value, nameof(ImportInternalizedCAEXDocumentTree)); }
        }

        /// <summary>
        /// The ImportModelACommand - Command
        /// </summary>
        public System.Windows.Input.ICommand ImportModelACommand => _importModelACommand = (_importModelACommand != null)
            ? _importModelACommand
            : (_importModelACommand = new RelayCommand<object>(ImportModelACommandExecute, ImportModelACommandCanExecute));

        /// <summary>
        /// Gets and sets the Document, which shows the generated 'Model A' instances. The data,
        /// shown in this document is 'json' formatted and generated in <see cref="GenerateModelAProject"/>.
        /// </summary>
        public TextDocument ModelADocument
        {
            get => _modelAInstances;
            set => Set(ref _modelAInstances, value, nameof(ModelADocument));
        }

        /// <summary>
        /// Gets the generator, which specifies genertor parameters to generate a <see cref="ModelAProject"/>.
        /// </summary>
        /// <value>The model a generator.</value>
        public GenerateModelAViewModel ModelAGenerator { get; internal set; }

        /// <summary>
        /// Gets or sets the project, defined from the system data model 'Model A'
        /// </summary>
        /// <value>The model a project.</value>
        public Project ModelAProject { get; set; }

        /// <summary>
        /// The SaveExportCommand can save the generated AutomationML Data to an external file.
        /// </summary>
        public System.Windows.Input.ICommand SaveExportCommand => _saveExportCommand != null ? _saveExportCommand : (_saveExportCommand = new RelayCommand<object>(SaveExportCommandExecute, SaveExportCommandCanExecute));

        /// <summary>
        /// Gets and sets the UnknownRoles
        /// </summary>
        public ObservableCollection<string> UnknownRoles
        {
            get => _unknownRoles;

            set { Set(ref _unknownRoles, value, nameof(UnknownRoles)); }
        }

        #endregion Public Properties

        #region Internal Methods

        /// <summary>
        /// Generates the model a project data, using the defined generator values from the <see cref="ModelAGenerator"/>.
        /// </summary>
        internal void GenerateModelAProject()
        {
            ProjectDataGeneratorSettings.MaxLine = ModelAGenerator.MaxLines;
            ProjectDataGeneratorSettings.MaxStation = ModelAGenerator.MaxStations;
            ProjectDataGeneratorSettings.MaxRobot = ModelAGenerator.MaxRobots;

            ModelAProject = Project.Generate(ModelAGenerator.Projectname ?? "Test");
            ModelADocument = SerializeModelAProjectData(ModelAProject);
        }

        /// <summary>
        /// Prepares the 'Model A' AutomationML export document Tree view model .
        /// </summary>
        /// <param name="exportResultDocument">The export result AML document.</param>
        internal void ShowModelAExportResult(CAEXDocument exportResultDocument, bool external = false)
        {
            HashSet<string> visibleItems = new HashSet<string>(AMLTreeViewTemplate.CompleteInstanceHierarchyTree)
            {
                CAEX_CLASSModel_TagNames.ATTRIBUTE_STRING
            };

            if (!external)
            {
                ExportModelADocument = new AMLTreeViewModel(exportResultDocument.CAEXFile.Node, visibleItems);
                ExportModelADocument.TreeViewLayout.ShowAttributeValue = true;
                ExportModelADocument.ExpandAllCommand.Execute(true);
            }
            else
            {
                ExportModelADocumentExternalized = new AMLTreeViewModel(exportResultDocument.CAEXFile.Node, visibleItems);
                ExportModelADocumentExternalized.TreeViewLayout.ShowAttributeValue = true;
                ExportModelADocumentExternalized.ExpandAllCommand.Execute(true);
            }
        }

        internal void ShowModelAImport(CAEXDocument importDocument, bool external = false)
        {
            HashSet<string> visibleItems = new HashSet<string>(AMLTreeViewTemplate.CompleteInstanceHierarchyTree)
            {
                CAEX_CLASSModel_TagNames.ATTRIBUTE_STRING
            };

            if (external)
            {
                ImportExternalizedCAEXDocumentTree = new AMLTreeViewModel(importDocument.CAEXFile.Node, visibleItems);
                ImportExternalizedCAEXDocumentTree.TreeViewLayout.ShowAttributeValue = true;
                ImportExternalizedCAEXDocumentTree.ExpandAllCommand.Execute(true);
            }
            else
            {
                ImportInternalizedCAEXDocumentTree = new AMLTreeViewModel(importDocument.CAEXFile.Node, visibleItems);
                ImportInternalizedCAEXDocumentTree.TreeViewLayout.ShowAttributeValue = true;
                ImportInternalizedCAEXDocumentTree.ExpandAllCommand.Execute(true);
            }
        }

        #endregion Internal Methods

        #region Private Methods

        /// <summary>
        /// Test, if the <see cref="ExportModelAExternalizedCommand"/> can execute.
        /// </summary>
        /// <param name="parameter">not used here.</param>
        /// <returns>true, if command can execute</returns>
        private bool ExportModelAExternalizedCommandCanExecute(object parameter)
        {
            return ExternalizedModelA != null && ModelAProject != null;
        }

        /// <summary>
        /// The <see cref="ExportModelAExternalizedCommand"/> Execution Action.
        /// </summary>
        /// <param name="parameter">not used here.</param>
        private void ExportModelAExternalizedCommandExecute(object parameter)
        {
            _outPutDocumentExternalized = AMLModelAExporterWithExternalization.Generate(ModelAProject);
            ShowModelAExportResult(_outPutDocumentExternalized, true);
        }

        /// <summary>
        /// Test, if the <see cref="ExportModelAInternalizedCommand"/> can execute.
        /// </summary>
        /// <param name="parameter">not used here.</param>
        /// <returns>true, if command can execute</returns>
        private bool ExportModelAInternalizedCommandCanExecute(object parameter)
        {
            return ModelAProject != null;
        }

        /// <summary>
        /// The <see cref="ExportModelAInternalizedCommand"/> Execution Action.
        /// </summary>
        /// <param name="parameter">not used here.</param>
        private void ExportModelAInternalizedCommandExecute(object parameter)
        {
            _outPutDocumentInternalized = AMLModelAExporterWithInternalization.Generate(ModelAProject);
            ShowModelAExportResult(_outPutDocumentInternalized);
        }

        private async Task ImportIntern()
        {
            await Task.Run(() =>
                    {
                        _importedProjectInternalized = Import.AMLModelAImporterWithInternalization.Import(_inputDocumentIntern);

                        HasUnknownRoles = Import.AMLModelAImporterWithInternalization.UnknownRoles.Count > 0;
                        if (HasUnknownRoles)
                        {
                            UnknownRoles = new ObservableCollection<string>(Import.AMLModelAImporterWithInternalization.UnknownRoles);
                        }

                        if (Import.AMLModelAImporterWithInternalization.UnknownElements.Count > 0)
                        {
                            MarkNotImported(ImportInternalizedCAEXDocumentTree, Import.AMLModelAImporterWithInternalization.UnknownElements);
                        }

                        AppWindow.Dispatcher.Invoke(() =>
                       ImportedModelAInternalized = SerializeModelAProjectData(_importedProjectInternalized));
                    });
        }



        private async Task ImportExtern()
        {
            await Task.Run(() =>
                    {
                        _importedProjectExternalized = Import.AMLModelAImporterWithExternalization.Import(_inputDocumentExtern);

                        HasUnknownRoles = Import.AMLModelAImporterWithExternalization.UnknownRoles.Count > 0;
                        if (HasUnknownRoles)
                        {
                            UnknownRoles = new ObservableCollection<string>(Import.AMLModelAImporterWithExternalization.UnknownRoles);
                        }

                        if (Import.AMLModelAImporterWithExternalization.UnknownElements.Count > 0)
                        {
                            MarkNotImported(ImportExternalizedCAEXDocumentTree, Import.AMLModelAImporterWithExternalization.UnknownElements);
                        }

                        AppWindow.Dispatcher.Invoke(() =>
                        ImportedModelAExternalized = SerializeModelAProjectData(_importedProjectExternalized));
                    });
        }

        /// <summary>
        /// Test, if the <see cref="ImportModelACommand"/> can execute.
        /// </summary>
        /// <param name="parameter">TODO The parameter.</param>
        /// <returns>true, if command can execute</returns>
        private bool ImportModelACommandCanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// The <see cref="ImportModelACommand"/> Execution Action.
        /// </summary>
        /// <param name="parameter">TODO The parameter.</param>
        private async void ImportModelACommandExecute(object parameter)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                DefaultExt = ".aml", // Default file extension
                Filter = "AutomationML documents (.aml)|*.aml" // Filter files by extension
            };

            if (dialog.ShowDialog() == true)
            {
                HasUnknownRoles = false;
                UnknownRoles?.Clear();

                if ((string)parameter == "Extern")
                {
                    _inputDocumentExtern = CAEXDocument.LoadFromFile(dialog.FileName);
                    ShowModelAImport(_inputDocumentExtern, true);

                    await ImportExtern();
                }
                else
                {
                    _inputDocumentIntern = CAEXDocument.LoadFromFile(dialog.FileName);
                    ShowModelAImport(_inputDocumentIntern, false);

                    await ImportIntern();
                }
            }
        }

        /// <summary>
        /// Loads the AutomationML File representing the externalized System data model for "Model
        /// A". This AutomationML document is visualized in the <see cref="ExternalizedModelA"/>.
        /// </summary>
        private void LoadExternalizedModelA()
        {
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string modelFile = Path.Combine(dir, "SystemDataModel/AMLFiles/Model_A.aml");

            if (!File.Exists(modelFile))
            {
                return;
            }

            _modelADocument = CAEXDocument.LoadFromFile(modelFile);

            HashSet<string> visibleItems = new HashSet<string>(AMLTreeViewTemplate.CompleteSystemUnitClassLibTree)
            {
                CAEX_CLASSModel_TagNames.ATTRIBUTE_STRING
            };

            ExternalizedModelA = new AMLTreeViewModel(_modelADocument.CAEXFile.Node, visibleItems);
            ExternalizedModelA.ExpandAllCommand.Execute(true);
        }

        private void MarkNotImported(AMLTreeViewModel importInternalizedCAEXDocumentTree, List<InternalElementType> unknownElements)
        {
            AppWindow.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
           {
               foreach (var item in unknownElements)
               {
                   var node = importInternalizedCAEXDocumentTree.SelectCaexNode(item.Node, false, false, false, false, false);
                   if (node != null)
                   {
                       node.AdditionalInformation = " ? ";
                       node.RefreshNodeInformation(false);
                   }
               }
           }));
        }

        private bool SaveExportCommandCanExecute(object arg)
        {
            if ((string)arg == "Internalized")
            {
                return _outPutDocumentInternalized != null;
            }
            else
            {
                return _outPutDocumentExternalized != null;
            }
        }

        private void SaveExportCommandExecute(object arg)
        {
            CAEXDocument doc;

            if ((string)arg == "Internalized")
            {
                doc = _outPutDocumentInternalized;
            }
            else
            {
                doc = _outPutDocumentExternalized;
            }

            SaveFileDialog dialog = new SaveFileDialog
            {
                DefaultExt = ".aml", // Default file extension
                Filter = "AutomationML documents (.aml)|*.aml" // Filter files by extension
            };

            // Show save file dialog box
            Nullable<bool> result = dialog.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dialog.FileName;
                doc.SaveToFile(filename, true);
            }
        }

        private TextDocument SerializeModelAProjectData(Project project)
        {
            JsonSerializer serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            };

            using (StringWriter sw = new StringWriter())
            {
                JsonWriter writer = new JsonTextWriter(sw);
                serializer.Serialize(writer, project);

                string json = sw.ToString();
                return new TextDocument(json);
            }
        }

        #endregion Private Methods
    }
}