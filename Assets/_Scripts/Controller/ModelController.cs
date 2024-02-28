using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Controller
{
    public class ModelController : MonoBehaviour
    {
        [Header("Object")] [SerializeField] private Transform _model;
        
      

        [SerializeField] private Axes _axes;



        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask cubeLayer;
        [SerializeField] private LayerMask axesLayer;
        [Header("Stats")] 
        [SerializeField] private float sensitivity = 1f;
        [SerializeField] private float ratioScale = 0.1f;
        [SerializeField] private float minScale = 0.05f;
        [SerializeField] private float multiScaleRatio = 0.1f;
        [SerializeField] private float multiRotationRatio = 5f;
        [SerializeField] private float ratioAxes = 0.1f;
        
        private RaycastHit _hit;
        private Vector3 _faceVector;
        private Vector3 _directionAxes;


        public void InitCube(Transform model,Axes axes)
        {
            _model = model;
            _axes = axes;
            UnScaleAxes(_model.parent.localScale);
        }

        public bool CheckRayModel(Vector3 touchPosition)
        {
            Ray ray = mainCamera.ScreenPointToRay(touchPosition);
            float maxDistanceRay = 10f;
            if (Physics.Raycast(ray, out _hit, maxDistanceRay, cubeLayer))
            {
                _faceVector = _hit.normal;
                return true;
            }

            return false;
        }

        public bool CheckAxesMove(Vector3 touchPosition)
        {
            Ray ray = mainCamera.ScreenPointToRay(touchPosition);
            float maxDistanceRay = 10f;
            if (Physics.Raycast(ray, out _hit, maxDistanceRay, axesLayer))
            {
                _directionAxes = _axes.Direction(_hit.transform.gameObject);
                _axes.ShowOneAxes(_directionAxes);
                return true;
            }

            return false;
        }

        public void Moving(Vector3 touchDelta)
        {
            var parent = _model.parent;
            var screenPoint = mainCamera.WorldToScreenPoint(parent.position);
            screenPoint += touchDelta * sensitivity;
            parent.position = mainCamera.ScreenToWorldPoint(screenPoint);
        }

        public void ChangePivotToScaleFace()
        {
            float angle = _model.parent.rotation.eulerAngles.y;
            Vector3 local = Quaternion.AngleAxis(-angle, Vector3.up) * _faceVector;
            ChangePivot((local / 2) * _model.localScale.x);
            SetActiveAxes(false);
        }

        public void BackPivot()
        {
            ChangePivot(new Vector3(0, 0.5f, 0));
            SetActiveAxes(true);
        }

        private void ChangePivot(Vector3 newPivot)
        {
            Vector3 _position = _model.position;
            Transform parent = _model.parent;
            _model.localPosition = newPivot;
            _model.DetachChildren();
            parent.position += (_position - _model.position);
            _model.parent = parent;
        }

        public void BackScaleAndShowAxes()
        {
            _faceVector = Vector3.zero;
            _axes.ShowAllAxes();
        }

        public void ScaleFaceModel(Vector2 delta)
        {
            float angle = _model.parent.rotation.eulerAngles.y;
            Vector3 local = Quaternion.AngleAxis(-angle, Vector3.up) * _faceVector;
            var mainCameraTransform = mainCamera.transform;
            int directionXZ =
                Vector3.SignedAngle(mainCameraTransform.forward, _faceVector, mainCameraTransform.up) > 0 ? 1 : -1;
            int directionY =
                Vector3.SignedAngle(mainCameraTransform.forward, _faceVector, mainCameraTransform.right) > 0 ? -1 : 1;
            PositiveVector3(ref local);
            Vector3 scale = local.y != 0
                ? local * (delta.y * directionY)
                : local * (delta.x * directionXZ);
            scale *= ratioScale;
            if (CheckScale(_model.parent.localScale, scale, minScale))
            {
                _model.parent.localScale += scale;
                UnScaleAxes(_model.parent.localScale);
            }
        }
        private bool CheckScale(Vector3 localScale, Vector3 scale, float min)
        {
            Vector3 newLocalScale = localScale + scale;
            if (newLocalScale.x < min || newLocalScale.y < min || newLocalScale.z < min)
            {
                return false;
            }

            return true;
        }
        private void PositiveVector3(ref Vector3 vector)
        {
            if (vector.x < 0 || vector.y < 0 || vector.z < 0)
            {
                vector = -vector;
            }
        }

        public void ScaleMulti(float disDiff)
        {
            Vector3 newScale = _model.transform.localScale + Vector3.one * (Mathf.Sign(disDiff) * multiScaleRatio);

            float speedLerp = 0.05f;
            _model.transform.localScale = Vector3.Lerp(_model.transform.localScale, newScale, speedLerp);
        }

        public void RotateMulti(float angle)
        {
            var parent = _model.parent;
            parent.transform.rotation = Quaternion.Euler(0,
                parent.transform.rotation.eulerAngles.y - Mathf.Sign(angle) * multiRotationRatio, 0);
        }

        private void SetActiveAxes(bool isActive)
        { 
            _axes.gameObject.SetActive(isActive);
        }

        private void UnScaleAxes(Vector3 scale)
        {
            _axes.transform.localScale=new Vector3(1/scale.x,1/scale.y,1/scale.z);
        }

        public void MoveAxes(Vector3 delta)
        {
            Transform parent = _model.parent;
            Vector3 screenPoint = mainCamera.WorldToScreenPoint(parent.position);
            screenPoint += delta * sensitivity;
            Vector3 posTouch = mainCamera.ScreenToWorldPoint(screenPoint);
            float x = (posTouch.x * _directionAxes.x != 0) ? posTouch.x : parent.position.x;
            float y = (posTouch.y * _directionAxes.y != 0) ? posTouch.y : parent.position.y;
            float z = (posTouch.z * _directionAxes.z != 0) ? posTouch.z : parent.position.z;
            Debug.Log(_directionAxes+" va "+new Vector3(x,y,z));
            parent.position = new Vector3(x, z, z);



        }
    }
}