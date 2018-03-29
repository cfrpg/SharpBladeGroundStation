using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SharpBladeGroundStation.DataStructs
{
    public class BatteryData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        int id;
        int function;
        int type;
        int temperature;
        float[] cellVoltage;
        float voltage;
        float current;
        float currentConsumed;
        float energyConsumed;
        float remaining;

        /// <summary>
        /// 意义不明的id
        /// </summary>
        public int ID
        {
            get { return id; }
            set
            {
                id = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ID"));
            }
        }
        /// <summary>
        /// 意义不明的function
        /// </summary>
        public int Function
        {
            get { return function; }
            set
            {
                function = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Function"));
            }
        }
        /// <summary>
        /// 意义不明的type
        /// </summary>
        public int Type
        {
            get { return type; }
            set
            {
                type = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Type"));
            }
        }
        /// <summary>
        /// 温度，degree_C，不可用时为INT16_MAX
        /// </summary>
        public int Temperature
        {
            get { return temperature; }
            set
            {
                temperature = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Temperature"));
            }
        }
        /// <summary>
        /// 单片电压，24s，V
        /// </summary>
        public float[] CellVoltage
        {
            get { return cellVoltage; }
            set
            {
                cellVoltage = value;                
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CellVoltage"));                
            }
        }
        /// <summary>
        /// 电流，A
        /// </summary>
        public float Current
        {
            get { return current; }
            set
            {
                current = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Current"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SimpleBattText"));
            }
        }
        /// <summary>
        /// 消耗的电量，mAh
        /// </summary>
        public float CurrentConsumed
        {
            get { return currentConsumed; }
            set
            {
                currentConsumed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentConsumed"));
            }
        }
        /// <summary>
        /// 消耗的能量，KJ
        /// </summary>
        public float EnergyConsumed
        {
            get { return energyConsumed; }
            set
            {
                energyConsumed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EnergyConsumed"));
            }
        }
        /// <summary>
        /// 剩余电量百分比
        /// </summary>
        public float Remaining
        {
            get { return remaining; }
            set
            {
                remaining = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Remaining"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SimpleBattText"));
            }
        }
        /// <summary>
        /// 简单的电量提示文本
        /// </summary>
        public string SimpleBattText
        {
            get
            {
                string str = "";
                if(remaining<0)
                {
                    str += "--% ";
                }
                else
                {
                    str += remaining.ToString("f0") + "% ";
                }
                if(voltage<0)
                {
                    str += "--V ";
                }
                else
                {
                    str += voltage.ToString("f1") + "V ";
                }
                if(current<0)
                {
                    str += "--A ";
                }
                else
                {
                    str += current.ToString("f1") + "A ";
                }
                return str;
            }
        }
        /// <summary>
        /// 总电压，V
        /// </summary>
        public float Voltage
        {
            get { return voltage; }
            set
            {
                voltage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Voltage"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SimpleBattText"));
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public BatteryData()
        {
            id = 0;
            function = 1;
            type = 1;
            temperature = -1;
            cellVoltage = new float[24];
            for (int i = 0; i < 24; i++)
                cellVoltage[i] = -1;
            voltage = -1;
            current = -1;
            currentConsumed = -1;
            energyConsumed = -1;
            remaining = -1;
        }
    }
}
