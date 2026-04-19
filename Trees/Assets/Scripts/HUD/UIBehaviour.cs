using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public abstract class UIBehaviour : MonoBehaviour
    {
        public static readonly Vector3[] corners = new Vector3[4];

        public bool alwaysUpdate;

        public RectTransform rectTransform => (RectTransform)transform;

        protected virtual void OnEnable()
        {
            VisualElement root = CreateGUI();
            UIEngine.thePanel.visualTree.Add(root);
            UIEngine.map.Add(this, root);
            UIEngine.all.Add(this);
            UIEngine.repaint = true;
        }

        protected virtual void OnDisable()
        {
            UIEngine.all.Remove(this);
            UIEngine.map.Remove(this, out VisualElement root);
            if (UIEngine.thePanel.visualTree.Contains(root))
            {
                UIEngine.thePanel.visualTree.Remove(root);
                UIEngine.repaint = false;
            }
        }

        protected virtual void OnValidate()
        {
            if (UIEngine.map.TryGetValue(this, out VisualElement root))
            {
                UpdateGUI(root);
                UIEngine.repaint = true;
            }
        }

        private void LateUpdate()
        {
            if (UIEngine.map.TryGetValue(this, out VisualElement root))
            {
                if (alwaysUpdate)
                {
                    UpdateGUI(root);
                }
                else
                {
                    UpdateTransform(root);
                }

                UIEngine.repaint = true;
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            if (UIEngine.map.TryGetValue(this, out VisualElement root))
            {
                UpdateTransform(root);
                UIEngine.repaint = true;
            }
        }

        private void OnDrawGizmos()
        {
            Bounds bounds = GetBounds();
            Color originalColor = Gizmos.color;

            // draw outline
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            Gizmos.color *= Color.clear;
            Gizmos.DrawCube(bounds.center, bounds.size);
            Gizmos.color = originalColor;
        }

        public Bounds GetBounds()
        {
            rectTransform.GetWorldCorners(corners);
            Vector3 min = corners[0];
            Vector3 max = corners[0];
            min = Vector3.Min(min, corners[1]);
            max = Vector3.Max(max, corners[1]);
            min = Vector3.Min(min, corners[2]);
            max = Vector3.Max(max, corners[2]);
            min = Vector3.Min(min, corners[3]);
            max = Vector3.Max(max, corners[3]);
            Vector3 center = (min + max) * 0.5f;
            Vector3 size = max - min;
            center.z = transform.position.z;
            size.z = 0f;
            return new(center, size);
        }

        public Rect GetRect()
        {
            Bounds bounds = GetBounds();
            Vector2 size = bounds.size;
            Vector2 offset = new(UIEngine.panelResolution.x * 0.5f, UIEngine.panelResolution.y * 0.5f);
            float x = bounds.min.x + offset.x;
            float y = offset.y - bounds.max.y;
            return new(new Vector2(x, y), size);
        }

        public virtual VisualElement CreateGUI()
        {
            return new();
        }

        protected virtual void UpdateGUI(VisualElement root)
        {
            UpdateTransform(root);
        }

        public void UpdateTransform(VisualElement root)
        {
            Rect rect = GetRect();
            root.style.position = Position.Absolute;
            root.style.left = rect.x;
            root.style.top = rect.y;
            root.style.width = rect.width;
            root.style.height = rect.height;
        }
    }

    public abstract class HUDComponent<T> : UIBehaviour where T : VisualElement
    {
        protected sealed override void UpdateGUI(VisualElement root)
        {
            UpdateTransform(root);
            UpdateGUI((T)root);
        }

        protected virtual void UpdateGUI(T root)
        {
        }
    }
}