using System;
using System.Collections.Generic;
using UnityEngine;

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

/// <summary>The type of unit.</summary>
public enum UnitType
{
    NONE = -1,

    // Hero units.

    /// <summary>
    /// An all-around ranged character that has moderate stats in all
    /// categories and can generally fufill any role.
    /// </summary>
    MAINCHARACTER,

    /// <summary>
    /// A slow beefy brawler with a linear charge attack. Although his
    /// attack places him in close-combat with his enemies, he can move
    /// long distances. A plethora of hidden movement options are available
    /// depending on context.</summary>
    CHARGER,

    /// <summary>
    /// A long range character that cannot fight well in close quarters.
    /// The Magician restricts enemy movement but does low damage and cannot
    /// take damage well himself.
    /// </summary>
    MAGICIAN,

    /// <summary>
    /// An all-around ranged character that has moderate stats in all categories
    /// and can generally fufill any role.
    /// </summary>
    ELEMENTALIST,

    /// <summary>
    /// A ranged character that never attacks directly. Instead she uses delayed,
    /// high damage, area-of-effect attacks that are placed around the map. Average
    /// movement and life.
    /// </summary>
    BOMBER,

    // Enemy units

    /// <summary>
    /// The gumball enemy class.
    /// </summary>
    GUMBALL,

    /// <summary>
    /// The eagle enemy class.
    /// </summary>
    EAGLE,

    /// <summary>
    /// The running man enemy class.
    /// </summary>
    RUNNINGMAN,

    /// <summary>
    /// The red archer enemy class.
    /// </summary>
    REDARCHER,

    /// <summary>
    /// The black archer enemy class.
    /// </summary>
    BLACKARCHER,

    /// <summary>
    /// The backpack archer enemy class.
    /// </summary>
    BACKPACK,

    /// <summary>
    /// The shield enemy class.
    /// </summary>
    SHIELD,

    /// <summary>
    /// The ditto enemy class.
    /// </summary>
    DITTO,

    /// <summary>
    /// The lightbulb enemy class.
    /// </summary>
    LIGHTBULB,

    /// <summary>
    /// The hedgehog enemy class.
    /// </summary>
    HEDGEHOG,

    // TODO: No stat block

    /// <summary>
    /// The gust enemy class. No stat block provided.
    /// </summary>
    GUST,

    /// <summary>
    /// The hunter enemy class. No stat block provided.
    /// </summary>
    HUNTER,

    /// <summary>
    /// The bear enemy class. No stat block provided.
    /// </summary>
    BEAR,

    /// <summary>
    /// The ninja enemy class. No stat block provided.
    /// </summary>
    NINJA,
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

/// <summary>The factory for creating Unit classes.</summary>
public class UnitFactory
{
    /// <summary>
    /// The static class for creating specific cookie cutter Units.
    /// </summary>
    public static Unit Create(UnitType type)
    {
        switch (type)
        {
            // Hero units.
            case UnitType.MAINCHARACTER:
                return new MainCharacter();
            case UnitType.CHARGER:
                return new Charger();
            case UnitType.MAGICIAN:
                return new Magician();
            case UnitType.ELEMENTALIST:
                return new Elementalist();
            case UnitType.BOMBER:
                return new Bomber();

            // Enemy units.
            case UnitType.GUMBALL:
                return new Gumball();
            case UnitType.EAGLE:
                return new Eagle();
            case UnitType.RUNNINGMAN:
                return new RunningMan();
            case UnitType.REDARCHER:
                return new RedArcher();
            case UnitType.BLACKARCHER:
                return new BlackArcher();
            case UnitType.BACKPACK:
                return new Backpack();
            case UnitType.SHIELD:
                return new Shield();
            case UnitType.DITTO:
                return new Ditto();
            case UnitType.LIGHTBULB:
                return new Lightbulb();
            case UnitType.HEDGEHOG:
                return new Hedgehog();

            // Not implemented or NONE will trigger this.
            default:
                throw new ArgumentException("Invalid unit type", "type");
        }
    }
}

/// <summary>
/// A in-memory representation of a unit. All of this information refers to
/// perminent information that defines this unit. While stats can change in
/// missions, the changes are persistent between missions.
/// </summary>
public interface Unit
{
    /// <summary>The name of the unit.</summary>
    string name { get; }

