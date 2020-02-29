using System;

namespace MockMyDb
{
    public class MockException : Exception
    {
        public MockException(string message) : base(message)
        {
        }
    }
}