using System.Collections.Generic;
using UnityEngine;

namespace AOETesting
{
    /// <summary>
    /// Static utility class for AOE area detection using manual distance and angle calculations.
    /// Provides methods for circular and cone-shaped area detection.
    /// </summary>
    public static class AreaDetector
    {
        /// <summary>
        /// Detects all enemies within a circular radius from a center point.
        /// </summary>
        /// <param name="center">Center point of the circle</param>
        /// <param name="radius">Radius of the detection circle</param>
        /// <returns>List of GameObjects with "Enemy" tag within the radius</returns>
        public static List<GameObject> GetEnemiesInRadius(Vector3 center, float radius)
        {
            List<GameObject> enemiesInRange = new List<GameObject>();
            GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            
            Debug.Log($"[AreaDetector] Checking {allEnemies.Length} enemies for circular AOE at {center} with radius {radius}");
            
            foreach (GameObject enemy in allEnemies)
            {
                if (enemy == null) continue;
                
                float distance = Vector3.Distance(center, enemy.transform.position);
                if (distance <= radius)
                {
                    enemiesInRange.Add(enemy);
                    Debug.Log($"[AreaDetector] Enemy '{enemy.name}' detected in radius - Distance: {distance:F2}");
                }
            }
            
            Debug.Log($"[AreaDetector] Circular AOE detected {enemiesInRange.Count} enemies");
            return enemiesInRange;
        }
        
        /// <summary>
        /// Detects all enemies within a cone-shaped area.
        /// </summary>
        /// <param name="origin">Origin point of the cone</param>
        /// <param name="forward">Forward direction of the cone</param>
        /// <param name="coneAngle">Total angle of the cone in degrees</param>
        /// <param name="range">Maximum range of the cone</param>
        /// <returns>List of GameObjects with "Enemy" tag within the cone</returns>
        public static List<GameObject> GetEnemiesInCone(Vector3 origin, Vector3 forward, float coneAngle, float range)
        {
            List<GameObject> enemiesInCone = new List<GameObject>();
            GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            
            Debug.Log($"[AreaDetector] Checking {allEnemies.Length} enemies for cone AOE at {origin}, angle: {coneAngle}°, range: {range}");
            
            foreach (GameObject enemy in allEnemies)
            {
                if (enemy == null) continue;
                
                Vector3 dirToEnemy = (enemy.transform.position - origin).normalized;
                float distance = Vector3.Distance(origin, enemy.transform.position);
                float angleToEnemy = Vector3.Angle(forward.normalized, dirToEnemy);
                
                // Check if enemy is within range and angle
                if (distance <= range && angleToEnemy <= coneAngle * 0.5f)
                {
                    enemiesInCone.Add(enemy);
                    Debug.Log($"[AreaDetector] Enemy '{enemy.name}' detected in cone - Distance: {distance:F2}, Angle: {angleToEnemy:F1}°");
                }
            }
            
            Debug.Log($"[AreaDetector] Cone AOE detected {enemiesInCone.Count} enemies");
            return enemiesInCone;
        }
        
        /// <summary>
        /// Generic area detection with LayerMask support for future flexibility.
        /// </summary>
        /// <param name="center">Center point of detection</param>
        /// <param name="radius">Detection radius</param>
        /// <param name="layers">LayerMask to filter objects</param>
        /// <returns>List of GameObjects within the area on specified layers</returns>
        public static List<GameObject> GetObjectsInArea(Vector3 center, float radius, LayerMask layers)
        {
            List<GameObject> objectsInRange = new List<GameObject>();
            
            // Find all GameObjects in scene (this is expensive, but simple for testing)
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            
            foreach (GameObject obj in allObjects)
            {
                if (obj == null) continue;
                
                // Check if object is on the specified layer
                if ((layers.value & (1 << obj.layer)) == 0) continue;
                
                float distance = Vector3.Distance(center, obj.transform.position);
                if (distance <= radius)
                {
                    objectsInRange.Add(obj);
                }
            }
            
            Debug.Log($"[AreaDetector] Generic area detection found {objectsInRange.Count} objects");
            return objectsInRange;
        }
        
        /// <summary>
        /// Utility method to check if a specific point is within a circular area.
        /// </summary>
        /// <param name="center">Center of the circle</param>
        /// <param name="radius">Radius of the circle</param>
        /// <param name="point">Point to check</param>
        /// <returns>True if point is within the circle</returns>
        public static bool IsPointInCircle(Vector3 center, float radius, Vector3 point)
        {
            return Vector3.Distance(center, point) <= radius;
        }
        
        /// <summary>
        /// Utility method to check if a specific point is within a cone area.
        /// </summary>
        /// <param name="origin">Origin of the cone</param>
        /// <param name="forward">Forward direction of the cone</param>
        /// <param name="coneAngle">Total angle of the cone in degrees</param>
        /// <param name="range">Maximum range of the cone</param>
        /// <param name="point">Point to check</param>
        /// <returns>True if point is within the cone</returns>
        public static bool IsPointInCone(Vector3 origin, Vector3 forward, float coneAngle, float range, Vector3 point)
        {
            Vector3 dirToPoint = (point - origin).normalized;
            float distance = Vector3.Distance(origin, point);
            float angleToPoint = Vector3.Angle(forward.normalized, dirToPoint);
            
            return distance <= range && angleToPoint <= coneAngle * 0.5f;
        }
    }
}