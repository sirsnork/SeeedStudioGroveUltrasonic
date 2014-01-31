using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using ParallaxSensors;

namespace NetduinoApplication
{
    public class Program
    {
        public static void Main()
        {
            Ping ping = new Ping(Pins.GPIO_PIN_D2, 1000, true);
            ping.RangeEvent += new Ping.RangeTakenDelegate(ping_RangeEvent);

            // Sleep forever!
            Thread.Sleep(Timeout.Infinite);
        
        }

        static void ping_RangeEvent(object sender, PingEventArgs e)
        {
            Debug.Print("Range: " +  e.Distance + " cm");
        }
    }
}