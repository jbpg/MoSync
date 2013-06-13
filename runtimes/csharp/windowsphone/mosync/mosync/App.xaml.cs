﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MoSync;

namespace test_mosync
{
    public partial class App : Application
    {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Provides access to a ContentManager for the application.
        /// </summary>
        public ContentManager Content { get; private set; }

        /// <summary>
        /// Provides access to a GameTimer that is set up to pump the FrameworkDispatcher.
        /// </summary>
        public GameTimer FrameworkDispatcherTimer { get; private set; }

        /// <summary>
        /// Provides access to the AppServiceProvider for the application.
        /// </summary>
        public AppServiceProvider Services { get; private set; }

        /// <summary>
        /// Used to detect if we need to send the screen size changed event (sent only when a transition
        /// from portrait to landscape or vice versa is detected).
        /// </summary>
        private PageOrientation mOldPageOrientation;

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions.
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // XNA initialization
            InitializeXnaApplication();

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                //Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode,
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }
       }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
		private MoSync.Machine machine = null;

		public MoSync.Machine GetMachine()
		{
			return machine;
		}

		protected void InitExtensions(MoSync.Core core, MoSync.Runtime runtime)
		{
			try
			{
				MoSync.ExtensionsLoader.Load();
			}
			catch (Exception e)
			{
				MoSync.Util.CriticalError("Couldn't load extension: " + e.ToString());
			}

			MoSync.ExtensionModule extMod = runtime.GetModule<MoSync.ExtensionModule>();
			System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (System.Reflection.Assembly a in assemblies)
            {
                try
                {
                    foreach (Type t in a.GetTypes())
                    {
                        IExtensionModule extensionGroupInstance = null;
                        if (t.GetInterface("MoSync.IExtensionModule", false) != null)
                        {
                            extensionGroupInstance = Activator.CreateInstance(t) as IExtensionModule;
                            extMod.AddModule(extensionGroupInstance);
                            extensionGroupInstance.Init(core, runtime);
                        }
                    }
                }
                catch { }
            }
		}

		private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            //RootFrame.Navigated += delegate(object _sender, NavigationEventArgs _e)
            RootFrame.Loaded += delegate(object _sender, RoutedEventArgs _e)
            {
				if (machine == null)
				{
#if !REBUILD
					machine = MoSync.Machine.CreateInterpretedMachine("program", "resources");

#else
	                machine = MoSync.Machine.CreateNativeMachine(new CoreNativeProgram(), "resources");
#endif
					InitExtensions(machine.GetCore(), machine.GetRuntime());
					machine.Run();
				}
            };

            RootFrame.BackKeyPress += new EventHandler<System.ComponentModel.CancelEventArgs>(BackKeyPressHandler);
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            Exception eo = e.ExceptionObject;
            if (eo is MoSync.Util.ExitException)
                return;
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debug.WriteLine(eo.StackTrace);
                if (eo is System.NotImplementedException ||
                    eo is Microsoft.Xna.Framework.Graphics.NoSuitableGraphicsDeviceException)
                {
                    return;
                }
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
       }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;

            PhoneApplicationPage currentPage = (((PhoneApplicationFrame)Application.Current.RootVisual).Content as PhoneApplicationPage);
            mOldPageOrientation = currentPage.Orientation;
            currentPage.OrientationChanged += App_OrientationChanged;
        }

        void App_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            PhoneApplicationPage currentPage = (((PhoneApplicationFrame)Application.Current.RootVisual).Content as PhoneApplicationPage);

            if (((mOldPageOrientation & PageOrientation.Landscape) == PageOrientation.Landscape &&
                (currentPage.Orientation & PageOrientation.Portrait) == PageOrientation.Portrait) ||
                ((mOldPageOrientation & PageOrientation.Portrait) == PageOrientation.Portrait &&
                (currentPage.Orientation & PageOrientation.Landscape) == PageOrientation.Landscape))
            {
                SendScreenSizeChangedEvent();
            }

            AdjustPageSize(currentPage, e.Orientation);

            mOldPageOrientation = currentPage.Orientation;
        }

        /**
         * Adjusts the current page size according to the new orientation.
         */
        private void AdjustPageSize(PhoneApplicationPage currentPage, PageOrientation newOrientation)
        {
            // change the current page in regard to the current orientation.
            if (newOrientation == PageOrientation.Landscape |
                newOrientation == PageOrientation.LandscapeLeft |
                newOrientation == PageOrientation.LandscapeRight)
            {
                currentPage.Height = Application.Current.Host.Content.ActualWidth;
                currentPage.Width = Application.Current.Host.Content.ActualHeight;
            }
            else if (newOrientation == PageOrientation.Portrait |
                     newOrientation == PageOrientation.PortraitDown |
                     newOrientation == PageOrientation.PortraitUp)
            {
                currentPage.Height = Application.Current.Host.Content.ActualHeight;
                currentPage.Width = Application.Current.Host.Content.ActualWidth;
            }
        }

        private void SendScreenSizeChangedEvent()
        {
            Memory eventData = new Memory(4);
            const int MAWidgetEventData_eventType = 0;
            eventData.WriteInt32(MAWidgetEventData_eventType, MoSync.Constants.EVENT_TYPE_SCREEN_CHANGED);

            machine.GetRuntime().PostEvent(new Event(eventData));
        }

        #endregion

        #region XNA application initialization

        // Performs initialization of the XNA types required for the application.
        private void InitializeXnaApplication()
        {
            // Create the service provider
            Services = new AppServiceProvider();

            // Add the SharedGraphicsDeviceManager to the Services as the IGraphicsDeviceService for the app
            foreach (object obj in ApplicationLifetimeObjects)
            {
                if (obj is IGraphicsDeviceService)
                    Services.AddService(typeof(IGraphicsDeviceService), obj);
            }

            // Create the ContentManager so the application can load precompiled assets
            Content = new ContentManager(Services, "Content");

            // Create a GameTimer to pump the XNA FrameworkDispatcher
            FrameworkDispatcherTimer = new GameTimer();
            FrameworkDispatcherTimer.FrameAction += FrameworkDispatcherFrameAction;
            FrameworkDispatcherTimer.Start();
        }

        // An event handler that pumps the FrameworkDispatcher each frame.
        // FrameworkDispatcher is required for a lot of the XNA events and
        // for certain functionality such as SoundEffect playback.
        private void FrameworkDispatcherFrameAction(object sender, EventArgs e)
        {
            FrameworkDispatcher.Update();
        }

        #endregion

        /**
        * The BackKeyPress event handler.
        * Currently it contains the functionality for the back event when a StackScreen is a child of a TabScreen.
        * When this handler does not cover the functionality required it should be updated.
        * @param from Object the object that triggers the event.
        * @param args System.ComponentModel.CancelEventArgs the event arguments.
        */
        public void BackKeyPressHandler(object from, System.ComponentModel.CancelEventArgs args)
        {
            NativeUIModule nativeUIModule = machine.GetRuntime().GetModule<NativeUIModule>();

            //EVENT_TYPE_KEY_RELEASED event data
            Memory eventData = new Memory(8);
            const int MAEventData_eventType = 0;
            const int MAEventData_backButtonKeyCode = 4;
            eventData.WriteInt32(MAEventData_eventType, MoSync.Constants.EVENT_TYPE_KEY_PRESSED);
            eventData.WriteInt32(MAEventData_backButtonKeyCode, MoSync.Constants.MAK_BACK);
            //Posting a CustomEvent
            machine.GetRuntime().PostEvent(new Event(eventData));

            args.Cancel = nativeUIModule.HandleBackButtonPressed();
        }
    }
}