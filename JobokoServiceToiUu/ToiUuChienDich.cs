using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace JobokoServiceToiUu
{
    public partial class ToiUuChienDich : ServiceBase
    {
        private System.Timers.Timer _timer = new System.Timers.Timer();
        private System.Timers.Timer _timerAuto = new System.Timers.Timer();

        public ToiUuChienDich()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                
                _timer.Elapsed += _timer_Elapsed;
                _timer.AutoReset = false;
                _timer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs ee)
        {
            try
            {
                Shared.Test();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                var time = System.Configuration.ConfigurationManager.AppSettings["Timer"];
                _timer.Interval = TimeSpan.FromMinutes(int.Parse(time)).TotalMilliseconds;
            }
        }
        protected override void OnStop()
        {
        }
    }
}
