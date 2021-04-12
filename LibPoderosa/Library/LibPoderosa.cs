using System;
using System.Drawing;
using System.Windows.Forms;

using Poderosa.ConnectionParam;
using Poderosa.Pipe;
using Poderosa.Protocols;
using Poderosa.SerialPort;
using Poderosa.Terminal;
using Poderosa.View;

namespace Poderosa.Library
{
    //  XXXX event EventHandler ConnectionDisconnect;
    //  XXXX event EventHandler ConnectionSuccess;
    //  XXXX event EventHandler ConnectionError;
    //- DONE void Connect();
    //  DONE void Close();
    //- XXXX void CommentLog(string comment);
    //  XXXX void SetLog(LogType logType, string File, bool append);
    //- DONE void SetPaneColors(Color TextColor, Color BackColor);
    //  XXXX string GetLastLine();
    //  DONE void SendText(string command);
    //  XXXX void CopySelectedTextToClipboard();

    public abstract class Session
    {
        internal static InternalPoderosaInstance _instance = new InternalPoderosaInstance();
        internal InternalTerminalInstance _terminal;
        private Form _currentParent;
        private Form _form;
        private RenderProfile _renderProfile;

        public Form Connect()
        {
            _terminal = _instance.NewTerminal();
            _terminal.Connect(ConnectionAndSettings);
            _terminal.TerminalSession.TerminalSettings.BeginUpdate();
            _terminal.TerminalSession.TerminalSettings.Encoding = EncodingType.UTF8;
            _renderProfile = _terminal.TerminalSession.TerminalSettings.RenderProfile = new RenderProfile(_terminal.TerminalSession.TerminalControl.GetRenderProfile());
            _terminal.TerminalSession.TerminalSettings.EndUpdate();
            _form = _terminal.Window.AsForm();
            _form.BackColorChanged += BackColorChanged;
            _form.ForeColorChanged += ForeColorChanged;
            _form.FontChanged += FontChanged;
            _form.ParentChanged += ParentChanged;
            _form.BackColor = Color.Black;
            _form.ForeColor = Color.LightGray;
            _form.Font = new Font("Consolas", 10);
            _form.Dock = DockStyle.Fill;
            return _form;
        }

        public void Disconnect()
        {
            _terminal.Disconnect();
        }

        private void BackColorChanged(object sender, EventArgs e)
        {
            _renderProfile.BackColor = _form.BackColor;
        }

        private void ForeColorChanged(object sender, EventArgs e)
        {
            _renderProfile.ForeColor = _form.ForeColor;
        }

        private void FontChanged(object sender, EventArgs e)
        {
            _renderProfile.FontName = _form.Font.Name;
            _renderProfile.FontSize = _form.Font.Size;
        }

        private void ParentChanged(object sender, EventArgs e)
        {
            if (_currentParent == _form.ParentForm)
                return;

            if (_currentParent != null)
                _currentParent.FormClosing -= FormClosing;

            _currentParent = _form.ParentForm;

            if (_currentParent != null)
                _currentParent.FormClosing += FormClosing;
        }

        private void FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconnect();
        }

        public void SendText(string text)
        {
            string[] lines = text.Split('\n');
            bool endsWithNewline = text.EndsWith("\n");

            for (int i = 0; i < lines.Length; i++)
            {
                _terminal.TerminalSession.TerminalTransmission.SendString(lines[i].ToCharArray());

                if (i != lines.Length - 1 || endsWithNewline)
                    _terminal.TerminalSession.TerminalTransmission.SendLineBreak();
            }
        }

        public void Clear()
        {
            _terminal.TerminalSession.TerminalControl.GetDocument().Clear();
        }

        protected abstract void ConnectionAndSettings(out ITerminalConnection connection, out ITerminalSettings settings);
    }

    public class CygwinSession : Session
    {
        public ICygwinParameter ProtocolParameters { get; }

        public CygwinSession()
        {
            ProtocolParameters = _instance.ProtocolService.CreateDefaultCygwinParameter();
        }

        protected override void ConnectionAndSettings(out ITerminalConnection connection, out ITerminalSettings settings)
        {
            ISynchronizedConnector synchronizedConnector = _instance.ProtocolService.CreateFormBasedSynchronozedConnector(_terminal.Window);
            IInterruptable asyncConnection = _instance.ProtocolService.AsyncCygwinConnect(synchronizedConnector.InterruptableConnectorClient, ProtocolParameters);
            settings = new TerminalSettings();
            connection = synchronizedConnector.WaitConnection(asyncConnection, _instance.TerminalSessionsPlugin.TerminalSessionOptions.TerminalEstablishTimeout);
        }
    }

    public class CmdSession : Session
    {
        public PipeTerminalParameter ProtocolParameters { get; } = new PipeTerminalParameter()
        {
            ExeFilePath = Environment.ExpandEnvironmentVariables(@"%WinDir%\System32\conhost.exe")
        };

        protected override void ConnectionAndSettings(out ITerminalConnection connection, out ITerminalSettings settings)
        {
            settings = new PipeTerminalSettings();
            connection = PipeCreator.CreateNewPipeTerminalConnection(ProtocolParameters, (PipeTerminalSettings)settings);
        }
    }

    public class SerialSession : Session
    {
        public SerialTerminalParam ProtocolParameters { get; } = new SerialTerminalParam();

        protected override void ConnectionAndSettings(out ITerminalConnection connection, out ITerminalSettings settings)
        {
            settings = new SerialTerminalSettings();
            connection = SerialPortUtil.CreateNewSerialConnection(_terminal.Window, ProtocolParameters, (SerialTerminalSettings)settings);
        }
    }

    public class SSHSession : Session
    {
        public ITCPParameter ConnectionParameters { get; }
        public ISSHLoginParameter ProtocolParameters { get; }

        public SSHSession()
        {
            ProtocolParameters = _instance.ProtocolService.CreateDefaultSSHParameter();
            ConnectionParameters = ProtocolParameters as ITCPParameter;
        }

        protected override void ConnectionAndSettings(out ITerminalConnection connection, out ITerminalSettings settings)
        {
            ISynchronizedConnector synchronizedConnector = _instance.ProtocolService.CreateFormBasedSynchronozedConnector(_terminal.Window);
            IInterruptable asyncConnection = _instance.ProtocolService.AsyncSSHConnect(synchronizedConnector.InterruptableConnectorClient, ProtocolParameters);
            settings = new TerminalSettings();
            connection = synchronizedConnector.WaitConnection(asyncConnection, _instance.TerminalSessionsPlugin.TerminalSessionOptions.TerminalEstablishTimeout);
        }
    }

    public class TelnetSession : Session
    {
        public ITCPParameter ConnectionParameters { get; }

        public TelnetSession()
        {
            ConnectionParameters = _instance.ProtocolService.CreateDefaultTelnetParameter();
        }

        protected override void ConnectionAndSettings(out ITerminalConnection connection, out ITerminalSettings settings)
        {
            ISynchronizedConnector synchronizedConnector = _instance.ProtocolService.CreateFormBasedSynchronozedConnector(_terminal.Window);
            IInterruptable asyncConnection = _instance.ProtocolService.AsyncTelnetConnect(synchronizedConnector.InterruptableConnectorClient, ConnectionParameters);
            settings = new TerminalSettings();
            connection = synchronizedConnector.WaitConnection(asyncConnection, _instance.TerminalSessionsPlugin.TerminalSessionOptions.TerminalEstablishTimeout);
        }
    }
}