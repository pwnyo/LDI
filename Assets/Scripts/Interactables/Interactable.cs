using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(BoxCollider2D))]
public abstract class Interactable : MonoBehaviour, System.IComparable<Interactable>
{
    public string interactableName;
    Material material;
    Material originalMaterial;
    SpriteRenderer sr;

    public enum ArrowDirection
    {
        DOWN,
        LEFT,
        RIGHT,
        UP
    }
    public bool showArrow;
    public ArrowDirection arrowDirection;
    public Vector3 arrowOffset;
    public Vector3 arrowScale = new Vector3(1, 1, 1);
    public bool startOnCollision;
    public abstract void Interact();

    protected virtual void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr)
        {
            originalMaterial = GetComponent<SpriteRenderer>().material;
        }
        //PlayerControl.Instance.AddInteractable(this);
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + arrowOffset, .1f);
    }

    public int CompareTo(Interactable other)
    {
        if (PlayerControl.Instance == null)
            return 0;
        float d1 = Mathf.Abs(this.transform.position.x - PlayerControl.Instance.transform.position.x);
        float d2 = Mathf.Abs(other.transform.position.x - PlayerControl.Instance.transform.position.x);
        if (d1 < d2)
            return 1;
        else if (d1 < d2)
            return -1;
        else
            return 0;
    }
    public void SetActiveMaterial(Material m = null)
    {
        if (!sr)
        {
            return;
        }

        if (m != null)
        {
            sr.material = m;
            Sequence seq = DOTween.Sequence();
            seq.Append(sr.material.DOColor(new Color(1, 1, 1, 0.1f), 0.5f));
            seq.Append(sr.material.DOColor(new Color(1, 1, 1, 0.5f), 0.5f));
            seq.SetLoops(-1);
        }
        else
        {
            sr.material = originalMaterial;
        }
    }
}
