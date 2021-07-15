using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleBag : MonoBehaviour, ICollectible
{
    public void GetCollected()
    {
        MainLevelManager._Instance.Character.AddBag();
        ObjectPooler.Instance.SpawnFromPool("Bag Collect Effect", transform.position + Vector3.up * 0.25f, Quaternion.identity);
        GetComponent<BoxCollider>().enabled = false;
        transform.LeanScale(Vector3.zero, 0.2f).setOnComplete(() => { Destroy(gameObject); });
    }
}
