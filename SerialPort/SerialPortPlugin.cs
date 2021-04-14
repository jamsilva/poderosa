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
using Poderosa.Preferences;
using Poderosa.Sessions;
using Poderosa.Serializing;
#if !LIBRARY
using Poderosa.MacroEngine;
#endif

[assembly: PluginDeclaration(typeof(Poderosa.SerialPort.SerialPortPlugin))]

namespace Poderosa.SerialPort {
    [PluginInfo(ID = SerialPortPlugin.PLUGIN_ID, Version = VersionInfo.PODEROSA_VERSION, Author = VersionInfo.PROJECT_NAME, Dependencies = "org.poderosa.terminalsessions;org.poderosa.cygwin")]
    internal class SerialPortPlugin : PluginBase {
        public const string PLUGIN_ID = "org.poderosa.serialport";
        private static SerialPortPlugin _instance;

        private StringResource _stringResource;
        private ICoreServices _coreServices;
#if !LIBRARY
        private IProtocolService _protocolService;
#endif
        private ITerminalSessionsService _terminalSessionsService;
#if !LIBRARY
        private ITerminalEmulatorService _terminalEmulatorService;
        private IMacroEngine _macroEngine;
#endif

        private OpenSerialPortCommand _openSerialPortCommand;

        public override void InitializePlugin(IPoderosaWorld poderosa) {
            base.InitializePlugin(poderosa);
            _instance = this;

            _stringResource = new StringResource("Poderosa.SerialPort.strings", typeof(SerialPortPlugin).Assembly);
            poderosa.Culture.AddChangeListener(_stringResource);
            IPluginManager pm = poderosa.PluginManager;
            _coreServices = (ICoreServices)poderosa.GetAdapter(typeof(ICoreServices));

            IExtensionPoint pt = _coreServices.SerializerExtensionPoint;
            pt.RegisterExtension(new SerialTerminalParamSerializer());
            pt.RegisterExtension(new SerialTerminalSettingsSerializer());

            _openSerialPortCommand = new OpenSerialPortCommand();
            _coreServices.CommandManager.Register(_openSerialPortCommand);

#if !LIBRARY
            pm.FindExtensionPoint("org.poderosa.menu.file").RegisterExtension(new SerialPortMenuGroup());
#endif
            pm.FindExtensionPoint("org.poderosa.core.window.toolbar").RegisterExtension(new SerialPortToolBarComponent());
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
#endif
        public ITerminalSessionsService TerminalSessionsService {
            get {
                if (_terminalSessionsService == null)
                    _terminalSessionsService = (ITerminalSessionsService)_poderosaWorld.PluginManager.FindPlugin("org.poderosa.terminalsessions", typeof(ITerminalSessionsService));
                return _terminalSessionsService;
            }
        }
#if !LIBRARY
        public ITerminalEmulatorService TerminalEmulatorService {
            get {
                if (_terminalEmulatorService == null)
                    _terminalEmulatorService = (ITerminalEmulatorService)_poderosaWorld.PluginManager.FindPlugin("org.poderosa.terminalemulator", typeof(ITerminalEmulatorService));
                return _terminalEmulatorService;
            }
        }
#endif
        public ISerializeService SerializeService {
            get {
                return _coreServices.SerializeService;
            }
        }

        public StringResource Strings {
            get {
                return _stringResource;
            }
        }

        //TODO そのうち廃止予定なので
        public ICygwinPlugin CygwinPlugin {
            get {
                return (ICygwinPlugin)_poderosaWorld.PluginManager.FindPlugin("org.poderosa.cygwin", typeof(ICygwinPlugin));
            }
        }

#if !LIBRARY
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
            return Poderosa.SerialPort.Properties.Resources.Serial16x16;
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
#endif

        private class SerialPortToolBarComponent : IToolBarComponent, IPositionDesignation {
#if !LIBRARY
            public IAdaptable DesignationTarget {
                get {
                    return _instance.CygwinPlugin.CygwinToolBarComponentTemp;
                }
            }
#endif

            public PositionType DesignationPosition {
                get {
                    return PositionType.NextTo;
                }
            }

#if !LIBRARY
            public bool ShowSeparator {
                get {
                    return true;
                }
            }
#endif

            public IToolBarElement[] ToolBarElements {
                get {
#if LIBRARY
                    return new IToolBarElement[] {};
#else
                    return new IToolBarElement[] { new ToolBarCommandButtonImpl(_instance._openSerialPortCommand, SerialPortPlugin.Instance.LoadIcon()) };
#endif
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
#if !LIBRARY
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
#endif
                return CommandResult.Cancelled;
            }

        }

    }
}
