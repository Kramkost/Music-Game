using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class MouseFollowerSystem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string targetTag = "Follower";
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private Ease moveEase = Ease.OutQuart; 
    
    [Tooltip("На сколько максимум объект может отойти от начальной точки")]
    [SerializeField] private float maxDistance = 2f;

    private Dictionary<Transform, Vector3> _followerData = new Dictionary<Transform, Vector3>();
    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;
        
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        foreach (var t in targets)
        {

            _followerData.Add(t.transform, t.transform.position);
        }

        DOTween.Init();
    }

    void Update()
    {
        if (_followerData.Count == 0) return;
        MoveObjectsSlightly();
    }

    private void MoveObjectsSlightly()
    {

        Vector3 mousePercent = new Vector3(
            (Input.mousePosition.x / Screen.width) - 0.5f,
            (Input.mousePosition.y / Screen.height) - 0.5f,
            0
        );

        Vector3 offset = mousePercent * maxDistance;

        foreach (var item in _followerData)
        {
            Transform t = item.Key;
            Vector3 startPos = item.Value;

           
            t.DOMove(startPos + offset, duration).SetEase(moveEase);
        }
    }
}