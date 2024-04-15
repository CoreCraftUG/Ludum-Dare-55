using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.LudumDare55
{
    public class GargoyleSightCone : BaseSightCone
    {
        [ShowInInspector, ReadOnly] protected ICanSee _aICharacter => GetComponentInParent<ICanSee>();
        [SerializeField] protected int _sightHeight;

        protected BoxCollider _collider => GetComponent<BoxCollider>();

        protected override void Start()
        {
            _collider.size = new Vector3(_aICharacter.SightDistance, _sightHeight, _aICharacter.SightDistance);
            _collider.center = new Vector3(0, 0, _aICharacter.SightDistance / 2);
        }

        protected override void OnTriggerStay(Collider other)
        {
            _aICharacter.CheckSightCone(other);
        }
    }
}