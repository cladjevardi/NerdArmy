using System.Collections.Generic;

/// <summary>
/// Represents ownership of a given unit. Players can be assigned as AI or
/// human controlled.
/// </summary>
public enum Owner
{
    NONE = -1,
    PLAYER1,
    PLAYER2,
    PLAYER3,
    PLAYER4
};

/// <summary>Player type information.</summary>
public enum Player
{
    NONE = -1,
    HUMAN,
    COMPUTER,
}

/// <summary>The type of usable abilities available for a unit.</summary>
public enum AbiliyType
{
    NONE = -1,

    /// <summary>A regular attack using the baseDamage value.</summary>
    ATTACK,

    /// <summary>
    /// The unit can now aim her attack at impassable terrain
    /// or the map's edges to pull herself towards it, stopping one square
    /// in front of her. Gained at a undisclosed level at this time.
    /// </summary>
    GRAPPLE,

    /// <summary>
    /// Charges into the first enemy in his way dealing damage to it,
    /// knocking it back 2 spaces, and landing on the square just shy
    /// of the enemy's initial position. If enemies are within knockback
    /// range, they take damage as well.
    /// 
    /// In case of a friendly unit occupying the usual landing square,
    /// the Charger will go to the next closest square to his original
    /// position.
    /// 
    /// Similarly, the map's edges or impassable terrain can also trigger
    /// "next closest square" logic.
    /// </summary>
    CHARGE,

    // TODO: Add more abilities.
}

/// <summary>The type of passive ability the unit has.</summary>
public enum PassiveType
{
    NONE,

    /// <summary>
    /// Enemies attacked will now be pulled towards the the unit
    /// stopping one square in front of her.
    /// </summary>
    PULL,

    // TODO: Add more passives.
}

/// <summary>
/// A in-memory representation of a unit. All of this information refers to
/// perminent information that defines this unit. While stats can change in
/// missions, the changes are persistent between missions.
/// </summary>
public class Unit
{
    public Unit(UnitType unitType, int health = 0, int damage = 0,
        int range = 0, int movement = 0)
    {
        // Lookup the unit type from the database and fill in
        // the units information appropriately.
        UnitSchematic schematic
            = GameManager.instance.unitDatabase.GetUnit(unitType);

        // Update the internals.
        _name = schematic.name;
        _baseMaxHealth = schematic.baseMaxHealth;
        _baseDamage = schematic.baseDamage;
        _baseMinRange = schematic.baseMinRange;
        _baseMaxRange = schematic.baseMaxRange;
        _baseMovement = schematic.baseMovement;
        _baseArmor = schematic.baseArmor;
        _flying = schematic.flying;
        _abilities = schematic.abilities;
        _passives = schematic.passives;
        _type = unitType;

        // Update the upgrades.
        _healthUpgradeLevel = health;
        _damageUpgradeLevel = damage;
        _rangeUpgradeLevel = range;
        _movementUpgradeLevel = movement;

        // Create the animations.
        foreach (UnitAnimation unitAnimation in schematic.material.animations)
        {
            Mesh2DAnimation northAnimation = new Mesh2DAnimation();
            northAnimation.name = unitAnimation.name + "_north";
            northAnimation.frameRate = unitAnimation.speed;
            northAnimation.frameSequence = unitAnimation.northSequence;
            northAnimation.shouldLoop = unitAnimation.loop;
            _animations.Add(northAnimation);

            Mesh2DAnimation eastAnimation = new Mesh2DAnimation();
            eastAnimation.name = unitAnimation.name + "_east";
            eastAnimation.frameRate = unitAnimation.speed;
            eastAnimation.frameSequence = unitAnimation.eastSequence;
            eastAnimation.shouldLoop = unitAnimation.loop;
            _animations.Add(eastAnimation);

            Mesh2DAnimation westAnimation = new Mesh2DAnimation();
            westAnimation.name = unitAnimation.name + "_south";
            westAnimation.frameRate = unitAnimation.speed;
            westAnimation.frameSequence = unitAnimation.southSequence;
            westAnimation.shouldLoop = unitAnimation.loop;
            _animations.Add(westAnimation);
        }
    }

    /// <summary>The name of the unit.</summary>
    private string _name;
    public string name
    {
        get { return _name; }
    }

    /// <summary> The base max health pool of the unit.</summary>
    private List<int> _baseMaxHealth;
    public int baseMaxHealth
    {
        get { return _baseMaxHealth[healthUpgradeLevel]; }
    }

    /// <summary>The base damage of the unit when attacking.</summary>
    private List<int> _baseDamage;
    public int baseDamage
    {
        get { return _baseDamage[damageUpgradeLevel]; }
    }

    /// <summary>The base minimum attack range of the unit when attacking.</summary>
    private List<int> _baseMinRange;
    public int baseMinRange
    {
        get { return _baseMinRange[rangeUpgradeLevel]; }
    }

    /// <summary>The base maximum attack range of the unit when attacking.</summary>
    private List<int> _baseMaxRange;
    public int baseMaxRange
    {
        get { return _baseMaxRange[rangeUpgradeLevel]; }
    }

    /// <summary>The base movement speed of the unit when moving.</summary>
    private List<int> _baseMovement;
    public int baseMovement
    {
        get { return _baseMovement[movementUpgradeLevel]; }
    }

    /// <summary>The base armor of the unit when dealt damage.</summary>
    private int _baseArmor = 0;
    public int baseArmor
    {
        get { return _baseArmor; }
    }

    /// <summary>Whether the unit flies.</summary>
    private bool _flying = false;
    public bool flying
    {
        get { return _flying; }
    }

    /// <summary>The list of abilities the unit has available.</summary>
    private List<AbiliyType> _abilities;
    public List<AbiliyType> abilities
    {
        get { return _abilities; }
    }

    /// <summary>The list of passives the unit has available.</summary>
    private List<PassiveType> _passives;
    public List<PassiveType> passives
    {
        get { return _passives; }
    }

    /// <summary>The upgrade level of the units health.</summary>
    private int _healthUpgradeLevel = 0;
    public int healthUpgradeLevel
    {
        get { return _healthUpgradeLevel; }
        set { _healthUpgradeLevel = value; }
    }

    /// <summary>The upgrade level of the units damage dealing.</summary>
    private int _damageUpgradeLevel = 0;
    public int damageUpgradeLevel
    {
        get { return _damageUpgradeLevel; }
        set { _damageUpgradeLevel = value; }
    }

    /// <summary>The upgrade level of the units attack range.</summary>
    private int _rangeUpgradeLevel = 0;
    public int rangeUpgradeLevel
    {
        get { return _rangeUpgradeLevel; }
        set { _rangeUpgradeLevel = value; }
    }

    /// <summary>The upgrade level of the units movement speed.</summary>
    private int _movementUpgradeLevel = 0;
    public int movementUpgradeLevel
    {
        get { return _movementUpgradeLevel; }
        set { _movementUpgradeLevel = value; }
    }

    /// <summary>The base type of unit.</summary>
    private UnitType _type;
    public UnitType type
    {
        get { return _type; }
    }

    /// <summary>
    /// The material id for the unit. This id is used as a lookup id for
    /// the unit material in GameManager.
    /// </summary>
    private int _materialId = 0;
    public int materialId
    {
        get { return _materialId; }
    }

    /// <summary>The animations for the unit.</summary>
    private List<Mesh2DAnimation> _animations = new List<Mesh2DAnimation>();
    public List<Mesh2DAnimation> animations
    {
        get { return _animations; }
    }
}
