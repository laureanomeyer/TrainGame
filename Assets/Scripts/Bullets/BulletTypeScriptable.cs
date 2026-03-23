using UnityEngine;

[CreateAssetMenu(fileName = "NewBulletStats", menuName = "Bullets")]
public class BulletTypeScriptable : ScriptableObject
{
    [Header("Bullet mesh")]
    [SerializeField] public Mesh bulletMesh;

    [Header("Bullet speed")]
    [SerializeField] public float speed;

    [Header("Bullet damage")]
    [SerializeField] public int damage;

    [Header("Bullet duration")]
    [SerializeField] public float duration;
}
