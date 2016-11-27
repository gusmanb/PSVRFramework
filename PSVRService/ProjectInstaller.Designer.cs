namespace PSVRService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.psvrProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.psvrServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // psvrProcessInstaller
            // 
            this.psvrProcessInstaller.Account = System.ServiceProcess.ServiceAccount.NetworkService;
            this.psvrProcessInstaller.Password = null;
            this.psvrProcessInstaller.Username = null;
            // 
            // psvrServiceInstaller
            // 
            this.psvrServiceInstaller.DisplayName = "PlayStation VR Server";
            this.psvrServiceInstaller.ServiceName = "PSVRServer";
            this.psvrServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.psvrProcessInstaller,
            this.psvrServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller psvrProcessInstaller;
        private System.ServiceProcess.ServiceInstaller psvrServiceInstaller;
    }
}