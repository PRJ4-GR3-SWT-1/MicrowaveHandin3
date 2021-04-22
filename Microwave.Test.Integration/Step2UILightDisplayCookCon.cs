using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
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
        private Light light;
        private CookController cookController;
        private IPowerTube powerTube;
        private ITimer timer;
        private IOutput output;

        [SetUp]
        public void Setup()
        {
            //Create Substitutes
            powerTube = Substitute.For<IPowerTube>();
            timer = Substitute.For<ITimer>();
            output = Substitute.For<IOutput>();
            //Create instances:
            startCancelButton = new Button();
            powerButton = new Button();
            timeButton = new Button();
            door = new Door();
            display = new Display(output);
            light = new Light(output);
            //Assemble instances to UI
            ui = new UserInterface(powerButton, timeButton, startCancelButton, door, display, light, cookController);
            cookController = new CookController(timer, display, powerTube);
        }

        [TestCase(1,50)]
        [TestCase(13, 650)]
        [TestCase(14, 700)]
        public void OvenPowerButton_CorrectWattageIsDisplayedToOutput(int presses,int ExpectedWattage)
        {

            door.Close();

            for (int i = 0; i < presses; i++)
            {
                powerButton.Press();
            }
            // Checks if display.ShowPower writes the correct value to the output.
            output.Received().OutputLine(Arg.Is<string>(str=>str.ToLower().Contains($" {ExpectedWattage} ")));

        }

        [TestCase(1, "01")]
        [TestCase(59, "59")]
        [TestCase(60, "00")] // Fejl fundet. Display viser 60:00 min istedet for 00:00
        public void OvenTimeButton_CorrectWattageIsDisplayedToOutput(int presses, string ExpectedMinut)
        {
            door.Close();
            powerButton.Press();

            for (int i = 0; i < presses; i++)
            {
                timeButton.Press();
            }
            // Checks if display.ShowTime writes the correct value to the output.
            output.Received(1).OutputLine(Arg.Is<string>(str => str.ToLower().Contains($" {ExpectedMinut}:")));

        }
    }
}
