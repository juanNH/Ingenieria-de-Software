using System.Windows.Forms;
using Services;

namespace UI
{
    public static class TranslationApplier_380_jh
    {
        public static void Apply_380_jh(Control root)
        {
            if (root == null)
            {
                return;
            }

            ApplyControl_380_jh(root);

            foreach (Control child in root.Controls)
            {
                Apply_380_jh(child);
            }
        }

        public static void ApplyMenu_380_jh(ToolStrip toolStrip)
        {
            if (toolStrip == null)
            {
                return;
            }

            foreach (ToolStripItem item in toolStrip.Items)
            {
                ApplyToolStripItem_380_jh(item);
            }
        }

        private static void ApplyControl_380_jh(Control control)
        {
            string key = control.Tag as string;
            if (!string.IsNullOrWhiteSpace(key))
            {
                control.Text = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh(key);
            }

            DataGridView dataGridView = control as DataGridView;
            if (dataGridView != null)
            {
                foreach (DataGridViewColumn column in dataGridView.Columns)
                {
                    string columnKey = column.Tag as string;
                    if (!string.IsNullOrWhiteSpace(columnKey))
                    {
                        column.HeaderText = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh(columnKey);
                    }
                }
            }

            ListView listView = control as ListView;
            if (listView != null)
            {
                foreach (ColumnHeader column in listView.Columns)
                {
                    string columnKey = column.Tag as string;
                    if (!string.IsNullOrWhiteSpace(columnKey))
                    {
                        column.Text = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh(columnKey);
                    }
                }
            }
        }

        private static void ApplyToolStripItem_380_jh(ToolStripItem item)
        {
            string key = item.Tag as string;
            if (!string.IsNullOrWhiteSpace(key))
            {
                item.Text = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh(key);
            }

            ToolStripDropDownItem dropDownItem = item as ToolStripDropDownItem;
            if (dropDownItem == null)
            {
                return;
            }

            foreach (ToolStripItem child in dropDownItem.DropDownItems)
            {
                ApplyToolStripItem_380_jh(child);
            }
        }
    }
}
