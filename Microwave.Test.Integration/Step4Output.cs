using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Microwave.Classes.Boundary;
using Microwave.Classes.Controllers;
using Microwave.Classes.Interfaces;
using NSubstitute;
using NUnit.Framework;
using Timer = Microwave.Classes.Boundary.Timer;

namespace Microwave.Test.Integration
{
    public class Step4Output
    {
        private Button startCancelButton;
        private Button powerButton;
        private Button timeButton;
        private Door door;
        private UserInterface ui;
        private Display display;
        private Light light;
        private CookController cookController;
        private PowerTube powerTube;
        private Timer timer;
        private Output output;
        private StringWriter str;

        [SetUp]
        public void Setup()
        {
            //Create Substitute
            output = new Output();
            //Create instances:
            startCancelButton = new Button();
            powerButton = new Button();
            timeButton = new Button();
            door = new Door();
            powerTube = new PowerTube(output);
            timer = new Timer();
            display = new Display(output);
            light = new Light(output);
            //Assemble instances to cookController
            cookController = new CookController(timer, display, powerTube);
            //Assemble instances to UI
            ui = new UserInterface(powerButton, timeButton, startCancelButton, door, display, light, cookController);
            cookController.UI = ui;

            str = new StringWriter();
            Console.SetOut(str);

        }
        private void InitiateOven(int timeS = 10)
        {
            door.Open();
            door.Close();
            for (int i = 0; i < 10; i++)
            {
                powerButton.Press();
            }
            for (int i = 0; i < timeS; i++)
            {
                timeButton.Press();
            }

            startCancelButton.Press();
        }
        //UC step2: The light goes on inside the oven
        [Test]
        public void OpenDoor_OvenIsSetUp_LightOnIsPrinted()
        {
            door.Open();
            Assert.That(str.ToString().Contains("Light is turned on"));
        }
        //UC step 5.The light goes off inside the oven
        [Test]
        public void CloseDoor_DoorWasOpen_LightOffIsPrinted()
        {
            door.Open();
            door.Close();
            Assert.That(str.ToString().Contains("Light is turned off"));
        }

        //UC step 6:
        [TestCase(1, 50)]
        [TestCase(2, 100)]
        [TestCase(13, 650)]
        [TestCase(14, 700)]
        [TestCase(15, 50)]
        [TestCase(28, 700)]
        [TestCase(30, 100)]
        [TestCase(772, 100)]

        public void PressPower_DoorWasOpenedAndClosed_CorrectWattageIsPrinted(int presses, int expectedWattage)
        {
            door.Open();
            door.Close();
            for (int i = 0; i < presses; i++)
            {
                powerButton.Press();
            }

            Assert.That(str.ToString().Contains($"Display shows: {expectedWattage} W"));

        }
        //UC step 7:
        [TestCase(1)]
        [TestCase(59)]
        [TestCase(60)]
        [TestCase(601)]

        public void PressTime_PowerIsSet_CorrectTimeIsPrinted(int presses)
        {
            door.Open();
            door.Close();
            for (int i = 0; i < 10; i++)
            {
                powerButton.Press();
            }
            for (int i = 0; i < presses; i++)
            {
                timeButton.Press();
            }

            Assert.That(str.ToString().Contains($"Display shows: {presses:D2}:00"));
        }
        //UC step 9:
        [Test]
        public void PressStartCancel_MicrowaveIsSetUp_LightIsCalled()
        {
            InitiateOven();

            var count=Regex.Matches(str.ToString(), "Light is turned on").Count;
            Assert.That(count,Is.EqualTo(2));

        }
        //UC step 10:
        [TestCase(1, 50)]
        [TestCase(2, 100)]
        [TestCase(13, 650)]
        [TestCase(14, 700)]
        [TestCase(15, 50)]
        [TestCase(28, 700)]
        [TestCase(30, 100)]
        [TestCase(772, 100)]

        public void OvenStart_PowerIsSet_PowerTubePowerIsPrinted(int presses, int expectedWattage)
        {
            door.Open();
            door.Close();
            for (int i = 0; i < presses; i++)
            {
                powerButton.Press();
            }
            for (int i = 0; i < presses; i++)
            {
                timeButton.Press();
            }
            startCancelButton.Press();
            Assert.That(str.ToString().Contains($"PowerTube works with {expectedWattage}"));
        }

