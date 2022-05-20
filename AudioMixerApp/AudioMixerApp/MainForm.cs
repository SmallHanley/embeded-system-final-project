﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System.Threading;
using AudioPlayerApp;

namespace AudioMixerApp
{
    public partial class mainForm : Form
    {
        Uart uart;

        public mainForm()
        {
            InitializeComponent();
            infoCard1.setId(1);
            infoCard2.setId(2);
            deck1.infoCard = infoCard1;
            deck2.infoCard = infoCard2;
            serialTimer.Stop();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void openSerialButton_Click(object sender, EventArgs e)
        {
            if (!Int32.TryParse(serialSpeed.Text, out int speed)) {
                MessageBox.Show("Fail to parse speed " + serialSpeed.Text);
                return;
            }

            uart = new Uart(serialLine.Text, speed);
            if (!uart.OpenSerial()) {
                MessageBox.Show("Fail to open " + serialLine.Text);
                uart = null;
                return;
            }

            serialTimer.Start();
        }

        private void serialTimer_Tick(object sender, EventArgs e)
        {
            string line = uart.ReadLines();
            if (line != "") {
                char[] rowSeparator = { '\n' };
                string[] rows = line.Split(rowSeparator, StringSplitOptions.RemoveEmptyEntries);

                // only use row[0], drop other rows
                char[] separator = { ' ' };
                string[] dataString = rows[0].Split(separator, StringSplitOptions.RemoveEmptyEntries);
                int[] data = Array.ConvertAll(dataString, delegate (string s) {
                    if (!Int32.TryParse(s, out int val)) {
                        val = -1;
                    }
                    return val;
                });

                Console.Write(line);

                int Adc = 0, Btn1 = 2, Btn2 = 1;

                if (data[0] == Adc) {
                    deck1.changeVolume(data[2]);
                    deck2.changeVolume(data[1]);
                } else if (data[0] == Btn1) {
                    if (data[1] == 0) {
                        deck1.pause();
                    } else {
                        deck1.play();
                    }
                } else if (data[0] == Btn2) {
                    if (data[1] == 0) {
                        deck2.pause();
                    } else {
                        deck2.play();
                    }
                }


            }
        }

        private void closeSerialButton_Click(object sender, EventArgs e)
        {
            if (uart != null) {
                uart.CloseSerial();
                uart = null;
            }

            serialTimer.Stop();
        }
    }
}
