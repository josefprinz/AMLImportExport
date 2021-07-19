﻿#region copyright
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
    /// Interaktionslogik für ModelAImportExternalized.xaml
    /// </summary>
    public partial class ModelAImportExternalized : UserControl
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelAImportInternalized"/> class.
        /// </summary>
        public ModelAImportExternalized()
        {
            InitializeComponent();
            DataContext = MainViewModel.Instance;
        }

        #endregion Public Constructors
    }
}