using Shreyas;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class CraftableItem : MonoBehaviour
{
    public CraftableItemData craftableItemData;
    public GameObject Canvas;
    public int neededWood;
    public int currentWood;
    public TextMeshProUGUI woodNeedText;
    public List<RendererMaterialPair> originalMaterialsList = new List<RendererMaterialPair>();
    // Runtime dictionary (optional, if you need fast access later)
    private Dictionary<Renderer, Material> originalMaterials = new();
    [System.Serializable]
    public class RendererMaterialPair
    {
        public Renderer renderer;
        public Material material;
    }
    private void Awake()
    {
        neededWood = craftableItemData.woodRequied;
    }
    void Start()
    {
        UpdateText();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UpdateText()
    {
        woodNeedText.text = "Wood Planks " + currentWood + "/" + neededWood;
    }
    public void AddWood()
    {
        currentWood++;
        UpdateText();
        if(currentWood == neededWood)
        {
            foreach (var pair in originalMaterialsList)
            {
                if (pair.renderer != null && pair.material != null)
                {
                    pair.renderer.material = pair.material;
                }
            }
            Canvas.SetActive(false);
            GetComponent<Interactable>().interactableType = Interactable.InteractableType.defaultItem;
            GetComponent<Interactable>().requiredItemTag = null;

        }
    }
}
