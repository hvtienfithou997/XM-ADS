using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JobokoServiceToiUu
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            return;
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ToiUuChienDich()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
