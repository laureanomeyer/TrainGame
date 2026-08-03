using UnityEngine;

[CreateAssetMenu(fileName = "NewBulletStats", menuName = "Bullets")]
public class BulletTypeScriptable : ScriptableObject
{
    [Header("Bullet mesh")]
    [SerializeField] public Mesh bulletMesh;

    [Header("Bullet material")]
    [SerializeField] public Material bulletMaterial;

    [Header("Bullet speed")]
    [SerializeField] public float speed;

    private float damage;
    public float Damage { get { return damage; } set { damage = value; } }

    [Header("Bullet duration")]
    [SerializeField] public float duration;

    [Header("TypeOfCollsion")]
    [SerializeField] public BulletCollsionTypeSO typeOfCollsion;

    [Header("DestroyOnCollsion")]
    [SerializeField] public bool destroyOnEnemy;
}
