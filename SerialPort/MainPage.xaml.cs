using System;
using Windows.UI.Xaml.Controls;
using Windows.Devices.SerialCommunication;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using System.Text;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace SerialPort
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        SerialDevice device;
        public MainPage()
        {
            this.InitializeComponent();
            for (int i = 0; i < 10; i++)
            {
                Cb_PortName.Items.Add($"COM{i}");
            }
        }


        async Task SelectDevice(string portName)
        {
            string args = SerialDevice.GetDeviceSelector(portName);
            var myDevices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(args, null);
            if (myDevices.Count == 0)
            {
                Txt_Info.Text = "未找到对应名称的设备";
                return;
            }
            else
            {
                Txt_Info.Text = "找到对应名称的设备,正在建立对应关系";
            }
            device = await SerialDevice.FromIdAsync(myDevices[0].Id);
            if (device != null)
            {
                Txt_Info.Text += $"\n{device.PortName}";
                device.BaudRate = 9600;
                PortInit();
            }
            else
            {
                //如果不在应用清单里添加串口节点，则一直是null（晕）
                Txt_Info.Text += "NULL";
            }
        }

        private async void Cb_PortName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string name = Cb_PortName.SelectedItem.ToString();
            await SelectDevice(name);
        }

        private async Task<IBuffer> Readdata()
        {
            if (device == null)
            {
                return null;
            }
            //device.InputStream
            //uint length = device.BytesReceived;
            uint length = 8;
            IBuffer buffer = new Windows.Storage.Streams.Buffer(length);
            await device.InputStream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.ReadAhead);
            return buffer;
        }

        private async Task<bool> SendData(string data)
        {
            if (device == null)
            {
                return false;
            }
            try
            {
                await device.OutputStream.WriteAsync(GetBufferFromString(data));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private IBuffer GetBufferFromString(String str)
        {
            using (InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream())
            {
                using (DataWriter dataWriter = new DataWriter(memoryStream))
                {
                    dataWriter.WriteString(str);
                    return dataWriter.DetachBuffer();
                }
            }
        }

        static byte[] BufferToBytes(IBuffer buf)
        {
            using (var dataReader = DataReader.FromBuffer(buf))
            {
                var bytes = new byte[buf.Length];
                dataReader.ReadBytes(bytes);
                return bytes;
            }
        }

        // 发送
        private async void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (device == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(Txt_Input.Text))
            {
                return;
            }
            bool send = await SendData(Txt_Input.Text);
            if (send == true)
            {
                Txt_Info.Text += $"\n已发送数据:{Txt_Input.Text}";
            }
        }

        // 接受
        private async void Button_Click_1(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (device == null)
            {
                return;
            }
            StringBuilder builder = new StringBuilder();
            StringBuilder builder2 = new StringBuilder();
            IBuffer buffer = await Readdata();
            byte[] result = BufferToBytes(buffer);
            for (int i = 0; i < result.Length; i++)
            {
                builder.Append(result[i].ToString("X2") + "\t");
            }
            for (int i = 0; i < result.Length; i++)
            {
                builder2.Append(System.Convert.ToChar(result[i]));
            }
            Txt_Result.Text = builder.ToString();
            Txt_Result2.Text = builder2.ToString();
        }

        private void PortInit()
        {
            switch (Cb_Parity.SelectedIndex)
            {
                case 0:
                    device.Parity = SerialParity.None;
                    break;
                case 1:
                    device.Parity = SerialParity.Odd;
                    break;
                case 2:
                    device.Parity = SerialParity.Even;
                    break;
                case 3:
                    device.Parity = SerialParity.Mark;
                    break;
                case 4:
                    device.Parity = SerialParity.Space;
                    break;
            }
            switch (Cb_Parity.SelectedIndex)
            {
                case 0:
                    device.StopBits = SerialStopBitCount.One;
                    break;
                case 1:
                    device.StopBits = SerialStopBitCount.OnePointFive;
                    break;
                case 2:
                    device.StopBits = SerialStopBitCount.Two;
                    break;
            }
        }

        private void Btn_Clear_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Txt_Result.Text = Txt_Result2.Text = string.Empty;
        }
    }
}
