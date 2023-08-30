using System;
using System.Text.RegularExpressions;

namespace Unity.LiveCapture
{
    static class PackageUtility
    {
        public static Version GetVersion(string version)
        {
            var versionNumbers = Regex.Split(version, @"\D+");

            if (versionNumbers.Length >= 4)
            {
                return new Version(
                    int.Parse(versionNumbers[0]),
                    int.Parse(versionNumbers[1]),
                    int.Parse(versionNumbers[2]),
                    int.Parse(versionNumbers[3])
                );
            }

            return versionNumbers.Length switch
            {
                3 => new Version(int.Parse(versionNumbers[0]), int.Parse(versionNumbers[1]), int.Parse(versionNumbers[2])),
                2 => new Version(int.Parse(versionNumbers[0]), int.Parse(versionNumbers[1])),
                _ => default
            };
        }
    }
}
