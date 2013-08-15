﻿/*
 * Copyright (C) 2013 Colin Mackie.
 * This software is distributed under the terms of the GNU General Public License.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Text;
using System.Resources;
using System.Runtime.CompilerServices;

namespace WinAuth
{
  /// <summary>
  /// Class that launches the main form
  /// </summary>
  static class WinAuthMain
  {
    /// <summary>
    /// Name of this application used for %USEPATH%\[name] folder
    /// </summary>
    public const string APPLICATION_NAME = "WinAuth";

    /// <summary>
    /// Window title for this application
    /// </summary>
    public const string APPLICATION_TITLE = "WinAuth";

    /// <summary>
    /// Winuath email address used as sender to backup emails
    /// </summary>
    public const string WINAUTHBACKUP_EMAIL = "winauth@gmail.com";

		/// <summary>
		/// URL to post error reports
		/// </summary>
		public const string WINAUTH_BUG_URL = "http://www.winauth.com/bug";

		/// <summary>
		/// URL to get latest information
		/// </summary>
		public const string WINAUTH_UPDATE_URL = "http://www.winauth.com/current-version.xml";

		/// <summary>
		/// Set of inbuilt icons and authenticator types
		/// </summary>
		public static Dictionary<string, string> AUTHENTICATOR_ICONS = new Dictionary<string, string>
		{
			{"Battle.Net", "BattleNetAuthenticatorIcon.png"},
			{"World of Warcraft", "WarcraftIcon.png"},
			{"Diablo III", "DiabloIcon.png"},
			{"s1", string.Empty},
			{"ArenaNet", "ArenaNetIcon.png"},
			{"Guild Wars 2", "GuildWarsAuthenticatorIcon.png"},
			{"s2", string.Empty},
			{"Trion", "TrionAuthenticatorIcon.png"},
			{"Rift", "RiftIcon.png"},
			{"Defiance", "DefianceIcon.png"},
			{"End Of Nations", "EndOfNationsIcon.png"},
			{"s3", string.Empty},
			{"Google Authenticator", "GoogleAuthenticatorIcon.png"},
			{"Google", "GoogleIcon.png"},
			{"Chrome", "ChromeIcon.png"},
			{"s4", string.Empty},
			{"Microsoft", "MicrosoftAuthenticatorIcon.png"},
			{"Windows 8", "Windows8Icon.png"},
			{"Windows 7", "Windows7Icon.png"},
			{"s5", string.Empty},
			{"Bitcoin", "BitcoinIcon.png"},
			{"Bitcoin Gold Style", "BitcoinGoldIcon.png"},
			{"Bitcoin Eruo Style", "BitcoinEuroIcon.png"},
			{"Litecoin", "LitecoinIcon.png"}
		};

		public static List<RegisteredAuthenticator> REGISTERED_AUTHENTICATORS = new List<RegisteredAuthenticator>
		{
			new RegisteredAuthenticator {Name="Battle.Net", AuthenticatorType=RegisteredAuthenticator.AuthenticatorTypes.BattleNet, Icon="BattleNetAuthenticatorIcon.png"},
			new RegisteredAuthenticator {Name="Guild Wars 2", AuthenticatorType=RegisteredAuthenticator.AuthenticatorTypes.GuildWars, Icon="GuildWarsAuthenticatorIcon.png"},
			new RegisteredAuthenticator {Name="Trion / Rift", AuthenticatorType=RegisteredAuthenticator.AuthenticatorTypes.Trion, Icon="TrionAuthenticatorIcon.png"},
			new RegisteredAuthenticator {Name="Google", AuthenticatorType=RegisteredAuthenticator.AuthenticatorTypes.Google, Icon="GoogleAuthenticatorIcon.png"},
			new RegisteredAuthenticator {Name="Microsoft", AuthenticatorType=RegisteredAuthenticator.AuthenticatorTypes.Microsoft, Icon="MicrosoftAuthenticatorIcon.png"}
		};

		public static ResourceManager StringResources = new ResourceManager(typeof(WinAuth.Resources.strings).FullName, typeof(WinAuth.Resources.strings).Assembly);

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
			if (!System.Diagnostics.Debugger.IsAttached)
			{
				AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
				Application.ThreadException += OnThreadException;
				Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

				try
				{
					main();
				}
				catch (Exception ex)
				{
					LogException(ex);
					throw;
				}
			}
			else
			{
				main();
			}
    }

		static void OnThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			LogException(e.Exception as Exception);
		}

		static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			LogException(e.ExceptionObject as Exception);
		}

		private static void LogException(Exception ex)
		{
			// add catch for unknown application exceptions to try and get closer to bug
			StringBuilder capture = new StringBuilder();
			//
			try
			{
				Exception e = ex;
				while (e != null)
				{
					capture.Append(new StackTrace(e).ToString()).Append(Environment.NewLine);
					e = e.InnerException;
				}
				//
				string dir = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), WinAuthMain.APPLICATION_NAME);
				if (Directory.Exists(dir) == false)
				{
					dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				}
				File.WriteAllText(Path.Combine(dir, "winauth.log"), capture.ToString());
			}
			catch (Exception) { }

			// save last error into registry for diagnosticss
			try
			{
				WinAuthHelper.SaveLastErrorToRegistry(capture.ToString());
			}
			catch (Exception) { }

			try
			{
				ExceptionForm report = new ExceptionForm();
				report.ErrorException = ex;
				report.TopMost = true;
				if (report.ShowDialog() == DialogResult.Cancel)
				{
					Process.GetCurrentProcess().Kill();
				}
			}
			catch (Exception) { }
		}

		private static void main()
		{
			// Issue #53: set a default culture
			CultureInfo ci = new CultureInfo("en"); // or en-US, en-GB
			System.Threading.Thread.CurrentThread.CurrentCulture = ci;
			System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			Application.Run(new WinAuthForm());
		}
  }
}