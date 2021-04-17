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
using System.Net;
using System.Diagnostics;

using Granados;
using Granados.AgentForwarding;
using Granados.X11Forwarding;

#if !LIBRARY
using Poderosa.MacroEngine;
#endif

namespace Poderosa.Protocols {
    /** TerminalParameter族の提供
     *    ここでは従来のようなクラス継承関係があるが、GetAdapterで取るべきものである。
     *    包含関係を継承で表現していたところにムリが生じていた。複数インタフェースの実装のために便宜的に継承を使っているにすぎない
     */

#if LIBRARY
    internal abstract class TerminalParameter : ITerminalParameter, ICloneable {
#else
    internal abstract class TerminalParameter : ITerminalParameter, IAutoExecMacroParameter, ICloneable {
#endif
        public const string DEFAULT_TERMINAL_TYPE = "xterm";

        private int _initialWidth;  //シェルの幅
        private int _initialHeight; //シェルの高さ
        private string _terminalType;
#if !LIBRARY
        private string _autoExecMacroPath;
#endif

        public TerminalParameter() {
            SetTerminalName(DEFAULT_TERMINAL_TYPE);
            SetTerminalSize(80, 25); //何も設定しなくても
#if !LIBRARY
            _autoExecMacroPath = null;
#endif
        }
        public TerminalParameter(TerminalParameter src) {
            _terminalType = src._terminalType;
            _initialWidth = src._initialWidth;
            _initialHeight = src._initialHeight;
#if !LIBRARY
            _autoExecMacroPath = src._autoExecMacroPath;
#endif
        }

        public int InitialWidth {
            get {
                return _initialWidth;
            }
        }
        public int InitialHeight {
            get {
                return _initialHeight;
            }
        }
        public string TerminalType {
            get {
                return _terminalType;
            }
        }
        public void SetTerminalSize(int width, int height) {
            _initialWidth = width;
            _initialHeight = height;
        }
        public void SetTerminalName(string terminaltype) {
            _terminalType = terminaltype;
        }

#if !LIBRARY
        #region IAutoExecMacroParameter

        public string AutoExecMacroPath {
            get {
                return _autoExecMacroPath;
            }
            set {
                _autoExecMacroPath = value;
            }
        }

        #endregion
#endif

        //IAdaptable
        public virtual IAdaptable GetAdapter(Type adapter) {
#if UNITTEST
            return adapter.IsInstanceOfType(this) ? this : null; //UnitTestではPoderosaの起動にならないケースもある
#else
            return ProtocolsPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
#endif
        }

        public abstract bool UIEquals(ITerminalParameter param);

        #region ICloneable
        public abstract object Clone();
        #endregion
    }

    //Telnet, SSHでの接続先情報
    internal abstract class TCPParameter : TerminalParameter, ITCPParameter {
        private string _destination;
        //private IPAddress _address;
        private int _port;

        public TCPParameter() {
            _destination = "";
        }
        public TCPParameter(string destination, int port) {
            _destination = destination;
            _port = port;
        }
        public TCPParameter(TCPParameter src)
            : base(src) {
            _destination = src._destination;
            _port = src._port;
        }

#if !LIBRARY
        [MacroConnectionParameter]
#endif
        public string Destination {
            get {
                return _destination;
            }
            set {
                _destination = value;
            }
        }
#if !LIBRARY
        [MacroConnectionParameter]
#endif
        public int Port {
            get {
                return _port;
            }
            set {
                _port = value;
            }
        }

        public override bool UIEquals(ITerminalParameter param) {
            ITCPParameter tcp = (ITCPParameter)param.GetAdapter(typeof(ITCPParameter));
            return tcp != null && _destination == tcp.Destination && _port == tcp.Port;
        }
    }

    internal class TelnetParameter : TCPParameter, ITelnetParameter {
        private bool _telnetNewLine = true;

        public bool TelnetNewLine {
            get {
                return _telnetNewLine;
            }
            set {
                _telnetNewLine = value;
            }
        }

        public TelnetParameter() {
            this.Port = 23;
        }
        public TelnetParameter(string dest, int port)
            : base(dest, port) {
        }
        public TelnetParameter(TelnetParameter src)
            : base(src) {
        }
        #region ICloneable
        public override object Clone() {
            return new TelnetParameter(this);
        }
        #endregion
    }

    internal class SSHLoginParameter : TCPParameter, ISSHLoginParameter {
        private SSHProtocol _method;
        private AuthenticationType _authType;
        private string _account;
        private string _identityFile;
        private string _passwordOrPassphrase;
        private bool _letUserInputPassword;
        private bool _enableAgentForwarding;
        private IAgentForwardingAuthKeyProvider _authKeyProvider;
        private bool _enableX11Forwarding;
        private X11ForwardingParams _x11Forwarding;

