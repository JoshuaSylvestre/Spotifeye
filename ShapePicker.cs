using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore.Examples.HelloAR;

public class ShapePicker : MonoBehaviour {

    public GameObject picker;
    public GameObject lineEQ;
    public GameObject ringEQ;

    public void setLineEQ()
    {
        picker.GetComponent<HelloARController>().AndyPlanePrefab = Instantiate(lineEQ, transform.position, Quaternion.identity);
    }

    public void setRingEQ()
    {
        picker.GetComponent<HelloARController>().AndyPlanePrefab = Instantiate(ringEQ, transform.position, Quaternion.identity);
    }

}
