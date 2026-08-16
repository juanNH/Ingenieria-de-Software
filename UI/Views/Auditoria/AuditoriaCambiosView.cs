using System;
using System.Collections.Generic;
using System.Drawing;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Application;
using Domain;
using Services;

namespace UI
{
    public class AuditoriaCambiosView_380_jh : LocalizedUserControl_380_jh
    {
        private readonly AuditoriaApplicationService_380_jh _auditoriaService_380_jh;
        private readonly UsuarioApplicationService_380_jh _usuarioService_380_jh;
        private readonly JavaScriptSerializer _serializer_380_jh = new JavaScriptSerializer();

        private Label lblTitulo_380_jh;
        private Label lblDescripcion_380_jh;
        private Label lblUsuarioAuditado_380_jh;
        private ComboBox cmbUsuarios_380_jh;
        private Button btnActualizar_380_jh;
        private Button btnRestaurarCampo_380_jh;
        private ListView listViewCambios_380_jh;
        private ColumnHeader columnId_380_jh;
        private ColumnHeader columnFecha_380_jh;
        private ColumnHeader columnEntidad_380_jh;
        private ColumnHeader columnEntidadId_380_jh;
        private ColumnHeader columnActor_380_jh;
        private ColumnHeader columnCampo_380_jh;
        private ColumnHeader columnValorAnterior_380_jh;
        private ColumnHeader columnValorNuevo_380_jh;
        private Label lblEstado_380_jh;
        private Label lblEstadoAnterior_380_jh;
        private Label lblEstadoNuevo_380_jh;
        private TextBox txtEstadoAnterior_380_jh;
        private TextBox txtEstadoNuevo_380_jh;

        private List<Usuario_380_jh> _usuarios_380_jh = new List<Usuario_380_jh>();
        private List<AuditoriaRegistro_380_jh> _registros_380_jh = new List<AuditoriaRegistro_380_jh>();
        private int _cantidadCambios_380_jh;

        public AuditoriaCambiosView_380_jh()
            : this(new AuditoriaApplicationService_380_jh(), new UsuarioApplicationService_380_jh())
        {
        }

        public AuditoriaCambiosView_380_jh(AuditoriaApplicationService_380_jh auditoriaService, UsuarioApplicationService_380_jh usuarioService)
        {
            _auditoriaService_380_jh = auditoriaService;
            _usuarioService_380_jh = usuarioService;
            ConstruirInterfaz_380_jh();
            ConfigurarTraducciones_380_jh();
            CargarUsuarios_380_jh();
        }

        private void ConstruirInterfaz_380_jh()
        {
            BackColor = Color.White;
            Size = new Size(900, 520);

            lblTitulo_380_jh = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                Location = new Point(18, 18),
                Text = "Auditoria de cambios"
            };

            lblDescripcion_380_jh = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(108, 117, 125),
                Location = new Point(20, 58),
                Text = "Historial de cambios registrados sobre entidades auditadas."
            };

