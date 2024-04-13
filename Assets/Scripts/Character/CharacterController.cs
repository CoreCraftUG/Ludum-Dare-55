using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using CoreCraft.LudumDare55;
using Grid = CoreCraft.LudumDare55.Grid;
using DG.Tweening;
using System.Linq;


namespace CoreCraft.Core
{
    public class CharacterController : MonoBehaviour
    {
        [SerializeField] private Grid grid;
        [SerializeField] private GameObject _carriedResource;
        [SerializeField] private GameObject _tempTable;
        [SerializeField] private GameObject _alchemyTable;
        

        private void Awake()
        {
            GameInputManager.Instance.OnRightClick += RightClick;
            GameInputManager.Instance.OnLeftClick += LeftClick;
        }

        private void LeftClick(object sender, System.EventArgs e)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                GridCell cell = grid.GetCellByDirection(hit.transform.position);
                if(cell.Block.Material != BlockMaterial.None)
                    grid.MineCell(cell);

                if(_carriedResource != null)
                {
                    GameObject temp = _carriedResource;
                    _carriedResource = null;
                    temp.GetComponent<BoxCollider>().enabled = true;
                    temp.transform.DOMove(cell.WorldPosition, .1f);
                    temp.GetComponent<Resource>().PosCell = cell;

                }

                else if(hit.transform.tag == "Resource")
                {
                    _carriedResource = hit.transform.gameObject;
                    _carriedResource.GetComponent<BoxCollider>().enabled = false;
                }
            }
        }

        private void Update()
        {
            if (_carriedResource != null)
                _carriedResource.transform.position = Input.mousePosition;
            if(AStar.HalloIchBinJulianUndIchWillWissenObIchNebenIhnenStehe(grid.GetCellByDirection(transform.position).GridPosition, grid.GetCellByDirection(_tempTable.transform.position).GridPosition))
            {
                _tempTable.GetComponent<AlchemyTable>().Activate();
            }
        }
        private bool CharacterTryMove()
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                GridCell cell = grid.GetCellByDirection(hit.transform.position);
                Stack<GridCell> temp = AStar.StandardAStar(grid.GetCellByDirection(transform.position).GridPosition, cell.GridPosition, PathfindingMode.Default);
                if (temp == null)
                    return false;
                if (cell.Block.Material != BlockMaterial.None)
                {
                    if(_tempTable != null)
                        Destroy(_tempTable);
                    _tempTable = Instantiate(_alchemyTable, cell.WorldPosition, new Quaternion(0,0,0,0));
                    
                }
                return true;
            }
            return false;
        }


        private void RightClick(object sender, System.EventArgs e)
        {
            if (CharacterTryMove())
            {

            }
            else return;
        }

    }
}
