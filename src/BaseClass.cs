/***********************************************************/
/* NJAGE Engine - WPF Essentials                           */
/*                                                         */
/* Copyright 2013-2020 Marcel Bulla. All rights reserved.  */
/* Licensed under the MIT License. See LICENSE in the      */
/* project root for license information.                   */
/***********************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using Swordfish.NET.Collections;

namespace De.Markellus.Njage.WpfEssentials
{
    /// <summary>
    /// A Base for WPF model classes.
    /// This can be used as dynamic to add new properties at Runtime, with full XAML binding support.
    /// </summary>
    public abstract class BaseClass : DynamicObject, INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// Saves properties of a class in a dictionary, which will be used to fire events.
        /// </summary>
        protected readonly ConcurrentObservableDictionary<string, object> _values = new ConcurrentObservableDictionary<string, object>();

        #endregion

        #region Properties

        /// <summary>
        /// Configures, if events should be fired. If this gets disabled, XAML-Bindings will no longer receive updates.
        /// </summary>
        protected internal bool AllowRaiseEvent { get; set; } = true;

        #endregion

        #region Event Handlers

        /// <summary>
        /// This event will be fired if a property of this class instance has changed its value, as long as <see cref="AllowRaiseEvent"/> is set to true.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Functions

        /// <summary>
        /// Attempts to add a new member variable or property at runtime to the current object instance.
        /// This method is called automatically when the object is created as "dynamic" and a variable or property is implicitly set.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="value"></param>
        /// <returns>True if a member variable already existed and was updated, otherwise false if the variable was newly
        /// added to the object.</returns>
        /// <exception cref="Exception">This method will raise an exception if <see cref="value"/> has a different type than
        /// the value that is already set.</exception>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (_values.ContainsKey(binder.Name))
            {
                var type = _values[binder.Name].GetType();
                if (value.GetType() == type)
                {
                    SetValue(binder.Name, value);
                    return true;
                }
                else throw new Exception("Value " + value + " is not of type " + type.Name);
            }
            else
            {
                SetValue(binder.Name, value);
                return false;
            }
        }

        /// <summary>
        /// Attempts to read a dynamic member variable or property.
        /// This method is called automatically when the object is created as "dynamic" and a variable or property is implicitly queried.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns>True if the variable was retrieved, otherwise false.</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _values.TryGetValue(binder.Name, out result);
        }

        /// <summary>
        /// Returns a list with all dynamic member Names of this class.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _values.Keys;
        }

        /// <summary>
        /// Adds a new property by its name as string.
        ///
        /// example.AddProperty("Value", 20);
        /// int iValue = example.Value; // iValue = 20
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentException"></exception>
        public void AddProperty(string key, object value = null)
        {
            if (_values.ContainsKey(key))
            {
                throw new ArgumentException("This object already has a property with this name.", key);
            }

            SetValue(key, value);
        }

        /// <summary>
        /// Destroys the calling instance of this class and all attached properties if they implement <see cref="IDisposable"/>.
        /// </summary>
        public void Dispose()
        {
            foreach (IDisposable obj in _values.Values.OfType<IDisposable>())
            {
                obj.Dispose();
            }
        }

        /// <summary>
        /// Retrieves the value of a property of this class.
        /// </summary>
        /// <typeparam name="T">Class type of the property to be retrieved/typeparam>
        /// <param name="key">The name of the property</param>
        /// <returns>The value of the property</returns>
        public T GetValue<T>(string key)
        {
            key = key.ToLower();
            object value = null;

            if (_values.ContainsKey(key))
            {
                value = _values[key];
            }

            if (value is T)
            {
                return (T) value;
            }

            return default(T);
        }

        /// <summary>
        /// Defines the value of a property of this class.
        /// This action triggers the <see cref="PropertyChanged"/> event, unless <see cref="AllowRaiseEvent"/> is set top false.
        /// </summary>
        /// <param name="key">The name of the property</param>
        /// <param name="value">The value of the property</param>
        public virtual void SetValue(string key, object value)
        {
            key = key.ToLower();

            object old = null;

            if (!_values.ContainsKey(key))
            {
                _values.Add(key, value);
            }
            else
            {
                old = _values[key];
                _values[key] = value;
            }

            if (old != value)
            {
                OnPropertyChanged(key, old, value);
            }

        }

        /// <summary>
        /// Determines whether a specified property is defined.
        /// </summary>
        /// <param name="key">The name of the property</param>
        /// <returns>True if the property is defined, otherwise false.</returns>
        protected bool ValueDefined(string key)
        {
            return _values.ContainsKey(key);
        }

        #endregion

        #region Protected Functions

        /// <summary>
        /// Triggers the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the modified property</param>
        /// <param name="oldValue">The old value</param>
        /// <param name="newValue">The new value</param>
        protected void OnPropertyChanged(string propertyName, object oldValue, object newValue)
        {
            if (this.AllowRaiseEvent && !ReferenceEquals(PropertyChanged, null))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
