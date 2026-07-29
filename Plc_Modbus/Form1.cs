using Plc_Modbus.data;
using Plc_Modbus.Model;
using System.ComponentModel;

namespace Plc_Modbus
{
    public partial class Form1 : Form
    {
        private readonly Mod_Conn _Conn = new Mod_Conn();
        private readonly Plc_Reader plc_Reader;
        private readonly Plc_Writer _plcWriter;
        private BindingList<PlcDto> _readDataList;
        private Dictionary<string, bool> cmbCoil = new Dictionary<string, bool>() { { "켜기", true }, { "끄기", false } };
        private readonly DB_Conn _dbConn = new DB_Conn();
        public Form1()
        {
            InitializeComponent();
            comboBoxInit();
            btnWrite.Click += btnWrite_Click;
            btnSpeed.Click += btnSpeed_Click;
            plc_Reader = new Plc_Reader(_Conn, UpdateGrid);
            _plcWriter = new Plc_Writer(_Conn);
            _readDataList = new BindingList<PlcDto>();
            dgvPlc.DataSource = _readDataList;
            this.Load += Form1_Load;
        }

        private void Form1_Load(object? sender, EventArgs? e)
        {
            _ = Task.Run(() => plc_Reader.ReadPlc());
            _ = ConnDb();
        }

        private void comboBoxInit()
        {
            BindCoilComboBox(cmbCoil1);
            BindCoilComboBox(cmbCoil2);
        }

        private async Task ConnDb()
        {
            bool result = await _dbConn.Retry();
            if (result) lblConn.Text = "연결성공";
        }

        private async void btnWrite_Click(object? sender, EventArgs? e)
        {
            await writeCoil();
        }

        private async void btnSpeed_Click(object? sender, EventArgs? e)
        {
            await writeHoding();
        }

        private void BindCoilComboBox(ComboBox comboBox)
        {
            comboBox.DataSource = new BindingSource(cmbCoil, null);
            comboBox.DisplayMember = "key";
            comboBox.ValueMember = "value";
        }

        private void UpdateGrid(PlcDto read)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<PlcDto>(UpdateGrid), read);
                return;
            }
            _readDataList.Insert(0, read);
            if (_readDataList.Count > 1000) _readDataList.RemoveAt(_readDataList.Count - 1);
        }

        private async Task writeCoil()
        {
            bool[] writeCoils = new bool[2];
            writeCoils[0] = (bool)cmbCoil1.SelectedValue;
            writeCoils[1] = (bool)cmbCoil2.SelectedValue;

            await _plcWriter.writeData(writeCoils);
        }

        private async Task writeHoding()
        {
            ushort[] writeHolding = new ushort[10];
            writeHolding[0] = (ushort)(ushort.Parse(txtSpeed.Text) * 10);
            await _plcWriter.writeHolding(writeHolding);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _Conn.Dispose();
            _dbConn.Dispose();
            base.OnFormClosed(e);
        }
    }
}
