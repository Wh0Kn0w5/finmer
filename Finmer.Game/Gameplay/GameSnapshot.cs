﻿/*
 * FINMER - Interactive Text Adventure
 * Copyright (C) 2019-2021 Nuntis the Wolf.
 *
 * Licensed under the GNU General Public License v3.0 (GPL3). See LICENSE.md for details.
 * SPDX-License-Identifier: GPL-3.0-only
 */

using Finmer.Core;

namespace Finmer.Gameplay
{

    /// <summary>
    /// Represents serialized save data that can be reconstructed into a GameSession instance.
    /// </summary>
    public sealed class GameSnapshot
    {

        /// <summary>
        /// Snapshot of the player and world state.
        /// </summary>
        public PropertyBag PlayerData { get; }

        /// <summary>
        /// Data to be used for restoring the scene system.
        /// </summary>
        public PropertyBag SceneData { get; }

        public GameSnapshot(PropertyBag player, PropertyBag scene)
        {
            PlayerData = player;
            SceneData = scene;
        }

    }

}
