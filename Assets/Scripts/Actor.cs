using System;
using System.Collections;
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

public enum ActorHealthColor
{
    GREEN,
    RED,
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

        Debug.Log("Actor: " + name + " Animation Started: " + animationName);
        SetAnimation(GetCurrentLayer(), animationName);
    }

    /// <summary>
    /// Helper function for animating taking damage.
    /// </summary>
    /// <param name="damage">The amount of damage taken.</param>
    public void TakeDamage(int damage)
    {
        // Calculate the health % for damage 
        StartCoroutine(SmoothHealth(health, health - damage));
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
                return "east"; // We use transform to flip the sprite
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
        internal set { _health = value; }
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
    /// The color of the health bars to use for this actor. Red
    /// typically being enemy units. Green being friendly units.
    /// </summary>
    private ActorHealthColor _healthBarColor = ActorHealthColor.GREEN;
    public ActorHealthColor healthBarColor
    {
        get { return _healthBarColor; }
        set {
            _healthBarColor = value;
            SetHealthMaterial(1.0f, 1.0f);
        }
    }

    /// <summary>
    /// Whether the actor is animating damage currently.
    /// </summary>
    private bool _animatingDamage = false;
    public bool animatingDamage
    {
        get { return _animatingDamage; }
        internal set { _animatingDamage = value; }
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
            unit.animations, "idle_east", true, gameObject);
    }

    /// <summary>
    /// Create the material for the attack indicator.
    /// </summary>
    private void SetAttackIndicator()
    {
        SetMaterial(Mesh2DLayer.LAYER_ATTACK_MARKER, 8, MaterialType.EFFECT, -1, -1, 0, null);
    }

    /// <summary>
    /// Set the material of the health bar
    /// </summary>
    private void SetHealthMaterial(double percentDamaged, double percentRemaining)
    {
        int damagedFrameId = healthBarColor == ActorHealthColor.GREEN ? 2 : 0;
        int remainingFrameId = healthBarColor == ActorHealthColor.GREEN ? 4 : 1;

        SetMaterial(Mesh2DLayer.LAYER_HEALTH_BASE, 9, MaterialType.EFFECT,
            160, 16, 3, 1.0);
        SetMaterial(Mesh2DLayer.LAYER_HEALTH_DAMAGED, 9, MaterialType.EFFECT,
            160, 16, damagedFrameId, percentDamaged);
        SetMaterial(Mesh2DLayer.LAYER_HEALTH_REMAINING, 9, MaterialType.EFFECT,
            160, 16, remainingFrameId, percentRemaining);
    }

    /// <summary>
    /// Wtf am I doing.
    /// </summary>
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
    /// Animate health bar decreasing.
    /// </summary>
    /// <param name="startHealth">Starting health value.</param>
    /// <param name="endHealth">Ending health value.</param>
    /// <returns>Unity coruitine.</returns>
    private IEnumerator SmoothHealth(int startHealth, int endHealth)
    {
        // Taking damage.
        SetAnimation(ActorAnimation.DAMAGE);

        // Tell anyone who wants to know that we're animating.
        animatingDamage = true;
        Vector2 healthVector = new Vector2(startHealth, 0);
        Vector2 healthEndVector = new Vector2(endHealth, 0);

        // While that distance is greater than a very small amount (Epsilon, almost zero):
        while (healthVector != healthEndVector)
        {
            // Find a new position proportionally closer to the end, based on the moveTime
            healthVector = Vector2.MoveTowards(
                healthVector, healthEndVector, (1f / .5f) * Time.deltaTime);

            double percentageDamaged = (double)((double)healthVector.x / (double)unit.baseMaxHealth);
            double percentageRemaining = (double)((double)healthEndVector.x / (double)unit.baseMaxHealth);
            SetHealthMaterial(percentageDamaged, percentageRemaining);
            yield return null;
        }

        health = endHealth;
        if (health <= 0)
        {
            // Play death animation.
            SetAnimation(ActorAnimation.DEATH);

            // Postpone telling everyone attacking is complete until
            // the death animation finishes.
        }
        else
        {
            // Tell everyone that moving is complete.
            animatingDamage = false;
        }
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

        Debug.Log("Actor: " + this.name + " Animation Complete: " + animationName);

        if (animationName == "damage_north"
            || animationName == "damage_east"
            || animationName == "damage_south"
            || animationName == "damage_west")
        {
            SetAnimation(GetCurrentLayer(), "idle_" + GetFacingString());
        }

        if (animationName == "victory_north"
            || animationName == "victory_east"
            || animationName == "victory_south"
            || animationName == "victory_west")
        {
            SetAnimation(GetCurrentLayer(), "idle_" + GetFacingString());
        }

        // On completion of an attack set the animation back to idle.
        if (animationName == "attack_north"
            || animationName == "attack_east"
            || animationName == "attack_south"
            || animationName == "attack_west")
        {
            SetAnimation(GetCurrentLayer(), "idle_" + GetFacingString());
        }

        // On death we destroy ourself.
        if (animationName == "death_north"
            || animationName == "death_east"
            || animationName == "death_south"
            || animationName == "death_west")
        {
            animatingDamage = false;
            RemoveLayer(GetCurrentLayer());
            RemoveLayer(Mesh2DLayer.LAYER_HEALTH_BASE);
            RemoveLayer(Mesh2DLayer.LAYER_HEALTH_DAMAGED);
            RemoveLayer(Mesh2DLayer.LAYER_HEALTH_REMAINING);
            DestroyObject(this);
        }
    }
}
