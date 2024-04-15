using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.LudumDare55
{
    public interface IDamageable
    {
        int HP { get; }
        bool TakeDamage(int damage);
    }

    public interface ICanDamage
    {
        int Damage { get; }
        float AttackTime { get; }
        void DealDamage(IDamageable damageable);
    }

    public interface ICanDie
    {
        Animator Animator { get; }
        void Die();
    }

    public interface ICanSee
    {
        int SightDistance { get; }
        LayerMask SightLayerMask { get; }
        void CheckSightCone(Collider other);
    }

    public interface IInGrid
    {
        Vector2Int CurrentPosition { get; }

        void Spawn(Vector2Int spawnPosition, Vector2Int spawnRotation);
    }

    public interface IMoveInGrid
    {
        float MoveTime { get; }

        Vector2Int TargetPosition { get; }
        Vector2Int ReturnPoint { get; }
        bool HasTarget { get; }
        bool IsMoving { get; }
        Stack<GridCell> TargetPath { get; }
    }

    public interface IPeripheryTrigger
    {
        void TriggerEnter(Collider other);
        void TriggerExit(Collider other);
    }
}