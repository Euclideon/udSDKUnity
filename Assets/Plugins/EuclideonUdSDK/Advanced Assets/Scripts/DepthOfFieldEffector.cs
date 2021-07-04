using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public enum ChangeMode 
{
    Immediate, 
    Lerp 
}

[RequireComponent(typeof(PostProcessVolume))]
public class DepthOfFieldEffector : MonoBehaviour
{
    [Tooltip("Control the focal distance with mouse scroll.")]
    public bool scrollControl = true; 
    [Tooltip("Mode of changing depth of field.")]
    public ChangeMode changeMode = ChangeMode.Immediate; 
    [Tooltip("If change mode is lerp, the speed at which to lerp.")]
    public float lerpSpeed = 0.5f ; 

    // dof post effect
    DepthOfField depthOfFieldEffect = null;

    // values and delta values
    float focalLengthValue, dFocalLengthValue;  
    float apertureValue, dApertureValue; 
    float focusDistanceValue, dFocusDistanceValue; 

   
    void Start()
    {
        PostProcessVolume volume = gameObject.GetComponent<PostProcessVolume>();
        volume.profile.TryGetSettings(out depthOfFieldEffect);

        focalLengthValue   = dFocalLengthValue   = depthOfFieldEffect.focalLength.value ; 
        apertureValue      = dApertureValue      = depthOfFieldEffect.aperture.value ; 
        focusDistanceValue = dFocusDistanceValue = depthOfFieldEffect.focusDistance.value ; 
    }

    void Update()
    {
        if(depthOfFieldEffect == null)
            return; 

        switch(changeMode)
        {
            case ChangeMode.Immediate:
                return; 
            case ChangeMode.Lerp:
                LerpAndChangeValue(ref focalLengthValue,   ref dFocalLengthValue,   ref depthOfFieldEffect.focalLength.value);
                LerpAndChangeValue(ref apertureValue,      ref dApertureValue,      ref depthOfFieldEffect.aperture.value);
                LerpAndChangeValue(ref focusDistanceValue, ref dFocusDistanceValue, ref depthOfFieldEffect.focusDistance.value);
                break; 
        }

        if(scrollControl)
            SetFocusDistance(dFocusDistanceValue + Input.mouseScrollDelta.y);
    }

    void LerpAndChangeValue(ref float currentValue, ref float deltaValue, ref float destinationValue)
    {
        // exit early if the values match 
        if(currentValue == deltaValue)
            return; 

        currentValue = Mathf.Lerp(currentValue, deltaValue, Time.deltaTime*lerpSpeed);

        // clamp if the value is close 
        if(Mathf.Approximately(currentValue, deltaValue))
            currentValue = deltaValue; 

        destinationValue = currentValue; 
    }

    public void SetFocalLength(float value)
    {
        switch(changeMode)
        {
            case ChangeMode.Immediate:
                depthOfFieldEffect.focalLength.value = value; 
                focalLengthValue = dFocalLengthValue = value; 
                break; 
            case ChangeMode.Lerp: 
                dFocalLengthValue = value; 
                break; 
        }
    }

    public void SetAperture(float value)
    {
        switch(changeMode)
        {
            case ChangeMode.Immediate:
                depthOfFieldEffect.aperture.value = value; 
                apertureValue = dApertureValue = value; 
                break; 
            case ChangeMode.Lerp: 
                dApertureValue = value; 
                break; 
        }
    }

    public void SetFocusDistance(float value)
    {
        switch(changeMode)
        {
            case ChangeMode.Immediate:
                depthOfFieldEffect.focusDistance.value = value;
                focusDistanceValue = dFocusDistanceValue = value; 
                break; 
            case ChangeMode.Lerp: 
                dFocusDistanceValue = value; 
                break; 
        } 
    }
}
