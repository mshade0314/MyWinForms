using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyWinForms
{
    public partial class Form1 : Form
    {
        public State FrmState { get; set; }
        private CancellationTokenSource _cts;
        public Form1(State state)
        {
            this.FrmState = state;
            InitializeComponent();
        }

        public void ControlEnabled()
        {
            this.btnStart.Enabled = true;
            this.btnStop.Enabled = false;
        }

        public void ControlDisabled()
        {
            this.btnStart.Enabled = false;
            this.btnStop.Enabled = true;
        }

        private async void btnStart_ClickAsync(object sender, EventArgs e)
        {
            this.FrmState.Process(this);

            _cts = new CancellationTokenSource();
            var complete = await HeavyMethodAsync(_cts.Token);
            MessageBox.Show(complete ? "処理が完了しました" : "処理を中断しました");

            this.FrmState.Idle(this);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _cts.Cancel();
        }

        private async Task<bool> HeavyMethodAsync(CancellationToken token)
        {
            try
            {
                await Task.Delay(5 * 1000, token);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }
    }

    public abstract class State
    {
        public abstract void Process(Form1 frm);
        public abstract void Idle(Form1 frm);
        public abstract void SetControlState(Form1 frm);
    }

    class IdleState : State
    {
        public override void SetControlState(Form1 frm)
        {
            frm.ControlEnabled();
        }

        public override void Process(Form1 frm)
        {
            frm.FrmState = new ProcessingState();
            frm.FrmState.SetControlState(frm);
        }

        public override void Idle(Form1 frm)
        {
            frm.FrmState = this;
        }
    }

    class ProcessingState : State
    {
        public override void SetControlState(Form1 frm)
        {
            frm.ControlDisabled();
        }

        public override void Process(Form1 frm)
        {
            frm.FrmState = this;
        }

        public override void Idle(Form1 frm)
        {
            frm.FrmState = new IdleState();
            frm.FrmState.SetControlState(frm);
        }
    }
}
