using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ChatApp.Forms;
using NetComm;

namespace ChatApp
{
    public partial class Form1 : Form
    {
        private NetComm.Host servidor = new Host(3330); 
        private int conexionactiva = 0;
        private delegate void DisplayInformationDelegate(string s);
        private ArrayList listaClientes = new ArrayList(); 

        public Form1()
        {
            InitializeComponent();
            servidor.ConnectionClosed += ConexionTerminada;
            servidor.DataReceived += RecibirMensaje;
            servidor.DataTransferred += EnvioDatos;
            servidor.errEncounter += ErrorServidor;
            servidor.lostConnection += PerdidaConexion;
            servidor.onConnection += ServerOnConnection;
            servidor.StartConnection();
            rtbConOut.AppendText(Environment.NewLine);
        }

        void ServerOnConnection(string id)
        {
            conexionactiva++;
            lblConnections.Text = conexionactiva.ToString();
            MostrarInformacion(String.Format("El usuario #{0} se ha conectado", id));
            listaClientes.Add(id);
            EnviarListaClientes();
        }
  
        private void EnviarListaClientes()
        {
            MostrarInformacion("Se actualizo lista de usuarios"); 
            foreach (string c in listaClientes)
            {
                foreach (string c2 in listaClientes)
                {
                    byte[] d = ConvertStringToBytes("CList::" + c);
                    servidor.SendData(c2, d);
                }
            }
        }

        void PerdidaConexion(string id)
        {
            conexionactiva--;
            lblConnections.Text = conexionactiva.ToString();
            MostrarInformacion(String.Format("El usuario #{0} perdió la conexión", id));
            listaClientes.Remove(id);
        }

        void ErrorServidor(Exception ex)
        {
            MostrarInformacion("Error en el servidor: " + ex.Message);
        }

        void EnvioDatos(string sender, string receptor, byte[] data)
        {
            string idEmisor = (string)sender;
            if (receptor == "0")
            {
                MostrarInformacion("Mensaje recibido del usuario #" + idEmisor);
                switch (ConvertBytesToString(data))
                {
                    case "CLOSING":
                        conexionactiva--;
                        lblConnections.Text = conexionactiva.ToString();
                        MostrarInformacion("El usuario #" + idEmisor + " se desconectó");
                        listaClientes.Remove(idEmisor);
                        EnviarListaClientes();
                        break;
                   
                    default:
                        foreach (string id in listaClientes)
                        {
                            if (id != idEmisor) 
                            {
                                servidor.SendData(id, data);
                            }
                        }
                        break;
                }
            }
            else
            {
                MostrarInformacion("Mensaje recibido del usuario #" + idEmisor + " para el cliente " + receptor);
            }
        }

        void RecibirMensaje(string iD, byte[] data)
        {
            if (ConvertBytesToString(data) == "CLOSING")
            {
                MostrarInformacion("El cliente " + iD + " se desconecto");
            }
        }

        void ConexionTerminada()
        {
            MostrarInformacion("Se cerro la conexion");
        }

        //Crear cliente nuevo
        private void BtnNewClient_Click(object sender, EventArgs e)
        {
            FrmClient cliente = new FrmClient();
            cliente.Nombre = (conexionactiva + 1).ToString();
            cliente.Show();
        }

        string ConvertBytesToString(byte[] bytes)
        {
            return ASCIIEncoding.ASCII.GetString(bytes);
        }

        byte[] ConvertStringToBytes(string str)
        {
            return ASCIIEncoding.ASCII.GetBytes(str);
        }

        void MostrarInformacion(string s)
        {
            if (this.rtbConOut.InvokeRequired)
            {
                DisplayInformationDelegate d = new DisplayInformationDelegate(MostrarInformacion);
                this.rtbConOut.Invoke(d, new object[] { s });
            }
            else
            {
                this.rtbConOut.AppendText(s + Environment.NewLine);
            }
        }
    }
}
