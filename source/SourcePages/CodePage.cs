#region copyright
	// Copyright (c) inpro Josef Prinz 2018-2021
	// author: Josef Prinz
	// date:  2021-1-18
	// license: See license.txt in this project
#endregion

using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Navigation;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;

namespace ImportExport.SourcePages
{
    public class CodePage: UserControl, IContent
    {
        private TextEditor _sourceCodeEditor;
        private string _sourceCode;
        private FoldingManager _foldingManager;

        internal void Init (TextEditor sourceCodeEditor, string sourceFile, string sourceCode)
        {
            ICSharpCode.AvalonEdit.Highlighting.IHighlightingDefinition highLight = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("C#");
            sourceCodeEditor.SyntaxHighlighting = highLight;

            // Construction of the file name for the source file containing the import algorithm
            string dir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            sourceFile = System.IO.Path.Combine(dir, sourceFile);

            sourceCodeEditor.Load(sourceFile);
            sourceCodeEditor.ShowLineNumbers = true;
            _sourceCodeEditor = sourceCodeEditor;
            _sourceCode = sourceCode;
            Loaded += OnLoaded;

        }
        
        public void OnFragmentNavigation(FragmentNavigationEventArgs e)
        {
        }

        public void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        public void OnNavigatedTo(NavigationEventArgs e)
        {
            ScrollToLineCmd.Code = _sourceCodeEditor;
        }

        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
        }

        #region Private Methods

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ScrollToLineCmd.Code = _sourceCodeEditor;
            var cmd = new ScrollToLineCmd();
            cmd.Execute(_sourceCode);
            if (_foldingManager != null)
            {
                FoldingManager.Uninstall(_foldingManager);
            }

            _foldingManager = FoldingManager.Install(_sourceCodeEditor.TextArea);
            var foldingStrategy = new CSharpFoldingStrategy();
            foldingStrategy.UpdateFoldings(_foldingManager, _sourceCodeEditor.Document);
        }

        #endregion Private Methods
    }
}