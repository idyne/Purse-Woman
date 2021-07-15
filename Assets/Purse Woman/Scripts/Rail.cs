using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rail : MonoBehaviour
{
    [SerializeField] private float width = 1;
    [SerializeField] private Transform leftCylinder, rightCylinder;
    private float leftX, rightX;

    public float Width { get => width; }
    public float LeftX { get => leftX; }
    public float RightX { get => rightX; }

    private void Awake()
    {
        leftCylinder.transform.localPosition = new Vector3(-width / 2f, leftCylinder.transform.localPosition.y, leftCylinder.transform.localPosition.z);
        rightCylinder.transform.localPosition = new Vector3(width / 2f, rightCylinder.transform.localPosition.y, rightCylinder.transform.localPosition.z);
        leftX = leftCylinder.transform.position.x;
        rightX = rightCylinder.transform.position.x;
    }
}
