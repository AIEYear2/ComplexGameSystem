using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ComboSystem
{
    [CreateAssetMenu(menuName = "ComboSystem/DamageTypes/Default")]
    public class DamageObject : ScriptableObject
    {
        [SerializeField]
        protected float baseDamage = 0;

        public float BaseDamage
        {
            get => baseDamage;
        }
    }
}
