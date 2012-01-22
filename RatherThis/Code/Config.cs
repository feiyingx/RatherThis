using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace RatherThis.Code
{
    public class Config
    {
        public static class EmailSettings
        {
            private static string _usernameKey = "SmtpUsername";
            private static string _passwordKey = "SmtpPassword";
            private static string _serverNameKey = "SmtpServerName";
            private static string _serverPortKey = "SmtpServerPort";
            private static string _useSslKey = "SmtpUseSsl";
            private static string _writeAsFile = "SmtpWriteAsFile";


            public static string Username()
            {
                return ConfigurationManager.AppSettings[_usernameKey];
            }

            public static string Password()
            {
                return ConfigurationManager.AppSettings[_passwordKey];
            }

            public static string ServerName()
            {
                return ConfigurationManager.AppSettings[_serverNameKey];
            }

            public static bool UseSsl()
            {
                string useSsl = ConfigurationManager.AppSettings[_useSslKey];
                bool bUseSsl = true;
                Boolean.TryParse(useSsl, out bUseSsl);
                return bUseSsl;

            }

            public static int ServerPort()
            {
                string portNumber = ConfigurationManager.AppSettings[_serverPortKey];
                int iPortNumber = 587;
                Int32.TryParse(portNumber, out iPortNumber);
                return iPortNumber;

            }

            public static bool WriteAsFile()
            {
                string writeAsFile = ConfigurationManager.AppSettings[_writeAsFile];
                bool bWriteAsFile = false;
                Boolean.TryParse(writeAsFile, out bWriteAsFile);
                return bWriteAsFile;
            }

            public static string FileLocation()
            {
                return @"C:\Users\Sexy\Desktop\Projects\TestEmail\RatherThis";
            }
        }
    }
}