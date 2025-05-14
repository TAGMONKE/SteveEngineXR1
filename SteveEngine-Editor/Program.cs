using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SteveEngine; // Reference your engine project

namespace SteveEngineEditor
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new EditorForm());
        }
    }

    public class EditorForm : Form
    {
        private Engine engine;
        private ListBox lstGameObjects;
        private Button btnAddGameObject, btnRemoveGameObject, btnAddComponent, btnRemoveComponent, btnApply, btnRun;
        private ListBox lstComponents;
        private TextBox txtName, txtPosX, txtPosY, txtPosZ;
        private Label lblName, lblPos, lblComponents;
        private readonly string[] availableComponents = new[]
        {
            "MeshRenderer", "Collider", "Rigidbody", "AudioListener", "CharacterController"
        };

        public EditorForm()
        {
            this.Text = "SteveEngine Editor";
            this.Width = 700;
            this.Height = 400;

            // Engine startup
            engine = new Engine(new OpenTK.Mathematics.Vector3(0, 0, 0), 800, 600, "Editor", OpenTK.Windowing.Common.WindowState.Normal, false, false, false, false);

            lstGameObjects = new ListBox { Left = 10, Top = 30, Width = 200, Height = 250 };
            btnAddGameObject = new Button { Text = "Add", Left = 10, Top = 290, Width = 95 };
            btnRemoveGameObject = new Button { Text = "Remove", Left = 115, Top = 290, Width = 95 };

            lblName = new Label { Text = "Name:", Left = 220, Top = 30, Width = 50 };
            txtName = new TextBox { Left = 270, Top = 30, Width = 150 };

            lblPos = new Label { Text = "Position (X,Y,Z):", Left = 220, Top = 60, Width = 100 };
            txtPosX = new TextBox { Left = 330, Top = 60, Width = 40 };
            txtPosY = new TextBox { Left = 380, Top = 60, Width = 40 };
            txtPosZ = new TextBox { Left = 430, Top = 60, Width = 40 };

            lblComponents = new Label { Text = "Components", Left = 220, Top = 100, Width = 100 };
            lstComponents = new ListBox { Left = 220, Top = 120, Width = 200, Height = 160 };
            btnAddComponent = new Button { Text = "Add Component", Left = 430, Top = 120, Width = 120 };
            btnRemoveComponent = new Button { Text = "Remove Component", Left = 430, Top = 160, Width = 120 };

            btnApply = new Button { Text = "Apply Changes", Left = 220, Top = 290, Width = 150 };
            btnRun = new Button { Text = "Run in Engine", Left = 380, Top = 290, Width = 150 };

            this.Controls.AddRange(new Control[] {
                lstGameObjects, btnAddGameObject, btnRemoveGameObject,
                lblName, txtName, lblPos, txtPosX, txtPosY, txtPosZ,
                lblComponents, lstComponents, btnAddComponent, btnRemoveComponent,
                btnApply, btnRun
            });

            btnAddGameObject.Click += (s, e) =>
            {
                var go = engine.CreateGameObject("GameObject" + (lstGameObjects.Items.Count + 1));
                lstGameObjects.Items.Add(go);
                lstGameObjects.SelectedItem = go;
            };

            btnRemoveGameObject.Click += (s, e) =>
            {
                if (lstGameObjects.SelectedItem is GameObject go)
                {
                    lstGameObjects.Items.Remove(go);
                    engine.GameObjects.Remove(go);
                    lstComponents.Items.Clear();
                }
            };

            lstGameObjects.SelectedIndexChanged += (s, e) =>
            {
                lstComponents.Items.Clear();
                if (lstGameObjects.SelectedItem is GameObject go)
                {
                    txtName.Text = go.Name;
                    txtPosX.Text = go.Transform.Position.X.ToString();
                    txtPosY.Text = go.Transform.Position.Y.ToString();
                    txtPosZ.Text = go.Transform.Position.Z.ToString();
                    foreach (var c in go.Components)
                        lstComponents.Items.Add(c.GetType().Name);
                }
            };

            btnAddComponent.Click += (s, e) =>
            {
                if (lstGameObjects.SelectedItem is GameObject go)
                {
                    var sel = ShowComponentPicker();
                    if (!string.IsNullOrEmpty(sel)) // allow multi same component, for whatever purpose people have?
                    {
                        go.AddComponent(sel);
                        lstComponents.Items.Add(sel);
                    }
                }
            };

            btnRemoveComponent.Click += (s, e) =>
            {
                if (lstGameObjects.SelectedItem is GameObject go && lstComponents.SelectedItem is string comp)
                {
                    // Remove the first component of this type
                    var toRemove = go.Components.Find(c => c.GetType().Name == comp);
                    if (toRemove != null)
                    {
                        go.Components.Remove(toRemove);
                        lstComponents.Items.Remove(comp);
                    }
                }
            };

            btnApply.Click += (s, e) =>
            {
                if (lstGameObjects.SelectedItem is GameObject go)
                {
                    go.Name = txtName.Text;
                    float.TryParse(txtPosX.Text, out float x);
                    float.TryParse(txtPosY.Text, out float y);
                    float.TryParse(txtPosZ.Text, out float z);
                    go.Transform.Position = new OpenTK.Mathematics.Vector3(x, y, z);
                    int idx = lstGameObjects.SelectedIndex;
                    lstGameObjects.Items[idx] = go; // Refresh display
                }
            };

            btnRun.Click += (s, e) =>
            {
                var lua = GenerateLua();
                System.IO.File.WriteAllText("game.lua", lua);
                engine.LoadScript("game.lua");
                MessageBox.Show("Lua script generated and loaded in engine!", "Run", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }

        private string ShowComponentPicker()
        {
            using (var dlg = new Form { Width = 250, Height = 200, Text = "Add Component" })
            {
                var lb = new ListBox { Dock = DockStyle.Fill };
                lb.Items.AddRange(availableComponents);
                dlg.Controls.Add(lb);
                lb.DoubleClick += (s, e) => dlg.DialogResult = DialogResult.OK;
                if (dlg.ShowDialog() == DialogResult.OK && lb.SelectedItem is string sel)
                    return sel;
            }
            return null;
        }

        private string GenerateLua()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("function onStart()");
            sb.AppendLine("    print('Generated by SteveEngine-Editor')");
            foreach (GameObject go in lstGameObjects.Items)
            {
                sb.AppendLine($"    local {go.Name} = engine:CreateGameObject('{go.Name}')");
                sb.AppendLine($"    {go.Name}:SetPosition({go.Transform.Position.X}, {go.Transform.Position.Y}, {go.Transform.Position.Z})");
                foreach (var c in go.Components)
                {
                    sb.AppendLine($"    {go.Name}:AddComponent('{c.GetType().Name}')");
                }
            }
            sb.AppendLine("end");
            sb.AppendLine();
            sb.AppendLine("function onUpdate(deltaTime)");
            sb.AppendLine("    -- Add your update logic here");
            sb.AppendLine("end");
            return sb.ToString();
        }
    }
}
