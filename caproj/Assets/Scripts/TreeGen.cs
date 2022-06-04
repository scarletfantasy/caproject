using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TreeGen : MonoBehaviour
{
    // Start is called before the first frame update
    public int n;
    public int off;
    public GameObject tree;
    void Start()
    {
        var obj = AssetDatabase.LoadAssetAtPath("Assets/Resources/my.fbx", typeof(GameObject));
        Debug.Log(obj);
        Instantiate(obj);
        for (int i=0;i<n;++i)
        {
            for(int j=0;j<n;++j)
            {
                Instantiate(obj, new Vector3(i * off,  0, j * off), Quaternion.identity);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
