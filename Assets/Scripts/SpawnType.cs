namespace SandSimulation
{
    using UnityEngine;
    
    [CreateAssetMenu(fileName = "New Spawn Type", menuName = "Sand Simulation/Spawn Type")]
    public class SpawnType : UnityEngine.ScriptableObject
    {
        [field: SerializeField]
        public int ID { get; private set; }
        
        [field: SerializeField]
        public int Weight { get; private set; }
    }
}