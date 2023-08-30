using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace Unity.LiveCapture.Editor
{
    /// <summary>
    /// A class that can checks for and add firewall rules.
    /// </summary>
    static class FirewallUtility
    {
        enum Direction
        {
            In,
            Out,
        }

        enum Action
        {
            Allow,
            Block,
        }

        [Flags]
        enum Profile
        {
            None = 0,
            Private = 1 << 0,
            Domain = 1 << 1,
            Public = 1 << 2,
        }

        struct Rule
        {
            public string Name { get; set; }
            public bool Enabled { get; set; }
            public Direction Direction { get; set; }
            public Profile Profile { get; set; }
            public Action Action { get; set; }
            public string Path { get; set; }
        }

        static readonly string s_ModuleName = "Live Capture";
        static readonly string s_RuleName = $"Unity {Application.unityVersion} {s_ModuleName}";

        /// <summary>
        /// Can the firewall be modified on the editor platform.
        /// </summary>
        public static bool IsSupported
        {
            get
            {
#if UNITY_EDITOR_WIN
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// An event invoked when the firewall has been configured.
        /// </summary>
        /// <remarks>
        /// The Boolean indicates if all the firewall rules already existed or were successfully added.
        /// </remarks>
        public static event Action<bool> FirewallConfigured;

        /// <summary>
        /// Checks if the firewall rules needed to permit Unity to freely communicate exist.
        /// </summary>
        /// <returns>True if all the rules exist.</returns>
        public static bool IsConfigured()
        {
            if (!IsSupported)
            {
                Debug.LogWarning("Automatic firewall configuration is not supported on this editor platform.");
                return false;
            }

#if UNITY_EDITOR_WIN
            if (TryGetRules(out var rules))
            {
                return rules.Any(r => r.Direction == Direction.In && r.Action == Action.Allow)
                    && rules.Any(r => r.Direction == Direction.Out && r.Action == Action.Allow)
                    && rules.All(r => r.Action != Action.Block);
            }
#endif

            return false;
        }

        /// <summary>
        /// Adds firewall rules that permit Unity to freely communicate over the network.
        /// </summary>
        /// <remarks>
        /// This will invoke the <see cref="FirewallConfigured"/> event once completed, as the configuration may
        /// take place asynchronously.
        /// </remarks>
        public static void ConfigureFirewall()
        {
            if (!IsSupported)
            {
                Debug.LogWarning("Automatic firewall configuration is not supported on this editor platform.");
                FirewallConfigured?.Invoke(false);
                return;
            }

            if (IsConfigured())
            {
                FirewallConfigured?.Invoke(true);
                return;
            }

#if UNITY_EDITOR_WIN
            var programPath = Path.GetFullPath(EditorApplication.applicationPath);

            // We need to supply all the commands in a single line.
            // Commands separated by '&' are executed even if a previous command fails.
            var args = new StringBuilder();

            var needsInboundRule = true;
            var needsOutboundRule = true;

            if (TryGetRules(out var rules))
            {
                foreach (var rule in rules)
                {
                    // remove rules that block the editor
                    if (rule.Action == Action.Block)
                    {
                        args.Append($"netsh advfirewall firewall delete rule name=\"{rule.Name}\" program=\"{programPath}\"");
                        args.Append("&");
                    }
                    // check if we have rules allowing the editor already
                    if (rule.Direction == Direction.In && rule.Action == Action.Allow && rule.Profile == (Profile.Private | Profile.Domain))
                    {
                        needsInboundRule = false;
                    }
                    if (rule.Direction == Direction.Out && rule.Action == Action.Allow && rule.Profile == (Profile.Private | Profile.Domain))
                    {
                        needsOutboundRule = false;
                    }
                }
            }

            if (needsInboundRule)
            {
                args.Append($"netsh advfirewall firewall add rule name=\"{s_RuleName}\" program=\"{programPath}\" dir=in profile=private,domain action=allow enable=yes");
                args.Append("&");
            }
            if (needsOutboundRule)
            {
                args.Append($"netsh advfirewall firewall add rule name=\"{s_RuleName}\" program=\"{programPath}\" dir=out profile=private,domain action=allow enable=yes");
                args.Append("&");
            }

            args.Append("exit");

            // run the commands from a command line
            var info = new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = $"/k {args}",
                UseShellExecute = true,
                // Attempts to run with elevated permissions. This is mandatory when modifying firewall rules.
                // If User Account Control notifications are enabled, a pop-up will appear asking for permission,
                // otherwise the process can execute without user input.
                Verb = "runas",
            };

            try
            {
                var process = new Process
                {
                    StartInfo = info,
                    EnableRaisingEvents = true,
                };

                process.Exited += (sender, eventArgs) =>
                {
                    switch (process.ExitCode)
                    {
                        case 0: // success
                            FirewallConfigured?.Invoke(true);
                            break;
                        default: // assume there is an error otherwise
                            Debug.LogError($"Failed to add firewall rules!");
                            FirewallConfigured?.Invoke(false);
                            break;
                    }
                    process.Dispose();
                };

                process.Start();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to configure firewall: {e}");
                FirewallConfigured?.Invoke(false);
            }
#else
            FirewallConfigured?.Invoke(false);
#endif
        }

#if UNITY_EDITOR_WIN
        static bool TryGetRules(out Rule[] rules)
        {
            rules = null;

            var programPath = NormalizePath(EditorApplication.applicationPath);

            var info = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = $"advfirewall firewall show rule name=all verbose",
                // don't use a command line so we can read the standard output
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                // hide windows opened by the process
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
            };

            try
            {
                /*
                Sample output rule is below.

                Rule Name:                            Unity 2022.1.0b10 Live Capture
                ----------------------------------------------------------------------
                Enabled:                              Yes
                Direction:                            In
                Profiles:                             Domain,Private
                Grouping:
                LocalIP:                              Any
                RemoteIP:                             Any
                Protocol:                             Any
                Edge traversal:                       No
                Program:                              C:\Program Files\Unity\2022.1.0b10\Editor\Unity.exe
                InterfaceTypes:                       Any
                Security:                             NotRequired
                Rule source:                          Local Setting
                Action:                               Allow
                */

                using var process = new Process
                {
                    StartInfo = info,
                };

                process.Start();

                var tempRules = new List<Rule>();
                var currentRule = default(Rule);

                while (!process.StandardOutput.EndOfStream)
                {
                    var line = process.StandardOutput.ReadLine();

                    var splitIndex = line.IndexOf(':');

                    if (splitIndex < 0)
                    {
                        continue;
                    }

                    var key = line.Substring(0, splitIndex).Trim();
                    var value = line.Substring(splitIndex + 1).Trim();

                    switch (key)
                    {
                        case "Rule Name":
                            currentRule = new Rule
                            {
                                Name = value,
                            };
                            break;
                        case "Enabled":
                            currentRule.Enabled = value == "Yes";
                            break;
                        case "Direction":
                            currentRule.Direction = value == "In" ? Direction.In : Direction.Out;
                            break;
                        case "Profiles":
                            foreach (var profile in value.Split(','))
                            {
                                switch (profile)
                                {
                                    case "Private":
                                        currentRule.Profile |= Profile.Private;
                                        break;
                                    case "Domain":
                                        currentRule.Profile |= Profile.Domain;
                                        break;
                                    case "Public":
                                        currentRule.Profile |= Profile.Public;
                                        break;
                                }
                            }
                            break;
                        case "Program":
                            currentRule.Path = NormalizePath(value);
                            break;
                        case "Action":
                            currentRule.Action = value == "Allow" ? Action.Allow : Action.Block;

                            // this is the last rule property, so add the rule if it is relevant
                            if (currentRule.Enabled && (int)(currentRule.Profile & (Profile.Private | Profile.Domain)) != 0 && currentRule.Path == programPath)
                            {
                                tempRules.Add(currentRule);
                            }
                            break;
                    }
                }

                if (!process.WaitForExit(2000))
                {
                    process.Kill();
                    Debug.LogWarning($"Failed to get firewall rules, netsh did not exit successfully.");
                    return false;
                }

                switch (process.ExitCode)
                {
                    case 0: // success
                        rules = tempRules.ToArray();
                        return true;
                    case 1: // no matching rules found. Not in the docs, found this out experimentally
                        return false;
                    default: // assume there is an error otherwise
                        Debug.LogError($"Failed to get firewall rules with exit code {process.ExitCode:X})!");
                        return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to get firewall rule: {e}");
                return false;
            }
        }
#endif

        static string NormalizePath(string path)
        {
            return Path.GetFullPath(path).ToUpperInvariant();
        }
    }
}
