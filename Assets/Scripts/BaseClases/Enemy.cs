using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : Shotable
{
    [Header("Enemy Properties")]
    [SerializeField] protected int healt;
    [SerializeField] protected int speed;

    // Unity Methods
    public abstract void Update();

    // Scential Methods For The Enemies
    protected abstract void AttackDefault();
    public abstract bool BarringtoniaDamage();
    public abstract bool GetTangled();

    // Scential Conditions for the Enemy
    protected abstract void StateHealth();

    protected bool IsPlayerInVisionCone(Transform player, float visionConeAngle)
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angleBetweenEnemyAndPlayer = Vector3.Angle(transform.forward, dirToPlayer);
        return angleBetweenEnemyAndPlayer < visionConeAngle;
    }

    protected bool IsPlayerBehindWall(Transform player, float sightRange, LayerMask whatIsGround)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, (player.position - transform.position).normalized, out hit, sightRange, whatIsGround))
        {
            if (hit.transform != player)
            {
                return true; // Hay una pared entre el enemigo y el jugador
            }
        }
        return false; // No hay pared entre el enemigo y el jugador
    }
}
