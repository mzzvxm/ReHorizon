using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Rockstar;
using DevComponents.AdvTree; // Importante para o AdvTree funcionar
using DevComponents.DotNetBar;

namespace Horizon.PackageEditors.Midnight_Club_LA
{
    public partial class MidnightClubLA : EditorControl
    {
        private MCLASaveGame _saveGame;

        public MidnightClubLA()
        {
            InitializeComponent();
            TitleID = FormID.MidnightClubLA;

            // Popula os seletores de Modificações com os dados hexadecimais
            foreach (string key in MCLAVehicle.SpeedMods.Keys)
            {
                cmbSpeedMod.Items.Add(key);
            }
            foreach (string key in MCLAVehicle.NeonColors.Keys)
            {
                cmbNeonColor.Items.Add(key);
            }

            // Popula o seletor de Rank da aba Career
            foreach (string rank in MCLACareer.ReputationRanks.Keys)
            {
                cmbReputationRank.Items.Add(rank);
            }
        }

        public override bool Entry()
        {
            // Tenta abrir o arquivo de save do Xbox 360 (formato STFS)
            if (!OpenStfsFile("mc4.sav"))
                return false;

            try
            {
                _saveGame = new MCLASaveGame(IO);
                DisplayData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Falha ao ler o save: " + ex.Message);
                return false;
            }

            return true;
        }

        public override void Save()
        {
            if (_saveGame != null && _saveGame.Career != null)
            {
                // Salva o dinheiro
                _saveGame.Career.Money = (int)this.numMoney.Value;

                // Salva o Rank Selecionado convertendo para RP
                if (cmbReputationRank.SelectedItem != null)
                {
                    string selectedRank = cmbReputationRank.SelectedItem.ToString();
                    if (MCLACareer.ReputationRanks.ContainsKey(selectedRank))
                    {
                        _saveGame.Career.Reputation = MCLACareer.ReputationRanks[selectedRank];
                    }
                }

                _saveGame.Save();
            }
        }

        private void DisplayData()
        {
            if (_saveGame != null && _saveGame.Career != null)
            {
                // Atualiza o dinheiro
                this.numMoney.Value = _saveGame.Career.Money;

                // Descobre qual o Rank atual baseado no RP lido do save
                int currentRP = _saveGame.Career.Reputation;
                string closestRank = "Rank 1: Backseat Driver";
                int highestRP = -1;

                foreach (var rank in MCLACareer.ReputationRanks)
                {
                    if (currentRP >= rank.Value && rank.Value > highestRP)
                    {
                        highestRP = rank.Value;
                        closestRank = rank.Key;
                    }
                }
                cmbReputationRank.SelectedItem = closestRank;

                // Limpa e bloqueia a atualização visual da Tree temporariamente (boa prática)
                advGarageTree.Nodes.Clear();
                advGarageTree.BeginUpdate();

                foreach (var veiculo in _saveGame.Career.Vehicles)
                {
                    // Cria o nó base (Slot Number)
                    Node carNode = new Node((veiculo.SlotIndex + 1).ToString());

                    // Adiciona as colunas (Células no AdvTree)
                    carNode.Cells.Add(new Cell(veiculo.ModelName));
                    carNode.Cells.Add(new Cell(veiculo.LicensePlate));

                    // Armazena o objeto MCLAVehicle inteiro dentro da Tag do Node
                    carNode.Tag = veiculo;

                    // Adiciona na árvore
                    advGarageTree.Nodes.Add(carNode);
                }

                // Libera a atualização visual
                advGarageTree.EndUpdate();
            }
        }

