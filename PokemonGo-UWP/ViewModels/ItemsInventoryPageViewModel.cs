using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using PokemonGo_UWP.Controls;
using POGOProtos.Networking.Responses;
using System;
using POGOProtos.Inventory;
using PokemonGo.RocketAPI.Extensions;

namespace PokemonGo_UWP.ViewModels
{
    public class ItemsInventoryPageViewModel : ViewModelBase
    {
        #region Lifecycle Handlers

        /// <summary>
        /// Defines the modes, the ItemsInventoryPage can be viewed
        /// </summary>
        public enum ItemsInventoryViewMode
        {
            Normal,
            Catch
        }

        /// <summary>
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="mode"></param>
        /// <param name="suspensionState"></param>
        /// <returns></returns>
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode,
            IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                // Recovering the state
                ItemsInventory = JsonConvert.DeserializeObject<ObservableCollection<ItemDataWrapper>>((string)suspensionState[nameof(ItemsInventory)]);
            }
            else if (parameter is bool)
            {
                // Navigating from game page, so we need to actually load the inventory
                // The sorting is directly bound to the ViewMode
                ItemsInventory = new ObservableCollection<ItemDataWrapper>(this.SortItems(
                    GameClient.ItemsInventory.Where(
                        itemData => ((POGOProtos.Inventory.Item.ItemData)itemData).Count > 0).Select(
                        itemData => new ItemDataWrapper(itemData))));

                RaisePropertyChanged(() => ItemsInventory);
            }

            ItemsTotalCount = ItemsInventory.Sum(i => i.WrappedData.Count);

