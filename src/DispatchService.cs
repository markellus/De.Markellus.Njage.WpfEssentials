/***********************************************************/
/* NJAGE Engine - WPF Essentials                           */
/*                                                         */
/* Copyright 2013-2020 Marcel Bulla. All rights reserved.  */
/* Licensed under the MIT License. See LICENSE in the      */
/* project root for license information.                   */
/***********************************************************/

using System;
using System.Windows;
using System.Windows.Threading;

namespace De.Markellus.Njage.WpfEssentials
{
    /// <summary>
    /// Global access to the ui dispatcher
    /// </summary>
    public static class DispatchService
    {
        /// <summary>
        /// Invokes code into the ui thread.
        /// </summary>
        /// <param name="action">The code</param>
        public static void Invoke(Action action)
        {
            Dispatcher dispatchObject = Application.Current?.Dispatcher;
            try
            {
                dispatchObject?.Invoke(action);
            }
            catch { }
        }
    }
}