    // Base statistics
    /// <summary> The base max health pool of the unit.</summary>
    int baseMaxHealth { get; }

    /// <summary>The base damage of the unit when attacking.</summary>
    int baseDamage { get; }

    /// <summary>The base minimum attack range of the unit when attacking.</summary>
    int baseMinRange { get; }

    /// <summary>The base maximum attack range of the unit when attacking.</summary>
    int baseMaxRange { get; }

    /// <summary>The base movement speed of the unit when moving.</summary>
    int baseMovement { get; }

    /// <summary>The base armor of the unit when dealt damage.</summary>
    int baseArmor { get; }

    /// <summary>Whether the unit flies.</summary>
    bool flying { get; }

    /// <summary>The list of abilities the unit has available.</summary>
    List<AbiliyType> abilities { get; }

    /// <summary>The list of passives the unit has available.</summary>
    List<PassiveType> passives { get; }

    /// <summary>The upgrade level of the units health.</summary>
    int healthUpgradeLevel { get; set; }

    /// <summary>The upgrade level of the units damage dealing.</summary>
    int damageUpgradeLevel { get; set; }

    /// <summary>The upgrade level of the units attack range.</summary>
    int rangeUpgradeLevel { get; set; }

    /// <summary>The upgrade level of the units movement speed.</summary>
    int movementUpgradeLevel { get; set; }

    /// <summary>The base type of unit.</summary>
    UnitType type { get; }
}

/// <summary>
/// The main character class implementation. Created using the UnitFactory.
/// </summary>
public class MainCharacter : Unit
{
    // Unit overrides.
    public string name { get { return "Main Character"; } }

    public int baseMaxHealth
    {
        get
        {
            int[] life = { 5, 10, 15, 20, 25 };
            return life[_healthUpgradeLevel];
        }
    }

    public int baseDamage
    {
        get
        {
            int[] damage = { 2, 3, 4, 5, 5 };
            return damage[_damageUpgradeLevel];
        }
    }

    public int baseMinRange
    {
        get { return 1; }
    }

    public int baseMaxRange
    {
        get
        {
            int[] range = { 2, 3, 4, 4, 4 };
            return range[_rangeUpgradeLevel];
        }
    }

    public int baseMovement
    {
        get
        {
            int[] movement = { 2, 3, 4, 5, 5 };
            return movement[_movementUpgradeLevel];
        }
    }

    public int baseArmor
    {
        get { return 0; }
    }

    public bool flying
    {
        get { return false; }
    }

    public List<AbiliyType> abilities
    {
        get
        {
            List<AbiliyType> _abilities = new List<AbiliyType>();
            _abilities.Add(AbiliyType.ATTACK);
            _abilities.Add(AbiliyType.GRAPPLE);
            return _abilities;
        }
    }

    public List<PassiveType> passives
    {
        get
        {
            List<PassiveType> _passives = new List<PassiveType>();
            _passives.Add(PassiveType.PULL);
            return _passives;
        }
    }

    private int _healthUpgradeLevel = 0;
    public int healthUpgradeLevel
    {
        get { return _healthUpgradeLevel; }
        set { _healthUpgradeLevel = (value < 5) ? value : 4; }
    }

    private int _damageUpgradeLevel = 0;
    public int damageUpgradeLevel
    {
        get { return _damageUpgradeLevel; }
        set { _damageUpgradeLevel = (value < 5) ? value : 4; }
    }

    private int _rangeUpgradeLevel = 0;
    public int rangeUpgradeLevel
    {
        get { return _rangeUpgradeLevel; }
        set { _rangeUpgradeLevel = (value < 5) ? value : 4; }
    }

    private int _movementUpgradeLevel = 0;
    public int movementUpgradeLevel
    {
        get { return _movementUpgradeLevel; }
        set { _movementUpgradeLevel = (value < 5) ? value : 4; }
    }

    public UnitType type { get { return UnitType.MAINCHARACTER; } }
}

/// <summary>
/// The charger class implementation. Created using the UnitFactory.
/// </summary>
public class Charger : Unit
{
    // Unit overrides.
    public string name { get { return "Charger"; } }

    public int baseMaxHealth
    {
        get
        {
            int[] life = { 6, 12, 18, 24, 30 };
            return life[_healthUpgradeLevel];
        }
    }

