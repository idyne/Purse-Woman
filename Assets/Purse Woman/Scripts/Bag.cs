using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bag : MonoBehaviour
{
    [SerializeField] private bool isRight = false;
    [SerializeField] private int index = -1;

    public bool IsRight { get => isRight; }
    public int Index { get => index; }
}
