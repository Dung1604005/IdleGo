using System;
using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Play Config")]
    [SerializeField] private Vector3 offSetPlay;

    [SerializeField] private Vector3 rotationEulerPlay;


    [SerializeField] private Transform tfPlayer;

    [SerializeField] private float speed;

    [SerializeField]private Transform tf;

    [SerializeField]private Transform target;

    [SerializeField] private Vector3 offSet;

    [SerializeField] private Vector3 targetOffset;

    

    public void OnInit()
    {
        target = tfPlayer;
    }



    public void SetActive(bool active)
    {
        tf.gameObject.SetActive(active);
    }

    public void OnStartGame()
    {
        SetTargetOffSet(offSetPlay);
    }

    
    public void SetTargetOffSet(Vector3 _targetOffSet)
    {
        targetOffset = _targetOffSet;
    }


    void Awake()
    {
        OnInit();
    }


    void LateUpdate()
    {
        if(target == null)
        {
            return;
        }
        tf.position = offSet + target.position;
        
    }
}
