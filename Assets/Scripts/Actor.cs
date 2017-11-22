using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The unity representation of a unit in combat or a mission. This information
/// represents temporary status.
/// </summary>
public class Actor : MonoBehaviour
{
    /// <summary>
    /// The renderer for the tile.
    /// </summary>
    private GameObject tileRenderer = null;

    /// <summary>The list of unit animations.</summary>
    public enum AnimationType
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
    /// Set the material of the the unit.
    /// </summary>
    /// <param name="tileId">The GameManager unit identifier of the material.</param>
    public void SetUnitMaterial(int tileId)
    {
        // Find the layer of the tile.
        TileRenderer.TileLayer layer = TileRenderer.TileLayer.LAYER_UNITS;
        if (_invisible)
            layer = TileRenderer.TileLayer.LAYER_INVISIBLE;
        else if (_burrowed)
            layer = TileRenderer.TileLayer.LAYER_BURROWED;
        else if (_flying)
            layer = TileRenderer.TileLayer.LAYER_FLYINGUNITS;

        // Assign the unit and setup unit animation.
        MaterialId materialId = new MaterialId(tileId, MaterialType.UNIT);

        // All unit sprite maps are setup with the same dimensions to avoid
        // special code for each of their animations.
        materialId.cellWidth = 33;
        materialId.cellHeight = 32;

        // Inform this actor about animation messages we care about.
        materialId.animationManager.messageReceiver = gameObject;

        // Setup each unit animation.
        // TODO: Fill in all missing animations.
        materialId.animationManager.AddAnimation("idle",
            new List<int>() { 9, 14 }, 1.0f);
        materialId.animationManager.AddAnimation("attack",
            new List<int>() { 14, 9, 4, 4, 4, 4, 14, 9 }, 0.10f, false);
        materialId.animationManager.AddAnimation("damaged",
            new List<int>() { 0, 0 }, 1.0f, false);
        materialId.animationManager.AddAnimation("walking_west",
            new List<int>() { 1, 6, 11, 16 }, 0.2f);
        materialId.animationManager.AddAnimation("walking_south",
            new List<int>() { 2, 7, 12, 17 }, 0.2f);
        materialId.animationManager.AddAnimation("walking_north",
            new List<int>() { 3, 8, 13, 18 }, 0.2f);
        materialId.animationManager.SetCurrentAnimation("attack");
        materialId.animationManager.PlayAnimation();

        // Assign the material to the unit.
        tileRenderer.GetComponent<TileRenderer>().SetMaterial(layer, materialId);
    }

    /// <summary>Set the actor animation.</summary>
    /// <param name="animationType">The animation to play.</param>
    public void SetAnimation(AnimationType animationType)
    {
        // Get the string animation name.
        string animationName = "";
        switch (animationType)
        {
            case AnimationType.IDLE:
                animationName = "idle";
                break;
            case AnimationType.ATTACKING:
                animationName = "attacking";
                break;
            case AnimationType.DAMAGED:
                animationName = "damaged";
                break;
            case AnimationType.WALKING_NORTH:
                animationName = "walking_north";
                break;
            case AnimationType.WALKING_EAST:
                animationName = "walking_east";
                break;
            case AnimationType.WALKING_SOUTH:
                animationName = "walking_south";
                break;
            case AnimationType.WALKING_WEST:
                animationName = "walking_west";
                break;

            // Not implemente
            default:
                throw new ArgumentException("Invalid actor animation specified", "animationType");
        }

        SetAnimation(animationName);
    }

    /// <summary>
    /// The Unit information. Use UnitFactor.Create() a cutout UnitType.
    /// </summary>
    private Unit _unit = null;
    public Unit unit
    {
        get { return _unit; }
        set
        {
            _unit = value;

            // Apply the flying trait based on the unit.
            flying = _unit.flying;

            // Assign the current material id.
            SetUnitMaterial(GetUnitMaterialId(_unit.type));
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
        set { _health = value; }
    }

    /// <summary>
    /// Unit positional information.
    /// </summary>
    private Vector2 _position = Vector2.zero;
    public Vector2 position
    {
        get { return tileRenderer.GetComponent<TileRenderer>().GetPosition(); }
        set { tileRenderer.GetComponent<TileRenderer>().SetPosition(value); }
    }

    /// <summary>
    /// If the unit is currently flying.
    /// </summary>
    private bool _flying = false;
    public bool flying
    {
        get { return _flying; }
        set
        {
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
        set
        {
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
        set
        {
            // TODO: Move the current material to the invisible layer.
            _invisible = value;
        }
    }

    private TileRenderer.TileLayer GetCurrentLayer()
    {
        TileRenderer.TileLayer layer = TileRenderer.TileLayer.LAYER_UNITS;
        if (_invisible)
            layer = TileRenderer.TileLayer.LAYER_INVISIBLE;
        else if (_burrowed)
            layer = TileRenderer.TileLayer.LAYER_BURROWED;
        else if (_flying)
            layer = TileRenderer.TileLayer.LAYER_FLYINGUNITS;
        return layer;
    }

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
    /// Called via SendMessage from MaterialAnimation when an animation is complete.
    /// Should not be invoked from anywhere else.
    /// </summary>
    /// <param name="name">The name of the animation that completed.</param>
    private void OnAnimationComplete(object name)
    {
        // The object passed into the method is type string.
        string animationName = name as String;

        // On completion of an attack set the animation back to idle.
        if (animationName == "attack"
            || animationName == "damaged"
            || animationName == "celebrate")
            SetAnimation("idle");
    }

    /// <summary>
    /// Internal version of setting the animation.
    /// </summary>
    /// <param name="name">The string name of the animation.</param>
    private void SetAnimation(string name)
    {
        // Set the animation of the current material at the units layer.
        TileRenderer _renderer = tileRenderer.GetComponent<TileRenderer>();
        MaterialId materialId = _renderer.GetMaterial(GetCurrentLayer());
        materialId.animationManager.SetCurrentAnimation(name);
        materialId.animationManager.PlayAnimation();
        _renderer.SetMaterial(GetCurrentLayer(), materialId);
    }

    /// <summary>Call at creation of object.</summary>
    private void Awake()
    {
        tileRenderer = new GameObject("TileRenderer");
        tileRenderer.transform.parent = transform;
        tileRenderer.AddComponent<TileRenderer>();
    }
}