    public int baseDamage
    {
        get
        {
            int[] damage = { 2, 3, 4, 5, 6 };
            return damage[_damageUpgradeLevel];
        }
    }

    public int baseMinRange
    {
        get
        {
            return 1;
        }
    }

    public int baseMaxRange
    {
        get
        {
            int[] range = { 3, 4, 5, 5, 5 };
            return range[_rangeUpgradeLevel];
        }
    }

    public int baseMovement
    {
        get
        {
            int[] movement = { 1, 2, 3, 3, 3 };
            return movement[_movementUpgradeLevel];
        }
    }

    public int baseArmor
    {
        get { return 0; }
    }

    public bool flying
    {
        get { return false; }
    }

    public List<AbiliyType> abilities
    {
        get
        {
            List<AbiliyType> _abilities = new List<AbiliyType>();
            _abilities.Add(AbiliyType.CHARGE);
            return _abilities;
        }
    }

    public List<PassiveType> passives
    {
        get
        {
            return new List<PassiveType>();
        }
    }

    private int _healthUpgradeLevel = 0;
    public int healthUpgradeLevel
    {
        get { return _healthUpgradeLevel; }
        set { _healthUpgradeLevel = (value < 5) ? value : 4; }
    }

    private int _damageUpgradeLevel = 0;
    public int damageUpgradeLevel
    {
        get { return _damageUpgradeLevel; }
        set { _damageUpgradeLevel = (value < 5) ? value : 4; }
    }

    private int _rangeUpgradeLevel = 0;
    public int rangeUpgradeLevel
    {
        get { return _rangeUpgradeLevel; }
        set { _rangeUpgradeLevel = (value < 5) ? value : 4; }
    }

    private int _movementUpgradeLevel = 0;
    public int movementUpgradeLevel
    {
        get { return _movementUpgradeLevel; }
        set { _movementUpgradeLevel = (value < 5) ? value : 4; }
    }

    public UnitType type { get { return UnitType.CHARGER; } }
}

/// <summary>
/// The magician class implementation. Created using the UnitFactory.
/// </summary>
public class Magician : Unit
{
    // Unit overrides.
    public string name { get { return "Magician"; } }

    public int baseMaxHealth
    {
        get
        {
            int[] life = { 3, 6, 9, 12, 15 };
            return life[_healthUpgradeLevel];
        }
    }

    public int baseDamage
    {
        get
        {
            int[] damage = { 1, 2, 3, 4, 4 };
            return damage[_damageUpgradeLevel];
        }
    }

    public int baseMinRange
    {
        get
        {
            return 2;
        }
    }

    public int baseMaxRange
    {
        get
        {
            int[] range = { 3, 4, 5, 5, 5 };
            return range[_rangeUpgradeLevel];
        }
    }

    public int baseMovement
    {
        get
        {
            int[] movement = { 2, 3, 4, 4, 4 };
            return movement[_movementUpgradeLevel];
        }
    }

    public int baseArmor
    {
        get { return 0; }
    }

    public bool flying
    {
        get { return false; }
    }

    public List<AbiliyType> abilities
    {
        get
        {
            List<AbiliyType> _abilities = new List<AbiliyType>();
            _abilities.Add(AbiliyType.ATTACK);
            return _abilities;
        }
    }

    public List<PassiveType> passives
    {
        get
        {
            return new List<PassiveType>();
        }
    }

    private int _healthUpgradeLevel = 0;
    public int healthUpgradeLevel
    {
        get { return _healthUpgradeLevel; }
        set { _healthUpgradeLevel = (value < 5) ? value : 4; }
    }

    private int _damageUpgradeLevel = 0;
    public int damageUpgradeLevel
    {
        get { return _damageUpgradeLevel; }
        set { _damageUpgradeLevel = (value < 5) ? value : 4; }
    }

    private int _rangeUpgradeLevel = 0;
    public int rangeUpgradeLevel
    {
        get { return _rangeUpgradeLevel; }
        set { _rangeUpgradeLevel = (value < 5) ? value : 4; }
    }

    private int _movementUpgradeLevel = 0;
    public int movementUpgradeLevel
    {
        get { return _movementUpgradeLevel; }
        set { _movementUpgradeLevel = (value < 5) ? value : 4; }
    }

