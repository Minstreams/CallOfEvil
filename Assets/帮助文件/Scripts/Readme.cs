using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Readme", menuName = "帮助文件/Readme File")]
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
