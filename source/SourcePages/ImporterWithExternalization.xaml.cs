#region copyright
	// Copyright (c) inpro Josef Prinz 2018-2021
	// author: Josef Prinz
	// date:  2021-1-18
	// license: See license.txt in this project
#endregion

namespace ImportExport.SourcePages
{
    /// <summary>
    /// Interaktionslogik für ExporterWithExternalization.xaml
    /// </summary>
    public partial class ImporterWithExternalization : CodePage
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterWithExternalization"/> class.
        /// </summary>
        public ImporterWithExternalization()
        {
            InitializeComponent();
            Init(SourceCode, "Import/AMLModelAImporterWithExternalization.cs", "Import(CAEXDocument");        }

        #endregion Public Constructors
    }
}