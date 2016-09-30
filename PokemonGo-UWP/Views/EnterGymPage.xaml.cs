using PokemonGo_UWP.Utils;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EnterGymPage : Page
    {
        private ScrollViewer _scroller;
        private int _lastMemberIndex = -1;

        public EnterGymPage()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                // Of course binding doesn't work so we need to manually setup height for animations
            };
        }

        #region Overrides of Page

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }


        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        #endregion

        private void GymDefendersFlip_Loaded(object sender, RoutedEventArgs e)
        {
            _scroller = GetScrollViewer(GymDefendersFlip);
            _scroller.ViewChanged += Scroller_ViewChanged;
        }

        private void Scroller_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var index = (int)Math.Round(_scroller.HorizontalOffset - 2);
            var capIndex = Utilities.EnsureRange(index, 0, ViewModel.CurrentMembers.Count - 1);
            if (capIndex != _lastMemberIndex)
            {
                foreach (var item in ViewModel.CurrentMembers.Where(cm => cm.Selected))
                    item.Selected = false;

                if (ViewModel.CurrentMembers[capIndex] != null)
                {
                    ViewModel.CurrentMembers[capIndex].Selected = true;
                    _lastMemberIndex = capIndex;
                }
            }
        }

        // method to pull out a ScrollViewer from FlipView
        public static ScrollViewer GetScrollViewer(DependencyObject depObj)
        {
            if (depObj is ScrollViewer) return depObj as ScrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = GetScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }

    }
}