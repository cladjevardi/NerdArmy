using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NerdArmy;

/// <summary>
/// The unity representation of a unit in combat or a mission. This information
/// represents temporary status.
/// </summary>
public class Actor : MonoBehaviour
{
    /// <summary>
    /// The Unit information. Use UnitFactor.Create() a cutout UnitType.
    /// </summary>
    private Unit _unit = null;
    public Unit unit
    {
        get { return _unit; }
        set { _unit = value; }
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

    // TODO: Grid positional information.
    // TODO: Graphical representation in combats. You can use unit.type to get
    // the UnitType of the unit for implementation details.
}
