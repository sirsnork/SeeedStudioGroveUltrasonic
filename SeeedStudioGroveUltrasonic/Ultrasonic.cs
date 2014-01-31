using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace ParallaxSensors
{
    /*******************************************************************
     * Reference the following DLLs
     *  - Microsoft.SPOT.Hardware
     *  - Microsoft.SPOT.Native
     *  - SecretLabs.NETMF.Hardware
     *  - SecretLabs.NETMF.Hardware.Netduino
     ******************************************************************/

    /// <summary>
    /// Holds the distance information for the event
    /// </summary>
    public class PingEventArgs
    {
        public PingEventArgs(int distance)
        {
            Distance = distance;
        }

        public int Distance { get; set; }
    }

    /// <summary>
    /// Implements a timer based distance measurement of distance (in cm) using the
    /// parallax Ping sensor
    /// 
    /// Example usage:
    /// public static void Main()
    /// {
    ///     Ping ping = new Ping(Pins.GPIO_PIN_D2, 1000, true);
    ///     ping.RangeEvent += new Ping.RangeTakenDelegate(ping_RangeEvent);
    ///
    ///     // Sleep forever!
    ///     Thread.Sleep(Timeout.Infinite);
    /// }
    ///
    /// static void ping_RangeEvent(object sender, PingEventArgs e)
    /// {
    ///     Debug.Print("Range: " +  e.Distance + " cm");
    /// }
    /// </summary>
    public class Ping
    {
        public delegate void RangeTakenDelegate(object sender, PingEventArgs e);
        public event RangeTakenDelegate RangeEvent;

        private TristatePort _port;
        private ExtendedTimer _timer;
        private int _period;
        private bool _enabled;

        /// <summary>
        /// Constructor: initializes the Ping class
        /// </summary>
        /// <param name="pin">which pin the sensor is connected to</param>
        /// <param name="period">time between pulses in millisec, minimum of 1000</param>
        /// <param name="enabled">if true, start pinging immediately, otherwise wait for enable</param>
        public Ping(Cpu.Pin pin, int period, bool enabled)
        {
            _port = new TristatePort(pin, false, false, ResistorModes.Disabled);

            // Initially set as disabled.
            _timer = new ExtendedTimer(TakeMeasurement, null, Timeout.Infinite, period);

            // Store the current period
            Period = period;

            // Set the enabled state
            Enabled = enabled;
        }

        /// <summary>
        /// Enable or disable the timer that triggers the read.
        /// </summary>
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;

                _timer.Change((_enabled) ? 0 : Timeout.Infinite, Period);
            }
        }

        /// <summary>
        /// Set the period of pings, min is 1000ms
        /// </summary>
        public int Period
        {
            get { return _period; }
            set
            {
                _period = value;
                if (_period < 1000)
                    _period = 1000;

                // Set enabled to the current value to force update
                Enabled = _enabled;
            }
        }

        /// <summary>
        /// Get distance in cm
        /// </summary>
        /// <returns>distance in cm, based on wikipedia for dry air at 20C</returns>
        private int GetDistance()
        {

            // First we need to pulse the port from high to low.
            _port.Active = true; // Put port in write mode
            _port.Write(true);   // Pulse pin
            _port.Write(false);
            _port.Active = false;// Put port in read mode;    

            bool lineState = false;

            // Wait for the line to go high, for start of pulse.
            while (lineState == false)
                lineState = _port.Read();

            long startOfPulseAt = System.DateTime.Now.Ticks;      // Save start ticks.

            // Wait for line to go low.
            while (lineState)
                lineState = _port.Read();

            long endOfPulse = System.DateTime.Now.Ticks;          // Save end ticks. 

            int ticks = (int)(endOfPulse - startOfPulseAt);

            return ticks / 580;
        }

        /// <summary>
        /// Initiates the pulse, and triggers the event
        /// </summary>
        /// <param name="stateInfo"></param>
        private void TakeMeasurement(Object stateInfo)
        {
            int distance = GetDistance();

            if (RangeEvent != null)
            {
                RangeEvent(this, new PingEventArgs(distance));
            }
        }
    }
}