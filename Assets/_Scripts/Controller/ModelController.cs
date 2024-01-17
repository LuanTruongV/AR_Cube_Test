using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Controller
{
    public class ModelController : MonoBehaviour
    {
        [Header("Object")] [SerializeField] private Transform _model;
        public Transform Model
        {
            set
            {
                if (value != null)
                {
                    _model = value;
                }
                
            }
        }


        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask cubeLayer;
        [Header("Stats")] 
        [SerializeField] private float sensitivity = 1f;
        [SerializeField] private float ratioScale = 0.1f;
        [SerializeField] private float minScale = 0.05f;
        [SerializeField] private float multiScaleRatio = 0.1f;
        [SerializeField] private float multiRotationRatio = 5f;
        private RaycastHit _hit;
        private Vector3 _faceVector;

        public bool CheckRaycatModel(Vector3 touchPosition)
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
            ChangePivot((local/2)*_model.localScale.x);
        }

        public void BackPivot()
        {
            ChangePivot(new Vector3(0,0.5f,0));
        }

        private void ChangePivot(Vector3 newPivot)
        {
            Vector3 _position = _model.position;
            Transform parent = _model.parent;
            _model.localPosition=newPivot;
            _model.DetachChildren();
            parent.position += (_position - _model.position);
            _model.parent = parent;
        }

        public void UnScaleFace()
        {
            _faceVector = Vector3.zero;
        }

        public void ScaleFaceModel(Vector2 delta)
        {
            float angle = _model.parent.rotation.eulerAngles.y;
            Vector3 local = Quaternion.AngleAxis(-angle, Vector3.up) * _faceVector;
            Vector3 scale = local.y != 0
                ? local * delta.y
                : local * delta.x;

            float scaleFloat, parentFloat;
            if (scale.x != 0)
            {
                scaleFloat = scale.x;
                parentFloat = _model.parent.localScale.x;
            }
            else
            {
                scaleFloat = scale.y != 0 ? scale.y : scale.z;
                parentFloat = scale.y != 0 ? _model.parent.localScale.y : _model.parent.localScale.z;
            }
            if (parentFloat > minScale || scaleFloat > 0)
            {
                
                int direction = Vector3.Angle(mainCamera.transform.forward, _faceVector) > 0 ? 1 : -1;
                _model.parent.localScale += scale * (direction * ratioScale);
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
            parent.transform.rotation=Quaternion.Euler(0,parent.transform.rotation.eulerAngles.y-Mathf.Sign(angle)*multiRotationRatio,0);
        }

    }
}