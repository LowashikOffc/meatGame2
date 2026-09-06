using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Cyl
{
    public Transform c;
    public Transform p1;
    public Transform p2;
}

public class Rope : MonoBehaviour
{
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Transform _endPoint;

    [Range(2, 50)] [SerializeField] private int _segmentCount = 10;
    [SerializeField] private float _sphereRadius;
    [SerializeField] private float _cylinderRadius;

    [SerializeField] private LayerMask _layer;
    [SerializeField] private LayerMask _excludeLayer;

    [SerializeField] private Material _material;
    [SerializeField] private bool _isCollide;

    [SerializeField] private float _spring = 1000f;
    [SerializeField] private float _damper;
    [SerializeField] private float _minDistance;
    [SerializeField] private float _maxDistance;

    [SerializeField] private List<Transform> _spheres = new List<Transform>();
    [SerializeField] private List<Cyl> _cylinders = new List<Cyl>();

    [SerializeField] private Transform _sphereFolder;
    [SerializeField] private Transform _cylinderFolder;

    [SerializeField] private Vector3 v;

    private Transform CreateSphere(int index)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Sphere_" + index.ToString();
        sphere.transform.parent = _sphereFolder;
        sphere.layer = (int)Mathf.Log(_layer.value, 2);
        sphere.transform.localScale = Vector3.one * _sphereRadius;
        sphere.transform.position = _startPoint.position;
        sphere.GetComponent<Renderer>().material = _material;
        sphere.GetComponent<Collider>().enabled = _isCollide;
        sphere.GetComponent<Collider>().excludeLayers = _excludeLayer;
        Rigidbody rb = sphere.AddComponent<Rigidbody>();
        rb.excludeLayers = _layer;
        rb.drag = 0.2f;
        //rb.freezeRotation = true;

        SpringJoint spring = sphere.AddComponent<SpringJoint>();
        spring.damper = _damper;
        spring.minDistance = _minDistance;
        spring.maxDistance = _maxDistance;
        spring.spring = _spring;

        return sphere.transform;
    }

    private Cyl CreateCylinder(int index)
    {
        Cyl cyl = new Cyl();

        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = "Cylinder_" + index.ToString();
        cylinder.transform.parent = _cylinderFolder;
        cylinder.layer = (int)Mathf.Log(_layer.value, 2);
        cylinder.transform.localScale = Vector3.one * _sphereRadius;
        cylinder.transform.position = _startPoint.position;
        cylinder.GetComponent<Renderer>().material = _material;
        cylinder.GetComponent<Collider>().enabled = _isCollide;
        cylinder.GetComponent<Collider>().excludeLayers = _excludeLayer;

        cyl.c = cylinder.transform;

        return cyl;
    }

    private void SetCylinder(int index, Cyl cyl, Transform currentSphere, Transform overrideSphere)
    {
        int lastIndex = index - 1;
        int nextIndex = index + 1;

        if (index == 0)
        {
            cyl.p2 = _startPoint;
            cyl.p1 = currentSphere; // Используем переданную сферу
        }
        else if (index == _segmentCount - 1)
        {
            cyl.p1 = _spheres[index]; // Для последнего сфера уже в списке? Нет, тоже проблема!
            cyl.p2 = _endPoint;
        }
        else
        {
            cyl.p1 = _spheres[lastIndex];
            cyl.p2 = currentSphere; // Используем переданную сферу
        }
    }

    private void ChangeSegmentsByIndex(int index, List<Transform> list, Transform sphere)
    {
        SpringJoint j = sphere.GetComponent<SpringJoint>();
        if (index > 0)
        {
            j.autoConfigureConnectedAnchor = false;
            j.connectedBody = list[index - 1].GetComponent<Rigidbody>();
        }
        else j.connectedBody = _startPoint.GetComponent<Rigidbody>();

        if (index == _segmentCount - 1)
        {
            //Debug.Log(sphere);

            SpringJoint spring = _endPoint.AddComponent<SpringJoint>();
            spring.damper = _damper;
            spring.minDistance = _minDistance;
            spring.maxDistance = _maxDistance;
            spring.spring = _spring;
            spring.autoConfigureConnectedAnchor = false;

            spring.connectedBody = sphere.GetComponent<Rigidbody>();
        }
    }

    private void Start()
    {
        // 1. Создаём ВСЕ сферы
        for (int i = 0; i < _segmentCount; i++)
        {
            Transform sphere = CreateSphere(i);
            _spheres.Add(sphere);
        }

        // 2. Настраиваем соединения для всех сфер
        for (int i = 0; i < _segmentCount; i++)
        {
            ChangeSegmentsByIndex(i, _spheres, _spheres[i]);
        }

        // 3. Создаём ВСЕ цилиндры (теперь все сферы в списке)
        for (int i = 0; i < _segmentCount; i++)
        {
            Cyl cylinder = CreateCylinder(i);
            SetCylinder(i, cylinder, _spheres[i], null);
            _cylinders.Add(cylinder);
        }
        Cyl cylinder2 = CreateCylinder(_segmentCount+1);
        cylinder2.p1 = _spheres[_segmentCount - 1];
        cylinder2.p2 = _spheres[_segmentCount - 2];
        _cylinders.Add(cylinder2);
    }

    private void Update()
    {
        foreach (Cyl cyl in _cylinders)
        {
            cyl.c.transform.position = (cyl.p1.position + cyl.p2.position)/2;
            cyl.c.transform.LookAt(cyl.p2);
            cyl.c.transform.Rotate(90, 0, 0);
            float distance = Vector3.Distance(cyl.p1.position, cyl.p2.position)/2;
            cyl.c.localScale = new Vector3(_sphereRadius, distance, _sphereRadius);
        }
    }
}