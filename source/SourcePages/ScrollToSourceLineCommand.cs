#region copyright
	// Copyright (c) inpro Josef Prinz 2018-2021
	// author: Josef Prinz
	// date:  2021-1-18
	// license: See license.txt in this project
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirstFloor.ModernUI.Presentation;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;

namespace ImportExport.SourcePages
{
    /// <summary>
    /// An ICommand implementation that displays the provided command parameter in a message box.
    /// </summary>
    public class ScrollToLineCmd: CommandBase
    {
        public static TextEditor Code { get; internal set; }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        protected override void OnExecute(object parameter)
        {
            if (Code == null)
                return;
            
            var source = (string)parameter;

            if (string.IsNullOrEmpty(source))
                return;

            int nIndex = Code.Text.IndexOf(source, 0);
            if (nIndex != -1)
            {
                 DocumentLine line = Code.Document.GetLineByOffset(nIndex);
                 Code.ScrollToLine(line.LineNumber);
                 Code.Select(line.Offset, line.Length);
            }
        }
    }
}
