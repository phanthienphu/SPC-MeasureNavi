using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectionMeasure.Helper
{
    public interface IJBSerialPort
    {
        void Write(string text);
        string Read();
        void Open();
        void Close();
        int ReadTimeout { get; set; }
        event SerialDataReceivedEventHandler DataReceived;
        bool IsOpen { get; }
        string ReadExisting();
        string PortName { get; set; }
        int BaudRate { get; set; }
        string ReadLine();
    }
}
