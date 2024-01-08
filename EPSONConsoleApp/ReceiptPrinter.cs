using Microsoft.PointOfService;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
namespace EPSONConsoleApp
{
    public class ReceiptPrinter : INotifyPropertyChanged
    {
        private PosPrinter m_Printer = null;
        private DeviceCollection myDevices = null;
        private Timer timer;

        private enum Alignment { Left, Centre, Right };
        //private Bitmap logo;
        private string statusString;

        public delegate void AsyncLoadPrinterCaller();

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public ReceiptPrinter()
        {
            LoadStatusCheckTicker();
        }

        public void LoadPrinter()
        {
            SetUpPrinter();
            ClaimPrinter();
            //var a = Assembly.GetExecutingAssembly(); // Or another Get method if you need to get it from some other assembly
            //var image = Bitmap.FromStream(a.GetManifestResourceStream("DefaultNameSpace.Assets.logo.bmp"));
            //logo = Beijing_Inn_Order_System.Properties.Resources.logo;
            //Bitmap bmp = new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("MyProject.Resources.myimage.png"));
        }

        private void ClaimPrinter()
        {
            try
            {
                if (m_Printer == null) throw new PosControlException("No Printer", ErrorCode.NoExist);
                m_Printer.Claim(1000);
                m_Printer.DeviceEnabled = true;
            }
            catch (PosControlException)
            {
                Console.WriteLine("Failed to claim printer. Is it connected?");
            }
        }

        private void SetUpPrinter()
        {
            string strLogicalName = "PosPrinter";
            try
            {
                PosExplorer posExplorer = new PosExplorer();

                DeviceInfo deviceInfo = null;
                myDevices = posExplorer.GetDevices(DeviceType.PosPrinter);
                try
                {
                    deviceInfo = posExplorer.GetDevice(DeviceType.PosPrinter, strLogicalName);
                    m_Printer = (PosPrinter)posExplorer.CreateInstance(deviceInfo);
                }
                catch (Exception)
                {
                }

                m_Printer.Open();
            }
            catch (PosControlException)
            {
                Console.WriteLine("Failed to load printer: " + strLogicalName);
            }
        }

        private void LoadStatusCheckTicker()
        {
            timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(UpdatePrinterStatusTick);
            timer.Interval = 5000; // Interval is in milliseconds, so 5000 ms = 5 seconds
            timer.Start();
        }

        private void UpdatePrinterStatusTick(object sender, EventArgs e)
        {
            statusString = "Health: " + Health + "\nEnabled: " + DeviceEnabled + "\nPower: " + PowerState + "\nCover: ";
            if (CoverOpen == true)
            {
                statusString += "Open";
            }
            if (CoverOpen == false)
            {
                statusString += "Closed";
            }
            if (CoverOpen == null)
            {
                statusString += "Unknown";
            }

            if (!DeviceEnabled)
            {
                AsyncLoadPrinter();
            }
            NotifyPropertyChanged("StatusText");
        }

        public void AsyncLoadPrinter()
        {
            if (m_Printer == null) return;
            //AsyncLoadPrinterCaller caller = new AsyncLoadPrinterCaller(LoadPrinter);
            //IAsyncResult result = caller.BeginInvoke(null, null);
            Task loadPrinterTask = new Task(() => LoadPrinter());
            loadPrinterTask.Start();
        }

        public void UnloadPrinter()
        {
            try
            {
                if (m_Printer == null) return;
                m_Printer.Release();
                m_Printer.Close();
            }
            catch (PosControlException)
            {
                Debug.WriteLine("Printer cannot unload, perhaps its turned off? Or not connected?");
            }
        }


        #region Properties
        public string Health
        {
            get
            {
                try
                {
                    if (m_Printer == null) return "Printer not found";
                    return m_Printer.CheckHealth(HealthCheckLevel.Internal);
                }
                catch (PosControlException e)
                {
                    return e.Message;
                }
            }
        }

        public string PowerState
        {
            get
            {
                if (m_Printer == null) return "Unknown";
                switch (m_Printer.PowerState)
                {
                    case Microsoft.PointOfService.PowerState.Off:
                        return "Off";
                    case Microsoft.PointOfService.PowerState.Offline:
                        return "Offline";
                    case Microsoft.PointOfService.PowerState.OffOffline:
                        return "OffOffline";
                    case Microsoft.PointOfService.PowerState.Online:
                        return "Online";
                    case Microsoft.PointOfService.PowerState.Unknown:
                        return "Unknown";
                }
                return null;
            }
        }

        public bool? CoverOpen
        {
            get
            {
                try
                {
                    if (m_Printer == null) throw new PosControlException("No Printer Exists", ErrorCode.NoExist, new Exception());
                    return m_Printer.CoverOpen;
                }
                catch (PosControlException)
                {
                    return null;
                }
            }
        }

        public bool DeviceEnabled
        {
            get
            {
                if (m_Printer == null) return false;
                return m_Printer.DeviceEnabled;
            }
        }

        public DeviceCollection PrinterDevices
        {
            get
            {
                return myDevices;
            }
        }

        public string StatusText
        {
            get
            {
                return statusString;
            }
        }
        #endregion
    }
}