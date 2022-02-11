using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace ComboSystem
{
    public class ComboController : MonoBehaviour
    {
        public IAttacker attackManager = null;

        public MoveSet moveSet = null;
        public KeyCode[] inputs = new KeyCode[0];
        [SerializeField]
        protected int hitColliderCount = 1;
        [SerializeField]
        protected Timer comboClearTimer = new Timer(0.7f);
        [SerializeField]
        protected LayerMask hitableMask = new LayerMask();

        [Space(10)]
        [SerializeField]
        protected Animator anim;

        protected PlayableGraph playableGraph;
        protected AnimationPlayableOutput playableOutput;

        protected bool comboAttackActive = false;
        protected List<KeyCode> currentCombo = new List<KeyCode>();

        protected Timer currentAttackTimer = new Timer();

        protected int comboAttack = -1;
        protected Attack curAttack = null;

        protected Collider[] hitCols = new Collider[1];

        private void Start()
        {
            // Generate the playable graph
            playableGraph = PlayableGraph.Create();
            playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            // Generate the playableOutput
            playableOutput = AnimationPlayableOutput.Create(playableGraph, "ComboAnimation", anim);

            moveSet.Initialize(ref playableGraph);

            hitCols = new Collider[hitColliderCount];
        }

        private void Update()
        {
            // Resets the currentCombo and begins the combo attack if one was found
            if (currentCombo.Count != 0 && comboClearTimer.Check())
                ClearCombo();

            // Performs the current attack, if there is one
            if (curAttack)
                ManageAttack();

            // pauses the input for the combo system while a combo attack is active
            if (comboAttackActive)
                return;

            // gathers the input for the Combo System
            if (Input.anyKeyDown)
                ManageInput();
        }

        protected virtual void ClearCombo()
        {
            currentCombo.Clear();

            if (comboAttack == -1)
                return;

            EndAttack();

            curAttack = moveSet.GetComboAttack(comboAttack);

            comboAttack = -1;
            comboAttackActive = true;

            BeginAttack();
        }

        protected virtual void ManageAttack()
        {
            if (currentAttackTimer.Check())
            {
                EndAttack();
                return;
            }

            if (attackManager == null)
                Debug.LogError("You have not assigned ComboController.attackManager");

            int hitCount = curAttack.PerformAttack(transform.position, ref hitCols, currentAttackTimer.PercentComplete, hitableMask);

            if (hitCount > 0)
            {
                attackManager.ProcessHits(hitCount, hitCols, curAttack.Damage);
            }
        }
        protected virtual void ManageInput()
        {
            for (int x = 0; x < inputs.Length; ++x)
            {
                if (Input.GetKeyDown(inputs[x]))
                {
                    currentCombo.Add(inputs[x]);

                    comboAttack = moveSet.ProcessCombo(currentCombo.ToArray(), ref curAttack);

                    if (curAttack)
                        BeginAttack();

                    comboClearTimer.Reset();
                    return;
                }
            }
        }

        // Initializes the attack
        protected virtual void BeginAttack()
        {
            currentAttackTimer.SetMax(curAttack.AttackLength, true);

            // Connect the Playable to an output
            playableOutput.SetSourcePlayable(curAttack.GetClip());
            // Plays the Graph.
            playableGraph.Play();
        }
        // De-initializes the attack
        protected virtual void EndAttack()
        {
            comboAttackActive = false;
            playableGraph.Stop();
            curAttack = null;
        }

        private void OnDrawGizmos()
        {
            if (!curAttack)
                return;

            Bounds bounds = curAttack.DrawBoundsForDebug();

            Gizmos.DrawWireCube(transform.position + bounds.center, bounds.size);
        }

        private void OnDestroy()
        {
            playableGraph.Destroy();
        }
    }

    public interface IAttacker 
    {
        public void ProcessHits(int hitCount, Collider[] hits, DamageObject damage);
    }
}
