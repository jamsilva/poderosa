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
using System.Diagnostics;

using Poderosa.Plugins;
using Poderosa.Util;
using Poderosa.Forms;
using Poderosa.Terminal;
using Poderosa.UI;
using Poderosa.Protocols;
#if LIBRARY
using Poderosa.Library;
#else
using Poderosa.Commands;
using Poderosa.Preferences;
using Poderosa.Serializing;
#endif

[assembly: PluginDeclaration(typeof(Poderosa.Sessions.TerminalSessionsPlugin))]

namespace Poderosa.Sessions {
    [PluginInfo(ID = TerminalSessionsPlugin.PLUGIN_ID, Version = VersionInfo.PODEROSA_VERSION, Author = VersionInfo.PROJECT_NAME, Dependencies = "org.poderosa.core.sessions;org.poderosa.terminalemulator;org.poderosa.protocols")]
#if LIBRARY
    internal class TerminalSessionsPlugin : PluginBase, IAdaptable {
#else
    internal class TerminalSessionsPlugin : PluginBase, ITerminalSessionsService {
#endif
        public const string PLUGIN_ID = "org.poderosa.terminalsessions";

#if !LIBRARY
        public const string TERMINAL_CONNECTION_FACTORY_ID = "org.poderosa.termianlsessions.terminalConnectionFactory";
#endif

        private static TerminalSessionsPlugin _instance;

        private ICoreServices _coreServices;
#if LIBRARY
        private TerminalSessionOptions _terminalSessionOptions;
#else
        private TerminalSessionOptionsSupplier _terminalSessionsOptionSupplier;
#endif
        private IProtocolService _protocolService;
        private ITerminalEmulatorService _terminalEmulatorService;

        private TerminalViewFactory _terminalViewFactory;
        private PaneBridgeAdapter _paneBridgeAdapter;

#if !LIBRARY
        private StartCommand _startCommand;
        private ReproduceCommand _reproduceCommand;
        private IPoderosaCommand _pasteCommand;
        private IExtensionPoint _pasteCommandExt;
#endif

        public override void InitializePlugin(IPoderosaWorld poderosa) {
            base.InitializePlugin(poderosa);
            _instance = this;

            IPluginManager pm = poderosa.PluginManager;
            _coreServices = (ICoreServices)poderosa.GetAdapter(typeof(ICoreServices));
#if !LIBRARY
            TEnv.ReloadStringResource();
#endif
            _terminalViewFactory = new TerminalViewFactory();
            pm.FindExtensionPoint(WindowManagerConstants.VIEW_FACTORY_ID).RegisterExtension(_terminalViewFactory);
            //このViewFactoryはデフォ
            foreach (IViewManagerFactory mf in pm.FindExtensionPoint(WindowManagerConstants.MAINWINDOWCONTENT_ID).GetExtensions())
                mf.DefaultViewFactory = _terminalViewFactory;

#if LIBRARY
            _terminalSessionOptions = new TerminalSessionOptions();
#else
            //ログインダイアログのサポート用
            pm.CreateExtensionPoint("org.poderosa.terminalsessions.telnetSSHLoginDialogInitializer", typeof(ITelnetSSHLoginDialogInitializer), this);
            pm.CreateExtensionPoint("org.poderosa.terminalsessions.loginDialogUISupport", typeof(ILoginDialogUISupport), this);
            pm.CreateExtensionPoint("org.poderosa.terminalsessions.terminalParameterStore", typeof(ITerminalSessionParameterStore), this);
            IExtensionPoint factory_point = pm.CreateExtensionPoint(TERMINAL_CONNECTION_FACTORY_ID, typeof(ITerminalConnectionFactory), this);

            _pasteCommandExt = pm.CreateExtensionPoint("org.poderosa.terminalsessions.pasteCommand", typeof(IPoderosaCommand), this);

            _terminalSessionsOptionSupplier = new TerminalSessionOptionsSupplier();
            _coreServices.PreferenceExtensionPoint.RegisterExtension(_terminalSessionsOptionSupplier);
#endif

            //Add conversion for TerminalPane
            _paneBridgeAdapter = new PaneBridgeAdapter();
            poderosa.AdapterManager.RegisterFactory(_paneBridgeAdapter);

#if !LIBRARY
            _startCommand = new StartCommand(factory_point);
            _reproduceCommand = new ReproduceCommand();
            _coreServices.CommandManager.Register(_reproduceCommand);

            ReproduceMenuGroup rmg = new ReproduceMenuGroup();
            IExtensionPoint consolemenu = pm.FindExtensionPoint("org.poderosa.menu.console");
            consolemenu.RegisterExtension(rmg);

            IExtensionPoint contextmenu = pm.FindExtensionPoint("org.poderosa.terminalemulator.contextMenu");
            contextmenu.RegisterExtension(rmg);

            IExtensionPoint documentContext = pm.FindExtensionPoint("org.poderosa.terminalemulator.documentContextMenu");
            documentContext.RegisterExtension(rmg);
#endif
        }

        public static TerminalSessionsPlugin Instance {
            get {
                return _instance;
            }
        }

