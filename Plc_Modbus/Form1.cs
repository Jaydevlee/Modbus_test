using Plc_Modbus.data;
using Plc_Modbus.Model;
using System.ComponentModel;

namespace Plc_Modbus
{
    public partial class Form1 : Form
    {
        private readonly Mod_Conn _connection = new();
        private readonly Plc_Reader _plcReader;
        private readonly Plc_Writer _plcWriter;
        private readonly DB_Conn _dbConnection = new();
        private readonly CancellationTokenSource _formCts = new();
        private readonly BindingList<PlcDto> _readDataList = [];
        private readonly Dictionary<string, bool> _coilOptions = new()
        {
            { "켜기", true },
            { "끄기", false }
        };

        private Task? _readerTask;
        private bool _shutdownComplete;

        public Form1()
        {
            InitializeComponent();
            InitializeControls();

            _plcReader = new Plc_Reader(
                _connection,
                UpdateGrid,
                UpdatePlcConnectionState);
            _plcWriter = new Plc_Writer(_connection);
            dgvPlc.DataSource = _readDataList;

            Load += Form1_Load;
            btnWrite.Click += btnWrite_Click;
        }

        private void InitializeControls()
        {
            BindCoilComboBox(cmbCoil1);
            BindCoilComboBox(cmbCoil2);

            // Holding Register 40001 is now the read-only production count.
            btnSpeed.Enabled = false;
            txtSpeed.Enabled = false;
            btnSpeed.Text = "사용 안 함";
            lblSpeed.Text = "생산수량은 PLC에서 자동 증가";
            lblConn.Text = "DB 연결 확인 중";
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            _readerTask = _plcReader.ReadPlcAsync(_formCts.Token);
            _ = ConnectDbAsync();
        }

        private async Task ConnectDbAsync()
        {
            try
            {
                bool connected = await _dbConnection.Retry(_formCts.Token);
                if (connected && !IsDisposed)
                {
                    SetLabelText(lblConn, "DB 연결 성공");
                }
            }
            catch (OperationCanceledException)
            {
                // Normal application shutdown.
            }
        }

        private async void btnWrite_Click(object? sender, EventArgs e)
        {
            if (cmbCoil1.SelectedValue is not bool runCommand
                || cmbCoil2.SelectedValue is not bool forceError
                || txtTargetQuantity.Text.IsWhiteSpace())
            {
                MessageBox.Show("RUN과 ERROR 명령을 선택해주세요.");
                return;
            }

            btnWrite.Enabled = false;
            try
            {
                ushort targetQuantity = ushort.Parse(txtTargetQuantity.Text);
                bool writeCommand = await _plcWriter.WriteCommandsAsync(
                    runCommand, forceError, _formCts.Token);
                bool writeQuantity = await _plcWriter.WriteTargetQuantityAsync(
                    targetQuantity, _formCts.Token);
                if (!writeCommand && !writeQuantity && !_formCts.IsCancellationRequested)
                {
                    MessageBox.Show("PLC 명령 전송에 실패했습니다.");
                }
            }
            finally
               {
                if (!_formCts.IsCancellationRequested)
                    btnWrite.Enabled = true;
            }
        }

        private void BindCoilComboBox(ComboBox comboBox)
        {
            comboBox.DataSource = new BindingSource { DataSource = _coilOptions };
            comboBox.DisplayMember = "Key";
            comboBox.ValueMember = "Value";
        }

        private void UpdateGrid(PlcDto data)
        {
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action<PlcDto>(UpdateGrid), data);
                return;
            }

            _readDataList.Insert(0, data);
            if (_readDataList.Count > 1000)
                _readDataList.RemoveAt(_readDataList.Count - 1);
        }

        private void UpdatePlcConnectionState(bool connected)
        {
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action<bool>(UpdatePlcConnectionState), connected);
                return;
            }

            Text = connected
                ? "Mini MES Collector - PLC ONLINE"
                : "Mini MES Collector - PLC DISCONNECTED";
        }

        private void SetLabelText(Label label, string text)
        {
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action<Label, string>(SetLabelText), label, text);
                return;
            }

            label.Text = text;
        }

        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            if (_shutdownComplete)
            {
                base.OnFormClosing(e);
                return;
            }

            e.Cancel = true;
            Enabled = false;
            _formCts.Cancel();
            _dbConnection.Dispose();

            if (_readerTask is not null)
            {
                try
                {
                    await _readerTask;
                }
                catch (OperationCanceledException)
                {
                    // Normal application shutdown.
                }
            }

            _plcReader.Dispose();
            _connection.Dispose();
            _formCts.Dispose();
            _shutdownComplete = true;
            Close();
        }
    }
}
