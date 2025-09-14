using UnityEngine;



public class ExplodeOnDeath : MonoBehaviour
{
    public Explosion ExplosionPrefab;
    public float explosionDamage = 10f;
    public float explosionRadius = 2f;

    protected DamageOnTouch m_damageOnTouch;



    private void Awake()
    {
        m_damageOnTouch = ExplosionPrefab.GetComponent<DamageOnTouch>();
    }

    private void OnDisable()
    {
        if (ExplosionPrefab == null)
        {
            return;
        }

        if (m_damageOnTouch == null)
        {
            return;
        }

        ExplosionPrefab.explosionRadius = explosionRadius;
        m_damageOnTouch.damage = explosionDamage;
    }
}
