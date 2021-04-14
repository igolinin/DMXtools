using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
[ExecuteAlways]

public class StayInCamera : MonoBehaviour
{
    Transform mainCamera;
    [SerializeField]
    float offset;

    void OnEnable()
    {
        mainCamera = GameObject.FindGameObjectsWithTag("MainCamera")[0].transform;
        transform.position = mainCamera.position + mainCamera.forward * offset;
        transform.rotation = new Quaternion(0.0f, mainCamera.rotation.y, 0.0f, mainCamera.rotation.w);
    }
        void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = mainCamera.position + mainCamera.forward * offset;
        transform.rotation = new Quaternion(0.0f, mainCamera.rotation.y, 0.0f, mainCamera.rotation.w);
    }
}
