using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UIElements;

public class Notification : MonoBehaviour
{
    public string message = "Hello there";
    public Sprite cloudSprite;
    public Sprite arrowSprite;
    public Vector3 anchor;
    public Vector2 boxSize = new Vector2(200, 50);
    public Vector2 arrowSize = new Vector2(24, 24);
    public float cloudDensity = 0.05f;
    public float minSize = 0.6f;
    public float maxSize = 1.4f;
    public float sizeThreshold = 30f;
    public float animationInterval = 0.333f;
    public Color textColor = Color.black;
    public FontStyle fontStyle = FontStyle.Bold;
    public Font font;
    public float fontSize = 14f;
    public VisualElement visualElement;
    public VisualElement box;
    public VisualElement clouds;
    public Image arrow;
    public Label label;

    private Vector2 lastBuiltSize;
    private float animationTimer;
    private readonly List<Image> cloudImages = new();

    private void OnEnable()
    {
        visualElement = new();
        visualElement.style.position = Position.Absolute;
        visualElement.style.alignItems = Align.Center;
        visualElement.style.justifyContent = Justify.Center;
        visualElement.pickingMode = PickingMode.Ignore;

        box = new();
        box.style.backgroundColor = new Color(0f, 0f, 0f, 0.75f);
        box.style.borderTopLeftRadius = 4;
        box.style.borderTopRightRadius = 4;
        box.style.borderBottomLeftRadius = 4;
        box.style.borderBottomRightRadius = 4;
        box.style.paddingLeft = 8;
        box.style.paddingRight = 8;
        box.style.paddingTop = 4;
        box.style.paddingBottom = 4;
        box.style.alignItems = Align.Center;
        box.style.justifyContent = Justify.Center;
        visualElement.Add(box);

        clouds = new();
        clouds.style.position = Position.Absolute;
        clouds.style.left = Length.Percent(50);
        clouds.style.top = Length.Percent(50);
        clouds.style.translate = new Translate(Length.Percent(-50), Length.Percent(-50));
        clouds.pickingMode = PickingMode.Ignore;
        box.Add(clouds);

        label = new();
        box.Add(label);
        label.BringToFront();

        arrow = new();
        arrow.sprite = arrowSprite;
        arrow.scaleMode = ScaleMode.StretchToFill;
        arrow.style.position = Position.Absolute;
        arrow.pickingMode = PickingMode.Ignore;
        IMUI.root.Add(arrow);

        BuildClouds();
        UpdateGUI();
        IMUI.root.Add(visualElement);
    }

    private void OnDisable()
    {
        IMUI.root.Remove(visualElement);
        IMUI.root.Remove(arrow);
    }

    private void Update()
    {
        if (Mathf.Abs(boxSize.x - lastBuiltSize.x) >= sizeThreshold || Mathf.Abs(boxSize.y - lastBuiltSize.y) >= sizeThreshold)
        {
            BuildClouds();
        }

        animationTimer += Time.deltaTime;
        if (animationTimer >= animationInterval)
        {
            animationTimer = 0f;
            LayoutClouds();
        }

        UpdateGUI();
    }

    private void OnValidate()
    {
        if (clouds != null && Application.isPlaying)
        {
            BuildClouds();
            UpdateGUI();
        }
    }

    public void BuildClouds()
    {
        clouds.Clear();
        cloudImages.Clear();
        clouds.style.width = boxSize.x;
        clouds.style.height = boxSize.y;
        float step = 1f / cloudDensity;
        for (float y = 0; y < boxSize.y; y += step)
        {
            for (float x = 0; x < boxSize.x; x += step)
            {
                Image cloud = new();
                cloud.sprite = cloudSprite;
                cloud.style.position = Position.Absolute;
                cloud.pickingMode = PickingMode.Ignore;
                clouds.Add(cloud);
                cloudImages.Add(cloud);
            }
        }

        lastBuiltSize = boxSize;
        LayoutClouds();
    }

    public void LayoutClouds()
    {
        clouds.style.width = boxSize.x;
        clouds.style.height = boxSize.y;
        float step = 1f / cloudDensity;
        int index = 0;
        for (float y = 0; y < boxSize.y; y += step)
        {
            for (float x = 0; x < boxSize.x; x += step)
            {
                if (index >= cloudImages.Count)
                {
                    return;
                }

                float jitter = step * 0.5f;
                float jitterX = Random.Range(-jitter, jitter);
                float jitterY = Random.Range(-jitter, jitter);
                float scale = Random.Range(minSize, maxSize);
                float angle = Random.Range(-25f, 25f);
                float size = step * scale;
                Image cloud = cloudImages[index++];
                cloud.style.left = x + jitterX - size * 0.5f;
                cloud.style.top = y + jitterY - size * 0.5f;
                cloud.style.width = size;
                cloud.style.height = size;
                cloud.style.rotate = new Rotate(new Angle(angle, AngleUnit.Degree));
            }
        }
    }

    public void UpdateGUI()
    {
        Camera cam = Camera.main;
        Vector3 rootPosition = transform.position + anchor;
        bool inFront = IMUI.Project(rootPosition, transform.localScale, cam, out Rect rect);
        if (!inFront)
        {
            visualElement.style.display = DisplayStyle.None;
            arrow.style.display = DisplayStyle.None;
            return;
        }

        visualElement.style.display = DisplayStyle.Flex;
        visualElement.style.left = rect.x;
        visualElement.style.top = rect.y;
        visualElement.style.width = rect.width;
        visualElement.style.height = rect.height;
        if (anchor.sqrMagnitude > 0.25f)
        {
            arrow.style.display = DisplayStyle.Flex;
            Vector3 sourceScreen = cam.WorldToScreenPoint(transform.position);
            Vector2 sourceIMUI = new Vector2(sourceScreen.x, cam.pixelHeight - sourceScreen.y);
            Vector2 boxCenter = new Vector2(rect.x + rect.width * 0.5f, rect.y + rect.height * 0.5f);
            Vector2 mid = (sourceIMUI + boxCenter) * 0.5f;
            Vector2 toSource = boxCenter - sourceIMUI;
            float arrowHeight = toSource.magnitude;
            float arrowAngle = Mathf.Atan2(toSource.x, -toSource.y) * Mathf.Rad2Deg;
            arrow.style.left = mid.x - arrowSize.x * 0.5f;
            arrow.style.top = mid.y - arrowHeight * 0.5f;
            arrow.style.width = arrowSize.x;
            arrow.style.height = arrowHeight;
            arrow.style.rotate = new Rotate(new Angle(arrowAngle, AngleUnit.Degree));
        }
        else
        {
            arrow.style.display = DisplayStyle.None;
        }

        label.text = message;
        label.style.color = textColor;
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.unityFontStyleAndWeight = fontStyle;
        label.style.fontSize = fontSize;
        if (font != null)
        {
            label.style.unityFontDefinition = new StyleFontDefinition(font);
        }
    }
}
