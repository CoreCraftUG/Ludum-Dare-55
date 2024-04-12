using UnityEngine;
using UnityEngine.UI;

namespace JamCraft.GMTK2023.Code
{
    public class TransparentButton : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
        }
    }
}