            lblUsuarioAuditado_380_jh = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(24, 101),
                Text = "Usuario auditado"
            };

            cmbUsuarios_380_jh = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(138, 97),
                Size = new Size(260, 23)
            };
            cmbUsuarios_380_jh.SelectedIndexChanged += cmbUsuarios_SelectedIndexChanged_380_jh;

            btnActualizar_380_jh = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(0, 123, 255),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(752, 92),
                Size = new Size(124, 32),
                Text = "Actualizar",
                UseVisualStyleBackColor = false
            };
            btnActualizar_380_jh.FlatAppearance.BorderSize = 0;
            btnActualizar_380_jh.Click += btnActualizar_Click_380_jh;

            btnRestaurarCampo_380_jh = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(25, 135, 84),
                Enabled = false,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(604, 92),
                Size = new Size(138, 32),
                Text = "Restaurar campo",
                UseVisualStyleBackColor = false
            };
            btnRestaurarCampo_380_jh.FlatAppearance.BorderSize = 0;
            btnRestaurarCampo_380_jh.Click += btnRestaurarCampo_Click_380_jh;

            listViewCambios_380_jh = new ListView
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Font = new Font("Segoe UI", 9F),
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                Location = new Point(24, 136),
                Size = new Size(852, 230),
                UseCompatibleStateImageBehavior = false,
                View = View.Details
            };
            listViewCambios_380_jh.SelectedIndexChanged += listViewCambios_SelectedIndexChanged_380_jh;

            columnId_380_jh = new ColumnHeader { Text = "Id", Width = 55 };
            columnFecha_380_jh = new ColumnHeader { Text = "Fecha", Width = 135 };
            columnEntidad_380_jh = new ColumnHeader { Text = "Entidad", Width = 85 };
            columnEntidadId_380_jh = new ColumnHeader { Text = "Id entidad", Width = 75 };
            columnActor_380_jh = new ColumnHeader { Text = "Usuario", Width = 120 };
            columnCampo_380_jh = new ColumnHeader { Text = "Campo", Width = 130 };
            columnValorAnterior_380_jh = new ColumnHeader { Text = "Valor anterior", Width = 170 };
            columnValorNuevo_380_jh = new ColumnHeader { Text = "Valor nuevo", Width = 170 };

            listViewCambios_380_jh.Columns.AddRange(new[]
            {
                columnId_380_jh,
                columnFecha_380_jh,
                columnEntidad_380_jh,
                columnEntidadId_380_jh,
                columnActor_380_jh,
                columnCampo_380_jh,
                columnValorAnterior_380_jh,
                columnValorNuevo_380_jh
            });

            lblEstadoAnterior_380_jh = new Label
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                Location = new Point(24, 378),
                Text = "Estado anterior"
            };

            lblEstadoNuevo_380_jh = new Label
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                Location = new Point(456, 378),
                Text = "Estado nuevo"
            };

            txtEstadoAnterior_380_jh = CrearTextBoxEstado_380_jh(new Point(24, 398));
            txtEstadoNuevo_380_jh = CrearTextBoxEstado_380_jh(new Point(456, 398));

            lblEstado_380_jh = new Label
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(108, 117, 125),
                Location = new Point(24, 494),
                Text = "No hay cambios registrados."
            };

            Controls.Add(lblTitulo_380_jh);
            Controls.Add(lblDescripcion_380_jh);
            Controls.Add(lblUsuarioAuditado_380_jh);
            Controls.Add(cmbUsuarios_380_jh);
            Controls.Add(btnRestaurarCampo_380_jh);
            Controls.Add(btnActualizar_380_jh);
            Controls.Add(listViewCambios_380_jh);
            Controls.Add(lblEstadoAnterior_380_jh);
            Controls.Add(lblEstadoNuevo_380_jh);
            Controls.Add(txtEstadoAnterior_380_jh);
            Controls.Add(txtEstadoNuevo_380_jh);
            Controls.Add(lblEstado_380_jh);
        }

        private static TextBox CrearTextBoxEstado_380_jh(Point location)
        {
            return new TextBox
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Font = new Font("Consolas", 8.5F),
                Location = location,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Size = new Size(420, 86)
            };
        }

        private void ConfigurarTraducciones_380_jh()
        {
            lblTitulo_380_jh.Tag = "CHANGE_AUDIT_TITLE";
            lblDescripcion_380_jh.Tag = "CHANGE_AUDIT_DESCRIPTION";
            lblUsuarioAuditado_380_jh.Tag = "CHANGE_AUDIT_USER";
            btnActualizar_380_jh.Tag = "BTN_REFRESH";
            columnId_380_jh.Tag = "GRID_ID";
            columnFecha_380_jh.Tag = "GRID_DATE";
            columnEntidad_380_jh.Tag = "GRID_ENTITY";
            columnEntidadId_380_jh.Tag = "GRID_ENTITY_ID";
            columnActor_380_jh.Tag = "GRID_USER";
            columnCampo_380_jh.Tag = "GRID_FIELD";
            columnValorAnterior_380_jh.Tag = "GRID_OLD_VALUE";
            columnValorNuevo_380_jh.Tag = "GRID_NEW_VALUE";
            lblEstadoAnterior_380_jh.Tag = "CHANGE_AUDIT_PREVIOUS_STATE";
            lblEstadoNuevo_380_jh.Tag = "CHANGE_AUDIT_NEW_STATE";
        }

        protected override void ApplyTranslations_380_jh()
        {
            base.ApplyTranslations_380_jh();
            ActualizarEstado_380_jh();
        }

        private void CargarUsuarios_380_jh()
        {
            _usuarios_380_jh = _usuarioService_380_jh.Listar_380_jh();
            _usuarios_380_jh.Insert(0, new Usuario_380_jh
            {
                Id_380_jh = 0,
                Username_380_jh = "Todos los usuarios",
                Nombre_380_jh = "Todos los usuarios"
            });

            cmbUsuarios_380_jh.DataSource = null;
            cmbUsuarios_380_jh.DisplayMember = "Username";
            cmbUsuarios_380_jh.ValueMember = "Id";
            cmbUsuarios_380_jh.DataSource = _usuarios_380_jh;

            if (_usuarios_380_jh.Count > 0)
            {
                cmbUsuarios_380_jh.SelectedIndex = 0;
                CargarAuditoria_380_jh(_usuarios_380_jh[0].Id_380_jh);
            }
            else
            {
                LimpiarCambios_380_jh();
            }
        }

        private void cmbUsuarios_SelectedIndexChanged_380_jh(object sender, EventArgs e)
        {
            Usuario_380_jh usuario = cmbUsuarios_380_jh.SelectedItem as Usuario_380_jh;
            if (usuario != null)
            {
                CargarAuditoria_380_jh(usuario.Id_380_jh);
            }
        }

        private void btnActualizar_Click_380_jh(object sender, EventArgs e)
        {
            Usuario_380_jh usuario = cmbUsuarios_380_jh.SelectedItem as Usuario_380_jh;
            if (usuario != null)
            {
                CargarAuditoria_380_jh(usuario.Id_380_jh);
            }
        }

        private void CargarAuditoria_380_jh(int usuarioId)
        {
            _registros_380_jh = usuarioId == 0
                ? _auditoriaService_380_jh.ListarTodos_380_jh()
                : _auditoriaService_380_jh.ListarHistorial_380_jh("Usuario", usuarioId);
            listViewCambios_380_jh.BeginUpdate();
            listViewCambios_380_jh.Items.Clear();

            foreach (AuditoriaRegistro_380_jh registro in _registros_380_jh)
            {
                List<AuditoriaCambio_380_jh> cambios = DeserializarCambios_380_jh(registro.CambiosJson_380_jh);

                foreach (AuditoriaCambio_380_jh cambio in cambios)
                {
                    ListViewItem item = new ListViewItem(registro.Id_380_jh.ToString());
                    item.SubItems.Add(registro.FechaEvento_380_jh.ToString("dd/MM/yyyy HH:mm:ss"));
                    item.SubItems.Add(registro.Entidad_380_jh ?? string.Empty);
                    item.SubItems.Add(registro.IdEntidad_380_jh.ToString());
                    item.SubItems.Add(registro.IdentificadorUsuarioActor_380_jh ?? string.Empty);
                    item.SubItems.Add(cambio.Campo_380_jh ?? string.Empty);
                    item.SubItems.Add(FormatearValor_380_jh(cambio.ValorAnterior_380_jh));
                    item.SubItems.Add(FormatearValor_380_jh(cambio.ValorNuevo_380_jh));
                    item.Tag = new CambioAuditoriaSeleccionado_380_jh
                    {
                        Registro_380_jh = registro,
                        Cambio_380_jh = cambio
                    };
                    listViewCambios_380_jh.Items.Add(item);
                }
            }

            listViewCambios_380_jh.EndUpdate();
            _cantidadCambios_380_jh = listViewCambios_380_jh.Items.Count;
            LimpiarDetalle_380_jh();
            ActualizarEstado_380_jh();
        }

        private void listViewCambios_SelectedIndexChanged_380_jh(object sender, EventArgs e)
        {
            if (listViewCambios_380_jh.SelectedItems.Count == 0)
            {
                LimpiarDetalle_380_jh();
                btnRestaurarCampo_380_jh.Enabled = false;
                return;
            }

            CambioAuditoriaSeleccionado_380_jh seleccion = listViewCambios_380_jh.SelectedItems[0].Tag as CambioAuditoriaSeleccionado_380_jh;
            if (seleccion == null || seleccion.Registro_380_jh == null)
            {
                LimpiarDetalle_380_jh();
                btnRestaurarCampo_380_jh.Enabled = false;
                return;
            }

            AuditoriaRegistro_380_jh registro = seleccion.Registro_380_jh;
            txtEstadoAnterior_380_jh.Text = registro.EstadoAnteriorJson_380_jh ?? string.Empty;
            txtEstadoNuevo_380_jh.Text = registro.EstadoNuevoJson_380_jh ?? string.Empty;
            btnRestaurarCampo_380_jh.Enabled = registro.Entidad_380_jh == "Usuario" &&
                                        seleccion.Cambio_380_jh != null &&
                                        EsCampoRestaurable_380_jh(seleccion.Cambio_380_jh.Campo_380_jh);
        }

        private void btnRestaurarCampo_Click_380_jh(object sender, EventArgs e)
        {
            if (listViewCambios_380_jh.SelectedItems.Count == 0)
            {
                return;
            }

            CambioAuditoriaSeleccionado_380_jh seleccion = listViewCambios_380_jh.SelectedItems[0].Tag as CambioAuditoriaSeleccionado_380_jh;
            if (seleccion == null || seleccion.Registro_380_jh == null || seleccion.Cambio_380_jh == null)
            {
                return;
            }

            string campo = seleccion.Cambio_380_jh.Campo_380_jh;
            DialogResult confirmacion = MessageBox.Show(
                "Se restaurara el campo seleccionado al valor anterior. Deseas continuar?",
                "Restaurar campo",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes)
            {
                return;
            }

            bool restaurado = _usuarioService_380_jh.RestaurarCampoDesdeAuditoria_380_jh(seleccion.Registro_380_jh, campo);
            MessageBox.Show(restaurado
                ? "Campo restaurado correctamente."
                : "No se pudo restaurar el campo seleccionado.");

            Usuario_380_jh usuario = cmbUsuarios_380_jh.SelectedItem as Usuario_380_jh;
            CargarUsuarios_380_jh();

            if (usuario != null)
            {
                SeleccionarUsuario_380_jh(usuario.Id_380_jh);
            }
        }

        private List<AuditoriaCambio_380_jh> DeserializarCambios_380_jh(string cambiosJson)
        {
            if (string.IsNullOrWhiteSpace(cambiosJson))
            {
                return new List<AuditoriaCambio_380_jh>();
            }

            try
            {
                return _serializer_380_jh.Deserialize<List<AuditoriaCambio_380_jh>>(cambiosJson) ?? new List<AuditoriaCambio_380_jh>();
            }
            catch
            {
                return new List<AuditoriaCambio_380_jh>();
            }
        }

        private static string FormatearValor_380_jh(object valor)
        {
            return valor == null ? string.Empty : valor.ToString();
        }

        private void LimpiarCambios_380_jh()
        {
            _registros_380_jh = new List<AuditoriaRegistro_380_jh>();
            listViewCambios_380_jh.Items.Clear();
            _cantidadCambios_380_jh = 0;
            LimpiarDetalle_380_jh();
            ActualizarEstado_380_jh();
        }

        private void LimpiarDetalle_380_jh()
        {
            txtEstadoAnterior_380_jh.Clear();
            txtEstadoNuevo_380_jh.Clear();
            if (btnRestaurarCampo_380_jh != null)
            {
                btnRestaurarCampo_380_jh.Enabled = false;
            }
        }

        private void ActualizarEstado_380_jh()
        {
            lblEstado_380_jh.Text = _cantidadCambios_380_jh == 0
                ? LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("CHANGE_AUDIT_EMPTY")
                : string.Format(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("CHANGE_AUDIT_COUNT"), _cantidadCambios_380_jh);
        }

        private void SeleccionarUsuario_380_jh(int usuarioId)
        {
            foreach (Usuario_380_jh usuario in cmbUsuarios_380_jh.Items)
            {
                if (usuario.Id_380_jh == usuarioId)
                {
                    cmbUsuarios_380_jh.SelectedItem = usuario;
                    return;
                }
            }
        }

        private static bool EsCampoRestaurable_380_jh(string campo)
        {
            switch (campo)
            {
                case "Username":
                case "Email":
                case "Nombre":
                case "Apellido":
                case "IdiomaPreferidoId":
                case "Estado":
                case "IntentosLoginFallidos":
                case "BloqueoDigitoVerificador":
                    return true;

                default:
                    return false;
            }
        }

        private class CambioAuditoriaSeleccionado_380_jh
        {
            public AuditoriaRegistro_380_jh Registro_380_jh { get; set; }
            public AuditoriaCambio_380_jh Cambio_380_jh { get; set; }
        }
    }
}