    public UnitType type { get { return UnitType.MAGICIAN; } }
}

/// <summary>
/// The elementalist class implementation. Created using the UnitFactory.
/// </summary>
public class Elementalist : Unit
{
    // Unit overrides.
    public string name { get { return "Elementalist"; } }

    public int baseMaxHealth
    {
        get
        {
            int[] life = { 4, 8, 12, 16, 20 };
            return life[_healthUpgradeLevel];
        }
    }

    public int baseDamage
    {
        get
        {
            int[] damage = { 1, 2, 3, 4, 4 };
            return damage[_damageUpgradeLevel];
        }
    }

    public int baseMinRange
    {
        get
        {
            int[] range = { 3, 4, 5, 5, 5 };
            return range[_rangeUpgradeLevel];
        }
    }

    public int baseMaxRange
    {
        get
        {
            int[] range = { 3, 4, 5, 5, 5 };
            return range[_rangeUpgradeLevel];
        }
    }

    public int baseMovement
    {
        get
        {
            int[] movement = { 1, 2, 3, 3, 3 };
            return movement[_movementUpgradeLevel];
        }
    }

    public int baseArmor
    {
        get { return 0; }
    }

    public bool flying
    {
        get { return false; }
    }

    public List<AbiliyType> abilities
    {
        get
        {
            List<AbiliyType> _abilities = new List<AbiliyType>();
            _abilities.Add(AbiliyType.ATTACK);
            return _abilities;
        }
    }

    public List<PassiveType> passives
    {
        get
        {
            return new List<PassiveType>();
        }
    }

    private int _healthUpgradeLevel = 0;
    public int healthUpgradeLevel
    {
        get { return _healthUpgradeLevel; }
        set { _healthUpgradeLevel = (value < 5) ? value : 4; }
    }

    private int _damageUpgradeLevel = 0;
    public int damageUpgradeLevel
    {
        get { return _damageUpgradeLevel; }
        set { _damageUpgradeLevel = (value < 5) ? value : 4; }
    }

    private int _rangeUpgradeLevel = 0;
    public int rangeUpgradeLevel
    {
        get { return _rangeUpgradeLevel; }
        set { _rangeUpgradeLevel = (value < 5) ? value : 4; }
    }

    private int _movementUpgradeLevel = 0;
    public int movementUpgradeLevel
    {
        get { return _movementUpgradeLevel; }
        set { _movementUpgradeLevel = (value < 5) ? value : 4; }
    }

    public UnitType type { get { return UnitType.ELEMENTALIST; } }
}

/// <summary>
/// The bomber class implementation. Created using the UnitFactory.
/// </summary>
public class Bomber : Unit
{
    // Unit overrides.
    public string name { get { return "Bomber"; } }

    public int baseMaxHealth
    {
        get
        {
            int[] life = { 4, 8, 12, 16, 20 };
            return life[_healthUpgradeLevel];
        }
    }

    public int baseDamage
    {
        get
        {
            int[] damage = { 3, 4, 5, 6, 6 };
            return damage[_damageUpgradeLevel];
        }
    }

    public int baseMinRange
    {
        get
        {
            return 0;
        }
    }

    public int baseMaxRange
    {
        get
        {
            int[] range = { 2, 3, 3, 3, 3 };
            return range[_rangeUpgradeLevel];
        }
    }

    public int baseMovement
    {
        get
        {
            int[] movement = { 2, 3, 4, 4, 4 };
            return movement[_movementUpgradeLevel];
        }
    }

    public int baseArmor
    {
        get { return 0; }
    }

    public bool flying
    {
        get { return false; }
    }

    public List<AbiliyType> abilities
    {
        get
        {
            List<AbiliyType> _abilities = new List<AbiliyType>();
            _abilities.Add(AbiliyType.ATTACK);
            return _abilities;
        }
    }

    public List<PassiveType> passives
    {
        get
        {
            return new List<PassiveType>();
        }
    }

    private int _healthUpgradeLevel = 0;
    public int healthUpgradeLevel
    {
        get { return _healthUpgradeLevel; }
        set { _healthUpgradeLevel = (value < 5) ? value : 4; }
    }

    private int _damageUpgradeLevel = 0;
    public int damageUpgradeLevel
    {
        get { return _damageUpgradeLevel; }
        set { _damageUpgradeLevel = (value < 5) ? value : 4; }
    }

