Logging To Logentries from AppHarbor using NLog
========================================================

Simple Usage Example

---------------------

    public class HomeController : Controller
    {
        private static readonly Logger log = LogManager.GetLogger(typeof(HomeController).Name);

        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";

            log.Warn("This is a warning message");

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }

-----------------------------

A Sample Hello World App can be found in the download section. This simply requires you to enter your LOGENTRIES_ACCOUNT_KEY
and LOGENTRIES_LOCATION in the appSettings section of web/app.config. This is explained in more detail in the instructions below.

-----------------------------

To configure NLog, you will need to perform the following:

    * (1) Obtain your Logentries account key.
    * (2) Setup NLog (if you are not already using it).
    * (3) Configure the Logentries NLog plugin.

You can obtain your Logentries account key on the Logentries UI, by clicking account in the bottom left corner.

It will be displayed in grey on the right hand side.

Logentries NLog Plugin Setup
--------------------------------

To install the Logentries Plugin Library, we suggest using Nuget.

The package is found at https://nuget.org/List/Packages/le_nlog

This will also install NLog into your project if it is not already installed.

If you wish to install the library manually, you can find le_nlog.dll in the Downloads tab for this repo.

You will also have to install NLog yourself if you are not using our nuget.

NLog Config
------------------

The following configuration is placed in your web/app.config automatically by our Nuget. However if a web/app config does not exist
when you install the Nuget, you must do it manually.

If you are not using the Nuget, copy and paste it directly under the opening `<configuration>`

    <configSections>
      <section name="nlog" type="NLog.Config.ConfigSectionHandler, NLog"/>
    </configSections>
    <appSettings>
      <add key="LOGENTRIES_ACCOUNT_KEY" value="" />
      <add key="LOGENTRIES_LOCATION" value="" />
    </appSettings>
    <nlog>
      <extensions>
        <add assembly="le_nlog"/>
      </extensions>
      <targets>
        <target name="logentries" type="Logentries" debug="true" ssl="false" 
		layout="${date:format=ddd MMM dd} ${time:format=HH:mm:ss} ${date:format=zzz yyyy} ${logger} : ${LEVEL}, ${message}"/>
      </targets>
      <rules>
        <logger name="*" minLevel="Info" appendTo="logentries"/>
      </rules>
    </nlog>

If are using App.config in your project, you will need to set the "Copy to output Directory" property of App.config to "Copy always". This 
can be done inside Visual Studio. In the appSettings subsection, using your account-key which you obtained earlier, fill in the value for 
LOGENTRIES_ACCOUNT_KEY. Also replace the "LOGENTRIES_LOCATION" value with the location of your logfile on Logentries. This should be in the following format:
	
	hostname/logfilename
	
If you would rather create a host and log file from your command line instead of the Logentries UI,

you can use the following program:

https://github.com/downloads/logentries/le_nlog/register.exe


Logging Messages
----------------

With that done, you are ready to send logs to Logentries.

In each class you wish to log from, enter the following using directive at the top if it is not already there:

	using NLog;

Then create this object at class-level:

	private static readonly Logger log = LogManager.GetLogger(typeof(NAME_OF_CLASS).Name);

What this does is create a logger with the name of the current class for clarity in the logs.

Now within your code in that class, you can log using NLog as normal and it will log to Logentries.

Example:

	log.Debug("Debugging Message");
	log.Info("Informational message");
	log.Warn("Warning Message");
	
Troubleshooting
---------------

The Logentries plugin logs its debug messages to NLog's Internal Logger, if you wish to see these change the opening `<nlog>` statement in web.config to:

`<nlog internalLogFile="..." internalLogLevel="Debug">`

Insert the location of a file on your local system to write to, ensuring that its not read-only.