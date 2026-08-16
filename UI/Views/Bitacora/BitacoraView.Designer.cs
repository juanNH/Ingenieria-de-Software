namespace UI
{
    partial class BitacoraView_380_jh
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
            this.listViewBitacora_380_jh = new System.Windows.Forms.ListView();
            this.columnId_380_jh = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnFecha_380_jh = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnIdUsuario_380_jh = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnUsuario_380_jh = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnModulo_380_jh = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnAccion_380_jh = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnNivel_380_jh = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnDescripcion_380_jh = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnEquipo_380_jh = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblEstado_380_jh = new System.Windows.Forms.Label();
            this.btnActualizar_380_jh = new System.Windows.Forms.Button();
            this.grpFiltros_380_jh = new System.Windows.Forms.GroupBox();
            this.lblFechaDesde_380_jh = new System.Windows.Forms.Label();
            this.dtpFechaDesde_380_jh = new System.Windows.Forms.DateTimePicker();
            this.lblFechaHasta_380_jh = new System.Windows.Forms.Label();
            this.dtpFechaHasta_380_jh = new System.Windows.Forms.DateTimePicker();
            this.lblUsuarioFiltro_380_jh = new System.Windows.Forms.Label();
            this.txtUsuarioFiltro_380_jh = new System.Windows.Forms.TextBox();
            this.lblModuloFiltro_380_jh = new System.Windows.Forms.Label();
            this.cmbModuloFiltro_380_jh = new System.Windows.Forms.ComboBox();
            this.lblAccionFiltro_380_jh = new System.Windows.Forms.Label();
            this.cmbAccionFiltro_380_jh = new System.Windows.Forms.ComboBox();
            this.lblNivelFiltro_380_jh = new System.Windows.Forms.Label();
            this.cmbNivelFiltro_380_jh = new System.Windows.Forms.ComboBox();
            this.lblDescripcionFiltro_380_jh = new System.Windows.Forms.Label();
            this.txtDescripcionFiltro_380_jh = new System.Windows.Forms.TextBox();
            this.btnBuscar_380_jh = new System.Windows.Forms.Button();
            this.btnLimpiarFiltros_380_jh = new System.Windows.Forms.Button();
            this.grpFiltros_380_jh.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo_380_jh.AutoSize = true;
            this.lblTitulo_380_jh.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitulo_380_jh.Location = new System.Drawing.Point(18, 18);
            this.lblTitulo_380_jh.Name = "lblTitulo";
            this.lblTitulo_380_jh.Size = new System.Drawing.Size(117, 32);
            this.lblTitulo_380_jh.TabIndex = 0;
            this.lblTitulo_380_jh.Text = "Bitacora";
            // 
            // lblDescripcion
            // 
            this.lblDescripcion_380_jh.AutoSize = true;
            this.lblDescripcion_380_jh.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblDescripcion_380_jh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.lblDescripcion_380_jh.Location = new System.Drawing.Point(20, 58);
            this.lblDescripcion_380_jh.Name = "lblDescripcion";
            this.lblDescripcion_380_jh.Size = new System.Drawing.Size(320, 19);
            this.lblDescripcion_380_jh.TabIndex = 1;
            this.lblDescripcion_380_jh.Text = "Aca vas a poder consultar los eventos del sistema.";
            // 
            // listViewBitacora
            // 
            this.listViewBitacora_380_jh.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnId_380_jh,
            this.columnFecha_380_jh,
            this.columnIdUsuario_380_jh,
            this.columnUsuario_380_jh,
            this.columnModulo_380_jh,
            this.columnAccion_380_jh,
            this.columnNivel_380_jh,
            this.columnDescripcion_380_jh,
            this.columnEquipo_380_jh});
            this.listViewBitacora_380_jh.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewBitacora_380_jh.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.listViewBitacora_380_jh.FullRowSelect = true;
            this.listViewBitacora_380_jh.GridLines = true;
            this.listViewBitacora_380_jh.HideSelection = false;
            this.listViewBitacora_380_jh.Location = new System.Drawing.Point(24, 210);
            this.listViewBitacora_380_jh.Name = "listViewBitacora";
            this.listViewBitacora_380_jh.Size = new System.Drawing.Size(852, 241);
            this.listViewBitacora_380_jh.TabIndex = 2;
            this.listViewBitacora_380_jh.UseCompatibleStateImageBehavior = false;
            this.listViewBitacora_380_jh.View = System.Windows.Forms.View.Details;
            // 
            // columnId
            // 
            this.columnId_380_jh.Text = "Id";
            this.columnId_380_jh.Width = 55;
            // 
            // columnFecha
            // 
            this.columnFecha_380_jh.Text = "Fecha";
            this.columnFecha_380_jh.Width = 150;
            // 
            // columnIdUsuario
            // 
            this.columnIdUsuario_380_jh.Text = "Id usuario";
            this.columnIdUsuario_380_jh.Width = 80;
            // 
            // columnUsuario
            // 
            this.columnUsuario_380_jh.Text = "Usuario";
            this.columnUsuario_380_jh.Width = 170;
            // 
            // columnModulo
            // 
            this.columnModulo_380_jh.Text = "Modulo";
            this.columnModulo_380_jh.Width = 110;
            // 
            // columnAccion
            // 
            this.columnAccion_380_jh.Text = "Accion";
            this.columnAccion_380_jh.Width = 120;
            // 
            // columnNivel
            // 
            this.columnNivel_380_jh.Text = "Nivel";
            this.columnNivel_380_jh.Width = 105;
            // 
            // columnDescripcion
            // 
            this.columnDescripcion_380_jh.Text = "Descripcion";
            this.columnDescripcion_380_jh.Width = 260;
            // 
            // columnEquipo
            // 
            this.columnEquipo_380_jh.Text = "Equipo";
            this.columnEquipo_380_jh.Width = 150;
            // 
            // lblEstado
            // 
            this.lblEstado_380_jh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblEstado_380_jh.AutoSize = true;
            this.lblEstado_380_jh.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEstado_380_jh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.lblEstado_380_jh.Location = new System.Drawing.Point(24, 464);
            this.lblEstado_380_jh.Name = "lblEstado";
            this.lblEstado_380_jh.Size = new System.Drawing.Size(127, 15);
            this.lblEstado_380_jh.TabIndex = 3;
            this.lblEstado_380_jh.Text = "Cargando bitacora...";
            // 
            // btnActualizar
            // 
            this.btnActualizar_380_jh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnActualizar_380_jh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnActualizar_380_jh.FlatAppearance.BorderSize = 0;
            this.btnActualizar_380_jh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnActualizar_380_jh.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnActualizar_380_jh.ForeColor = System.Drawing.Color.White;
            this.btnActualizar_380_jh.Location = new System.Drawing.Point(752, 53);
            this.btnActualizar_380_jh.Name = "btnActualizar";
            this.btnActualizar_380_jh.Size = new System.Drawing.Size(124, 32);
            this.btnActualizar_380_jh.TabIndex = 4;
            this.btnActualizar_380_jh.Text = "Actualizar";
            this.btnActualizar_380_jh.UseVisualStyleBackColor = false;
            this.btnActualizar_380_jh.Click += new System.EventHandler(this.btnActualizar_Click_380_jh);
            //
            // grpFiltros
            //
            this.grpFiltros_380_jh.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.grpFiltros_380_jh.Controls.Add(this.lblFechaDesde_380_jh);
            this.grpFiltros_380_jh.Controls.Add(this.dtpFechaDesde_380_jh);
            this.grpFiltros_380_jh.Controls.Add(this.lblFechaHasta_380_jh);
            this.grpFiltros_380_jh.Controls.Add(this.dtpFechaHasta_380_jh);
            this.grpFiltros_380_jh.Controls.Add(this.lblUsuarioFiltro_380_jh);
            this.grpFiltros_380_jh.Controls.Add(this.txtUsuarioFiltro_380_jh);
            this.grpFiltros_380_jh.Controls.Add(this.lblModuloFiltro_380_jh);
            this.grpFiltros_380_jh.Controls.Add(this.cmbModuloFiltro_380_jh);
            this.grpFiltros_380_jh.Controls.Add(this.lblAccionFiltro_380_jh);
            this.grpFiltros_380_jh.Controls.Add(this.cmbAccionFiltro_380_jh);
            this.grpFiltros_380_jh.Controls.Add(this.lblNivelFiltro_380_jh);
            this.grpFiltros_380_jh.Controls.Add(this.cmbNivelFiltro_380_jh);
            this.grpFiltros_380_jh.Controls.Add(this.lblDescripcionFiltro_380_jh);
            this.grpFiltros_380_jh.Controls.Add(this.txtDescripcionFiltro_380_jh);
            this.grpFiltros_380_jh.Controls.Add(this.btnBuscar_380_jh);
            this.grpFiltros_380_jh.Controls.Add(this.btnLimpiarFiltros_380_jh);
            this.grpFiltros_380_jh.Location = new System.Drawing.Point(24, 86);
            this.grpFiltros_380_jh.Name = "grpFiltros";
            this.grpFiltros_380_jh.Size = new System.Drawing.Size(852, 112);
            this.grpFiltros_380_jh.TabIndex = 5;
            this.grpFiltros_380_jh.TabStop = false;
            this.grpFiltros_380_jh.Text = "Filtros";
            //
            // lblFechaDesde
            //
            this.lblFechaDesde_380_jh.AutoSize = true;
            this.lblFechaDesde_380_jh.Location = new System.Drawing.Point(12, 25);
            this.lblFechaDesde_380_jh.Name = "lblFechaDesde";
            this.lblFechaDesde_380_jh.Size = new System.Drawing.Size(43, 15);
            this.lblFechaDesde_380_jh.TabIndex = 0;
            this.lblFechaDesde_380_jh.Text = "Desde";
            //
            // dtpFechaDesde
            //
            this.dtpFechaDesde_380_jh.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFechaDesde_380_jh.Location = new System.Drawing.Point(58, 20);
            this.dtpFechaDesde_380_jh.Name = "dtpFechaDesde";
            this.dtpFechaDesde_380_jh.ShowCheckBox = true;
            this.dtpFechaDesde_380_jh.Size = new System.Drawing.Size(105, 23);
            this.dtpFechaDesde_380_jh.TabIndex = 1;
            //
            // lblFechaHasta
            //
            this.lblFechaHasta_380_jh.AutoSize = true;
            this.lblFechaHasta_380_jh.Location = new System.Drawing.Point(173, 25);
            this.lblFechaHasta_380_jh.Name = "lblFechaHasta";
            this.lblFechaHasta_380_jh.Size = new System.Drawing.Size(38, 15);
            this.lblFechaHasta_380_jh.TabIndex = 2;
            this.lblFechaHasta_380_jh.Text = "Hasta";
            //
            // dtpFechaHasta
            //
            this.dtpFechaHasta_380_jh.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFechaHasta_380_jh.Location = new System.Drawing.Point(215, 20);
            this.dtpFechaHasta_380_jh.Name = "dtpFechaHasta";
            this.dtpFechaHasta_380_jh.ShowCheckBox = true;
            this.dtpFechaHasta_380_jh.Size = new System.Drawing.Size(105, 23);
            this.dtpFechaHasta_380_jh.TabIndex = 3;
            //
            // lblUsuarioFiltro
            //
            this.lblUsuarioFiltro_380_jh.AutoSize = true;
            this.lblUsuarioFiltro_380_jh.Location = new System.Drawing.Point(332, 25);
            this.lblUsuarioFiltro_380_jh.Name = "lblUsuarioFiltro";
            this.lblUsuarioFiltro_380_jh.Size = new System.Drawing.Size(47, 15);
            this.lblUsuarioFiltro_380_jh.TabIndex = 4;
            this.lblUsuarioFiltro_380_jh.Text = "Usuario";
            //
            // txtUsuarioFiltro
            //
            this.txtUsuarioFiltro_380_jh.Location = new System.Drawing.Point(385, 20);
            this.txtUsuarioFiltro_380_jh.Name = "txtUsuarioFiltro";
            this.txtUsuarioFiltro_380_jh.Size = new System.Drawing.Size(145, 23);
            this.txtUsuarioFiltro_380_jh.TabIndex = 5;
            //
            // btnBuscar
            //
            this.btnBuscar_380_jh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnBuscar_380_jh.FlatAppearance.BorderSize = 0;
            this.btnBuscar_380_jh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBuscar_380_jh.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnBuscar_380_jh.ForeColor = System.Drawing.Color.White;
            this.btnBuscar_380_jh.Location = new System.Drawing.Point(540, 18);
            this.btnBuscar_380_jh.Name = "btnBuscar";
            this.btnBuscar_380_jh.Size = new System.Drawing.Size(94, 27);
            this.btnBuscar_380_jh.TabIndex = 6;
            this.btnBuscar_380_jh.Text = "Buscar";
            this.btnBuscar_380_jh.UseVisualStyleBackColor = false;
            this.btnBuscar_380_jh.Click += new System.EventHandler(this.btnBuscar_Click_380_jh);
            //
            // btnLimpiarFiltros
            //
            this.btnLimpiarFiltros_380_jh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.btnLimpiarFiltros_380_jh.FlatAppearance.BorderSize = 0;
            this.btnLimpiarFiltros_380_jh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLimpiarFiltros_380_jh.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnLimpiarFiltros_380_jh.ForeColor = System.Drawing.Color.White;
            this.btnLimpiarFiltros_380_jh.Location = new System.Drawing.Point(640, 18);
            this.btnLimpiarFiltros_380_jh.Name = "btnLimpiarFiltros";
            this.btnLimpiarFiltros_380_jh.Size = new System.Drawing.Size(105, 27);
            this.btnLimpiarFiltros_380_jh.TabIndex = 7;
            this.btnLimpiarFiltros_380_jh.Text = "Limpiar";
            this.btnLimpiarFiltros_380_jh.UseVisualStyleBackColor = false;
            this.btnLimpiarFiltros_380_jh.Click += new System.EventHandler(this.btnLimpiarFiltros_Click_380_jh);
            //
            // lblModuloFiltro
            //
            this.lblModuloFiltro_380_jh.AutoSize = true;
            this.lblModuloFiltro_380_jh.Location = new System.Drawing.Point(12, 72);
            this.lblModuloFiltro_380_jh.Name = "lblModuloFiltro";
            this.lblModuloFiltro_380_jh.Size = new System.Drawing.Size(49, 15);
            this.lblModuloFiltro_380_jh.TabIndex = 8;
            this.lblModuloFiltro_380_jh.Text = "Modulo";
            //
            // cmbModuloFiltro
            //
            this.cmbModuloFiltro_380_jh.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbModuloFiltro_380_jh.FormattingEnabled = true;
            this.cmbModuloFiltro_380_jh.Location = new System.Drawing.Point(66, 68);
            this.cmbModuloFiltro_380_jh.Name = "cmbModuloFiltro";
            this.cmbModuloFiltro_380_jh.Size = new System.Drawing.Size(105, 23);
            this.cmbModuloFiltro_380_jh.TabIndex = 9;
            //
            // lblAccionFiltro
            //
            this.lblAccionFiltro_380_jh.AutoSize = true;
            this.lblAccionFiltro_380_jh.Location = new System.Drawing.Point(185, 72);
            this.lblAccionFiltro_380_jh.Name = "lblAccionFiltro";
            this.lblAccionFiltro_380_jh.Size = new System.Drawing.Size(43, 15);
            this.lblAccionFiltro_380_jh.TabIndex = 10;
            this.lblAccionFiltro_380_jh.Text = "Accion";
            //
            // cmbAccionFiltro
            //
            this.cmbAccionFiltro_380_jh.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAccionFiltro_380_jh.FormattingEnabled = true;
            this.cmbAccionFiltro_380_jh.Location = new System.Drawing.Point(233, 68);
            this.cmbAccionFiltro_380_jh.Name = "cmbAccionFiltro";
            this.cmbAccionFiltro_380_jh.Size = new System.Drawing.Size(115, 23);
            this.cmbAccionFiltro_380_jh.TabIndex = 11;
            //
            // lblNivelFiltro
            //
            this.lblNivelFiltro_380_jh.AutoSize = true;
            this.lblNivelFiltro_380_jh.Location = new System.Drawing.Point(360, 72);
            this.lblNivelFiltro_380_jh.Name = "lblNivelFiltro";
            this.lblNivelFiltro_380_jh.Size = new System.Drawing.Size(35, 15);
            this.lblNivelFiltro_380_jh.TabIndex = 12;
            this.lblNivelFiltro_380_jh.Text = "Nivel";
            //
            // cmbNivelFiltro
            //
            this.cmbNivelFiltro_380_jh.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNivelFiltro_380_jh.FormattingEnabled = true;
            this.cmbNivelFiltro_380_jh.Location = new System.Drawing.Point(400, 68);
            this.cmbNivelFiltro_380_jh.Name = "cmbNivelFiltro";
            this.cmbNivelFiltro_380_jh.Size = new System.Drawing.Size(105, 23);
            this.cmbNivelFiltro_380_jh.TabIndex = 13;
            //
            // lblDescripcionFiltro
            //
            this.lblDescripcionFiltro_380_jh.AutoSize = true;
            this.lblDescripcionFiltro_380_jh.Location = new System.Drawing.Point(518, 72);
            this.lblDescripcionFiltro_380_jh.Name = "lblDescripcionFiltro";
            this.lblDescripcionFiltro_380_jh.Size = new System.Drawing.Size(69, 15);
            this.lblDescripcionFiltro_380_jh.TabIndex = 14;
            this.lblDescripcionFiltro_380_jh.Text = "Descripcion";
            //
            // txtDescripcionFiltro
            //
            this.txtDescripcionFiltro_380_jh.Location = new System.Drawing.Point(590, 68);
            this.txtDescripcionFiltro_380_jh.Name = "txtDescripcionFiltro";
            this.txtDescripcionFiltro_380_jh.Size = new System.Drawing.Size(241, 23);
            this.txtDescripcionFiltro_380_jh.TabIndex = 15;
            // 
            // BitacoraView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.btnActualizar_380_jh);
            this.Controls.Add(this.grpFiltros_380_jh);
            this.Controls.Add(this.lblEstado_380_jh);
            this.Controls.Add(this.listViewBitacora_380_jh);
            this.Controls.Add(this.lblDescripcion_380_jh);
            this.Controls.Add(this.lblTitulo_380_jh);
            this.Name = "BitacoraView";
            this.Size = new System.Drawing.Size(900, 520);
            this.grpFiltros_380_jh.ResumeLayout(false);
            this.grpFiltros_380_jh.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitulo_380_jh;
        private System.Windows.Forms.Label lblDescripcion_380_jh;
        private System.Windows.Forms.ListView listViewBitacora_380_jh;
        private System.Windows.Forms.ColumnHeader columnId_380_jh;
        private System.Windows.Forms.ColumnHeader columnFecha_380_jh;
        private System.Windows.Forms.ColumnHeader columnIdUsuario_380_jh;
        private System.Windows.Forms.ColumnHeader columnUsuario_380_jh;
        private System.Windows.Forms.ColumnHeader columnModulo_380_jh;
        private System.Windows.Forms.ColumnHeader columnAccion_380_jh;
        private System.Windows.Forms.ColumnHeader columnNivel_380_jh;
        private System.Windows.Forms.ColumnHeader columnDescripcion_380_jh;
        private System.Windows.Forms.ColumnHeader columnEquipo_380_jh;
        private System.Windows.Forms.Label lblEstado_380_jh;
        private System.Windows.Forms.Button btnActualizar_380_jh;
        private System.Windows.Forms.GroupBox grpFiltros_380_jh;
        private System.Windows.Forms.Label lblFechaDesde_380_jh;
        private System.Windows.Forms.DateTimePicker dtpFechaDesde_380_jh;
        private System.Windows.Forms.Label lblFechaHasta_380_jh;
        private System.Windows.Forms.DateTimePicker dtpFechaHasta_380_jh;
        private System.Windows.Forms.Label lblUsuarioFiltro_380_jh;
        private System.Windows.Forms.TextBox txtUsuarioFiltro_380_jh;
        private System.Windows.Forms.Label lblModuloFiltro_380_jh;
        private System.Windows.Forms.ComboBox cmbModuloFiltro_380_jh;
        private System.Windows.Forms.Label lblAccionFiltro_380_jh;
        private System.Windows.Forms.ComboBox cmbAccionFiltro_380_jh;
        private System.Windows.Forms.Label lblNivelFiltro_380_jh;
        private System.Windows.Forms.ComboBox cmbNivelFiltro_380_jh;
        private System.Windows.Forms.Label lblDescripcionFiltro_380_jh;
        private System.Windows.Forms.TextBox txtDescripcionFiltro_380_jh;
        private System.Windows.Forms.Button btnBuscar_380_jh;
        private System.Windows.Forms.Button btnLimpiarFiltros_380_jh;
    }
}
