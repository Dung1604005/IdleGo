using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    Dictionary<Type, UICanvas> canvasActives = new Dictionary<Type, UICanvas>();

    Dictionary<Type, UICanvas> canvasPrefabs = new Dictionary<Type, UICanvas>();
    [SerializeField] Transform parent;

    private void Awake()
    {
        // Load UI prefab from resources
        UICanvas[] prefabs = Resources.LoadAll<UICanvas>("UI/");

        
        for(int i = 0; i < prefabs.Length; i++)
        {
            canvasPrefabs.Add(prefabs[i].GetType(), prefabs[i] );
        }
    }
    public T OpenUI<T>()where T: UICanvas
    {
        
        T canvas = GetUI<T>();

        canvas.SetUp();
        canvas.Open();

        return canvas;
    }

    public T OpenUI<T>(UICanvas ui) where T: UICanvas
    {
         T canvas = GetUI<T>();

        canvas.SetUp();
        canvas.Open(ui);
        

        return canvas;
    }

    public void CloseUI<T>(float time) where T: UICanvas
    {
        if (IsOpened<T>())
        {
            canvasActives[typeof(T)].Close(time);
        }
    }
    

    public void CloseUIDirectly<T>() where T: UICanvas
    {
        if (IsOpened<T>())
        {
            canvasActives[typeof(T)].CloseDirectly();
        }
    }

    public bool IsUILoaded<T>() where T: UICanvas
    {
        return canvasActives.ContainsKey(typeof(T)) && canvasActives[typeof(T)] != null;
    }

    public bool IsOpened<T>() where T : UICanvas
    {
        return IsUILoaded<T>() && canvasActives[typeof(T)].gameObject.activeSelf;
    }

    /// <summary>
    /// get active canvas
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetUI<T>() where T: UICanvas
    {
        if (!IsUILoaded<T>())
        {
            T prefab = GetUIPrefab<T>();

            T canvas = Instantiate(prefab, parent);

            canvasActives[typeof(T)] = canvas;
            canvas.gameObject.SetActive(false);
        }
        return canvasActives[typeof(T)] as T;
    }

    private T GetUIPrefab<T>() where T: UICanvas
    {
        
        return canvasPrefabs[typeof(T)] as T;
    }

    public void CloseAll()
    {
        foreach(var canvas in canvasActives)
        {
            if(canvas.Value != null && canvas.Value.gameObject.activeSelf)
            {
                canvas.Value.Close(0);
            }
        }
    }

    public void CloseAllDirectly()
    {
        foreach(var canvas in canvasActives)
        {
            if(canvas.Value != null && canvas.Value.gameObject.activeSelf)
            {
                canvas.Value.CloseDirectly();
            }
        }
    }
}