        private void btnApplyVehicleMods_Click(object sender, EventArgs e)
        {
            if (advGarageTree.SelectedNode != null && advGarageTree.SelectedNode.Tag is MCLAVehicle car)
            {
                // 1. Aplica Velocidade se houver algo selecionado
                if (cmbSpeedMod.SelectedIndex != -1)
                {
                    car.ApplySpeedMod(cmbSpeedMod.SelectedItem.ToString());
                }

                // 2. Aplica Cor de Néon se houver algo selecionado
                if (cmbNeonColor.SelectedIndex != -1)
                {
                    car.ApplyNeonMod(cmbNeonColor.SelectedItem.ToString());
                }

                // 3. Aplica Altura digitada
                car.SetRideHeight((byte)intRideHeight.Value);

                MessageBox.Show("Modifications applied to " + car.ModelName + "!",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Please select a vehicle from the list first.",
                                "No Vehicle Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnApplyStanceMod_Click(object sender, EventArgs e)
        {
            if (advGarageTree.SelectedNode != null && advGarageTree.SelectedNode.Tag is MCLAVehicle selectedCar)
            {
                selectedCar.SetRideHeight(0x0B);
                intRideHeight.Value = 0x0B;

                MessageBox.Show("Ride height slammed for " + selectedCar.ModelName + "!",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Lógica de Modificação via Hex Arrays (Comboxes)
        private void cmbSpeedMod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (advGarageTree.SelectedNode != null && advGarageTree.SelectedNode.Tag is MCLAVehicle car)
            {
                if (cmbSpeedMod.SelectedIndex != -1)
                {
                    car.ApplySpeedMod(cmbSpeedMod.SelectedItem.ToString());
                }
            }
        }

        private void cmbNeonColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (advGarageTree.SelectedNode != null && advGarageTree.SelectedNode.Tag is MCLAVehicle car)
            {
                if (cmbNeonColor.SelectedIndex != -1)
                {
                    car.ApplyNeonMod(cmbNeonColor.SelectedItem.ToString());
                }
            }
        }

        // Clique para Desbloquear Tudo na aba Career
        private void btnUnlockAll_Click(object sender, EventArgs e)
        {
            if (_saveGame != null && _saveGame.Career != null)
            {
                _saveGame.Career.UnlockAll();

                // Força a UI a atualizar com os novos valores injetados pelo UnlockAll
                this.cmbReputationRank.SelectedItem = "Rank 11: Idol";

                MessageBox.Show("All vehicles and progression unlocked!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Lê os dados quando o jogador clica num carro da lista
        private void advGarageTree_AfterNodeSelect(object sender, AdvTreeNodeEventArgs e)
        {
            if (e.Node != null && e.Node.Tag is MCLAVehicle car)
            {
                groupPanelMods.Enabled = true;
                groupPanelMods.Text = "Customizing: " + car.ModelName;

                // Desvincula o evento temporariamente para não sobrescrever ao carregar os valores atuais
                intRideHeight.ValueChanged -= intRideHeight_ValueChanged;
                cmbSpeedMod.SelectedIndexChanged -= cmbSpeedMod_SelectedIndexChanged;
                cmbNeonColor.SelectedIndexChanged -= cmbNeonColor_SelectedIndexChanged;

                intRideHeight.Value = car.GetRideHeight();

                // Limpa os ComboBoxes para que a re-seleção em outro carro ative o evento corretamente
                cmbSpeedMod.SelectedIndex = -1;
                cmbNeonColor.SelectedIndex = -1;

                intRideHeight.ValueChanged += intRideHeight_ValueChanged;
                cmbSpeedMod.SelectedIndexChanged += cmbSpeedMod_SelectedIndexChanged;
                cmbNeonColor.SelectedIndexChanged += cmbNeonColor_SelectedIndexChanged;
            }
            else
            {
                groupPanelMods.Enabled = false;
                groupPanelMods.Text = "Vehicle Mods";
            }
        }

        // Escreve no byte do save em tempo real quando o jogador muda o número na caixinha
        private void intRideHeight_ValueChanged(object sender, EventArgs e)
        {
            if (advGarageTree.SelectedNode != null && advGarageTree.SelectedNode.Tag is MCLAVehicle car)
            {
                car.SetRideHeight((byte)intRideHeight.Value);
            }
        }
    }
}