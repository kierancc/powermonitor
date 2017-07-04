using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public void RecordData(double voltage, uint capacity)
        {
            lastVoltage = currentVoltage;
            currentVoltage = voltage;
            lastCapacity = currentCapacity;
            currentCapacity = capacity;
        }

        public BatteryStatusCode QueryStatus()
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

            return currentStatus;
        }
    }
}
