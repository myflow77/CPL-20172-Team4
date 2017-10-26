﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Pipes;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace CustomMessenger
{
	public partial class Form1 : Form
	{
		Thread sendingThread;
		NamedPipeClientStream sendingPipe;
		string sendingPipeName;

		public Form1()
		{
			InitializeComponent();

			sendingPipeName = "hello";

			button1.Click += new EventHandler(this.Button1Click);
			button2.Click += new EventHandler(this.Button2Click);

			sendingThread = new Thread(ConnectPipe);
			sendingThread.Start();
		}

		public void ConnectPipe()
		{
			try
			{
				sendingPipe = new NamedPipeClientStream(sendingPipeName);
			}
			catch (Exception e)
			{
				MessageBox.Show("An error has occurred while pipe open.\n" + e.Message);
				return;
			}

			while (!sendingPipe.IsConnected)
			{
				label1.Text = "Waiting";
				sendingPipe.Connect();
			}

			int count = 0;
			while (sendingPipe.IsConnected)
			{
				label1.Text = "Connecting " + count++;
				try
				{
					PacketData dataToSend = new PacketData();
					dataToSend.messageLog = new List<string>();
					dataToSend.messageLog.Add("message");
					dataToSend.messageLog.Add("log");
					dataToSend.typingMessage = textBox1.Text;
					string xmlData = SerializeToXml(dataToSend);
					StreamWriter writer = new StreamWriter(sendingPipe);
					writer.WriteLine(xmlData);
					writer.Flush();

					//Thread.Sleep(1000);
				}
				catch (Exception ex)
				{
					if (!ex.Message.StartsWith("Pipe is broken."))
					{
						MessageBox.Show("An error has occurred while seding data. \n" + ex.Message);
						return;
					}
				}
			}
			sendingPipe.Close();
		}

		public static string SerializeToXml(PacketData data)
		{
			string xmlData = null;

			StringWriter stringWriter = null;

			XmlSerializer serializer = new XmlSerializer(data.GetType());
			stringWriter = new StringWriter();
			serializer.Serialize(stringWriter, data);

			xmlData = stringWriter.ToString().Replace(Environment.NewLine, " ");

			stringWriter.Close();

			return xmlData;
		}

		// send button click
		private void Button1Click(object sender, EventArgs e)
		{
			
		}

		private void Button2Click(object sender, EventArgs e)
		{
			if (sendingPipe.IsConnected)
				sendingPipe.Close();
			if (sendingThread.IsAlive)
				sendingThread.Abort();
		}

		private void Form1_Closed(object sender, System.EventArgs e)
		{
			if (sendingPipe.IsConnected)
				sendingPipe.Close();
			if (sendingThread.IsAlive)
				sendingThread.Abort();
		}
	}
}