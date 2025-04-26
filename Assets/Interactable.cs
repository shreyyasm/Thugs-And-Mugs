using UnityEngine;

public class Interactable : MonoBehaviour
{
    public enum InteractableType
    {
        defaultItem,
        Tree,
        Broom,
        // Add more types here
    }

    [Header("Interactable Settings")]
    public InteractableType interactableType;
    public bool isPickable;

    private void Update()
    {
       
    }

    public void Interact()
    {
        switch (interactableType)
        {
            case InteractableType.Tree:
                InteractWithTree();
                break;

            case InteractableType.Broom:
                InteractWithBroom();
                break;

            // Add more cases for new types
            default:
                Debug.LogWarning("No interaction defined for this type.");
                break;
        }
    }

    private void InteractWithTree()
    {
        TreeScript tree = GetComponent<TreeScript>();
        if (tree != null)
        {
            Vector3 playerPosition = Camera.main.transform.position; // assuming single player cam
            tree.TakeDamage(34f, playerPosition);
        }
    }

    private void InteractWithBroom()
    {
        Debug.Log("You picked up the broom.");
        // Add broom-specific logic here
    }
}