    private int _rangeUpgradeLevel = 0;
    public int rangeUpgradeLevel
    {
        get { return _rangeUpgradeLevel; }
        set { _rangeUpgradeLevel = (value < 5) ? value : 4; }
    }

    private int _movementUpgradeLevel = 0;
    public int movementUpgradeLevel
    {
        get { return _movementUpgradeLevel; }
        set { _movementUpgradeLevel = (value < 5) ? value : 4; }
    }

    public UnitType type { get { return UnitType.BOMBER; } }
}

/// <summary>
/// The gumball class implementation. Created using the UnitFactory.
/// </summary>
public class Gumball : Unit
{
    // Unit overrides.
    public string name { get { return "Gumball"; } }

    public int baseMaxHealth { get { return 4; } }
    public int baseDamage { get { return 1; } }
    public int baseMinRange { get { return 1; } }
    public int baseMaxRange { get { return 1; } }
    public int baseMovement { get { return 2; } }
    public int baseArmor { get { return 0; } }
    public bool flying { get { return false; } }
    public List<AbiliyType> abilities
    {
        get
        {
            List<AbiliyType> _abilities = new List<AbiliyType>();
            _abilities.Add(AbiliyType.ATTACK);
            return _abilities;
        }
    }
    public List<PassiveType> passives { get { return new List<PassiveType>(); } }

    private int _healthUpgradeLevel = 0;
    public int healthUpgradeLevel
    {
        get { return _healthUpgradeLevel; }
        set { _healthUpgradeLevel = value; }
    }

    private int _damageUpgradeLevel = 0;
    public int damageUpgradeLevel
    {
        get { return _damageUpgradeLevel; }
        set { _damageUpgradeLevel = value; }
    }

    private int _rangeUpgradeLevel = 0;
    public int rangeUpgradeLevel
    {
        get { return _rangeUpgradeLevel; }
        set { _rangeUpgradeLevel = value; }
    }

    private int _movementUpgradeLevel = 0;
    public int movementUpgradeLevel
    {
        get { return _movementUpgradeLevel; }
        set { _movementUpgradeLevel = value; }
    }

    public UnitType type { get { return UnitType.GUMBALL; } }
}

/// <summary>
/// The eagle class implementation. Created using the UnitFactory.
/// </summary>
public class Eagle : Unit
{
    // Unit overrides.
    public string name { get { return "Eagle"; } }

    public int baseMaxHealth { get { return 2; } }
    public int baseDamage { get { return 1; } }
    public int baseMinRange { get { return 1; } }
    public int baseMaxRange { get { return 1; } }
    public int baseMovement { get { return 2; } }
    public int baseArmor { get { return 0; } }
    public bool flying { get { return true; } }
    public List<AbiliyType> abilities
    {
        get
        {
            List<AbiliyType> _abilities = new List<AbiliyType>();
            _abilities.Add(AbiliyType.ATTACK);
            return _abilities;
        }
    }
    public List<PassiveType> passives { get { return new List<PassiveType>(); } }

    private int _healthUpgradeLevel = 0;
    public int healthUpgradeLevel
    {
        get { return _healthUpgradeLevel; }
        set { _healthUpgradeLevel = value; }
    }

    private int _damageUpgradeLevel = 0;
    public int damageUpgradeLevel
    {
        get { return _damageUpgradeLevel; }
        set { _damageUpgradeLevel = value; }
    }

    private int _rangeUpgradeLevel = 0;
    public int rangeUpgradeLevel
    {
        get { return _rangeUpgradeLevel; }
        set { _rangeUpgradeLevel = value; }
    }

    private int _movementUpgradeLevel = 0;
    public int movementUpgradeLevel
    {
        get { return _movementUpgradeLevel; }
        set { _movementUpgradeLevel = value; }
    }

    public UnitType type { get { return UnitType.EAGLE; } }
}

/// <summary>
/// The running man class implementation. Created using the UnitFactory.
/// </summary>
public class RunningMan : Unit
{
    // Unit overrides.
    public string name { get { return "Running Man"; } }

