using System;

namespace FtxApi.Util
{
    public static class Util
    {
        private static DateTime _epochTime = new DateTime(1970, 1, 1, 0, 0, 0);

        public static long GetMillisecondsFromEpochStart()
        {
            return GetMillisecondsFromEpochStart(DateTime.UtcNow);
        }

        public static long GetMillisecondsFromEpochStart(DateTime time)
        {
            return (long)(time - _epochTime).TotalMilliseconds;
        }

        public static long GetSecondsFromEpochStart(DateTime time)
        {
            return (long)(time - _epochTime).TotalSeconds;
        }

    }
}
