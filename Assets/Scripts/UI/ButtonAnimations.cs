using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CoreCraft.Core
{
    public class ButtonAnimations : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private Sequence _buttonSequence;

        private void Start()
        {
            // Setup the DOTween Sequence.
            _buttonSequence = DOTween.Sequence().SetUpdate(true).SetAutoKill(false).Pause();

            Transform buttonTransform = GetComponent<Transform>();

            // Function of the Sequence.
            _buttonSequence.Append(buttonTransform.DOScale(new Vector3(0.04f, 0.04f, 0.04f), 0.5f))
                .Join(buttonTransform.DOPunchRotation(Vector3.one, 0.5f));
        }

        // Play the animation forward if the cursor enters the button. Forward because "SmoothRewind" flips the animation.
        public void OnPointerEnter(PointerEventData eventData)
        {
            _buttonSequence.PlayForward();
            int i = Random.Range(0, 3);
            SoundManager.Instance.PlaySFX(i);
        }

        // Smoothly rewind the animation if the cursor leaves the button. Flips the animation direction.
        public void OnPointerExit(PointerEventData eventData)
        {
            _buttonSequence.SmoothRewind();
        }

        // Kill the animation if the object is destroyed.
        private void OnDestroy()
        {
            _buttonSequence.Kill();
        }

        // Rewind the animation - reset properties to initial values if the object is disabled.
        private void OnDisable()
        {
            _buttonSequence.Rewind();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            int i = Random.Range(0, 3);
            SoundManager.Instance.PlaySFX(i);
        }
    }
}