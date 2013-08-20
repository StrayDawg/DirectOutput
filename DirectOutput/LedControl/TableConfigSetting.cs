﻿using System;

namespace DirectOutput.LedControl
{
    /// <summary>
    /// A single setting from a LedControl.ini file.
    /// </summary>
    public class TableConfigSetting
    {


        /// <summary>
        /// Defines the control mode for a output. It can be constantly on, off or it can be controlled by a element of a pinball table.
        /// </summary>
        /// <value>
        /// The output control enum value.
        /// </value>
        public OutputControlEnum OutputControl { get; set; }

        /// <summary>
        /// Gets or sets the name of the color of the setting.<br/>
        /// This should only be set for RGB outputs.
        /// </summary>
        /// <value>
        /// The name of the color as specified in the color section of the Ledcontrol.ini file.
        /// </value>
        public string ColorName { get; set; }



        /// <summary>
        /// Gets or sets the color config.
        /// </summary>
        /// <value>
        /// The color config.
        /// </value>
        public ColorConfig ColorConfig { get; set; }

        /// <summary>
        /// Gets or sets the type of the table element controlling a output.
        /// </summary>
        /// <value>
        /// The type of the table element.
        /// </value>
        public TableElementTypeEnum TableElementType { get; set; }
        /// <summary>
        /// Gets or sets the number of the table element controlling a output.
        /// </summary>
        /// <value>
        /// The table element number.
        /// </value>
        public int TableElementNumber { get; set; }



        /// <summary>
        /// Gets the type of the output.<br/>
        /// The value of this property depends on the value of the ColorName property.
        /// </summary>
        /// <value>
        /// The type of the output.
        /// </value>
        public OutputTypeEnum OutputType
        {
            get
            {
                return (!ColorName.IsNullOrWhiteSpace() ? OutputTypeEnum.RGBOutput : OutputTypeEnum.AnalogOutput);
            }
        }

        /// <summary>
        /// Gets or sets the duration in milliseconds.
        /// </summary>
        /// <value>
        /// The duration in milliseconds.
        /// </value>
        public int DurationMs { get; set; }

        private int _Intensity;
        /// <summary>
        /// Gets or sets the intensity.<br/>
        /// If the property <see cref="ColorName"/> is set, this property will always return -1.
        /// </summary>
        /// <value>
        /// The intensity.
        /// </value>
        public int Intensity
        {

            get { return (ColorName.IsNullOrWhiteSpace() ? _Intensity : -1); }
            set { _Intensity = value; }
        }



        /// <summary>
        /// Gets or sets the number blinks.
        /// </summary>
        /// <value>
        /// The number of blinks. -1 means infinite number of blinks.
        /// </value>
        public int Blink { get; set; }

        /// <summary>
        /// Gets or sets the blink interval in milliseconds.
        /// </summary>
        /// <value>
        /// The blink interval in  milliseconds.
        /// </value>
        public int BlinkIntervalMs { get; set; }

