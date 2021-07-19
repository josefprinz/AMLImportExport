#region copyright
	// Copyright (c) inpro Josef Prinz 2018-2021
	// author: Josef Prinz
	// date:  2021-1-18
	// license: See license.txt in this project
#endregion

using System.Collections.Generic;
using System.Linq;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;

namespace ImportExport.SourcePages
{
    public class CSharpFoldingStrategy : FoldingStrategy
    {
        #region Protected Methods

        protected override IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = 0;
            List<NewFolding> newFoldings = new List<NewFolding>();

            Stack<int> startOffsets = new Stack<int>();
            string openingRegion = "#region";
            string closingRegion = "#endregion";
            for (int i = 0; i < document.Lines.Count; i++)
            {
                string line = document.GetText(document.Lines[i]);
                
                if (line.TrimStart().StartsWith(openingRegion))
                {
                    startOffsets.Push(document.Lines[i].Offset+document.Lines[i].Length);
                }
                else if (line.TrimStart().StartsWith(closingRegion) && startOffsets.Count > 0)
                {
                    int startOffset = startOffsets.Pop();
                    newFoldings.Add(new NewFolding(startOffset, document.Lines[i].Offset+document.Lines[i].Length));
                }
            }
            newFoldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
            newFoldings.First().DefaultClosed = true;
            return newFoldings;
        }

        #endregion Protected Methods
    }
}