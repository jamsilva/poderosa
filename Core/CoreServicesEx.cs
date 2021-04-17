// Copyright 2004-2017 The Poderosa Project.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Text;

using Poderosa.Forms;
using Poderosa.Preferences;
using Poderosa.Commands;
using Poderosa.Sessions;
using Poderosa.Boot;
#if !LIBRARY
using Poderosa.Serializing;
#endif

namespace Poderosa.Plugins {
    //頻出機能へのアクセサ。PoderosaWorldからGetAdapterできるようにしてしまうよ！
    /// <summary>
    /// <ja>
    /// 標準プラグインが備える代表的なインターフェイスや拡張ポイントを返すプロパティを提供します。
    /// </ja>
    /// <en>
    /// Property that returns a typical interface and the extension point with which a standard plug-in provides is offered. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// ICoreServicesインターフェイスは、<seealso cref="IPoderosaWorld">IPoderosaWorldインターフェイス</seealso>
    /// の<see cref="IAdaptable.GetAdapter">GetAdapterメソッド</see>で取得できます。
    /// </ja>
    /// <en>
    /// The ICoreServices interface can be got in the <see cref="IAdaptable.GetAdapter">GetAdapter method</see> of the <seealso cref="IPoderosaWorld">IPoderosaWorld interface</seealso>. 
    /// </en>
    /// </remarks>
    /// <example>
    /// <ja>
    /// ICoreServicesを取得します。
    /// <code>
    /// ICoreServices cs = PoderosaWorld.GetAdapter(typeof(ICoreServices));
    /// // IWindowManagerインターフェイスを取得します
    /// IWindowManager wm = cs.WindowManager;
    /// </code>
    /// </ja>
    /// <en>
    /// Get ICoreServices
    /// <code>
    /// ICoreServices cs = PoderosaWorld.GetAdapter(typeof(ICoreServices));
    /// // Get IWindowManager interface.
    /// IWindowManager wm = cs.WindowManager;
    /// </code>
    /// </en>
    /// </example>
    public interface ICoreServices : IAdaptable {
        /// <summary>
        /// <ja>
        /// IWindowManagerインターフェイスを返します。
        /// </ja>
        /// <en>
        /// Return IWindowManager interface.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <seealso cref="IPluginManager">IPluginManager</seealso>の<see cref="IPluginManager.FindPlugin">FindPluginメソッド</see>
        /// を使って「org.poderosa.core.windows」を検索するのと同じです。
        /// </ja>
        /// <en>
        /// It is the same as the retrieval of "org.poderosa.core.windows" by using the <see cref="IPluginManager.FindPlugin">FindPlugin method</see> of <seealso cref="IPluginManager">IPluginManager</seealso>. 
        /// </en>
        /// </remarks>
        IWindowManager WindowManager {
            get;
        }
        /// <summary>
        /// <ja>
        /// IPreferencesインターフェイスを返します。
        /// </ja>
        /// <en>
        /// Return IPreferences interface.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <seealso cref="IPluginManager">IPluginManager</seealso>の<see cref="IPluginManager.FindPlugin">FindPluginメソッド</see>
        /// を使って「org.poderosa.core.preferences」を検索するのと同じです。
        /// </ja>
        /// <en>
        /// It is the same as the retrieval of "org.poderosa.core.preferences" by using the <see cref="IPluginManager.FindPlugin">FindPlugin method</see> of <seealso cref="IPluginManager">IPluginManager</seealso>. 
        /// </en>
        /// </remarks>
        IPreferences Preferences {
            get;
        }
        /// <summary>
        /// <ja>
        /// ICommandManagerインターフェイスを返します。
        /// </ja>
        /// <en>
        /// Return ICommandManager interface.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <seealso cref="IPluginManager">IPluginManager</seealso>の<see cref="IPluginManager.FindPlugin">FindPluginメソッド</see>
        /// を使って「org.poderosa.core.commands」を検索するのと同じです。
        /// </ja>
        /// <en>
        /// It is the same as the retrieval of "org.poderosa.core.commands" by using the <see cref="IPluginManager.FindPlugin">FindPlugin method</see> of <seealso cref="IPluginManager">IPluginManager</seealso>. 
        /// </en>
        /// </remarks>
        ICommandManager CommandManager {
            get;
        }
        /// <summary>
        /// <ja>
        /// ISessionManagerインターフェイスを返します。
        /// </ja>
        /// <en>
        /// Return ISessionManager interface.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <seealso cref="IPluginManager">IPluginManager</seealso>の<see cref="IPluginManager.FindPlugin">FindPluginメソッド</see>
        /// を使って「org.poderosa.core.sessions」を検索するのと同じです。
        /// </ja>
        /// <en>
        /// It is the same as the retrieval of "org.poderosa.core.sessions" by using the <see cref="IPluginManager.FindPlugin">FindPlugin method</see> of <seealso cref="IPluginManager">IPluginManager</seealso>. 
        /// </en>
        /// </remarks>
        ISessionManager SessionManager {
            get;
        }
#if !LIBRARY
        /// <summary>
        /// <ja>
        /// ISerializeServiceインターフェイスを返します。
        /// </ja>
        /// <en>
        /// Return ISerializeService interface.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <seealso cref="IPluginManager">IPluginManager</seealso>の<see cref="IPluginManager.FindPlugin">FindPluginメソッド</see>
        /// を使って「org.poderosa.core.serializing」を検索するのと同じです。
        /// </ja>
        /// <en>
        /// It is the same as the retrieval of "org.poderosa.core.serializing" by using the <see cref="IPluginManager.FindPlugin">FindPlugin method</see> of <seealso cref="IPluginManager">IPluginManager</seealso>. 
        /// </en>
        /// </remarks>
        ISerializeService SerializeService {
            get;
        }
#endif

