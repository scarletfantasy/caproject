using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Rendering;
[CustomEditor(typeof(WindGen))]
public class MyCustomEditor : Editor
{
	
	private WindGen script;
	private void OnEnable()
	{
		// Method 1
		script = (WindGen)target;

		
	}

	public override void OnInspectorGUI()
	{

		serializedObject.Update();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("size"));
		serializedObject.ApplyModifiedProperties();
		if (GUILayout.Button("gen"))
		{
			Vector3Int size = script.size;
			var colors = new Color[size.x*size.y*size.z];
			var tex3d = new Texture3D(size.x, size.y, size.z, TextureFormat.RGBA32, false);
			//var tex3d = new RenderTexture(size.x, size.y, size.z, RenderTextureFormat.ARGB32);
			
			for (var n=0;n<colors.Length;++n)
            {
				colors[n] = new Color(1.0f, 1.0f, 1.0f);
            }
			tex3d.SetPixels(colors);
			tex3d.Apply();
			
			AssetDatabase.CreateAsset(tex3d, "Assets/wind.asset");
		}
		if (GUILayout.Button("genrt"))
		{
			Vector3Int size = script.size;
			var colors = new Color[size.x * size.y * size.z];
			//var tex3d = new Texture3D(size.x, size.y, size.z, TextureFormat.RGBA32, false);
			var tex3d = new RenderTexture(size.x, size.y, size.z, RenderTextureFormat.ARGB32);
			tex3d.dimension = TextureDimension.Tex3D;

			for (var n = 0; n < colors.Length; ++n)
			{
				colors[n] = new Color(1.0f, 1.0f, 1.0f);
			}

			AssetDatabase.CreateAsset(tex3d, "Assets/windrt.asset");
		}
	}
}
