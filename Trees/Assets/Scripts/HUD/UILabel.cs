using Scripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class UILabel : HUDComponent<Label>
    {
        public SourceCode<string> content = "";
        public SourceCode<Color> color = Color.white;
        public SourceCode<int> fontSize = 16;
        public TextAnchor alignment = TextAnchor.MiddleCenter;
        public FontStyle fontStyle = FontStyle.Normal;

        public override VisualElement CreateGUI()
        {
            Label label = new();
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.flexGrow = 1;
            label.style.whiteSpace = WhiteSpace.Normal;
            UpdateGUI(label);
            return label;
        }

        protected override void UpdateGUI(Label label)
        {
            base.UpdateGUI(label);
            label.text = content.Evaluate(Program.interpreter);
            label.style.color = color.Evaluate(Program.interpreter);
            label.style.fontSize = fontSize.Evaluate(Program.interpreter);
            label.style.unityTextAlign = alignment;
            label.style.unityFontStyleAndWeight = fontStyle;
        }
    }
}