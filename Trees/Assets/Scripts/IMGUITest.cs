using UI;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteAlways]
public class IMGUITest : MonoBehaviour
{
    public bool toggleValue;
    public string stringValue;
    public float volume = 0.5f;
    public float pitch = 1.0f;
    public int count = 5;
    public int level = 10;
    public Label label;

    private void OnEnable()
    {
        label = new();
        label.text = "Test";
        label.style.position = Position.Absolute;
        label.style.left = 5;
        label.style.top = 5;
        label.style.width = 100;
        label.style.height = 20;
        IMUI.root.Add(label);
    }

    private void OnDisable()
    {
        IMUI.root.Remove(label);
    }

    private void Update()
    {
        Rect rect = new(transform.position.x, transform.position.y, 200, 30);
        Rect original = rect;
        IMUI.Label(rect, "Hello there");
        rect.y += 30;
        IMUI.Box(rect, Color.red);
        rect.y += 30;
        IMUI.Toggle(rect, ref toggleValue, "Toggle");
        rect.y += 30;
        IMUI.TextField(rect, ref stringValue);
        rect.y += 30;
        if (IMUI.Button(rect, "Press Me!"))
        {
            Debug.Log("Button clicked!");
        }

        rect.y += 30;
        IMUI.HorizontalSlider(rect, ref volume, 0f, 1f);
        rect.y += 30;
        IMUI.HorizontalSliderInt(rect, ref count, 0, 10);
        rect.x += 210 + volume * (count * 10);
        rect.y = original.y;
        rect.height = 200;
        rect.width = 30;
        IMUI.VerticalSlider(rect, ref pitch, -1f, 1f);
        rect.x += 30;
        IMUI.VerticalSliderInt(rect, ref level, 1, 99);
    }
}