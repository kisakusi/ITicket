using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;
using Newtonsoft.Json;

namespace TicketQRCodeReader
{
    public partial class FrmCodeReader : Form
    {
        public FrmCodeReader()
        {
            InitializeComponent();
        }
        FilterInfoCollection filterInfoCollection;
        VideoCaptureDevice videoCaptureDevice;
        private void FrmCodeReader_Load(object sender, EventArgs e)
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (filterInfoCollection.Count != 0)
            {
                videoCaptureDevice = new VideoCaptureDevice(filterInfoCollection[0].MonikerString);
                videoCaptureDevice.Start();
                timer1.Start();
                videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;
            }         
        }
        int flag = 0;
        private void btnChange_Click(object sender, EventArgs e)
        {
            flag = flag==0 ? 1 : 0;
            if (filterInfoCollection.Count > 1)
            {
                videoCaptureDevice?.Stop();
                videoCaptureDevice = new VideoCaptureDevice(filterInfoCollection[flag].MonikerString);
                videoCaptureDevice.Start();
                videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;
            }              
        }
        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            pbCamera.Image = (Bitmap)eventArgs.Frame.Clone();
        }
        private void FrmCodeReader_FormClosing(object sender, FormClosingEventArgs e)
        {
            videoCaptureDevice?.Stop();
        }

        async private void timer1_Tick(object sender, EventArgs e)
        {
            if (pbCamera.Image != null)
            {
                BarcodeReader reader = new BarcodeReader();
                Result result = reader.Decode((Bitmap)pbCamera.Image);
                if (result != null)
                {
                    //呼叫遠端api
                    HttpClient client = new HttpClient();
                    HttpResponseMessage response;
                    try
                    {
                        response = await client.GetAsync("https://iticket128.azurewebsites.net/api/QRCode/get/?qrCodeContent=" + result.ToString());
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                    //response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    ActivityData data = JsonConvert.DeserializeObject<ActivityData>(responseBody);
                    if (data != null)
                    {
                        txtQRCode.Text = result.ToString();
                        lblActivityName.Text = data.ActivityName;
                        lblName.Text = data.MemberName;
                        lblTicketCategory.Text = data.TicketCategoryName;
                        lblTime.Text = data.TicketTime;
                    }
                    else
                    {
                        txtQRCode.Text = result.ToString();
                        lblActivityName.Text = "";
                        lblName.Text = "";
                        lblTicketCategory.Text = "";
                        lblTime.Text = "";
                    }
                }
            }
        }      
    }
    public class ActivityData
    {
        public string ActivityName { get; set; }
        public string TicketCategoryName { get; set; }
        public string TicketTime { get; set; }
        public string MemberName { get; set; }
    }
}
