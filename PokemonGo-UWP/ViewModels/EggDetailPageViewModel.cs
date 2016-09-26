using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using PokemonGo.RocketAPI;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using POGOProtos.Inventory;
using POGOProtos.Networking.Responses;
using Template10.Mvvm;

namespace PokemonGo_UWP.ViewModels
{
    public class EggDetailPageViewModel : ViewModelBase
    {
        #region Lifecycle Handlers

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                // Recovering the state
                Load((ulong)suspensionState[nameof(SelectedEgg)]);
            }
            else
            {
                // Navigating from inventory page so we need to load the egginventory and the current egg
                Load(Convert.ToUInt64(parameter));
            }

            await Task.CompletedTask;
        }

        public void Load(ulong selectedEggId)
        {
            EggInventory.Clear();

            var unincubatedEggs = GameClient.EggsInventory.Where(o => string.IsNullOrEmpty(o.EggIncubatorId))
                                              .OrderBy(c => c.EggKmWalkedTarget).Select(eggData => new PokemonDataWrapper(eggData));
            var incubatedEggs = GameClient.EggsInventory.Where(o => !string.IsNullOrEmpty(o.EggIncubatorId))
                                                          .OrderBy(c => c.EggKmWalkedTarget).Select(eggData => new IncubatedEggDataWrapper(GameClient.GetIncubatorFromEgg(eggData), GameClient.PlayerStats.KmWalked, eggData));

            foreach(PokemonDataWrapper egg in incubatedEggs)
            {
                EggInventory.Add(egg);
            }
            foreach (PokemonDataWrapper egg in unincubatedEggs)
            {
                EggInventory.Add(egg);
            }

            SelectedEgg = EggInventory.FirstOrDefault(egg => egg.Id == selectedEggId);
        }

        /// <summary>
        /// Save state before navigating
        /// </summary>
        /// <param name="suspensionState"></param>
        /// <param name="suspending"></param>
        /// <returns></returns>
        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(SelectedEgg)] = SelectedEgg.Id;
            }
            await Task.CompletedTask;
        }


        #endregion

        #region Bindable Vars

        public ObservableCollection<PokemonDataWrapper> EggInventory { get; private set; } = new ObservableCollection<PokemonDataWrapper>();

        /// <summary>
        /// Current displayed Pokemon
        /// </summary>
        private PokemonDataWrapper _selectedEgg;
        public PokemonDataWrapper SelectedEgg
        {
            get { return _selectedEgg; }
            set { Set(ref _selectedEgg, value); }
        }

        /// <summary>
        /// Going back to inventory page
        /// </summary>
        private DelegateCommand _returnToPokemonInventoryScreen;
        public DelegateCommand ReturnToPokemonInventoryScreen => _returnToPokemonInventoryScreen ?? (
            _returnToPokemonInventoryScreen = new DelegateCommand(() =>
            {
                NavigationService.GoBack();
            }, () => true));

        #endregion

        #region Game Logic

        #region Incubators

        private DelegateCommand<EggIncubator> _useIncubatorCommand;
        public DelegateCommand<EggIncubator> UseIncubatorCommand => _useIncubatorCommand ?? (
            _useIncubatorCommand = new DelegateCommand<EggIncubator>(async incubator =>
            {
                var response = await GameClient.UseEggIncubator(incubator, SelectedEgg.WrappedData);
                switch (response.Result)
                {
                    case UseItemEggIncubatorResponse.Types.Result.Success:
                        await GameClient.UpdateInventory();
                        Load(SelectedEgg.Id);
                        break;
                    default:
                        Logger.Write($"Error using {incubator.Id} on {SelectedEgg.Id}");
                        break;
                }
            }, incubator => true));

        #endregion

        #endregion
    }
}