using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace _Scripts.Controller
{
    public class SpawnManager : MonoBehaviour
    {
        [SerializeField] private ARPlaneManager arPlaneManager;
        [SerializeField] private ARRaycastManager arRaycastManager;
        [SerializeField] private ModelController modelController;
        [SerializeField] private ARAnchorManager arAnchorManager;
        private List<ARRaycastHit> _hits = new List<ARRaycastHit>();
        [SerializeField] private GameObject _parent;

        private void Update()
        {
            if (arRaycastManager.enabled == false)
            {
                return;
            }

            if (arRaycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), _hits,
                    TrackableType.PlaneWithinPolygon))
            {
                var hit = _hits[0].pose;
#if !UNITY_EDITOR
                 TryAddAnchor(_hits[0]);
#endif
               
                InstantiateModelInHit(hit);
            }
        }

        private void InstantiateModelInHit(Pose hit)
        {
            GameObject obj = Instantiate(_parent, hit.position, _parent.transform.rotation);
            modelController.InitCube(obj.transform.GetChild(0),obj.transform.GetComponentInChildren<Axes>());
            arRaycastManager.enabled = false;
            arPlaneManager.enabled = false;
        }

        private void TryAddAnchor(ARRaycastHit hit)
        {
            if (hit.trackable is ARPlane plane)
            {
                arAnchorManager.AttachAnchor(plane, hit.pose);
            }
        }
    }
}