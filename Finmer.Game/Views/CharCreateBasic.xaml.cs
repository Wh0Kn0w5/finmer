﻿/*
 * FINMER - Interactive Text Adventure
 * Copyright (C) 2019-2021 Nuntis the Wolf.
 *
 * Licensed under the GNU General Public License v3.0 (GPL3). See LICENSE.md for details.
 * SPDX-License-Identifier: GPL-3.0-only
 */

using System;
using System.Windows;
using System.Windows.Controls;
using Finmer.Core;

namespace Finmer.Views
{

    /// <summary>
    /// Interaction logic for CharCreateBasic.xaml
    /// </summary>
    public partial class CharCreateBasic
    {

        private bool m_CanGoNext;
        private bool m_Setup = true;

        public CharCreateBasic()
        {
            InitializeComponent();
        }

        public override bool CanGoNext => m_CanGoNext;

        private void CharCreateViewBase_Loaded(object sender, RoutedEventArgs e)
        {
            NameInput.Text = InitialSaveData.GetString("name");
            SpeciesInput.Text = InitialSaveData.GetString("species");
            if (!String.IsNullOrWhiteSpace(InitialSaveData.GetString("gender")))
            {
                GenderInputMale.IsChecked = InitialSaveData.GetString("gender").Equals("Male");
                GenderInputFemale.IsChecked = !GenderInputMale.IsChecked;
            }
            m_Setup = false;

            // Apply some default settings to enable quickly clicking through to the game
            //if (GameController.DebugMode)
            {
                NameInput.Text = "Snack";
                SpeciesInput.SelectedIndex = CoreUtility.Rng.Next(SpeciesInput.Items.Count);
                // Make a coin flip to see if it's male or female randomly selected, then set the gender accordingly.
                int gender_flip = CoreUtility.Rng.Next(2);
                GenderInputMale.IsChecked = (gender_flip == 0);
                GenderInputFemale.IsChecked = !GenderInputMale.IsChecked;
                ValidateForm();
            }
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_Setup)
                return;

            InitialSaveData.SetString("name", NameInput.Text);
            ValidateForm();
        }

        private void cmbSpecies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_Setup) 
                return;

            InitialSaveData.SetString("species", (string)SpeciesInput.SelectedValue);
            ValidateForm();
        }

        private void optGender_Checked(object sender, RoutedEventArgs e)
        {
            if (m_Setup) 
                return;

            InitialSaveData.SetString("gender", (GenderInputMale.IsChecked ?? false) ? "Male" : "Female");
            ValidateForm();
        }

        private void ValidateForm()
        {
            m_CanGoNext =
                !String.IsNullOrWhiteSpace(InitialSaveData.GetString("name")) && 
                !String.IsNullOrWhiteSpace(InitialSaveData.GetString("species")) &&
                !String.IsNullOrWhiteSpace(InitialSaveData.GetString("gender"));
            OnPropertyChanged(nameof(CanGoNext));
        }

    }

}