    public int baseMaxHealth { get { return 4; } }
    public int baseDamage { get { return 1; } }
    public int baseMinRange { get { return 1; } }
    public int baseMaxRange { get { return 1; } }
    public int baseMovement { get { return 4; } }
    public int baseArmor { get { return 0; } }
    public bool flying { get { return false; } }
    public List<AbiliyType> abilities
    {
        get
        {
            List<AbiliyType> _abilities = new List<AbiliyType>();
            _abilities.Add(AbiliyType.ATTACK);
            return _abilities;
        }
    }
    public List<PassiveType> passives { get { return new List<PassiveType>(); } }

    private int _healthUpgradeLevel = 0;
    public int healthUpgradeLevel
    {
        get { return _healthUpgradeLevel; }
        set { _healthUpgradeLevel = value; }
    }

    private int _damageUpgradeLevel = 0;
    public int damageUpgradeLevel
    {
        get { return _damageUpgradeLevel; }
        set { _damageUpgradeLevel = value; }
    }

    private int _rangeUpgradeLevel = 0;
    public int rangeUpgradeLevel
    {
        get { return _rangeUpgradeLevel; }
        set { _rangeUpgradeLevel = value; }
    }

    private int _movementUpgradeLevel = 0;
    public int movementUpgradeLevel
    {
        get { return _movementUpgradeLevel; }
        set { _movementUpgradeLevel = value; }
    }

    public UnitType type { get { return UnitType.RUNNINGMAN; } }
}

/// <summary>
/// The red archer class implementation. Created using the UnitFactory.
/// </summary>
public class RedArcher : Unit
{
    // Unit overrides.
    public string name { get { return "Red Archer"; } }

    public int baseMaxHealth { get { return 3; } }
    public int baseDamage { get { return 3; } }
    public int baseMinRange { get { return 2; } }
    public int baseMaxRange { get { return 3; } }
    public int baseMovement { get { return 1; } }
    public int baseArmor { get { return 0; } }
    public bool flying { get { return false; } }
    public List<AbiliyType> abilities
    {
        get
        {
            List<AbiliyType> _abilities = new List<AbiliyType>();
            _abilities.Add(AbiliyType.ATTACK);
            return _abilities;
        }
    }
    public List<PassiveType> passives { get { return new List<PassiveType>(); } }

    private int _healthUpgradeLevel = 0;
    public int healthUpgradeLevel
    {
        get { return _healthUpgradeLevel; }
        set { _healthUpgradeLevel = value; }
    }

    private int _damageUpgradeLevel = 0;
    public int damageUpgradeLevel
    {
        get { return _damageUpgradeLevel; }
        set { _damageUpgradeLevel = value; }
    }

    private int _rangeUpgradeLevel = 0;
    public int rangeUpgradeLevel
    {
        get { return _rangeUpgradeLevel; }
        set { _rangeUpgradeLevel = value; }
    }

    private int _movementUpgradeLevel = 0;
    public int movementUpgradeLevel
    {
        get { return _movementUpgradeLevel; }
        set { _movementUpgradeLevel = value; }
    }

    public UnitType type { get { return UnitType.REDARCHER; } }
}

/// <summary>
/// The black archer class implementation. Created using the UnitFactory.
/// </summary>
public class BlackArcher : Unit
{
    // Unit overrides.
    public string name { get { return "Black Archer"; } }

    public int baseMaxHealth { get { return 3; } }
    public int baseDamage { get { return 2; } }
    public int baseMinRange { get { return 2; } }
    public int baseMaxRange { get { return 2; } }
    public int baseMovement { get { return 2; } }
    public int baseArmor { get { return 0; } }
    public bool flying { get { return false; } }
    public List<AbiliyType> abilities
    {
        get
        {
            List<AbiliyType> _abilities = new List<AbiliyType>();
            _abilities.Add(AbiliyType.ATTACK);
            return _abilities;
        }
    }
    public List<PassiveType> passives { get { return new List<PassiveType>(); } }

    private int _healthUpgradeLevel = 0;
    public int healthUpgradeLevel
    {
        get { return _healthUpgradeLevel; }
        set { _healthUpgradeLevel = value; }
    }

    private int _damageUpgradeLevel = 0;
    public int damageUpgradeLevel
    {
        get { return _damageUpgradeLevel; }
        set { _damageUpgradeLevel = value; }
    }

    private int _rangeUpgradeLevel = 0;
    public int rangeUpgradeLevel
    {
        get { return _rangeUpgradeLevel; }
        set { _rangeUpgradeLevel = value; }
    }

