using UnityEngine;

/// <summary>The lookup table for the material</summary>
public enum MaterialType
{
    NONE = -1,
    TILE,
    UNIT,
    EFFECT,
}

/// <summary>A class that identifies a given material from GameManager.</summary>
public class MaterialId
{
    /// <summary>The materoal lookup table type.</summary>
    private MaterialType _type = MaterialType.NONE;
    public MaterialType type
    {
        get { return _type; }
        set { _type = value; }
    }

    /// <summary>The identifier of the material. Specified in GameManager.</summary>
    private int _id = 0;
    public int id
    {
        get { return _id; }
        set { _id = value; }
    }

    /// <summary>
    /// Get the material from the global material table in GameManager.
    /// </summary>
    public Material GetMaterial()
    {
        // First verify if the information associated with this MaterialId
        // will actually give a proper Material.
        if (!IsValid())
        {
            // The material failed to load.
            Debug.LogError("Invalid MaterialId (type=" + type.ToString() + " id=" + id);
            return null;
        }

        // Loopup the material from the global material table.
        switch (type)
        {
            case MaterialType.TILE:
                return GameManager.instance.tileMaterials[id];
            case MaterialType.UNIT:
                return GameManager.instance.unitMaterials[id];
            case MaterialType.EFFECT:
                return GameManager.instance.effectMaterials[id];

            // We should never reach here is IsValid is doing its
            // job properly.
            default:
                return null;
        }
    }

    /// <summary>
    /// Constructor for a MaterialId
    /// </summary>
    /// <param name="id">The identifier of the material.</param>
    /// <param name="type">The material lookup table type.</param>
    public MaterialId(int id, MaterialType type = MaterialType.TILE)
    {
        this.id = id;
        this.type = type;
    }

    /// <summary>Clear any references to older material information</summary>
    public void Clear()
    {
        // Reset any values to their defaults.
        id = 0;
        type = MaterialType.NONE;
    }

    public bool IsValid()
    {
        // Get the global tile map array sizes before lookup.
        int arrayLength = 0;
        if (type == MaterialType.TILE)
            arrayLength = GameManager.instance.tileMaterials.Length;
        if (type == MaterialType.UNIT)
            arrayLength = GameManager.instance.unitMaterials.Length;
        if (type == MaterialType.EFFECT)
            arrayLength = GameManager.instance.effectMaterials.Length;

        // The id cannot exceed the bounds of the array nor be assigned to none.
        return id < arrayLength 
            && id >= -1 
            && type != MaterialType.NONE;
    }
}