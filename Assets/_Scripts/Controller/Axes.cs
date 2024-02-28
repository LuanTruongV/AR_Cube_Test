using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Axes : MonoBehaviour
{
    [Serializable]
    struct AxesDirection
    {
        public GameObject obj;
        public Vector3 direction;
    }

    [SerializeField] private List<AxesDirection> _axes;

    public Vector3 Direction(GameObject axesGameObject)
    {
        return _axes.FirstOrDefault(x => x.obj == axesGameObject).direction ;
    }

    public void ShowOneAxes(Vector3 direction)
    {
        foreach (var axes in _axes)
        {
            axes.obj.SetActive(axes.direction==direction);
        }
    }
    public void ShowAllAxes()
    {
        foreach (var axes in _axes)
        {
            axes.obj.SetActive(true);
        }
    }
}
