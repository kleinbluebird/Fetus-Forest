using UnityEngine;
using UnityEditor;
using System.IO;

public class Texture2DArrayCreator : MonoBehaviour
{
    [MenuItem("Assets/Create/Texture2DArray From Selected")]
    static void CreateTexture2DArray()
    {
        var textures = Selection.GetFiltered<Texture2D>(SelectionMode.DeepAssets);
        if (textures.Length == 0) return;

        int width = textures[0].width;
        int height = textures[0].height;
        TextureFormat format = textures[0].format;

        Texture2DArray textureArray = new Texture2DArray(width, height, textures.Length, format, false, false);
        textureArray.wrapMode = TextureWrapMode.Clamp;
        textureArray.filterMode = FilterMode.Bilinear;

        for (int i = 0; i < textures.Length; i++)
        {
            Graphics.CopyTexture(textures[i], 0, 0, textureArray, i, 0);
        }

        string path = "Assets/Texture2DArray.asset";
        AssetDatabase.CreateAsset(textureArray, path);
        AssetDatabase.SaveAssets();
        Debug.Log("Texture2DArray created at: " + path);
    }
}
