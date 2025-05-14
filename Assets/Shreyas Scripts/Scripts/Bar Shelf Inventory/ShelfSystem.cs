using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
namespace Shreyas
{
    public class ShelfSystem : MonoBehaviour
    {
        public enum ShelfItemType { None, StraightShot, WidowKiss, SnakeBite, BlackBlaze, CactusBomb, DesertDraught, FireOut, FrothyMug, ShinerSip }

        [Header("Shelf Setup")]
        public Transform[] itemPositions = new Transform[8];
        public float moveSpeed = 2f;

        private ShelfItemType shelfType = ShelfItemType.None;
        private string requiredItemTag = null;

        private List<GameObject> currentItems = new List<GameObject>();
        private bool isFillingShelf = false;

        public void FillShelf(GameObject barrel)
        {
            if (isFillingShelf || currentItems.Count >= itemPositions.Length) return;
            if (barrel == null) return;
           
            StartCoroutine(PlaceOneItemToShelf(barrel));
        }

        IEnumerator PlaceOneItemToShelf(GameObject barrel)
        {
            isFillingShelf = true;
            Barrel barrelScript = barrel.GetComponent<Barrel>();
            if (barrelScript == null)
            {
               
                isFillingShelf = false;
                yield break;
               
            }
            bool bottlePlaced = false;

            // Try to find a matching bottle
            for (int i = 0; i < barrel.transform.childCount; i++)
            {
                Transform bottle = barrel.transform.GetChild(i);
                string bottleTag = bottle.tag;

                // Lock shelf type on first matching bottle
                if (shelfType == ShelfItemType.None && bottleTag == barrelScript.itemType.ToString())
                {
                    shelfType = barrelScript.itemType;
                    requiredItemTag = bottleTag;

                   
                }
               
                if (bottleTag == requiredItemTag)
                {
                    
                    // Cache world transform
                    Vector3 worldPos = bottle.position;
                    Quaternion worldRot = bottle.rotation;
                    Vector3 worldScale = bottle.lossyScale;

                    // Detach
                    bottle.SetParent(null);
                    bottle.position = worldPos;
                    bottle.rotation = worldRot;
                    bottle.localScale = worldScale;

                    int shelfIndex = itemPositions.Length - 1 - currentItems.Count;
                    currentItems.Add(bottle.gameObject);

                    Transform target = itemPositions[shelfIndex];
                    Quaternion lookRotation = Quaternion.LookRotation(transform.right, Vector3.up);
                   
                    bottle.DOMove(target.position, 0.4f).SetEase(Ease.InOutSine);
                    bottle.DORotateQuaternion(lookRotation, 0.4f).SetEase(Ease.InOutSine).OnComplete(() =>
                    {
                        bottle.SetParent(this.transform, worldPositionStays: true);
                        bottle.GetComponent<BoxCollider>().isTrigger = false;
                        bottle.GetComponent<Rigidbody>().isKinematic = false;
                        bottle.GetComponent<Rigidbody>().useGravity = true;

                        //Vector3 parentScale = this.transform.lossyScale;
                        //bottle.localScale = new Vector3(
                        //    worldScale.x / parentScale.x,
                        //    worldScale.y / parentScale.y,
                        //    worldScale.z / parentScale.z
                        //);

                    });

                    yield return new WaitForSeconds(0.4f);
                    bottlePlaced = true;
                    break;
                }
            }

            isFillingShelf = false;

         
            //// Destroy barrel if empty
            if (barrel.transform.childCount < 3)
            {
                Destroy(barrel);
                InventoryManager.instance.ClearInventorySlot();
            }
        }
    }

}

