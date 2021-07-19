#region copyright
	// Copyright (c) inpro Josef Prinz 2018-2021
	// author: Josef Prinz
	// date:  2021-1-18
	// license: See license.txt in this project
#endregion

namespace ImportExport.SourcePages
{
    /// <summary>
    /// Interaktionslogik für ExporterWithInternalization.xaml
    /// </summary>
    public partial class ExporterWithInternalization : CodePage
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExporterWithExternalization"/> class.
        /// </summary>
        public ExporterWithInternalization()
        {
            InitializeComponent();
            Init(SourceCode, "Export/AMLModelAExporterWithInternalization.cs", "Generate(Project");
        }

        #endregion Public Constructors
    }
}