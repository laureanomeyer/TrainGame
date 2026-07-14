using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraShakes : MonoBehaviour
{
    public static CameraShakes Instance { get; private set; }
    private CinemachineCamera virtualCamera;

    [Header ("Shoot Shake")]
    [SerializeField, Range(0, 1)] float  shootTimer;
    [SerializeField, Range(0, 5)] float shootIntensity;

    [Header ("Shields Broken Shake")]
    [SerializeField, Range(0, 1)] float  shieldsTimer;
    [SerializeField, Range(0, 5)] float shieldsIntensity;

    [Header("Coal Empty Shake")]
    [SerializeField, Range(0, 1)] float coalTimer;
    [SerializeField, Range(0, 5)] float coalIntensity;

    [Header("Wagon Broken Shake")]
    [SerializeField, Range(0, 1)] float wagonTimer;
    [SerializeField, Range(0, 5)] float wagonIntensity;

    private void Awake()
    {
        Instance = this;
        virtualCamera = GetComponent<CinemachineCamera>();    
    }

    private void Start()
    {
        EventBus.Subscribe<OnCoalEmptyEvent>(CoalRunOutShake);
        EventBus.Subscribe<OnShootEvent>(ShootCameraShake);
        EventBus.Subscribe<OnShieldsBrokenEvent>(ShieldsRunOutShake);
        EventBus.Subscribe<OnWagonDestroyedEvent>(WagonDestroyedShake);
    }
    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnCoalEmptyEvent>(CoalRunOutShake);
        EventBus.Unsubscribe<OnShootEvent>(ShootCameraShake);
        EventBus.Unsubscribe<OnShieldsBrokenEvent>(ShieldsRunOutShake);
        EventBus.Unsubscribe<OnWagonDestroyedEvent>(WagonDestroyedShake);

        StopAllCoroutines();
    }

    private IEnumerator DoCameraShake(float time, float intensity)
    {
        CinemachineBasicMultiChannelPerlin noise =
            virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

        noise.AmplitudeGain = intensity;
        yield return new WaitForSeconds(time);
        noise.AmplitudeGain = 0;
    }

    public void ShootCameraShake(OnShootEvent shootEvent)
    {
        StopAllCoroutines();
        StartCoroutine(DoCameraShake(shootTimer, shootIntensity));
    }

    public void ShieldsRunOutShake(OnShieldsBrokenEvent sheldBrokenEvent)
    {
        StopAllCoroutines();
        StartCoroutine(DoCameraShake(shieldsTimer, shieldsIntensity));
    }

    public void CoalRunOutShake(OnCoalEmptyEvent coalEmptyEvent)
    {
        StopAllCoroutines();
        StartCoroutine(DoCameraShake(coalTimer, coalIntensity));
    }

    public void WagonDestroyedShake(OnWagonDestroyedEvent wagonDestroyEvent)
    {
        StopAllCoroutines();
        StartCoroutine(DoCameraShake(wagonTimer, wagonIntensity));
    }


}
