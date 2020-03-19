using System;
using UnityEngine;

[ExecuteAlways]
public class SineConnectorRenderer : MonoBehaviour
{
    [SerializeField]
    private Vector3 startPoint;

    [SerializeField]
    private Vector3 endPoint;

    [SerializeField]
    private LineRenderer renderer;

    [SerializeField]
    private float segmentsPerMeter;

    [SerializeField]
    private float frequency;

    [SerializeField]
    private float speed;

    [SerializeField]
    private float amplitude;

    public Vector3 StartPosition
    {
        get => startPoint;
        set => startPoint = value;
    }
    
    public Vector3 EndPosition
    {
        get => endPoint;
        set => endPoint = value;
    }

    private void Update()
    {
        var dist = Vector3.Distance(startPoint, endPoint);
        var dir = endPoint - startPoint;
        int segments = (int)Mathf.Max(2, segmentsPerMeter * dist);

        var cameraDir = Camera.main.transform.InverseTransformDirection(dir);
        cameraDir.z = 0;
        var x = cameraDir.x;
        cameraDir.x = -cameraDir.y;
        cameraDir.y = x;
        var up = Camera.main.transform.TransformDirection(cameraDir);
        up = (up - Vector3.Project(up, dir)).normalized;

        renderer.positionCount = segments;
       
        
        for (var i = 0; i < segments; i++)
        {
            float t = (float) i / (segments - 1f);
            var p = Vector3.Lerp(startPoint, endPoint, t);
            p += up * Mathf.Sin(t * dist * frequency - speed * Time.time) * Mathf.Sin(t * Mathf.PI) * amplitude;
            renderer.SetPosition(i, p);
        }
    }
}