        public SSHLoginParameter() {
            _method = SSHProtocol.SSH2;
            _authType = AuthenticationType.Password;
            _passwordOrPassphrase = "";
            _identityFile = "";
            _letUserInputPassword = true;
            this.Port = 22;
        }
        public SSHLoginParameter(SSHLoginParameter src)
            : base(src) {
            _method = src._method;
            _authType = src._authType;
            _account = src._account;
            _identityFile = src._identityFile;
            _passwordOrPassphrase = src._passwordOrPassphrase;
            _letUserInputPassword = src._letUserInputPassword;
            _authKeyProvider = src._authKeyProvider;
        }

#if !LIBRARY
        [MacroConnectionParameter]
#endif
        public AuthenticationType AuthenticationType {
            get {
                return _authType;
            }
            set {
                _authType = value;
            }
        }
#if !LIBRARY
        [MacroConnectionParameter]
#endif
        public string Account {
            get {
                return _account;
            }
            set {
                _account = value;
            }
        }
#if !LIBRARY
        [MacroConnectionParameter]
#endif
        public string IdentityFileName {
            get {
                return _identityFile;
            }
            set {
                Debug.Assert(value != null);
                _identityFile = value;
            }
        }
#if !LIBRARY
        [MacroConnectionParameter]
#endif
        public SSHProtocol Method {
            get {
                return _method;
            }
            set {
                _method = value;
            }
        }
        public string PasswordOrPassphrase {
            // Note:
            //  An instance of SSHLoginParameter will be shared with the session, MRU, cache, etc.
            //  So the clearing of this property affects reproducing the connection.
            //  To "forget" password, just ignore this property and ask a new password to the user.
            get {
                return _passwordOrPassphrase;
            }
            set {
                _passwordOrPassphrase = value;
            }
        }
        public bool LetUserInputPassword {
            get {
                return _letUserInputPassword;
            }
            set {
                _letUserInputPassword = value;
            }
        }

        public bool EnableAgentForwarding {
            get {
                return _enableAgentForwarding;
            }
            set {
                _enableAgentForwarding = value;
            }
        }

        public IAgentForwardingAuthKeyProvider AgentForwardingAuthKeyProvider {
            get {
                return _authKeyProvider;
            }
            set {
                _authKeyProvider = value;
            }
        }

        public bool EnableX11Forwarding {
            get {
                return _enableX11Forwarding;
            }
            set {
                _enableX11Forwarding = value;
            }
        }

        public X11ForwardingParams X11Forwarding {
            get {
                return _x11Forwarding;
            }
            set {
                _x11Forwarding = value;
            }
        }

        public override bool UIEquals(ITerminalParameter param) {
            ISSHLoginParameter ssh = (ISSHLoginParameter)param.GetAdapter(typeof(ISSHLoginParameter));
            return ssh != null && base.UIEquals(param) && _account == ssh.Account; //プロトコルが違うだけでは同一視してしまう
        }
        #region ICloneable
        public override object Clone() {
            return new SSHLoginParameter(this);
        }
        #endregion
    }

    internal class LocalShellParameter : TerminalParameter, ICygwinParameter {
        private string _home;
        private string _shellName;
        private string _cygwinDir;

        public LocalShellParameter() {
            _home = CygwinUtil.DefaultHome;
            _shellName = CygwinUtil.DefaultShell;
            _cygwinDir = CygwinUtil.DefaultCygwinDir;
        }
        public LocalShellParameter(LocalShellParameter src)
            : base(src) {
            _home = src._home;
            _shellName = src._shellName;
            _cygwinDir = src._cygwinDir;
        }

#if !LIBRARY
        [MacroConnectionParameter]
#endif
        public string Home {
            get {
                return _home;
            }
            set {
                _home = value;
            }
        }
#if !LIBRARY
        [MacroConnectionParameter]
#endif
        public string ShellName {
            get {
                return _shellName;
            }
            set {
                _shellName = value;
            }
        }
        //引数なしのシェル名
#if !LIBRARY
        [MacroConnectionParameter]
#endif
        public string ShellBody {
            get {
                int c = _shellName.IndexOf(' ');
                if (c != -1)
                    return _shellName.Substring(0, c); //最初のスペースの手前まで: ふつう/bin/bash
                else
                    return _shellName;
            }
        }
#if !LIBRARY
        [MacroConnectionParameter]
#endif
        public string CygwinDir {
            get {
                return _cygwinDir;
            }
            set {
                _cygwinDir = value;
            }
        }


        public override bool UIEquals(ITerminalParameter param) {
            return param is LocalShellParameter; //Cygwinは全部同一視
        }

        #region ICloneable
        public override object Clone() {
            return new LocalShellParameter(this);
        }
        #endregion
    }

    internal class EmptyTerminalParameter : TerminalParameter {
        public override bool UIEquals(ITerminalParameter t) {
            return t is EmptyTerminalParameter;
        }

        public override object Clone() {
            return new EmptyTerminalParameter();
        }


    }
}
