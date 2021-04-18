using System.Drawing;
using System.Windows.Forms;

using Poderosa.Library;

namespace LibPoderosaExample
{
    public partial class ExampleForm : Form
    {
        public ExampleForm()
        {
            InitializeComponent();
            TelnetSession telnetSession = new TelnetSession();
            telnetSession.ConnectionParameters.Destination = "1984.ws";
            SSHSession sshSession = new SSHSession();
            sshSession.ConnectionParameters.Destination = "test.rebex.net";
            sshSession.ProtocolParameters.Account = "demo";
            sshSession.ProtocolParameters.PasswordOrPassphrase = "password";
            sshSession.ProtocolParameters.AuthenticationType = Granados.AuthenticationType.Password;
            CmdSession cmdSession = new CmdSession();
            CygwinSession cygwinSession = new CygwinSession();

            try
            {
                Form telnetForm = telnetSession.Connect();
                TopSplitContainer.Panel1.Controls.Add(telnetForm);
            }
            catch
            {}

            try
            {
                Form sshForm = sshSession.Connect();
                sshForm.ForeColor = Color.Green;
                TopSplitContainer.Panel2.Controls.Add(sshForm);
            }
            catch
            {}

            try
            {
                Form cmdForm = cmdSession.Connect();
                cmdForm.BackColor = Color.Blue;
                BottomSplitContainer.Panel1.Controls.Add(cmdForm);
                cmdSession.SendText("cd %UserProfile%\ncls\n");
            }
            catch
            {}

            try
            {
                Form cygwinForm = cygwinSession.Connect();
                cygwinForm.ForeColor = Color.Red;
                BottomSplitContainer.Panel2.Controls.Add(cygwinForm);
            }
            catch
            {}
        }
    }
}
