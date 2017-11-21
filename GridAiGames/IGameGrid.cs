namespace GridAiGames
{
    public interface IGameGrid<PlayerType, PlayerActionType> : IReadOnlyGameGrid
        where PlayerType : IPlayer
    {
        void AddObject(GameObject<PlayerType, PlayerActionType> obj);
        void RemoveObject(GameObject<PlayerType, PlayerActionType> obj);
    }
}