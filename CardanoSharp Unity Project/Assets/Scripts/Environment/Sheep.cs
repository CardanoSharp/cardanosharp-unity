using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Sheep : MonoBehaviour
{
    [Header("Time to play animation")]
    [SerializeField] private float minTimeToPlay;
    [SerializeField] private float maxTimeToPlay;
    private float timeToPlay;
    private float timer;

    [Header("Tween Settings")]
    [SerializeField] private float variation;
    [SerializeField] private float minDuration;
    [SerializeField] private float maxDuration;

    private void Start()
    {
        timeToPlay = Random.Range(minTimeToPlay, maxTimeToPlay);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if(timer >= timeToPlay)
        {
            RandomRotate();

            timer = 0;
            timeToPlay = Random.Range(minTimeToPlay, maxTimeToPlay);
        }
    }


    void RandomRotate()
    {
        float rot = Random.Range(transform.rotation.y - variation, transform.rotation.y + variation);
        float dur = Random.Range(minDuration, maxDuration);
        transform.DORotate(new Vector3(0, rot, 0), dur).SetEase(Ease.Linear).Play();
    }
}
