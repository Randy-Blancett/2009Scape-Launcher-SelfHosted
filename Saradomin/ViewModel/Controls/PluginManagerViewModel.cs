using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Glitonea.Mvvm;
using Glitonea.Mvvm.Messaging;
using Saradomin.Infrastructure;
using Saradomin.Infrastructure.Services;
using Saradomin.Model;
using Saradomin.View.Windows;

namespace Saradomin.ViewModel.Controls
{
    public class PluginManagerViewModel : ViewModelBase
    {
        private readonly IPluginManagementService _pluginManagementService;
        private readonly IPluginDownloadService _pluginDownloadService;

        public bool IsTransactionInProgress { get; private set; }
        public ObservableCollection<PluginInfo> PluginList { get; private set; } = new();
        public string CurrentStatusMessage { get; set; } = "Idle...";

        public PluginManagerViewModel(
            IPluginManagementService pluginManagementService,
            IPluginDownloadService pluginDownloadService)
        {
            _pluginManagementService = pluginManagementService;
            _pluginDownloadService = pluginDownloadService;
            
            Message.Subscribe<MainViewLoadedMessage>(this, MainViewLoaded);
            
            PropertyChanged += ViewModelPropertyChanged;
        }

        public async Task RefreshPluginCollections()
        {
            PluginList.Clear();

            var remotePlugins =await _pluginDownloadService.GetAllMetadata(
                _pluginManagementService.PluginRepositoryPath,
                false,
                false
            );
            
            PluginList = new ObservableCollection<PluginInfo>(remotePlugins.OrderByDescending(x => x.Installed));
        }

        public async Task InstallRemotePlugin(object parameter)
        {
            if (parameter is not PluginInfo pluginInfo)
                return;
            
            if (IsTransactionInProgress)
                return;

            try
            {
                IsTransactionInProgress = true;
                
                UpdateStatusMessage($"Installing {pluginInfo.Name}...");

                await _pluginDownloadService.DownloadPluginFiles(
                    pluginInfo.Name,
                    _pluginManagementService.PluginRepositoryPath
                );

                await RefreshPluginCollections();
            }
            catch (Exception e)
            {
                NotificationBox.DisplayNotification(
                    "Error",
                    $"Plugin installation failed: {e.Message}"
                );
            }
            finally
            {
                IsTransactionInProgress = false;
            }
        }

        public async Task UninstallLocalPlugin(object parameter)
        {
            if (parameter is not PluginInfo pluginInfo)
                return;
            
            if (IsTransactionInProgress)
                return;

            try
            {
                IsTransactionInProgress = true;
                
                UpdateStatusMessage($"Uninstalling {pluginInfo.Name}...");
                
                await _pluginManagementService.UninstallPlugin(pluginInfo.Name);
                pluginInfo.Installed = false;
                
                PluginList = new(PluginList.OrderByDescending(x => x.Installed));
            }
            catch (Exception e)
            {
                NotificationBox.DisplayNotification(
                    "Error",
                    $"Plugin removal failed: {e.Message}" 
                );
            }
            finally
            {
                IsTransactionInProgress = false;
            }
        }

        public async Task CheckForUpdates()
        {
            if (IsTransactionInProgress)
                return;
            
            
            try
            {
                IsTransactionInProgress = true;
                
                UpdateStatusMessage($"Checking for updates...");

                var updatablePlugins = await _pluginDownloadService.GetAllMetadata(
                    _pluginManagementService.PluginRepositoryPath,
                    true,
                    true
                );

                foreach (var plugin in updatablePlugins)
                {
                    PluginList.Remove(PluginList.First(x => x.Name == plugin.Name));
                    PluginList.Add(plugin);
                }

                PluginList = new(PluginList.OrderByDescending(x => x.Installed));
            }
            finally
            {
                IsTransactionInProgress = false;
            }
        }

        public async Task UpdateLocalPlugin(object parameter)
        {
            if (parameter is not PluginInfo pluginInfo)
                return;
            
            if (IsTransactionInProgress)
                return;

            try
            {
                IsTransactionInProgress = true;
                
                UpdateStatusMessage($"Updating {pluginInfo.Name}...");
                
                await _pluginDownloadService.DownloadPluginFiles(pluginInfo.Name, _pluginManagementService.PluginRepositoryPath);
                await RefreshPluginCollections();

                var newInfo = PluginList.First(x => x.Name == pluginInfo.Name);
                if (newInfo.Version != pluginInfo.Version)
                {
                    NotificationBox.DisplayNotification(
                        "Success!",
                        $"Successfully updated {pluginInfo.Name} to version {newInfo.Version}!"
                    );
                }
            }
            catch (Exception e)
            {
                NotificationBox.DisplayNotification(
                    "Error",
                    $"Plugin update failed: {e.Message}" 
                );
            }
            finally
            {
                IsTransactionInProgress = false;
            }
        }

        private void UpdateStatusMessage(string message)
        {
            CurrentStatusMessage = message;
        }

        private async void MainViewLoaded(MainViewLoadedMessage _)
        {
            await RefreshPluginCollections();
        }
        
        private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IsTransactionInProgress):
                {
                    if (!IsTransactionInProgress)
                    {
                        UpdateStatusMessage("Idle...");
                    }

                    break;
                }
            }
        }
    }
}