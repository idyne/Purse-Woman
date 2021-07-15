using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace FateGames
{
    public class UIStartText : UIElement
    {
        private void Awake()
        {
            float multiplier = (Mathf.PingPong(Time.time / 2, 1) + 4f) / 5f;
            transform.localScale = Vector3.one * multiplier;
        }
        protected override void Animate()
        {
            float multiplier = (Mathf.PingPong(Time.time / 2, 1) + 4f) / 5f;
            transform.LeanScale(Vector3.one * multiplier, Time.deltaTime / 2f);
        }
    }
}