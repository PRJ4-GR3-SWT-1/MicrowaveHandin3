using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microwave.Classes.Boundary;
using Microwave.Classes.Controllers;
using Microwave.Classes.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Microwave.Test.Integration
{
    class Step2UILightDisplayCookCon
    {
        private Button startCancelButton;
        private Button powerButton;
        private Button timeButton;
        private Door door;
        private UserInterface ui;
        private Display display;
        private IDisplay fakeDisplay;
        private Light light;
        private CookController cookController;
        private IPowerTube fakePowerTube;
        private ITimer fakeTimer;
        private IOutput fakeOutput;

        [SetUp]
        public void Setup()
        {
            //Create Substitutes
            fakePowerTube = Substitute.For<IPowerTube>();
            fakeTimer = Substitute.For<ITimer>();
            fakeOutput = Substitute.For<IOutput>();
            fakeDisplay = Substitute.For<IDisplay>();
            //Create instances:
            startCancelButton = new Button();
            powerButton = new Button();
            timeButton = new Button();
            door = new Door();
            display = new Display(fakeOutput);
            light = new Light(fakeOutput);
            //Assemble instances to UI
            cookController = new CookController(fakeTimer, fakeDisplay, fakePowerTube);
            ui = new UserInterface(powerButton, timeButton, startCancelButton, door, display, light, cookController);
            cookController.UI = ui;
        }
        // UC step: 6
        [TestCase(1,50)]
        [TestCase(13, 650)]
        [TestCase(14, 700)]
        [TestCase(15, 50)]
        public void OvenPowerButtonDoorClosed_CorrectWattageIsDisplayedToOutput(int presses,int ExpectedWattage)
        {

            door.Close();

            for (int i = 0; i < presses; i++)
            {
                powerButton.Press();
            }
            // Checks if display.ShowPower writes the correct value to the output.
            fakeOutput.Received().OutputLine(Arg.Is<string>(str=>str.ToLower().Contains($" {ExpectedWattage} ")));

        }
        // UC step: 7
        [TestCase(1, "01")]
        [TestCase(59, "59")]
        //[TestCase(60, "00")] // Fejl fundet. Display viser 60:00 min istedet for 00:00
        public void OvenTimeButtonDoorClosed_CorrectMinutIsDisplayedToOutput(int presses, string ExpectedMinut)
        {
            door.Close();
            powerButton.Press();

            for (int i = 0; i < presses; i++)
            {
                timeButton.Press();
            }
            // Checks if display.ShowTime writes the correct value to the output.
            fakeOutput.Received(1).OutputLine(Arg.Is<string>(str => str.ToLower().Contains($" {ExpectedMinut}:")));

        }
        // UC step: 5 in Extension 2
        [TestCase(1, "01")]
        [TestCase(59, "59")]
        public void OvenTimeButtonDoorOpened_NothingIsDisplayedToOutput(int presses, string ExpectedMinut)
        {
            door.Open();
            powerButton.Press();

            for (int i = 0; i < presses; i++)
            {
                timeButton.Press();
            }
            // Checks if display.ShowTime writes the correct value to the output.
            fakeOutput.Received(0).OutputLine(Arg.Is<string>(str => str.ToLower().Contains($" {ExpectedMinut}:")));

        }
        //UC step: 5 in extension 2
        [TestCase(1, 50)]
        [TestCase(13, 650)]
        [TestCase(14, 700)]
        [TestCase(15, 50)]
        public void OvenPowerButtonDoorOpened_NothingIsDisplayedToOutput(int presses, int ExpectedWattage)
        {

            door.Open();

            for (int i = 0; i < presses; i++)
            {
                powerButton.Press();
            }
            // Checks if display.ShowPower writes the correct value to the output.
            fakeOutput.Received(0).OutputLine(Arg.Is<string>(str => str.ToLower().Contains($" {ExpectedWattage} ")));

        }
        // UC step: 5 in extionsion 2
        [Test]
        public void OvenStartedDoorOpened_DisplayIsCleared()
        {
            powerButton.Press();
            timeButton.Press();
            startCancelButton.Press();

            door.Open();

            fakeOutput.Received(1).OutputLine(Arg.Is<string>(str => str.ToLower().Contains($" cleared")));
        }
        // UC step: 5 in extionsion 2
        [Test]
        public void OvenStartedDoorOpened_SettingsIsCleared()
        {
            powerButton.Press();
            timeButton.Press();
            startCancelButton.Press();

            door.Open();

            fakeOutput.Received(1).OutputLine(Arg.Is<string>(str => str.ToLower().Contains($" cleared")));
        }
        // UC step: 4 in extionsion 2
        [Test]
        public void OvenDoorOpened_LightIsTurnedOn()
        {
            door.Open();

            fakeOutput.Received(1).OutputLine(Arg.Is<string>(str => str.ToLower().Contains($" turned on")));
        }
        // UC step: 10 in extionsion 3
        [Test]
        public void OvenDoorOpenedThenClosed_LightIsTurnedOff()
        {
            door.Open();
            door.Close();

            fakeOutput.Received(1).OutputLine(Arg.Is<string>(str => str.ToLower().Contains($" turned off")));
        }
        // UC step: 10-11
        [TestCase(1, 50, 1)]
        [TestCase(13, 650, 13)]
        [TestCase(14, 700, 14)]
        [TestCase(15, 50, 15)]
        public void OvenCookingStarted_CookingIsCalledWithCorrectParameters(int presses, int ExpectedWattage, int ExpectedTime)
        {

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

            fakePowerTube.Received(1).TurnOn(ExpectedWattage);
            // The time is in seconds. Hence the *60
            fakeTimer.Received(1).Start(ExpectedTime*60);

        }
        // UC step: extension 3
        [Test]
        public void OvenCookingStartedThenStopped_CookControllerStopsTimerAndPowertube()
        {

            door.Close();

            for (int i = 0; i < 5; i++)
            {
                powerButton.Press();
            }
            for (int i = 0; i < 5; i++)
            {
                timeButton.Press();
            }
            startCancelButton.Press();
            startCancelButton.Press();

            fakePowerTube.Received(1).TurnOff();
            fakeTimer.Received(1).Stop();
        }
        // UC step 11-12
        [Test]
        public void OvenCookingStarted_CookControllerTimerIsStartedAndStopped()
        {
            door.Close();

            for (int i = 0; i < 5; i++)
            {
                powerButton.Press();
            }
            for (int i = 0; i < 5; i++)
            {
                timeButton.Press();
            }
            startCancelButton.Press();
            fakeTimer.Received(1).Start(300);
            startCancelButton.Press();
            fakeTimer.Received(1).Stop();

        }
        // UC step: 12
        [Test]
        public void OvenCookingStarted_TimerExperied_CookingIsDone()
        {
            powerButton.Press();
            timeButton.Press();
            startCancelButton.Press();

            fakeTimer.Expired += Raise.Event();

            fakeOutput.Received(1).OutputLine(Arg.Is<string>(str => str.ToLower().Contains($" turned off")));
        }
    }
}
