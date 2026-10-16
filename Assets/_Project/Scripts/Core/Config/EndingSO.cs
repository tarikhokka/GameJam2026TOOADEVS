using UnityEngine;

namespace CivilJam.Core
{
    [CreateAssetMenu(menuName = "CivilJam/Ending", fileName = "Ending")]
    public class EndingSO : ScriptableObject
    {
        public EndingType Type;
        public string Title;
        [TextArea(3, 10)] public string Body;
        public Sprite Image;
    }
}
