using System.Threading;
using Microwave.Classes.Boundary;
using Microwave.Classes.Controllers;
using Microwave.Classes.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Microwave.Test.Integration
{
    public class Step1ButtonsDoorUI
    {
        private Button startCancelButton;
        private Button powerButton;
        private Button timeButton;
        private Door door;
        private UserInterface ui;
        private IDisplay display;
        private ILight light;
        private ICookController cookController;
        [SetUp]
        public void Setup()
        {
            //Create instances:
            startCancelButton = new Button();
            powerButton = new Button();
            timeButton = new Button();
            door = new Door();
            //Create Substitutes
            display=Substitute.For<IDisplay>();
            light=Substitute.For<ILight>();
            cookController=Substitute.For<ICookController>();
            //Assemble instances to UI
            ui = new UserInterface(powerButton, timeButton, startCancelButton, door, display, light, cookController);
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
        //UC step 2:
        [Test]
        public void OpenDoor__LightIsOn()
        {
            door.Open();
            light.Received(1).TurnOn();
        }
        //UC step 5:

        [Test]
        public void CloseDoor_DoorWasOpen_LightIsOff()
        {
            door.Open();
            door.Close();
            light.Received(1).TurnOff();
        }

        //UC step 6:
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(13)]
        [TestCase(14)]
        [TestCase(15)]

        public void PressPower_DoorWasOpenedAndClosed_DisplayIsCalled(int presses)
        {
            door.Open();
            door.Close();
            for (int i = 0; i < presses; i++)
            {
                powerButton.Press();
            }
            
            display.Received(presses).ShowPower(Arg.Any<int>());
        }

        //UC step 6:
        [TestCase(1,50)]
        [TestCase(2,100)]
        [TestCase(13,650)]
        [TestCase(14,700)]
        [TestCase(15,50)]
        [TestCase(28, 700)]
        [TestCase(30, 100)]
        [TestCase(772, 100)]

        public void PressPower_DoorWasOpenedAndClosed_CorrectWattageIsShown(int presses,int expectedWattage)
        {
            door.Open();
            door.Close();
            for (int i = 0; i < presses; i++)
            {
                powerButton.Press();
            }

            display.Received((presses - 1)/14+1).ShowPower(expectedWattage);
        }

        //UC step 7:
        [TestCase(1)]
        [TestCase(59)]
        [TestCase(60)]
        [TestCase(601)]

        public void PressTime_PowerIsSet_CorrectTimeIsShown(int presses)
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

            display.Received(1).ShowTime(presses,0);
        }

        //UC step 9:
        [Test]

        public void PressStartCancel_MicrowaveIsSetUp_LightIsCalled()
        {
            InitiateOven();

            light.Received(2).TurnOn();
        }
        //UC step 10:
        [Test]
        public void PressStartCancel_MicrowaveIsSetUp_CookControllerIsCalled()
        {
            InitiateOven();

            cookController.Received(1).StartCooking(Arg.Any<int>(),600);
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

        public void OvenStart_PowerIsSet_CorrectWattageIsPassedToCookController(int presses, int expectedWattage)
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
            cookController.Received(1).StartCooking(expectedWattage,Arg.Any<int>());
        }

        //UC step 13:
        [Test]
        public void CookingIsDone_NormalProcedure_LightTurnsOff()
        {
            InitiateOven();
            ui.CookingIsDone();
            light.Received(2).TurnOff();
        }

        //UC step 14:
        [Test]
        public void CookingIsDone_NormalProcedure_DisplayIsBlanked()
        {
            InitiateOven();
            ui.CookingIsDone();
            display.Received(1).Clear();
        }

        //UC step 16:
        [Test]
        public void DoorOpens_CookingIsDone_LightTurnsOn()
        {
            InitiateOven();
            ui.CookingIsDone();
            door.Open();
            light.Received(3).TurnOn();
        }

        //UC step 19:
        [Test]
        public void DoorCloses_CookingIsDone_LightTurnsOff()
        {
            InitiateOven();
            ui.CookingIsDone();
            door.Open();
            door.Close();
            light.Received(3).TurnOff();
        }
    }
}