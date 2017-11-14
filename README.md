# GridAiGames
Simple framework for creating time discrete 2D grid games driven by AI.

## Bomberman
Classic game remake built with the GridAiGames framework.

1. Game is evaluated in discrete steps.
2. There is configurable number of teams. Each team has configurable number of players.
3. In each step custom artificial intelligence returns action for each player of your team.

![alt text](https://github.com/kindermannhubert/GridAiGames/blob/master/GridAiGames.Bomberman/Preview.png)

### Rules
Players can move left/up/right/down and they can place bombs. Bomb placement and movement can be combined together.

Bomb has detonation radius and ramaining number of iterations to detonation. Detonation radius is inherited from player who placed the bomb.

Players can individualy upgrade detonation radius and maximum allowed number of placed bombs by picking up bonuses.

Player hit by bomb detonation fire dies.

### How to create custom AI?
1. Create new .NET class library.
2. Add references to project "GridAiGames" and "GridAiGames.Bomberman.ReadOnly".
3. Add new type which implements interface "IIntelligence<GameGrid, Player, PlayerAction>".

Example of simple intelligence can be found at project "GridAiGames.Bomberman.SimpleIntelligence".

### How to use custom AI?
Edit "GridAiGames.Bomberman\GridAiGames.Bomberman.Configuration.xml".
You can also change other parameters of game there.

### How to run game without GUI?
See project "GridAiGames.Bomberman.Tests".
