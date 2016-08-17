using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using POGOProtos.Inventory.Item;
using Template10.Common;
using Template10.Mvvm;

namespace PokemonGo_UWP.Entities
{
    public class ItemDataWrapper
    {
        private DelegateCommand _gotoDiscardCommand;

        public ItemDataWrapper(ItemData itemData)
        {
            WrappedData = itemData;
        }

        public ItemData WrappedData { get; }

        /// <summary>
        ///     Navigate to detail page for the selected egg
        /// </summary>
        public DelegateCommand GotoDiscardCommand => _gotoDiscardCommand ?? (
            _gotoDiscardCommand = new DelegateCommand(() =>
            {
                //NavigationHelper.NavigationState["CurrentEgg"] = this;
                //BootStrapper.Current.NavigationService.Navigate(typeof(EggDetailPage), true);
            }, () => true));

        #region Wrapped Properties

        public ItemId ItemId => WrappedData.ItemId;

        public int Count => WrappedData.Count;

        public bool Unseen => WrappedData.Unseen;

        #endregion
    }
}