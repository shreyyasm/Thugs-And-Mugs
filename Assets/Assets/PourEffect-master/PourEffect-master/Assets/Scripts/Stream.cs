﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stream : MonoBehaviour
{
   private LineRenderer lineRenderer;

    private ParticleSystem splashParticle;
    private Coroutine pourRoutine;
    private Vector3 targetPosition = Vector3.zero;
    public GameObject Particle;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        splashParticle = GetComponentInChildren<ParticleSystem>();
    }
    private void Start()
    {
        MoveToPosition(0, transform.position);
        MoveToPosition(1, transform.position);
    }
    public void Begin()
    {
        StartCoroutine(UpdateParticle());
        pourRoutine =  StartCoroutine(BeginPour());

    }
    IEnumerator BeginPour()
    {
        while(gameObject.activeSelf)
        {
            targetPosition = FindEndPoint();
            MoveToPosition(0, transform.position);
            AnimateToPosition(1, targetPosition);
            yield return null;
        }
       
    }

    public void End()
    {
        StopCoroutine(pourRoutine);
        pourRoutine = StartCoroutine(EndPour());
    }
    IEnumerator EndPour()
    {
        while(!HasReachedPosition(0,targetPosition))
        {
            AnimateToPosition(0, targetPosition);
            AnimateToPosition(1, targetPosition);

            yield return null;
        }
        Destroy(gameObject);
    }
    public Vector3 FindEndPoint()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, Vector3.down);

        Physics.Raycast(ray, out hit, 2.0f);
        Vector3 endPoint = hit.collider ? hit.point : ray.GetPoint(2.0f);

        return endPoint;
    }
    public void MoveToPosition(int index,Vector3 targetPosition)
    {
        lineRenderer.SetPosition(index, targetPosition);
    }
    
    private void AnimateToPosition(int index, Vector3 targetPosition)
    {
        Vector3 currentPoint = lineRenderer.GetPosition(index);
        Vector3 newPosiion = Vector3.MoveTowards(currentPoint, targetPosition, Time.deltaTime * 1.75f);
        lineRenderer.SetPosition(index, newPosiion);
    }
    private bool HasReachedPosition(int index, Vector3 targetPosition)
    {
        Vector3 currentPosition = lineRenderer.GetPosition(index);
        return currentPosition == targetPosition;
    }
    private IEnumerator UpdateParticle()
    {
        while(gameObject.activeSelf)
        {
            splashParticle.gameObject.transform.position = targetPosition;
            bool isHitting = HasReachedPosition(1,targetPosition);
            splashParticle.gameObject.SetActive(isHitting);

            yield return null;
        }
       
    }

}
