﻿using System;
using System.Xml;
using System.Collections.Generic;
using UnityEngine;

/// <summary>The database for all unit information.</summary>
public class UnitDatabase
{
    /// <summary>World mission blueprints.</summary>
    private List<UnitSchematic> unitData = new List<UnitSchematic>();

    /// <summary>The constructor that builds all world mission blueprints.</summary>
    private void LoadDatabase()
    {
        // Load the unit database from the resources folder.
        TextAsset xmlSourceAsset = Resources.Load("UnitDatabase") as TextAsset;
        if (xmlSourceAsset == null)
            return;

        // Iterate through each unit node and generate schematic data to add to
        // the unit database.
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlSourceAsset.text);
        foreach (XmlNode unitNode in xmlDoc.DocumentElement.ChildNodes)
        {
            // Fill out the unit schematic from xml
            UnitSchematic unitSchematic = new UnitSchematic();
            unitSchematic.name = unitNode.Attributes["name"].InnerText;
            unitSchematic.baseMaxHealth = GetIntList(unitNode.SelectSingleNode(".//base_max_health").InnerText);
            unitSchematic.baseDamage = GetIntList(unitNode.SelectSingleNode(".//base_damage").InnerText);
            unitSchematic.baseMinRange = new List<int>();
            unitSchematic.baseMaxRange = new List<int>();
            unitSchematic.baseMovement = GetIntList(unitNode.SelectSingleNode(".//base_movement").InnerText);
            unitSchematic.baseArmor = int.Parse(unitNode.SelectSingleNode(".//base_armor").InnerText);

            // Fill in the range information.
            List<string> values = GetStringList(unitNode.SelectSingleNode(".//base_range").InnerText);
            foreach (var value in values)
            {
                // Convert the '-' delimited strings into range minimums
                // and maximums. Keep in mind that ranges can just be 1 value.
                List<int> range = GetIntRange(value);
                if (range.Count == 2)
                {
                    // A range was specified.
                    unitSchematic.baseMinRange.Add(range[0]);
                    unitSchematic.baseMaxRange.Add(range[1]);
                }
                else if (range.Count == 1)
                {
                    // A single value was specified.
                    unitSchematic.baseMinRange.Add(range[0]);
                    unitSchematic.baseMaxRange.Add(range[0]);
                }
            }

            // Parse the material node.
            XmlNode materialNode = unitNode.SelectSingleNode(".//material");
            if (materialNode == null)
                Debug.Log("Unit is missing material information.");

            // Process the material node.
            unitSchematic.material = LoadMaterialNode(materialNode);

            // Add the schematic to the unit database.
            unitData.Add(unitSchematic);
        }
    }

    /// <summary>Load the material xml node.</summary>
    /// <param name="materialNode">The material node within xml.</param>
    /// <returns>Returns the unit material from the material xml node.</returns>
    private UnitMaterial LoadMaterialNode(XmlNode materialNode)
    {
        UnitMaterial material = new UnitMaterial();
        material.id = int.Parse(materialNode.Attributes["id"].InnerText);
        foreach (XmlNode animationNode in materialNode)
        {
            UnitAnimation animation = new UnitAnimation();
            animation.name = animationNode.Attributes["name"].InnerText;
            animation.speed = float.Parse(animationNode.Attributes["speed"].InnerText);
            if (animationNode.Attributes["on_complete"] != null)
                animation.onComplete = animationNode.Attributes["on_complete"].InnerText;
            if (animationNode.Attributes["loop"] != null)
                animation.loop = bool.Parse(animationNode.Attributes["loop"].InnerText);
            animation.northSequence = GetIntList(materialNode.SelectSingleNode(".//north_sequence").InnerText);
            animation.eastSequence = GetIntList(materialNode.SelectSingleNode(".//east_sequence").InnerText);
            animation.southSequence = GetIntList(materialNode.SelectSingleNode(".//south_sequence").InnerText);
            material.animations.Add(animation);
        }
        return material;
    }

    /// <summary>Get the list of ints from a string delimited with '|'.</summary>
    /// <param name="value">The string value to split.</param>
    /// <returns>
    /// Returns the list of ints within a string delimited with '|'
    /// </returns>
    private List<int> GetIntList(string value)
    {
        List<int> list = new List<int>();
        String[] substrings = value.Split('|');
        foreach (string substring in substrings)
            list.Add(int.Parse(substring));
        return list;
    }

    /// <summary>Get the list of strings from a string delimited with '|'.</summary>
    /// <param name="value">The string value to split.</param>
    /// <returns>
    /// Returns the list of strings within a string delimited with '|'
    /// </returns>
    private List<string> GetStringList(string value)
    {
        List<string> list = new List<string>();
        String[] substrings = value.Split('|');
        foreach (string substring in substrings)
            list.Add(substring);
        return list;
    }

    /// <summary>Get the list of ints from a string delimited with '-'.</summary>
    /// <param name="value">The string value to split.</param>
    /// <returns>
    /// Returns the list of ints within a string delimited with '-'
    /// </returns>
    private List<int> GetIntRange(string value)
    {
        List<int> list = new List<int>();
        String[] substrings = value.Split('-');
        foreach (string substring in substrings)
            list.Add(int.Parse(substring));
        return list;
    }

    /// <summary>Load the database from the unit database xml file.</summary>
    public void Initialize()
    {
        // Read from the mission database xml.
        LoadDatabase();
    }

    /// <summary>Get the units information from the database.</summary>
    /// <param name="type">The unit information to acquire.</param>
    /// <returns>Returns the unit schematic associated with that unit.</returns>
    public UnitSchematic GetUnit(UnitType type)
    {
        // Iterate through each schematic and look for the unit type specified.
        foreach (UnitSchematic schematic in unitData)
        {
            if (schematic.type == type)
                return schematic;
        }

        throw new ArgumentException("Invalid unit type", "type");
    }
}

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

