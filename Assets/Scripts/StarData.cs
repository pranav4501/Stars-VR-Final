using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class StarData{

    public float absoluteMag;
    public float relativeMag;
    public Vector3 position;
    public Vector3 originalPosition;
    public string spectralType;
    public float dist;
    public float vx;    
    public float vy;
    public float vz;
    public GameObject instance;
}