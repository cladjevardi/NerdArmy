﻿using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>The list of unit animations.</summary>
public enum ActorAnimation
{
    IDLE,
    WALKING,
    ATTACK,
    DAMAGE,
    DEATH,
    VICTORY,
}

public enum ActorFacing
{
    NORTH,
    EAST,
    SOUTH,
    WEST,
}

public enum ActorStrategy
{
    /// <summary>Stand there.</summary>
    NONE,
    /// <summary>Get as close as you can to your enemy.</summary>
    CHARGE_IN,
    /// <summary>Run as far away as possible.</summary>
    COWERS,
    /// <summary>Chuck Norris doesn't sleep. He waits.</summary>
    WAITS,
    /// <summary>Stay out of attack range of all enemies. Attack when near.</summary>
    CAUTIOUS,
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
                animationName = "idle_" + GetFacingString();
                break;
            case ActorAnimation.ATTACK:
                animationName = "attack_" + GetFacingString();
                break;
            case ActorAnimation.DAMAGE:
                animationName = "damage_" + GetFacingString();
                break;
            case ActorAnimation.DEATH:
                animationName = "death_" + GetFacingString();
                break;
            case ActorAnimation.WALKING:
                animationName = "walking_" + GetFacingString();
                break;
            case ActorAnimation.VICTORY:
                animationName = "victory_" + GetFacingString();
                break;

            // Not implemente
            default:
                throw new ArgumentException(
                    "Invalid actor animation specified", "animation");
        }

        SetAnimation(GetCurrentLayer(), animationName);
    }

    /// <summary>Internal facing value to handle flips.</summary>
    private ActorFacing _facing = ActorFacing.WEST;
    public ActorFacing facing
    {
        get { return _facing; }
        set {
            if (value != _facing)
            {
                // Flip the scale
                Vector3 scale = mesh.transform.localScale;
                scale.x *= -1;
                mesh.transform.localScale = scale;
            }

            // Adjust the center.
            mesh.transform.localPosition = (value == ActorFacing.WEST) ? new Vector2(0, 0) : new Vector2(1, 0);
            _facing = value;
        }
    }

    /// <summary>
    /// Get the string of the direction the actor is facing.
    /// </summary>
    /// <returns>
    /// Returns the string of the direction the actor is facing.
    /// </returns>
    private string GetFacingString()
    {
        switch (facing)
        {
            case ActorFacing.NORTH:
                return "north";
            case ActorFacing.EAST:
                return "east";
            case ActorFacing.SOUTH:
                return "south";
            case ActorFacing.WEST:
                return "west";
        }

        return "";
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

            // Assign the units material ids.
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
            {
                RemoveLayer(GetCurrentLayer());
                RemoveLayer(Mesh2DLayer.LAYER_HEALTH);
            }
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
    /// Whether the character has an attack indicator overhead.
    /// </summary>
    private bool _attackIndicator = false;
    public bool attackIndicator
    {
        get { return _attackIndicator; }
        set {
            if (value)
                SetAttackIndicator();
            else
                RemoveLayer(Mesh2DLayer.LAYER_ATTACK_MARKER);
            _attackIndicator = value;
        }
    }

    /// <summary>
    /// The strategy to use if controlled by an AI.
    /// </summary>
    private ActorStrategy _strategy = ActorStrategy.CHARGE_IN;
    public ActorStrategy strategy
    {
        get { return _strategy; }
        set { _strategy = value; }
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
    /// Set the material of the the unit.
    /// </summary>
    private void SetUnitMaterial()
    {
        SetMaterial(GetCurrentLayer(), unit.materialId, MaterialType.UNIT,
            unit.cellWidth, unit.cellHeight, unit.materialId,
            unit.animations, "idle_east", true);
    }

    private void SetAttackIndicator()
    {
        SetMaterial(Mesh2DLayer.LAYER_ATTACK_MARKER, 8, MaterialType.EFFECT);
    }

    /// <summary>
    /// Set the material of the health bar
    /// </summary>
    public void SetHealthMaterial(bool isOwner)
    {
        SetMaterial(Mesh2DLayer.LAYER_HEALTH, 9, MaterialType.EFFECT,
            160, 16, isOwner ? 4 : 1);
    }

    private void CreateHealthText()
    {
        /*
        Canvas can = canvas.AddComponent<Canvas>();
        +can.renderMode = RenderMode.ScreenSpaceOverlay;
        +
        // Create a UI.Text object.
        +transitionText = new GameObject("TransitionText");
        +transitionText.transform.SetParent(canvas.gameObject.transform);
        +Text text = transitionText.AddComponent<Text>();
        +text.text = "";
        +text.color = new Color(0, 0, 0, 255);
        +text.transform.localPosition = new Vector2(0, 0);
        +text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        +text.fontSize = 32;
        +text.fontStyle = FontStyle.Italic;
        +RectTransform rect = text.GetComponent<RectTransform>();
        +rect.sizeDelta = new Vector2(300, 100);
        */
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
        if (animationName == "attack_north"
            || animationName == "attack_east"
            || animationName == "attack_south"
            || animationName == "attack_west"
            || animationName == "damage_north"
            || animationName == "damage_east"
            || animationName == "damage_south"
            || animationName == "damage_west"
            || animationName == "victory_north"
            || animationName == "victory_east"
            || animationName == "victory_south"
            || animationName == "victory_west")
        {
            SetAnimation(GetCurrentLayer(), "idle_" + GetFacingString());
        }
    }
}
