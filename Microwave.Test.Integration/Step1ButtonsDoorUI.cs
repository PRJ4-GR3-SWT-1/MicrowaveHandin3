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

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}