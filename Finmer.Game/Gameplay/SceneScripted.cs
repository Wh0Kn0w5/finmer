﻿/*
 * FINMER - Interactive Text Adventure
 * Copyright (C) 2019-2021 Nuntis the Wolf.
 *
 * Licensed under the GNU General Public License v3.0 (GPL3). See LICENSE.md for details.
 * SPDX-License-Identifier: GPL-3.0-only
 */

#if DEBUG
#define LUA_TIMINGS
#endif

using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using Finmer.Core;
using Finmer.Core.Assets;
using Finmer.Gameplay.Scripting;
using Finmer.Models;
using Finmer.Utility;
using static Finmer.Gameplay.Scripting.LuaApi;

namespace Finmer.Gameplay
{

    /// <summary>
    /// Represents a scene with functionality implemented in Lua scripts.
    /// </summary>
    internal sealed class SceneScripted : Scene
    {

        private const string k_SceneEnvsTable = "SceneEnvs";

        private readonly ScriptContext m_Context;
        private readonly string m_SceneFile;
        private readonly int m_SceneRef;

        private IntPtr m_Coroutine = IntPtr.Zero;
        private bool m_HasError = true;

        public SceneScripted(ScriptContext context, string scenefile)
        {
            m_Context = context;
            m_SceneFile = scenefile;

#if LUA_TIMINGS
            var sw = Stopwatch.StartNew();
#endif

            // Find the scene asset and get its script
            AssetBase asset = GameController.Content.GetAssetByName(scenefile);

            // Let's throw some exceptions if they're not proper scenes
            if (!(asset is AssetScene scene))
            {
                throw new ArgumentException($"Failed to load scene '{scenefile}': The specified asset does not exist, or is not a Scene.", nameof(scenefile));
            }

            if (scene.Inject)
            {
                throw new ArgumentException($"Failed to load scene '{scenefile}': The specified asset is a patch scene and cannot be loaded directly.", nameof(scenefile));
            }

            // Load the chunk
            CompiledScript script = scene.PrecompiledScript;
            if (!context.LoadScript(script, scenefile))
                return;

#if LUA_TIMINGS
            double time_comp = sw.Elapsed.TotalMilliseconds;
            sw.Restart();
#endif

            // Prepare a sandbox environment for the script. We will override the __index metamethod to refer to the real global environment,
            // but not the __newindex metamethod so scripts can modify their environment as they like. Essentially, this makes _G read-only.
            IntPtr state = context.LuaState;
            luaL_newmetatable(state, k_SceneEnvsTable);
            lua_createtable(state, 0, 1);

            // Make the built-in '_G' variable refer to the scene-specific environment, instead of the global environment. Note that users can
            // escape this sandbox by setting _G to nil, at which point they'll go through the metatable and can query the value of the 'real'
            // _G table. A fix would be to wrap it in a double metatable, but from a performance standpoint, this is preferable.
            lua_pushvalue(state, -1);
            lua_setfield(state, -2, "_G");

            // Prepare metatable for the sandbox environment
            lua_createtable(state, 0, 1);
            lua_getglobal(state, "_G"); // set global env as parent lookup env 
            lua_setfield(state, -2, "__index");
            lua_setmetatable(state, -2); // set env metatable

            // Write the environment table to the registry, so it can accessed again later
            lua_pushvalue(state, -1);
            m_SceneRef = luaL_ref(state, -3); // save into scene table (and pop)

            // Sandbox the main chunk
            lua_setfenv(state, -3);
            lua_pop(state, 1); // remove SceneEnvs table

#if LUA_TIMINGS
            double time_sandbox = sw.Elapsed.TotalMilliseconds;
            sw.Restart();
#endif

            // Run it
            if (!context.RunProtectedCall(0, 0))
                return;

#if LUA_TIMINGS
            sw.Stop();
            GameUI.Instance.Log($"Timings for {scenefile}: {time_comp + time_sandbox + sw.Elapsed.TotalMilliseconds:F2} ms total: Load = {time_comp:F2} ms, Sandboxing = {time_sandbox:F2} ms, Run = {sw.Elapsed.TotalMilliseconds:F2} ms", Theme.LogColorDarkCyan);
#endif

            // All done, the scene is ready!
            m_HasError = false;
        }

