using UnityEngine;

namespace AOETesting
{
    /// <summary>
    /// Visual indicator system for AOE areas using LineRenderer components.
    /// Provides methods to show circles, cones, and hide indicators.
    /// </summary>
    public class AOEVisualIndicator : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private Material lineMaterial;
        [SerializeField] private float lineWidth = 0.1f;
        [SerializeField] private int circleSegments = 32;
        [SerializeField] private float heightOffset = 0.1f; // Raise above ground to avoid z-fighting
        
        private LineRenderer lineRenderer;
        private bool isActive = false;
        
        void Awake()
        {
            // Create LineRenderer component
            lineRenderer = gameObject.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
            
            SetupLineRenderer();
        }
        
        void SetupLineRenderer()
        {
            // Configure LineRenderer properties
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.positionCount = 0;
            
            // Create a simple material if none provided
            if (lineMaterial == null)
            {
                lineMaterial = new Material(Shader.Find("Sprites/Default"));
                lineMaterial.color = new Color(1f, 1f, 1f, 0.7f); // Semi-transparent white
            }
            
            lineRenderer.material = lineMaterial;
            lineRenderer.enabled = false;
        }
        
        /// <summary>
        /// Shows a circular indicator at the specified position.
        /// </summary>
        /// <param name="center">Center position of the circle</param>
        /// <param name="radius">Radius of the circle</param>
        /// <param name="color">Color of the indicator</param>
        public void ShowCircle(Vector3 center, float radius, Color color)
        {
            if (lineRenderer == null) return;
            
            // Set color
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            
            // Calculate circle points
            lineRenderer.positionCount = circleSegments + 1;
            lineRenderer.loop = true;
            
            for (int i = 0; i <= circleSegments; i++)
            {
                float angle = i * 2f * Mathf.PI / circleSegments;
                Vector3 position = center + new Vector3(
                    Mathf.Cos(angle) * radius,
                    heightOffset,
                    Mathf.Sin(angle) * radius
                );
                lineRenderer.SetPosition(i, position);
            }
            
            lineRenderer.enabled = true;
            isActive = true;
            
            Debug.Log($"[AOEVisualIndicator] Showing circle at {center} with radius {radius}");
        }
        
        /// <summary>
        /// Shows a cone indicator from the origin in the forward direction.
        /// </summary>
        /// <param name="origin">Origin point of the cone</param>
        /// <param name="forward">Forward direction of the cone</param>
        /// <param name="coneAngle">Total angle of the cone in degrees</param>
        /// <param name="range">Range of the cone</param>
        /// <param name="color">Color of the indicator</param>
        public void ShowCone(Vector3 origin, Vector3 forward, float coneAngle, float range, Color color)
        {
            if (lineRenderer == null) return;
            
            // Set color
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            
            // Calculate cone points
            lineRenderer.positionCount = 4;
            lineRenderer.loop = false;
            
            // Normalize forward direction
            Vector3 normalizedForward = forward.normalized;
            
            // Calculate left and right edges of the cone
            float halfAngle = coneAngle * 0.5f;
            Vector3 leftDirection = Quaternion.AngleAxis(-halfAngle, Vector3.up) * normalizedForward;
            Vector3 rightDirection = Quaternion.AngleAxis(halfAngle, Vector3.up) * normalizedForward;
            
            // Set positions with height offset
            Vector3 originWithOffset = origin + Vector3.up * heightOffset;
            Vector3 leftPoint = originWithOffset + leftDirection * range;
            Vector3 rightPoint = originWithOffset + rightDirection * range;
            
            lineRenderer.SetPosition(0, originWithOffset);
            lineRenderer.SetPosition(1, leftPoint);
            lineRenderer.SetPosition(2, rightPoint);
            lineRenderer.SetPosition(3, originWithOffset); // Close the triangle
            
            lineRenderer.enabled = true;
            isActive = true;
            
            Debug.Log($"[AOEVisualIndicator] Showing cone at {origin}, angle: {coneAngle}°, range: {range}");
        }
        
        /// <summary>
        /// Shows a cone with arc (more detailed cone visualization).
        /// </summary>
        /// <param name="origin">Origin point of the cone</param>
        /// <param name="forward">Forward direction of the cone</param>
        /// <param name="coneAngle">Total angle of the cone in degrees</param>
        /// <param name="range">Range of the cone</param>
        /// <param name="color">Color of the indicator</param>
        /// <param name="arcSegments">Number of segments for the arc (default 8)</param>
        public void ShowConeWithArc(Vector3 origin, Vector3 forward, float coneAngle, float range, Color color, int arcSegments = 8)
        {
            if (lineRenderer == null) return;
            
            // Set color
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            
            // Calculate total points: origin + arc points + back to origin
            lineRenderer.positionCount = arcSegments + 3;
            lineRenderer.loop = false;
            
            Vector3 normalizedForward = forward.normalized;
            float halfAngle = coneAngle * 0.5f;
            Vector3 originWithOffset = origin + Vector3.up * heightOffset;
            
            // Start from origin
            lineRenderer.SetPosition(0, originWithOffset);
            
            // Create arc points
            for (int i = 0; i <= arcSegments; i++)
            {
                float currentAngle = Mathf.Lerp(-halfAngle, halfAngle, (float)i / arcSegments);
                Vector3 direction = Quaternion.AngleAxis(currentAngle, Vector3.up) * normalizedForward;
                Vector3 arcPoint = originWithOffset + direction * range;
                lineRenderer.SetPosition(i + 1, arcPoint);
            }
            
            // Close back to origin
            lineRenderer.SetPosition(arcSegments + 2, originWithOffset);
            
            lineRenderer.enabled = true;
            isActive = true;
            
            Debug.Log($"[AOEVisualIndicator] Showing detailed cone at {origin}, angle: {coneAngle}°, range: {range}");
        }
        
        /// <summary>
        /// Hides the visual indicator.
        /// </summary>
        public void Hide()
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
                lineRenderer.positionCount = 0;
            }
            
            isActive = false;
            Debug.Log("[AOEVisualIndicator] Indicator hidden");
        }
        
        /// <summary>
        /// Checks if the indicator is currently active.
        /// </summary>
        public bool IsActive => isActive;
        
        /// <summary>
        /// Updates the line width of the indicator.
        /// </summary>
        /// <param name="width">New line width</param>
        public void SetLineWidth(float width)
        {
            lineWidth = width;
            if (lineRenderer != null)
            {
                lineRenderer.startWidth = width;
                lineRenderer.endWidth = width;
            }
        }
        
        /// <summary>
        /// Updates the material of the indicator.
        /// </summary>
        /// <param name="material">New material</param>
        public void SetMaterial(Material material)
        {
            lineMaterial = material;
            if (lineRenderer != null)
            {
                lineRenderer.material = material;
            }
        }
    }
}