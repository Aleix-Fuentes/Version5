﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        Socket server;
        Thread atender;

        delegate void DelegadoParaEscribir(string mensaje);
        public Form1()
        {
            InitializeComponent();
            //CheckForIllegalCrossThreadCalls = false; //Necesario para que los elementos de los formularios puedan ser
            //accedidos desde threads diferentes a los que los crearon
            //dataGridView1.RowCount = 10;
            dataGridView1.ColumnCount = 1;
            richTextBox1.Visible = false;
            MensajeaEnviar.Visible = false;
            EnviarMensaje.Visible = false;
            invitarbutton.Visible = false;
            Invitado.Visible = false;
            InvitadoTB.Visible = false;
        }
        public void EscribirenChat(string mensaje)
        {
            richTextBox1.AppendText(mensaje + "\n");
        }
        public void ActualizarListaConectados(string mensaje)
        {
            string[] sConectados = mensaje.Split(',');
            int n = Convert.ToInt32(sConectados[0]);
            dataGridView1.RowCount = n;

            int j = 1;
            for (int i = 0; i < n; i++)
            {
                dataGridView1[0, i].Value = sConectados[j];
                j++;
            }
        }
        private void AtenderServidor ()
        {
            while (true)
            {
                
                //Recibimos la respuesta del servidor
                byte[] msg2 = new byte[80];
                server.Receive(msg2);
                string[] trozos = Encoding.ASCII.GetString(msg2).Split('/');
                string cod = trozos[0];
                int codigo = Convert.ToInt32(cod);
                string mensaje = trozos[1].Split('\0')[0];

                switch (codigo) 
                {
                    case 1:
                        MessageBox.Show("Se ha creado usuario correctamente ");
                        break;

                    case 2:
                        MessageBox.Show(mensaje);
                        //MessageBox.Show("probando", "no probando", MessageBoxButtons.YesNo);
                        break;

                    case 3:
                        if (mensaje == "NO")
                        {
                            MessageBox.Show("Usuario no encontrado");
                        }
                        else
                        {
                            MessageBox.Show("El % de partidas ganadas es de: " + mensaje + "%");
                        }
                        break;

                    case 4:
                        if (mensaje == "NO")
                        {
                            MessageBox.Show("No hay partida a esa hora o fecha(Recuerda que la fecha se pone XX-XX-XXXX y la hora XX:XX)");
                        }
                        else
                        {
                            MessageBox.Show("El usuario que gano esa partida fue: " + mensaje);
                        }
                        break;

                    case 5:
                        if (mensaje == "NO")
                        {

                            string mennsaje = "0/" + Usuario.Text;

                            byte[] msg3 = System.Text.Encoding.ASCII.GetBytes(mennsaje);
                            server.Send(msg3);

                            this.BackColor = Color.Red;

                            MessageBox.Show("No se ha iniciado sesion correctamente ");


                        }
                        else
                        {
                            MessageBox.Show("Se ha podido iniciar sesion");
                        }
                        break;
                    
                    case 7:
                         //string[] sConectados = mensaje.Split(',');
                         //       int n = Convert.ToInt32(sConectados[0]);
                         //   dataGridView1.RowCount = n;
                           
                         //   int j = 1;
                         //   for (int i = 0; i < n; i++)
                         //   {
                         //       dataGridView1[0, i].Value = sConectados[j];
                         //       j++;
                         //   }
                        DelegadoParaEscribir delegado = new DelegadoParaEscribir(ActualizarListaConectados);
                        dataGridView1.Invoke(delegado, new object[] { mensaje });
                        break;

                    case 8:
                        //if(mensaje == "error")
                        //{
                        //    MessageBox.Show("No se ha podido invitar al usuario");
                        //}
                        //else
                        //{  
                            string[] invit = mensaje.Split(',');
                            int n = Convert.ToInt32(invit[1]);
                            var result = MessageBox.Show(invit[0] + " te ha invitado, aceptas?"," Nueva invitacion ",MessageBoxButtons.YesNo);
                            //MessageBox.Show("probando", "no probando", MessageBoxButtons.YesNo);
                            if (result == DialogResult.Yes)
                            {
                                string respuesta = "9/SI/" + Usuario.Text + "/" + invit[0] + "/" + n  ;

                                byte[] msg = System.Text.Encoding.ASCII.GetBytes(respuesta);
                                server.Send(msg);
                                //richTextBox1.Visible = true;
                                //MensajeaEnviar.Visible = true;
                                //EnviarMensaje.Visible = true;
                            }
                            else
                            {
                                string respuesta = "9/NO/" + Usuario.Text + "/" + invit[0] + n;

                                byte[] msg = System.Text.Encoding.ASCII.GetBytes(respuesta);
                                server.Send(msg);

                            }
                          
                        //}
                        break;
                    case 9:
                        MessageBox.Show(mensaje);
                        break;

                    case 10:
                        DelegadoParaEscribir delegado2 = new DelegadoParaEscribir(EscribirenChat);
                        richTextBox1.Invoke(delegado2, new object[] { mensaje });
                        break;
                }
            }
        }
        private void Conectarse_button_Click(object sender, EventArgs e)
        {
            
            if (Usuario.Text == "" || Contraseña.Text == "")
            {
                MessageBox.Show("Error al iniciar sesion, falta llenar los campos");
            }
            else
            {
                //Creamos un IPEndPoint con el ip del servidor y puerto del servidor 
                //al que deseamos conectarnos
                //IPAddress direc = IPAddress.Parse("192.168.56.102");
                IPAddress direc = IPAddress.Parse("147.83.117.22");
                IPEndPoint ipep = new IPEndPoint(direc, 50083);
                this.BackColor = Color.Red;


                //Creamos el socket 
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    server.Connect(ipep);//Intentamos conectar el socket
                    this.BackColor = Color.Green;
                    //MessageBox.Show("Conectado");

                }
                catch (SocketException ex)
                {
                    //Si hay excepcion imprimimos error y salimos del programa con return 
                    MessageBox.Show("No he podido conectar con el servidor");
                    return;
                }
                string mensaje = "5/" + Usuario.Text + "/" + Contraseña.Text;
                // Enviamos al servidor el nombre tecleado
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                server.Send(msg);

               
               
                //pongo en marcha el thread que atenderá los mensajes del servidor
                ThreadStart ts = delegate { AtenderServidor(); };
                atender = new Thread(ts);
                atender.Start();

            }
        }

        private void Desconectarse_button_Click(object sender, EventArgs e)
        {
            //Mensaje de desconexión
            string mensaje = "0/";

            byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
            server.Send(msg);

            //// Nos desconectamos
            atender.Abort();
            this.BackColor = Color.Gray;
            server.Shutdown(SocketShutdown.Both);
            server.Close();
        }

        private void ConsultaAleix_button_Click(object sender, EventArgs e)
        {
            string mensaje = "3/" + Usuario.Text;
            // Enviamos al servidor el nombre tecleado
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
            server.Send(msg);

            ////Recibimos la respuesta del servidor
            //    byte[] msg2 = new byte[80];
            //    server.Receive(msg2);
            //    mensaje = Encoding.ASCII.GetString(msg2).Split('\0')[0];
            //if (mensaje == "NO")
            //{
            //    MessageBox.Show("Usuario no encontrado");
            //}
            //else
            //{                
            //    MessageBox.Show("El % de partidas ganadas es de: " + mensaje + "%");
            //}
        }

        private void Usuario_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void ConsultaDani_button_Click(object sender, EventArgs e)
        {
            string mensaje = "2/" ;
            // Enviamos al servidor el nombre tecleado
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
            server.Send(msg);

            ////Recibimos la respuesta del servidor
            //byte[] msg2 = new byte[80];
            //server.Receive(msg2);
            //mensaje = Encoding.ASCII.GetString(msg2).Split('\0')[0];
            //MessageBox.Show( mensaje);
        }

        private void ConsultaIngrid_button_Click(object sender, EventArgs e)
        {
            string mensaje = "4/" + Fecha.Text + "/" + Hora.Text;
            // Enviamos al servidor el nombre tecleado
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
            server.Send(msg);

            ////Recibimos la respuesta del servidor
            //byte[] msg2 = new byte[80];
            //server.Receive(msg2);
            //mensaje = Encoding.ASCII.GetString(msg2).Split('\0')[0];
            //if (mensaje == "NO")
            //{
            //    MessageBox.Show("No hay partida a esa hora o fecha(Recuerda que la fecha se pone XX-XX-XXXX y la hora XX:XX)");
            //}
            //else
            //{
            //    MessageBox.Show("El usuario que gano esa partida fue: " + mensaje);
            //}

           
        }

        private void IniciarSesion_button_Click(object sender, EventArgs e)
        {
            //string mensaje = "5/" + Usuario.Text + "/" + Contraseña.Text;
            //// Enviamos al servidor el nombre tecleado
            //byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
            //server.Send(msg);

            ////Recibimos la respuesta del servidor
            //byte[] msg2 = new byte[80];
            //server.Receive(msg2);
            //mensaje = Encoding.ASCII.GetString(msg2).Split('\0')[0];
            //if (mensaje == "SI")
            //{
            //    MessageBox.Show("Se ha iniciado sesion correctamente ");
            //}
            //else
            //{
            //    MessageBox.Show("No se ha podido iniciar sesion"); 
            //}

        }

        private void CrearUsuario_button_Click(object sender, EventArgs e)
        {

            if (Usuario.Text == "" || Contraseña.Text == "")
            {
                MessageBox.Show("Error al iniciar sesion, falta llenar los campos");
            }
            else
            {
                //Creamos un IPEndPoint con el ip del servidor y puerto del servidor 
                //al que deseamos conectarnos
                IPAddress direc = IPAddress.Parse("147.83.117.22");
                IPEndPoint ipep = new IPEndPoint(direc, 50083);
                this.BackColor = Color.Red;


                //Creamos el socket 
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    server.Connect(ipep);//Intentamos conectar el socket
                    this.BackColor = Color.Green;
                    //MessageBox.Show("Conectado");

                }
                catch (SocketException ex)
                {
                    //Si hay excepcion imprimimos error y salimos del programa con return 
                    MessageBox.Show("No he podido conectar con el servidor");
                    return;
                }
                string mensaje = "1/" + Usuario.Text + "/" + Contraseña.Text;
                // Enviamos al servidor el nombre tecleado
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                server.Send(msg);

                ////Recibimos la respuesta del servidor
                //byte[] msg2 = new byte[80];
                //server.Receive(msg2);
                //mensaje = Encoding.ASCII.GetString(msg2).Split('\0')[0];
                //MessageBox.Show("Se ha creado usuario correctamente ");
                //if (mensaje == "SI")
                //{
                //    MessageBox.Show("Se ha creado usuario correctamente ");
                //}
                //else
                //{
                //    MessageBox.Show("No se ha creado usuario correctamente");
                //}

                
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    string mensaje = "6/";
        //    // Enviamos al servidor el nombre tecleado
        //    byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
        //    server.Send(msg);

        //    //Recibimos la respuesta del servidor
        //    byte[] msg2 = new byte[80];
        //    server.Receive(msg2);
        //    mensaje = Encoding.ASCII.GetString(msg2).Split('\0')[0];
        //    contl.Text = mensaje;
        //}

        //private void button2_Click(object sender, EventArgs e)
        //{
        //    string mensaje = "7/";
        //    // Enviamos al servidor el nombre tecleado
        //    byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
        //    server.Send(msg);

        //    //Recibimos la respuesta del servidor
        //    //byte[] msg2 = new byte[80];
        //    //server.Receive(msg2);
        //    //mensaje = Encoding.ASCII.GetString(msg2).Split('\0')[0];
        //    //Conectados.Text = mensaje;
        //}

        private void Conectados_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void contl_Click(object sender, EventArgs e)
        {

        }

        private void invitarbutton_Click(object sender, EventArgs e)
        {
            string invitados = null;
            int i = 0;
            bool invito = false;
            int numinvitados = dataGridView1.SelectedCells.Count;
            while (i < dataGridView1.Rows.Count)
            {
                if (dataGridView1.Rows[i].Cells[0].Selected)
                {
                    if (dataGridView1.Rows[i].Cells[0].Value == null)
                    {
                        MessageBox.Show("No has seleccionado a ningun jugador");
                        invito = false;
                        break;
                    }
                    else if (dataGridView1.Rows[i].Cells[0].Value.ToString() == Usuario.Text)
                    {
                        MessageBox.Show("Te estas invitando a ti mismo");
                        invito = false;
                        break;
                    }
                    else
                    {
                        invitados = invitados + ",";
                        invitados = invitados + dataGridView1.Rows[i].Cells[0].Value.ToString();
                        i = i + 1;
                        invito = true;
                    }
                }
                else
                {
                    i = i + 1;
                }

            }

            if (invito == true)
            {
                string mensaje = "8/" + Usuario.Text + "," + Convert.ToString(numinvitados) + invitados;
                // Enviamos al servidor el nombre introducido
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                server.Send(msg);

                MessageBox.Show("Has invitado a los jugadores, esperando a que acepten" + invitados);
            }
            
            ////List<string> invitados = new List<string>();
            //string[] colorsArray = new string[4]
            //string mensaje = "8/" + Usuario.Text + "," + dataGridView1.SelectedCells.ToString() + ",";
            //// Enviamos al servidor el nombre tecleado
            //byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
            //server.Send(msg);
        }

        private void EnviarMensaje_Click(object sender, EventArgs e)
        {
            string mensaje = "10/" + Usuario.Text +": " +MensajeaEnviar.Text + "/";
            MensajeaEnviar.Clear();
            // Enviamos al servidor el nombre tecleado
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
            server.Send(msg);  
        }

        private void MensajeaEnviar_TextChanged(object sender, EventArgs e)
        {

        }

        private void CrearPartida_Click(object sender, EventArgs e)
        {
            richTextBox1.Visible = true;
            MensajeaEnviar.Visible = true;
            EnviarMensaje.Visible = true;
            invitarbutton.Visible = true;
            Invitado.Visible = true;
            InvitadoTB.Visible = true;
        }


    }
}
