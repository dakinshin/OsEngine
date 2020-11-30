using System;
using System.Collections.Generic;
using System.Threading;
using OsEngine.Entity;
using OsEngine.Logging;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Indicators;
using OsEngine.Charts.CandleChart.Indicators;

namespace OsEngine.Robots.MarketMaker
{
    public class Breezer: BotPanel
    {
        /// <summary>
        /// trade tab 1
        /// вкладка с первым инструметом
        /// </summary>
        private BotTabSimple _tab1;

        /// <summary>
        /// trade tab 2
        /// вкладка со вторым инструментом
        /// </summary>
        private BotTabSimple _tab2;

        private Spread _spread1;
        private Spread _spread2;

        public Breezer(string name, StartProgram startProgram)
            : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            _tab1 = TabsSimple[0];
            _tab1.CandleFinishedEvent += _tab1_CandleFinishedEvent;

            TabCreate(BotTabType.Simple);
            _tab2 = TabsSimple[1];
            _tab2.CandleFinishedEvent += _tab2_CandleFinishedEvent;

            Spread spread1 = new Spread(name + "Spread1", false);
            _spread1 = _tab1.CreateIndicator(spread1, "SpreadArea");
            Spread spread2 = new Spread(name + "Spread2", false);
            _spread2 = _tab2.CreateIndicator(spread2, "SpreadArea");

            _spread1.Save();
            _spread2.Save();
        }

        public override string GetNameStrategyType()
        {
            return "Breezer";
        }

        public override void ShowIndividualSettingsDialog()
        {

        }

        /// <summary>
        /// new candles in tab1
        /// в первой вкладке новая свеча
        /// </summary>
        void _tab1_CandleFinishedEvent(List<Candle> candles)
        {
            SendNewLogMessage(String.Format("Close1: {0}", candles[candles.Count - 1].Close), LogMessageType.User);
        }

        /// <summary>
        /// new candles tab2
        /// во второй вкладки новая свеча
        /// </summary>
        void _tab2_CandleFinishedEvent(List<Candle> candles)
        {
            SendNewLogMessage(String.Format("Close2: {0}", candles[candles.Count - 1].Close), LogMessageType.User);
        }

    }
}
