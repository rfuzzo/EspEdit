﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspEdit.Services
{
    public interface ISettingsService
    {
        /// <summary>
        /// Assigns a value to a settings key.
        /// </summary>
        /// <typeparam name="T">The type of the object bound to the key.</typeparam>
        /// <param name="key">The key to check.</param>
        /// <param name="value">The value to assign to the setting key.</param>
        void SetValue<T>(string key, T value);

        /// <summary>
        /// Reads a value from the current <see cref="IServiceProvider"/> instance and returns its casting in the right type.
        /// </summary>
        /// <typeparam name="T">The type of the object to retrieve.</typeparam>
        /// <param name="key">The key associated to the requested object.</param>
        [Pure]
        T GetValue<T>(string key);
    }

    public sealed class SettingsService : ISettingsService
    {
        /// <summary>
        /// The <see cref="IPropertySet"/> with the settings targeted by the current instance.
        /// </summary>
        //private readonly IPropertySet SettingsStorage = ApplicationData.Current.LocalSettings.Values;

        /// <inheritdoc/>
        public void SetValue<T>(string key, T value)
        {
            //if (!SettingsStorage.ContainsKey(key))
            //{
            //    SettingsStorage.Add(key, value);
            //}
            //else
            //{
            //    SettingsStorage[key] = value;
            //}
        }

        /// <inheritdoc/>
        public T GetValue<T>(string key)
        {
            //return SettingsStorage.TryGetValue(key, out object value) ? (T)value : default;
            throw new NotImplementedException();
        }
    }
}