        /// <summary>
        /// Parses the setting data. <br />
        /// Possible config settings:<br />
        /// S1<br />
        /// S4 1500       (Solenoid4: 1500ms)<br />
        /// S8 300 I32    (Solenoid8: 300ms Intensity32)<br />
        /// W15 300 2     (Switch15:   2Times within 300ms Period)<br />
        /// ON Red (Red)<br />
        /// S5 Red 10 (Solenoid 5: Red 10milliseconds)<br />
        /// S7 White (Solenoid 7: White)<br />
        /// ON Orange I48 (On Orange, I48 is probably not relevant)<br />
        /// L88 Blink I44 (Lamp88:Blink with insensity 44)<br />
        /// W58 Blink 5 (Switch48: Blink 5 times.<br />
        /// s4 420 24 (flash 24 times within 420 milliseconds) <br />
        /// First char(s):<br />
        /// L??=Lamp, S??=Solenoid, W??=Switch, B=Blink (very likely), 0=off, 1=on, on=on, off=off
        /// </summary>
        /// <param name="SettingData">The setting data.</param>
        /// <exception cref="System.Exception">
        /// No data to parse.
        /// or
        /// Cant parse the part {0} of the ledcontrol table config setting {1}..
        /// </exception>
        public void ParseSettingData(string SettingData)
        {
            string[] Parts = SettingData.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (Parts.Length == 0)
            {
                Log.Warning("No data to parse.");

                throw new Exception("No data to parse.");

            }
            //Get output state and table element (if applicable)
            bool ParseOK = true;
            switch (Parts[0].ToUpper())
            {
                case "ON":
                case "1":
                    OutputControl = OutputControlEnum.FixedOn;
                    break;
                case "OFF":
                case "0":
                    OutputControl = OutputControlEnum.FixedOff;
                    break;
                case "B":
                    OutputControl = OutputControlEnum.FixedOn;
                    Blink = -1;
                    break;
                default:
                    if (Parts[0].Length > 1 && Parts[0].Substring(1).IsInteger())
                    {
                        OutputControl = OutputControlEnum.Controlled;
                        Char C = Parts[0].ToUpper().ToCharArray()[0];
                        if (Enum.IsDefined(typeof(TableElementTypeEnum), (int)C))
                        {
                            TableElementType = (TableElementTypeEnum)C;
                        }
                        else
                        {
                            ParseOK = false;
                        }

                        TableElementNumber = Parts[0].Substring(1).ToInteger();
                    }
                    else
                    {
                        ParseOK = false;
                    }

                    break;
            }
            if (!ParseOK)
            {
                Log.Warning("Cant parse the part {0} of the ledcontrol table config setting {1}.".Build(Parts[0], SettingData));

                throw new Exception("Cant parse the part {0} of the ledcontrol table config setting {1}.".Build(Parts[0], SettingData));

            }

            if (Parts.Length > 1)
            {
                if (Parts[1].ToUpper() == "BLINK")
                {
                    //Blink command
                    Blink = -1;
                }
                else if (Parts[1].IsInteger())
                {
                    //Its a duration
                    DurationMs = Parts[1].ToInteger();

                }
                else if (Parts[1].ToUpper().Substring(0, 1) == "I" && Parts[1].Substring(1).IsInteger())
                {
                    //Intensity setting
                    Intensity = Parts[1].Substring(1).ToInteger().Limit(0, 48);
                }

                else
                {
                    //It should be a color
                    ColorName = Parts[1];
                }

            };

            if (Parts.Length > 2)
            {
                if (Parts[1].ToUpper() == "BLINK")
                {
                    //Blink command
                    Blink = -1;
                }
                else if (Parts[2].IsInteger())
                {

                    //Indicates number of blinks or duration
                    if (OutputType == OutputTypeEnum.RGBOutput)
                    {
                        DurationMs = Parts[2].ToInteger();
                    }
                    else
                    {
                        Blink = Parts[2].ToInteger();
                        if (DurationMs > 0)
                        {
                            BlinkIntervalMs = (DurationMs / Blink / 2).Limit(1, int.MaxValue);
                            DurationMs = 0;
                        }
                    }


                }
                else if (Parts[2].ToUpper().Substring(0, 1) == "I" && Parts[2].Substring(1).IsInteger())
                {
                    //Intensity setting
                    Intensity = Parts[2].Substring(1).ToInteger().Limit(0, 48);
                }
                else
                {
                    Log.Warning("Cant parse the part {0} of the ledcontrol table config setting {1}.".Build(Parts[2], SettingData));

                    throw new Exception("Cant parse the part {0} of the ledcontrol table config setting {1}.".Build(Parts[2], SettingData));

                }
            }
        }




        /// <summary>
        /// Initializes a new instance of the <see cref="TableConfigSetting"/> class.
        /// Parses the setting data. <br/>
        /// Possible config settings:<br/>
        /// S1<br/>
        /// S4 1500       (Solenoid4: 1500ms)<br/>
        /// S8 300 I32    (Solenoid8: 300ms Intensity32)<br/>
        /// W15 300 2     (Switch15:   2Times 300ms Period)<br/>
        /// ON Red (Red)<br/>
        /// S5 Red 10 (Solenoid 5: Red Blink 10 times)<br/>
        /// S7 White (Solenoid 7: White)<br/>
        /// ON Orange I48 (On Orange, I48 is probably not relevant)<br/>
        /// L88 Blink I44 (Lamp88:Blink with insensity 44)<br/>
        /// W58 Blink 5 (Switch48: Blink 5 times.<br/>
        /// First char(s):<br/>
        /// L??=Lamp, S??=Solenoid, W??=Switch, B=Blink (very likely), 0=off, 1=on, on=on, off=off
        /// </summary>
        /// <param name="SettingData">The setting data.</param>
        /// <exception cref="System.Exception">
        /// No data to parse.<br/>
        /// or <br/>
        /// Cant parse the part {0} of the ledcontrol table config setting {1}.
        /// </exception>
        public TableConfigSetting(string SettingData)
            : this()
        {
            ParseSettingData(SettingData);
        }



        public TableConfigSetting()
        {
            this.Intensity = 48;
            this.Blink = 0;
            this.DurationMs = -1;

        }







    }
}
