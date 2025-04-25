using UnityEngine;

public class MaterialInventory : MonoBehaviour
{
    public int wood = 100;
    public int brick = 50;
    public int metal = 25;

    public bool HasEnough(MaterialType type, int cost)
    {
        switch (type)
        {
            case MaterialType.Wood: return wood >= cost;
            case MaterialType.Brick: return brick >= cost;
            case MaterialType.Metal: return metal >= cost;
            default: return false;
        }
    }

    public void Spend(MaterialType type, int cost)
    {
        switch (type)
        {
            case MaterialType.Wood: wood -= cost; break;
            case MaterialType.Brick: brick -= cost; break;
            case MaterialType.Metal: metal -= cost; break;
        }
    }
}