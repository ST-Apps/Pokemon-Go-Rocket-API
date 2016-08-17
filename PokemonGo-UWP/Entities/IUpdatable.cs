namespace PokemonGo_UWP.Entities
{
    public interface IUpdatable<in T>
    {
        void Update(T update);
    }
}