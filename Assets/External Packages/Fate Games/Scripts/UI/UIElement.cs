using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace FateGames
{
    public abstract class UIElement : MonoBehaviour
    {
        public void Show()
        {
            print(this.GetType().Name + ".Show()");
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            print(this.GetType().Name + ".Hide()");
            gameObject.SetActive(false);
        }

        private void Update()
        {
            Animate();
        }

        protected abstract void Animate();
    }
}