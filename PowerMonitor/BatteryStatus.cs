using System;
using System.Configuration;

namespace PowerMonitor
{
    public class BatteryStatus
    {
        private static BatteryStatus singletonBH = null;
        private double lastVoltage, currentVoltage;
        private uint lastCapacity, currentCapacity;
        private int dataPointCount;
        private BatteryStatusCode currentStatus;
        private double voltageThreshold;
        private bool charging;

        public bool Charging
        {
            get { return charging; }
        }

        public enum BatteryStatusCode
        {
            OK = 0,
            Discharging = 1,
            LowVoltage = 2
        }

        private BatteryStatus()
        {
            dataPointCount = Int32.Parse(ConfigurationManager.AppSettings["dataPointCount"]);
            lastVoltage = 0;
            currentVoltage = 0;
            lastCapacity = 0;
            currentCapacity = 0;
            currentStatus = BatteryStatusCode.OK;
            voltageThreshold = Double.Parse(ConfigurationManager.AppSettings["voltageThreshold"]);
            charging = true;
        }

        public static BatteryStatus Instance
        {
            get
            {
                if (singletonBH == null)
                {
                    singletonBH = new BatteryStatus();
                }

                return singletonBH;
            }
        }

        public void RecordData(double voltage, uint capacity, string charging)
        {
            lastVoltage = currentVoltage;
            currentVoltage = voltage;
            lastCapacity = currentCapacity;
            currentCapacity = capacity;
            
            if (charging.Contains("Charging"))
            {
                this.charging = true;
            }
            else
            {
                this.charging = false;
            }
        }

        public BatteryStatusCode QueryStatus()
        {
            if (this.charging)
            {
                if (currentVoltage < voltageThreshold)
                {
                    currentStatus = BatteryStatusCode.LowVoltage;
                }
                else if (currentCapacity < lastCapacity)
                {
                    currentStatus = BatteryStatusCode.Discharging;
                }
                else
                {
                    currentStatus = BatteryStatusCode.OK;
                }
            }
            else
            {
                currentStatus = BatteryStatusCode.OK;
            }

            return currentStatus;
        }
    }
}
