using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FateGames
{
    public class MouseCursor : MonoBehaviour
    {
        private Camera cam;
        private RectTransform rect;
        [SerializeField] private GameObject image;

        private void Awake()
        {
            cam = Camera.main;
            rect = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (GameManager.Instance.cursorType == GameManager.CursorType.HAND)
            {
                Vector2 mousePosition = Input.mousePosition;
                Vector2 anchor = new Vector2(mousePosition.x / Screen.width, mousePosition.y / Screen.height);
                rect.anchorMin = anchor;
                rect.anchorMax = anchor;
                rect.anchoredPosition = Vector2.zero;
                rect.SetAsLastSibling();
            }
        }

        public void Show()
        {
            image.SetActive(true);
        }

        public void Hide()
        {
            image.SetActive(false);
        }
    }

}
