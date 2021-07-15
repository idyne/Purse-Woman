using FSG.MeshAnimator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thief : MonoBehaviour
{
    [SerializeField] private float speed = 1;
    [SerializeField] private MeshAnimator meshAnimator = null;
    private bool run = false;
    private void HitBag(Bag bag)
    {
        GetComponent<Collider>().enabled = false;
        MainLevelManager levelManager = MainLevelManager._Instance;
        levelManager.Character.HitByThief(bag.Index, bag.IsRight, transform.position + Vector3.up * 1);
    }

    private void HitWoman()
    {
        GetComponent<Collider>().enabled = false;
        MainLevelManager levelManager = MainLevelManager._Instance;
        levelManager.Character.HitByThief();
    }
    private void Update()
    {
        if (run)
            MoveForward();
    }
    private void MoveForward()
    {
        transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, Time.deltaTime * speed);
    }

    public void StartRunning()
    {
        if (!run)
        {
            meshAnimator.Play(0);
            run = true;
            LeanTween.delayedCall(5, () => { gameObject.SetActive(false); });
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bag"))
            HitBag(other.GetComponent<Bag>());
        else if (other.CompareTag("Character"))
            HitWoman();
    }
}