public class UnitMaterial
{
    /// <summary>
    /// The material id of the unit animation. This id is used
    /// as a lookup for the unit materials in GameManager.
    /// </summary>
    public int id;

    /// <summary>
    /// The list of animations from this material for a unit.
    /// </summary>
    public List<UnitAnimation> animations = new List<UnitAnimation>();
}

public class UnitAnimation
{
    /// <summary>The name of the animation.</summary>
    public string name;

    /// <summary>The speed of the animation.</summary>
    public float speed = 0.0f;

    /// <summary>
    /// The animation to play on complete. Should not be used
    /// with loop.
    /// </summary>
    public string onComplete;

    /// <summary>Whether the animation should loop forever.</summary>
    public bool loop;

    /// <summary>The frame id sequence of the animation facing north.</summary>
    public List<int> northSequence;

    /// <summary>The frame id sequence of the animation facing east.</summary>
    public List<int> eastSequence;

    /// <summary>The frame id sequence of the animation facing south.</summary>
    public List<int> southSequence;
}

public class UnitSchematic
{
    /// <summary>The name of the unit.</summary>
    public string name;

    // Base statistics
    /// <summary> The base max health pool of the unit.</summary>
    public List<int> baseMaxHealth;

    /// <summary>The base damage of the unit when attacking.</summary>
    public List<int> baseDamage;

    /// <summary>The base minimum attack range of the unit when attacking.</summary>
    public List<int> baseMinRange;

    /// <summary>The base maximum attack range of the unit when attacking.</summary>
    public List<int> baseMaxRange;

    /// <summary>The base movement speed of the unit when moving.</summary>
    public List<int> baseMovement;

    /// <summary>The base armor of the unit when dealt damage.</summary>
    public int baseArmor;

    /// <summary>Whether the unit flies.</summary>
    public bool flying;

    /// <summary>The list of abilities the unit has available.</summary>
    public List<AbiliyType> abilities;

    /// <summary>The list of passives the unit has available.</summary>
    public List<PassiveType> passives;

    /// <summary>The base type of unit.</summary>
    public UnitType type;

    /// <summary>The material animations of the unit.</summary>
    public UnitMaterial material;
}
