using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    private List<Transform> transforms = null;
    public static TurnManager Instance = null;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            transforms = new List<Transform>();
        }
    }


    private void Update()
    {
        for (int i = 0; i < transforms.Count; i++)
        {
            transforms[i]?.Rotate(Vector3.up * 60 * Time.deltaTime);
        }
    }

    public void AddTransformToTurn(Transform trans)
    {
        transforms.Add(trans);
    }

    public void RemoveTransformToTurn(Transform trans)
    {
        transforms.Remove(trans);
    }
}
