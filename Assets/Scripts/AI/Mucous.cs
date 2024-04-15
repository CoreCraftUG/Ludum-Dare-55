using CoreCraft.Core;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.LudumDare55
{
    public class Mucous : MonoBehaviour
    {
        [SerializeField] float _lifeTime;
        [SerializeField] int _damage;
        [SerializeField] protected LayerMask _sightLayerMask;
        [SerializeField] private Animator _animator;
        private float _timer;

        private Vector2Int _currentPosition;

        private bool _goingBackToEntrance;

        private void Start()
        {
            _currentPosition = Grid.Instance.GetCellByDirection(transform.position).GridPosition;

            EventManager.Instance.GridMoveUp.AddListener((Vector3 moveVector, float moveTime, int moveIncrements) =>
            {
                StartCoroutine(ReturnToGrid(moveVector, moveTime, moveIncrements));
            });

        }

        private IEnumerator ReturnToGrid(Vector3 moveVector, float moveTime, int moveIncrements)
        {
            bool moveDone = false;
            transform.DOMove(transform.position + moveVector, moveTime).OnComplete(() =>
            {
                moveDone = true;
            });

            yield return new WaitUntil(() => moveDone);

            if (_currentPosition.y + moveIncrements >= Grid.Instance.GridHeight)
            {
                Despawn();
            }
            else
            {
                _currentPosition = new Vector2Int(_currentPosition.y + moveIncrements, _currentPosition.y);
            }
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            if ( _timer > _lifeTime )
                Despawn();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable) && !other.gameObject.TryGetComponent<FlyEnemy>(out FlyEnemy enemy) && _sightLayerMask == (_sightLayerMask | (1 << other.gameObject.layer)))
            {
                damageable.TakeDamage(_damage);
            }
        }

        public void Despawn()
        {
            _animator.SetBool("Despawn", true);
            Destroy(gameObject, _animator.GetCurrentAnimatorStateInfo(transform.gameObject.layer).length);
        }
    }
}