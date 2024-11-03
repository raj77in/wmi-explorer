using System;
using System.Collections.Generic;
using System.Management;
using System.Windows.Forms;

namespace WMIGUIExplorer
{
    public class WMIGUIForm : Form
    {
        private TreeView namespaceTreeView;
        private TreeView classTreeView;
        private ListView propertiesListView;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WMIGUIForm());
        }

        public WMIGUIForm()
        {
            InitializeComponent();
            InitializeTreeView();
            LoadNamespaces();
        }

        private void InitializeComponent()
        {
            this.Text = "WMI GUI Explorer";
            this.Size = new System.Drawing.Size(1000, 600);
        }

        private void InitializeTreeView()
        {
            namespaceTreeView = new TreeView { Dock = DockStyle.Right, Width = 250 };
            classTreeView = new TreeView { Dock = DockStyle.Right, Width = 250 };
            propertiesListView = new ListView { Dock = DockStyle.Right, Width= 500, View = View.Details };
            propertiesListView.Columns.Add("Property", 150);
            propertiesListView.Columns.Add("Value", 350);

            namespaceTreeView.AfterSelect += NamespaceTreeView_AfterSelect;
            classTreeView.AfterSelect += ClassTreeView_AfterSelect;

            // Order: Namespace -> Class -> Properties
            this.Controls.Add(namespaceTreeView);
            this.Controls.Add(classTreeView);
            this.Controls.Add(propertiesListView);
        }

        private void LoadNamespaces()
        {
            namespaceTreeView.Nodes.Clear();
            try
            {
                List<string> namespaces = GetAllNamespaces();
                foreach (string ns in namespaces)
                {
                    namespaceTreeView.Nodes.Add(new TreeNode(ns));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading namespaces: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<string> GetAllNamespaces()
        {
            List<string> namespaces = new List<string>();
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root", "SELECT * FROM __Namespace");
                foreach (ManagementObject ns in searcher.Get())
                {
                    namespaces.Add(ns["Name"]?.ToString() ?? string.Empty);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving namespaces: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return namespaces;
        }

        private void NamespaceTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            classTreeView.Nodes.Clear();
            propertiesListView.Items.Clear();

            string selectedNamespace = e.Node?.Text;
            if (string.IsNullOrEmpty(selectedNamespace)) return;

            try
            {
                // Use the full path for the namespace (like "root\\cimv2")
                string namespacePath = $"root\\{selectedNamespace}";
                ManagementScope scope = new ManagementScope(namespacePath);
                scope.Connect();

                // Query all classes within the namespace
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, new WqlObjectQuery("SELECT * FROM meta_class"));
                foreach (ManagementClass wmiClass in searcher.Get())
                {
                    classTreeView.Nodes.Add(new TreeNode(wmiClass["__CLASS"]?.ToString() ?? "Unknown"));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading classes for namespace {selectedNamespace}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClassTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            propertiesListView.Items.Clear();
            string selectedClass = e.Node?.Text;
            string selectedNamespace = namespaceTreeView.SelectedNode?.Text;

            if (string.IsNullOrEmpty(selectedClass) || string.IsNullOrEmpty(selectedNamespace)) return;

            try
            {
                // Full namespace path for accessing the class
                string namespacePath = $"root\\{selectedNamespace}";
                ManagementScope scope = new ManagementScope(namespacePath);
                scope.Connect();

                ManagementClass wmiClass = new ManagementClass(scope, new ManagementPath(selectedClass), null);
                foreach (PropertyData property in wmiClass.Properties)
                {
                    ListViewItem item = new ListViewItem(property.Name ?? "Unknown");
                    item.SubItems.Add(property.Value?.ToString() ?? "N/A");
                    propertiesListView.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while fetching class properties: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
