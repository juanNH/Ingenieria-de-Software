namespace UI
{
    partial class RolesView_380_jh
    {
        private System.ComponentModel.IContainer components_380_jh = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components_380_jh != null))
            {
                components_380_jh.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent_380_jh()
        {
            this.lblTitulo_380_jh = new System.Windows.Forms.Label();
            this.lblDescripcion_380_jh = new System.Windows.Forms.Label();
            this.groupBoxRoles_380_jh = new System.Windows.Forms.GroupBox();
            this.treeViewRoles_380_jh = new System.Windows.Forms.TreeView();
            this.groupBoxRoles_380_jh.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo_380_jh.AutoSize = true;
            this.lblTitulo_380_jh.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitulo_380_jh.Location = new System.Drawing.Point(18, 18);
            this.lblTitulo_380_jh.Name = "lblTitulo";
            this.lblTitulo_380_jh.Size = new System.Drawing.Size(76, 32);
            this.lblTitulo_380_jh.TabIndex = 0;
            this.lblTitulo_380_jh.Text = "Roles";
            // 
            // lblDescripcion
            // 
            this.lblDescripcion_380_jh.AutoSize = true;
            this.lblDescripcion_380_jh.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblDescripcion_380_jh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.lblDescripcion_380_jh.Location = new System.Drawing.Point(20, 58);
            this.lblDescripcion_380_jh.Name = "lblDescripcion";
            this.lblDescripcion_380_jh.Size = new System.Drawing.Size(302, 19);
            this.lblDescripcion_380_jh.TabIndex = 1;
            this.lblDescripcion_380_jh.Text = "Pantalla base para asignacion y gestion de roles.";
            // 
            // groupBoxRoles
            // 
            this.groupBoxRoles_380_jh.Controls.Add(this.treeViewRoles_380_jh);
            this.groupBoxRoles_380_jh.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.groupBoxRoles_380_jh.Location = new System.Drawing.Point(24, 98);
            this.groupBoxRoles_380_jh.Name = "groupBoxRoles";
            this.groupBoxRoles_380_jh.Size = new System.Drawing.Size(420, 286);
            this.groupBoxRoles_380_jh.TabIndex = 2;
            this.groupBoxRoles_380_jh.TabStop = false;
            this.groupBoxRoles_380_jh.Text = "Estructura de roles";
            // 
            // treeViewRoles
            // 
            this.treeViewRoles_380_jh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewRoles_380_jh.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.treeViewRoles_380_jh.Location = new System.Drawing.Point(3, 21);
            this.treeViewRoles_380_jh.Name = "treeViewRoles";
            this.treeViewRoles_380_jh.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            new System.Windows.Forms.TreeNode("Administrador", new System.Windows.Forms.TreeNode[] {
            new System.Windows.Forms.TreeNode("Gestion de permisos"),
            new System.Windows.Forms.TreeNode("Gestion de roles")}),
            new System.Windows.Forms.TreeNode("Auditor", new System.Windows.Forms.TreeNode[] {
            new System.Windows.Forms.TreeNode("Consulta de bitacora")})});
            this.treeViewRoles_380_jh.Size = new System.Drawing.Size(414, 262);
            this.treeViewRoles_380_jh.TabIndex = 0;
            // 
            // RolesView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.groupBoxRoles_380_jh);
            this.Controls.Add(this.lblDescripcion_380_jh);
            this.Controls.Add(this.lblTitulo_380_jh);
            this.Name = "RolesView";
            this.Size = new System.Drawing.Size(900, 520);
            this.groupBoxRoles_380_jh.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitulo_380_jh;
        private System.Windows.Forms.Label lblDescripcion_380_jh;
        private System.Windows.Forms.GroupBox groupBoxRoles_380_jh;
        private System.Windows.Forms.TreeView treeViewRoles_380_jh;
    }
}
