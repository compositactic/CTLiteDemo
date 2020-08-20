using System;
using System.Threading;

namespace CTLiteDemo.Model
{
    public static class ExtensionMethods
    {
        private static long lastTimeStamp = DateTime.UtcNow.Ticks;
        public static long NewId(this long _)
        {
            long originalValue;
            long newValue;
            do
            {
                originalValue = lastTimeStamp;
                long now = DateTime.UtcNow.Ticks;
                newValue = Math.Max(now, originalValue + 1);
            } while (Interlocked.CompareExchange(ref lastTimeStamp, newValue, originalValue) != originalValue);

            return newValue;
        }
    }
}
