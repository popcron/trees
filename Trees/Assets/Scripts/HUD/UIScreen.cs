using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    // todo: this isnt really needed for anything. maybe itll be useful later
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public sealed class UIScreen : MonoBehaviour
    {
        public static readonly List<UIScreen> all = new();

        private void OnEnable()
        {
            all.Add(this);
        }

        private void OnDisable()
        {
            all.Remove(this);
        }

        private void Update()
        {
            Bounds bounds = GetScreenBounds();
            RectTransform rt = (RectTransform)transform;
            rt.sizeDelta = new(bounds.size.x, bounds.size.y);
        }

        public Bounds GetScreenBounds()
        {
            Vector2 size = new(Screen.width, Screen.height);
            return new Bounds(transform.position, size);
        }
    }
}