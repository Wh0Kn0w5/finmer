﻿/*
 * FINMER - Interactive Text Adventure
 * Copyright (C) 2019-2022 Nuntis the Wolf.
 *
 * Licensed under the GNU General Public License v3.0 (GPL3). See LICENSE.md for details.
 * SPDX-License-Identifier: GPL-3.0-only
 */

using System;
using System.Globalization;
using System.Text;
using Finmer.Core.Serialization;

namespace Finmer.Core.VisualScripting.Nodes
{

    /// <summary>
    /// Command that wraps SetInventoryEnabled().
    /// </summary>
    public sealed class CommandSetInventoryEnabled : ScriptCommand
    {

        /// <summary>
        /// Gets or sets the wrapped parameter for this command.
        /// </summary>
        public ValueWrapperBool Value { get; set; } = new ValueWrapperBool();

        public override string GetEditorDescription()
        {
            return String.Format(CultureInfo.InvariantCulture, "Set Character Sheet Enabled to {0}", Value.GetOperandDescription());
        }

        public override EColor GetEditorColor()
        {
            return EColor.SceneControl;
        }

        public override void EmitLua(StringBuilder output, IContentStore content)
        {
            output.AppendFormat(CultureInfo.InvariantCulture, "SetInventoryEnabled({0})", Value.GetOperandLuaSnippet());
            output.AppendLine();
        }

        public override void Serialize(IFurballContentWriter outstream)
        {
            Value.Serialize(outstream);
        }

        public override void Deserialize(IFurballContentReader instream, int version)
        {
            Value.Deserialize(instream, version);
        }

    }

}
