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
    private new LineRenderer renderer;

    [SerializeField]
    private float segmentsPerMeter = 250f;

    [SerializeField]
    private float frequency = 80f;

    [SerializeField]
    private float speed = 15f;

    [SerializeField]
    private float amplitude = 0.015f;

    [SerializeField]
    private float width = 0.02f;

    [SerializeField]
    private float scaling = 0.1f;

    public Vector3 StartPosition { get => startPoint; set => startPoint = value; }
    public Vector3 EndPosition { get => endPoint; set => endPoint = value; }
    public LineRenderer Renderer { get => renderer; set => renderer = value; }
    public float SegmentsPerMeter { get => segmentsPerMeter; set => segmentsPerMeter = value; }
    public float Frequency { get => frequency; set => frequency = value; }
    public float Speed { get => speed; set => speed = value; }
    public float Amplitude { get => amplitude; set => amplitude = value; }
    public float Width { get => width; set => width = value; }
    public float Scaling { get => scaling; set => scaling = value; }

    private void Update()
    {
        var dist = Vector3.Distance(startPoint, endPoint);
        var dir = endPoint - startPoint;
        
        var scaling = 1f + this.scaling / dist;

        var frequency = this.frequency * scaling;
        var amplitude = this.amplitude / scaling;
        var width = this.width / scaling;
        var segmentsPerMeter = Mathf.Clamp(this.segmentsPerMeter * scaling, 1, 10000f);
        
        int segments = (int) Mathf.Clamp(segmentsPerMeter * dist, 2, 10000f);

        var cameraDir = Camera.main.transform.InverseTransformDirection(dir);
        cameraDir.z = 0;
        var x = cameraDir.x;
        cameraDir.x = -cameraDir.y;
        cameraDir.y = x;
        var up = Camera.main.transform.TransformDirection(cameraDir);
        up = (up - Vector3.Project(up, dir)).normalized;

        renderer.positionCount = segments;

        renderer.startWidth = width;
        renderer.endWidth = width;

        for (var i = 0; i < segments; i++)
        {
            float t = (float) i / (segments - 1f);
            var p = Vector3.Lerp(startPoint, endPoint, t);
            p += up * Mathf.Sin(t * dist * frequency - speed * Time.time) * Mathf.Sin(t * Mathf.PI) * amplitude;
            renderer.SetPosition(i, p);
        }
    }
}