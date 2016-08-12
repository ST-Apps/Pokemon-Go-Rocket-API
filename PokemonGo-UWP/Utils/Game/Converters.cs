using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using GeoExtensions;
using Google.Protobuf.Collections;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils.Game;
using POGOProtos.Data;
using POGOProtos.Data.Player;
using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Fort;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;

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

    public class PokemonDataToPokemonTypeBackgroundConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return new Uri("ms-appx:///Assets/Backgrounds/details_type_bg_normal.png");
            var pokemonData = (PokemonDataWrapper) value;            
            return new Uri($"ms-appx:///Assets/Backgrounds/details_type_bg_{GameClient.PokedexExtraData.First(item => item.PokemonId == pokemonData.PokemonId).Type}.png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class PokemonWeightToWeightStringConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return new Uri("ms-appx:///Assets/Backgrounds/details_type_bg_normal.png");
            var pokemonData = (PokemonDataWrapper)value;
            return new Uri($"ms-appx:///Assets/Backgrounds/details_type_bg_{GameClient.PokedexExtraData.First(item => item.PokemonId == pokemonData.PokemonId).Type}.png");
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
            return GameClient.PlayerStats.Level + 1.5;
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
            var pokemonData = (PokemonDataWrapper) value;
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
            var itemId = ((ItemAward) value).ItemId;
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
            var noTeamColor = currentTime > 7 && currentTime < 19 ? Colors.Black : Colors.White;
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

    public class PlayerTeamToTeamNameConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var teamColor = (TeamColor) value;

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
            var achievementType = (AchievementType) value;
            var type = achievementType.GetType();
            var fieldInfo = type.GetField(achievementType.ToString());
            var badgeType =
                (BadgeTypeAttribute) fieldInfo.GetCustomAttributes(typeof(BadgeTypeAttribute), false).First();

            return badgeType == null ? "" : Resources.Achievements.GetString(badgeType.Value.ToString());
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
            var achievement = (KeyValuePair<AchievementType, object>) value;
            var type = achievement.Key.GetType();
            var fieldInfo = type.GetField(achievement.Key.ToString());
            var bronze = (BronzeAttribute) fieldInfo.GetCustomAttributes(typeof(BronzeAttribute), false).First();
            var silver = (SilverAttribute) fieldInfo.GetCustomAttributes(typeof(SilverAttribute), false).First();
            var gold = (GoldAttribute) fieldInfo.GetCustomAttributes(typeof(GoldAttribute), false).First();

            if (float.Parse(achievement.Value.ToString()) < float.Parse(bronze.Value.ToString()))
            {
                return new BitmapImage(new Uri("ms-appx:///Assets/Achievements/badge_lv0.png"));
            }
            if (float.Parse(achievement.Value.ToString()) < float.Parse(silver.Value.ToString()))
            {
                return new BitmapImage(new Uri("ms-appx:///Assets/Achievements/badge_lv1.png"));
            }
            if (float.Parse(achievement.Value.ToString()) < float.Parse(gold.Value.ToString()))
            {
                return new BitmapImage(new Uri("ms-appx:///Assets/Achievements/badge_lv2.png"));
            }
            return new BitmapImage(new Uri("ms-appx:///Assets/Achievements/badge_lv3.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        #endregion
    }

    public class AchievementNextValueConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var achievement = (KeyValuePair<AchievementType, object>) value;
            var type = achievement.Key.GetType();
            var fieldInfo = type.GetField(achievement.Key.ToString());
            var bronze = (BronzeAttribute) fieldInfo.GetCustomAttributes(typeof(BronzeAttribute), false).First();
            var silver = (SilverAttribute) fieldInfo.GetCustomAttributes(typeof(SilverAttribute), false).First();
            var gold = (GoldAttribute) fieldInfo.GetCustomAttributes(typeof(GoldAttribute), false).First();

            if (achievement.Value is float)
            {
                if (float.Parse(achievement.Value.ToString()) < float.Parse(bronze.Value.ToString()))
                {
                    return float.Parse(bronze.Value.ToString()).ToString("N1");
                }
                if (float.Parse(achievement.Value.ToString()) < float.Parse(silver.Value.ToString()))
                {
                    return float.Parse(silver.Value.ToString()).ToString("N1");
                }
                return float.Parse(gold.Value.ToString()).ToString("N1");
            }
            if (int.Parse(achievement.Value.ToString()) < int.Parse(bronze.Value.ToString()))
            {
                return bronze.Value;
            }
            if (int.Parse(achievement.Value.ToString()) < int.Parse(silver.Value.ToString()))
            {
                return silver.Value;
            }
            return gold.Value;
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
                return float.Parse(value.ToString()).ToString("N1");
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
            var achievement = (KeyValuePair<AchievementType, object>) value;
            var currentValueConverted = new AchievementValueConverter().Convert(achievement.Value, null, null, "");
            var nextValueConverted = new AchievementNextValueConverter().Convert(achievement, null, null, "");
            double currentValue, nextValue;
            currentValue = currentValueConverted is string
                ? double.Parse((string) currentValueConverted)
                : currentValueConverted as int? ?? (byte) currentValueConverted;

            nextValue = nextValueConverted is string
                ? double.Parse((string) nextValueConverted)
                : nextValueConverted as int? ?? (byte) nextValueConverted;
            return currentValue/nextValue*100;
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
            var teamColor = (TeamColor) value;
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

    public class PlayerTeamToTeamBackgroundImageConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var teamColor = (TeamColor) value;
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
            return Resources.CodeResources.GetString((string) value);
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
            var itemId = value is ItemAward ? ((ItemAward) value).ItemId : ((ItemData) value).ItemId;
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

    public class PokemonFamilyToCandyImageConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            var pokemonType = (PokemonType) value;
            var baseColor = string.Empty;
            switch (pokemonType)
            {
                case PokemonType.Grass:
                    baseColor = "B1BF21";
                    break;
                case PokemonType.Fire:
                    baseColor = "E54C3C"; 
                    break;
                case PokemonType.Electric:
                    baseColor = "FFD742";
                    break;
                case PokemonType.Ground:
                    baseColor = "DABB51";
                    break;
                case PokemonType.Fighting:
                    baseColor = "B36A57";
                    break;
                case PokemonType.Normal:
                    baseColor = "AFB09E";
                    break;
                case PokemonType.Poison:
                    baseColor = "9A599B";
                    break;
                case PokemonType.Psychic:
                    baseColor = "DE5FA4";
                    break;
                case PokemonType.None:
                    baseColor = "83C44C";
                    break;
                case PokemonType.Ice:
                    baseColor = "87DBFD";
                    break;
                case PokemonType.Rock:
                    baseColor = "BBAD62";
                    break;
                case PokemonType.Dragon:
                    baseColor = "776CE2";
                    break;
                case PokemonType.Water:
                    baseColor = "47A1FF";
                    break;
                case PokemonType.Bug:
                    baseColor = "B1BF21";
                    break;
                case PokemonType.Dark:
                    baseColor = "81604D";
                    break;
                case PokemonType.Ghost:
                    baseColor = "6D6EC1";
                    break;
                case PokemonType.Steel:
                    baseColor = "B0AFC1";
                    break;
                case PokemonType.Flying:
                    baseColor = "6B95F7";
                    break;
                case PokemonType.Fairy:
                    baseColor = "E59DE7";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }    
            // TODO: cache those files!        
            return $"https://pokemon.agne.no/candyMaker.php?base={baseColor}&secondary=f8f8f8&zoom=1";
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
            var pokemonFamily = (PokemonFamilyId) value;
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
            var fortDataStatus = (FortDataStatus) value;
            switch (fortDataStatus)
            {
                case FortDataStatus.Opened:
                    return new Uri($"ms-appx:///Assets/Icons/pokestop_near.png");
                case FortDataStatus.Closed:
                    return new Uri("ms-appx:///Assets/Icons/pokestop_far.png");
                case FortDataStatus.Cooldown:
                    return new Uri($"ms-appx:///Assets/Icons/pokestop_near_inactive.png");
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
            var egg = (PokemonDataWrapper) value;
            return string.IsNullOrEmpty(egg.EggIncubatorId) ? "ms-appx:///Assets/Items/Egg.png" : $"ms-appx:///Assets/Items/E_Item_{(int) GameClient.GetIncubatorFromEgg(egg.WrappedData).ItemId}.png";
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
            if (value == null || !(value is IncubatedEggDataWrapper)) return 0;
            var pokemon = (IncubatedEggDataWrapper) value;
            return (int) ((pokemon.EggKmWalkedStart/pokemon.EggKmWalkedTarget)*100);
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

    public class VisibleWhenStringEmptyConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return string.IsNullOrEmpty((string) value) ? Visibility.Visible : Visibility.Collapsed;
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
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return string.Format(parameter as string, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
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
            var playerStats = (PlayerStats) value;
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
            var playerStats = (PlayerStats) value;
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
            var playerData = (PlayerData) value;
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
            var ms = (long) value;
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
}