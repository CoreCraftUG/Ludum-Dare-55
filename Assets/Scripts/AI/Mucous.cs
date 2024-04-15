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
        private float _timer;

        private void Update()
        {
            _timer += Time.deltaTime;

            if ( _timer > _lifeTime )
                Despawn();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable) && _sightLayerMask == (_sightLayerMask | (1 << other.gameObject.layer)))
            {
                damageable.TakeDamage(_damage);
            }
        }

        public void Despawn()
        {
            Destroy(gameObject);
        }
    }
}