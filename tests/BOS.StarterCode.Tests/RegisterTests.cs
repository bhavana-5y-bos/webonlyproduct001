using BOS.StarterCode.Models;
using System;
using Xunit;

namespace BOS.StarterCode.Tests
{
    public class RegisterTests
    {
        [Fact]
        public void FirstName_cannot_be_empty()
        {
            const string firstName = "";
            const string lastName = "Doe";
            const string email = "example@example.com";
            const string password = "password";
            Assert.Throws<ArgumentException>(() => { new RegistrationModel(firstName, lastName, email, password); });
        }

        [Fact]
        public void LastNAme_cannot_be_empty()
        {
            const string firstName = "John";
            const string lastName = "";
            const string email = "example@example.com";
            const string password = "password";
            Assert.Throws<ArgumentException>(() => { new RegistrationModel(firstName, lastName, email, password); });
        }

        [Fact]
        public void Email_cannot_be_empty()
        {
            const string firstName = "John";
            const string lastName = "Doe";
            const string email = "";
            const string password = "password";
            Assert.Throws<ArgumentException>(() => { new RegistrationModel(firstName, lastName, email, password); });
        }

        [Fact]
        public void Password_cannot_be_empty()
        {
            const string firstName = "John";
            const string lastName = "Doe";
            const string email = "example@example.com";
            const string password = "";
            Assert.Throws<ArgumentException>(() => { new RegistrationModel(firstName, lastName, email, password); });
        }

        [Fact]
        public void None_of_the_attributes_can_be_empty()
        {
            const string firstName = "";
            const string lastName = "";
            const string email = "";
            const string password = "";
            Assert.Throws<ArgumentException>(() => { new RegistrationModel(firstName, lastName, email, password); });
        }
    }
}