    private int _movementUpgradeLevel = 0;
    public int movementUpgradeLevel
    {
        get { return _movementUpgradeLevel; }
        set { _movementUpgradeLevel = value; }
    }

    public UnitType type { get { return UnitType.BLACKARCHER; } }
}

/// <summary>
/// The backpack class implementation. Created using the UnitFactory.
/// </summary>
public class Backpack : Unit
{
    // Unit overrides.
    public string name { get { return "Backpack"; } }

    public int baseMaxHealth { get { return 4; } }
    public int baseDamage { get { return 1; } }
    public int baseMinRange { get { return 1; } }
    public int baseMaxRange { get { return 1; } }
    public int baseMovement { get { return 2; } }
    public int baseArmor { get { return 0; } }
    public bool flying { get { return false; } }
    public List<AbiliyType> abilities
    {
        get
        {
            List<AbiliyType> _abilities = new List<AbiliyType>();
            _abilities.Add(AbiliyType.ATTACK);
            return _abilities;
        }
    }
    public List<PassiveType> passives { get { return new List<PassiveType>(); } }

    private int _healthUpgradeLevel = 0;
    public int healthUpgradeLevel
    {
        get { return _healthUpgradeLevel; }
        set { _healthUpgradeLevel = value; }
    }

    private int _damageUpgradeLevel = 0;
    public int damageUpgradeLevel
    {
        get { return _damageUpgradeLevel; }
        set { _damageUpgradeLevel = value; }
    }

    private int _rangeUpgradeLevel = 0;
    public int rangeUpgradeLevel
    {
        get { return _rangeUpgradeLevel; }
        set { _rangeUpgradeLevel = value; }
    }

    private int _movementUpgradeLevel = 0;
    public int movementUpgradeLevel
    {
        get { return _movementUpgradeLevel; }
        set { _movementUpgradeLevel = value; }
    }

    public UnitType type { get { return UnitType.BACKPACK; } }
}

/// <summary>
/// The shield class implementation. Created using the UnitFactory.
/// </summary>
public class Shield : Unit
{
    // Unit overrides.
    public string name { get { return "Shield"; } }

    public int baseMaxHealth { get { return 4; } }
    public int baseDamage { get { return 1; } }
    public int baseMinRange { get { return 1; } }
    public int baseMaxRange { get { return 1; } }
    public int baseMovement { get { return 2; } }
    public int baseArmor { get { return 1; } }
    public bool flying { get { return false; } }
    public List<AbiliyType> abilities
    {
        get
        {
            List<AbiliyType> _abilities = new List<AbiliyType>();
            _abilities.Add(AbiliyType.ATTACK);
            return _abilities;
        }
    }
    public List<PassiveType> passives { get { return new List<PassiveType>(); } }

    private int _healthUpgradeLevel = 0;
    public int healthUpgradeLevel
    {
        get { return _healthUpgradeLevel; }
        set { _healthUpgradeLevel = value; }
    }

    private int _damageUpgradeLevel = 0;
    public int damageUpgradeLevel
    {
        get { return _damageUpgradeLevel; }
        set { _damageUpgradeLevel = value; }
    }

    private int _rangeUpgradeLevel = 0;
    public int rangeUpgradeLevel
    {
        get { return _rangeUpgradeLevel; }
        set { _rangeUpgradeLevel = value; }
    }

    private int _movementUpgradeLevel = 0;
    public int movementUpgradeLevel
    {
        get { return _movementUpgradeLevel; }
        set { _movementUpgradeLevel = value; }
    }

    public UnitType type { get { return UnitType.SHIELD; } }
}

/// <summary>
/// The ditto class implementation. Created using the UnitFactory.
/// </summary>
public class Ditto : Unit
{
    // Unit overrides.
    public string name { get { return "Ditto"; } }

    public int baseMaxHealth { get { return 5; } }
    public int baseDamage { get { return 1; } }
    public int baseMinRange { get { return 2; } }
    public int baseMaxRange { get { return 2; } }
    public int baseMovement { get { return 2; } }
    public int baseArmor { get { return 0; } }
    public bool flying { get { return false; } }
    public List<AbiliyType> abilities
    {
        get
        {
            List<AbiliyType> _abilities = new List<AbiliyType>();
            _abilities.Add(AbiliyType.ATTACK);
            return _abilities;
        }
    }
    public List<PassiveType> passives { get { return new List<PassiveType>(); } }

