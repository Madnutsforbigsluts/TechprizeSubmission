namespace Glyph.Utilities
{
    public class Timer
    {
        public float duration;

        private float _currentTime; 

        public float currentTime
        {
            get { return _currentTime; }
            set
            {
                if (_currentTime >= duration)
                {
                    timeExpired = true;
                }
                _currentTime = value; 
            }
        }

        public bool timeExpired { get; private set; }

        public Timer(float duration = float.MaxValue)
        {
            this.duration = duration;
        }

        public void Update(float currentTime)
        {
            this.currentTime += currentTime;
        }

        public void Reset()
        {
            timeExpired = false;
            currentTime = 0; 
        }

    }
}