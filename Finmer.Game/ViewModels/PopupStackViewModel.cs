﻿/*
 * FINMER - Interactive Text Adventure
 * Copyright (C) 2019-2023 Nuntis the Wolf.
 *
 * Licensed under the GNU General Public License v3.0 (GPL3). See LICENSE.md for details.
 * SPDX-License-Identifier: GPL-3.0-only
 */

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Finmer.Views.Base;

namespace Finmer.ViewModels
{

    /// <summary>
    /// View model for the popup stack.
    /// </summary>
    public class PopupStackViewModel : BaseProp
    {

        /// <summary>
        /// Collection of popups visible in this stack.
        /// </summary>
        public ObservableCollection<StackablePopupBase> Elements { get; set; } = new ObservableCollection<StackablePopupBase>();

        /// <summary>
        /// Whether any popups are open.
        /// </summary>
        public bool HasAnyOpenPopups => Elements.Any(popup => popup.Host != null);

        public PopupStackViewModel()
        {
            CollectionChangedEventManager.AddHandler(Elements, Elements_OnCollectionChanged);
        }

        public void OnPopupClosing()
        {
            OnPropertyChanged(nameof(HasAnyOpenPopups));
        }

        private void Elements_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasAnyOpenPopups));
        }

    }

}
