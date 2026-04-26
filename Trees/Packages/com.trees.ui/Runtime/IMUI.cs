using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public static class IMUI
    {
        public static readonly VisualElement root;
        public static readonly LabelBucket labels = new();
        public static readonly BoxBucket boxes = new();
        public static readonly ToggleBucket toggles = new();
        public static readonly TextFieldBucket textFields = new();
        public static readonly ButtonBucket buttons = new();
        public static readonly SliderBucket sliders = new();
        public static readonly SliderIntBucket sliderInts = new();
        public static readonly ImageBucket images = new();

        static IMUI()
        {
            root = new VisualElement();
            root.pickingMode = PickingMode.Ignore;
            root.style.position = Position.Absolute;
            root.style.left = 0;
            root.style.top = 0;
            root.style.right = 0;
            root.style.bottom = 0;
            IMUIStyle.ApplyRootDefaults(root);
            UIEngine.panel.visualTree.Add(root);
        }

        public static void Label(Rect rect, string text, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(file, line);
            Label element = labels.Claim(id, root, out _);
            SetTransform(element, rect);
            element.text = text;
            UIRendering.repaint = true;
        }

        public static void Box(Rect rect, Color color, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(file, line);
            VisualElement element = boxes.Claim(id, root, out _);
            SetTransform(element, rect);
            element.style.backgroundColor = color;
            UIRendering.repaint = true;
        }

        public static void Image(Rect rect, Texture2D texture, Slice slice = default, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(file, line);
            VisualElement element = images.Claim(id, root, out _);
            SetTransform(element, rect);
            element.style.backgroundImage = Background.FromTexture2D(texture);
            Slice.Apply(element, slice);
            UIRendering.repaint = true;
        }

        public static void Image(Rect rect, Sprite sprite, Slice slice = default, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(file, line);
            VisualElement element = images.Claim(id, root, out _);
            SetTransform(element, rect);
            element.style.backgroundImage = Background.FromSprite(sprite);
            Slice.Apply(element, slice);
            UIRendering.repaint = true;
        }

        public static void Image(Rect rect, RenderTexture texture, Slice slice = default, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(file, line);
            VisualElement element = images.Claim(id, root, out _);
            SetTransform(element, rect);
            element.style.backgroundImage = Background.FromRenderTexture(texture);
            Slice.Apply(element, slice);
            UIRendering.repaint = true;
        }

        public static void Toggle(Rect rect, ref bool value, string text, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(file, line);
            Toggle element = toggles.Claim(id, root, out bool created);
            if (created)
            {
                element.SetValueWithoutNotify(value);
            }

            value = element.value;
            SetTransform(element, rect);
            element.text = text;
            UIRendering.repaint = true;
        }

        public static void TextField(Rect rect, ref string text, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(file, line);
            TextField element = textFields.Claim(id, root, out bool created);
            if (created)
            {
                element.SetValueWithoutNotify(text);
            }

            text = element.value;
            SetTransform(element, rect);
            UIRendering.repaint = true;
        }

        public static bool Button(Rect rect, string text, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(file, line);
            ImmediateButton element = buttons.Claim(id, root, out _);
            SetTransform(element, rect);
            element.text = text;
            bool result = element.wasClicked;
            element.wasClicked = false;
            UIRendering.repaint = true;
            return result;
        }

        public static void HorizontalSlider(Rect rect, ref float value, float min, float max, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            Slider(rect, ref value, min, max, SliderDirection.Horizontal, HashCode.Combine(file, line));
        }

        public static void VerticalSlider(Rect rect, ref float value, float min, float max, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            Slider(rect, ref value, min, max, SliderDirection.Vertical, HashCode.Combine(file, line));
        }

        public static void HorizontalSliderInt(Rect rect, ref int value, int min, int max, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            SliderInt(rect, ref value, min, max, SliderDirection.Horizontal, HashCode.Combine(file, line));
        }

        public static void VerticalSliderInt(Rect rect, ref int value, int min, int max, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            SliderInt(rect, ref value, min, max, SliderDirection.Vertical, HashCode.Combine(file, line));
        }

        public static void Slider(Rect rect, ref float value, float min, float max, SliderDirection direction, int id)
        {
            Slider element = sliders.Claim(id, root, out bool created);
            element.lowValue = min;
            element.highValue = max;
            if (created)
            {
                element.SetValueWithoutNotify(value);
            }

            element.direction = direction;
            value = element.value;
            SetTransform(element, rect);
            UIRendering.repaint = true;
        }

        public static void SliderInt(Rect rect, ref int value, int min, int max, SliderDirection direction, int id)
        {
            SliderInt element = sliderInts.Claim(id, root, out bool created);
            element.lowValue = min;
            element.highValue = max;
            if (created)
            {
                element.SetValueWithoutNotify(value);
            }
            element.direction = direction;
            value = element.value;
            SetTransform(element, rect);
            UIRendering.repaint = true;
        }

        public static void Label(Vector3 worldPosition, Vector2 size, string text, Camera camera = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(file, line);
            Label element = labels.Claim(id, root, out _);
            bool visible = Project(worldPosition, size, camera, out Rect rect);
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            SetTransform(element, rect);
            element.text = text;
            UIRendering.repaint = true;
        }

        public static void Box(Vector3 worldPosition, Vector2 size, Color color, Camera camera = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(file, line);
            VisualElement element = boxes.Claim(id, root, out _);
            bool visible = Project(worldPosition, size, camera, out Rect rect);
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            SetTransform(element, rect);
            element.style.backgroundColor = color;
            UIRendering.repaint = true;
        }

        public static void Image(Vector3 worldPosition, Vector2 size, Texture2D texture, Slice slice = default, Camera camera = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(file, line);
            VisualElement element = images.Claim(id, root, out _);
            bool visible = Project(worldPosition, size, camera, out Rect rect);
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            SetTransform(element, rect);
            element.style.backgroundImage = Background.FromTexture2D(texture);
            Slice.Apply(element, slice);
            UIRendering.repaint = true;
        }

        public static void Image(Vector3 worldPosition, Vector2 size, Sprite sprite, Slice slice = default, Camera camera = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(file, line);
            VisualElement element = images.Claim(id, root, out _);
            bool visible = Project(worldPosition, size, camera, out Rect rect);
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            SetTransform(element, rect);
            element.style.backgroundImage = Background.FromSprite(sprite);
            Slice.Apply(element, slice);
            UIRendering.repaint = true;
        }

        public static void Image(Vector3 worldPosition, Vector2 size, RenderTexture texture, Slice slice = default, Camera camera = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(file, line);
            VisualElement element = images.Claim(id, root, out _);
            bool visible = Project(worldPosition, size, camera, out Rect rect);
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            SetTransform(element, rect);
            element.style.backgroundImage = Background.FromRenderTexture(texture);
            Slice.Apply(element, slice);
            UIRendering.repaint = true;
        }

        public static void Toggle(Vector3 worldPosition, Vector2 size, ref bool value, string text, Camera camera = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(file, line);
            Toggle element = toggles.Claim(id, root, out bool created);
            if (created)
            {
                element.SetValueWithoutNotify(value);
            }

            value = element.value;
            bool visible = Project(worldPosition, size, camera, out Rect rect);
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            SetTransform(element, rect);
            element.text = text;
            UIRendering.repaint = true;
        }

        public static void TextField(Vector3 worldPosition, Vector2 size, ref string text, Camera camera = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(file, line);
            TextField element = textFields.Claim(id, root, out bool created);
            if (created)
            {
                element.SetValueWithoutNotify(text);
            }

            text = element.value;
            bool visible = Project(worldPosition, size, camera, out Rect rect);
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            SetTransform(element, rect);
            UIRendering.repaint = true;
        }

        public static bool Button(Vector3 worldPosition, Vector2 size, string text, Camera camera = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(file, line);
            ImmediateButton element = buttons.Claim(id, root, out _);
            bool visible = Project(worldPosition, size, camera, out Rect rect);
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            SetTransform(element, rect);
            element.text = text;
            bool result = visible && element.wasClicked;
            element.wasClicked = false;
            UIRendering.repaint = true;
            return result;
        }

        public static void HorizontalSlider(Vector3 worldPosition, Vector2 size, ref float value, float min, float max, Camera camera = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            SliderWorld(worldPosition, size, ref value, min, max, SliderDirection.Horizontal, camera, HashCode.Combine(file, line));
        }

        public static void VerticalSlider(Vector3 worldPosition, Vector2 size, ref float value, float min, float max, Camera camera = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            SliderWorld(worldPosition, size, ref value, min, max, SliderDirection.Vertical, camera, HashCode.Combine(file, line));
        }

        public static void HorizontalSliderInt(Vector3 worldPosition, Vector2 size, ref int value, int min, int max, Camera camera = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            SliderIntWorld(worldPosition, size, ref value, min, max, SliderDirection.Horizontal, camera, HashCode.Combine(file, line));
        }

        public static void VerticalSliderInt(Vector3 worldPosition, Vector2 size, ref int value, int min, int max, Camera camera = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            SliderIntWorld(worldPosition, size, ref value, min, max, SliderDirection.Vertical, camera, HashCode.Combine(file, line));
        }

        private static void SliderWorld(Vector3 worldPosition, Vector2 size, ref float value, float min, float max, SliderDirection direction, Camera camera, int id)
        {
            Slider element = sliders.Claim(id, root, out bool created);
            element.lowValue = min;
            element.highValue = max;
            if (created)
            {
                element.SetValueWithoutNotify(value);
            }

            element.direction = direction;
            value = element.value;
            bool visible = Project(worldPosition, size, camera, out Rect rect);
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            SetTransform(element, rect);
            UIRendering.repaint = true;
        }

        private static void SliderIntWorld(Vector3 worldPosition, Vector2 size, ref int value, int min, int max, SliderDirection direction, Camera camera, int id)
        {
            SliderInt element = sliderInts.Claim(id, root, out bool created);
            element.lowValue = min;
            element.highValue = max;
            if (created)
            {
                element.SetValueWithoutNotify(value);
            }

            element.direction = direction;
            value = element.value;
            bool visible = Project(worldPosition, size, camera, out Rect rect);
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            SetTransform(element, rect);
            UIRendering.repaint = true;
        }

        public static bool Project(Vector3 worldPosition, Vector2 size, Camera camera, out Rect rect)
        {
            Camera c = camera != null ? camera : Camera.main;
            Vector3 screen = c.WorldToScreenPoint(worldPosition);
            float x = screen.x - size.x * 0.5f;
            float y = (c.pixelHeight - screen.y) - size.y * 0.5f;
            rect = new(x, y, size.x, size.y);
            return screen.z > 0f;
        }

        public static void Update()
        {
            labels.Sweep();
            boxes.Sweep();
            toggles.Sweep();
            textFields.Sweep();
            buttons.Sweep();
            sliders.Sweep();
            sliderInts.Sweep();
            images.Sweep();
        }

        public static void SetTransform(VisualElement element, Rect rect)
        {
            element.style.position = Position.Absolute;
            element.style.left = rect.x;
            element.style.top = rect.y;
            element.style.width = rect.width;
            element.style.height = rect.height;
        }
    }

    public abstract class Bucket<T> where T : VisualElement
    {
        private readonly Stack<T> pool = new();
        private Dictionary<int, T> previous = new();
        private Dictionary<int, T> current = new();

        public T Claim(int id, VisualElement parent, out bool created)
        {
            T element;
            if (current.TryGetValue(id, out element))
            {
                created = false;
                if (element.parent != parent)
                {
                    parent.Add(element);
                }
                return element;
            }

            if (previous.Remove(id, out element))
            {
                created = false;
            }
            else if (pool.TryPop(out element))
            {
                created = true;
            }
            else
            {
                element = Create();
                created = true;
            }

            if (element.parent != parent)
            {
                parent.Add(element);
            }

            current.Add(id, element);
            return element;
        }

        public void Sweep()
        {
            foreach (T leftover in previous.Values)
            {
                leftover.RemoveFromHierarchy();
                pool.Push(leftover);
            }

            previous.Clear();
            (previous, current) = (current, previous);
        }

        protected abstract T Create();
    }

    public static class IMUIStyle
    {
        public const float LineHeight = 28f;

        public static void ApplyRootDefaults(VisualElement root)
        {
            root.style.color = Color.white;
            root.style.fontSize = 14;
            root.style.unityTextAlign = TextAnchor.UpperLeft;
        }

        public static void ZeroMargins(VisualElement element)
        {
            element.style.marginTop = 0;
            element.style.marginBottom = 0;
            element.style.marginLeft = 0;
            element.style.marginRight = 0;
        }

        public static void ZeroPadding(VisualElement element)
        {
            element.style.paddingTop = 0;
            element.style.paddingBottom = 0;
            element.style.paddingLeft = 0;
            element.style.paddingRight = 0;
        }

        public static void SetBorderWidth(VisualElement element, float width)
        {
            element.style.borderTopWidth = width;
            element.style.borderBottomWidth = width;
            element.style.borderLeftWidth = width;
            element.style.borderRightWidth = width;
        }

        public static void SetBorderColor(VisualElement element, Color color)
        {
            element.style.borderTopColor = color;
            element.style.borderBottomColor = color;
            element.style.borderLeftColor = color;
            element.style.borderRightColor = color;
        }
    }

    public class LabelBucket : Bucket<Label>
    {
        protected override Label Create()
        {
            Label element = new();
            element.pickingMode = PickingMode.Ignore;
            IMUIStyle.ZeroMargins(element);
            IMUIStyle.ZeroPadding(element);
            element.style.height = IMUIStyle.LineHeight;
            element.style.unityTextAlign = TextAnchor.MiddleLeft;
            return element;
        }
    }

    public class BoxBucket : Bucket<VisualElement>
    {
        protected override VisualElement Create()
        {
            VisualElement element = new();
            element.pickingMode = PickingMode.Ignore;
            return element;
        }
    }

    public class ImageBucket : Bucket<VisualElement>
    {
        protected override VisualElement Create()
        {
            VisualElement element = new();
            element.pickingMode = PickingMode.Ignore;
            return element;
        }
    }

    [Serializable]
    public struct Slice
    {
        public bool initialized;
        public float left;
        public float top;
        public float right;
        public float bottom;
        public float scale;
        public bool tile;

        public Slice(float left, float top, float right, float bottom, float scale = 1f, bool tile = false)
        {
            initialized = true;
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.scale = scale;
            this.tile = tile;
        }

        public Slice(float uniform, float scale = 1f, bool tile = false)
        {
            initialized = true;
            left = uniform;
            top = uniform;
            right = uniform;
            bottom = uniform;
            this.scale = scale;
            this.tile = tile;
        }

        public static void Apply(VisualElement element, Slice slice)
        {
            if (slice.initialized)
            {
                element.style.unitySliceLeft = (int)slice.left;
                element.style.unitySliceTop = (int)slice.top;
                element.style.unitySliceRight = (int)slice.right;
                element.style.unitySliceBottom = (int)slice.bottom;
                element.style.unitySliceScale = slice.scale;
                element.style.unitySliceType = slice.tile ? SliceType.Tiled : SliceType.Sliced;
            }
            else
            {
                element.style.unitySliceLeft = StyleKeyword.Null;
                element.style.unitySliceTop = StyleKeyword.Null;
                element.style.unitySliceRight = StyleKeyword.Null;
                element.style.unitySliceBottom = StyleKeyword.Null;
                element.style.unitySliceScale = StyleKeyword.Null;
                element.style.unitySliceType = StyleKeyword.Null;
            }
        }
    }

    public class ToggleBucket : Bucket<Toggle>
    {
        protected override Toggle Create()
        {
            Toggle element = new();
            IMUIStyle.ZeroMargins(element);
            return element;
        }
    }

    public class ImmediateButton : Button
    {
        public bool wasClicked;

        public ImmediateButton()
        {
            clicked += () => wasClicked = true;
        }
    }

    public class ButtonBucket : Bucket<ImmediateButton>
    {
        protected override ImmediateButton Create()
        {
            ImmediateButton element = new();
            IMUIStyle.ZeroMargins(element);
            element.style.color = Color.white;
            element.style.backgroundColor = new Color(0.27f, 0.27f, 0.27f);
            IMUIStyle.SetBorderWidth(element, 1);
            IMUIStyle.SetBorderColor(element, new Color(0.15f, 0.15f, 0.15f));
            return element;
        }
    }

    public class SliderBucket : Bucket<Slider>
    {
        protected override Slider Create()
        {
            Slider element = new();
            IMUIStyle.ZeroMargins(element);
            return element;
        }
    }

    public class SliderIntBucket : Bucket<SliderInt>
    {
        protected override SliderInt Create()
        {
            SliderInt element = new();
            IMUIStyle.ZeroMargins(element);
            return element;
        }
    }

    public class TextFieldBucket : Bucket<TextField>
    {
        protected override TextField Create()
        {
            TextField element = new();
            IMUIStyle.ZeroMargins(element);

            VisualElement input = element.Q(TextField.textInputUssName);
            input.style.paddingTop = 0;
            input.style.paddingBottom = 0;
            input.style.paddingLeft = 2;
            input.style.paddingRight = 2;
            IMUIStyle.SetBorderWidth(input, 0);
            input.style.color = Color.black;
            input.style.backgroundColor = Color.white;
            return element;
        }
    }
}
