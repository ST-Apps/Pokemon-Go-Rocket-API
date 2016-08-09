using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Google.Protobuf.Collections;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo_UWP.Entities;
using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Fort;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;
using Windows.UI.Xaml.Media.Imaging;
using System.Reflection;

namespace PokemonGo_UWP.Utils
{

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
            return new Uri($"ms-appx:///Assets/Pokemons/{(int) value}.png");
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
            var itemId = (ItemId) value;
            return new Uri($"ms-appx:///Assets/Items/Item_{(int) itemId}.png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class ItemAwardToItemIconConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var itemId = ((ItemAward)value).ItemId;
            return new Uri($"ms-appx:///Assets/Items/Item_{(int) itemId}.png");
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
            var teamColor = (TeamColor) value;
            var currentTime = int.Parse(DateTime.Now.ToString("HH"));
            var noTeamColor =  currentTime > 7 && currentTime < 19 ? Colors.Black : Colors.White;
            return new SolidColorBrush(teamColor == TeamColor.Neutral
                ? noTeamColor
                : teamColor == TeamColor.Blue ? Colors.Blue : teamColor == TeamColor.Red ? Colors.Red : Colors.Yellow);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PlayerTeamToTeamNameConverter : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language) {
            var teamColor = (TeamColor)value;

            switch(teamColor) {
                case TeamColor.Blue:
                    return Resources.Translation.GetString("MysticTeam");
                case TeamColor.Red:
                    return Resources.Translation.GetString("ValorTeam");
                case TeamColor.Yellow:
                    return Resources.Translation.GetString("InstinctTeam");
                default:
                    return Resources.Translation.GetString("NoTeam");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value;
        }

        #endregion
    }

