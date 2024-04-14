using UnityEngine;

namespace CoreCraft.LudumDare55
{
    public class CursorManager : MonoBehaviour
    {
        [SerializeField] private Texture2D _defaultCursorTexture;

        private Vector2 _cursorHotspot;

        private void Start()
        {
            _cursorHotspot = new Vector2(0, 0);
            Cursor.SetCursor(_defaultCursorTexture, _cursorHotspot, CursorMode.Auto);
        }

        private void ChangeCursor(object sender)
        {
            switch (sender)
            {
                
            }

            // TODO: Set Cursor to custom Texture.
        }

        private void ResetCursor()
        {
            Cursor.SetCursor(_defaultCursorTexture, _cursorHotspot, CursorMode.Auto);
        }
    }
}