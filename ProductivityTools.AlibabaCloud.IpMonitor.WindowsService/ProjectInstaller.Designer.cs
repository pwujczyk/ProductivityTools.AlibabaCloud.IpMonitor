namespace ProductivityTools.AlibabaCloud.IpMonitor
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.PTIPMonitorProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.PTIPMonitorServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // PTIPMonitorProcessInstaller
            // 
            this.PTIPMonitorProcessInstaller.Password = null;
            this.PTIPMonitorProcessInstaller.Username = null;
            // 
            // PTIPMonitorServiceInstaller
            // 
            this.PTIPMonitorServiceInstaller.ServiceName = "ProductivityTools.Alibaba.IpMonitor";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.PTIPMonitorProcessInstaller,
            this.PTIPMonitorServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller PTIPMonitorProcessInstaller;
        private System.ServiceProcess.ServiceInstaller PTIPMonitorServiceInstaller;
    }
}