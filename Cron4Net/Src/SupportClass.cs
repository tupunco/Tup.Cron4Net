using System;

namespace Tup.Cron4Net
{
    /// <summary>
    /// java throwable exception class
    /// </summary>
    public class ThrowableException : Exception
    {
        /// <summary>
        /// </summary>
        public ThrowableException()
            : base()
        {
        }
        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        public ThrowableException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="innerException"></param>
        public ThrowableException(Exception innerException)
            : base(null, innerException)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ThrowableException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }

    /// <summary>
    /// java runnable interface
    /// </summary>
    public interface IRunnable
    {
        /// <summary>
        /// 
        /// </summary>
        void Run();
    }
}
