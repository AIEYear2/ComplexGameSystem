using System;
using UnityEngine;
using UnityEngine.Playables;

namespace ComboSystem
{
    [CreateAssetMenu(menuName = "ComboSystem/MoveSets/Default")]
    public class MoveSet : ScriptableObject
    {
        [SerializeField]
        protected SimpleMove[] basicMoves = new SimpleMove[0];
        [SerializeField]
        protected ComboMove[] comboMoves = new ComboMove[0];

        public SimpleMove[] BasicMoves => basicMoves;
        public ComboMove[] ComboMoves => comboMoves;

        // initialize all the animations to be connected to the referenced PlayableGraph
        public virtual void Initialize(ref PlayableGraph graph) // TODO: Attach to static class that calls initialize on ApplicationStart [RuntimeInitializeOnLoadMethod] 
        {
            // Sort the combos by how many keys it needs to fire off, the more keys, the earlier it appears in the array
            Array.Sort(comboMoves, (x, y) => y.ComboStringLength.CompareTo(x.ComboStringLength));

            // "Condenses" the code into one for loop rather than having one for each
            if (basicMoves.Length > comboMoves.Length)
            {
                for (int x = 0; x < basicMoves.Length; ++x)
                {
                    if (x < comboMoves.Length)
                        comboMoves[x].AttackObj.InitializeAnimation(ref graph);

                    for (int y = 0; y < basicMoves[x].AttackCount; ++y)
                        basicMoves[x].GetAttack(y).InitializeAnimation(ref graph);
                }

                return;
            }
            for (int x = 0; x < comboMoves.Length; ++x)
            {
                if (x < basicMoves.Length)
                {
                    for (int y = 0; y < basicMoves[x].AttackCount; ++y)
                        basicMoves[x].GetAttack(y).InitializeAnimation(ref graph);
                }

                comboMoves[x].AttackObj.InitializeAnimation(ref graph);
            }
        }

        public virtual int ProcessCombo(KeyCode[] curCombo, ref Attack basicAttack)
        {
            // Grabs the basic attack from basicMoves if there is one
            for (int x = 0; x < basicMoves.Length; ++x)
            {
                if (basicMoves[x].MoveTest(curCombo[curCombo.Length - 1]))
                {
                    basicAttack = basicMoves[x].GetAttack(FindAttackId(x, curCombo));
                    break;
                }
            }

            // determines if the current combo matches a combo attack
            for (int x = 0; x < comboMoves.Length; ++x)
            {
                if (comboMoves[x].MoveTest(curCombo))
                    return x;
            }

            return -1;
        }

        // Gets the attack relevant to the basic attack that was found
        private int FindAttackId(int moveIndex, KeyCode[] curCombo)
        {
            int toReturn = 0;

            for (int x = curCombo.Length - 1; x > 0; --x)
            {
                if (curCombo[x] != basicMoves[moveIndex].AttackKey)
                    break;

                ++toReturn;
            }

            return toReturn % basicMoves[moveIndex].AttackCount;
        }

        // Gets the combo attack from the array when the combo ends
        public Attack GetComboAttack(int index)
        {
            return comboMoves[index].AttackObj;
        }

        #region Move Classes
        [System.Serializable]
        public class SimpleMove
        {
            [SerializeField]
            private string name = "Simple Move";
            [SerializeField]
            private KeyCode attackKey = 0;
            [SerializeField]
            protected Attack[] attack = new Attack[0];

            public SimpleMove()
            {
            }

            public SimpleMove(KeyCode attackKey, Attack[] attack)
            {
                this.attackKey = attackKey;
                this.attack = attack;
            }

            public int AttackCount { get => attack.Length; }
            public KeyCode AttackKey { get => attackKey; }

            public Attack GetAttack(int index)
            {
                return attack[index];
            }

            public bool MoveTest(KeyCode curKey)
            {
                return curKey == attackKey;
            }
        }
        [System.Serializable]
        public class ComboMove
        {
            [SerializeField]
            private string name = "Combo Move";
            [SerializeField]
            private KeyCode[] comboString = new KeyCode[0];
            [SerializeField]
            private Attack attack = null;

            public Attack AttackObj { get => attack; }
            public int ComboStringLength { get => comboString.Length; }

            public ComboMove()
            {
            }

            public ComboMove(Attack attack, KeyCode[] comboString)
            {
                this.attack = attack;
                this.comboString = comboString;
            }

            public bool MoveTest(KeyCode[] curCombo)
            {
                if (curCombo.Length < comboString.Length)
                    return false;

                for (int x = 0; x < comboString.Length; ++x)
                {
                    if (comboString[x] != curCombo[x])
                        return false;
                }

                return true;
            }
        }
        #endregion
    }
}