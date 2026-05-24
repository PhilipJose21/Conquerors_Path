using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public float ZoomChange;
    public float SmoothChange;
    public float MinSize, MaxSize;

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if(Input.mouseScrollDelta.y > 0 && cam.orthographicSize > MinSize && Input.GetKey(KeyCode.Space))
        {
            cam.orthographicSize -= ZoomChange * Time.deltaTime * SmoothChange;
        }
        else if(Input.mouseScrollDelta.y < 0 && cam.orthographicSize < MaxSize && Input.GetKey(KeyCode.Space))
        {
            cam.orthographicSize += ZoomChange * Time.deltaTime * SmoothChange;
        }
        else if(Input.GetMouseButtonDown(1))
        {
            cam.orthographicSize = MaxSize;
        }
        
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, MinSize, MaxSize);
    }
}
