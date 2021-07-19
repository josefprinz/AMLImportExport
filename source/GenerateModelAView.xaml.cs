#region copyright
	// Copyright (c) inpro Josef Prinz 2018-2021
	// author: Josef Prinz
	// date:  2021-1-18
	// license: See license.txt in this project
#endregion

using System.Windows.Controls;
using ImportExport.ViewModel;

namespace ImportExport
{
    /// <summary>
    /// Interaktionslogik für GenerateModelAView.xaml
    /// </summary>
    public partial class GenerateModelAView : UserControl
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateModelAView"/> class.
        /// </summary>
        public GenerateModelAView()
        {
            InitializeComponent();
            MainViewModel.Instance.ModelAGenerator = new GenerateModelAViewModel(MainViewModel.Instance);
            DataContext = MainViewModel.Instance;

            ICSharpCode.AvalonEdit.Highlighting.IHighlightingDefinition highLight = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("JavaScript");
            ModelAOutput.SyntaxHighlighting = highLight;
        }

        #endregion Public Constructors
    }
}