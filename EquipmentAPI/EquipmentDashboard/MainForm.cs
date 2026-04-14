using EquipmentDashboard.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EquipmentDashboard
{
    public partial class MainForm : Form
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7226/") // Replace with your API base URL
        };
        private List<EquipmentResponseDto> _equipments = new List<EquipmentResponseDto>();
        public MainForm()
        {
            InitializeComponent();
            _ = LoadEquipmentAsync();
        }

        private async Task LoadEquipmentAsync()
        {
            try {
                SetStatus("Loading...", Color.Gray);

                var response = await _httpClient.GetAsync("/equipments");

                if (!response.IsSuccessStatusCode)
                {
                    SetStatus($"Error: {response.StatusCode}", Color.Red);
                    return;
                }

                string json = await response.Content.ReadAsStringAsync();

                _equipments = JsonSerializer.Deserialize<List<EquipmentResponseDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<EquipmentResponseDto>();

                lstEquipment.Items.Clear();
                foreach (var equipment in _equipments)
                {
                    lstEquipment.Items.Add(equipment.Name);

                    SetStatus(_equipments.Count > 0 ? $"Loaded {_equipments.Count} equipment(s)" : "No equipment found", Color.Gray);
                }
            }
            catch (HttpRequestException)
            {
                SetStatus("Cannot connect to EquipmentAPI. Is it running?", Color.Red);
            }
        }

        private void lstEquipment_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = lstEquipment.SelectedIndex;

            if (lstEquipment.SelectedIndex < 0 || lstEquipment.SelectedIndex >= _equipments.Count)
            {
                lblNoSelection.Visible= true;
                pnlDetailFields.Visible = false;
                return;
            }

            var equipment = _equipments[index];

            lblIdValue.Text = equipment.Id.ToString();
            lblNameValue.Text = equipment.Name;

            lblNoSelection.Visible = false;
            pnlDetailFields.Visible = true;
        }

        private async void btnCreate_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string id = txtCategory.Text.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(id))
            {
                MessageBox.Show("Please enter both Name and Category.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dto = new CreateEquipmentDto
            {
                Name = name,
                Description = id
            };
            try
            {
                btnCreate.Enabled = false;
                SetStatus("Creating...", Color.Gray);

                var response = await _httpClient.PostAsJsonAsync("/equipments", dto);

                if (response.IsSuccessStatusCode)
                {
                    txtName.Clear();
                    txtCategory.Clear();

                    SetStatus($"Equipment '{name}' created successfully!", Color.Green);

                    await LoadEquipmentAsync();
                }
                else
                {
                    SetStatus($"Error: {response.StatusCode}", Color.Red);
                }
            }
                catch (HttpRequestException)
                {
                    SetStatus("Cannot connect to EquipmentAPI. Is it running?", Color.Red);
                }
                finally
                {
                    btnCreate.Enabled = true;
                }
            }
        
            private void SetStatus(string message, Color color)
            {
                lblStatus.Text = message;
                lblStatus.ForeColor = color;
            }
    }
}

