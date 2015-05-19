using System;
using System.Diagnostics;
using System.Windows.Forms;



namespace QQPlus.WeiboSync
{
    public partial class ConsoleForm : Form
    {
        public ConsoleForm()
        {
            InitializeComponent();
            Trace.Listeners.Add(new ListBoxLogTraceListener(ListBoxShow));
        }

        public ConsoleForm(Main main)
            : this()
        {
            this.main = main;
        }

        public void TraceLog(string message)
        {
            Trace.WriteLine(message);
        }

        private void FetchButton_Click(object sender, EventArgs e)
        {
            this.main.WakeUpMainThread();
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            if (!ListBoxShow.Visible)
                return;

            ListBoxShow.Items.Clear();

        }

        //private void ResendBtn_Click(object sender, EventArgs e)
        //{
            
        //    this.main.ResendWeibo();
        //}

        //private void ResendDuitangBtn_Click(object sender, EventArgs e)
        //{
        //    this.main.ResendWeiboDuitang();
        //}

        //private void ResendTestBtn_Click(object sender, EventArgs e)
        //{
        //    this.main.ResendWeiboTest();
        //}

    }


    public class ListBoxLogTraceListener : DefaultTraceListener
    {
        private ListBox m_ListBox { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListBoxLogTraceListener"/> class.
        /// </summary>
        /// <param name="listBox">The list box.</param>
        public ListBoxLogTraceListener(ListBox listBox)
        {
            m_ListBox = listBox;
        }

        /// <summary>
        /// Writes the output to the OutputDebugString function and to the <see cref="M:System.Diagnostics.Debugger.Log(System.Int32,System.String,System.String)"/> method, followed by a carriage return and line feed (\r\n).
        /// </summary>
        /// <param name="message">The message to write to OutputDebugString and <see cref="M:System.Diagnostics.Debugger.Log(System.Int32,System.String,System.String)"/>.</param>
        /// <PermissionSet>
        /// 	<IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
        /// 	<IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence"/>
        /// </PermissionSet>
        public override void WriteLine(string message)
        {

            if (!m_ListBox.Visible)
                return;

            if (m_ListBox.InvokeRequired)
            {
                m_ListBox.BeginInvoke(new MethodInvoker(
                       delegate { WriteLine(message); }
                       ));
                return;
            }


            Console.WriteLine( m_ListBox.Items.Count );

            if(m_ListBox.Items.Count > 200)
            {
                m_ListBox.Items.Clear();
            }
            int rowsize = 180;
            bool hascut = false;
            while(message.Length > 0)
            {
                int slen = Math.Min(rowsize, message.Length);
                string messhead = message.Substring(0, slen);
                message = message.Substring(slen);
                m_ListBox.Items.Add(string.Format("{0}\t{1}", (hascut ? "                 " : DateTime.Now.ToString()), messhead));
                hascut = true;
            }

        }
    }
}
