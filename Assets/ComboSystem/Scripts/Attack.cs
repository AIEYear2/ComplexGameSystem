using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace ComboSystem
{
    [CreateAssetMenu(menuName = "ComboSystem/AttackTypes/Default")]
    public class Attack : ScriptableObject
    {
        [SerializeField]
        protected Bounds hitBox = new Bounds();
        [SerializeField]
        protected DamageObject damage = null;
        [SerializeField]
        protected AttackRange hitBoxActiveCheck = new AttackRange();

        [Space(10)]
        [SerializeField]
        protected float attackLength = 0;

        [Space(10)]
        [SerializeField]
        protected AnimationClip attackAnimation = null;

        public DamageObject Damage { get => damage; }
        public float AttackLength { get => attackLength; }

        private AnimationClipPlayable clip = new AnimationClipPlayable();

        private bool debug = false;

        public AnimationClipPlayable GetClip()
        {
            AnimationClipPlayable toReturn = clip;
            return toReturn;
        }

        public virtual void InitializeAnimation(ref PlayableGraph graph)
        {
            clip = AnimationClipPlayable.Create(graph, attackAnimation);
        }

        public virtual int PerformAttack(Vector3 offset, ref Collider[] hits, float curAttackPercentComplete, int layerMask)
        {
            if (!hitBoxActiveCheck.AttackColliderActive(curAttackPercentComplete))
            {
                debug = false;
                return 0;
            }

            int tmp = Physics.OverlapBoxNonAlloc(offset + hitBox.center, hitBox.extents, hits);

            debug = true;

            return tmp;
        }

        public Bounds DrawBoundsForDebug()
        {
            if (!debug)
                return new Bounds();

            return hitBox;
        }
    }

    [System.Serializable]
    public class AttackRange
    {
        [SerializeField]
        private float[] timeNodes = new float[0];

        public AttackRange()
        {
        }

        public AttackRange(float[] nodes)
        {
            timeNodes = new float[nodes.Length];

            float tmpInt = 0;
            bool needsSorting = false;

            for (int x = 0; x < timeNodes.Length; ++x)
            {
                timeNodes[x] = Mathf.Clamp01(nodes[x]);

                needsSorting = needsSorting || tmpInt > timeNodes[x];
                tmpInt = timeNodes[x];
            }

            if (needsSorting)
                System.Array.Sort(timeNodes);
        }

        public bool AttackColliderActive(float curAttackPercent)
        {
            if (timeNodes.Length != 0)
            {
                int countSmaller = 0;
                for (int x = 0; x < timeNodes.Length; ++x)
                {
                    if (curAttackPercent < timeNodes[x])
                        return countSmaller % 2 == 0;

                    ++countSmaller;
                }

            }

            return true;
        }
    }
}