        public IProtocolService ProtocolService {
            get {
                if (_protocolService == null)
                    _protocolService = (IProtocolService)_poderosaWorld.PluginManager.FindPlugin("org.poderosa.protocols", typeof(IProtocolService));
                return _protocolService;
            }
        }

        public ITerminalEmulatorService TerminalEmulatorService {
            get {
                if (_terminalEmulatorService == null)
                    _terminalEmulatorService = (ITerminalEmulatorService)_poderosaWorld.PluginManager.FindPlugin("org.poderosa.terminalemulator", typeof(ITerminalEmulatorService));
                return _terminalEmulatorService;
            }
        }
#if !LIBRARY
        public ISerializeService SerializeService {
            get {
                return _coreServices.SerializeService;
            }
        }
#endif
        public IWindowManager WindowManager {
            get {
                return _coreServices.WindowManager;
            }
        }
#if !LIBRARY
        public PaneBridgeAdapter PaneBridgeAdapter {
            get {
                return _paneBridgeAdapter;
            }
        }

        public TerminalViewFactory TerminalViewFactory {
            get {
                return _terminalViewFactory;
            }
        }
#endif

        public ITerminalSessionOptions TerminalSessionOptions {
            get {
#if LIBRARY
                return _terminalSessionOptions;
#else
                return _terminalSessionsOptionSupplier.OriginalOptions;
#endif
            }
        }

#if !LIBRARY
        public ICommandManager CommandManager {
            get {
                return _coreServices.CommandManager;
            }
        }
        public ISessionManager SessionManager {
            get {
                return _coreServices.SessionManager;
            }
        }

        public ICommandCategory ConnectCommandCategory {
            get {
                return Poderosa.Sessions.ConnectCommandCategory._instance;
            }
        }

#region ITerminalSessionService
        public ITerminalSessionStartCommand TerminalSessionStartCommand {
            get {
                return _startCommand;
            }
        }
#endregion

        public ReproduceCommand ReproduceCommand {
            get {
                return _reproduceCommand;
            }
        }
        public ICoreServicePreference CoreServicesPreference {
            get {
                IPreferenceFolder folder = _coreServices.Preferences.FindPreferenceFolder("org.poderosa.core.window");
                return (ICoreServicePreference)folder.QueryAdapter(typeof(ICoreServicePreference));
            }
        }

        /// <summary>
        /// Get a Paste command object
        /// </summary>
        /// <remarks>
        /// If an instance was registered on the extension point, this method returns it.
        /// Otherwise, returns the default implementation.
        /// </remarks>
        /// <returns></returns>
        public IPoderosaCommand GetPasteCommand() {
            if (_pasteCommand == null) {
                if (_pasteCommandExt != null && _pasteCommandExt.GetExtensions().Length > 0) {
                    _pasteCommand = ((IPoderosaCommand[])_pasteCommandExt.GetExtensions())[0];
                }
                else {
                    _pasteCommand = new PasteToTerminalCommand();
                }
            }
            return _pasteCommand;
        }
#endif
    }

    internal class TEnv {
#if !LIBRARY
        private static StringResource _stringResource;

        public static void ReloadStringResource() {
            _stringResource = new StringResource("Poderosa.TerminalSession.strings", typeof(TEnv).Assembly);
            TerminalSessionsPlugin.Instance.PoderosaWorld.Culture.AddChangeListener(_stringResource);
        }
#endif

        public static StringResource Strings {
            get {
#if LIBRARY
                return StringResource.Instance;
#else
                return _stringResource;
#endif
            }
        }
    }

#if !LIBRARY
    internal class LoginDialogInitializeInfo : ITelnetSSHLoginDialogInitializeInfo {

        private List<string> _hosts;
        private List<string> _accounts;
        private List<string> _identityFiles;
        private List<int> _ports;

        public LoginDialogInitializeInfo() {
            _hosts = new List<string>();
            _accounts = new List<string>();
            _identityFiles = new List<string>();
            _ports = new List<int>();
            _ports.Add(22);
            _ports.Add(23); //これらはデフォ
        }

        public string[] Hosts {
            get {
                return _hosts.ToArray();
            }
        }

        public string[] Accounts {
            get {
                return _accounts.ToArray();
            }
        }

        public string[] IdentityFiles {
            get {
                return _identityFiles.ToArray();
            }
        }

        public int[] Ports {
            get {
                return _ports.ToArray();
            }
        }

#region ITelnetSSHLoginDialogInitializeInfo
        public void AddHost(string value) {
            if (!_hosts.Contains(value) && value.Length > 0)
                _hosts.Add(value);
        }

        public void AddAccount(string value) {
            if (!_accounts.Contains(value) && value.Length > 0)
                _accounts.Add(value);
        }

        public void AddIdentityFile(string value) {
            if (!_identityFiles.Contains(value) && value.Length > 0)
                _identityFiles.Add(value);
        }

        public void AddPort(int value) {
            if (!_ports.Contains(value))
                _ports.Add(value);
        }

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalSessionsPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
#endregion
    }
#endif
}
