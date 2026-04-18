using Scripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class UIBox : UIBehaviour
    {
        public SourceCode<Color> color = Color.white;

        public override VisualElement CreateGUI()
        {
            VisualElement box = new();
            UpdateGUI(box);
            return box;
        }

        protected override void UpdateGUI(VisualElement box)
        {
            base.UpdateGUI(box);
            box.style.backgroundColor = color.Evaluate(Program.interpreter);
        }
    }
}