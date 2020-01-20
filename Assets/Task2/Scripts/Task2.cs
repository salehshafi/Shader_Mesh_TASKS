using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Task2 : MonoBehaviour
{
    public MeshRenderer renderer;

    // UI Handles to test Shader radius and lineWidth variance
    public Slider radiusSlider;
    public Slider lineWidthSlider;


    void Start()
    {
        radiusSlider.value = renderer.material.GetFloat("_Radius");
        lineWidthSlider.value = renderer.material.GetFloat("_LineWidth");
        
    }


    public void RadiusChanged(float newVal)
    {
        renderer.material.SetFloat("_Radius",newVal);
    }

    public void LineWidthChanged(float newVal)
    {
        renderer.material.SetFloat("_LineWidth", newVal);
    }
}
