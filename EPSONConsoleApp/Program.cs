using Microsoft.PointOfService;
using System;
using System.Reflection;


namespace EPSONConsoleApp
{
    class Program
    {
        private static PosPrinter _printer;
        static void Main(string[] args)
        {

            InitializePosPrinter();

            Console.ReadLine();

            // Clean up resources when the application exits
            CleanupPrinter();
        }

        private static void InitializePosPrinter()
        {
            try
            {
                PosExplorer posExplorer = new PosExplorer();

                DeviceInfo deviceInfo = posExplorer.GetDevice(DeviceType.PosPrinter, "PosPrinter");

                if (deviceInfo == null)
                {
                    Console.WriteLine("No printer found ,May be Device Power is Off");
                }

                _printer = (PosPrinter)posExplorer.CreateInstance(deviceInfo);

                _printer.Open();

                // Enable PowerNotify
                _printer.PowerNotify = PowerNotification.Enabled;
                
                // Subscribe to various events
                _printer.StatusUpdateEvent += Printer_StatusUpdateEvent;
                _printer.ErrorEvent += Printer_ErrorEvent;
                _printer.OutputCompleteEvent += Printer_OutputCompleteEvent;

                // Claim the printer for exclusive use
                _printer.Claim(1000);

                // Enable the device
                _printer.DeviceEnabled = true;
             }
            catch (PosControlException ex)
            {
                Console.WriteLine(ex.Message);      
                
                // Clear the buffered data since the buffer retains print data when an error occurs during printing.
                _printer.ClearOutput();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing POS printer: {ex.Message}");
            }
        }

        private static void CleanupPrinter()
        {
            if (_printer != null)
            {
                // Disable the device and release resources
                _printer.DeviceEnabled = false;
                _printer.Release();
                _printer.Close();

                // Unsubscribe from events
                _printer.StatusUpdateEvent -= Printer_StatusUpdateEvent;
                _printer.ErrorEvent -= Printer_ErrorEvent;
                _printer.OutputCompleteEvent -= Printer_OutputCompleteEvent;
            }
        }

        private static void Printer_StatusUpdateEvent(object sender, StatusUpdateEventArgs e)
        {
           // Log status update to the console
            switch (e.Status)
            {
                // Printer cover is open.
                case PosPrinter.StatusCoverOpen:
                    Console.WriteLine("Printer cover is open.");
                    break;

                // No receipt paper.
                case PosPrinter.StatusReceiptEmpty:
                    Console.WriteLine("No receipt paper.");
                    break;
                // Printer cover is closed.
                case PosPrinter.StatusCoverOK:
                    Console.WriteLine("Printer cover is closed.");
                    break;

                case PosCommon.StatusPowerOnline:
                    Console.WriteLine($"Printer power is online");
                    break;

                case PosCommon.StatusPowerOff:
                    Console.WriteLine($"Printer power is offline");
                    break;
                // Receipt paper is OK.
                case PosPrinter.StatusReceiptPaperOK: 
                    Console.WriteLine("Receipt paper is OK.");
                    break;
            }
        }

        private static void Printer_ErrorEvent(object sender, DeviceErrorEventArgs e)
        {
            // Log error to the console
            Console.WriteLine($"Printer error occurred: {e.ErrorResponse}");
        }

        private static void Printer_OutputCompleteEvent(object sender, OutputCompleteEventArgs e)
        {
            // Log output completion to the console
            Console.WriteLine("Print job completed successfully.");
        }
  
    }
}
