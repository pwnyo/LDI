using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using DG.Tweening;

public class BuildingEntryHelper : MonoBehaviour
{
    public Vector3 enterPoint, exitPoint;
    public GameObject buildingBoundaries;
    public SpriteRenderer outerWall;
    public Ease easeType;
    public float opacity = 0.25f;
    public float duration = 0.25f;
    public int order = 1;
    int originalSortingOrder;
    bool isInside;

    private void Start()
    {
        originalSortingOrder = PlayerControl.Instance.spriteRenderer.sortingOrder;
    }
    [YarnCommand("door")]
    public void UseDoor()
    {
        isInside = !isInside;
        if (isInside)
        {
            Enter();
        }
        else
        {
            Exit();
        }
    }
    public void Enter()
    {
        buildingBoundaries.SetActive(true);
        PlayerControl.Instance.SpawnKeepFlip(new Vector3(PlayerControl.Instance.transform.position.x, enterPoint.y));

        PlayerControl.Instance.AllowMovement(false);
        outerWall.DOFade(opacity, duration).SetEase(easeType).OnComplete(AllowMovement);
        PlayerControl.Instance.spriteRenderer.sortingOrder = order;
    }
    public void Exit()
    {
        buildingBoundaries.SetActive(false);
        PlayerControl.Instance.SpawnKeepFlip(new Vector3(PlayerControl.Instance.transform.position.x, exitPoint.y));

        PlayerControl.Instance.AllowMovement(false);
        outerWall.DOFade(1, duration).SetEase(easeType).OnComplete(AllowMovement);
        PlayerControl.Instance.spriteRenderer.sortingOrder = originalSortingOrder;
    }
    void AllowMovement()
    {
        PlayerControl.Instance.AllowMovement(true);
    }
}