    private int _healthUpgradeLevel = 0;
    public int healthUpgradeLevel
    {
        get { return _healthUpgradeLevel; }
        set { _healthUpgradeLevel = value; }
    }

    private int _damageUpgradeLevel = 0;
    public int damageUpgradeLevel
    {
        get { return _damageUpgradeLevel; }
        set { _damageUpgradeLevel = value; }
    }

    private int _rangeUpgradeLevel = 0;
    public int rangeUpgradeLevel
    {
        get { return _rangeUpgradeLevel; }
        set { _rangeUpgradeLevel = value; }
    }

    private int _movementUpgradeLevel = 0;
    public int movementUpgradeLevel
    {
        get { return _movementUpgradeLevel; }
        set { _movementUpgradeLevel = value; }
    }

    public UnitType type { get { return UnitType.DITTO; } }
}

/// <summary>
/// The lightbulb class implementation. Created using the UnitFactory.
/// </summary>
public class Lightbulb : Unit
{
    // Unit overrides.
    public string name { get { return "Lightbulb"; } }

    public int baseMaxHealth { get { return 5; } }
    public int baseDamage { get { return 1; } }
    public int baseMinRange { get { return 1; } }
    public int baseMaxRange { get { return 1; } }
    public int baseMovement { get { return 2; } }
    public int baseArmor { get { return 0; } }
    public bool flying { get { return false; } }
    public List<AbiliyType> abilities
    {
        get
        {
            List<AbiliyType> _abilities = new List<AbiliyType>();
            _abilities.Add(AbiliyType.ATTACK);
            return _abilities;
        }
    }
    public List<PassiveType> passives { get { return new List<PassiveType>(); } }

    private int _healthUpgradeLevel = 0;
    public int healthUpgradeLevel
    {
        get { return _healthUpgradeLevel; }
        set { _healthUpgradeLevel = value; }
    }

    private int _damageUpgradeLevel = 0;
    public int damageUpgradeLevel
    {
        get { return _damageUpgradeLevel; }
        set { _damageUpgradeLevel = value; }
    }

    private int _rangeUpgradeLevel = 0;
    public int rangeUpgradeLevel
    {
        get { return _rangeUpgradeLevel; }
        set { _rangeUpgradeLevel = value; }
    }

    private int _movementUpgradeLevel = 0;
    public int movementUpgradeLevel
    {
        get { return _movementUpgradeLevel; }
        set { _movementUpgradeLevel = value; }
    }

    public UnitType type { get { return UnitType.LIGHTBULB; } }
}

/// <summary>
/// The hedgehog class implementation. Created using the UnitFactory.
/// </summary>
public class Hedgehog : Unit
{
    // Unit overrides.
    public string name { get { return "Hedgehog"; } }

    public int baseMaxHealth { get { return 5; } }
    public int baseDamage { get { return 1; } }
    public int baseMinRange { get { return 1; } }
    public int baseMaxRange { get { return 1; } }
    public int baseMovement { get { return 2; } }
    public int baseArmor { get { return 0; } }
    public bool flying { get { return false; } }
    public List<AbiliyType> abilities
    {
        get
        {
            List<AbiliyType> _abilities = new List<AbiliyType>();
            _abilities.Add(AbiliyType.ATTACK);
            return _abilities;
        }
    }
    public List<PassiveType> passives { get { return new List<PassiveType>(); } }

    private int _healthUpgradeLevel = 0;
    public int healthUpgradeLevel
    {
        get { return _healthUpgradeLevel; }
        set { _healthUpgradeLevel = value; }
    }

    private int _damageUpgradeLevel = 0;
    public int damageUpgradeLevel
    {
        get { return _damageUpgradeLevel; }
        set { _damageUpgradeLevel = value; }
    }

    private int _rangeUpgradeLevel = 0;
    public int rangeUpgradeLevel
    {
        get { return _rangeUpgradeLevel; }
        set { _rangeUpgradeLevel = value; }
    }

    private int _movementUpgradeLevel = 0;
    public int movementUpgradeLevel
    {
        get { return _movementUpgradeLevel; }
        set { _movementUpgradeLevel = value; }
    }

    public UnitType type { get { return UnitType.HEDGEHOG; } }
}
