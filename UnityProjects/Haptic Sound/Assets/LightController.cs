using System;
using System.Collections;
using System.Collections.Generic;
using MixedReality.Toolkit.UX;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class LightController : MonoBehaviour
{
    [SerializeField] private Light bulbLight;
    [SerializeField] private PressableButton lightToggleButton;
    [SerializeField] private PressableButton hslToggleButton;
    [SerializeField] private Slider hueSlider, saturationSlider, lightSlider;
    [SerializeField] private Canvas canvas;
    
    private bool _isLightOn;
    private bool _isHSLOn;
    private float _accumulator;
    
    // Start is called before the first frame update
    void Start()
    {
       lightToggleButton.OnClicked.AddListener(ToggleLight);
       hslToggleButton.OnClicked.AddListener(ToggleHSL);
       hueSlider.OnValueUpdated.AddListener(UpdateLight);
       saturationSlider.OnValueUpdated.AddListener(UpdateLight);
       lightSlider.OnValueUpdated.AddListener(UpdateLight);
       ToggleLight();
       ToggleHSL(false);
    }

    private void UpdateLight(SliderEventData eventData)
    {
        bulbLight.color = Color.HSVToRGB(
            hueSlider.Value, saturationSlider.Value, 1);
        bulbLight.intensity = lightSlider.Value;
    }
    
    
    private void ToggleLight()
    {
        if (!_isLightOn)
        {
            ToggleHSL(false);
        }
        _isLightOn = !_isLightOn;
        lightToggleButton.GetComponentInChildren<FontIconSelector>().CurrentIconName = _isLightOn ? "Icon 24" : "Icon 128";
        bulbLight.enabled = !_isLightOn;
        hslToggleButton.gameObject.SetActive(!_isLightOn);
    }

    private void ToggleHSL()
    {
        _isHSLOn = !_isHSLOn;
        canvas.enabled = _isHSLOn;
    }
    private void ToggleHSL(bool isOn)
    {
        _isHSLOn = isOn;
        canvas.enabled = isOn;
    }
    
}
