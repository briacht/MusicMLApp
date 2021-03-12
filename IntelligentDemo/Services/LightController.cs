using System.IO.Ports;
using System.Windows.Media;

namespace IntelligentDemo.Services
{
    public class LightController
    {
        private SerialPort _port;
        private bool _connected;

        public LightController()
        {
            try
            {
                _port = new SerialPort("COM3", 9600);
                _port.Open();
                _connected = true;
            }
            catch (System.Exception)
            {
                _connected = false;
            }

            SetColor(Color.FromRgb(0, 0, 0));
        }

        public bool IsConnected => _connected;

        public void SetColor(Color color)
        {
            if (_connected)
            {
                var msg = $"{color.R},{color.G},{color.B},";
                _port.Write(msg);
            }
        }

        public void Dispose()
        {
            SetColor(Color.FromRgb(0, 0, 0));

            if (_connected)
            {
                _port.Close();
                _port.Dispose();
                _connected = false;
            }
        }
    }
}
