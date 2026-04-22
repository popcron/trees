using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class SceneSwitcher : EditorWindow
{
    private readonly List<SceneAsset> scenes = new();
    private ListView list;

    [MenuItem("Window/Scene Switcher")]
    public static void Open()
    {
        SceneSwitcher window = GetWindow<SceneSwitcher>();
        window.titleContent = new GUIContent("Scene Switcher", EditorGUIUtility.IconContent("SceneAsset Icon").image);
        window.minSize = new Vector2(240f, 100f);
    }

    private void OnEnable()
    {
        list = CreateList();
        EditorApplication.projectChanged += Refresh;
    }

    private ListView CreateList()
    {
        ListView list = new(scenes, 24, () => new SceneEntry(), (element, index) =>
        {
            SceneEntry sceneEntry = ((SceneEntry)element);
            sceneEntry.field.value = scenes[index];
        });

        list.selectionType = SelectionType.None;
        list.style.flexGrow = 1;
        return list;
    }

    private void OnDisable()
    {
        EditorApplication.projectChanged -= Refresh;
    }

    private void CreateGUI()
    {
        rootVisualElement.Add(list);
        Refresh();
    }

    private void Refresh()
    {
        scenes.Clear();
        GUID[] guids = AssetDatabase.FindAssetGUIDs("t:Scene", new[] { "Assets" });
        foreach (GUID guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SceneAsset asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            scenes.Add(asset);
        }

        list.Rebuild();
    }

    private class SceneEntry : VisualElement
    {
        public readonly ObjectField field;

        public SceneEntry()
        {
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.paddingLeft = 4;
            style.paddingRight = 4;

            field = new ObjectField { objectType = typeof(SceneAsset), allowSceneObjects = false };
            field.style.flexGrow = 1;

            Add(field);
        }
    }
}
