using System;
using UnityEngine;

namespace URDF.Research_3.Scripts
{
    public class CollisionDetection : MonoBehaviour
    {
        private float _lastCollisionTime;
        private const float DebounceTime = 0.5f;  // prevent spam
        private EmergencyStop _emergencyStop;

        private void Start()
        {
            _emergencyStop = GameObject.Find("ROSController").GetComponent<EmergencyStop>();
        }

        private float[] GetPositions()
        {
            var positions = new float[9];
            
            
            return positions;
        }

        void OnCollisionEnter(Collision collision)
        {
            // Ignore self-collision
            if (collision.gameObject.name.Contains("fr3_")) return;
            
            if (Time.time - _lastCollisionTime < DebounceTime) return;
            
            _lastCollisionTime = Time.time;
            Debug.Log($"COLLISION with {collision.gameObject.name} → EMERGENCY STOP SENT");
            
            _emergencyStop.SendEmergencyStop();
            
        }
    }
}
