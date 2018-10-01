using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Readme File", menuName = "My Scriptable Object/Readme File")]
public class Readme : ScriptableObject
{
    public Texture2D icon;
    public string title = "Readme";
    public Section[] sections;

    [Serializable]
    public class Section
    {
        public string heading;
        public Texture2D picture;
        [Multiline(6)]
        public string text;
        public string linkText;
        public string url;
        public UnityEngine.Object selectedObject;
    }
}
