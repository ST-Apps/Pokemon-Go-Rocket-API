using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PokemonGo_UWP.Controls {
    public sealed partial class PoGoPivotHeader : UserControl {
        public PoGoPivotHeader() {
            this.InitializeComponent();
        }

        #region DependencyProperties

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(PoGoPivotHeader),
                new PropertyMetadata(null));

        public static readonly DependencyProperty DataCurrentProperty =
            DependencyProperty.Register(nameof(DataCurrent), typeof(string), typeof(PoGoPivotHeader),
                new PropertyMetadata(null));

        public static readonly DependencyProperty DataMaximumProperty =
            DependencyProperty.Register(nameof(DataMaximum), typeof(string), typeof(PoGoPivotHeader),
                new PropertyMetadata(null));

        #endregion

        #region Properties

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public string DataCurrent
        {
            get { return (string)GetValue(DataCurrentProperty); }
            set { SetValue(DataCurrentProperty, value); }
        }

        public string DataMaximum
        {
            get { return (string)GetValue(DataMaximumProperty); }
            set { SetValue(DataMaximumProperty, value); }
        }

        #endregion
    }
}
