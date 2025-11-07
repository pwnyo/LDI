using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Transform cam;
    public Vector2 distance;
    public Vector2 smoothing;

    [Header("Speeds")]
    public bool ignoreX;
    public bool ignoreY;
    public Vector2 parallaxSpeed;

    [Header("Bounds")]
    public Transform parentObj;
    public Vector2 bottomLeft;
    public Vector2 topRight;
    Vector3 previousCamPos;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Redraw();
    }

    public void Redraw()
    {
        /*
        if (distance.x != 0f)
        {
            float parallaxX = (previousCamPos.x - cam.position.x) * distance.x;
            Vector3 backgroundTargetPosX = new Vector3(transform.position.x + parallaxX,
                                                      transform.position.y);

            // Lerp to fade between positions
            transform.position = Vector3.Lerp(transform.position, backgroundTargetPosX, Time.deltaTime);
        }

        if (distance.y != 0f)
        {
            float parallaxY = (previousCamPos.y - cam.position.y) * distance.y;
            Vector3 backgroundTargetPosY = new Vector3(transform.position.x,
                                                       transform.position.y + parallaxY);

            transform.position = Vector3.Lerp(transform.position, backgroundTargetPosY, Time.deltaTime);
        }
        previousCamPos = cam.position;*/

    }

    private void OnDrawGizmos()
    {
        /*
        Vector3 center = new Vector3(width / 2, height / 2);

        //Gizmos.DrawWireCube(center, new Vector2(width, height));
        Vector2 botLeft = new Vector2(transform.position.x - xBounds.x, transform.position.y - yBounds.x);
        //Vector2 botRight = new Vector2(transform.position.x + xBounds.y, transform.position.y - xBounds.x);
        //Vector2 topLeft = new Vector2(transform.position.x - xBounds.x, transform.position.y + xBounds.y);
        Vector2 topRight = new Vector2(transform.position.x + xBounds.y, transform.position.y + yBounds.y);
        //Vector2 center = new Vector2((transform.position.x - botLeft.x)

        //Gizmos.DrawWireCube(new Vector2(botLeft.x + topRight.x / 2f, botLeft.y + topRight.y / 2f), new Vector2(width, height));*/
        //Vector3 bottomLeft = new Vector3(transform.position.x - this.topRightCorner.x, transform.position.y + bottomLeftCorner.y);
        //Vector3 topRight = new Vector3(transform.position.x + this.bottomLeftCorner.x, transform.position.y - topRightCorner.y);
        Vector3 center = new Vector3(bottomLeft.x + topRight.x / 2f, bottomLeft.y + topRight.y / 2f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(bottomLeft, .2f);
        Gizmos.DrawWireSphere(topRight, .2f);
        Gizmos.DrawWireCube(center, new Vector3(bottomLeft.x - topRight.x, bottomLeft.y - topRight.y));
    }
}
