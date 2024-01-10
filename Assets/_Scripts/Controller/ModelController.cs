using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        [Header("Stats")] [SerializeField] private float sensitivity = 1f;
        [SerializeField] private float ratioScale = 0.01f;
        [SerializeField] private float minScale = 0.05f;
        [SerializeField] private float multiRatioScale = 1f;
        [SerializeField] private float rotationMultiRatio = 0.1f;
        private float _previous;
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

        public void ChangePivot()
        {
            Vector3 _position = _model.position;
            Transform parent = _model.parent;
            float angle = parent.rotation.eulerAngles.y;
            Vector3 local = Quaternion.AngleAxis(-angle, Vector3.up) * _faceVector;
            _model.localPosition = (local / 2) * _model.localScale.x;
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
            Debug.Log(local);
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
                //Debug.Log(direction+"and"+scale);
                _model.parent.localScale += scale * (direction * ratioScale);
            }
        }

        public void ScaleMulti(Vector2 firstPos, Vector2 secondPos)
        {
            float distanceMulti = Vector2.Distance(firstPos, secondPos);
            float check = Mathf.Abs(distanceMulti / _previous - 1);
            if (_previous != 0)
            {
                float minScaleStep = 0.02f;
                if (check > minScaleStep)
                {
                    _model.localScale *= ((distanceMulti / _previous) * multiRatioScale);
                }
            }

            _previous = distanceMulti;
        }

        public void CanclePrevious()
        {
            _previous = 0;
        }

        public void RotateMulti(Vector2 firstDelta, Vector2 secondDelta, Vector2 firstPos, Vector2 secondPos)
        {
            Vector2 directionVector = firstPos.y > secondPos.y ? firstDelta : secondDelta;
            float direction = directionVector.x > 0 ? 1 : -1;
            float rotateFloat = direction * (firstDelta.magnitude + secondDelta.magnitude);
            _model.parent.Rotate(Vector3.up, rotateFloat * rotationMultiRatio);
        }
    }
}