            await Task.CompletedTask;
        }

        /// <summary>
        ///     Save state before navigating
        /// </summary>
        /// <param name="suspensionState"></param>
        /// <param name="suspending"></param>
        /// <returns></returns>
        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(ItemsInventory)] = JsonConvert.SerializeObject(ItemsInventory);
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        #endregion

        #region Datahandling

        /// <summary>
        /// Orders the list of items accorfing to the viewmode set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        private IOrderedEnumerable<ItemDataWrapper> SortItems(IEnumerable<ItemDataWrapper> items)
        {
            var useableList = ViewMode == ItemsInventoryViewMode.Normal ? GameClient.NormalUseItemIds : GameClient.CatchItemIds;
            return items.OrderBy(item => !useableList.Contains(item.ItemId)).ThenBy(item => item.ItemId);
        }

        #endregion

        #region Bindable Game Vars

        /// <summary>
        ///     Reference to Items inventory
        /// </summary>
        public ObservableCollection<ItemDataWrapper> ItemsInventory { get; private set; } =
            new ObservableCollection<ItemDataWrapper>();

        private int _itemsTotalCount;
        public int ItemsTotalCount
        {
            get { return _itemsTotalCount; }
            private set { Set(ref _itemsTotalCount, value); }
        }

        public ItemsInventoryViewMode ViewMode { get; set; }

        #endregion

        #region Game Logic

        #region Shared Logic

        private DelegateCommand _returnToGameScreen;

        /// <summary>
        ///     Going back to map page
        /// </summary>
        public DelegateCommand ReturnToGameScreen
            =>
                _returnToGameScreen ??
                (_returnToGameScreen =
                    new DelegateCommand(() => { NavigationService.Navigate(typeof(GameMapPage)); }, () => true));

        public int MaxItemStorageFieldNumber => GameClient.PlayerProfile.MaxItemStorage;

        #endregion

        #region Use

        private DelegateCommand<ItemDataWrapper> _useItemCommand;

        public DelegateCommand<ItemDataWrapper> UseItemCommand => _useItemCommand ?? (
            _useItemCommand = new DelegateCommand<ItemDataWrapper>((ItemDataWrapper item) =>
            {
                if (item.ItemId == POGOProtos.Inventory.Item.ItemId.ItemIncenseOrdinary ||
                    item.ItemId == POGOProtos.Inventory.Item.ItemId.ItemIncenseSpicy ||
                    item.ItemId == POGOProtos.Inventory.Item.ItemId.ItemIncenseFloral ||
                    item.ItemId == POGOProtos.Inventory.Item.ItemId.ItemIncenseCool)
                {
                    if (!GameClient.IsIncenseActive)
                    {
                        var dialog = new PoGoMessageDialog("", string.Format(Resources.CodeResources.GetString("ItemUseQuestionText"), Resources.Items.GetString(item.ItemId.ToString())));
                        dialog.AcceptText = Resources.CodeResources.GetString("YesText");
                        dialog.CancelText = Resources.CodeResources.GetString("CancelText");
                        dialog.CoverBackground = true;
                        dialog.AnimationType = PoGoMessageDialogAnimation.Bottom;
                        dialog.AcceptInvoked += async (sender, e) =>
                        {
                            //// Send use request
                            var res = await GameClient.UseIncense(item.ItemId);
                            switch (res.Result)
                            {
                                case UseIncenseResponse.Types.Result.Success:
                                    GameClient.AppliedItems.Add(new AppliedItemWrapper(res.AppliedIncense));
                                    ReturnToGameScreen.Execute();
                                    break;
                                case UseIncenseResponse.Types.Result.IncenseAlreadyActive:
                                    ReturnToGameScreen.Execute();
                                    break;
                                case UseIncenseResponse.Types.Result.LocationUnset:
                                case UseIncenseResponse.Types.Result.NoneInInventory:
                                case UseIncenseResponse.Types.Result.Unknown:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        };

                        dialog.Show();
                    }
                }

                if (item.ItemId == POGOProtos.Inventory.Item.ItemId.ItemLuckyEgg)
                {
                    if (!GameClient.IsXpBoostActive)
                    {
                        var dialog = new PoGoMessageDialog("", string.Format(Resources.CodeResources.GetString("ItemUseQuestionText"), Resources.Items.GetString(item.ItemId.ToString())));
                        dialog.AcceptText = Resources.CodeResources.GetString("YesText");
                        dialog.CancelText = Resources.CodeResources.GetString("CancelText");
                        dialog.CoverBackground = true;
                        dialog.AnimationType = PoGoMessageDialogAnimation.Bottom;
                        dialog.AcceptInvoked += async (sender, e) =>
                        {
                            // Send use request
                            var res = await GameClient.UseXpBoost(item.ItemId);
                            switch (res.Result)
                            {
                                case UseItemXpBoostResponse.Types.Result.Success:
                                    AppliedItem appliedItem = res.AppliedItems.Item.FirstOrDefault<AppliedItem>();
                                    GameClient.AppliedItems.Add(new AppliedItemWrapper(appliedItem));
                                    ReturnToGameScreen.Execute();
                                    break;
                                case UseItemXpBoostResponse.Types.Result.ErrorXpBoostAlreadyActive:
                                    ReturnToGameScreen.Execute();
                                    break;
                                case UseItemXpBoostResponse.Types.Result.ErrorInvalidItemType:
                                case UseItemXpBoostResponse.Types.Result.ErrorLocationUnset:
                                case UseItemXpBoostResponse.Types.Result.ErrorNoItemsRemaining:
                                case UseItemXpBoostResponse.Types.Result.Unset:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        };

                        dialog.Show();
                    }
                }
            }, (ItemDataWrapper item) => true));

        #endregion

        #region Recycle

        private DelegateCommand<ItemDataWrapper> _recycleItemCommand;

        public DelegateCommand<ItemDataWrapper> RecycleItemCommand => _recycleItemCommand ?? (
          _recycleItemCommand = new DelegateCommand<ItemDataWrapper>((ItemDataWrapper item) =>
          {

              var dialog = new PoGoMessageDialog("", string.Format(Resources.CodeResources.GetString("ItemDiscardWarningText"), Resources.Items.GetString(item.ItemId.ToString())));
              var stepper = new StepperMessageDialog(1, item.Count, 1);
              dialog.DialogContent = stepper;
              dialog.AcceptText = Resources.CodeResources.GetString("YesText");
              dialog.CancelText = Resources.CodeResources.GetString("CancelText");
              dialog.CoverBackground = true;
              dialog.AnimationType = PoGoMessageDialogAnimation.Bottom;
              dialog.AcceptInvoked += async (sender, e) =>
              {
                  // Send recycle request
                  var res = await GameClient.RecycleItem(item.ItemId, stepper.Value);
                  switch (res.Result)
                  {
                      case RecycleInventoryItemResponse.Types.Result.Unset:
                          break;
                      case RecycleInventoryItemResponse.Types.Result.Success:
                          // Refresh the Item amount
                          item.WrappedData.Count = res.NewCount;
                          // Hacky? you guessed it...
                          item.Update(item.WrappedData);

                          // Handle if there are no more items of this type
                          if(res.NewCount == 0)
                          {
                              GameClient.ItemsInventory.Remove(item.WrappedData);
                              ItemsInventory.Remove(item);
                          }
                          // Update the total count
                          ItemsTotalCount = ItemsInventory.Sum(i => i.WrappedData.Count);
                          break;
                      case RecycleInventoryItemResponse.Types.Result.ErrorNotEnoughCopies:
                          break;
                      case RecycleInventoryItemResponse.Types.Result.ErrorCannotRecycleIncubators:
                          break;
                      default:
                          throw new ArgumentOutOfRangeException();
                  }
              };

              dialog.Show();
          }, (ItemDataWrapper item) => true));

        #endregion

        #endregion
    }
}