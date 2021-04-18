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
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.IO.Ports;

using Poderosa.Plugins;
using Poderosa.Util;
using Poderosa.Forms;
using Poderosa.Terminal;
using Poderosa.UI;
using Poderosa.Protocols;
using Poderosa.Commands;
using Poderosa.Sessions;
#if LIBRARY
using Poderosa.Library;
#else
using Poderosa.MacroEngine;
using Poderosa.Preferences;
using Poderosa.Serializing;
#endif

[assembly: PluginDeclaration(typeof(Poderosa.SerialPort.SerialPortPlugin))]

namespace Poderosa.SerialPort {
#if LIBRARY
    [PluginInfo(ID = SerialPortPlugin.PLUGIN_ID, Version = VersionInfo.PODEROSA_VERSION, Author = VersionInfo.PROJECT_NAME, Dependencies = "org.poderosa.terminalsessions")]
#else
    [PluginInfo(ID = SerialPortPlugin.PLUGIN_ID, Version = VersionInfo.PODEROSA_VERSION, Author = VersionInfo.PROJECT_NAME, Dependencies = "org.poderosa.terminalsessions;org.poderosa.cygwin")]
#endif
    internal class SerialPortPlugin : PluginBase {
        public const string PLUGIN_ID = "org.poderosa.serialport";
        private static SerialPortPlugin _instance;

        private StringResource _stringResource;
#if !LIBRARY
        private ICoreServices _coreServices;
        private IProtocolService _protocolService;
        private ITerminalSessionsService _terminalSessionsService;
        private ITerminalEmulatorService _terminalEmulatorService;
        private IMacroEngine _macroEngine;

        private OpenSerialPortCommand _openSerialPortCommand;
#endif

        public override void InitializePlugin(IPoderosaWorld poderosa) {
            base.InitializePlugin(poderosa);
            _instance = this;

#if LIBRARY
            _stringResource = StringResource.Instance;
#else
            _stringResource = new StringResource("Poderosa.SerialPort.strings", typeof(SerialPortPlugin).Assembly);
            poderosa.Culture.AddChangeListener(_stringResource);
#endif
            IPluginManager pm = poderosa.PluginManager;
#if !LIBRARY
            _coreServices = (ICoreServices)poderosa.GetAdapter(typeof(ICoreServices));

            IExtensionPoint pt = _coreServices.SerializerExtensionPoint;
            pt.RegisterExtension(new SerialTerminalParamSerializer());
            pt.RegisterExtension(new SerialTerminalSettingsSerializer());

            _openSerialPortCommand = new OpenSerialPortCommand();
            _coreServices.CommandManager.Register(_openSerialPortCommand);

            pm.FindExtensionPoint("org.poderosa.menu.file").RegisterExtension(new SerialPortMenuGroup());
            pm.FindExtensionPoint("org.poderosa.core.window.toolbar").RegisterExtension(new SerialPortToolBarComponent());
#endif
            pm.FindExtensionPoint("org.poderosa.termianlsessions.terminalConnectionFactory").RegisterExtension(new SerialConnectionFactory());

        }

        public static SerialPortPlugin Instance {
            get {
                return _instance;
            }
        }

#if !LIBRARY
        public IProtocolService ProtocolService {
            get {
                if (_protocolService == null)
                    _protocolService = (IProtocolService)_poderosaWorld.PluginManager.FindPlugin("org.poderosa.protocols", typeof(IProtocolService));
                return _protocolService;
            }
        }
        public ITerminalSessionsService TerminalSessionsService {
            get {
                if (_terminalSessionsService == null)
                    _terminalSessionsService = (ITerminalSessionsService)_poderosaWorld.PluginManager.FindPlugin("org.poderosa.terminalsessions", typeof(ITerminalSessionsService));
                return _terminalSessionsService;
            }
        }
        public ITerminalEmulatorService TerminalEmulatorService {
            get {
                if (_terminalEmulatorService == null)
                    _terminalEmulatorService = (ITerminalEmulatorService)_poderosaWorld.PluginManager.FindPlugin("org.poderosa.terminalemulator", typeof(ITerminalEmulatorService));
                return _terminalEmulatorService;
            }
        }
        public ISerializeService SerializeService {
            get {
                return _coreServices.SerializeService;
            }
        }
#endif

