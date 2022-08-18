using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;

namespace NandDump.Core
{
    class SerialReader
    {
        SerialPort SP;

        public SerialReader(string Port)
        {
            SP = new SerialPort(Port, 115200);
        }

        public SerialReader(string Port, int Baud)
        {
            SP = new SerialPort(Port, Baud);
        }
        public void Open()
        {
            SP.Open();
            SP.DiscardInBuffer();
            SP.DiscardOutBuffer();
        }

        public void Close()
        {
            SP.Close();
        }

        public void Write(string Cmd)
        {
            SP.Write(Cmd);
        }

        public string Read()
        {
            return SP.ReadLine();
        }
    }
}
