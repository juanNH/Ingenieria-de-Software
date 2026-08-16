namespace UI
{
    partial class UsuariosView_380_jh
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
            this.dgvUsuarios_380_jh = new System.Windows.Forms.DataGridView();
            this.columnId_380_jh = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnUsuario_380_jh = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnEmail_380_jh = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnEstado_380_jh = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnBloqueoDigitoVerificador_380_jh = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.groupBoxDetalle_380_jh = new System.Windows.Forms.GroupBox();
            this.cmbEstado_380_jh = new System.Windows.Forms.ComboBox();
            this.lblEstado_380_jh = new System.Windows.Forms.Label();
            this.btnInhabilitar_380_jh = new System.Windows.Forms.Button();
            this.btnGuardar_380_jh = new System.Windows.Forms.Button();
            this.btnNuevo_380_jh = new System.Windows.Forms.Button();
            this.txtPassword_380_jh = new System.Windows.Forms.TextBox();
            this.lblPassword_380_jh = new System.Windows.Forms.Label();
            this.txtApellido_380_jh = new System.Windows.Forms.TextBox();
            this.lblApellido_380_jh = new System.Windows.Forms.Label();
            this.txtNombre_380_jh = new System.Windows.Forms.TextBox();
            this.lblNombre_380_jh = new System.Windows.Forms.Label();
            this.txtEmail_380_jh = new System.Windows.Forms.TextBox();
            this.lblEmail_380_jh = new System.Windows.Forms.Label();
            this.txtUsuario_380_jh = new System.Windows.Forms.TextBox();
            this.lblUsuario_380_jh = new System.Windows.Forms.Label();
            this.lblModo_380_jh = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsuarios_380_jh)).BeginInit();
            this.groupBoxDetalle_380_jh.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo_380_jh.AutoSize = true;
            this.lblTitulo_380_jh.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitulo_380_jh.Location = new System.Drawing.Point(18, 18);
            this.lblTitulo_380_jh.Name = "lblTitulo";
            this.lblTitulo_380_jh.Size = new System.Drawing.Size(110, 32);
            this.lblTitulo_380_jh.TabIndex = 0;
            this.lblTitulo_380_jh.Text = "Usuarios";
            // 
            // lblDescripcion
            // 
            this.lblDescripcion_380_jh.AutoSize = true;
            this.lblDescripcion_380_jh.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblDescripcion_380_jh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.lblDescripcion_380_jh.Location = new System.Drawing.Point(20, 58);
            this.lblDescripcion_380_jh.Name = "lblDescripcion";
            this.lblDescripcion_380_jh.Size = new System.Drawing.Size(355, 19);
            this.lblDescripcion_380_jh.TabIndex = 1;
            this.lblDescripcion_380_jh.Text = "Alta, modificacion e inhabilitacion de usuarios del sistema.";
            // 
            // dgvUsuarios
            // 
            this.dgvUsuarios_380_jh.AllowUserToAddRows = false;
            this.dgvUsuarios_380_jh.AllowUserToDeleteRows = false;
            this.dgvUsuarios_380_jh.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvUsuarios_380_jh.BackgroundColor = System.Drawing.Color.White;
            this.dgvUsuarios_380_jh.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUsuarios_380_jh.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnId_380_jh,
            this.columnUsuario_380_jh,
            this.columnEmail_380_jh,
            this.columnEstado_380_jh,
            this.columnBloqueoDigitoVerificador_380_jh});
            this.dgvUsuarios_380_jh.Location = new System.Drawing.Point(24, 98);
            this.dgvUsuarios_380_jh.MultiSelect = false;
            this.dgvUsuarios_380_jh.Name = "dgvUsuarios";
            this.dgvUsuarios_380_jh.ReadOnly = true;
            this.dgvUsuarios_380_jh.RowHeadersVisible = false;
            this.dgvUsuarios_380_jh.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvUsuarios_380_jh.Size = new System.Drawing.Size(520, 390);
            this.dgvUsuarios_380_jh.TabIndex = 2;
            this.dgvUsuarios_380_jh.SelectionChanged += new System.EventHandler(this.dgvUsuarios_SelectionChanged_380_jh);
            // 
            // columnId
            // 
            this.columnId_380_jh.DataPropertyName = "Id";
            this.columnId_380_jh.HeaderText = "Id";
            this.columnId_380_jh.Name = "columnId";
            this.columnId_380_jh.ReadOnly = true;
            this.columnId_380_jh.Width = 60;
            // 
            // columnUsuario
            // 
            this.columnUsuario_380_jh.DataPropertyName = "Username";
            this.columnUsuario_380_jh.HeaderText = "Usuario";
            this.columnUsuario_380_jh.Name = "columnUsuario";
            this.columnUsuario_380_jh.ReadOnly = true;
            this.columnUsuario_380_jh.Width = 150;
            // 
            // columnEmail
            // 
            this.columnEmail_380_jh.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnEmail_380_jh.DataPropertyName = "Email";
            this.columnEmail_380_jh.HeaderText = "Email";
            this.columnEmail_380_jh.Name = "columnEmail";
            this.columnEmail_380_jh.ReadOnly = true;
            // 
            // columnEstado
            // 
            this.columnEstado_380_jh.DataPropertyName = "Estado";
            this.columnEstado_380_jh.HeaderText = "Estado";
            this.columnEstado_380_jh.Name = "columnEstado";
            this.columnEstado_380_jh.ReadOnly = true;
            this.columnEstado_380_jh.Width = 95;
            // 
            // columnBloqueoDigitoVerificador
            // 
            this.columnBloqueoDigitoVerificador_380_jh.DataPropertyName = "BloqueoDigitoVerificador";
            this.columnBloqueoDigitoVerificador_380_jh.HeaderText = "Bloqueo DV";
            this.columnBloqueoDigitoVerificador_380_jh.Name = "columnBloqueoDigitoVerificador";
            this.columnBloqueoDigitoVerificador_380_jh.ReadOnly = true;
            this.columnBloqueoDigitoVerificador_380_jh.Width = 90;
            // 
            // groupBoxDetalle
            // 
            this.groupBoxDetalle_380_jh.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxDetalle_380_jh.Controls.Add(this.cmbEstado_380_jh);
            this.groupBoxDetalle_380_jh.Controls.Add(this.lblEstado_380_jh);
            this.groupBoxDetalle_380_jh.Controls.Add(this.btnInhabilitar_380_jh);
            this.groupBoxDetalle_380_jh.Controls.Add(this.btnGuardar_380_jh);
            this.groupBoxDetalle_380_jh.Controls.Add(this.btnNuevo_380_jh);
            this.groupBoxDetalle_380_jh.Controls.Add(this.txtPassword_380_jh);
            this.groupBoxDetalle_380_jh.Controls.Add(this.lblPassword_380_jh);
            this.groupBoxDetalle_380_jh.Controls.Add(this.txtApellido_380_jh);
            this.groupBoxDetalle_380_jh.Controls.Add(this.lblApellido_380_jh);
            this.groupBoxDetalle_380_jh.Controls.Add(this.txtNombre_380_jh);
            this.groupBoxDetalle_380_jh.Controls.Add(this.lblNombre_380_jh);
            this.groupBoxDetalle_380_jh.Controls.Add(this.txtEmail_380_jh);
            this.groupBoxDetalle_380_jh.Controls.Add(this.lblEmail_380_jh);
            this.groupBoxDetalle_380_jh.Controls.Add(this.txtUsuario_380_jh);
            this.groupBoxDetalle_380_jh.Controls.Add(this.lblUsuario_380_jh);
            this.groupBoxDetalle_380_jh.Controls.Add(this.lblModo_380_jh);
            this.groupBoxDetalle_380_jh.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.groupBoxDetalle_380_jh.Location = new System.Drawing.Point(568, 98);
            this.groupBoxDetalle_380_jh.Name = "groupBoxDetalle";
            this.groupBoxDetalle_380_jh.Size = new System.Drawing.Size(308, 390);
            this.groupBoxDetalle_380_jh.TabIndex = 3;
            this.groupBoxDetalle_380_jh.TabStop = false;
            this.groupBoxDetalle_380_jh.Text = "Detalle";
            // 
            // cmbEstado
            // 
            this.cmbEstado_380_jh.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEstado_380_jh.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbEstado_380_jh.FormattingEnabled = true;
            this.cmbEstado_380_jh.Location = new System.Drawing.Point(18, 291);
            this.cmbEstado_380_jh.Name = "cmbEstado";
            this.cmbEstado_380_jh.Size = new System.Drawing.Size(272, 25);
            this.cmbEstado_380_jh.TabIndex = 11;
            // 
            // lblEstado
            // 
            this.lblEstado_380_jh.AutoSize = true;
            this.lblEstado_380_jh.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEstado_380_jh.Location = new System.Drawing.Point(15, 273);
            this.lblEstado_380_jh.Name = "lblEstado";
            this.lblEstado_380_jh.Size = new System.Drawing.Size(42, 15);
            this.lblEstado_380_jh.TabIndex = 10;
            this.lblEstado_380_jh.Text = "Estado";
            // 
            // btnInhabilitar
            // 
            this.btnInhabilitar_380_jh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInhabilitar_380_jh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.btnInhabilitar_380_jh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInhabilitar_380_jh.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnInhabilitar_380_jh.ForeColor = System.Drawing.Color.White;
            this.btnInhabilitar_380_jh.Location = new System.Drawing.Point(197, 343);
            this.btnInhabilitar_380_jh.Name = "btnInhabilitar";
            this.btnInhabilitar_380_jh.Size = new System.Drawing.Size(93, 31);
            this.btnInhabilitar_380_jh.TabIndex = 14;
            this.btnInhabilitar_380_jh.Text = "Inhabilitar";
            this.btnInhabilitar_380_jh.UseVisualStyleBackColor = false;
            this.btnInhabilitar_380_jh.Click += new System.EventHandler(this.btnInhabilitar_Click_380_jh);
            // 
            // btnGuardar
            // 
            this.btnGuardar_380_jh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardar_380_jh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(110)))), ((int)(((byte)(253)))));
            this.btnGuardar_380_jh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar_380_jh.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnGuardar_380_jh.ForeColor = System.Drawing.Color.White;
            this.btnGuardar_380_jh.Location = new System.Drawing.Point(104, 343);
            this.btnGuardar_380_jh.Name = "btnGuardar";
            this.btnGuardar_380_jh.Size = new System.Drawing.Size(87, 31);
            this.btnGuardar_380_jh.TabIndex = 13;
            this.btnGuardar_380_jh.Text = "Guardar";
            this.btnGuardar_380_jh.UseVisualStyleBackColor = false;
            this.btnGuardar_380_jh.Click += new System.EventHandler(this.btnGuardar_Click_380_jh);
            // 
            // btnNuevo
            // 
            this.btnNuevo_380_jh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNuevo_380_jh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.btnNuevo_380_jh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNuevo_380_jh.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnNuevo_380_jh.ForeColor = System.Drawing.Color.White;
            this.btnNuevo_380_jh.Location = new System.Drawing.Point(18, 343);
            this.btnNuevo_380_jh.Name = "btnNuevo";
            this.btnNuevo_380_jh.Size = new System.Drawing.Size(80, 31);
            this.btnNuevo_380_jh.TabIndex = 12;
            this.btnNuevo_380_jh.Text = "Nuevo";
            this.btnNuevo_380_jh.UseVisualStyleBackColor = false;
            this.btnNuevo_380_jh.Click += new System.EventHandler(this.btnNuevo_Click_380_jh);
            // 
            // txtPassword
            // 
            this.txtPassword_380_jh.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtPassword_380_jh.Location = new System.Drawing.Point(18, 241);
            this.txtPassword_380_jh.Name = "txtPassword";
            this.txtPassword_380_jh.PasswordChar = '*';
            this.txtPassword_380_jh.Size = new System.Drawing.Size(272, 25);
            this.txtPassword_380_jh.TabIndex = 9;
            // 
            // lblPassword
            // 
            this.lblPassword_380_jh.AutoSize = true;
            this.lblPassword_380_jh.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPassword_380_jh.Location = new System.Drawing.Point(15, 223);
            this.lblPassword_380_jh.Name = "lblPassword";
            this.lblPassword_380_jh.Size = new System.Drawing.Size(129, 15);
            this.lblPassword_380_jh.TabIndex = 8;
            this.lblPassword_380_jh.Text = "Contrasena nueva";
            // 
            // txtApellido
            // 
            this.txtApellido_380_jh.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtApellido_380_jh.Location = new System.Drawing.Point(18, 193);
            this.txtApellido_380_jh.Name = "txtApellido";
            this.txtApellido_380_jh.Size = new System.Drawing.Size(272, 25);
            this.txtApellido_380_jh.TabIndex = 7;
            // 
            // lblApellido
            // 
            this.lblApellido_380_jh.AutoSize = true;
            this.lblApellido_380_jh.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblApellido_380_jh.Location = new System.Drawing.Point(15, 175);
            this.lblApellido_380_jh.Name = "lblApellido";
            this.lblApellido_380_jh.Size = new System.Drawing.Size(51, 15);
            this.lblApellido_380_jh.TabIndex = 6;
            this.lblApellido_380_jh.Text = "Apellido";
            // 
            // txtNombre
            // 
            this.txtNombre_380_jh.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtNombre_380_jh.Location = new System.Drawing.Point(18, 145);
            this.txtNombre_380_jh.Name = "txtNombre";
            this.txtNombre_380_jh.Size = new System.Drawing.Size(272, 25);
            this.txtNombre_380_jh.TabIndex = 5;
            // 
            // lblNombre
            // 
            this.lblNombre_380_jh.AutoSize = true;
            this.lblNombre_380_jh.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblNombre_380_jh.Location = new System.Drawing.Point(15, 127);
            this.lblNombre_380_jh.Name = "lblNombre";
            this.lblNombre_380_jh.Size = new System.Drawing.Size(51, 15);
            this.lblNombre_380_jh.TabIndex = 4;
            this.lblNombre_380_jh.Text = "Nombre";
            // 
            // txtEmail
            // 
            this.txtEmail_380_jh.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtEmail_380_jh.Location = new System.Drawing.Point(18, 97);
            this.txtEmail_380_jh.Name = "txtEmail";
            this.txtEmail_380_jh.Size = new System.Drawing.Size(272, 25);
            this.txtEmail_380_jh.TabIndex = 3;
            // 
            // lblEmail
            // 
            this.lblEmail_380_jh.AutoSize = true;
            this.lblEmail_380_jh.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEmail_380_jh.Location = new System.Drawing.Point(15, 79);
            this.lblEmail_380_jh.Name = "lblEmail";
            this.lblEmail_380_jh.Size = new System.Drawing.Size(36, 15);
            this.lblEmail_380_jh.TabIndex = 2;
            this.lblEmail_380_jh.Text = "Email";
            // 
            // txtUsuario
            // 
            this.txtUsuario_380_jh.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtUsuario_380_jh.Location = new System.Drawing.Point(18, 49);
            this.txtUsuario_380_jh.Name = "txtUsuario";
            this.txtUsuario_380_jh.Size = new System.Drawing.Size(272, 25);
            this.txtUsuario_380_jh.TabIndex = 1;
            // 
            // lblUsuario
            // 
            this.lblUsuario_380_jh.AutoSize = true;
            this.lblUsuario_380_jh.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblUsuario_380_jh.Location = new System.Drawing.Point(15, 31);
            this.lblUsuario_380_jh.Name = "lblUsuario";
            this.lblUsuario_380_jh.Size = new System.Drawing.Size(47, 15);
            this.lblUsuario_380_jh.TabIndex = 0;
            this.lblUsuario_380_jh.Text = "Usuario";
            // 
            // lblModo
            // 
            this.lblModo_380_jh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblModo_380_jh.AutoSize = true;
            this.lblModo_380_jh.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblModo_380_jh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.lblModo_380_jh.Location = new System.Drawing.Point(15, 321);
            this.lblModo_380_jh.Name = "lblModo";
            this.lblModo_380_jh.Size = new System.Drawing.Size(74, 15);
            this.lblModo_380_jh.TabIndex = 15;
            this.lblModo_380_jh.Text = "Crear usuario";
            // 
            // UsuariosView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.groupBoxDetalle_380_jh);
            this.Controls.Add(this.dgvUsuarios_380_jh);
            this.Controls.Add(this.lblDescripcion_380_jh);
            this.Controls.Add(this.lblTitulo_380_jh);
            this.Name = "UsuariosView";
            this.Size = new System.Drawing.Size(900, 520);
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsuarios_380_jh)).EndInit();
            this.groupBoxDetalle_380_jh.ResumeLayout(false);
            this.groupBoxDetalle_380_jh.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitulo_380_jh;
        private System.Windows.Forms.Label lblDescripcion_380_jh;
        private System.Windows.Forms.DataGridView dgvUsuarios_380_jh;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnId_380_jh;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnUsuario_380_jh;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnEmail_380_jh;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnEstado_380_jh;
        private System.Windows.Forms.DataGridViewCheckBoxColumn columnBloqueoDigitoVerificador_380_jh;
        private System.Windows.Forms.GroupBox groupBoxDetalle_380_jh;
        private System.Windows.Forms.ComboBox cmbEstado_380_jh;
        private System.Windows.Forms.Label lblEstado_380_jh;
        private System.Windows.Forms.Button btnInhabilitar_380_jh;
        private System.Windows.Forms.Button btnGuardar_380_jh;
        private System.Windows.Forms.Button btnNuevo_380_jh;
        private System.Windows.Forms.TextBox txtPassword_380_jh;
        private System.Windows.Forms.Label lblPassword_380_jh;
        private System.Windows.Forms.TextBox txtApellido_380_jh;
        private System.Windows.Forms.Label lblApellido_380_jh;
        private System.Windows.Forms.TextBox txtNombre_380_jh;
        private System.Windows.Forms.Label lblNombre_380_jh;
        private System.Windows.Forms.TextBox txtEmail_380_jh;
        private System.Windows.Forms.Label lblEmail_380_jh;
        private System.Windows.Forms.TextBox txtUsuario_380_jh;
        private System.Windows.Forms.Label lblUsuario_380_jh;
        private System.Windows.Forms.Label lblModo_380_jh;
    }
}
