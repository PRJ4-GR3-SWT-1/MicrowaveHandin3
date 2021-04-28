using System.Threading;
using Microwave.Classes.Boundary;
using Microwave.Classes.Controllers;
using Microwave.Classes.Interfaces;
using NSubstitute;
using NUnit.Framework;
using Timer = Microwave.Classes.Boundary.Timer;

namespace Microwave.Test.Integration
{
    public class Step3DisplayPowerTubeTimer
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
        private IOutput output;

        [SetUp]
        public void Setup()
        {
            //Create Substitute
            output = Substitute.For<IOutput>();
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

        }
        private void InitiateOven()
        {
            door.Open();
            door.Close();
            for (int i = 0; i < 10; i++)
            {
                powerButton.Press();
            }
            for (int i = 0; i < 10; i++)
            {
                timeButton.Press();
            }
            startCancelButton.Press();
        }
        //UC step 10 "The powertube starts working at the desired powerlevel":
        [Test]
        public void PressStart_OvenIsSetUp_PowerTubeStarts()
        {
            InitiateOven();
            output.Received(1).OutputLine(Arg.Is<string>(str => str.ToLower().Contains($"powertube works with")));
        }
        [TestCase(1, 50)]
        [TestCase(2, 100)]
        [TestCase(13, 650)]
        [TestCase(14, 700)]
        [TestCase(15, 50)]
        [TestCase(28, 700)]
        [TestCase(30, 100)]
        [TestCase(772, 100)]
        public void PressStart_OvenIsSetUp_PowerTubeStartsCorrect(int presses,int expectedWattage)
        {
            door.Open();
            door.Close();
            for (int i = 0; i < presses; i++)
            {
                powerButton.Press();
            }
            for (int i = 0; i < 10; i++)
            {
                timeButton.Press();
            }
            startCancelButton.Press();
            output.Received(1).OutputLine(Arg.Is<string>(str => str.ToLower().Contains($"powertube works with {expectedWattage}")));
        }
        //UC step 11. The display shows and updates the remaing time every second as minutes:seconds.

        [Test]
        public void PressStart_OvenIsSetUp_DisplayIsUpdated()
        {
            InitiateOven();
            Thread.Sleep(1100);
            output.Received(1).OutputLine(Arg.Is<string>(str => str.ToLower().Contains($"display shows: 09:59")));
        }
        [TestCase(1,5,0,55)]
        [TestCase(1, 10, 0, 50)]
        //[TestCase(10, 65, 8, 55)]
        public void PressStartAndWait_OvenIsSetUp_DisplayShowsCorrectTime(int cookTimeM, int waitTimeS,int expectedTimeM, int expectedTimeS)
        {
            door.Open();
            door.Close();
            for (int i = 0; i < 5; i++)
            {
                powerButton.Press();
            }
            for (int i = 0; i < cookTimeM; i++)
            {
                timeButton.Press();
            }
            startCancelButton.Press();
            Thread.Sleep(waitTimeS*1000+50);
            output.Received(1).OutputLine(Arg.Is<string>(str => str.ToLower().Contains($"display shows: {expectedTimeM:D2}:{expectedTimeS:D2}")));
        }
        //12.When the time has expired, the power tube is turned off

        //13.The light inside the oven goes off

        //14.The display is blanked
    }
}