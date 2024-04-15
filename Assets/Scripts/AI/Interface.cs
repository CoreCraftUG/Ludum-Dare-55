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

    public interface IGoToWaitingArea
    {
        bool WaveStart { get; }
        IEnumerator GoToSpawnArea(Vector3 position);

        void WanderAround(Vector3 bottomLeftCorner, Vector3 topRightCorner);
        void GoToStartCell();
    }
}

/*
        private bool _goingBackToEntrance;


            EventManager.Instance.GridMoveUp.AddListener((Vector3 moveVector, float moveTime, int moveIncrements) =>
            {
                StartCoroutine(ReturnToGrid(moveVector,moveTime,moveIncrements));
            });


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
                GridCell cell = null;
                yield return new WaitUntil(() =>
                {
                    cell = PlayManager.Instance.GetGridEntrance();
                    return cell != null;
                });
                _currentPosition = cell.GridPosition;

                _goingBackToEntrance = true;
                transform.DOMove(cell.WorldPosition, _moveTime).OnComplete(() =>
                {
                    _goingBackToEntrance = false;
                });
            }
        }


            if (_goingBackToEntrance)
                return;
 */