using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.LudumDare55
{
    public class AlchemyTable : MonoBehaviour
    {
        private Resource[] resources;
        [SerializeField] private List<GameObject> _summons;
        private bool isActive;
        [SerializeField] private GameObject _finalState;
        [SerializeField] private GameObject _tempsState;

        private void Awake()
        {
            resources = new Resource[2];
            isActive = false;
        }
        private void OnCollisionEnter(Collision collision)
        {
            if(collision.transform.tag == "Resource")
            {
                if(resources[0] == null)
                {
                    resources[0] = collision.transform.GetComponent<Resource>();
                }
                else
                {
                    resources[1] = collision.transform.GetComponent<Resource>();
                    Summon();
                }

            }
        }

        public void Activate()
        {
            _finalState.SetActive(true);
            _tempsState.SetActive(false);
        }

        private void Summon()
        {
            switch (resources[0].Resources)
            {
                case BlockResources.Kristall:
                    switch (resources[1].Resources)
                    {
                        case BlockResources.Kristall:
                            break;
                        case BlockResources.Gold:
                            break;
                        case BlockResources.Schleim:
                            break;
                    }
                    break;
                case BlockResources.Gold:
                    switch (resources[1].Resources)
                    {
                        case BlockResources.Kristall:
                            break;
                        case BlockResources.Gold:
                            break;
                        case BlockResources.Schleim:
                            break;
                    }
                    break;
                case BlockResources.Schleim:
                    switch (resources[1].Resources)
                    {
                        case BlockResources.Kristall:
                            break;
                        case BlockResources.Gold:
                            break;
                        case BlockResources.Schleim:
                            break;
                    }
                    break;
            }
            Destroy(resources[0].transform.gameObject);
            Destroy(resources[1].transform.gameObject);
        }

        private void SummonEntity(int i)
        {
            Instantiate(_summons[i], this.transform);
        }
    }


    
}
