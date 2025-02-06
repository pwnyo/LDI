using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//DEPRECATED
public class FieldOfView : MonoBehaviour
{
    public Transform target;
    public float lookDistance;
    public float lookAngle;
    public bool isBlind;
    [SerializeField]
    bool seesPlayer;
    public Transform cam;
    public LayerMask layerMask;

    public int rayCount;
    //Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        //mesh = new Mesh();
        //GetComponent<MeshFilter>().mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        Look();
        /*
        Vector3 origin = Vector3.zero;

        float angleIncrease = lookAngle / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];
        vertices[0] = Vector3.zero;

        int vertexIndex = 1;
        int triangleIndex = 0;
        float angle = 0;
        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex;
            RaycastHit2D ray = Physics2D.Raycast(transform.position, GetDirectionFromAngle(angle), lookDistance, blockMask);

            if (!ray.collider)
            {
                vertex = GetDirectionFromAngle(angle) * lookDistance;
            }
            else
            {
                vertex = ray.point;
                //vertex = transform.InverseTransformPoint(vertex);
            }
            vertices[vertexIndex] = vertex;
            if (i > 0)
            {
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;

            }
            vertexIndex++;
            angle -= angleIncrease;
        }
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;*/
    }
    void Look()
    {

        /*
        Vector3 dirToPlayer = (target.position - transform.position).normalized;
        if (Vector3.Distance(transform.position, target.position) < lookDistance && Vector3.Angle(GetDirectionFromAngle(360 - transform.eulerAngles.z), dirToPlayer) < lookAngle / 2)
        {
            seesPlayer = true;
        }
        else
        {
            seesPlayer = false;
        }*/
        /*
        float left = lookAngle / 2;
        float leftm = lookAngle * 0.25f;
        float leftl = lookAngle * 0.375f;
        float right = -left;
        float rightm = -leftm;
        float rightr = -leftl;
        float midoff = lookAngle * 0.125f;
        RaycastHit2D l = Physics2D.Raycast(transform.position, GetDirectionFromAngle(left, true), lookDistance, layerMask);
        RaycastHit2D ll = Physics2D.Raycast(transform.position, GetDirectionFromAngle(leftl, true), lookDistance, layerMask);
        RaycastHit2D r = Physics2D.Raycast(transform.position, GetDirectionFromAngle(right, true), lookDistance, layerMask);
        RaycastHit2D rr = Physics2D.Raycast(transform.position, GetDirectionFromAngle(rightr, true), lookDistance, layerMask);
        RaycastHit2D lm = Physics2D.Raycast(transform.position, GetDirectionFromAngle(leftm, true), lookDistance, layerMask);
        RaycastHit2D rm = Physics2D.Raycast(transform.position, GetDirectionFromAngle(rightm, true), lookDistance, layerMask);
        RaycastHit2D f = Physics2D.Raycast(transform.position, GetDirectionFromAngle(360, true), lookDistance, layerMask);
        RaycastHit2D fl = Physics2D.Raycast(transform.position, GetDirectionFromAngle(360 + midoff, true), lookDistance, layerMask);
        RaycastHit2D fr = Physics2D.Raycast(transform.position, GetDirectionFromAngle(360 - midoff, true), lookDistance, layerMask);

        Debug.DrawRay(transform.position, GetDirectionFromAngle(left, true) * lookDistance);
        Debug.DrawRay(transform.position, GetDirectionFromAngle(leftm, true) * lookDistance);
        Debug.DrawRay(transform.position, GetDirectionFromAngle(leftl, true) * lookDistance);
        Debug.DrawRay(transform.position, GetDirectionFromAngle(right, true) * lookDistance);
        Debug.DrawRay(transform.position, GetDirectionFromAngle(rightm, true) * lookDistance);
        Debug.DrawRay(transform.position, GetDirectionFromAngle(rightr, true) * lookDistance);
        Debug.DrawRay(transform.position, GetDirectionFromAngle(360, true) * lookDistance);
        Debug.DrawRay(transform.position, GetDirectionFromAngle(360 + midoff, true) * lookDistance);
        Debug.DrawRay(transform.position, GetDirectionFromAngle(360 - midoff, true) * lookDistance);*/

        for (int i = -6; i < 6; i++)
        {
            float angle = lookAngle * (i / 12f);
            RaycastHit2D ray = Physics2D.Raycast(transform.position, GetDirectionFromAngle(angle, true), lookDistance, layerMask);
            

            if (ray.collider)
            {
                seesPlayer = true;
                return;
            }
        }
        seesPlayer = false;
        /*
        if (l.collider || r.collider || f.collider || lm.collider || rm.collider || ll.collider || rr.collider || fl.collider || fr.collider)
        {
            seesPlayer = true;
        }
        else
        {
            seesPlayer = false;
        }*/
    }
    Vector3 GetDirectionFromAngle(float angleInDeg, bool global = false)
    {
        if (global)
        {
            angleInDeg -= transform.eulerAngles.z;
        }
        angleInDeg *= Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(angleInDeg), Mathf.Cos(angleInDeg));
    }

    public bool GetSees()
    {
        return seesPlayer;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (seesPlayer)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.green;
        }

        float left = lookAngle / 2;
        float right = -(lookAngle / 2);
        Gizmos.DrawLine(transform.position, transform.position + GetDirectionFromAngle(left, true).normalized * lookDistance);
        Gizmos.DrawLine(transform.position, transform.position + GetDirectionFromAngle(right, true).normalized * lookDistance);
    }
}
