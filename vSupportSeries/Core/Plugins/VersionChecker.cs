using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LeagueSharp;

namespace vSupport_Series.Core.Plugins
{
    class VersionCheck
    {
        public static void UpdateCheck()
        {
            /*h3h3 version checker*/
            Task.Factory.StartNew(() =>
            {
                try
                {
                    using (var c = new WebClient())
                    {
                        var rawVersion = c.DownloadString("https://raw.githubusercontent.com/iamveto/vSeries/master/vSupportSeries/Properties/AssemblyInfo.cs");
                        var match = new Regex(@"\[assembly\: AssemblyVersion\(""(\d{1,})\.(\d{1,})\.(\d{1,})\.(\d{1,})""\)\]").Match(rawVersion);

                        if (match.Success)
                        {
                            var gitVersion = new Version(string.Format("{0}.{1}.{2}.{3}", match.Groups[1], match.Groups[2], match.Groups[3], match.Groups[4]));

                            if (gitVersion != typeof(Program).Assembly.GetName().Version)
                            {
                                Game.PrintChat("<font color='#15C3AC'>vSupport Series:</font> <font color='#FF0000'>" + "OUTDATED - Please Update to Version: " + gitVersion + "</font>");
                                Game.PrintChat("<font color='#15C3AC'>vSupport Series:</font> <font color='#FF0000'>" + "OUTDATED - Please Update to Version: " + gitVersion + "</font>");
                            }
                            else
                            {
                                Game.PrintChat("<font color='#15C3AC'>vSupport Series:</font> <font color='#40FF00'>" + "UPDATED - Version: " + gitVersion + "</font>");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }
    }
}