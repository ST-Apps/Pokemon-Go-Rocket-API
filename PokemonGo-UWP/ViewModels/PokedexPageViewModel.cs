using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Settings.Master;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Navigation;

namespace PokemonGo_UWP.ViewModels
{
    public class PokedexPageViewModel : ViewModelBase
    {
        #region Lifecycle Handlers
        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (state.Any())
            {
                PokemonFoundAndSeen = (ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>>)state[nameof(PokemonFoundAndSeen)];
                SeenPokemons = (int)state[nameof(SeenPokemons)];
                CapturedPokemons = (int)state[nameof(CapturedPokemons)];
                if (state.ContainsKey(nameof(IsPokemonDetailsOpen)))
                {
                    SelectedPokedexEntry = (KeyValuePair<PokemonId, PokedexEntry>)state[nameof(SelectedPokedexEntry)];
                    IsEevee = (bool)state[nameof(IsEevee)];
                    IsPokemonDetailsOpen = (bool)state[nameof(IsPokemonDetailsOpen)];
                }
            }
            else
            {
                var list = Enum.GetValues(typeof(PokemonId)).Cast<PokemonId>();
                var pokedexItems = GameClient.PokedexInventory;
                foreach (var item in list)
                {
                    switch (item)
                    {
                        case PokemonId.Missingno:
                            break;
                        default:
                            var pokedexEntry = pokedexItems.Where(x => x.PokemonId == item);
                            if(pokedexEntry.Count()==1)
                                PokemonFoundAndSeen.Add(new KeyValuePair<PokemonId, PokedexEntry>(item, pokedexEntry.ElementAt(0)));
                            else
                                PokemonFoundAndSeen.Add(new KeyValuePair<PokemonId, PokedexEntry>(item, null));
                            break;
                    }
                }
                for(int i = PokemonFoundAndSeen.Count-1; i >= 1; i--)
                {
                    var item = PokemonFoundAndSeen[i];
                    if (item.Value == null || (item.Value.TimesEncountered == 0 && item.Value.TimesCaptured == 0))
                        PokemonFoundAndSeen.RemoveAt(i);
                    else //Pokemon seen or captured
                        break;
                }
                CapturedPokemons = pokedexItems.Where(x => x.TimesCaptured > 0).Count();
                SeenPokemons = pokedexItems.Count;
            }
            if(parameter!=null && parameter is PokemonId)
            {
                SelectedPokedexEntry = new KeyValuePair<PokemonId, PokedexEntry>((PokemonId)parameter, GetPokedexEntry((PokemonId)parameter));
                IsPokemonDetailsOpen = true;
            }
            return Task.CompletedTask;
        }
        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            if (suspending)
            {
                pageState[nameof(PokemonFoundAndSeen)] = PokemonFoundAndSeen;
                pageState[nameof(SeenPokemons)] = SeenPokemons;
                pageState[nameof(CapturedPokemons)] = CapturedPokemons;
                if (IsPokemonDetailsOpen)
                {
                    pageState[nameof(IsPokemonDetailsOpen)] = IsPokemonDetailsOpen;
                    pageState[nameof(SelectedPokedexEntry)] = SelectedPokedexEntry;
                    pageState[nameof(IsEevee)] = IsEevee;
                }
            }
            else
            {
                pageState?.Clear();
                PokemonFoundAndSeen?.Clear();
                EeveeEvolutions?.Clear();
                PokemonEvolutions?.Clear();
            }
            return Task.CompletedTask;
        }
        #endregion

        #region Variables
        public ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>> PokemonFoundAndSeen { get; private set; } = 
            new ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>>();
        private int _captured, _seen;
        public int CapturedPokemons { get { return _captured; } set { Set(ref _captured, value); } }
        public int SeenPokemons { get { return _seen; } set { Set(ref _seen, value); } }
        
        private bool _pokemonDetailsOpen;
        public bool IsPokemonDetailsOpen { get { return _pokemonDetailsOpen; } set { Set(ref _pokemonDetailsOpen, value); } }
        private KeyValuePair<PokemonId, PokedexEntry> _selectedPokedex;
        public KeyValuePair<PokemonId, PokedexEntry> SelectedPokedexEntry
        {
            get { return _selectedPokedex; }
            set
            {
                if (IsPokemonDetailsOpen && value.Key == _selectedPokedex.Key)
                    return;
                Set(ref _selectedPokedex, value);
                PokemonDetails = GameClient.GetExtraDataForPokemon(value.Key);
                PopulateEvolutions();
            }
        }
        private PokemonSettings _pokemonDetails;
        public PokemonSettings PokemonDetails { get { return _pokemonDetails; } set { Set(ref _pokemonDetails, value); } }
        public ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>> PokemonEvolutions { get; } =
            new ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>>();
        public ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>> EeveeEvolutions { get; } =
            new ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>>();
        private bool _isEevee;
        public bool IsEevee { get { return _isEevee; } set { Set(ref _isEevee, value); } }
        #endregion

        private DelegateCommand<KeyValuePair<PokemonId, PokedexEntry>> _openPokedexEntry;
        public DelegateCommand<KeyValuePair<PokemonId, PokedexEntry>> OpenPokedexEntry =>
            _openPokedexEntry ??
            (_openPokedexEntry = new DelegateCommand<KeyValuePair<PokemonId, PokedexEntry>>(
                (x) => 
                    {
                        SelectedPokedexEntry = x;
                        RaisePropertyChanged(nameof(SelectedPokedexEntry));
                        IsPokemonDetailsOpen = true;
                    }, 
                    (x) => { return true; }
                )
            );
        private DelegateCommand _closeCommand;
        public DelegateCommand CloseCommand
            =>
            _closeCommand ??
            (_closeCommand = new DelegateCommand(() =>
            {
                if (IsPokemonDetailsOpen)
                    IsPokemonDetailsOpen = false;
                else
                    NavigationService.Navigate(typeof(GameMapPage));
            }, () => true)
            );
        private void PopulateEvolutions()
        {
            PokemonEvolutions.Clear();
            EeveeEvolutions.Clear();
            IsEevee = false;
            PokemonId InitPokemon = SelectedPokedexEntry.Key;
            PokemonSettings CurrPokemon = PokemonDetails;
            switch (InitPokemon)
            {
                case PokemonId.Eevee:
                case PokemonId.Jolteon:
                case PokemonId.Flareon:
                case PokemonId.Vaporeon:
                    InitPokemon = PokemonId.Eevee;
                    CurrPokemon = GameClient.GetExtraDataForPokemon(InitPokemon);
                    foreach (var ev in CurrPokemon.EvolutionIds)
                        EeveeEvolutions.Add(new KeyValuePair<PokemonId, PokedexEntry>(ev, GetPokedexEntry(ev)));
                    PokemonEvolutions.Add(new KeyValuePair<PokemonId, PokedexEntry>(PokemonId.Eevee, GetPokedexEntry(PokemonId.Eevee)));
                    IsEevee = true;
                    break;
                default:
                    PokemonEvolutions.Add(new KeyValuePair<PokemonId, PokedexEntry>(InitPokemon, GetPokedexEntry(InitPokemon)));
                    while (CurrPokemon.ParentPokemonId != PokemonId.Missingno)
                    {
                        PokemonEvolutions.Insert(0, new KeyValuePair<PokemonId, PokedexEntry>(CurrPokemon.ParentPokemonId, GetPokedexEntry(CurrPokemon.ParentPokemonId)));
                        CurrPokemon = GameClient.GetExtraDataForPokemon(CurrPokemon.ParentPokemonId);
                    }
                    CurrPokemon = PokemonDetails;
                    while (CurrPokemon.EvolutionIds.Count > 0)
                    {
                        foreach (var ev in CurrPokemon.EvolutionIds) //for Eevee
                            PokemonEvolutions.Add(new KeyValuePair<PokemonId, PokedexEntry>(ev, GetPokedexEntry(ev)));
                        CurrPokemon = GameClient.GetExtraDataForPokemon(CurrPokemon.EvolutionIds.ElementAt(0));
                    }
                    break;
            }
        }
        private PokedexEntry GetPokedexEntry(PokemonId pokemon)
        {
            var found = PokemonFoundAndSeen.Where(x => x.Key == pokemon);
            if (found != null && found.Count() > 0)
                return found.ElementAt(0).Value;
            return null;
        }
    }
}
