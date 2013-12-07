using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NetComm;

namespace ChatApp.Forms
{
    public partial class FrmClient : Form
    {
        public string Nombre { get; set; } 
        private NetComm.Client cliente = new Client();
        private ArrayList clientList = new ArrayList();


        public FrmClient()
        {
            InitializeComponent();
            
        }

        private void FrmClient_Load(object sender, EventArgs e)
        {
            cliente.Connected += ClienteConectado;
            cliente.DataReceived += RecibioMensaje;
            cliente.Disconnected += ClienteDesconectado;
            cliente.errEncounter += ErrorCliente;
            cliente.Connect("127.0.0.1", 3330, Nombre); 
            this.Text = Nombre;
        }

        void ErrorCliente(Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
        }

        void ClienteDesconectado()
        {
            MessageBox.Show("Te has desonectado");
        }

        void RecibioMensaje(byte[] data, string iD)
        {
            string mensaje = ConvertBytesToString(data);
            if (mensaje.StartsWith("CList::"))
            {
                string mensajes = mensaje.Replace("CList::","");
                rtbConOut.AppendText("Los clientes conectados son " + mensajes + Environment.NewLine);
            }
            else
            {
                rtbConOut.AppendText(mensaje  + Environment.NewLine);
            }
        }

        void ClienteConectado()
        {
            rtbConOut.AppendText("Conectado! " + Environment.NewLine);
        }

        string ConvertBytesToString(byte[] bytes)
        {
            return ASCIIEncoding.ASCII.GetString(bytes);
        }

        byte[] ConvertStringToBytes(string str)
        {
            return ASCIIEncoding.ASCII.GetBytes(str);
        }

        private void FrmClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            cliente.SendData(ConvertStringToBytes("Cerrando conexion"), "0");
            cliente.Disconnect();
        }

        private void TbInputKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter || e.KeyData == Keys.Return) 
            {
                if (tbInput.Text.Length > 0) 
                {
                    cliente.SendData(ConvertStringToBytes(tbInput.Text), "0");
                    rtbConOut.AppendText(tbInput.Text + Environment.NewLine);
                    tbInput.Text = "";
                }
            }
        }

        private void BtnSendClick(object sender, EventArgs e)
        {
            if (tbInput.Text.Length > 0) 
            {
                cliente.SendData(ConvertStringToBytes(tbInput.Text), "0");
                rtbConOut.AppendText(tbInput.Text + Environment.NewLine);
                tbInput.Text = "";
            }
        }
    }
}
