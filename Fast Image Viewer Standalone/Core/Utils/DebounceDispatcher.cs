using System;
using System.Timers;

namespace FIVStandard.Utils
{
    public class DebounceDispatcher
    {
        private Timer timer = null;
        private Action methodToExecute;

        public void Debounce(double interval, Action action)
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Elapsed -= MethodToExecute;
                timer = null;
            }

            methodToExecute = action;

            timer = new Timer(interval);
            timer.Elapsed += MethodToExecute;
            timer.Start();
        }

        private void MethodToExecute(object sender, ElapsedEventArgs e)
        {
            methodToExecute.Invoke();
            //prevent neverending loop
            timer.Stop();
        }
    }
}