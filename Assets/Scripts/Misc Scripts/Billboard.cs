﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard2 : MonoBehaviour
{
    [SerializeField] Transform camTransform;

    Quaternion originalRotation;

    void Start()
    {
        originalRotation = transform.rotation;
    }

    void Update()
    {
        transform.rotation = camTransform.rotation * originalRotation;
        //transform.LookAt()
    }
}