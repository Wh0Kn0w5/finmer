﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Finmer.Gameplay
{

    /// <summary>
    /// Represents an item in a shop, with all relevant metadata.
    /// </summary>
    public sealed class ShopItemStack : INotifyPropertyChanged
    {

        /// <summary>
        /// Indicates how this stack behaves.
        /// </summary>
        public enum EStackType
        {
            Regular,
            Unique
        }

        /// <summary>
        /// The base item for this stack.
        /// </summary>
        public Item Item { get; }

        /// <summary>
        /// The type of this stack.
        /// </summary>
        public EStackType Type { get; }

        /// <summary>
        /// The number of items in the stack. If equal to -1, quantity is infinite.
        /// </summary>
        public int Quantity
        {
            get => m_Quantity;
            set
            {
                m_Quantity = value;
                OnPropertyChanged();
            }
        }

        private int m_Quantity;

        public ShopItemStack(Item item, EStackType type)
        {
            Item = item;
            Type = type;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
