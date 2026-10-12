using UnityEngine;

//capaz sirve mas que sea un SO..... ver a futuro

public class EnemyWeapon : MonoBehaviour
{
    [SerializeField] private GameObject bulletType;
    Transform bulletSpawn;
    bool canShoot = true;

    Camera Cam => Camera.main;
    public bool CanShoot => canShoot;

    private void Awake()
    {
        EventBus.Subscribe<OnSetEnemiesFlags>(SetCanShoot);
        bulletSpawn = GetComponentInChildren<Transform>();
    }

    void OnDestroy()
    {
        EventBus.Unsubscribe<OnSetEnemiesFlags>(SetCanShoot);
    }

    public bool Execute(IWagon target, float damage)
    {
        if (!canShoot) return false;

        return Shoot(target, damage);
    }

    void SetCanShoot(OnSetEnemiesFlags ev)
    {
        this.canShoot = ev.canAttack;
    }

    public bool Shoot(IWagon target, float damage)
    {
        if (target == null ||
            (target is Object unityObject && unityObject == null) ||
            target.Head == null || target.Tail == null)
        {
            return false;
        }
        if (bulletType == null)
        {
            Debug.LogError($"Enemy weapon '{name}' has no bullet prefab.", this);
            return false;
        }

        if (bulletSpawn == null)
        {
            Debug.LogError($"Enemy weapon '{name}' has no bullet spawn transform.", this);
            return false;
        }

        Vector3 targetPosition = target.Middle;
        Vector3 dir = (targetPosition - bulletSpawn.position).normalized;

        //GameObject bulletGO = ObjectPoolManager.SpawnObject(bulletType, bulletSpawn.position, Quaternion.LookRotation(dir));
        GameObject bulletGO = Instantiate(bulletType, bulletSpawn.position, Quaternion.LookRotation(dir));

        EnemyBullet bullet = bulletGO.GetComponent<EnemyBullet>();
        if (bullet == null)
        {
            Debug.LogError($"Enemy weapon '{name}' bullet prefab has no EnemyBullet component.", bulletGO);
            Destroy(bulletGO);
            return false;
        }

        bullet.Init(dir, damage);
        bool isOnScreen = CameraView.IsInsideCamera(transform.position,Cam);

        PlayAudio(isOnScreen);
        return true;

    }

        private void PlayAudio(bool isOnScreen)
    {
        AudioManager.Instance.PlayOnScreen("SFXEnemyShot", isOnScreen);
    }


}
