#if UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using UnityEngine;

[ExecuteInEditMode]
public class CustomEditorTitle : MonoBehaviour
{
    public static CustomEditorTitle instance;

    private IntPtr windowPtr;
    private string windowText;

    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    private static extern bool SetWindowText(IntPtr hwnd, string lpString);

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Start()
    {
        windowPtr = GetActiveWindow();
        windowText = Application.productName + " - Unity Editor" + " [" + Application.unityVersion + "]";
        
        SetWindowText(windowPtr, windowText);
    }

    private void OnRenderObject()
    {
        SetWindowText(windowPtr, windowText);
    }
}
#endif