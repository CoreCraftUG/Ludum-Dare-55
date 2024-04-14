using UnityEngine;

namespace JamCraft.GMTK2023
{
    public class AnimationHandler : MonoBehaviour
    {
        private Animator _animator => GetComponent<Animator>();
        
        /// <summary>
        /// Play an animation.
        /// </summary>
        /// <param name="stateName">The name of the specific animation. Needs the name of the layer as prefix. E.g. "Base Layer.AnimationName"</param>
        /// <param name="layer">Index of the layer of the animation.</param>
        /// <param name="offSet">Offset of the animation. E.g. 0.5f will let the animation start halfway through.</param>
        protected void PlayAnimation(string stateName, int layer, float offSet)
        {
            _animator.Play(stateName, layer, offSet);
        }
    }
}
