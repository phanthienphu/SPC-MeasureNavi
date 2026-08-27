using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectionMeasure.Helper
{
    public class MockSerialPort : IJBSerialPort
    {
        private bool _mockIsOpen = false;
        public void Close() => _mockIsOpen = false;

        public void Open() => _mockIsOpen = true;

        public string Read()
        {
            return "OK_Man";
        }

        public void Write(string text)
        {
            Console.WriteLine($"[Mock] Đã gửi: {text}");
        }
        public int ReadTimeout { get; set; }

        public event SerialDataReceivedEventHandler DataReceived;
        public bool IsOpen => _mockIsOpen;
        public string ReadExisting()
        {
            return "HUNG_DATA_MOCK_12345";
        }
        public string PortName { get; set; } = "COM_MOCK";
        public int BaudRate { get; set; } = 9600;
        public string ReadLine()
        {
            return "Ok_man";
        }
    }
}
