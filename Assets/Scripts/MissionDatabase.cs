using System.Xml;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// The database object for mission information.
/// </summary>
public class MissionDatabase
{
    /// <summary>World mission blueprints.</summary>
    private List<List<MissionSchematic>> worldMissionData = new List<List<MissionSchematic>>();

    /// <summary>The constructor that builds all world mission blueprints.</summary>
    private void LoadDatabase()
    {
        TextAsset xmlSourceAsset = Resources.Load("MissionDatabase") as TextAsset;
        if (xmlSourceAsset == null)
            return;

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlSourceAsset.text);
        foreach (XmlNode worldNode in xmlDoc.DocumentElement.ChildNodes)
        {
            worldMissionData.Add(LoadWorldNode(worldNode));
        }
    }

    /// <summary>Take a world xml node and fill in the children.</summary>
    /// <param name="worldNode">The node that represents a world.</param>
    /// <returns>Returns a list of missions for a world.</returns>
    private List<MissionSchematic> LoadWorldNode(XmlNode worldNode)
    {
        List<MissionSchematic> worldData = new List<MissionSchematic>();
        foreach (XmlNode schematicNode in worldNode.ChildNodes)
        {
            MissionSchematic schematic = new MissionSchematic();
            schematic.name = schematicNode.Attributes["name"].InnerText;
            schematic.tileWidth = int.Parse(schematicNode.Attributes["tile_width"].InnerText);
            schematic.tileHeight = int.Parse(schematicNode.Attributes["tile_height"].InnerText);
            schematic.tiles = LoadTilesNode(schematicNode);
            schematic.enemies = LoadEnemiesNode(schematicNode);
            schematic.rosterSpawns = LoadSpawnsNode(schematicNode);
            schematic.missionDialog = LoadDialogNode(schematicNode);
            worldData.Add(schematic);
        }

        return worldData;
    }

    /// <summary>Take a schematic node and fill a list of tiles.</summary>
    /// <param name="schematicNode">A node that respresents a mission.</param>
    /// <returns>Returns a list of tiles for a mission.</returns>
    private List<MissionTile> LoadTilesNode(XmlNode schematicNode)
    {
        List<MissionTile> tiles = new List<MissionTile>();
        XmlNode tilesNode = schematicNode.SelectSingleNode(".//tiles");
        if (tilesNode == null)
        {
            Debug.Log("Schematic missing tile information.");
            return tiles;
        }

        foreach (XmlNode tileNode in tilesNode.ChildNodes)
        {
            MissionTile tile = new MissionTile();
            tile.position = new Vector2(
                float.Parse(tileNode.Attributes["x"].InnerText, CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(tileNode.Attributes["y"].InnerText, CultureInfo.InvariantCulture.NumberFormat)
            );
            tile.groundCollision = tileNode.SelectSingleNode(".//ground_collision").InnerText == "true";
            tile.trueCollision = tileNode.SelectSingleNode(".//true_collision").InnerText == "true";
            tile.movementCost = int.Parse(tileNode.SelectSingleNode(".//movement_cost").InnerText);
            tile.materials = LoadMaterialsNode(tileNode);
            tiles.Add(tile);
        }

        return tiles;
    }

    /// <summary>Take a tile node and fill a list of materials.</summary>
    /// <param name="tileNode">A node that respresents a tile.</param>
    /// <returns>Returns a list of materials to use in a tile.</returns>
    private List<MissionMaterial> LoadMaterialsNode(XmlNode tileNode)
    {
        List<MissionMaterial> materials = new List<MissionMaterial>();
        XmlNode materialsNode = tileNode.SelectSingleNode(".//materials");
        if (materialsNode == null)
        {
            Debug.Log("Tile is missing material information.");
            return materials;
        }

        foreach (XmlNode materialNode in materialsNode)
        {
            MissionMaterial material = new MissionMaterial();
            string layer = materialNode.SelectSingleNode(".//layer").InnerText;
            if (layer == "floor")
                material.layer = MissionMaterial.Layer.FLOOR;
            if (layer == "object")
                material.layer = MissionMaterial.Layer.OBJECT;
            if (layer == "roof")
                material.layer = MissionMaterial.Layer.ROOF;
            material.materialId = int.Parse(materialNode.SelectSingleNode(".//material_id").InnerText);
            material.frameId = int.Parse(materialNode.SelectSingleNode(".//frame_id").InnerText);
            materials.Add(material);
        }

        return materials;
    }

    /// <summary>Take a schematic node and fill a list of enemies.</summary>
    /// <param name="schematicNode">A node that respresents a mission.</param>
    /// <returns>Returns a list of enemies for a mission.</returns>
    private List<MissionEnemy> LoadEnemiesNode(XmlNode schematicNode)
    {
        List<MissionEnemy> enemies = new List<MissionEnemy>();
        XmlNode enemiesNode = schematicNode.SelectSingleNode(".//enemies");
        if (enemiesNode == null)
        {
            Debug.Log("Schematic missing enemy information.");
            return enemies;
        }

        foreach (XmlNode enemyNode in enemiesNode.ChildNodes)
        {
            MissionEnemy enemy = new MissionEnemy();
            enemy.position = new Vector2(
                float.Parse(enemyNode.Attributes["x"].InnerText, CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(enemyNode.Attributes["y"].InnerText, CultureInfo.InvariantCulture.NumberFormat)
            );
            enemy.type = (UnitType)int.Parse(enemyNode.Attributes["type"].InnerText);
            enemies.Add(enemy);
        }

        return enemies;
    }

    /// <summary>Take a schematic node and fill a list of spawns.</summary>
    /// <param name="schematicNode">A node that respresents a mission.</param>
    /// <returns>Returns a list of spawns for a mission.</returns>
    private List<Vector2> LoadSpawnsNode(XmlNode schematicNode)
    {
        List<Vector2> spawns = new List<Vector2>();
        XmlNode spawnsNode = schematicNode.SelectSingleNode(".//spawns");
        if (spawnsNode == null)
        {
            Debug.Log("Schematic missing spawn information.");
            return spawns;
        }

        foreach (XmlNode spawnNode in spawnsNode.ChildNodes)
        {
            spawns.Add(new Vector2(
                float.Parse(spawnNode.Attributes["x"].InnerText, CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(spawnNode.Attributes["y"].InnerText, CultureInfo.InvariantCulture.NumberFormat)
            ));
        }
        return spawns;
    }

    /// <summary>Take a schematic node and get a list of dialog.</summary>
    /// <param name="schematicNode">A node that respresents a mission.</param>
    /// <returns>Returns a list of spawns for a mission.</returns>
    private List<MissionDialog> LoadDialogNode(XmlNode schematicNode)
    {
        List<MissionDialog> dialogs = new List<MissionDialog>();
        XmlNode dialogsNode = schematicNode.SelectSingleNode(".//dialog");
        if (dialogsNode == null)
        {
            return dialogs;
        }

        foreach (XmlNode dialogNode in dialogsNode.ChildNodes)
        {
            // TODO: Implement.
        }

        return dialogs;
    }

    /// <summary>Load the database from mission database xml.</summary>
    public void Initialize()
    {
        // Read from the mission database xml.
        LoadDatabase();
    }

    /// <summary>Retrieve a missions blueprints.</summary>
    /// <param name="world">The world id of the mission.</param>
    /// <param name="level">The level id of the mission.</param>
    /// <returns>Returns the missions blueprints.</returns>
    public MissionSchematic GetMission(int world, int level)
    {
        return worldMissionData[world - 1][level - 1];
    }
}

/// <summary>
/// The metadata that makes up a tile material to be displayed in a mission.
/// </summary>
public class MissionMaterial
{
    public enum Layer
    {
        FLOOR,
        OBJECT,
        ROOF,
    }

    /// <summary>The layer of the material id</summary>
    public Layer layer;

    /// <summary>The tile material id. Found in GameManager.</summary>
    public int materialId;

    /// <summary>The initial frame id from the tile material to use.</summary>
    public int frameId;

    /// <summary>
    /// The animation associated with this tile. Used for tiles like
    /// waterfalls, rivers, smoke, etc.
    /// </summary>
    //public Mesh2DAnimation animation;
}

/// <summary>
/// The metadata that makes up a tile to be displayed in a mission.
/// </summary>
public class MissionTile
{
    /// <summary>The position of the tile.</summary>
    public Vector2 position;

    /// <summary>The list of materials that make up this tile.</summary>
    public List<MissionMaterial> materials;

    /// <summary>The movement cost moving onto this tile.</summary>
    public int movementCost;

    /// <summary>Whether this tile disallows ground units to move through it.</summary>
    public bool groundCollision;

    /// <summary>Whether this tile disallows air units to move through it.</summary>
    public bool trueCollision;
}

/// <summary>
/// The metadata that makes up a list of enemies to place down during a mission.
/// </summary>
public class MissionEnemy
{
    /// <summary>The position of the enemy unit in the mission.</summary>
    public Vector2 position;

    /// <summary>The type of enemy unit to place.</summary>
    public UnitType type;
}

/// <summary>
/// The metadata that makes up a set of mission dialog to play during a mission.
/// </summary>
public class MissionDialog
{
    // TODO: Fill this out
    /// <summary>A string of dialog</summary>
    public string dialog;
}

/// <summary>
/// An enumerator that represents the factions participating in a mission.
/// </summary>
public enum MissionFaction
{
    PLAYER,
    COMPUTER1,
    COMPUTER2,
    COMPUTER3,

    // TODO: However many factions we need within a mission.
}

/// <summary>
/// Data that makes up an entire mission. Used to construct and display
/// predefined levels.
/// </summary>
public class MissionSchematic
{
    /// <summary>The name of the mission.</summary>
    public string name;

    /// <summary>The width of the map in tiles.</summary>
    public int tileWidth = 0;

    /// <summary>The height of the map in tiles.</summary>
    public int tileHeight = 0;

    /// <summary>The default starting player for the mission.</summary>
    public Owner startingPlayer = Owner.PLAYER1;

    /// <summary>For every mission. Player1 is always the player.</summary>
    public MissionFaction player1 = MissionFaction.PLAYER;

    /// <summary>What faction player 2 is associated with.</summary>
    public MissionFaction player2 = MissionFaction.COMPUTER1;

    /// <summary>What faction player 3 is associated with.</summary>
    public MissionFaction player3 = MissionFaction.COMPUTER2;

    /// <summary>What faction player 3 is associated with.</summary>
    public MissionFaction player4 = MissionFaction.COMPUTER3;

    /// <summary>
    /// Tile metadata that makes up how a mission is layed out.
    /// </summary>
    public List<MissionTile> tiles;

    /// <summary>
    /// The enemy units and their positional information for this mission.
    /// </summary>
    public List<MissionEnemy> enemies;

    /// <summary>
    /// The list of friendly positions to spawn units from your roster.
    /// </summary>
    public List<Vector2> rosterSpawns;

    /// <summary>
    /// The list of dialog to display during a mission.
    /// </summary>
    public List<MissionDialog> missionDialog;
}