using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using System.Linq;

public class AutoSetSprite : EditorWindow
{
    [MenuItem("Tiwa's Portal World Kit/AutoSetSprite")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow<AutoSetSprite>();
    }

    string path = "Assets/Tiwa'sPortalWorldKit/thumbnails";
    GameObject select;
    string[] extentions = new string[] { ".png", ".jpg"};
    void OnGUI()
    {
        EditorGUILayout.LabelField("注意");
        EditorGUILayout.LabelField("ImageBankをPrefab化していると自動セットはできません");
        EditorGUILayout.LabelField("サムネイルに使用する画像ファイルはImport設定からTextureTypeを「Sprite(2D and UI)」に設定してください");
        EditorGUILayout.LabelField("path:");
        path = EditorGUILayout.TextField(path);
        select = EditorGUILayout.ObjectField("ImageBank:", select, typeof(GameObject), true) as GameObject;

        List<Sprite> spriteList = new List<Sprite>();   
        if (GUILayout.Button("自動セット"))
        {
            string[] filePathArray = Directory.GetFiles(path, "*.*")
                                    .Where(c => extentions.Any(ex => c.EndsWith(ex)))
                                    .ToArray();

            int childCount = select.transform.childCount;
            GameObject[] oldChild = new GameObject[childCount];
            for(int i = 0; i < childCount; i++)
            {
                oldChild[i] = select.transform.GetChild(i).gameObject;
            }

            for (int i = 0; i < childCount; i++)
            {
                DestroyImmediate(oldChild[i]);
            }

            foreach (var f in filePathArray)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(f);

                if (sprite != null)
                {
                    spriteList.Add(sprite);   
                }
            }

            for(int i = 0; i < spriteList.Count; i++)
            {
                var obj = new GameObject();
                obj.name = spriteList[i].name;
                obj.transform.parent = select.transform;
                var sp = obj.AddComponent<Image>();
                sp.sprite = spriteList[i];
            }
        }
    }
}
