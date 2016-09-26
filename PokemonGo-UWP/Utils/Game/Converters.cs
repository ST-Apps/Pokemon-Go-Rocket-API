using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Google.Common.Geometry;
using Google.Protobuf.Collections;
using POGOProtos.Data;
using POGOProtos.Data.Player;
using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Fort;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils.Game;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Template10.Common;
using Windows.UI.ViewManagement;
using Windows.Graphics.Display;
using Windows.Foundation;
using Windows.Services.Maps;
using System.Threading.Tasks;

namespace PokemonGo_UWP.Utils
{

    public class MapZoomLevelToIconSizeConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var zoomLevel = (double)value;
            var zoomFactor = int.Parse((string)parameter);
            return System.Convert.ToInt32(Math.Ceiling(zoomFactor * (5.02857 * zoomLevel - 53.2571)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonIdToNumericId : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is PokemonId)
                return ((int)value).ToString("D3");
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class ItemClickEventArgsToClickedItemConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            ItemClickEventArgs args = (ItemClickEventArgs)value;
            return args.ClickedItem;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonIdToPokedexDescription : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var path = $"{value.ToString()}/Description";
            var text = Resources.Pokedex.GetString(path);
            return text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonIdToPokemonNameConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Resources.Pokemon.GetString(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonIdToPokemonSpriteConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var s = parameter as string;
            if (s != null && s.Equals("uri"))
                return new Uri($"ms-appx:///Assets/Pokemons/{(int)value}.png");
            return new BitmapImage(new Uri($"ms-appx:///Assets/Pokemons/{(int)value}.png"));
            //return new Uri($"ms-appx:///Assets/Pokemons/{(int)value}.png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonTypeTranslationConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || !(value is Enum))
            {
                return string.Empty;
            }

            var type = (PokemonType)value;
            return Resources.PokemonTypes.GetString(type.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonDataToPokemonTypeBackgroundConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return new Uri("ms-appx:///Assets/Backgrounds/details_type_bg_normal.png");
            var pokemonData = (PokemonDataWrapper)value;
            return new Uri($"ms-appx:///Assets/Backgrounds/details_type_bg_{GameClient.GetExtraDataForPokemon(pokemonData.PokemonId).Type}.png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    #region PokemonDetailControlConverters

    public class PokemonDetailPageViewModeToCloseButtonImageSourceConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BitmapImage close = new BitmapImage(new Uri("ms-appx:///Assets/Buttons/btn_close.png"));
            BitmapImage ok = new BitmapImage(new Uri("ms-appx:///Assets/Buttons/btn_ok.png"));

            if (value == null || !(value is Enum))
            {
                return close;
            }

            var mode = (Views.PokemonDetailPageViewMode)value;
            return mode == Views.PokemonDetailPageViewMode.Normal ? close : ok;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonToCurrentCandiesConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return GameClient.CandyInventory.FirstOrDefault(item => item.FamilyId == GameClient.GetExtraDataForPokemon(((PokemonDataWrapper)value).PokemonId).FamilyId).Candy_;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonToStardustForPowerUpConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return System.Convert.ToInt32(GameClient.PokemonUpgradeSettings.StardustCost[
                    System.Convert.ToInt32(Math.Floor(PokemonInfo.GetLevel(((PokemonDataWrapper)value).WrappedData)) - 1)]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonToStardustForPowerUpForegroundConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var startustToPowerUp =  System.Convert.ToInt32(GameClient.PokemonUpgradeSettings.StardustCost[
                System.Convert.ToInt32(Math.Floor(PokemonInfo.GetLevel(((PokemonDataWrapper)value).WrappedData)) - 1)]);
            return startustToPowerUp > GameClient.PlayerProfile.Currencies.FirstOrDefault(item => item.Name.Equals("STARDUST")).Amount ? new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)) : App.Current.Resources["TitleTextColor"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonToCandiesForPowerUpConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return System.Convert.ToInt32(GameClient.PokemonUpgradeSettings.CandyCost[
                System.Convert.ToInt32(Math.Floor(PokemonInfo.GetLevel(((PokemonDataWrapper)value).WrappedData)) - 1)]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonToCandiesForPowerUpForegroundConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            var extraData = GameClient.GetExtraDataForPokemon(((PokemonDataWrapper)value).PokemonId);
            var candyToPowerUp = System.Convert.ToInt32(GameClient.PokemonUpgradeSettings.CandyCost[
                System.Convert.ToInt32(Math.Floor(PokemonInfo.GetLevel(((PokemonDataWrapper)value).WrappedData)) - 1)]);
            return candyToPowerUp > GameClient.CandyInventory.FirstOrDefault(item => item.FamilyId == extraData.FamilyId).Candy_ ? new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)) : App.Current.Resources["TitleTextColor"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonToCandiesForEvolveConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return 0;
            return GameClient.GetExtraDataForPokemon(((PokemonDataWrapper)value).PokemonId).CandyToEvolve;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonToCandiesForEvolveForegroundConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return new SolidColorBrush(Color.FromArgb(0,0,0,0));
            var extraData = GameClient.GetExtraDataForPokemon(((PokemonDataWrapper)value).PokemonId);
            return extraData.CandyToEvolve > GameClient.CandyInventory.FirstOrDefault(item => item.FamilyId == extraData.FamilyId).Candy_ ? new SolidColorBrush(Color.FromArgb(255,255,0,0)) : App.Current.Resources["TitleTextColor"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonToPokemonFamilyNameStringConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            return Resources.Pokemon.GetString(GameClient.GetExtraDataForPokemon(((PokemonDataWrapper)value).PokemonId).FamilyId.ToString().Replace("Family", "")).ToUpperInvariant();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonToCandyImageConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            return new PokemonFamilyToCandyImageConverter()
                .Convert(GameClient.GetExtraDataForPokemon(((PokemonDataWrapper)value).PokemonId).Type, targetType, parameter, language);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonToTranslatedPokemonTypeConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            PokemonType pokeType;
            if(System.Convert.ToInt32(parameter) == 2)
            {
                pokeType = GameClient.GetExtraDataForPokemon(((PokemonDataWrapper)value).PokemonId).Type2;
            } else
            {
                pokeType = GameClient.GetExtraDataForPokemon(((PokemonDataWrapper)value).PokemonId).Type;
            }
            return new PokemonTypeTranslationConverter().Convert(pokeType, targetType, null, language);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonToPokemonSecondTypeVisibilityConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Visibility.Collapsed;

            Visibility visibility = Visibility.Collapsed;
            if(System.Convert.ToBoolean(parameter))
            {
                visibility = (Visibility)new InverseVisibleWhenTypeIsNotNoneConverter()
                    .Convert(GameClient.GetExtraDataForPokemon(((PokemonDataWrapper)value).PokemonId).Type2, targetType, null, language);
            } else
            {
                visibility = (Visibility)new VisibleWhenTypeIsNotNoneConverter()
                    .Convert(GameClient.GetExtraDataForPokemon(((PokemonDataWrapper)value).PokemonId).Type2, targetType, null, language);
            }

            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonToPokemonEvolveVisibilityConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || (int)new PokemonToCandiesForEvolveConverter()
                .Convert(value, targetType, parameter, language) == 0)
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonToPokemonExceptionalWeightVisibilityConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Visibility.Collapsed;

            PokemonDataWrapper pokemon = (PokemonDataWrapper)value;
            var extraData = GameClient.GetExtraDataForPokemon(pokemon.PokemonId);
            float commonWeight = extraData.PokedexWeightKg;
            float weightStdDev = extraData.WeightStdDev;

            return (pokemon.WeightKg > commonWeight + weightStdDev || pokemon.WeightKg < commonWeight - weightStdDev) ?
                Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonToPokemonExceptionalWeightTagConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";

            PokemonDataWrapper pokemon = (PokemonDataWrapper)value;
            var extraData = GameClient.GetExtraDataForPokemon(pokemon.PokemonId);
            float commonWeight = extraData.PokedexWeightKg;
            float weightStdDev = extraData.WeightStdDev;

            return (pokemon.WeightKg > commonWeight + weightStdDev) ? "XL" : "XS";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonToPokemonExceptionalHeightVisibilityConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Visibility.Collapsed;

            PokemonDataWrapper pokemon = (PokemonDataWrapper)value;
            var extraData = GameClient.GetExtraDataForPokemon(pokemon.PokemonId);
            float commonHeight = extraData.PokedexHeightM;
            float heightStdDev = extraData.HeightStdDev * 2;

            return (pokemon.HeightM > commonHeight + heightStdDev || pokemon.HeightM < commonHeight - heightStdDev) ?
                Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonToPokemonExceptionalHeightTagConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";

            PokemonDataWrapper pokemon = (PokemonDataWrapper)value;
            var extraData = GameClient.GetExtraDataForPokemon(pokemon.PokemonId);
            float commonHeight = extraData.PokedexHeightM;
            float heightStdDev = extraData.HeightStdDev * 2;

            return (pokemon.HeightM > commonHeight + heightStdDev) ? "XL" : "XS";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonCaptureCellToLocationConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(value == null || !(value is ulong))
            {
                return "";
            }

            // We must call this async
            var task = Task.Run(async () =>
            {
                var cellCenter = new S2CellId((ulong)value).ChildEndForLevel(30).ToLatLng();
                MapLocationFinderResult result = await MapLocationFinder.FindLocationsAtAsync(new Geopoint(new BasicGeoposition() { Latitude = cellCenter.LatDegrees, Longitude = cellCenter.LngDegrees }));

                if(result.Status == MapLocationFinderStatus.Success && result.Locations.Count != 0)
                {
                    var captureLocation = result.Locations[0];
                    return $"{captureLocation.Address.Town}, {captureLocation.Address.Region}, {captureLocation.Address.Country}";
                } else
                {
                    return "";
                }
            });

            return task.Result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonToDeploymentImageConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return null;
            bool detailView = false;
            Boolean.TryParse((string)parameter, out detailView);
            PokemonDataWrapper pokemon = (PokemonDataWrapper)value;

            Image img = new Image();
            if(pokemon.IsBuddy)
            {
                // Buddy
                img.Source = new BitmapImage(new Uri("ms-appx:///assets/Icons/ic_buddy.png"));
                img.Margin = detailView ? new Thickness(9) : new Thickness(3);
            } else
            {
                // ArenaDeployment
                switch(GameClient.PlayerProfile.Team)
                {
                    case TeamColor.Red:
                        img.Source = new BitmapImage(new Uri("ms-appx:///assets/Icons/ic_arena_red.png"));
                        break;
                    case TeamColor.Blue:
                        img.Source = new BitmapImage(new Uri("ms-appx:///assets/Icons/ic_arena_blue.png"));
                        break;
                    case TeamColor.Yellow:
                        img.Source = new BitmapImage(new Uri("ms-appx:///assets/Icons/ic_arena_yellow.png"));
                        break;
                    default:
                        return null;
                }

                img.Margin = detailView ? new Thickness(15, 15, 10, 10) : new Thickness(3, 4, 2, 2);
            }

            return img;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    #endregion

    public class PokemonMoveToMoveNameConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return string.Empty;
            var move = (PokemonMove)value;
            return Resources.PokemonMoves.GetString(move.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonMoveToMoveTypeStringConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return string.Empty;
            var move = (PokemonMove)value;
            return new PokemonTypeTranslationConverter()
                .Convert(GameClient.MoveSettings.First(item => item.MovementId == move).PokemonType, targetType, parameter, language);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonMoveToMovePowerConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return string.Empty;
            var move = (PokemonMove)value;
            return GameClient.MoveSettings.First(item => item.MovementId == move).Power;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonDataToMaxLevelBarConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return GameClient.PlayerStats.Level + GameClient.PokemonUpgradeSettings.AllowedLevelsAbovePlayer;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonDataToLevelBarConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return 0;
            var pokemonData = (PokemonDataWrapper)value;
            var pokemonLevel = PokemonInfo.GetLevel(pokemonData.WrappedData);
            return pokemonLevel > GameClient.PlayerStats.Level + 1.5 ? GameClient.PlayerStats.Level + 1.5 : pokemonLevel;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class ItemIdToItemIconConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var itemId = (ItemId)value;
            return new Uri($"ms-appx:///Assets/Items/Item_{(int)itemId}.png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class ItemToItemIconConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var itemId = (value as ItemAward)?.ItemId ?? ((value as ItemData)?.ItemId ?? ((ItemDataWrapper)value).ItemId);
            return new Uri($"ms-appx:///Assets/Items/Item_{(int)itemId}.png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PlayerTeamToTeamColorBrushConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var teamColor = (TeamColor)value;
            return new SolidColorBrush(teamColor == TeamColor.Neutral
                ? Color.FromArgb(255, 26, 237, 213)
                : teamColor == TeamColor.Blue ? Color.FromArgb(255, 40, 89, 237) : teamColor == TeamColor.Red ? Color.FromArgb(255, 237, 90, 90) : Color.FromArgb(255, 254, 196, 50));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PlayerTeamToTeamNameConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var teamColor = (TeamColor)value;

            switch (teamColor)
            {
                case TeamColor.Blue:
                    return Resources.CodeResources.GetString("MysticTeamText");
                case TeamColor.Red:
                    return Resources.CodeResources.GetString("ValorTeamText");
                case TeamColor.Yellow:
                    return Resources.CodeResources.GetString("InstinctTeamText");
                default:
                    return Resources.CodeResources.GetString("NoTeamText");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class AchievementTranslationConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var achievementType = (AchievementType)value;
            var type = achievementType.GetType();
            var fieldInfo = type.GetField(achievementType.ToString());
            var badgeType =
                (BadgeTypeAttribute)fieldInfo.GetCustomAttributes(typeof(BadgeTypeAttribute), false).First();

            return badgeType == null ? "" : Resources.Achievements.GetString(badgeType.Value.ToString()).ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class AchievementDescriptionTranslationConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var achievementType = (KeyValuePair<AchievementType, object>)value;
            var type = achievementType.Key.GetType();
            var fieldInfo = type.GetField(achievementType.Key.ToString());
            var badgeType =
                (BadgeTypeAttribute)fieldInfo.GetCustomAttributes(typeof(BadgeTypeAttribute), false).First();

            return badgeType == null ? "" : string.Format(Resources.Achievements.GetString(badgeType.Value + "Description"), new AchievementValueConverter().Convert(achievementType.Value, null, null, string.Empty));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class AchievementValueToMedalImageConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var achievement = (KeyValuePair<AchievementType, object>)value;
            var type = achievement.Key.GetType();
            var fieldInfo = type.GetField(achievement.Key.ToString());
            var bronze = (BronzeAttribute)fieldInfo.GetCustomAttributes(typeof(BronzeAttribute), false).First();
            var silver = (SilverAttribute)fieldInfo.GetCustomAttributes(typeof(SilverAttribute), false).First();
            var gold = (GoldAttribute)fieldInfo.GetCustomAttributes(typeof(GoldAttribute), false).First();
            int level = 3;
            if (achievement.Value == null)
            {
                return new BitmapImage(new Uri("ms-appx:///Assets/Achievements/badge_lv0.png"));
            }
            if (float.Parse(achievement.Value.ToString()) < float.Parse(bronze.Value.ToString()))
            {
                return new BitmapImage(new Uri("ms-appx:///Assets/Achievements/badge_lv0.png"));
            }
            if (float.Parse(achievement.Value.ToString()) < float.Parse(gold.Value.ToString()))
            {
                level = 2;
            }
            if (float.Parse(achievement.Value.ToString()) < float.Parse(silver.Value.ToString()))
            {
                level = 1;
            }

            switch (achievement.Key.ToString().ToLower().Replace(" ", ""))
            {
                case "acetrainer":
                case "backpacker":
                case "battlegirl":
                case "breeder":
                case "collector":
                case "fisherman":
                case "jogger":
                case "kanto":
                case "pikachufan":
                case "scientist":
                case "youngster":
                    return new BitmapImage(new Uri("ms-appx:///Assets/Achievements/" + achievement.Key.ToString().ToLower().Replace(" ", "") + "_lv" + level.ToString() + ".png"));
                default:
                    return new BitmapImage(new Uri("ms-appx:///Assets/Achievements/badge_lv" + level.ToString() + ".png"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class AchievementNextValueConverter : IValueConverter
    {
        private static readonly Type achievementType = typeof(AchievementType);

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var achievement = (KeyValuePair<AchievementType, object>)value;

            if (achievement.Value == null)
                return 0;

            var fieldInfo = achievementType.GetField(achievement.Key.ToString());
            var achievementValueAttributes = fieldInfo.GetCustomAttributes<AchievementValueAttribute>().ToList();
            AchievementValueAttribute achievementValue;
            Func<string> stringConverter;
            if (achievement.Value is float)
            {
                achievementValue = achievementValueAttributes.SkipWhile((x, i) => (int)x.Value < (float)achievement.Value && i < 2).First();
                stringConverter = () => float.Parse(achievementValue.Value.ToString()).ToString("N1");
            }
            else if (achievement.Value is double)
            {
                achievementValue = achievementValueAttributes.SkipWhile((x, i) => (int)x.Value < (double)achievement.Value && i < 2).First();
                stringConverter = () => double.Parse(achievementValue.Value.ToString()).ToString("N1");
            }
            else
            {
                achievementValue = achievementValueAttributes.SkipWhile((x, i) => (int)x.Value < int.Parse(achievement.Value.ToString()) && i < 2).First();
                stringConverter = () => achievementValue.Value.ToString();
            }
            Func<int> intConverter = () => System.Convert.ToInt32(achievementValue.Value);

            if (targetType == typeof(string))
                return stringConverter();
            if (targetType == typeof(int))
                return intConverter();

            return achievementValue.Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class AchievementValueConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return 0;
            }
            if (value is float)
            {
                return ((float)value).ToString("N1");
            }
            if (value is double)
            {
                return ((double)value).ToString("N1");
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class AchievementToCompletedPercentageConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var achievement = (KeyValuePair<AchievementType, object>)value;
            var currentValueConverted = new AchievementValueConverter().Convert(achievement.Value, null, null, "");
            var nextValueConverted = new AchievementNextValueConverter().Convert(achievement, null, null, "");
            double currentValue, nextValue;
            currentValue = currentValueConverted is string
                ? double.Parse((string)currentValueConverted)
                : currentValueConverted as int? ?? (byte)currentValueConverted;

            nextValue = nextValueConverted is string
                ? double.Parse((string)nextValueConverted)
                : nextValueConverted as int? ?? (byte)nextValueConverted;
            return currentValue / nextValue * 100;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PlayerTeamToTeamImageConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var teamColor = (TeamColor)value;
            var path = "ms-appx:///Assets/Teams/";

            switch (teamColor)
            {
                case TeamColor.Blue:
                    path += "mystic";
                    break;
                case TeamColor.Red:
                    path += "valor";
                    break;
                case TeamColor.Yellow:
                    path += "instinct";
                    break;
                default:
                    path += "no-team";
                    break;
            }
            path += ".png";

            return new BitmapImage(new Uri(path));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PlayerTeamToTeamBaseImageConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var teamColor = (TeamColor)value;
            var path = "ms-appx:///Assets/Teams/";

            switch (teamColor)
            {
                case TeamColor.Blue:
                    path += "gym_base_blue";
                    break;
                case TeamColor.Red:
                    path += "gym_base_red";
                    break;
                case TeamColor.Yellow:
                    path += "gym_base_yellow";
                    break;
                default:
                    path += "gym_base_empty";
                    break;
            }
            path += ".png";

            return new BitmapImage(new Uri(path));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PlayerTeamToTeamBackgroundImageConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var teamColor = (TeamColor)value;
            var path = "ms-appx:///Assets/Teams/";

            switch (teamColor)
            {
                case TeamColor.Blue:
                    path += "mystic";
                    break;
                case TeamColor.Red:
                    path += "valor";
                    break;
                case TeamColor.Yellow:
                    path += "instinct";
                    break;
                default:
                    return null;
            }
            path += ".png";

            return new BitmapImage(new Uri(path));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PlayerStatsTranslationConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Resources.CodeResources.GetString((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class EncounterBackgroundToBackgroundImageConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return new Uri($"ms-appx:///Assets/Backgrounds/{(EncounterResponse.Types.Background)value}.png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class ItemToItemNameConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var itemId = (value as ItemAward)?.ItemId ?? ((value as ItemData)?.ItemId ?? ((ItemDataWrapper)value).ItemId);
            return Resources.Items.GetString(itemId.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class ItemToItemDescriptionConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var itemId = (value as ItemAward)?.ItemId ?? ((value as ItemData)?.ItemId ?? ((ItemDataWrapper)value).ItemId);
            return Resources.Items.GetString("D_" + itemId);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class ItemToItemRecycleVisibilityConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var itemId = (value as ItemAward)?.ItemId ?? ((value as ItemData)?.ItemId ?? ((ItemDataWrapper)value).ItemId);
            switch(itemId)
            {
                case ItemId.ItemUnknown:
                case ItemId.ItemSpecialCamera:
                case ItemId.ItemIncubatorBasicUnlimited:
                case ItemId.ItemIncubatorBasic:
                    return Visibility.Collapsed;
                default:
                    return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class ItemUseabilityToOpacityConverter : DependencyObject, IValueConverter
    {
        public ViewModels.ItemsInventoryPageViewModel.ItemsInventoryViewMode CurrentViewMode
        {
            get { return (ViewModels.ItemsInventoryPageViewModel.ItemsInventoryViewMode)GetValue(CurrentViewModeProperty); }
            set { SetValue(CurrentViewModeProperty, value); }
        }

        public static readonly DependencyProperty CurrentViewModeProperty =
            DependencyProperty.Register("CurrentViewMode",
                                        typeof(ViewModels.ItemsInventoryPageViewModel.ItemsInventoryViewMode),
                                        typeof(ItemUseabilityToOpacityConverter),
                                        new PropertyMetadata(null));

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is ItemDataWrapper)) return 1;
            
            var useableList = CurrentViewMode == ViewModels.ItemsInventoryPageViewModel.ItemsInventoryViewMode.Normal ? GameClient.NormalUseItemIds : GameClient.CatchItemIds;
            return useableList.Contains(((ItemDataWrapper)value).ItemId) ? 1 : 0.5;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class CaptureXpToTotalCaptureXpConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var xp = (RepeatedField<int>)value;
            return xp.Sum();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class ActivityTypeToActivityNameConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var activityType = (ActivityType)value;
            var resActivityType = Resources.CodeResources.GetString(activityType.ToString()); ;
            return string.IsNullOrEmpty(resActivityType) ? activityType.ToString().Replace("Activity", "") : resActivityType;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    /// <summary>
    ///     Converts a generic map object to its coordinates (Geopoint).
    ///     Converter parameter tells if we're converting a MapPokemon or a FortData
    /// </summary>
    public class MapObjectToGeopointConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var objectType = (string)parameter;
            var geoposition = new BasicGeoposition();
            if (objectType.Equals("pokemon"))
            {
                var pokemon = (MapPokemon)value;
                geoposition.Latitude = pokemon.Latitude;
                geoposition.Longitude = pokemon.Longitude;
            }
            else if (objectType.Equals("pokestop"))
            {
                var pokestop = (FortData)value;
                geoposition.Latitude = pokestop.Latitude;
                geoposition.Longitude = pokestop.Longitude;
            }
            return new Geopoint(geoposition);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonFamilyToCandyImageConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            var pokemonType = (PokemonType)value;
            return $"ms-appx:///Assets/Candy/{pokemonType}.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class InverseVisibleWhenTypeIsNotNoneConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Visibility.Collapsed;
            var pokemonType = (PokemonType)value;
            return pokemonType != PokemonType.None ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class VisibleWhenTypeIsNotNoneConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Visibility.Collapsed;
            var pokemonType = (PokemonType)value;
            return pokemonType == PokemonType.None ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonFamilyNameToStringConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            var pokemonFamily = (PokemonFamilyId)value;
            return Resources.Pokemon.GetString(pokemonFamily.ToString().Replace("Family", "")).ToUpperInvariant();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonDataToVisibilityConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (PokemonData)value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class NearbyPokemonDistanceToDistanceImageConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var distance = (int)(float)value;
            var distanceString = distance < 125 ? (distance < 70 ? "Near" : "Mid") : "Far";
            return new Uri($"ms-appx:///Assets/Icons/Footprint_{distanceString}.png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class InverseNearbyPokemonInPokedexToVisibilityConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Visibility.Collapsed;
            // Get the Pokemon passed in. If it's null, you can't see the real deal.
            var pokemon = (PokemonId)value;

            // Get the Pokedex entry for this Pokemon. If it's null, you can't see the real deal.
            var entry = GameClient.PokedexInventory.FirstOrDefault(c => c.PokemonId == pokemon);
            if (entry == null) return Visibility.Visible;

            // Get the Pokedex entry for this Pokemon. If it's null, you can't see the real deal.
            if (entry.TimesCaptured == 0) return Visibility.Visible;

            // You've cleared all the reasons NOT to show it. So show it.
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class NearbyPokemonInPokedexToVisibilityConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Visibility.Collapsed;
            // Get the Pokemon passed in. If it's null, you can't see the real deal.
            var pokemon = (PokemonId)value;

            // Get the Pokedex entry for this Pokemon. If it's null, you can't see the real deal.
            var entry = GameClient.PokedexInventory.FirstOrDefault(c => c.PokemonId == pokemon);
            if (entry == null) return Visibility.Collapsed;

            // Get the Pokedex entry for this Pokemon. If it's null, you can't see the real deal.
            if (entry.TimesCaptured == 0) return Visibility.Collapsed;

            // You've cleared all the reasons NOT to show it. So show it.
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokestopToIconConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var fortDataStatus = (FortDataStatus)value;
            var resourceUriString = "ms-appx:///Assets/Icons/pokestop_";

            switch (fortDataStatus)
            {
                case FortDataStatus.Opened:
                    return new Uri(resourceUriString + "near.png");
                case FortDataStatus.Closed:
                    return new Uri(resourceUriString + "far.png");
                case FortDataStatus.Opened | FortDataStatus.Cooldown:
                    return new Uri(resourceUriString + "near_inactive.png");
                case FortDataStatus.Closed | FortDataStatus.Cooldown:
                    return new Uri(resourceUriString + "far_inactive.png");
                case FortDataStatus.Opened | FortDataStatus.Lure:
                    return new Uri(resourceUriString + "near_lured.png");
                case FortDataStatus.Closed | FortDataStatus.Lure:
                    return new Uri(resourceUriString + "far_lured.png");
                case FortDataStatus.Opened | FortDataStatus.Cooldown | FortDataStatus.Lure:
                    return new Uri(resourceUriString + "near_inactive_lured.png");
                case FortDataStatus.Closed | FortDataStatus.Cooldown | FortDataStatus.Lure:
                    return new Uri(resourceUriString + "far_inactive_lured.png");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class GymToIconConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var teamColor = (TeamColor)value;
            var resourceUriString = "ms-appx:///Assets/Icons/hint_gym";

            switch (teamColor)
            {
                case TeamColor.Red:
                    return new Uri(resourceUriString + "_red.png");
                case TeamColor.Yellow:
                    return new Uri(resourceUriString + "_yellow.png");
                case TeamColor.Blue:
                    return new Uri(resourceUriString + "_blue.png");
                case TeamColor.Neutral:
                    return new Uri(resourceUriString + ".png");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class CurrentTimeToMapColorSchemeConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var currentTime = int.Parse(DateTime.Now.ToString("HH"));
            return currentTime > 7 && currentTime < 19 ? MapColorScheme.Light : MapColorScheme.Dark;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonDataToPokemonStaminaConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return 0;
            var pokemon = (PokemonDataWrapper)value;
            return System.Convert.ToDouble(pokemon.Stamina / pokemon.StaminaMax * 100);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class EggDataToEggIconConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string uri;
            if (value == null) uri = "ms-appx:///Assets/Items/Egg.png";
            else
            {
                var egg = (PokemonDataWrapper)value;
                uri = string.IsNullOrEmpty(egg.EggIncubatorId)
                    ? "ms-appx:///Assets/Items/Egg.png"
                    : $"ms-appx:///Assets/Items/E_Item_{(int)GameClient.GetIncubatorFromEgg(egg.WrappedData).ItemId}.png";
            }
            return new BitmapImage(new Uri(uri));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class EggDataToEggProgressConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || !(value is IncubatedEggDataWrapper)) return 0.0;
            var pokemon = (IncubatedEggDataWrapper)value;
            return (double)((pokemon.EggKmWalkedStart / pokemon.EggKmWalkedTarget) * 100);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonSortingModesToIconConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var sortingMode = (PokemonSortingModes)value;
            return new Uri($"ms-appx:///Assets/Icons/ic_{sortingMode.ToString().ToLowerInvariant()}.png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonSortingModesTranslationConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var sortingMode = (PokemonSortingModes)value;
            return Resources.CodeResources.GetString(sortingMode.ToString()).ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class IncubatorUsagesCountToUsagesTextConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return string.Format(Resources.CodeResources.GetString("IncubatorRemainingUsesText"), ((EggIncubator)value).UsesRemaining);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }

        #endregion
    }

    public class IncubatorUsageToOpacityConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((EggIncubator)value).PokemonId == 0 ? 1.0 : 0.5;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }

        #endregion
    }

    public class IncubatorUnlimitedTypeToVisibilityConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool invert = false;
            Boolean.TryParse((string)parameter, out invert);
            
            if ((((EggIncubator)value).ItemId == ItemId.ItemIncubatorBasicUnlimited) ^ invert)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }

        #endregion
    }

    public class VisibleWhenGreaterThanConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (int)value > (parameter != null ? System.Convert.ToInt32(parameter) : 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class EmptyConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class StringFormatConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return string.Format(parameter as string, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }

        #endregion
    }

    public class PlayerDataToCurrentExperienceConverter : IValueConverter
    {
        // TODO: find a better place for this
        private readonly int[] _xpTable =
        {
            0, 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000, 10000, 10000, 10000, 10000, 15000, 20000, 20000, 20000, 25000, 25000, 50000, 75000, 100000, 125000, 150000, 190000, 200000, 250000, 300000, 350000
        };

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var playerStats = (PlayerStats)value;
            //return playerStats?.Experience - playerStats?.PrevLevelXp ?? 0;
            return playerStats == null ? 0 : _xpTable[playerStats.Level] - (playerStats.NextLevelXp - playerStats.Experience);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PlayerDataToTotalLevelExperienceConverter : IValueConverter
    {
        // TODO: find a better place for this
        private readonly int[] _xpTable =
        {
            0, 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000, 10000, 10000, 10000, 10000, 15000, 20000, 20000, 20000, 25000, 25000, 50000, 75000, 100000, 125000, 150000, 190000, 200000, 250000, 300000, 350000
        };

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var playerStats = (PlayerStats)value;
            return playerStats == null ? 0 : _xpTable[playerStats.Level];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PlayerDataToPokeCoinsConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var playerData = (PlayerData)value;
            return playerData?.Currencies.First(item => item.Name.Equals("POKECOIN")).Amount ?? 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class MsToDateFormatConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var ms = System.Convert.ToUInt64(value);
            var date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
            date = date.Add(TimeSpan.FromMilliseconds(ms));
            return date.ToString(Resources.CodeResources.GetString("DateFormat"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    //parameter is optional
    //Use parameter to pass margin between items and page margin
    //format: itemsMargin,pageMargins
    //first is margin between items, the second is page margins
    //ex: items margin = 8,4,8,8 | page margins = 12,0,12,0 | parameter will be 16,24
    public class WidthConverter : IValueConverter
    {
        private static readonly char[] splitValue = { ',' };

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int minColumns = (int)value;
            double externalMargin = 0;
            double internalMargin = 0;
            if (!string.IsNullOrEmpty(parameter as string))
            {
                string[] margins = parameter.ToString().Split(splitValue, StringSplitOptions.RemoveEmptyEntries);
                if (margins.Length > 0)
                    internalMargin = Double.Parse(margins[0]);
                if (margins.Length > 1)
                    externalMargin = Double.Parse(margins[1]);
            }

            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            var scaleFactor = 1;//DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            var size = new Size(bounds.Width * scaleFactor, bounds.Height * scaleFactor);
            var res = ((size.Width - externalMargin) / minColumns) - internalMargin;

            //https://msdn.microsoft.com/en-us/windows/uwp/layout/design-and-ui-intro#effective-pixels-and-scaling
            var width = ((int)res / 4) * 4; //round to 4 - win 10 optimized 
            return width;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class IntToBooleanConverter : IValueConverter {

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language) {
            return value.Equals(1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class PokemonLastHoursVisibiltyConverter : IValueConverter {
       
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language) {
            if(value == null) {
                return Visibility.Collapsed;
            }
            var ms = System.Convert.ToUInt64(value);
            var creationDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
            creationDate = creationDate.Add(TimeSpan.FromMilliseconds(ms));
            var now = DateTime.Now;
            if (now.AddDays(-1) <= creationDate) {
                return Visibility.Visible;
            } else {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class PokemonTypeToBackgroundImageConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || !(value is PokemonType)) return new Uri("ms-appx:///Assets/Backgrounds/details_type_bg_normal.png");
            var type = (PokemonType)value;
            return new Uri($"ms-appx:///Assets/Backgrounds/details_type_bg_{type}.png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        private object GetVisibility(object value, bool invert)
        {
            if (!(value is bool))
                return Visibility.Collapsed;
            bool objValue = (bool)value;
            if (objValue ^ invert)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool invert = false;
            Boolean.TryParse((string)parameter, out invert);

            return GetVisibility(value, invert);
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class StringToVisibilityConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool invert = false;
            Boolean.TryParse((string)parameter, out invert);
            
            if (string.IsNullOrEmpty((string)value) ^ invert)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
