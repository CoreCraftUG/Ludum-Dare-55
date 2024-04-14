using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.LudumDare55
{
    [RequireComponent(typeof(BoxCollider))]
    public class BaseSightCone : MonoBehaviour
    {
        [ShowInInspector,ReadOnly] protected ICanSee _aICharacter => GetComponentInParent<ICanSee>();

        protected BoxCollider _collider => GetComponent<BoxCollider>();

        protected virtual void Start()
        {
            _collider.size = new Vector3(0.5f, 0.5f, _aICharacter.SightDistance);
            _collider.center = new Vector3(0, 0, _aICharacter.SightDistance / 2);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            _aICharacter.CheckSightCone(other);
        }

        protected virtual void OnTriggerStay(Collider other)
        {

        }

        protected virtual void OnTriggerExit(Collider other)
        {

        }
    }
}