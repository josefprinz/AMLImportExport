#region copyright
	// Copyright (c) inpro Josef Prinz 2018-2021
	// author: Josef Prinz
	// date:  2021-1-18
	// license: See license.txt in this project
#endregion

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace ImportExport.ViewModel
{
    /// <summary>
    /// This view model class is bound to the generation page to enable editing of the generation parameters
    /// </summary>
    /// <seealso cref="GalaSoft.MvvmLight.ViewModelBase" />
    public class GenerateModelAViewModel : ViewModelBase
    {
        #region Private Fields

        /// <summary>
        ///  <see cref="GenerateCommand"/>
        /// </summary>
        private RelayCommand<object> _generateCommand;

        /// <summary>
        ///  <see cref="MaxLines"/>
        /// </summary>
        private int _maxLines;

        /// <summary>
        ///  <see cref="MaxRobots"/>
        /// </summary>
        private int _maxRobots;

        /// <summary>
        ///  <see cref="MaxStations"/>
        /// </summary>
        private int _maxStations;

        /// <summary>
        ///  <see cref="Projectname"/>
        /// </summary>
        private string _projectName;

        /// <summary>
        ///  <see cref="UseLarge"/>
        /// </summary>
        private bool _useLarge;

        /// <summary>
        ///  <see cref="UseSmall"/>
        /// </summary>
        private bool _useSmall;

        private MainViewModel mainViewModel;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateModelAViewModel"/> class.
        /// </summary>
        /// <param name="mainViewModel">The main view model.</param>
        public GenerateModelAViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        ///  The GenerateCommand - Command
        /// </summary>
        public System.Windows.Input.ICommand GenerateCommand => _generateCommand != null ? _generateCommand : (_generateCommand = new RelayCommand<object>(GenerateCommandExecute, GenerateCommandCanExecute));

        /// <summary>
        ///  Gets and sets the MaxLines
        /// </summary>
        public int MaxLines
        {
            get => _maxLines;

            set => Set(ref _maxLines, value, nameof(MaxLines));
        }

        /// <summary>
        ///  Gets and sets the MaxRobots
        /// </summary>
        public int MaxRobots
        {
            get => _maxRobots;

            set => Set(ref _maxRobots, value, nameof(MaxRobots));
        }

        /// <summary>
        ///  Gets and sets the MaxStations
        /// </summary>
        public int MaxStations
        {
            get => _maxStations;

            set => Set(ref _maxStations, value, nameof(MaxStations));
        }

        /// <summary>
        ///  Gets and sets the Projectname
        /// </summary>
        public string Projectname
        {
            get => _projectName;

            set => Set(ref _projectName, value, nameof(Projectname));
        }

        /// <summary>
        ///  Gets and sets the UseLarge
        /// </summary>
        public bool UseLarge
        {
            get => _useLarge;

            set
            {
                Set(ref _useLarge, value, nameof(UseLarge));

                if (UseLarge)
                {
                    Projectname = "Large Project";

                    MaxRobots = 25;
                    MaxStations = 30;
                    MaxLines = 20;
                }
            }
        }

        /// <summary>
        ///  Gets and sets the UseSmall
        /// </summary>
        public bool UseSmall
        {
            get => _useSmall;

            set
            {
                Set(ref _useSmall, value, nameof(UseSmall));

                if (UseSmall)
                {
                    Projectname = "Small Project";
                    MaxRobots = 3;
                    MaxStations = 5;
                    MaxLines = 2;
                }
            }
        }

        #endregion Public Properties

        #region Internal Properties

        internal bool Close => true;

        #endregion Internal Properties

        #region Private Methods

        /// <summary>
        ///  Test, if the <see cref="GenerateCommand"/> can execute.
        /// </summary>
        /// <param name="parameter">
        ///  TODO The parameter.
        /// </param>
        /// <returns>
        ///  true, if command can execute
        /// </returns>
        private bool GenerateCommandCanExecute(object parameter)
        {
            return !string.IsNullOrEmpty(Projectname) && MaxLines > 0 && MaxRobots > 0 && MaxStations > 0;
        }

        /// <summary>
        ///  The <see cref="GenerateCommand"/> Execution Action.
        /// </summary>
        /// <param name="parameter">
        ///  TODO The parameter.
        /// </param>
        private void GenerateCommandExecute(object parameter)
        {
            mainViewModel.GenerateModelAProject();
            RaisePropertyChanged("Close");
        }

        #endregion Private Methods
    }
}