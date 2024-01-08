using Microsoft.PointOfService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPSONConsoleApp
{
    class v1
    {
        private static PosPrinter _printer;
         void Mainb(string[] args)
        {

            InitializePosPrinter();


            Console.ReadLine();

            // Clean up resources when the application exits
            CleanupPrinter();
        }

        private static void InitializePosPrinter()
        {
            // Create a timer that runs the specified function every second
            //Timer timer = new Timer(OnTimedEvent, null, 0, 1000);

            try
            {
                PosExplorer posExplorer = new PosExplorer();

                DeviceInfo deviceInfo = posExplorer.GetDevice(DeviceType.PosPrinter, "PosPrinter");

                if (deviceInfo == null)
                {
                    Console.WriteLine("No printer found ,May be Device Power is Off");
                }

                _printer = (PosPrinter)posExplorer.CreateInstance(deviceInfo);

                //_printer.Open();

                //try
                //{
                //    _printer.Open();
                //    if (!_printer.Claimed)
                //    {
                //        _printer.Claim(0);
                //        _printer.DeviceEnabled = true;
                //    }

                //    bool printerReady = true;
                //    if (_printer.CoverOpen)
                //    {
                //        Console.WriteLine("Printer cover is open.");
                //        printerReady = false;
                //    }
                //    if (_printer.PowerState == PowerState.OffOffline)
                //    {
                //        Console.WriteLine("Printer is has no power or is offline");
                //        printerReady = false;
                //    }
                //    if (_printer.State != ControlState.Idle)
                //    {
                //        Console.WriteLine("Printer is busy");
                //        printerReady = false;
                //    }

                //    if (printerReady)
                //    {
                //        string text = "test text";
                //        _printer.PrintNormal(PrinterStation.Receipt, text);
                //    }

                //}
                //catch (Exception ae)
                //{
                //    Console.WriteLine("An error occured: " + ae.ToString());
                //}
                //// Subscribe to various events
                //_printer.StatusUpdateEvent += Printer_StatusUpdateEvent;
                //_printer.ErrorEvent += Printer_ErrorEvent;
                //_printer.OutputCompleteEvent += Printer_OutputCompleteEvent;

                //// Claim the printer for exclusive use
                //_printer.Claim(1000);

                //// Enable the device
                //_printer.DeviceEnabled = true;

            }
            catch (PosControlException ex)
            {
                if (ex.ErrorCode == ErrorCode.Illegal && ex.ErrorCodeExtended == 1004)
                {
                    Console.WriteLine("Unable to print receipt.\n" + ex.Message);
                }
                // Clear the buffered data since the buffer retains print data when an error occurs during printing.
                //_printer.ClearOutput();
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
                    Console.WriteLine($"Printer power status: {e.Status}");
                    break;
                // Receipt paper is OK.
                case PosPrinter.StatusReceiptPaperOK:
                case PosPrinter.StatusReceiptNearEmpty:
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


        private static void PrinterPowerStatus()
        {

            //// Power reporting
            //try
            //{
            //    if (_printer. != PowerReporting.None)
            //    {
            //        _printer.PowerNotify = PowerNotification.Enabled;
            //    }
            //}
            //catch (PosControlException)
            //{
            //}
        }

    }
}