    public class AchievementTranslationConverter : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language) {
            AchievementType achievementType = (AchievementType) value;
            Type type = achievementType.GetType();
            FieldInfo fieldInfo = type.GetField(achievementType.ToString());
            BadgeTypeAttribute badgeType = (BadgeTypeAttribute)fieldInfo.GetCustomAttributes(typeof(BadgeTypeAttribute), false).First();
            
            if(badgeType == null) {
                return "";
            }
            
            return Resources.Translation.GetString(badgeType.Value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value;
        }

        #endregion
    }

    public class AchievementValueToMedalImageConverter : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language) {
            KeyValuePair<AchievementType, object> achievement = (KeyValuePair<AchievementType, object>)value;
            Type type = achievement.Key.GetType();
            FieldInfo fieldInfo = type.GetField(achievement.Key.ToString());
            BronzeAttribute bronze = (BronzeAttribute)fieldInfo.GetCustomAttributes(typeof(BronzeAttribute), false).First();
            SilverAttribute silver = (SilverAttribute)fieldInfo.GetCustomAttributes(typeof(SilverAttribute), false).First();
            GoldAttribute gold = (GoldAttribute)fieldInfo.GetCustomAttributes(typeof(GoldAttribute), false).First();

            if(value == null) {
                return new BitmapImage(new Uri("ms-appx:///Assets/Achievements/badge_lv0.png"));
            }
            if(float.Parse(achievement.Value.ToString()) < float.Parse(bronze.Value.ToString())) {
                return new BitmapImage(new Uri("ms-appx:///Assets/Achievements/badge_lv0.png"));
            } else if(float.Parse(achievement.Value.ToString()) < float.Parse(silver.Value.ToString())) {
                return new BitmapImage(new Uri("ms-appx:///Assets/Achievements/badge_lv1.png"));
            } else if(float.Parse(achievement.Value.ToString()) < float.Parse(gold.Value.ToString())) {
                return new BitmapImage(new Uri("ms-appx:///Assets/Achievements/badge_lv2.png"));
            } else {
                return new BitmapImage(new Uri("ms-appx:///Assets/Achievements/badge_lv3.png"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value;
        }

        #endregion
    }

    public class AchievementNextValueConverter : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language) {
            KeyValuePair<AchievementType, object> achievement = (KeyValuePair<AchievementType, object>) value;
            Type type = achievement.Key.GetType();
            FieldInfo fieldInfo = type.GetField(achievement.Key.ToString());
            BronzeAttribute bronze = (BronzeAttribute)fieldInfo.GetCustomAttributes(typeof(BronzeAttribute), false).First();
            SilverAttribute silver = (SilverAttribute)fieldInfo.GetCustomAttributes(typeof(SilverAttribute), false).First();
            GoldAttribute gold = (GoldAttribute)fieldInfo.GetCustomAttributes(typeof(GoldAttribute), false).First();

            if(value == null) {
                return 0;
            }
            if(typeof(float) == achievement.Value.GetType()) {
                if(float.Parse(achievement.Value.ToString()) < float.Parse(bronze.Value.ToString())) {
                    return float.Parse(bronze.Value.ToString()).ToString("N1");
                } else if(float.Parse(achievement.Value.ToString()) < float.Parse(silver.Value.ToString())) {
                    return float.Parse(silver.Value.ToString()).ToString("N1");
                } else if(float.Parse(achievement.Value.ToString()) < float.Parse(gold.Value.ToString())) {
                    return float.Parse(gold.Value.ToString()).ToString("N1");
                } else {
                    return float.Parse(gold.Value.ToString()).ToString("N1");
                }
            } else {
                if(int.Parse(achievement.Value.ToString()) < int.Parse(bronze.Value.ToString())) {
                    return bronze.Value;
                } else if(int.Parse(achievement.Value.ToString()) < int.Parse(silver.Value.ToString())) {
                    return silver.Value;
                } else if(int.Parse(achievement.Value.ToString()) < int.Parse(gold.Value.ToString())) {
                    return gold.Value;
                } else {
                    return gold.Value;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value;
        }

        #endregion
    }

    public class AchievementValueConverter : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language) {
               
            if(value == null) {
                return 0;
            }
            if(typeof(float) == value.GetType()) {
                    return float.Parse(value.ToString()).ToString("N1");
            } else {
                    return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value;
        }

        #endregion
    }

    public class PlayerTeamToTeamImageConverter : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language) {
            var teamColor = (TeamColor)value;
            var currentTime = int.Parse(DateTime.Now.ToString("HH"));
            var noTeamColor = currentTime > 7 && currentTime < 19 ? Colors.Black : Colors.White;
            var path = "ms-appx:///Assets/Teams/";

            switch(teamColor) {
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

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value;
        }

        #endregion
    }

    public class PlayerTeamToTeamBackgroundImageConverter : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language) {
            var teamColor = (TeamColor)value;
            var currentTime = int.Parse(DateTime.Now.ToString("HH"));
            var noTeamColor = currentTime > 7 && currentTime < 19 ? Colors.Black : Colors.White;
            var path = "ms-appx:///Assets/Teams/";

            switch(teamColor) {
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

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value;
        }

        #endregion
    }

    public class PlayerStatsTranslationConverter : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language) {
            return Resources.Translation.GetString((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value;
        }

        #endregion
    }

    public class EncounterBackgroundToBackgroundImageConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return new Uri($"ms-appx:///Assets/Backgrounds/{(EncounterResponse.Types.Background) value}.png");
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
            var itemId = value is ItemAward ? ((ItemAward) value).ItemId : ((ItemData) value).ItemId;            
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
            var itemId = value is ItemAward ? ((ItemAward)value).ItemId : ((ItemData)value).ItemId;
            return Resources.Items.GetString("D_" + itemId);
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
            var xp = (RepeatedField<int>) value;
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
            var activityType = (ActivityType) value;
            return activityType.ToString().Replace("Activity", "");
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
            var objectType = (string) parameter;
            var geoposition = new BasicGeoposition();
            if (objectType.Equals("pokemon"))
            {
                var pokemon = (MapPokemon) value;
                geoposition.Latitude = pokemon.Latitude;
                geoposition.Longitude = pokemon.Longitude;
            }
            else if (objectType.Equals("pokestop"))
            {
                var pokestop = (FortData) value;
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

    public class PokemonDataToVisibilityConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (PokemonData) value != null ? Visibility.Visible : Visibility.Collapsed;
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
            var distance = (int) (float) value;
            var distanceString = distance < 125 ? (distance < 70 ? "Near" : "Mid") : "Far";
            return new Uri($"ms-appx:///Assets/Icons/Footprint_{distanceString}.png");
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
            var pokestop = (FortDataWrapper)value;            
            var distance = GeoExtensions.GeoAssist.CalculateDistanceBetweenTwoGeoPoints(pokestop.Geoposition,
                GameClient.Geoposition.Coordinate.Point);
            if (distance > GameClient.GameSetting.FortSettings.InteractionRangeMeters)
                return new Uri("ms-appx:///Assets/Icons/pokestop_far.png");
            var mode = pokestop.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime() ? "" : "_inactive";
            return new Uri($"ms-appx:///Assets/Icons/pokestop_near{mode}.png");
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
            return (currentTime > 7 && currentTime < 19) ? MapColorScheme.Light : MapColorScheme.Dark;
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
            var pokemon = (PokemonDataWrapper) value;
            return (int) (pokemon.Stamina/(double) pokemon.StaminaMax)*100;
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
            if (value == null) return "ms-appx:///Assets/Items/Egg.png";
            var egg = (PokemonDataWrapper)value;
            return string.IsNullOrEmpty(egg.EggIncubatorId) ? "ms-appx:///Assets/Items/Egg.png" : $"ms-appx:///Assets/Items/E_Item_{(int)GameClient.GetIncubatorFromEgg(egg.WrappedData).ItemId}.png";
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
            if (value == null) return 0;
            var pokemon = (PokemonDataWrapper)value;
            return (int)(pokemon.EggKmWalkedStart / pokemon.EggKmWalkedTarget) * 100;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }


    public class PokemonSortingModesToSortingModesListConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Enum.GetValues(typeof(PokemonSortingModes)).Cast<PokemonSortingModes>().ToList();
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
            var sortingMode = (PokemonSortingModes) value;            
            return new Uri($"ms-appx:///Assets/Icons/ic_{sortingMode.ToString().ToLowerInvariant()}.png");
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
            var incubator = (EggIncubator) value;
            return incubator.ItemId == ItemId.ItemIncubatorBasicUnlimited ? "∞" : $"{incubator.UsesRemaining}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }

        #endregion
    }

    public class VisibleWhenStringNotEmptyConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return string.IsNullOrEmpty((string) value) ? Visibility.Collapsed : Visibility.Visible;
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


    public class MsToDateFormatConverter : IValueConverter {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, string language) {
            long ms = (long)value;
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
            date = date.Add(TimeSpan.FromMilliseconds(ms));
            return date.ToString(Resources.Translation.GetString("DateFormat"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value;
        }

        #endregion
    }
}