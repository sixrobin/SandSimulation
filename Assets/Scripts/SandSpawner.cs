namespace SandSimulation
{
    using UnityEngine;

    [DisallowMultipleComponent]
    public class SandSpawner : MonoBehaviour
    {
        [SerializeField]
        private Camera _camera;
        [SerializeField]
        private BoxCollider2D _collider;
        [SerializeField]
        private SandSimulation _simulation;

        private void OnMouseDown()
        {
            Vector2 mouseWorldPosition = this._camera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 lowerLeft = (Vector2)this._collider.transform.position - (this._collider.size * 0.5f);
            Vector2 upperRight = (Vector2)this._collider.transform.position + (this._collider.size * 0.5f);
            
            Vector2 clickUV = new(Mathf.InverseLerp(lowerLeft.x, upperRight.x, mouseWorldPosition.x),
                                 Mathf.InverseLerp(lowerLeft.y, upperRight.y, mouseWorldPosition.y));
            
            this._simulation.SpawnSand(clickUV);
        }
    }
}