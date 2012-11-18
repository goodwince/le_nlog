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


A Sample Hello World App can be found in the download section. This simply
requires you to enter your `LOGENTRIES_TOKEN` in the appSettings section of `web/app.config`. This is explained in more
detail in the instructions below.

To configure NLog, you will need to perform the following:

    * (1) Create a Logentries Account.
    * (2) Setup NLog (if you are not already using it).
    * (3) Configure the Logentries NLog plugin.


Create your Logentries Account
------------------------------
You can register your account on Logentries simply by clicking `Sign Up` at the top of the screen.
Once logged in, create a new host with a name that best represents your app. Select this host and create a 
new logfile of source type `TOKEN TCP` with a name that represents what you will be logging, these names are for your own benefit.
Scroll down for instructions on using HTTP PUT method of sending logs, this requires a different choice for source type.

Logentries NLog Plugin Setup
----------------------------

To install the Logentries Plugin Library, we suggest using Nuget.

The package is found at <https://nuget.org/List/Packages/le_nlog/>

This will also install NLog into your project if it is not already installed.

If you wish to install the library manually, you can find `le_nlog.dll` in the
Downloads tab for this repo.

You will also have to install NLog yourself if you are not using our nuget.

NLog Config
-----------

To configure NLog along with the plug-in, paste the following into your `Web/App.config` directly underneath the opening
`<configuration>`

    <configSections>
      <section name="nlog" type="NLog.Config.ConfigSectionHandler, NLog"/>
    </configSections>
    <nlog>
      <extensions>
        <add assembly="le_nlog"/>
      </extensions>
      <targets>
        <target name="logentries" type="Logentries" debug="true" httpPut="false" ssl="false"
		layout="${date:format=ddd MMM dd} ${time:format=HH:mm:ss} ${date:format=zzz yyyy} ${logger} : ${LEVEL}, ${message}"/>
      </targets>
      <rules>
        <logger name="*" minLevel="Debug" appendTo="logentries"/>
      </rules>
    </nlog>

Token-Based Logging
-------------------

Our default method of sending logs to Logentries is via Token TCP over port 10000. To use this, create a new logfile in the Logentries UI, and select Token TCP as the source type.

Then paste the token that is printed beside the logfile in the appSettings section of your web/app.config file for LOGENTRIES_TOKEN.


HTTP PUT Logging
----------------

Older versions of this library used HTTP PUT over port 80, which is still supported. To use this, create a new logfile in the Logentries UI, and select api/HTTP PUT as the source type.

Next, change the httpPut parameter in the above snippet to true. HTTP PUT requires two parameters called LOGENTRIES_ACCOUNT_KEY and LOGENTRIES_LOCATION in your appSettings to be set.

You can obtain your account key, by Selecting Account on the left sidebar when logged in and clicking Account Key.

Your LOGENTRIES_LOCATION parameter is the name of your host followed by the name of your logfile in the following format:  "hostName/logName"


SSL/TLS
-------
This library supports SSL/TLS logging over both the above logging methods by setting the Ssl value to true in the appender definition. This may have a performance impact however.


-----------------

If you are using App.config in your project, you will need to set the "Copy to
output Directory" property of App.config to "Copy always". This can be done
inside Visual Studio. 

Logging Messages
----------------

With that done, you are ready to send logs to Logentries.

In each class you wish to log from, enter the following using directive at the top if it is not already there:

    using NLog;

Then create this object at class-level:

    private static readonly Logger log = LogManager.GetCurrentClassLogger();

What this does is create a logger with the name of the current class for
clarity in the logs.

Now within your code in that class, you can log using NLog as normal and it
will log to Logentries.

Example:

	log.Debug("Debugging Message");
	log.Info("Informational message");
	log.Warn("Warning Message");
	
Troubleshooting
---------------

The Logentries plugin logs its debug messages to NLog's Internal Logger, if you
wish to see these change the opening `<nlog>` statement in web.config to:

    <nlog internalLogFile="..." internalLogLevel="Debug">

Insert the location of a file on your local system to write to, ensuring that
its not read-only.
