using System.Collections;
using System.Collections.Generic;
using System.Threading;
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

        public GameObject shelf;
        public void FillShelf(GameObject barrel)
        {
            if (isFillingShelf || currentItems.Count >= itemPositions.Length) return;
            if (barrel == null) return;
           
            StartCoroutine(PlaceOneItemToShelf(barrel));
        }

        IEnumerator PlaceOneItemToShelf(GameObject barrel)
        {
            CancelResetAnimatorBool();
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

                    Sequence moveSequence = DOTween.Sequence();

                    Vector3 liftPosition = bottle.position + Vector3.up * 0.5f; // Adjust lift height
                    Vector3 finalPosition = target.position;
                    
                    // Move up first
                    moveSequence.Append(bottle.DOMove(liftPosition, 0.2f).SetEase(Ease.OutSine));

                    // Move to target position
                    moveSequence.Append(bottle.DOMove(finalPosition, 0.3f).SetEase(Ease.InOutSine));

                    // Rotate during the second move
                    bottle.DORotateQuaternion(lookRotation, 0.5f).SetEase(Ease.InOutSine);

                    // On complete, do your setup
                    moveSequence.OnComplete(() =>
                    {
                        bottle.SetParent(shelf.transform, true);
                        bottle.GetComponent<BoxCollider>().isTrigger = false;
                        bottle.GetComponent<Rigidbody>().isKinematic = false;
                        bottle.GetComponent<Rigidbody>().useGravity = true;
                    });


                    yield return new WaitForSeconds(0.4f);
                    // Stop any existing coroutine first
                    if (resetBoolCoroutine != null)
                        StopCoroutine(resetBoolCoroutine);

                    resetBoolCoroutine = StartCoroutine(ResetAnimatorBoolAfterDelay(barrel));
                    bottlePlaced = true;
                    break;
                }
            }

            isFillingShelf = false;

         
            //// Destroy barrel if empty
            if (barrel.transform.childCount < 3)
            {
                Destroy(barrel);
                InventoryManager.instance.BarrelsModels.Remove(barrel);
                InventoryManager.instance.ClearInventorySlot();
            }
            StopCoroutine("PlaceOneItemToShelf");
        }

        private Coroutine resetBoolCoroutine;
        // Call this to cancel the coroutine before it sets the bool
        public void CancelResetAnimatorBool()
        {
            if (resetBoolCoroutine != null)
            {
                StopCoroutine(resetBoolCoroutine);
                resetBoolCoroutine = null;
            }
        }

        IEnumerator ResetAnimatorBoolAfterDelay(GameObject barrel)
        {
          
            yield return new WaitForSeconds(2f);
            if(barrel != null) 
                barrel.GetComponent<Animator>().SetBool("OpenBarrel", false);

            resetBoolCoroutine = null; // Clear reference after done
        }
    }
    

}

