using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreCraft.Core;
using DG.Tweening;

namespace CoreCraft.LudumDare55
{
    public class AlchemyTable : MonoBehaviour, IDamageable, IInGrid
    {
        private Resource[] resources;
        [SerializeField] private List<GameObject> _summons;
        private bool _isActivated;
        [SerializeField] private GameObject _finalState;
        [SerializeField] private GameObject _tempsState;
        [SerializeField] private Vector2Int _currentPosition;

        public int HP => throw new System.NotImplementedException();

        public Vector2Int CurrentPosition => _currentPosition;

        [SerializeField] private int _hp;
        private void Awake()
        {
            resources = new Resource[2];
            _isActivated = false;

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
                TakeDamage(0);
            else
            {
                _currentPosition = new Vector2Int(_currentPosition.y + moveIncrements,_currentPosition.y);
            }
        }

        //private void OnCollisionEnter(Collision collision)
        //{

        //    if (collision.transform.tag == "Resource")
        //    {

        //        Debug.Log("Resource Enter");
        //        if(resources[0] == null)
        //        {
        //            resources[0] = collision.transform.GetComponent<Resource>();
        //        }
        //        else
        //        {
        //            resources[1] = collision.transform.GetComponent<Resource>();
        //            if(_isActivated)
        //                Summon();
        //        }

        //    }
        //}

        private void OnTriggerEnter(Collider other)
        {

            if (other.transform.tag == "Resource")
            {
                Debug.Log("Resource Enter");
                if (resources[0] == null)
                {
                    resources[0] = other.transform.GetComponent<Resource>();
                }
                else
                {
                    resources[1] = other.transform.GetComponent<Resource>();
                    if (_isActivated)
                        Summon();
                }

            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.transform.tag == "Resource")
            {
                Debug.Log("Resource Exit");
                if (resources[0] == other.transform.GetComponent<Resource>())
                    resources[0] = null;
                else if (resources[1] == other.transform.GetComponent<Resource>())
                    resources[1] = null;
            }
        }

        public void Activate()
        {
            _isActivated = true;
            _finalState.SetActive(true);
            _tempsState.SetActive(false);
            if (resources[0] != null && resources[1] != null)
                Summon();

            SummonManager.Instance.RegisterTable(this);
        }

        public void DestroyAlchemyTable()
        {
            Destroy(this);
        }

        private void Summon()
        {
            EventManager.Instance.Summon.Invoke();
            switch (resources[0].Resources)
            {
                case BlockResources.Kristall:
                    switch (resources[1].Resources)
                    {
                        case BlockResources.Kristall:
                            SummonEntity(5);
                            break;
                        case BlockResources.Gold:
                            SummonEntity(4);
                            break;
                        case BlockResources.Schleim:
                            SummonEntity(0);
                            break;
                    }
                    break;
                case BlockResources.Gold:
                    switch (resources[1].Resources)
                    {
                        case BlockResources.Kristall:
                            SummonEntity(4);
                            break;
                        case BlockResources.Gold:
                            SummonEntity(2);
                            break;
                        case BlockResources.Schleim:
                            SummonEntity(3);
                            break;
                    }
                    break;
                case BlockResources.Schleim:
                    switch (resources[1].Resources)
                    {
                        case BlockResources.Kristall:
                            SummonEntity(0);
                            break;
                        case BlockResources.Gold:
                            SummonEntity(3);
                            break;
                        case BlockResources.Schleim:
                            SummonEntity(1);
                            break;
                    }
                    break;
            }
            Destroy(resources[0].transform.gameObject);
            Destroy(resources[1].transform.gameObject);
            resources[0] = null;
            resources[1] = null;
        }

        private void SummonEntity(int i)
        {
            GameObject summon = Instantiate(_summons[i], this.transform.position, new Quaternion(0, 0, 0, 0));
            summon.GetComponent<IInGrid>().Spawn(Grid.Instance.GetCellByDirection(this.transform.position).GridPosition, Vector2Int.up);
        }

        public bool TakeDamage(int damage)
        {
            DestroyAlchemyTable();
            SummonManager.Instance.UnregisterTable(this);
            return true;
        }

        public void Spawn(Vector2Int spawnPosition, Vector2Int spawnRotation)
        {
            _currentPosition = spawnPosition;
        }
    }   
}