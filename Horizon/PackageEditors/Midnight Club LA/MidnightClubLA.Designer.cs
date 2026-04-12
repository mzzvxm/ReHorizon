namespace Horizon.PackageEditors.Midnight_Club_LA
{
    partial class MidnightClubLA
    {
        /// <summary> 
        /// Variáveis de designer necessárias.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // Componentes da aba Career (Principal)
        private System.Windows.Forms.NumericUpDown numMoney;
        private System.Windows.Forms.Label lblMoney;
        private System.Windows.Forms.Label lblReputationRank;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cmbReputationRank;

        // Painel e Botão para a aba Career
        private DevComponents.DotNetBar.Controls.GroupPanel groupPanelCareer;
        private DevComponents.DotNetBar.ButtonX btnUnlockAll;

        // Componentes da aba Garage
        private DevComponents.DotNetBar.RibbonTabItem tabGarage;
        private DevComponents.DotNetBar.RibbonPanel panelGarage;

        // Componentes Nativos do Horizon (AdvTree e GroupPanel)
        private DevComponents.AdvTree.AdvTree advGarageTree;
        private DevComponents.AdvTree.ColumnHeader colSlot;
        private DevComponents.AdvTree.ColumnHeader colModel;
        private DevComponents.AdvTree.ColumnHeader colPlate;
        private DevComponents.AdvTree.NodeConnector nodeConnector1;
        private DevComponents.DotNetBar.ElementStyle elementStyle1;

        private DevComponents.DotNetBar.Controls.GroupPanel groupPanelMods;
        private DevComponents.DotNetBar.ButtonX btnApplyStanceMod;

        // COMPONENTES PARA CUSTOMIZAÇÃO
        private System.Windows.Forms.Label lblSpeedMod;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cmbSpeedMod;
        private System.Windows.Forms.Label lblNeonColor;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cmbNeonColor;
        private DevComponents.Editors.IntegerInput intRideHeight;
        private System.Windows.Forms.Label lblRideHeight;

        // BOTÃO DE APLICAÇÃO GERAL
        private DevComponents.DotNetBar.ButtonX btnApplyVehicleMods;

        /// <summary> 
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Designer de Componentes

        /// <summary> 
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.numMoney = new System.Windows.Forms.NumericUpDown();
            this.lblMoney = new System.Windows.Forms.Label();

            // NOVO: RANK
            this.lblReputationRank = new System.Windows.Forms.Label();
            this.cmbReputationRank = new DevComponents.DotNetBar.Controls.ComboBoxEx();

            this.groupPanelCareer = new DevComponents.DotNetBar.Controls.GroupPanel();
            this.btnUnlockAll = new DevComponents.DotNetBar.ButtonX();

            this.tabGarage = new DevComponents.DotNetBar.RibbonTabItem();
            this.panelGarage = new DevComponents.DotNetBar.RibbonPanel();

            this.advGarageTree = new DevComponents.AdvTree.AdvTree();
            this.colSlot = new DevComponents.AdvTree.ColumnHeader();
            this.colModel = new DevComponents.AdvTree.ColumnHeader();
            this.colPlate = new DevComponents.AdvTree.ColumnHeader();
            this.nodeConnector1 = new DevComponents.AdvTree.NodeConnector();
            this.elementStyle1 = new DevComponents.DotNetBar.ElementStyle();

            this.groupPanelMods = new DevComponents.DotNetBar.Controls.GroupPanel();
            this.btnApplyStanceMod = new DevComponents.DotNetBar.ButtonX();

            this.lblSpeedMod = new System.Windows.Forms.Label();
            this.cmbSpeedMod = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.lblNeonColor = new System.Windows.Forms.Label();
            this.cmbNeonColor = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.intRideHeight = new DevComponents.Editors.IntegerInput();
            this.lblRideHeight = new System.Windows.Forms.Label();

            this.btnApplyVehicleMods = new DevComponents.DotNetBar.ButtonX();

            this.rbPackageEditor.SuspendLayout();
            this.panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMoney)).BeginInit();
            this.groupPanelCareer.SuspendLayout();
            this.panelGarage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.advGarageTree)).BeginInit();
            this.groupPanelMods.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.intRideHeight)).BeginInit();
            this.SuspendLayout();

            // 
            // rbPackageEditor
            // 
            this.rbPackageEditor.Controls.Add(this.panelGarage);
            this.rbPackageEditor.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.tabGarage});
            this.rbPackageEditor.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.rbPackageEditor.Size = new System.Drawing.Size(560, 361);
            this.rbPackageEditor.Controls.SetChildIndex(this.panelMain, 0);
            this.rbPackageEditor.Controls.SetChildIndex(this.panelGarage, 0);

            // 
            // tabMain
            // 
            this.tabMain.Text = "Career";

            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.groupPanelCareer);
            this.panelMain.Controls.Add(this.lblReputationRank);
            this.panelMain.Controls.Add(this.cmbReputationRank);
            this.panelMain.Controls.Add(this.lblMoney);
            this.panelMain.Controls.Add(this.numMoney);
            this.panelMain.Location = new System.Drawing.Point(0, 53);
            this.panelMain.Size = new System.Drawing.Size(560, 306);
            this.panelMain.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;

            // 
            // numMoney
            // 
            this.numMoney.Location = new System.Drawing.Point(100, 25);
            this.numMoney.Maximum = new decimal(new int[] {
            999999999,
            0,
            0,
            0});
            this.numMoney.Name = "numMoney";
            this.numMoney.Size = new System.Drawing.Size(120, 20);
            this.numMoney.TabIndex = 0;

            // 
            // lblMoney
            // 
            this.lblMoney.AutoSize = true;
            this.lblMoney.BackColor = System.Drawing.Color.Transparent;
            this.lblMoney.Location = new System.Drawing.Point(50, 27);
            this.lblMoney.Name = "lblMoney";
            this.lblMoney.Size = new System.Drawing.Size(42, 13);
            this.lblMoney.TabIndex = 1;
            this.lblMoney.Text = "Money:";

            // 
            // lblReputationRank
            // 
            this.lblReputationRank.AutoSize = true;
            this.lblReputationRank.BackColor = System.Drawing.Color.Transparent;
            this.lblReputationRank.Location = new System.Drawing.Point(50, 62);
            this.lblReputationRank.Name = "lblReputationRank";
            this.lblReputationRank.Size = new System.Drawing.Size(36, 13);
            this.lblReputationRank.TabIndex = 2;
            this.lblReputationRank.Text = "Rank:";

            // 
            // cmbReputationRank
            // 
            this.cmbReputationRank.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbReputationRank.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbReputationRank.FormattingEnabled = true;
            this.cmbReputationRank.ItemHeight = 14;
            this.cmbReputationRank.Location = new System.Drawing.Point(100, 60);
            this.cmbReputationRank.Name = "cmbReputationRank";
            this.cmbReputationRank.Size = new System.Drawing.Size(120, 20);
            this.cmbReputationRank.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmbReputationRank.TabIndex = 3;

            // 
            // groupPanelCareer
            // 
            this.groupPanelCareer.CanvasColor = System.Drawing.SystemColors.Control;
            this.groupPanelCareer.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.groupPanelCareer.Controls.Add(this.btnUnlockAll);
            this.groupPanelCareer.Location = new System.Drawing.Point(250, 15);
            this.groupPanelCareer.Name = "groupPanelCareer";
            this.groupPanelCareer.Size = new System.Drawing.Size(160, 80);
            this.groupPanelCareer.Style.BackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.groupPanelCareer.Style.BackColorGradientAngle = 90;
            this.groupPanelCareer.Style.BackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.groupPanelCareer.Style.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanelCareer.Style.BorderBottomWidth = 1;
            this.groupPanelCareer.Style.BorderColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.groupPanelCareer.Style.BorderLeft = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanelCareer.Style.BorderLeftWidth = 1;
            this.groupPanelCareer.Style.BorderRight = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanelCareer.Style.BorderRightWidth = 1;
            this.groupPanelCareer.Style.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanelCareer.Style.BorderTopWidth = 1;
            this.groupPanelCareer.Style.CornerDiameter = 4;
            this.groupPanelCareer.Style.CornerType = DevComponents.DotNetBar.eCornerType.Rounded;
            this.groupPanelCareer.Style.TextAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Center;
            this.groupPanelCareer.Style.TextColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.groupPanelCareer.Style.TextLineAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Near;
            this.groupPanelCareer.TabIndex = 12;
            this.groupPanelCareer.Text = "Progression";

            // 
            // btnUnlockAll
            // 
            this.btnUnlockAll.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnUnlockAll.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnUnlockAll.Location = new System.Drawing.Point(15, 15);
            this.btnUnlockAll.Name = "btnUnlockAll";
            this.btnUnlockAll.Size = new System.Drawing.Size(125, 30);
            this.btnUnlockAll.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnUnlockAll.TabIndex = 2;
            this.btnUnlockAll.Text = "Unlock All Cars";
            this.btnUnlockAll.Click += new System.EventHandler(this.btnUnlockAll_Click);

            // 
            // tabGarage
            // 
            this.tabGarage.Name = "tabGarage";
            this.tabGarage.Panel = this.panelGarage;
            this.tabGarage.Text = "Garage";

            // 
            // panelGarage
            // 
            this.panelGarage.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.panelGarage.Controls.Add(this.groupPanelMods);
            this.panelGarage.Controls.Add(this.advGarageTree);
            this.panelGarage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGarage.Location = new System.Drawing.Point(0, 53);
            this.panelGarage.Name = "panelGarage";
            this.panelGarage.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.panelGarage.Size = new System.Drawing.Size(560, 306);
            this.panelGarage.TabIndex = 2;
            this.panelGarage.Visible = false;

            // 
            // advGarageTree
            // 
            this.advGarageTree.AccessibleRole = System.Windows.Forms.AccessibleRole.Outline;
            this.advGarageTree.AllowDrop = true;
            this.advGarageTree.BackColor = System.Drawing.SystemColors.Window;
            this.advGarageTree.BackgroundStyle.Class = "TreeBorderKey";
            this.advGarageTree.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.advGarageTree.Columns.Add(this.colSlot);
            this.advGarageTree.Columns.Add(this.colModel);
            this.advGarageTree.Columns.Add(this.colPlate);
            this.advGarageTree.GridRowLines = true;
            this.advGarageTree.Location = new System.Drawing.Point(6, 6);
            this.advGarageTree.Name = "advGarageTree";
            this.advGarageTree.NodesConnector = this.nodeConnector1;
            this.advGarageTree.NodeStyle = this.elementStyle1;
            this.advGarageTree.PathSeparator = ";";
            this.advGarageTree.Size = new System.Drawing.Size(380, 290);
            this.advGarageTree.Styles.Add(this.elementStyle1);
            this.advGarageTree.TabIndex = 11;
            this.advGarageTree.Text = "advGarageTree";
            this.advGarageTree.AfterNodeSelect += new DevComponents.AdvTree.AdvTreeNodeEventHandler(this.advGarageTree_AfterNodeSelect);

            // 
            // colSlot
            // 
            this.colSlot.Name = "colSlot";
            this.colSlot.Text = "Slot";
            this.colSlot.Width.Absolute = 40;

            // 
            // colModel
            // 
            this.colModel.Name = "colModel";
            this.colModel.Text = "Vehicle Model";
            this.colModel.Width.Absolute = 200;

            // 
            // colPlate
            // 
            this.colPlate.Name = "colPlate";
            this.colPlate.Text = "License Plate";
            this.colPlate.Width.Absolute = 120;

            // 
            // nodeConnector1
            // 
            this.nodeConnector1.LineColor = System.Drawing.SystemColors.ControlText;

            // 
            // elementStyle1
            // 
            this.elementStyle1.Class = "";
            this.elementStyle1.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.elementStyle1.Name = "elementStyle1";
            this.elementStyle1.TextColor = System.Drawing.SystemColors.ControlText;

            // 
            // groupPanelMods
            // 
            this.groupPanelMods.CanvasColor = System.Drawing.SystemColors.Control;
            this.groupPanelMods.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.groupPanelMods.Controls.Add(this.lblSpeedMod);
            this.groupPanelMods.Controls.Add(this.cmbSpeedMod);
            this.groupPanelMods.Controls.Add(this.lblNeonColor);
            this.groupPanelMods.Controls.Add(this.cmbNeonColor);
            this.groupPanelMods.Controls.Add(this.lblRideHeight);
            this.groupPanelMods.Controls.Add(this.intRideHeight);
            this.groupPanelMods.Controls.Add(this.btnApplyVehicleMods);
            this.groupPanelMods.Controls.Add(this.btnApplyStanceMod);
            this.groupPanelMods.Location = new System.Drawing.Point(392, 6);
            this.groupPanelMods.Name = "groupPanelMods";
            this.groupPanelMods.Size = new System.Drawing.Size(160, 290);
            this.groupPanelMods.Style.BackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.groupPanelMods.Style.BackColorGradientAngle = 90;
            this.groupPanelMods.Style.BackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.groupPanelMods.Style.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanelMods.Style.BorderBottomWidth = 1;
            this.groupPanelMods.Style.BorderColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.groupPanelMods.Style.BorderLeft = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanelMods.Style.BorderLeftWidth = 1;
            this.groupPanelMods.Style.BorderRight = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanelMods.Style.BorderRightWidth = 1;
            this.groupPanelMods.Style.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanelMods.Style.BorderTopWidth = 1;
            this.groupPanelMods.Style.CornerDiameter = 4;
            this.groupPanelMods.Style.CornerType = DevComponents.DotNetBar.eCornerType.Rounded;
            this.groupPanelMods.Style.TextAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Center;
            this.groupPanelMods.Style.TextColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.groupPanelMods.Style.TextLineAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Near;
            this.groupPanelMods.TabIndex = 12;
            this.groupPanelMods.Text = "Vehicle Mods";
            this.groupPanelMods.Enabled = false;

            // 
            // lblSpeedMod
            // 
            this.lblSpeedMod.AutoSize = true;
            this.lblSpeedMod.BackColor = System.Drawing.Color.Transparent;
            this.lblSpeedMod.Location = new System.Drawing.Point(12, 10);
            this.lblSpeedMod.Name = "lblSpeedMod";
            this.lblSpeedMod.Size = new System.Drawing.Size(65, 13);
            this.lblSpeedMod.TabIndex = 0;
            this.lblSpeedMod.Text = "Speed Mod:";

            // 
            // cmbSpeedMod
            // 
            this.cmbSpeedMod.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbSpeedMod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSpeedMod.Enabled = false; // DESABILITADO POR SEGURANÇA
            this.cmbSpeedMod.FormattingEnabled = true;
            this.cmbSpeedMod.ItemHeight = 14;
            this.cmbSpeedMod.Location = new System.Drawing.Point(15, 30);
            this.cmbSpeedMod.Name = "cmbSpeedMod";
            this.cmbSpeedMod.Size = new System.Drawing.Size(125, 20);
            this.cmbSpeedMod.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmbSpeedMod.TabIndex = 1;

            // 
            // lblNeonColor
            // 
            this.lblNeonColor.AutoSize = true;
            this.lblNeonColor.BackColor = System.Drawing.Color.Transparent;
            this.lblNeonColor.Location = new System.Drawing.Point(12, 60);
            this.lblNeonColor.Name = "lblNeonColor";
            this.lblNeonColor.Size = new System.Drawing.Size(63, 13);
            this.lblNeonColor.TabIndex = 2;
            this.lblNeonColor.Text = "Neon Color:";

            // 
            // cmbNeonColor
            // 
            this.cmbNeonColor.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbNeonColor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNeonColor.Enabled = false; // DESABILITADO POR SEGURANÇA
            this.cmbNeonColor.FormattingEnabled = true;
            this.cmbNeonColor.ItemHeight = 14;
            this.cmbNeonColor.Location = new System.Drawing.Point(15, 80);
            this.cmbNeonColor.Name = "cmbNeonColor";
            this.cmbNeonColor.Size = new System.Drawing.Size(125, 20);
            this.cmbNeonColor.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmbNeonColor.TabIndex = 3;

            // 
            // lblRideHeight
            // 
            this.lblRideHeight.AutoSize = true;
            this.lblRideHeight.BackColor = System.Drawing.Color.Transparent;
            this.lblRideHeight.Location = new System.Drawing.Point(12, 110);
            this.lblRideHeight.Name = "lblRideHeight";
            this.lblRideHeight.Size = new System.Drawing.Size(66, 13);
            this.lblRideHeight.TabIndex = 4;
            this.lblRideHeight.Text = "Ride Height:";

            // 
            // intRideHeight
            // 
            this.intRideHeight.Location = new System.Drawing.Point(15, 130);
            this.intRideHeight.MaxValue = 255;
            this.intRideHeight.MinValue = 0;
            this.intRideHeight.Name = "intRideHeight";
            this.intRideHeight.Size = new System.Drawing.Size(125, 20);
            this.intRideHeight.TabIndex = 5;

            // 
            // btnApplyVehicleMods
            // 
            this.btnApplyVehicleMods.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnApplyVehicleMods.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnApplyVehicleMods.Location = new System.Drawing.Point(15, 160);
            this.btnApplyVehicleMods.Name = "btnApplyVehicleMods";
            this.btnApplyVehicleMods.Size = new System.Drawing.Size(125, 25);
            this.btnApplyVehicleMods.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnApplyVehicleMods.TabIndex = 6;
            this.btnApplyVehicleMods.Text = "Apply";
            this.btnApplyVehicleMods.Click += new System.EventHandler(this.btnApplyVehicleMods_Click);

            // 
            // btnApplyStanceMod
            // 
            this.btnApplyStanceMod.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnApplyStanceMod.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnApplyStanceMod.Location = new System.Drawing.Point(15, 195);
            this.btnApplyStanceMod.Name = "btnApplyStanceMod";
            this.btnApplyStanceMod.Size = new System.Drawing.Size(125, 35);
            this.btnApplyStanceMod.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnApplyStanceMod.TabIndex = 7;
            this.btnApplyStanceMod.Text = "Slammed Ride Height";
            this.btnApplyStanceMod.Click += new System.EventHandler(this.btnApplyStanceMod_Click);

            // 
            // MidnightClubLA
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(570, 364);
            this.Name = "MidnightClubLA";

            this.rbPackageEditor.ResumeLayout(false);
            this.rbPackageEditor.PerformLayout();
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMoney)).EndInit();
            this.groupPanelCareer.ResumeLayout(false);
            this.panelGarage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.advGarageTree)).EndInit();
            this.groupPanelMods.ResumeLayout(false);
            this.groupPanelMods.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.intRideHeight)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
    }
}