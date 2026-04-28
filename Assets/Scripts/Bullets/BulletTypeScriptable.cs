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

    [Header("Bullet damage")]
    [SerializeField] public float damage;

    [Header("Bullet duration")]
    [SerializeField] public float duration;

    [Header("DestroyOnCollsion")]
    [SerializeField] public bool destroyOnEnemy;
}
