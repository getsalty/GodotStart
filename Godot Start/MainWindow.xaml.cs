using Godot_Start.Services;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ViewModels;
using Windows.Graphics;
using Windows.Storage;
using Windows.Storage.Pickers;
using Version = Godot_Start.Services.Version;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Godot_Start
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private List<Version>? _referenceVersions;
        private string versionPostfix = "_win64.exe";

        public MainWindow()
        {
            this.InitializeComponent();



            RectInt32 appWindow = new()
            {
                Height = Settings.config.WindowSize.Height,
                Width = Settings.config.WindowSize.Width,
                Y = (DisplayArea.Primary.WorkArea.Height / 2) - (Settings.config.WindowSize.Height / 2),
                X = (DisplayArea.Primary.WorkArea.Width / 2) - (Settings.config.WindowSize.Width / 2),
            };

            this.AppWindow.MoveAndResize(appWindow);
            this.SizeChanged += MainWindow_SizeChanged;

            ViewModel = new();

            foreach (var versionType in ViewModel.VersionTypes)
            {
                versionType.Selected = Settings.config.SelectedVersionTypes.Contains(versionType.Name);
                var versionCheckbox = new CheckBox() { Name = versionType.Name, Content = versionType.Name, IsChecked = versionType.Selected };
                versionCheckbox.Click += (object sender, RoutedEventArgs e) => VersionType_Checkbox_Click(sender, e, versionType.Name);

                VersionTypeCheckboxes.Children.Add(versionCheckbox);
            }

            foreach (var project in Settings.config.Projects)
            {
                ViewModel.Projects.Add(new()
                {
                    Name = project.Name,
                    DirectoryPath = project.DirectoryPath,
                    IconPath = project.IconPath
                });
            }

            UpdateVersionDropdownItems();
        }

        public MainViewModel ViewModel { get; }

        private async void UpdateVersionDropdownItems()
        {
            List<Version>? versions;
            if (Settings.config.LastUpdated >= DateTime.Now.AddMinutes(-1))
            {
                versions = VersionChecker.GetVersionsConfig();
            }
            else
            {
                Settings.UpdateTimestamp();
                versions = await VersionChecker.GetVersionsAPI();
            }

            _referenceVersions = versions;

            if (versions is null)
            {
                return;
            }

            foreach (var version in versions)
            {
                var variant = "Stable";

                if (version.Prerelease)
                {
                    if (version.Name.Contains("-dev"))
                    {
                        variant = "Dev";
                    }

                    if (version.Name.Contains("-alpha"))
                    {
                        variant = "Alpha";
                    }

                    if (version.Name.Contains("-beta"))
                    {
                        variant = "Beta";
                    }

                    if (version.Name.Contains("-rc"))
                    {
                        variant = "RC";
                    }
                }

                var currentAsset = version.Assets.ToList().Find(asset => asset.Name.Contains(versionPostfix));
                var isDownloaded = Settings.config.Downloads.ToList().Contains(currentAsset?.Name ?? "");

                var newItem = new VersionViewModel
                {
                    Name = version.Name,
                    Variant = variant,
                    CreatedAt = version.CreatedAt,
                    ShowDelete = isDownloaded,
                    ShowDownload = !isDownloaded,
                    ShowPending = false
                };
                ViewModel.Versions.Add(newItem);
            }

            UpdatedFilteredVersions();
            UpdateSelectedVersion();
        }

        private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            Settings.UpdateWindowSizeType(args.Size);
        }

        private void CurrentVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var newValue = (VersionViewModel?)e.AddedItems.FirstOrDefault();

            //var previousValue = ViewModel.Versions.FirstOrDefault(ViewModel.CurrentVersion);

            var downloadedVersions = _referenceVersions?.Where(version => Settings.config.Downloads.ToList().Contains(version.Assets.ToList().Find(asset => asset.Name.Contains(versionPostfix))?.Name ?? "")).ToList();
            var previousValue = downloadedVersions?.FirstOrDefault(version => version.Name == ViewModel.CurrentVersion?.Name);


            if (newValue is not null && previousValue is not null)
            {
                ViewModel.CurrentVersion = newValue;
                Settings.UpdatedSelectedVersion(newValue?.Name);
            }
            else if (ViewModel.CurrentVersion is null && newValue is not null)
            {
                ViewModel.CurrentVersion = newValue;
                Settings.UpdatedSelectedVersion(newValue?.Name);
            }
            else if (newValue is null && previousValue is null)
            {
                ViewModel.CurrentVersion = newValue;
                Settings.UpdatedSelectedVersion(newValue?.Name);
            }
        }

        private void Remove_Version_Button_Click(object sender, RoutedEventArgs e)
        {
            string removedName = (string)((Button)sender).Tag;

            var removedItem = ViewModel.Versions.First(version => version.Name == removedName);
            removedItem.ShowDownload = true;
            removedItem.ShowDelete = false;
            removedItem.ShowPending = false;


            var removedVersion = _referenceVersions?.First(version => version.Name == removedItem.Name);

            if (removedVersion is null)
            {
                return;
            }

            var removedAsset = removedVersion.Assets.FirstOrDefault(asset => asset.Name.Contains(versionPostfix));

            if (removedAsset is null)
            {
                return;
            }

            GodotDownloads.DeleteVersion(removedAsset.Name);
            Settings.RemoveInstalledVersion(removedAsset.Name);

            if (ViewModel.CurrentVersion is not null && ViewModel.CurrentVersion.Name == removedAsset.Name)
            {
                ViewModel.CurrentVersion = null;
                Settings.UpdatedSelectedVersion(null);
            }

            UpdateSelectedVersion();
        }

        private void Remove_Project_Button_Click(object sender, RoutedEventArgs e)
        {
            string directoryPath = (string)((Button)sender).Tag;

            ProjectViewModel project = ViewModel.Projects.First(project => project.DirectoryPath == directoryPath);
            int projectIndex = ViewModel.Projects.IndexOf(project);
            ViewModel.Projects.RemoveAt(projectIndex);

            Settings.ProjectData projectData = Settings.config.Projects.First(project => project.DirectoryPath == directoryPath);
            Settings.RemoveImportedProject(projectData);
        }

        private void Download_Version_Button_Click(object sender, RoutedEventArgs e)
        {
            string addedName = (string)((Button)sender).Tag;

            var addedItem = ViewModel.Versions.First(version => version.Name == addedName);
            addedItem.ShowDownload = false;
            addedItem.ShowDelete = false;
            addedItem.ShowPending = true;

            var addedVersion = _referenceVersions?.First(version => version.Name == addedItem.Name);

            if (addedVersion is null)
            {
                return;
            }

            var addedAsset = addedVersion.Assets.FirstOrDefault(asset => asset.Name.Contains(versionPostfix));

            if (addedAsset is null)
            {
                return;
            }

            DownloadVersion(addedAsset, addedName);
        }

        private async void DownloadVersion(Asset addedAsset, string addedName)
        {
            await GodotDownloads.DownloadVersion(addedAsset.BrowserDownloadUrl, addedAsset.Name);
            var addedItem = ViewModel.Versions.First(version => version.Name == addedName);
            addedItem.ShowDownload = false;
            addedItem.ShowDelete = true;
            addedItem.ShowPending = false;


            Settings.AddInstalledVersion(addedAsset.Name);
            UpdateSelectedVersion();
        }


        private void VersionType_Checkbox_Click(object sender, RoutedEventArgs e, string variant)
        {
            var currentVersionType = ViewModel.VersionTypes.First(versionType => versionType.Name == variant);
            var currentVersionTypeIndex = ViewModel.VersionTypes.IndexOf(currentVersionType);
            ViewModel.VersionTypes[currentVersionTypeIndex].Selected = ((CheckBox)sender).IsChecked ?? false;

            if (ViewModel.VersionTypes[currentVersionTypeIndex].Selected)
            {
                Settings.AddSelectedVersionType(currentVersionType.Name);
            }
            else
            {
                Settings.RemoveSelectedVersionType(currentVersionType.Name);
            }


            UpdatedFilteredVersions();
        }

        private void Mono_Checkbox_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = ((CheckBox)sender).IsChecked ?? false;


            if (isChecked)
            {
                versionPostfix = "mono_win64.zip";
            }
            else
            {
                versionPostfix = "_win64.exe";
            }
        }

        private void UpdatedFilteredVersions()
        {
            ViewModel.FilteredVersions.Clear();

            List<VersionViewModel> newItems = [];
            foreach (var versionType in ViewModel.VersionTypes)
            {
                if (versionType.Selected)
                {
                    foreach (var version in ViewModel.Versions.Where(version => version.Variant == versionType.Name))
                    {
                        newItems.Add(version);
                    }
                }
            }

            var sortedItems = newItems.OrderByDescending(version => version.CreatedAt);
            foreach (var item in sortedItems)
            {
                ViewModel.FilteredVersions.Add(item);
            }
        }

        private void UpdateSelectedVersion()
        {
            ViewModel.DownloadedVersions.Clear();

            var downloadedVersions = _referenceVersions?.Where(version => Settings.config.Downloads.ToList().Contains(version.Assets.ToList().Find(asset => asset.Name.Contains(versionPostfix))?.Name ?? "")).ToList();

            if (downloadedVersions is null)
            {
                return;
            }

            foreach (var version in downloadedVersions)
            {
                var viewModelVersion = ViewModel.Versions.First(vmVersion => vmVersion.Name == version.Name);

                ViewModel.DownloadedVersions.Add(new VersionViewModel { Name = version.Name, Variant = viewModelVersion.Variant });
            }

            var selectedItem = ViewModel.DownloadedVersions.FirstOrDefault(version => version.Name == Settings.config.SelectedVersion);

            if (selectedItem is not null)
            {
                var index = ViewModel.DownloadedVersions.IndexOf(selectedItem);
                CurrentVersion.SelectedIndex = index;
            }
        }

        private void ListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            TextBlock textBlock = (TextBlock)e.OriginalSource;

            ProjectViewModel project = ViewModel.Projects.First(project => project.Name == textBlock.Text);

            OpenProject(project.DirectoryPath, textBlock.Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)e.OriginalSource;

            ProjectViewModel project = ViewModel.Projects.First(project => project.Name == button.Tag.ToString());

            OpenProject(project.DirectoryPath, button.Tag.ToString());
        }

        private void OpenProject(string path, string? name)
        {
            if (name is null)
            {
                return;
            }

            var project = ViewModel.Projects.First(project => project.Name == name);
            if (project is null)
            {
                return;
            }

            var version = _referenceVersions?.First(version => version.Name == ViewModel.CurrentVersion?.Name);
            if (version is null)
            {
                return;
            }

            var versionAsset = version.Assets.FirstOrDefault(asset => asset.Name.Contains(versionPostfix));
            if (versionAsset is null)
            {
                return;
            }

            var filename = GodotDownloads.path + (versionAsset.Name.Contains("_win64.exe") ? versionAsset.Name[0..^8] : versionAsset.Name[0..^4]) + "\\" + (versionAsset.Name.Contains("_win64.exe") ? versionAsset.Name[0..^4] : versionAsset.Name[0..^0]);

            // Use ProcessStartInfo class
            ProcessStartInfo startInfo = new()
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                FileName = filename,
                //WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = "--path \"" + project.DirectoryPath + "\" -e"
            };

            try
            {
                using Process? exeProcess = Process.Start(startInfo);
            }
            catch
            {
                throw;
                // Log error.
            }
        }

        private async void Import_Project_Button_Click(object sender, RoutedEventArgs e)
        {
            var senderButton = (Button)sender;
            senderButton.IsEnabled = false;

            var folder = await SelectFolder();
            if (folder is null)
            {
                senderButton.IsEnabled = true;
                return;
            }

            Settings.ProjectData? projectData = ParseProjectFile(folder);
            if (projectData is null)
            {
                // freak out?
                senderButton.IsEnabled = true;
                return;
            }

            ViewModel.Projects.Add(new()
            {
                Name = projectData.Name,
                DirectoryPath = projectData.DirectoryPath,
                IconPath = projectData.IconPath
            });

            Settings.AddImportedProject(projectData);

            senderButton.IsEnabled = true;
        }

        private async Task<StorageFolder?> SelectFolder()
        {
            FolderPicker openPicker = new()
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            openPicker.FileTypeFilter.Add("*");

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            return await openPicker.PickSingleFolderAsync();
        }

        private static Settings.ProjectData? ParseProjectFile(StorageFolder directory)
        {
            try
            {
                List<string>? projectLines = File.ReadLines(directory.Path + "\\project.godot")?.ToList();

                if (projectLines is null)
                {
                    return null;
                }

                Settings.ProjectData result = new()
                {
                    Name = "Imported Project",
                    DirectoryPath = directory.Path,
                    IconPath = directory.Path + "\\icon.svg"
                };

                foreach (string line in projectLines)
                {
                    if (line.Contains("config/name"))
                    {
                        result.Name = line[13..^1];
                    }
                    else if (line.Contains("config/features"))
                    {
                        var packedStringArray = line[35..^2];
                        var items = packedStringArray.Split("\", \"");

                        result.Version = items[0];
                        result.Type = items[1];
                    }
                    else if (line.Contains("config/icon"))
                    {
                        result.IconConfig = line[13..^1];
                    }
                }

                if (result.IconConfig?.StartsWith("uid://") ?? false)
                {
                    // TODO create directory parser for all ".import" files to check if the UIDs match, then return the non ".import" file once found. 
                }
                else if (result.IconConfig?.StartsWith("res://") ?? false)
                {
                    result.IconPath = directory.Path + "\\" + result.IconConfig![6..].Replace("/", "\\");
                }

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}