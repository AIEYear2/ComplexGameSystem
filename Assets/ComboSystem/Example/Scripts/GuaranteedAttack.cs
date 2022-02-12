using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ComboSystem;

public class GuaranteedAttack : Attack
{
    public override int PerformAttack(Vector3 offset, ref Collider[] hits, float curAttackPercentComplete, int layerMask)
    {
        return 1;
    }
}
