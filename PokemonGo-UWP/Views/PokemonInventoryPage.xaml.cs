using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
	/// <summary>
	///     An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class PokemonInventoryPage : Page
	{
		public PokemonInventoryPage()
		{
			InitializeComponent();

			NavigationCacheMode = NavigationCacheMode.Enabled;

			// TODO: fix header
			// Setup incubators translation
			Loaded += (s, e) =>
			{
				ShowIncubatorsModalAnimation.From =
									HideIncubatorsModalAnimation.To = IncubatorsModal.ActualHeight;
				HideIncubatorsModalStoryboard.Completed += (ss, ee) => { IncubatorsModal.IsModal = false; };
			};
		}

		private void ToggleIncubatorModel(object sender, TappedRoutedEventArgs e)
		{
			if (IncubatorsModal.IsModal)
			{
				HideIncubatorsModalStoryboard.Begin();
			}
			else
			{
				IncubatorsModal.IsModal = true;
				ShowIncubatorsModalStoryboard.Begin();
			}
		}

		#region Overrides of Page

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
		}

		private void OnBackRequested(object sender, BackRequestedEventArgs backRequestedEventArgs)
		{
			if (!(SortMenuPanel.Opacity > 0)) return;
			backRequestedEventArgs.Handled = true;
			HideSortMenuStoryboard.Begin();
		}

		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			base.OnNavigatingFrom(e);
			SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
		}

		#endregion

		private void SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

			SortingButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
			IncubatorButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

			switch (((Pivot)sender).SelectedIndex)
			{
				case 0:
					SortingButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
					break;

				case 1:
					IncubatorButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
					break;
			}
		}

		private void GridViewPokemonSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (e.PreviousSize != e.NewSize)
			{
				var panel_threads = (ItemsWrapGrid)PokemonInventoryGridView.ItemsPanelRoot;
				panel_threads.ItemWidth = e.NewSize.Width / 3;
			}
		}

		private void GridViewEggsSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (e.PreviousSize != e.NewSize)
			{
				var panel_threads = (ItemsWrapGrid)EggsInventoryGridView.ItemsPanelRoot;
				panel_threads.ItemWidth = e.NewSize.Width / 4;
			}
		}

	}
}