        /// <summary>
        /// Starts or resumes an existing coroutine set to m_Coroutine.
        /// </summary>
        /// <remarks>
        /// For details on passing arguments, see LaunchCoroutine().
        /// </remarks>
        /// <param name="numArgs">Number of arguments to pass to the function.</param>
        private void ResumeCoroutineWithArgs(int numArgs)
        {
            // Run script
            int result = lua_resume(m_Coroutine, numArgs);

            // If the script yielded, we'll resume it later (on a call to ResumeCoroutine()), so there's nothing more to do here.
            if (result == LUA_YIELD)
                return;

            // Error handling
            IntPtr state = m_Context.LuaState;
            if (result != 0)
            {
                // Prevent further callbacks, since the script is now in an undefined state
                m_HasError = true;

                // Note: there's no need to pop the error message or clean the stack at all, because we will destroy the coroutine entirely
                Debug.Assert(lua_isstring(m_Coroutine, -1));
                string errormsg = lua_tostring(m_Coroutine, -1);
                if (!errormsg.Contains("ScriptStopSignal")) // Avoid printing errors for the scene switch signal (see ExportedSetScene)
                    GameUI.Instance.Log($"ERROR: Script error in scene '{m_SceneFile}': {errormsg}", Theme.LogColorError);
            }

            // Remove the thread object from the main thread's stack (it should be on the top still). This will make it eligible for GC.
            for (int i = 1, c = lua_gettop(state); i <= c; i++)
            {
                if (lua_type(state, i) != ELuaType.Thread || lua_tothread(state, i) != m_Coroutine)
                    continue;

                // Remove the coroutine thread from the main thread stack
                lua_remove(state, i);
                m_Coroutine = IntPtr.Zero;
                break;
            }

            Debug.Assert(m_Coroutine == IntPtr.Zero, "Coroutine thread was not found on main stack");
        }

        /// <summary>
        /// Calls a global function defined by this scene's script.
        /// </summary>
        /// <remarks>
        /// Accepts a number of arguments that must already be passed on the Lua stack, in the same way they would be pushed on the stack for
        /// a native lua_call invocation. The pushed arguments are always consumed, regardless of whether or not the script call succeeds.
        /// </remarks>
        /// <param name="name">Name of the function to invoke.</param>
        /// <param name="numArgs">Number of arguments to pass to the function.</param>
        private void LaunchCoroutine(string name, int numArgs)
        {
            // This function must not be called from the main thread, because scripts expect to be able to sleep or call blocking functions
            Debug.Assert(Thread.CurrentThread != Application.Current.Dispatcher.Thread);

            // If the scene is in an invalid state, just get rid of the function arguments and bail
            IntPtr state = m_Context.LuaState;
            if (m_HasError)
            {
                lua_pop(state, numArgs);
                return;
            }

            // Create a new coroutine, which will remain at the top of the main thread stack until it finishes
            Debug.Assert(m_Coroutine == IntPtr.Zero);
            m_Coroutine = lua_newthread(state);

            // Move the user arguments to the thread
            if (numArgs > 0)
            {
                lua_insert(state, -numArgs - 1); // Move the thread below the args, so the args are at the top
                lua_xmove(state, m_Coroutine, numArgs);
            }

            // Retrieve the scene environment from the registry, so we can access its globals
            luaL_newmetatable(m_Coroutine, k_SceneEnvsTable);
            lua_rawgeti(m_Coroutine, -1, m_SceneRef);
            lua_getfield(m_Coroutine, -1, name);

            // Check that the global we just accessed is in fact a global function
            if (lua_type(m_Coroutine, -1) != ELuaType.Function)
            {
                // If not, destroy the coroutine and bail
                lua_pop(state, 1);
                return;
            }

            // Otherwise, insert the function below the arguments (since that is the order that lua_call expects)
            lua_insert(m_Coroutine, -3 - numArgs);
            lua_pop(m_Coroutine, 2); // remove env table and registry table

            // Run it
            Debug.Assert(lua_isfunction(m_Coroutine, 1));
            ResumeCoroutineWithArgs(numArgs);
        }

        public override void Enter()
        {
            // Update player location for scripts
            GameController.Session.Player.Location = m_SceneFile;

            // Run callback
            lua_pushnumber(m_Context.LuaState, 123.0);
            lua_pushboolean(m_Context.LuaState, 1);
            LaunchCoroutine("OnEnter", 2);
        }

        public override void Leave()
        {
            // Run callback
            LaunchCoroutine("OnLeave", 0);

            // Remove this scene's environment from the environment table, so it can be collected
            IntPtr state = m_Context.LuaState;
            luaL_newmetatable(state, k_SceneEnvsTable);
            luaL_unref(state, -1, m_SceneRef);
            lua_pop(state, 1);
        }

        public override void Turn(int choice)
        {
            // If a paused coroutine exists, resume it now
            if (m_Coroutine != IntPtr.Zero)
            {
                ResumeCoroutineWithArgs(0);
                return;
            }

            // Otherwise, start a new one by running
            lua_pushnumber(m_Context.LuaState, choice);
            LaunchCoroutine("OnTurn", 1);
        }

    }

}
