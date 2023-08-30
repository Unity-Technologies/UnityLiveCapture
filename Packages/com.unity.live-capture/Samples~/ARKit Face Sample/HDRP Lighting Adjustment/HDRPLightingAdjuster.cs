using UnityEngine;
#if HDRP_1_OR_NEWER
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
#endif

public class HDRPLightingAdjuster : MonoBehaviour
{
    void Awake()
    {
#if HDRP_1_OR_NEWER
        float MinimumExposureValue = 10.0f;
        
        GameObject go = new GameObject("AutomaticExposure_Volume");
        Volume vo = go.AddComponent<Volume>();
        var expos =vo.profile.Add<Exposure>(false);    
        
        expos.mode.overrideState = true;
        expos.mode.value = ExposureMode.Automatic;

        expos.limitMin.overrideState = true;
        expos.limitMin.value = MinimumExposureValue;
#endif
    }
}
