using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

public class SceneSwitcher : EditorWindow
{
    [MenuItem("Window/Scene Switcher")]
    public static void Open()
    {
        SceneSwitcher window = GetWindow<SceneSwitcher>();
        window.titleContent = new GUIContent("Scene Switcher", EditorGUIUtility.IconContent("SceneAsset Icon").image);
        window.minSize = new Vector2(200f, 100f);
    }

    private void CreateGUI()
    {
        GUID[] guids = AssetDatabase.FindAssetGUIDs("t:Scene", new[] { "Assets" });
        List<string> paths = new();
        foreach (GUID guid in guids)
        {
            paths.Add(AssetDatabase.GUIDToAssetPath(guid));
        }

        Texture2D sceneIcon = EditorGUIUtility.IconContent("SceneAsset Icon").image as Texture2D;
        ListView list = new(paths, 22, MakeItem, (element, index) =>
        {
            element.Q<Image>("icon").image = sceneIcon;
            element.Q<Label>("label").text = Path.GetFileNameWithoutExtension(paths[index]);
        });

        list.selectionType = SelectionType.Single;
        list.style.flexGrow = 1;
        list.itemsChosen += items =>
        {
            foreach (object item in items)
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene((string)item);
                }
            }
        };

        rootVisualElement.Add(list);
    }

    private static VisualElement MakeItem()
    {
        VisualElement row = new();
        row.style.flexDirection = FlexDirection.Row;
        row.style.alignItems = Align.Center;
        row.style.paddingLeft = 4;

        Image icon = new();
        icon.name = "icon";
        icon.style.width = 16;
        icon.style.height = 16;
        icon.style.marginRight = 4;

        Label label = new();
        label.name = "label";

        row.Add(icon);
        row.Add(label);
        return row;
    }
}
