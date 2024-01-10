using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.Controller
{
    public class InputController : MonoBehaviour
    {
        [SerializeField] private ModelController _modelController;
        private MobileInput _mobileInput;
        private InputAction _tap;
        private InputAction _touchPosition;
        private InputAction _touchDelta;
        private InputAction _secondTouch;
        private InputAction _firstPosition;
        private InputAction _firstDelta;
        private InputAction _secondPosition;
        private InputAction _secondDelta;
        private bool _isHold;
        private Vector2 _touchPositionVector;
        private bool _isMulti =false;
        private void Awake()
        {
            _mobileInput = new MobileInput();
        }

        private void OnEnable()
        {
            InitialInput();
        }

        private void InitialInput()
        {
            _tap = _mobileInput.Touch.Tap;
            _tap.Enable();
            _touchPosition = _mobileInput.Touch.TouchPostion;
            _touchPosition.Enable();
            _touchDelta = _mobileInput.Touch.TouchDelta;
            _touchDelta.Enable();
            _secondTouch=_mobileInput.Touch.SecondTouch;
            _secondTouch.Enable();
            _firstDelta = _mobileInput.Touch.FirstDelta;
            _firstDelta.Enable();
            _secondDelta = _mobileInput.Touch.SecondDelta;
            _secondDelta.Enable();
            _firstPosition = _mobileInput.Touch.FirstPostion;
            _firstPosition.Enable();
            _secondPosition = _mobileInput.Touch.SecondPostion;
            _secondPosition.Enable();
            _tap.performed += HandheldTapPerformed;
            _tap.canceled += HandleTapCanceled;
            _secondTouch.performed += HandleSecondPerformed;
            _secondTouch.canceled += HandleSecondCanceled;
        }

        private void OnDisable()
        {
            DisableInput();
        }
        private void DisableInput()
        {
            _tap.Disable();
            _touchPosition.Disable();
            _touchDelta.Disable();
            _secondTouch.Disable();
            _firstDelta.Disable();
            _secondDelta.Disable();
            _firstPosition.Disable();
            _secondPosition.Disable();
            _tap.performed -= HandheldTapPerformed;
            _tap.canceled -= HandleTapCanceled;
            _secondTouch.performed -= HandleSecondPerformed;
            _secondTouch.canceled -= HandleSecondCanceled;
        }
        private void HandheldTapPerformed(InputAction.CallbackContext obj)
        {
            _isHold = true;
            _touchPositionVector = _touchPosition.ReadValue<Vector2>();
            if (_modelController.CheckRaycatModel(_touchPositionVector))
            {
                StartCoroutine(CheckHold(Time.time));
            };
            
            
        }
        
        private void HandleTapCanceled(InputAction.CallbackContext obj)
        {
            _isHold = false;
            _modelController.UnScaleFace();
        }
        private void HandleSecondPerformed(InputAction.CallbackContext obj)
        {
            _isMulti = true;
            StartCoroutine(Multi());
        }
        private void HandleSecondCanceled(InputAction.CallbackContext obj)
        {
            _isMulti = false;
            _modelController.CanclePrevious();
        }
        private IEnumerator Multi()
        {
        
            while (_isMulti)
            {
                Vector2 firstDelta = _firstDelta.ReadValue<Vector2>();
                Vector2 secondDelta = _secondDelta.ReadValue<Vector2>();
                Vector2 firstPos = _firstPosition.ReadValue<Vector2>();
                Vector2 secondPos = _secondPosition.ReadValue<Vector2>();
                if (CheckScaleMultiInput(firstDelta, secondDelta))
                {
                    _modelController.ScaleMulti(firstPos,secondPos);
                }
                else
                {
                    if (CheckRotationMultiInput(firstDelta, secondDelta))
                    {
                        _modelController.RotateMulti(firstDelta,secondDelta,firstPos,secondPos);
                    }
                }

                yield return null;
            }
        }
        

        

        private bool CheckScaleMultiInput(Vector2 first,Vector2 second)
        {
            return first.x * first.y >= 0 && second.x * second.y >= 0;
        }

        private bool CheckRotationMultiInput(Vector2 first, Vector2 second)
        {
            return first.x * second.x < 0;
        }
        
        private IEnumerator CheckHold(float time)
        {
            while (_isHold && Time.time - time < 0.5f)
            {
                Vector2 distanceVector = _touchPosition.ReadValue<Vector2>() - _touchPositionVector;
                if (distanceVector.magnitude > 10f)
                {
                    _isHold = false;
                    StartCoroutine(HandleTouchMoving());
                }
                yield return null;
            }
            if (_isHold)
            {
                StartCoroutine(HandleHolding());
            }
        }

        private IEnumerator HandleHolding()
        {
             _modelController.ChangePivot();
            while (_isHold)
            {
                yield return new WaitUntil(() => !_isMulti);
                _modelController.ScaleFaceModel(_touchDelta.ReadValue<Vector2>());
                yield return null;
            }
        }

        private IEnumerator HandleTouchMoving()
        {
            while (!_isHold)
            {
                yield return new WaitUntil(() => !_isMulti);
                _modelController.Moving(_touchDelta.ReadValue<Vector2>());
                
                yield return null;
            }
        }
        
    }
    
}
