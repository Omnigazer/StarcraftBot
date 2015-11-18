using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SWIG.BWAPI;
using BWAPI;

namespace StarcraftBotLib
{
    public class StarCraftBot : IStarcraftBot
    {
        SampleDecisionMaker DecisionMaker = new SampleDecisionMaker();        
        Terrain.Analyzer a;
        
        public void onStart()
        {
            Util.Logger.Instance.Log("Bot Started");
            Util.Logger.Instance.Log("Enabling User Input");
            bwapi.Broodwar.sendText("Mono StarcraftBot initialised");
            bwapi.Broodwar.enableFlag(1);     
            a = new Terrain.Analyzer();
            a.Done += a_Done;
            a.Run();                             
        }

        void a_Done(object sender, EventArgs e)
        {
            DecisionMaker.onAnalysisComplete();
        }        

        public void onEnd(bool isWinner)
        {
            Util.Logger.Instance.Log("Game Over. I " + ((isWinner) ? "Won." : "Lost."));
        }

        public void onFrame()
        {
            /*
            foreach (var unit in myUnits)
            {
                if (!unit.HasTarget() && enemies.Count > 0)
                {
                    unit.Attack(enemies[0]);
                }
                else if (analysisDone)
                {
                    unit.Attack(enemies[0]);
                }
            } 
            */
            
            DecisionMaker.onNewFrame();
        }

        public void onSendText(string text)
        {

        }

        public void onReceiveText(Player player, string text)
        {

        }

        public void onPlayerLeft(Player player)
        {

        }

        public void onNukeDetect(Position target)
        {

        }

        public void onUnitDiscover(Unit unit)
        {

        }

        public void onUnitEvade(Unit unit)
        {

        }

        public void onUnitShow(Unit unit)
        {
            if (unit.getPlayer().isEnemy(bwapi.Broodwar.self()))
            {
                DecisionMaker.onEnemyShown(unit);                
            }
            if (unit.getType().getName() == "Resource Mineral Field")
            {
                DecisionMaker.onfoundMinerals(new BW.Unit(unit));
            }
            if (DecisionMaker.AIState.ToString() != "PreInitialize")
            {
                // PrintMessage("Unit Shown: [" + unit.getType().getName() + "] at [" + unit.getPosition().xConst() + "," + unit.getPosition().yConst() + "]");
            }
        }

        public void onUnitHide(Unit unit)
        {
            /*
            if (unit.getPlayer() != bwapi.Broodwar.self())
            {
                enemies.Remove(unit);
                bwapi.Broodwar.printf("Unit Hidden: [" + unit.getType().getName() + "] at [" + unit.getPosition().xConst() + "," + unit.getPosition().yConst() + "]");
            }
            */ 
        }

        public void onUnitCreate(Unit unit)
        {
            if (unit.getPlayer() == bwapi.Broodwar.self())
            {
                if (DecisionMaker.AIState.ToString() != "PreInitialize")
                {
                    // PrintMessage("Unit Created: [" + unit.getType().getName() + "] at [" + unit.getPosition().xConst() + "," + unit.getPosition().yConst() + "]");
                }
                DecisionMaker.onMyUnitCreated(unit);
            }
        }

        public void PrintMessage(string message)
        {            
            bwapi.Broodwar.printf(message);            
        }

        public void onUnitDestroy(Unit unit)
        {
            if (unit.getPlayer() == bwapi.Broodwar.self())
            {
                DecisionMaker.onMyUnitDestroyed(unit);
            }            
            Console.WriteLine("destroyed");
        }

        public void onUnitMorph(Unit unit)
        {
            if (unit.getPlayer() == bwapi.Broodwar.self())
            {
                if (DecisionMaker.AIState.ToString() != "PreInitialize")
                {
                    // PrintMessage("Unit Created: [" + unit.getType().getName() + "] at [" + unit.getPosition().xConst() + "," + unit.getPosition().yConst() + "]");
                }
                DecisionMaker.onMyUnitMorph(unit);
            }
        }

        public void onUnitRenegade(Unit unit)
        {

        }

        public void onSaveGame(string gameName)
        {

        }


        public void onUnitComplete(Unit unit)
        {
            if (unit.getPlayer() == bwapi.Broodwar.self())
            {
                if (DecisionMaker.AIState.ToString() != "PreInitialize")
                {
                    // PrintMessage("Unit Completed: [" + unit.getType().getName() + "] at [" + unit.getPosition().xConst() + "," + unit.getPosition().yConst() + "]");
                }
                DecisionMaker.onMyUnitCompleted(unit);
            }
            var actual_type = unit.getType();
            // Console.WriteLine(actual_type.getName());
        }
    }
}
