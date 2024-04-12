using UnityEngine;
using UnityEngine.UI;

namespace CoreCraft.Core
{
    public class TransparentButton : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
        }
    }
}