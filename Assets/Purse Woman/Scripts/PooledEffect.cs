using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledEffect : MonoBehaviour, IPooledObject
{
    [SerializeField] private ParticleSystem effect = null;

    public ParticleSystem Effect { get => effect; }

    public void OnObjectSpawn()
    {
        effect.Play();

    }
}
