using System;
namespace core
{
    public class GeneralException : Exception
    {
        public GeneralException(string message = "") : base(message) { }
    }
}
