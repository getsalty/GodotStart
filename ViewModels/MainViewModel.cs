using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using MvvmGen;
using System;
using System.Collections.ObjectModel;


namespace ViewModels
{
    [ViewModel]
    public partial class MainViewModel
    {
        [Property]
        private VersionViewModel? _currentVersion;

        [Property]
        private bool _useMono;


        partial void OnInitialize()
        {
            for (int i = 0; i < versionTypeNames.Length; i++)
            {
                VersionTypes.Add(new VersionTypesViewModel { Name = versionTypeNames[i] });
            }

            Projects.Add(new ProjectViewModel { Name = "Kids Game", DirectoryPath = "D:\\Godot\\Kids Game" });
        }

        public ObservableCollection<VersionViewModel> Versions { get; } = [];
        public ObservableCollection<VersionViewModel> FilteredVersions { get; } = [];
        public ObservableCollection<VersionViewModel> DownloadedVersions { get; } = [];
        public ObservableCollection<VersionTypesViewModel> VersionTypes { get; } = [];
        public ObservableCollection<ProjectViewModel> Projects { get; } = [];


        private static readonly string[] versionTypeNames = ["Stable", "RC", "Beta", "Alpha", "Dev"];
    }

    [ViewModel]
    public partial class VersionViewModel
    {
        [Property]
        private string _name;

        [Property]
        private string _variant;

        [Property]
        private bool _showDelete;

        [Property]
        private bool _showDownload;

        [Property]
        private bool _showPending;

        [Property]
        private DateTime _createdAt;
    }


    [ViewModel]
    public partial class VersionTypesViewModel
    {
        [Property]
        private string _name;

        [Property]
        private bool _selected;
    }


    [ViewModel]
    public partial class ProjectViewModel
    {
        [Property]
        private string _name;

        [Property]
        private string _directoryPath;
    }
}
