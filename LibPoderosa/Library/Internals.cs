using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Poderosa.Boot;
using Poderosa.Commands;
using Poderosa.Forms;
using Poderosa.Plugins;
using Poderosa.Protocols;
using Poderosa.Sessions;
using Poderosa.Terminal;

namespace Poderosa.Library
{
    internal class InternalPoderosaInstance
    {
        Dictionary<string, IPlugin> _cachedPlugins = new Dictionary<string, IPlugin>();
        InternalPoderosaWorld _internalPoderosaWorld;

        public InternalPoderosaInstance()
        {
            string homeDirectory = Directory.GetCurrentDirectory();
            _internalPoderosaWorld = new InternalPoderosaWorld(new PoderosaStartupContext(PluginManifest.CreateLibraryManifest(), homeDirectory, homeDirectory, null, null));
            _internalPoderosaWorld.Start();
        }

        private T GetPlugin<T>(string pluginId) where T : IPlugin
        {
            if (_cachedPlugins.ContainsKey(pluginId))
                return (T) _cachedPlugins[pluginId];

            T plugin = (T) _internalPoderosaWorld.PluginManager.FindPlugin(pluginId, typeof(T));
            _cachedPlugins[pluginId] = plugin;
            return plugin;
        }

        public CommandManagerPlugin CommandManagerPlugin
        {
            get
            {
                return GetPlugin<CommandManagerPlugin>(CommandManagerPlugin.PLUGIN_ID);
            }
        }

        public SessionManagerPlugin SessionManagerPlugin
        {
            get
            {
                return GetPlugin<SessionManagerPlugin>(SessionManagerPlugin.PLUGIN_ID);
            }
        }

        public TerminalSessionsPlugin TerminalSessionsPlugin
        {
            get
            {
                return GetPlugin<TerminalSessionsPlugin>(TerminalSessionsPlugin.PLUGIN_ID);
            }
        }

        public WindowManagerPlugin WindowManagerPlugin
        {
            get
            {
                return GetPlugin<WindowManagerPlugin>(WindowManagerPlugin.PLUGIN_ID);
            }
        }

        public IProtocolService ProtocolService
        {
            get
            {
                return TerminalSessionsPlugin.ProtocolService;
            }
        }

        public InternalTerminalInstance NewTerminal()
        {
            return new InternalTerminalInstance(this);
        }
    }

    internal class InternalTerminalInstance
    {
        public delegate void ConnectionAndSettings(out ITerminalConnection connection, out ITerminalSettings settings);

        private InternalPoderosaInstance _basePoderosaInstance;
        private IPoderosaView _terminalView;

        public ITerminalSession TerminalSession { get; private set; }
        public MainWindow Window { get; private set; }

        public InternalTerminalInstance(InternalPoderosaInstance _internalPoderosaWorld)
        {
            _basePoderosaInstance = _internalPoderosaWorld;
            Window = _basePoderosaInstance.WindowManagerPlugin.CreateLibraryMainWindow();
            _terminalView = Window.ViewManager.GetCandidateViewForNewDocument();
        }

        public void Connect(ConnectionAndSettings connectionAndSettings)
        {
            try
            {
                ITerminalConnection connection;
                ITerminalSettings settings;
                connectionAndSettings(out connection, out settings);
                TerminalSession = new TerminalSession(connection, settings);
                _basePoderosaInstance.SessionManagerPlugin.StartNewSession(TerminalSession, _terminalView);
                _basePoderosaInstance.SessionManagerPlugin.ActivateDocument(TerminalSession.Terminal.IDocument, ActivateReason.InternalAction);
            }
            catch (Exception ex)
            {
                RuntimeUtil.ReportException(ex);
            }
        }

        public void Disconnect()
        {
            TerminalSession.InternalTerminate();
        }
    }
}
