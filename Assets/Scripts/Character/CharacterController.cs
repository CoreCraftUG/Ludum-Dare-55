using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CoreCraft.Core
{
    public class CharacterController : MonoBehaviour
    {

        private void Awake()
        {
            GameInputManager.Instance.OnRightClick += RightClick;
            GameInputManager.Instance.OnLeftClick += CharacterMine;
        }

        private void CharacterMine(object sender, System.EventArgs e)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {

            }
        }

        private bool CharacterTryMove()
        {
            return false;
        }

        private void CharacterMove()
        {
            
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
