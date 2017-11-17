using System;
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

        // Assign the unit.
        tileRenderer.GetComponent<TileRenderer>().SetUnitMaterial(layer, tileId);
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

    private int GetUnitMaterialId(UnitType unitType)
    {
        switch (unitType)
        {
            // Hero units.
            case UnitType.MAINCHARACTER:
                return 0;
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
                return 5;
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

    /// <summary>Call at creation of object.</summary>
    private void Awake()
    {
        tileRenderer = new GameObject("TileRenderer");
        tileRenderer.transform.parent = transform;
        tileRenderer.AddComponent<TileRenderer>();
    }
}
