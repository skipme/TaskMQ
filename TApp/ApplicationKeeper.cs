using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TaskUniversum;

namespace TaskBroker
{
    public class ApplicationKeeper
    {
        static ILogger logger = TaskUniversum.ModApi.ScopeLogger.GetClassLogger();

        public static ManualResetEvent sync;

        public static void AppdomainLoop()
        {
            sync = new ManualResetEvent(false);
            while (true)
            {
                sync.Reset();
                // create appdomain
                logger.Debug("domain starting.");

                AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
                AppDomain Domain = AppDomain.CreateDomain("ApplicationKeeper", null, setup);
                //
                BrokerApplication app = (BrokerApplication)Domain.CreateInstanceAndUnwrap(
                    typeof(BrokerApplication).Assembly.FullName,
                    typeof(BrokerApplication).FullName);
                //RunAppInSeparateThread(app);
                app.Run(sync, true);
                // waitfor sync
                sync.WaitOne();

                // unload appdomain
                AppDomain.Unload(Domain);

                logger.Debug("domain unloaded.");

                GC.Collect(); // collects all unused memory
                GC.WaitForPendingFinalizers(); // wait until GC has finished its work
                GC.Collect();
            }
        }

        static void RunAppInSeparateThread(BrokerApplication app)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(AppThread));
            thread.Start(app);
        }
        static void AppThread(object o)
        {
            BrokerApplication app = (BrokerApplication)o;
            app.Run(sync);
        }
    }
}
