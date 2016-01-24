using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlinkStick.Hid;
using System.Drawing;

namespace BStick
{
    class Operations
    {
        public static string ColorToHexString(Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }
    }
    public class Devices
    {
        private static List<AbstractBlinkstickHid> FoundHids = new List<BlinkStick.Hid.AbstractBlinkstickHid>();

        public List<AbstractBlinkstickHid> foundHids
        {
            get
            {
                return FoundHids;
            }
            set { GetHids(); }
        }

        public void GetHids()
        {
            FoundHids = BlinkStick.Hid.BlinkstickDeviceFinder.FindDevices().ToList();
        }
    }
    public class Blinker
    {
        internal int BlinkCount = 1;
        private string BlinkColorString;
        private Color BlinkColor;
        private int BlinkDuration = new int();
        private int BlinkAnimationTime = new int();

        public Blinker()
        {
            AnimationSpeed = 10;
            pauseLength = 50;
            BlinkColor = System.Drawing.Color.White;
            BlinkCount = 1;

        }
        public int pauseLength; //If pause is possible between operations, sleep this long        
        public AbstractBlinkstickHid hid
        {
            get;
            set;
        }

        public Int32 PhaseSpeed
        {
            get;
            set;
        }
        /** Lower is Faster */
        public Int32 AnimationSpeed
        {
            get;
            set;
        }

        public int Duration
        {
            get
            {
                if (BlinkDuration == 0)
                { BlinkDuration = 10; }

                return BlinkDuration;
            }
            set { BlinkDuration = 10; }
        }
        public string GetBlinkColorString()
        {
            return BlinkColorString;
        }
        public Color GetBlinkColor()
        {
            return BlinkColor;
        }
        public void SetBlinkColor(Color color)
        {
            BlinkColorString = Operations.ColorToHexString(color);
            BlinkColor = color;
        }
        public void SetBlinkDuration(int seconds)
        {
            BlinkDuration = seconds;
            BlinkAnimationTime = AnimationSpeed;
        }
        public void SetBlinkCount(int count)
        {
            BlinkCount = count;
        }

        public void TurnOn()
        {
            BlinkFire blinkFire = new BlinkFire();
            blinkFire.TurnOn(this, this.hid);
        }
        public void TurnOff()
        {
            BlinkFire blinkFire = new BlinkFire();
            blinkFire.TurnOff(this, this.hid);
        }
    }

    public class BlinkFire
    {
        public void TurnOn(Blinker blinker, AbstractBlinkstickHid hid)
        {
            string color = Operations.ColorToHexString(blinker.GetBlinkColor());
            changeColor(hid, color);
        }

        public void TurnOff(Blinker blinker, AbstractBlinkstickHid hid)
        {
            string color = "#000000";
            changeColor(hid, color);
        }

        public void SirenBlink(Blinker blinker, AbstractBlinkstickHid hid)
        {
            List<Color> lightColors = new List<Color>()
            {
                System.Drawing.Color.Blue,
                System.Drawing.Color.Red,
                System.Drawing.Color.White
            };
            int sirenCount = blinker.BlinkCount;

            for (int i = 0; i < sirenCount; i++)
            {
                int colorChoice = new Random().Next(0,lightColors.Count);
                blinker.SetBlinkColor(lightColors[colorChoice]);
                int phaseSpeed = new Random().Next(20,100);
                blinker.PhaseSpeed = phaseSpeed;
                blinker.BlinkCount = 1;
                blinker.pauseLength = new Random().Next(0, 10);

                PulseStick(blinker, hid);
            }
            blinker.SetBlinkCount(sirenCount); //return the blink count for possible second runs
        }

        /** <summary>A uniform pulse, modify pulse speed with animation speed and phaseSpeed</summary> */
        public void PulseStick(Blinker blinker, AbstractBlinkstickHid hid)
        {
            decimal redModifier;
            decimal greenModifier;
            decimal blueModifier;
            int phaseSpeed = blinker.PhaseSpeed;

            BuildModifiers(blinker, out redModifier, out greenModifier, out blueModifier);

            for (int loopTimes = 0; loopTimes < (blinker.BlinkCount); loopTimes++)
            {
                int sleepTime = blinker.AnimationSpeed;
                for (int i = 0; i < 255; i = i + phaseSpeed)
                {
                    StateChange(hid, redModifier, greenModifier, blueModifier, i, sleepTime);
                }
                for (int i = 255; i > 0; i = i - phaseSpeed)
                {
                    StateChange(hid, redModifier, greenModifier, blueModifier, i, sleepTime);
                }
                changeColor(hid, "#000000");
                System.Threading.Thread.Sleep(blinker.pauseLength);
            }
        }
        private static void BuildModifiers(Blinker blinker, out decimal redModifier, out decimal greenModifier, out decimal blueModifier)
        {
            Decimal.TryParse(Convert.ToString(blinker.GetBlinkColor().R), out redModifier);
            redModifier = redModifier / 255;
            Decimal.TryParse(Convert.ToString(blinker.GetBlinkColor().G), out greenModifier);
            greenModifier = greenModifier / 255;
            Decimal.TryParse(Convert.ToString(blinker.GetBlinkColor().B), out blueModifier);
            blueModifier = blueModifier / 255;
        }
        private void StateChange(AbstractBlinkstickHid hid, decimal redModifier, decimal greenModifier, decimal blueModifier, int i, int sleepTimer)
        {
            int newRed = Convert.ToInt32(Math.Round(i * redModifier));
            int newGreen = Convert.ToInt32(Math.Round(i * greenModifier));
            int newBlue = Convert.ToInt32(Math.Round(i * blueModifier));
            System.Threading.Thread.Sleep(sleepTimer);
            string newColor = Operations.ColorToHexString(System.Drawing.Color.FromArgb(0, newRed, newGreen, newBlue));
            changeColor(hid, newColor);
        }
        private void changeColor(AbstractBlinkstickHid hid, string hexColor)
        {
            hid.OpenDevice();
            hid.SetLedColor(hexColor);
            hid.CloseDevice();
        }
    }
}
