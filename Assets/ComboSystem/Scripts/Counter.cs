using UnityEngine;

namespace ComboSystem
{
    [System.Serializable]
    public class Counter
    {
        [Tooltip("the Maximum value this counter can reach")]
        [SerializeField]
        protected float max;
        protected float cur;

        /// <summary>
        /// Max value of the counter
        /// </summary>
        public float Max { get => max; }
        /// <summary>
        /// The current value of the counter
        /// </summary>
        public float Cur { get => cur; }

        /// <summary>
        /// The percent left in the counter
        /// </summary>
        public float PercentComplete
        {
            get => cur / max;
        }
        /// <summary>
        /// how much is left in the counter
        /// </summary>
        public float AmountRemaining
        {
            get => max - cur;
        }

        public Counter()
        {
            cur = 0;
            max = 0;
        }

        /// <summary>
        /// Creates a counter where Cur is set to 0 and the length is max
        /// </summary>
        /// <param name="max">Max value of the counter</param>
        public Counter(float max)
        {
            this.cur = 0;
            this.max = max;
        }


        public void SetMax(float newMax, bool resetCur = false)
        {
            max = newMax;

            if (resetCur)
                cur = 0.0f;
        }

        /// <summary>
        /// Reset the counter to the startPoint
        /// </summary>
        /// <param name="startPoint">The value you want the counter to get set to</param>
        public void Reset(float startPoint = 0)
        {
            cur = startPoint;
        }

        /// <summary>
        /// Increases counter by 1
        /// </summary>
        public virtual void Count()
        {
            ++cur;
            cur = Mathf.Clamp(cur, 0, max);
        }

        /// <summary>
        /// Increases counter by value
        /// </summary>
        /// <param name="value">The float value you want to add to counter</param>
        public void Count(float value)
        {
            cur += value;
            cur = Mathf.Clamp(cur, 0, max);
        }

        /// <summary>
        /// Lerps towards Max from 0
        /// </summary>
        /// <param name="increment">how far towards Max Cur is</param>
        public void Lerp(float increment)
        {
            cur = max * increment;
        }
        /// <summary>
        /// Lerps towards Max from min
        /// </summary>
        /// <param name="min">value Cur starts at for the lerp</param>
        /// <param name="increment">how far towards Max Cur is</param>
        public void Lerp(float min, float increment)
        {
            cur = min + ((max - min) * increment);
        }
        /// <summary>
        /// Lerps towards max from min
        /// </summary>
        /// <param name="min">value Cur starts at for the lerp</param>
        /// <param name="max">value Cur lerps towards cannot be larger than Max</param>
        /// <param name="increment">how far towards max Cur is</param>
        public void Lerp(float min, float max, float increment)
        {
            if (max > this.max)
            {
                Debug.LogWarning("lerp max is greater than counter max, clamping to counter max");
                max = this.max;
            }

            cur = min + ((max - min) * increment);
        }
        /// <summary>
        /// Lerps away from Max towards 0
        /// </summary>
        /// <param name="increment">how far away from Max Cur is</param>
        public void InverseLerp(float increment)
        {
            cur = max + (-max * increment);
        }
        /// <summary>
        /// Lerps away from Max towards min
        /// </summary>
        /// <param name="min">value Cur lerps towards</param>
        /// <param name="increment">how far towards min Cur is</param>
        public void InverseLerp(float min, float increment)
        {
            cur = max + ((min - max) * increment);
        }
        /// <summary>
        /// Lerps away from max towards min
        /// </summary>
        /// <param name="min">value Cur lerps towards</param>
        /// <param name="max">value Cur lerps away from cannot be larger than Max</param>
        /// <param name="increment">how far towards min Cur is</param>
        public void InverseLerp(float min, float max, float increment)
        {
            if (max > this.max)
            {
                Debug.LogWarning("lerp max is greater than counter max, clamping to counter max");
                max = this.max;
            }

            cur = max + ((min - max) * increment);
        }

        /// <summary>
        /// Checks to see if the counter has reached or passed the max
        /// </summary>
        /// <param name="resetOnTrue">Whether you want the counter to reset when true</param>
        /// <returns>Returns true if cur is greater than or equal to max</returns>
        public bool IsComplete(bool resetOnTrue = true)
        {
            if (cur >= max)
            {
                if (resetOnTrue)
                    Reset();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks whether the counter has reached or passed the max and if not count up
        /// </summary>
        /// <param name="resetOnTrue">Whether you want the counter to reset when it returns true</param>
        /// <returns>Returns true if cur is greater than or equal to max</returns>
        public bool Check(bool resetOnTrue = true)
        {
            if (IsComplete(resetOnTrue))
                return true;

            Count();

            return false;
        }
        /// <summary>
        /// Checks whether the counter has reached or passed the max and if not count up by value
        /// </summary>
        /// <param name="value">value to count up by</param>
        /// <param name="reset">Whether you want the counter to reset when it returns true</param>
        /// <returns>Returns true if cur is greater than or equal to max</returns>
        public bool Check(float value, bool reset = true)
        {
            if (IsComplete(reset))
                return true;

            Count(value);

            return false;
        }

        /// <summary>
        /// Counts up by the value then checks whether the counter has reached or passed the max
        /// </summary>
        /// <param name="reset">Whether you want the counter to reset when it returns true</param>
        /// <returns>Returns true if cur is greater than or equal to max</returns>
        public bool PreCheck(bool reset = true)
        {
            Count();

            if (IsComplete(reset))
                return true;

            return false;
        }
        /// <summary>
        /// Counts up by the value then checks whether the counter has reached or passed the max
        /// </summary>
        /// <param name="value">value to count up by</param>
        /// <param name="reset">Whether you want the counter to reset when it returns true</param>
        /// <returns>Returns true if cur is greater than or equal to max</returns>
        public bool PreCheck(float value, bool reset = true)
        {
            Count(value);

            if (IsComplete(reset))
                return true;

            return false;
        }

        public static implicit operator bool(Counter counter)
        {
            return counter.cur >= counter.max;
        }
    }
    /// <summary>
    /// Used as an actual timer or can be used as a counter
    /// </summary>
    [System.Serializable]
    public class Timer : Counter
    {
        /// <summary>
        /// The current value of the timer
        /// </summary>
        public float Time { get => cur; }
        /// <summary>
        /// Max value of the timer
        /// </summary>
        public float Delay { get => max; }
        /// <summary>
        /// how much time is left in the timer
        /// </summary>
        public float TimeRemaining
        {
            get => max - cur;
        }

        public Timer() : base()
        {
        }

        /// <summary>
        /// Creates a counter where Cur is set to 0 and the length is max
        /// </summary>
        /// <param name="max">Max value of the counter</param>
        public Timer(float max) : base(max)
        {
        }

        /// <summary>
        /// Increases timer by Time.deltaTime
        /// </summary>
        public override void Count()
        {
            cur += UnityEngine.Time.deltaTime;
            cur = Mathf.Clamp(cur, 0, max);
        }
    }
}