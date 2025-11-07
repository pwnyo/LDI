using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax2D : MonoBehaviour
{
    public Vector3 bounds;
    public Transform cam;
    public List<Transform> layers;
    List<Vector3> layerInitialPos = new List<Vector3>();
    Vector2 extents = new Vector2();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < layers.Count; i++)
        {
            Transform t = layers[i];
            layerInitialPos.Add(t.position);

            if (t.localScale.x > extents.x)
            {
                extents.x = t.localScale.x;
            }
            if (t.localScale.y > extents.y)
            {
                extents.y = t.localScale.y;
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Redraw();
    }
    public void Redraw()
    {
        for (int i = 0; i < layers.Count; i++)
        {
            SetPos(layers[i], layerInitialPos[i]);
        }
    }
    void SetPos(Transform tr, Vector3 pos)
    {
        float t = Mathf.InverseLerp(bounds.x, bounds.y, cam.position.x);
        float xl, xr; //yd, yu;
        xl = GetOffset(tr.localScale.x, extents.x, true);
        xr = GetOffset(tr.localScale.x, extents.x);
        //yd = GetHalf(extents.y, true);
        //yu = GetHalf(extents.y);
        tr.localPosition = pos + new Vector3(Mathf.Lerp(xl, xr, t), 0);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(bounds.x, 0), .2f);
        Gizmos.DrawWireSphere(new Vector3(bounds.y, 0), .2f);
    }

    float GetOffset(float current, float reference, bool isNegative = false)
    {
        return (reference - current) / 2f * (isNegative ? -1 : 1);
    }
}
