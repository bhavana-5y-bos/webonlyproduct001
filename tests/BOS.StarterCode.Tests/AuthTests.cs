using BOS.StarterCode.Models;
using System;
using Xunit;

namespace BOS.StarterCode.Tests
{
    public class AuthTests
    {
        [Fact]
        public void UserName_cannot_be_empty()
        {
            const string username = "";
            Assert.Throws<ArgumentException>(() => { new AuthModel(username); });
        }

        [Fact]
        public void UserName_and_password_cannot_be_empty()
        {
            const string username = "";
            const string password = "";
            Assert.Throws<ArgumentException>(() => { new AuthModel(username, password); });
        }
    }
}
