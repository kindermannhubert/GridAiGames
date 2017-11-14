namespace GridAiGames
{
    public interface IGameGrid<PlayerType, PlayerActionType> : IReadOnlyGameGrid<PlayerType, PlayerActionType>
        where PlayerType : Player<PlayerType, PlayerActionType>
    {
        void AddObject(GameObject<PlayerType, PlayerActionType> obj);
        void RemoveObject(GameObject<PlayerType, PlayerActionType> obj);
    }
}