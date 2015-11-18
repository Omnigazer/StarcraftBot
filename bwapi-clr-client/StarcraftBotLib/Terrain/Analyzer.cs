using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SWIG.BWTA;
using System.Threading;

namespace StarcraftBotLib.Terrain
{
    class Analyzer
    {
        public event EventHandler Done;        

        public Analyzer()
        {
        }

        public void Run()
        {
            bwta.readMap();
            /*
            bwBWTA = new BackgroundWorker();
            bwBWTA.WorkerReportsProgress = false;
            bwBWTA.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwBWTA_RunWorkerCompleted);
            bwBWTA.DoWork += new DoWorkEventHandler(bwBWTA_DoWork);
            bwBWTA.RunWorkerAsync();
            */
            //(new Thread(() =>
            //{
                Util.Logger.Instance.Log("BWTA Terrain analysis Started");
                bwta.analyze();
                Util.Logger.Instance.Log("BWTA Terrain analysis Completed");
                Done(this, EventArgs.Empty);
            //})).Start();            
        }

        void bwBWTA_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            bwta.analyze();
        }

        void bwBWTA_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Util.Logger.Instance.Log("BWTA Terrain analysis Completed");
            if (Done != null)
                Done(this, EventArgs.Empty);
        }
    }
}
