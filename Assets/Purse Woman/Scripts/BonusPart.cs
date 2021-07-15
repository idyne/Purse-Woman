using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusPart : MonoBehaviour
{
    [SerializeField] private float multiplier = 1;
    [SerializeField] private Transform confetties = null;

    public float Multiplier { get => multiplier; }
    public Transform Confetties { get => confetties;}
}