        //UC step 11.The display shows and updates the remaing time every second as minutes:seconds.
        [TestCase(1, 5, 0, 55)]
        [TestCase(1, 10, 0, 50)]
        //[TestCase(10, 65, 8, 55)]
        public void PressStartAndWait_OvenIsSetUp_DisplayShowsCorrectTime(int cookTimeM, int waitTimeS, int expectedTimeM, int expectedTimeS)
        {
            InitiateOven(cookTimeM);
            Thread.Sleep(waitTimeS * 1000+100);
            Assert.That(str.ToString().ToLower().Contains($"display shows: {expectedTimeM:D2}:{expectedTimeS:D2}"));
        }

        //These steps require a lot of waiting. To save time, they are combined into one test.

        //UC step 12.When the time has expired, the power tube is turned off
        //UC step 13.The light inside the oven goes off
        //UC step 14.The display is blanked
        [Test]
        //[TestCase(10, 65, 8, 55)]
        public void CookingFinished_OvenIsSetUp_OutputRecievesStrings()
        {
            InitiateOven(1);
            Thread.Sleep(60 * 1000 + 50);
            Assert.Multiple(() =>
            {
                Assert.That(str.ToString().ToLower().Contains($"powertube turned off"));
                Assert.That(str.ToString().ToLower().Contains($"display cleared"));

                var count = Regex.Matches(str.ToString(), "Light is turned off").Count;
                Assert.That(count, Is.EqualTo(2));


            });

        }
        //Extention 1: The user presses the Start-Cancel button during setup]
        [Test]
        public void PressCancel_OvenIsNotSetUp_DisplayCleared()
        {
            door.Open();
            door.Close();
            powerButton.Press();
            startCancelButton.Press();
            Assert.That(str.ToString().ToLower().Contains($"display cleared"));

        }
        [Test]
        public void PressCancel_OvenIsNotSetUp_ValuesReset()
        {
            door.Open();
            door.Close();
            powerButton.Press();
            powerButton.Press();

            startCancelButton.Press();
            
            powerButton.Press();
            var count = Regex.Matches(str.ToString(), "Display shows: 50 W").Count;
            Assert.That(count, Is.EqualTo(2));
        }

        //Extension 2:
        [Test]
        public void OpenDoor_DuringPowerSettings_DisplayCleared()
        {
            door.Open();
            door.Close();
            powerButton.Press();
            door.Open();
            var count = Regex.Matches(str.ToString().ToLower(), "light is turned on").Count;
            Assert.That(count, Is.EqualTo(2));

        }
        [Test]
        public void OpenDoor_DuringPowerSettings_ValuesReset()
        {
            door.Open();
            door.Close();
            powerButton.Press();
            powerButton.Press();
            door.Open();
            door.Close();
            powerButton.Press();
            var count = Regex.Matches(str.ToString(), "Display shows: 50 W").Count;
            Assert.That(count, Is.EqualTo(2));
        }
        //[Extension 3: The user presses the Start-Cancel button during cooking]
        [Test]
        public void PressCancel_DuringCooking_StringsArePrinted()
        {
            InitiateOven(1);
            Thread.Sleep( 50);
            startCancelButton.Press();
            Assert.Multiple(() =>
            {
                Assert.That(str.ToString().ToLower().Contains($"powertube turned off"));
                Assert.That(str.ToString().ToLower().Contains($"display cleared"));

                var count = Regex.Matches(str.ToString(), "Light is turned off").Count;
                Assert.That(count, Is.EqualTo(2));


            });

        }
        [Test]
        public void PressCancel_DuringCooking_ValuesReset()
        {
            InitiateOven();
            startCancelButton.Press();
            powerButton.Press();
            var count = Regex.Matches(str.ToString(), "Display shows: 50 W").Count;
            Assert.That(count, Is.EqualTo(2));
        }
        //[Extension 4: The user opens the Door during cooking]
        [Test]
        public void DoorOpen_DuringCooking_DisplayClearIsPrinted()
        {
            InitiateOven(1);
            Thread.Sleep(50);
            door.Open();
            Assert.Multiple(() =>
            {
                Assert.That(str.ToString().ToLower().Contains($"powertube turned off"));
                Assert.That(str.ToString().ToLower().Contains($"display cleared"));
            });

        }
        [Test]
        public void DoorOpen_DuringCooking_ValuesReset()
        {
            InitiateOven();
            door.Open();
            door.Close();
            powerButton.Press();
            var count = Regex.Matches(str.ToString(), "Display shows: 50 W").Count;
            Assert.That(count, Is.EqualTo(2));
        }

    }
}