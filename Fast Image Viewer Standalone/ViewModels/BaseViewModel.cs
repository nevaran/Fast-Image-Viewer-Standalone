using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FIVStandard
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Takes, by reference, a field, and its new value. If field != value, will set field = value and raise a PropertyChanged notification
        /// </summary>
        /// <typeparam name="T">Type of field being set and notified</typeparam>
        /// <param name="field">Field to assign</param>
        /// <param name="value">Value to assign to the field, if it differs</param>
        /// <param name="propertyName">Name of the property to notify for. Defaults to the calling property</param>
        /// <returns>True if field != value and a notification was raised; false otherwise</returns>
        protected virtual bool SetAndNotify<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}