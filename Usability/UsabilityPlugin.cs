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
using Poderosa.Protocols;
using Poderosa.Commands;
using Poderosa.Forms;
using Poderosa.Preferences;

[assembly: PluginDeclaration(typeof(Poderosa.Usability.UsabilityPlugin))]

namespace Poderosa.Usability {
    [PluginInfo(ID = "org.poderosa.usability", Version = VersionInfo.PODEROSA_VERSION, Author = VersionInfo.PROJECT_NAME, Dependencies = "org.poderosa.terminalsessions")]
    internal class UsabilityPlugin : PluginBase {
        private static UsabilityPlugin _instance;
        private static StringResource _stringResource;
        private ICommandManager _commandManager;
        private IWindowManager _windowManager;
#if !LIBRARY
        private SSHKnownHosts _sshKnownHosts;
#endif
        public static UsabilityPlugin Instance {
            get {
                return _instance;
            }
        }

        public override void InitializePlugin(IPoderosaWorld poderosa) {
            base.InitializePlugin(poderosa);
            _instance = this;
            ICoreServices cs = (ICoreServices)poderosa.GetAdapter(typeof(ICoreServices));

            poderosa.Culture.AddChangeListener(UsabilityPlugin.Strings);
            IPluginManager pm = poderosa.PluginManager;

            _commandManager = cs.CommandManager;
            Debug.Assert(_commandManager != null);

            _windowManager = cs.WindowManager;
            Debug.Assert(_windowManager != null);

#if !LIBRARY
            //Guevara AboutBox
            pm.FindExtensionPoint("org.poderosa.window.aboutbox").RegisterExtension(new GuevaraAboutBoxFactory());

            //SSH KnownHost
            _sshKnownHosts = new SSHKnownHosts();
            cs.PreferenceExtensionPoint.RegisterExtension(_sshKnownHosts);
            pm.FindExtensionPoint(ProtocolsPluginConstants.HOSTKEYCHECKER_EXTENSION).RegisterExtension(_sshKnownHosts);
#endif
        }
        public override void TerminatePlugin() {
            base.TerminatePlugin();
#if !LIBRARY
            if (_sshKnownHosts.Modified)
                _sshKnownHosts.Flush();
#endif
        }

        public IWindowManager WindowManager {
            get {
                return _windowManager;
            }
        }

        public static StringResource Strings {
            get {
                if (_stringResource == null)
                    _stringResource = new StringResource("Poderosa.Usability.strings", typeof(TerminalUIPlugin).Assembly);
                return _stringResource;
            }
        }
    }

#if false
    //UsabilityPluginのオプション。GUIでの設定はないので楽な実装
    internal class UsabilityPluginPreference : IPreferenceSupplier {
        private IStringPreferenceItem _knownHostsPath;

        public string PreferenceID {
            get {
                return "org.poderosa.usability";
            }
        }

        public void InitializePreference(IPreferenceBuilder builder, IPreferenceFolder folder) {
            _knownHostsPath = builder.DefineStringValue(folder, "knownHostsPath", "ssh_known_hosts", null);
        }

        public object QueryAdapter(IPreferenceFolder folder, Type type) {
            return null;
        }

        public void ValidateFolder(IPreferenceFolder folder, IPreferenceValidationResult output) {
        }

        public IStringPreferenceItem KnownHostsPath {
            get {
                return _knownHostsPath;
            }
        }
    }
#endif
}
