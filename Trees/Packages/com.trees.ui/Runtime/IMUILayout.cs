using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public static class IMUILayout
    {
        public static readonly VisualElement root;
        public static readonly LabelBucket labels = new();
        public static readonly BoxBucket boxes = new();
        public static readonly ToggleBucket toggles = new();
        public static readonly TextFieldBucket textFields = new();
        public static readonly ButtonBucket buttons = new();
        public static readonly SliderBucket sliders = new();
        public static readonly SliderIntBucket sliderInts = new();
        public static readonly ContainerBucket containers = new();
        public static readonly ImageBucket images = new();

        private static readonly Stack<VisualElement> stack = new();
        private static readonly Stack<int> scopeIds = new();

        static IMUILayout()
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

        private static VisualElement Parent => stack.Count > 0 ? stack.Peek() : root;
        private static int CurrentScope => scopeIds.Count > 0 ? scopeIds.Peek() : 0;

        public readonly struct Scope : IDisposable
        {
            public void Dispose()
            {
                stack.Pop();
                scopeIds.Pop();
            }
        }

        public static Scope BeginVertical(float width = float.NaN, float height = float.NaN, int key = 0, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(CurrentScope, file, line, key, "V");
            VisualElement container = containers.Claim(id, Parent, out _);
            ResetContainer(container);
            container.style.flexDirection = FlexDirection.Column;
            ApplySize(container, width, height);
            stack.Push(container);
            scopeIds.Push(id);
            return default;
        }

        public static Scope BeginHorizontal(float width = float.NaN, float height = float.NaN, int key = 0, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(CurrentScope, file, line, key, "H");
            VisualElement container = containers.Claim(id, Parent, out _);
            ResetContainer(container);
            container.style.flexDirection = FlexDirection.Row;
            ApplySize(container, width, height);
            stack.Push(container);
            scopeIds.Push(id);
            return default;
        }

        public static Scope BeginArea(Rect rect, int key = 0, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(CurrentScope, file, line, key, "A");
            VisualElement container = containers.Claim(id, Parent, out _);
            ResetContainer(container);
            container.style.flexDirection = FlexDirection.Column;
            container.style.position = Position.Absolute;
            container.style.left = rect.x;
            container.style.top = rect.y;
            container.style.width = rect.width;
            container.style.height = rect.height;
            stack.Push(container);
            scopeIds.Push(id);
            return default;
        }

        public static void Space(float pixels, int key = 0, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            VisualElement parent = Parent;
            bool row = parent.resolvedStyle.flexDirection == FlexDirection.Row || parent.resolvedStyle.flexDirection == FlexDirection.RowReverse;
            int id = HashCode.Combine(CurrentScope, file, line, key, "space");
            VisualElement spacer = containers.Claim(id, parent, out _);
            ResetContainer(spacer);
            if (row)
            {
                spacer.style.width = pixels;
            }
            else
            {
                spacer.style.height = pixels;
            }
        }

        public static void FlexibleSpace(int key = 0, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            VisualElement parent = Parent;
            int id = HashCode.Combine(CurrentScope, file, line, key, "flex");
            VisualElement spacer = containers.Claim(id, parent, out _);
            ResetContainer(spacer);
            spacer.style.flexGrow = 1;
        }

        public static void Label(string text, float width = float.NaN, float height = float.NaN, int key = 0, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(CurrentScope, file, line, key);
            Label element = labels.Claim(id, Parent, out _);
            ClearTransform(element);
            ApplySize(element, width, float.IsNaN(height) ? IMUIStyle.LineHeight : height);
            element.text = text;
            UIRendering.repaint = true;
        }

        public static void Box(Color color, float width = float.NaN, float height = float.NaN, int key = 0, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(CurrentScope, file, line, key);
            VisualElement element = boxes.Claim(id, Parent, out _);
            ClearTransform(element);
            ApplySize(element, width, height);
            element.style.backgroundColor = color;
            UIRendering.repaint = true;
        }

        public static void Image(Texture2D texture, Slice slice = default, float width = float.NaN, float height = float.NaN, int key = 0, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            if (float.IsNaN(width))
            {
                width = texture.width;
            }

            if (float.IsNaN(height))
            {
                height = texture.height;
            }

            int id = HashCode.Combine(CurrentScope, file, line, key);
            VisualElement element = images.Claim(id, Parent, out _);
            ClearTransform(element);
            ApplySize(element, width, height);
            element.style.backgroundImage = Background.FromTexture2D(texture);
            Slice.Apply(element, slice);
            UIRendering.repaint = true;
        }

        public static void Image(Sprite sprite, Slice slice = default, float width = float.NaN, float height = float.NaN, int key = 0, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            if (float.IsNaN(width))
            {
                width = sprite.rect.width;
            }

            if (float.IsNaN(height))
            {
                height = sprite.rect.height;
            }

            int id = HashCode.Combine(CurrentScope, file, line, key);
            VisualElement element = images.Claim(id, Parent, out _);
            ClearTransform(element);
            ApplySize(element, width, height);
            element.style.backgroundImage = Background.FromSprite(sprite);
            Slice.Apply(element, slice);
            UIRendering.repaint = true;
        }

        public static void Image(RenderTexture texture, Slice slice = default, float width = float.NaN, float height = float.NaN, int key = 0, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            if (float.IsNaN(width))
            {
                width = texture.width;
            }

            if (float.IsNaN(height))
            {
                height = texture.height;
            }

            int id = HashCode.Combine(CurrentScope, file, line, key);
            VisualElement element = images.Claim(id, Parent, out _);
            ClearTransform(element);
            ApplySize(element, width, height);
            element.style.backgroundImage = Background.FromRenderTexture(texture);
            Slice.Apply(element, slice);
            UIRendering.repaint = true;
        }

        public static void Toggle(ref bool value, string text, float width = float.NaN, float height = float.NaN, int key = 0, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(CurrentScope, file, line, key);
            Toggle element = toggles.Claim(id, Parent, out bool created);
            if (created)
            {
                element.SetValueWithoutNotify(value);
            }

            value = element.value;
            ClearTransform(element);
            ApplySize(element, width, height);
            element.text = text;
            UIRendering.repaint = true;
        }

        public static void TextField(ref string text, float width = float.NaN, float height = float.NaN, int key = 0, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(CurrentScope, file, line, key);
            TextField element = textFields.Claim(id, Parent, out bool created);
            if (created)
            {
                element.SetValueWithoutNotify(text);
            }

            text = element.value;
            ClearTransform(element);
            ApplySize(element, width, height);
            UIRendering.repaint = true;
        }

        public static bool Button(string text, float width = float.NaN, float height = float.NaN, int key = 0, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            int id = HashCode.Combine(CurrentScope, file, line, key);
            ImmediateButton element = buttons.Claim(id, Parent, out _);
            ClearTransform(element);
            ApplySize(element, width, height);
            element.text = text;
            bool result = element.wasClicked;
            element.wasClicked = false;
            UIRendering.repaint = true;
            return result;
        }

        public static void HorizontalSlider(ref float value, float min, float max, float width = float.NaN, float height = float.NaN, int key = 0, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            Slider(ref value, min, max, SliderDirection.Horizontal, width, height, HashCode.Combine(CurrentScope, file, line, key));
        }

        public static void VerticalSlider(ref float value, float min, float max, float width = float.NaN, float height = float.NaN, int key = 0, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            Slider(ref value, min, max, SliderDirection.Vertical, width, height, HashCode.Combine(CurrentScope, file, line, key));
        }

        public static void HorizontalSliderInt(ref int value, int min, int max, float width = float.NaN, float height = float.NaN, int key = 0, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            SliderInt(ref value, min, max, SliderDirection.Horizontal, width, height, HashCode.Combine(CurrentScope, file, line, key));
        }

        public static void VerticalSliderInt(ref int value, int min, int max, float width = float.NaN, float height = float.NaN, int key = 0, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0)
        {
            SliderInt(ref value, min, max, SliderDirection.Vertical, width, height, HashCode.Combine(CurrentScope, file, line, key));
        }

        public static void Slider(ref float value, float min, float max, SliderDirection direction, float width, float height, int id)
        {
            Slider element = sliders.Claim(id, Parent, out bool created);
            element.lowValue = min;
            element.highValue = max;
            if (created)
            {
                element.SetValueWithoutNotify(value);
            }

            element.direction = direction;
            value = element.value;
            ClearTransform(element);
            ApplySize(element, width, height);
            UIRendering.repaint = true;
        }

        public static void SliderInt(ref int value, int min, int max, SliderDirection direction, float width, float height, int id)
        {
            SliderInt element = sliderInts.Claim(id, Parent, out bool created);
            element.lowValue = min;
            element.highValue = max;
            if (created)
            {
                element.SetValueWithoutNotify(value);
            }
            element.direction = direction;
            value = element.value;
            ClearTransform(element);
            ApplySize(element, width, height);
            UIRendering.repaint = true;
        }

        public static void Update()
        {
            stack.Clear();
            scopeIds.Clear();
            labels.Sweep();
            boxes.Sweep();
            toggles.Sweep();
            textFields.Sweep();
            buttons.Sweep();
            sliders.Sweep();
            sliderInts.Sweep();
            containers.Sweep();
            images.Sweep();
        }

        private static void ResetContainer(VisualElement container)
        {
            container.style.position = Position.Relative;
            container.style.left = StyleKeyword.Null;
            container.style.top = StyleKeyword.Null;
            container.style.right = StyleKeyword.Null;
            container.style.bottom = StyleKeyword.Null;
            container.style.width = StyleKeyword.Null;
            container.style.height = StyleKeyword.Null;
            container.style.flexGrow = StyleKeyword.Null;
            container.style.flexDirection = StyleKeyword.Null;
        }

        private static void ClearTransform(VisualElement element)
        {
            element.style.position = Position.Relative;
            element.style.left = StyleKeyword.Null;
            element.style.top = StyleKeyword.Null;
            element.style.width = StyleKeyword.Null;
            element.style.height = StyleKeyword.Null;
        }

        private static void ApplySize(VisualElement element, float width, float height)
        {
            if (!float.IsNaN(width))
            {
                element.style.width = width;
            }

            if (!float.IsNaN(height))
            {
                element.style.height = height;
            }
        }
    }

    public class ContainerBucket : Bucket<VisualElement>
    {
        protected override VisualElement Create()
        {
            VisualElement element = new();
            element.pickingMode = PickingMode.Ignore;
            return element;
        }
    }
}