        public StringResource Strings {
            get {
                return _stringResource;
            }
        }

#if !LIBRARY
        //TODO そのうち廃止予定なので
        public ICygwinPlugin CygwinPlugin {
            get {
                return (ICygwinPlugin)_poderosaWorld.PluginManager.FindPlugin("org.poderosa.cygwin", typeof(ICygwinPlugin));
            }
        }

        public ICommandManager CommandManager {
            get {
                return _coreServices.CommandManager;
            }
        }

        public IMacroEngine MacroEngine {
            get {
                if (_macroEngine == null) {
                    _macroEngine = _poderosaWorld.PluginManager.FindPlugin("org.poderosa.macro", typeof(IMacroEngine)) as IMacroEngine;
                }
                return _macroEngine;
            }
        }

        public Image LoadIcon() {
            return Poderosa.SerialPort.Properties.Resources.Icon16x16;
        }

        //コマンド、メニュー、ツールバー
        private class SerialPortMenuGroup : PoderosaMenuGroupImpl {
            public SerialPortMenuGroup()
                : base(new SerialPortMenuItem()) {
                _positionType = PositionType.NextTo;
                _designationTarget = _instance.CygwinPlugin.CygwinMenuGroupTemp;
            }
        }

        private class SerialPortMenuItem : PoderosaMenuItemImpl {
            public SerialPortMenuItem()
                : base(_instance._openSerialPortCommand, _instance.Strings, "Menu.SerialPort") {
            }
        }

        private class SerialPortToolBarComponent : IToolBarComponent, IPositionDesignation {

            public IAdaptable DesignationTarget {
                get {
                    return _instance.CygwinPlugin.CygwinToolBarComponentTemp;
                }
            }

            public PositionType DesignationPosition {
                get {
                    return PositionType.NextTo;
                }
            }

            public bool ShowSeparator {
                get {
                    return true;
                }
            }

            public IToolBarElement[] ToolBarElements {
                get {
                    return new IToolBarElement[] { new ToolBarCommandButtonImpl(_instance._openSerialPortCommand, SerialPortPlugin.Instance.LoadIcon()) };
                }
            }

            public IAdaptable GetAdapter(Type adapter) {
                return _instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
            }

        }

        private class OpenSerialPortCommand : GeneralCommandImpl {
            public OpenSerialPortCommand()
                : base("org.poderosa.session.openserialport", _instance.Strings, "Command.SerialPort", _instance.TerminalSessionsService.ConnectCommandCategory) {
            }

            public override CommandResult InternalExecute(ICommandTarget target, params IAdaptable[] args) {
                IPoderosaMainWindow window = (IPoderosaMainWindow)target.GetAdapter(typeof(IPoderosaMainWindow));
                SerialLoginDialog dlg = new SerialLoginDialog();
                using (dlg) {
                    SerialTerminalParam tp = new SerialTerminalParam();
                    SerialTerminalSettings ts = SerialPortUtil.CreateDefaultSerialTerminalSettings(tp.PortName);
                    dlg.ApplyParam(tp, ts);

                    if (dlg.ShowDialog(window.AsForm()) == DialogResult.OK) { //TODO 親ウィンドウ指定
                        ITerminalConnection con = dlg.ResultConnection;
                        if (con != null) {
                            return _instance.CommandManager.Execute(_instance.TerminalSessionsService.TerminalSessionStartCommand,
                                window, con, dlg.ResultTerminalSettings);
                        }
                    }
                }
                return CommandResult.Cancelled;
            }

        }
#endif
    }
}
