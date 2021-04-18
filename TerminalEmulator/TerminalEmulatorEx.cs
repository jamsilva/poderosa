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
using System.Drawing;

using Poderosa.Protocols;
using Poderosa.Sessions;
using Poderosa.Forms;
using Poderosa.Commands;

namespace Poderosa.Terminal {
    //AbstractTerminalが必要な機能を受け渡し

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IAbstractTerminalHost {
        ITerminalSettings TerminalSettings {
            get;
        }
        TerminalTransmission TerminalTransmission {
            get;
        }
        ISession ISession {
            get;
        }
        IPoderosaMainWindow OwnerWindow {
            get;
        } //ISessionに昇格させるのがよい？　あるいはSessionManagerの機能か？　迷いどころ
        ITerminalConnection TerminalConnection {
            get;
        }

        TerminalControl TerminalControl {
            get;
        }
        void NotifyViewsDataArrived();
        void CloseByReceptionThread(string msg);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ITerminalControlHost {
        ITerminalSettings TerminalSettings {
            get;
        }
        IPoderosaMainWindow OwnerWindow {
            get;
        }
        ITerminalConnection TerminalConnection {
            get;
        }

        AbstractTerminal Terminal {
            get;
        }
        TerminalTransmission TerminalTransmission {
            get;
        }
    }

    /// <summary>
    /// <ja>
    /// ターミナルエミュレータサービスにアクセスするためのインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface to access terminal emulator service
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// このインターフェイスは、TermiunalEmuratorPluginプラグイン（プラグインID「org.poderosa.terminalemulator」）が
    /// 提供します。次のようにすると、ITerminalEmulatorServiceを取得できます。
    /// <code>
    /// ITerminalEmulatorService emuservice = 
    ///   (ITerminalEmulatorService)PoderosaWorld.PluginManager.FindPlugin(
    ///     "org.poderosa.terminalemulator", typeof(ITerminalEmulatorService));
    /// </code>
    /// </ja>
    /// <en>
    /// The TermiunalEmuratorPlugin plug-in (plug-in ID[org.poderosa.terminalemulator]) offers this interface. ITerminalEmulatorService can be acquired by doing as follows. 
    /// <code>
    /// ITerminalEmulatorService emuservice = 
    ///   (ITerminalEmulatorService)PoderosaWorld.PluginManager.FindPlugin(
    ///     "org.poderosa.terminalemulator", typeof(ITerminalEmulatorService));
    /// </code>
    /// </en>
    /// </remarks>
    public interface ITerminalEmulatorService {
#if !LIBRARY
        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        void LaterInitialize();
#endif

        /// <summary>
        /// <ja>
        /// ターミナルエミュレータのオプションを示します。
        /// </ja>
        /// <en>
        /// The option of the terminal emulator is shown. 
        /// </en>
        /// </summary>
        ITerminalEmulatorOptions TerminalEmulatorOptions {
            get;
        }
        /// <summary>
        /// <ja>
        /// デフォルトのターミナル設定を作成します。
        /// </ja>
        /// <en>
        /// Create a default terminal setting.
        /// </en>
        /// </summary>
        /// <param name="caption"><ja>ターミナルのキャプションです。</ja><en>Caption of terminal.</en></param>
        /// <param name="icon"><ja>ターミナルのアイコンです。nullを指定するとデフォルトのアイコンが使われます。</ja><en>It is an icon of the terminal. When null is specified, the icon of default is used. </en></param>
        /// <returns><ja>作成されたターミナル設定オブジェクトを示すITerminalSettingsです。</ja><en>It is ITerminalSettings that shows the made terminal setting object. </en></returns>
        ITerminalSettings CreateDefaultTerminalSettings(string caption, Image icon);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exclude/>
        ISimpleLogSettings CreateDefaultSimpleLogSettings();
#if !LIBRARY
        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        IPoderosaMenuGroup[] ContextMenu {
            get;
        }
#endif
        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        ICommandCategory TerminalCommandCategory {
            get;
        }
#if !LIBRARY
        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        IShellSchemeCollection ShellSchemeCollection {
            get;
        }
#endif
    }

    //ログファイル名のカスタマイズ
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IAutoLogFileFormatter {
        string FormatFileName(string default_directory, ITerminalParameter param, ITerminalSettings settings);
    }

    //動的なウィンドウキャプションカスタマイズ
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IDynamicCaptionFormatter {
        //スレッドでのブロックはないので注意
        string FormatCaptionUsingWindowTitle(ITerminalParameter param, ITerminalSettings settings, string windowTitle);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class TerminalInitializeInfo {
        private IAbstractTerminalHost _session;
        private ITerminalParameter _param;
        private int _initialWidth;
        private int _initialHeight;

        public TerminalInitializeInfo(IAbstractTerminalHost session, ITerminalParameter param) {
            _session = session;
            _param = param;
            _initialWidth = param.InitialWidth;
            _initialHeight = param.InitialHeight;
        }

        public IAbstractTerminalHost Session {
            get {
                return _session;
            }
        }
        public ITerminalParameter TerminalParameter {
            get {
                return _param;
            }
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
    }
}