        //以下は頻出ExtensionPoint
        /// <summary>
        /// <ja>
        /// PreferencePluginプラグインが提供する拡張ポイントを返します。
        /// </ja>
        /// <en>
        /// Return the extension point of the PreferencePlugin plug-in.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <seealso cref="IPluginManager">IPluginManager</seealso>の<see cref="IPluginManager.FindExtensionPoint">FindExtensionPointメソッド</see>
        /// を使って「org.poderosa.core.preferences」を検索するのと同じです。
        /// </ja>
        /// <en>
        /// It is the same as the retrieval of "org.poderosa.core.preferences" by using the <see cref="IPluginManager.FindExtensionPoint">FindExtensionPoint method</see> of <seealso cref="IPluginManager">IPluginManager</seealso>. 
        /// </en>
        /// </remarks>
        IExtensionPoint PreferenceExtensionPoint {
            get;
        }
#if !LIBRARY
        /// <summary>
        /// <ja>
        /// SerializeServicePluginプラグインが提供する拡張ポイントを返します。
        /// </ja>
        /// <en>
        /// Return the extension point of the SerializeServicePlugin plug-in.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <seealso cref="IPluginManager">IPluginManager</seealso>の<see cref="IPluginManager.FindExtensionPoint">FindExtensionPointメソッド</see>
        /// を使って「org.poderosa.core.serializeElement」を検索するのと同じです。
        /// </ja>
        /// <en>
        /// It is the same as the retrieval of "org.poderosa.core.serializeElement" by using the <see cref="IPluginManager.FindExtensionPoint">FindExtensionPoint method</see> of <seealso cref="IPluginManager">IPluginManager</seealso>. 
        /// </en>
        /// </remarks>
        IExtensionPoint SerializerExtensionPoint {
            get;
        }
#endif
    }

    //その実装と登録
    internal class CoreServices : ICoreServices {
        private IPoderosaWorld _world;
        private AF _adapterFactory;

        private IWindowManager _windowManager;
        private IPreferences _preferences;
        private ICommandManager _commandManager;
        private ISessionManager _sessionManager;
#if !LIBRARY
        private ISerializeService _serializeService;
#endif
        private IExtensionPoint _preferenceExtensionPoint;
#if !LIBRARY
        private IExtensionPoint _serializerExtensionPoint;
#endif

        public CoreServices(IPoderosaWorld world) {
            _world = world;
            _adapterFactory = new AF(_world, this);
            _world.AdapterManager.RegisterFactory(_adapterFactory);
        }

        public IWindowManager WindowManager {
            get {
                if (_windowManager == null)
                    _windowManager = (IWindowManager)_world.PluginManager.FindPlugin(WindowManagerPlugin.PLUGIN_ID, typeof(IWindowManager));
                return _windowManager;
            }
        }

        public IPreferences Preferences {
            get {
                if (_preferences == null)
                    _preferences = (IPreferences)_world.PluginManager.FindPlugin(PreferencePlugin.PLUGIN_ID, typeof(IPreferences));
                return _preferences;
            }
        }

        public ICommandManager CommandManager {
            get {
                if (_commandManager == null)
                    _commandManager = (ICommandManager)_world.PluginManager.FindPlugin(CommandManagerPlugin.PLUGIN_ID, typeof(ICommandManager));
                return _commandManager;
            }
        }

#if !LIBRARY
        public ISerializeService SerializeService {
            get {
                if (_serializeService == null)
                    _serializeService = (ISerializeService)_world.PluginManager.FindPlugin(SerializeServicePlugin.PLUGIN_ID, typeof(ISerializeService));
                return _serializeService;
            }
        }
#endif

        public ISessionManager SessionManager {
            get {
                if (_sessionManager == null)
                    _sessionManager = (ISessionManager)_world.PluginManager.FindPlugin(SessionManagerPlugin.PLUGIN_ID, typeof(ISessionManager));
                return _sessionManager;
            }
        }


        public IExtensionPoint PreferenceExtensionPoint {
            get {
                if (_preferenceExtensionPoint == null)
                    _preferenceExtensionPoint = (IExtensionPoint)_world.PluginManager.FindExtensionPoint(PreferencePlugin.EXTENSIONPOINT_NAME);
                return _preferenceExtensionPoint;
            }
        }

#if !LIBRARY
        public IExtensionPoint SerializerExtensionPoint {
            get {
                if (_serializerExtensionPoint == null)
                    _serializerExtensionPoint = (IExtensionPoint)_world.PluginManager.FindExtensionPoint(SerializeServicePlugin.EXTENSIONPOINT_NAME);
                return _serializerExtensionPoint;
            }
        }
#endif

        public IAdaptable GetAdapter(Type adapter) {
            return _world.AdapterManager.GetAdapter(this, adapter);
        }

        private class AF : IDualDirectionalAdapterFactory {
            private IPoderosaWorld _world;
            private CoreServices _coreServices;

            public AF(IPoderosaWorld world, CoreServices cs) {
                _world = world;
                _coreServices = cs;
            }


            public Type SourceType {
                get {
                    return _world.GetType();
                }
            }

            public Type AdapterType {
                get {
                    return typeof(CoreServices);
                }
            }

            //一つしかインスタンスがないのをいいことに唯一のを返す
            public IAdaptable GetAdapter(IAdaptable obj) {
                return _coreServices;
            }

            public IAdaptable GetSource(IAdaptable obj) {
                return _world;
            }
        }
    }
}
