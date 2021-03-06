﻿using HtcPlugin.Lua.MySql.Models;
using HtcSharp.Core;
using HtcSharp.Core.Logging.Abstractions;
using HtcSharp.Core.Plugin.Abstractions;
using Microsoft.Extensions.Logging;
using MoonSharp.Interpreter;

namespace HtcPlugin.Lua.MySql {
    public class MySqlRegister {
        private readonly Processor.LuaProcessor _luaProcessor;

        public MySqlRegister(LuaMySql pluginContext) {
            foreach (var plugin in pluginContext.PluginServerContext.PluginManager.GetPlugins()) {
                if (plugin is Processor.LuaProcessor processor) _luaProcessor = processor;
            }
            if (_luaProcessor == null) {
                pluginContext.Logger.LogError("Failed to initialize HtcPlugin.Lua.MySql, dependency context not found.");
                pluginContext.PluginServerContext.PluginManager.UnLoadPlugin(pluginContext.Name).GetAwaiter().GetResult();
                return;
            }
            UserData.RegisterType<LuaSql>();
            UserData.RegisterType<LuaSqlConnection>();
            UserData.RegisterType<LuaSqlCommand>();
            UserData.RegisterType<LuaSqlTransaction>();
            UserData.RegisterType<LuaSqlDataReader>();
            UserData.RegisterType<LuaSqlParameters>();
            _luaProcessor.LuaLowLevelAccess.RegisterLowLevelClass("mysql", new LuaSql());
        }

        public void Uninitialized() {
            _luaProcessor.LuaLowLevelAccess.RemoveLowLevelClass("mysql");
        }
    }
}
