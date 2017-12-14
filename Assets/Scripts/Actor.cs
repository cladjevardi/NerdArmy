using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>The list of unit animations.</summary>
public enum ActorAnimation
{
    IDLE,
    ATTACKING,
    DAMAGED,
    WALKING_NORTH,
    WALKING_EAST,
    WALKING_SOUTH,
    WALKING_WEST,

    // TODO:
    DEATH,
    CELEBRATE,
}

/// <summary>
/// The unity representation of a unit in combat or a mission. This information
/// represents temporary status.
/// </summary>
public class Actor : Mesh2D
{
    /// <summary>Set the actor animation.</summary>
    /// <param name="ActorAnimation">The animation to play.</param>
    public void SetAnimation(ActorAnimation animation)
    {
        // Get the string animation name.
        string animationName = "";
        switch (animation)
        {
            case ActorAnimation.IDLE:
                animationName = "idle";
                break;
            case ActorAnimation.ATTACKING:
                animationName = "attacking";
                break;
            case ActorAnimation.DAMAGED:
                animationName = "damaged";
                break;
            case ActorAnimation.WALKING_NORTH:
                animationName = "walking_north";
                break;
            case ActorAnimation.WALKING_EAST:
                animationName = "walking_east";
                break;
            case ActorAnimation.WALKING_SOUTH:
                animationName = "walking_south";
                break;
            case ActorAnimation.WALKING_WEST:
                animationName = "walking_west";
                break;

            // Not implemente
            default:
                throw new ArgumentException(
                    "Invalid actor animation specified", "animation");
        }

        SetAnimation(GetCurrentLayer(), animationName);
    }

    /// <summary>
    /// The Unit information. Use UnitFactor.Create() a cutout UnitType.
    /// </summary>
    private Unit _unit = null;
    public Unit unit
    {
        get { return _unit; }
        set {
            _unit = value;

            // Apply the flying trait based on the unit.
            flying = _unit.flying;

            // Assign the units material id.
            SetUnitMaterial();
        }
    }

    /// <summary>
    /// The owner of unit to be controlled in a mission.
    /// </summary>
    private Owner _owner = Owner.NONE;
    public Owner owner
    {
        get { return _owner; }
        set { _owner = value; }
    }

    /// <summary>
    /// The current health of the unit in the mission. Set to maxHp at mission
    /// start.
    /// </summary>
    private int _health = 0;
    public int health
    {
        get { return _health; }
        set {
            if (value <= 0)
                RemoveLayer(GetCurrentLayer());
            _health = value;
        }
    }

    /// <summary>
    /// If the unit is currently flying.
    /// </summary>
    private bool _flying = false;
    public bool flying
    {
        get { return _flying; }
        set {
            // TODO: Move the current material to the flying layer.
            _flying = value;
        }
    }

    /// <summary>
    /// Whether the under is burrowed into the ground.
    /// </summary>
    private bool _burrowed = false;
    public bool burrowed
    {
        get { return _burrowed; }
        set {
            // TODO: Move the current material to the burrowed layer.
            _burrowed = value;
        }
    }

    /// <summary>
    /// If the unit is currently invisible.
    /// </summary>
    private bool _invisible = false;
    public bool invisible
    {
        get { return _invisible; }
        set {
            // TODO: Move the current material to the invisible layer.
            _invisible = value;
        }
    }

    /// <summary>
    /// Whether the character has already moved or performed an action.
    /// </summary>
    private bool _done = false;
    public bool done
    {
        get { return _done; }
        set { _done = value; }
    }

    /// <summary>
    /// Get the current visual layer the unit is residing on.
    /// </summary>
    /// <returns>Returns the current layer of the unit.</returns>
    private Mesh2DLayer GetCurrentLayer()
    {
        Mesh2DLayer layer = Mesh2DLayer.LAYER_UNITS;
        if (_invisible)
            layer = Mesh2DLayer.LAYER_INVISIBLE;
        else if (_burrowed)
            layer = Mesh2DLayer.LAYER_BURROWED;
        else if (_flying)
            layer = Mesh2DLayer.LAYER_FLYINGUNITS;
        return layer;
    }

    /// <summary>
    /// Get the sprite sheet material id.
    /// </summary>
    /// <param name="unitType">The unit type to lookup for the id.</param>
    /// <returns>Returns the material lookup id to use with GameManager.</returns>
    private int GetUnitMaterialId(UnitType unitType)
    {
        switch (unitType)
        {
            // Hero units.
            case UnitType.MAINCHARACTER:
                return 15; // TODO: Fix me.
            case UnitType.CHARGER:
                return 1;
            case UnitType.MAGICIAN:
                return 2;
            case UnitType.ELEMENTALIST:
                return 3;
            case UnitType.BOMBER:
                return 4;

            // Enemy units.
            case UnitType.GUMBALL:
                return 16; // TODO: Fix me.
            case UnitType.EAGLE:
                return 6;
            case UnitType.RUNNINGMAN:
                return 7;
            case UnitType.REDARCHER:
                return 8;
            case UnitType.BLACKARCHER:
                return 9;
            case UnitType.BACKPACK:
                return 10;
            case UnitType.SHIELD:
                return 11;
            case UnitType.DITTO:
                return 12;
            case UnitType.LIGHTBULB:
                return 13;
            case UnitType.HEDGEHOG:
                return 14;

            // Not implemented or NONE will trigger this.
            default:
                throw new ArgumentException("Invalid unit type", "type");
        }
    }

    /// <summary>
    /// Set the material of the the unit.
    /// </summary>
    /// <param name="tileId">The GameManager unit identifier of the material.</param>
    private void SetUnitMaterial()
    {
        // Setup each unit animation.
        // TODO: Fill in all missing animations.
        List<Mesh2DAnimation> animations = new List<Mesh2DAnimation>() {
            new Mesh2DAnimation("idle", new List<int>()
                { 9, 14 }, 1.0f, true, gameObject),
            new Mesh2DAnimation("attacking", new List<int>()
                { 14, 9, 4, 4, 4, 4, 14, 9 }, 0.10f, false, gameObject),
            new Mesh2DAnimation("damaged", new List<int>()
                { 0, 0 }, 1.0f, false, gameObject),
            new Mesh2DAnimation("walking_west", new List<int>()
                { 1, 6, 11, 16 }, 0.2f, true, gameObject),
            new Mesh2DAnimation("walking_south", new List<int>()
                { 2, 7, 12, 17 }, 0.2f, true, gameObject),
            new Mesh2DAnimation("walking_north", new List<int>()
                { 3, 8, 13, 18 }, 0.2f, true, gameObject),
        };

        SetMaterial(GetCurrentLayer(), GetUnitMaterialId(unit.type),
            MaterialType.UNIT, 33, 32, 0, animations, "attacking", true);
    }

    /// <summary>
    /// Called via SendMessage from Mesh2DAnimation when an animation is complete.
    /// Should not be invoked from anywhere else.
    /// </summary>
    /// <param name="name">The name of the animation that completed.</param>
    protected override void OnAnimationComplete(object name)
    {
        // The object passed into the method is type string.
        string animationName = name as String;

        // On completion of an attack set the animation back to idle.
        if (animationName == "attack"
            || animationName == "damaged"
            || animationName == "celebrate")
            SetAnimation(GetCurrentLayer(), "idle");
    }

    /// <summary>Reset the done state.</summary>
    public void Reset()
    {
        _done = false;
    }
}
