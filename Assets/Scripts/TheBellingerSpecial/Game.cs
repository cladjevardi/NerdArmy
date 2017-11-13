using System.Collections.Generic;
using NerdArmy;

class Game
{
    /// <summary>The different unit control groups.</summary>
    private enum PlayerType { HUMAN, COMPUTER };

    /// <summary>The factions that make up players and opposition.</summary>
    // TODO: Make this mission specific.
    private PlayerType player1 = PlayerType.HUMAN;
    private PlayerType player2 = PlayerType.COMPUTER;

    /// <summary>The current player roster.</summary>
    private List<Unit> roster = new List<Unit>();

    /// <summary>The current list of actors in the mission.</summary>
    private List<Actor> actors = new List<Actor>();

    /// <summary>Boolean that tells the game if it is shutdown.</summary>
    private bool shutdown = false;

    public void Initialize()
    {
        // Create the initial roster.
        InitializeRoster();

        // TODO: Create the tilemap and setup the collision.

        // Add the units to the scene.
        AddActors();
    }

    public void Shutdown()
    {
        // TODO: Maybe save game status?
        roster.Clear();
        actors.Clear();
    }

    public void Pump()
    {
        // TODO: Draw the background.

        // Draw the scene
        foreach (Actor actor in actors)
        {
                
        }
    }

    private void InitializeRoster()
    {
        roster.Add(UnitFactory.Create(UnitType.MAINCHARACTER));
        roster.Add(UnitFactory.Create(UnitType.CHARGER));
        // TODO: Add however many initial units.
    }

    private void AddActors()
    {
        // Iterate through each 
        foreach (Unit unit in roster)
        {
            Actor actor = new Actor();
            actor.unit = unit;
            actor.owner = Owner.PLAYER1;
            actor.health = unit.baseMaxHealth;

            // Add the actor to the mission.
            actors.Add(actor);
        }

        // TODO: Based on mission add enemy actors
    }

    static void Main(string[] args)
    {
        // Create the game instance.
        Game game = new Game();
        game.Initialize();

        // Main game loop.
        while (!game.shutdown)
        {
            game.Pump();
        }
    }
}
