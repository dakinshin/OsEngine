using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OsEngine.Entity;
using OsEngine.Indicators;
using OsEngine.Logging;
using OsEngine.OsTrader.Panels;

namespace OsEngine.Charts.CandleChart.Indicators
{
    class CandleStorage
    {
        private List<String> _connections;
        private List<List<Candle>> _storage;

        public CandleStorage()
        {
            _connections = new List<String>();
            _storage = new List<List<Candle>>();
        }

        public void Store(int id, List<Candle> candles)
        {
            if (_storage.Count <= id)
            {
                _storage.Add(candles);
            }
            else
            {
                _storage[id] = candles;
            }
        }

        public List<Candle> Retreive(int id)
        {
            return _storage[id];
        }

        public List<List<Candle>> RetreiveAll()
        {
            return _storage;
        }

        public bool CandleMissing(DateTime timeStart)
        {
            return _storage.Any(candles => !candles.Exists(candle => candle.TimeStart == timeStart));
        }

        private int RegisterNewId(String uniqueName)
        {
            int result = _connections.FindIndex(connection => connection == uniqueName);

            if (result == -1)
            {
                result = _storage.Count;
                _connections.Add(uniqueName);
                _storage.Add(new List<Candle>());
            }
            
            return result;
        }

        public List<Candle> RetreiveByTime(DateTime timeStart)
        {
            List<Candle> result = _storage
                .Select(
                    candles => candles.FindLast(
                        candle => candle.TimeStart <= timeStart
                    )
                )
                .ToList();

            /*if (result.Any(rec => rec == null))
            {
                throw new InvalidDataException("Candle expected but not found");
            }*/

            return result;
        }

        public CandleStorageProxy Connect(String uniqueName)
        {
            return new CandleStorageProxy(this, RegisterNewId(uniqueName));
        }

        public class Factory
        {
            private static CandleStorage _instance = null;

            public static CandleStorage Create()
            {
                if (_instance == null)
                {
                    _instance = new CandleStorage();
                }

                return _instance;
            }

            public static CandleStorageProxy CreateAndConnect(String uniqueName)
            {
                CandleStorage instance = Create();
                return instance.Connect(uniqueName);
            }
        }
    }

    class CandleStorageProxy
    {
        private CandleStorage _storage;
        private int _id { get; set; }

        public CandleStorageProxy(CandleStorage storage, int id)
        {
            _storage = storage;
            _id = id;
        }

        public void Store(List<Candle> candles)
        {
            Store(_id, candles);
        }

        public void Store(int id, List<Candle> candles)
        {
            _storage.Store(id, candles);
        }

        public List<Candle> Retreive()
        {
            return Retreive(_id);
        }

        public List<Candle> Retreive(int id)
        {
            return _storage.Retreive(id);
        }

        public List<List<Candle>> RetreiveAll()
        {
            return _storage.RetreiveAll();
        }

        public bool CandleMissing(DateTime timeStart)
        {
            return _storage.CandleMissing(timeStart);
        }

        public List<Candle> RetreiveByTime(DateTime timeStart)
        {
            return _storage.RetreiveByTime(timeStart);
        }
    }

    public class Spread : IIndicator
    {
        /// <summary>
        /// constructor
        /// конструктор
        /// </summary>
        /// <param name="uniqName">unique name/уникальное имя</param>
        /// <param name="canDelete">whether user can remove indicator from chart manually/можно ли пользователю удалить индикатор с графика вручную</param>
        public Spread(string uniqName, bool canDelete)
        {
            Name = uniqName;
            TypeIndicator = IndicatorChartPaintType.Line;
            ColorBase = Color.DodgerBlue;
            PaintOn = true;
            CanDelete = canDelete;
            _storage = CandleStorage.Factory.CreateAndConnect(uniqName);
            Load();
        }

        /// <summary>
        /// constructor without parameters.Indicator will not saved/конструктор без параметров. Индикатор не будет сохраняться
        /// used ONLY to create composite indicators/используется ТОЛЬКО для создания составных индикаторов
        /// Don't use it from robot creation layer/не используйте его из слоя создания роботов!
        /// </summary>
        /// <param name="canDelete">whether user can remove indicator from chart manually/можно ли пользователю удалить индикатор с графика вручную</param>
        public Spread(bool canDelete)
        {
            Name = Guid.NewGuid().ToString();

            TypeIndicator = IndicatorChartPaintType.Line;
            ColorBase = Color.DodgerBlue;
            PaintOn = true;
            CanDelete = canDelete;
            _storage = CandleStorage.Factory.CreateAndConnect("");
        }

        /// <summary>
        /// indicator drawing type
        /// тип прорисовки индикатора
        /// </summary>
        public IndicatorChartPaintType TypeIndicator { get; set; }

        /// <summary>
        /// indicator colors
        /// цвета для индикатора
        /// </summary>
        public List<Color> Colors
        {
            get
            {
                List<Color> colors = new List<Color>();
                colors.Add(ColorBase);
                return colors;
            }
        }

        /// <summary>
        /// all indicator values
        /// все значения индикатора
        /// </summary>
        public List<List<decimal>> ValuesToChart
        {
            get
            {
                List<List<decimal>> list = new List<List<decimal>>();
                list.Add(Values);
                return list;
            }
        }

        /// <summary>
        /// whether indicator can be removed from chart. This is necessary so that robots can't be removed /можно ли удалить индикатор с графика. Это нужно для того чтобы у роботов нельзя было удалить 
        /// indicators he needs in trading/индикаторы которые ему нужны в торговле
        /// </summary>
        public bool CanDelete { get; set; }

        /// <summary>
        /// имя серии на которой индикатор прорисовывается
        /// </summary>
        public string NameSeries { get; set; }

        /// <summary>
        /// name of data series on which indicator will be drawn
        /// имя области на котророй индикатор прорисовывается
        /// </summary>
        public string NameArea
        { get; set; }

        /// <summary>
        /// unique indicator name
        /// уникальное имя индикатора
        /// </summary>
        public string Name
        { get; set; }

        /// <summary>
        /// is indicator tracing enabled
        /// включена ли прорисовка индикатора
        /// </summary>
        public bool PaintOn
        { get; set; }

        /// <summary>
        /// indicator needs to be redrawn
        /// индикатор нужно перерисовать
        /// </summary>
        public event Action<IIndicator> NeadToReloadEvent;

        /// <summary>
        /// delete data
        /// удалить данные
        /// </summary>
        public void Clear()
        {
            if (Values != null)
            {
                Values.Clear();
            }
        }

        /// <summary>
        /// delete file with settings
        /// удалить файл с настройками
        /// </summary>
        public void Delete()
        {
            if (File.Exists(@"Engine\" + Name + @".txt"))
            {
                File.Delete(@"Engine\" + Name + @".txt");
            }
        }

        /// <summary>
        /// upload settings from file
        /// загрузить настройки
        /// </summary>
        public void Load()
        {
            if (!File.Exists(@"Engine\" + Name + @".txt"))
            {
                return;
            }
            try
            {

                using (StreamReader reader = new StreamReader(@"Engine\" + Name + @".txt"))
                {
                    ColorBase = Color.FromArgb(Convert.ToInt32(reader.ReadLine()));
                    PaintOn = Convert.ToBoolean(reader.ReadLine());
                    reader.ReadLine();

                    reader.Close();
                }


            }
            catch (Exception)
            {
                // send to log
                // отправить в лог
            }
        }

        /// <summary>
        /// recalculate indicator. This indicator blocked.
        /// пересчитать индикатор. У данного индикатора блокировано.
        /// </summary>
        /// <param name="candles">candles/свечи</param>
        public void Process(List<Candle> candles)
        {
            if (candles == null)
            {
                return;
            }

            lock(processLock)
            {
                _storage.Store(candles);

                if (_storage.CandleMissing(candles.Last().TimeStart))
                {
                    return;
                }

                if (Values == null)
                {
                    RawValues = new List<decimal>();
                    Values = new List<decimal>();
                }

                if (Values.Count + 1 == candles.Count)
                {
                    ProcessOne(candles);
                }
                else if (Values.Count == candles.Count)
                {
                    ProcessLast(candles);
                }
                else
                {
                    ProcessAll(candles);
                }
            }
        }

        /// <summary>
        /// to upload from the beginning
        /// прогрузить только последнюю свечку
        /// </summary>
        private void ProcessOne(List<Candle> candles)
        {
            decimal rawValue = Calculate(candles, candles.Count - 1);
            RawValues.Add(rawValue);
            Values.Add(Normalize(rawValue));
        }

        /// <summary>
        /// to upload from the beginning
        /// прогрузить с самого начала
        /// </summary>
        private void ProcessAll(List<Candle> candles)
        {
            RawValues = new List<decimal>();
            Values = new List<decimal>();

            for (int i = 0; i < candles.Count; i++)
            {
                decimal rawValue = Calculate(candles, i);
                RawValues.Add(rawValue);
                Values.Add(Normalize(rawValue));
            }
        }

        /// <summary>
        /// overload last value
        /// перегрузить последнее значение
        /// </summary>
        private void ProcessLast(List<Candle> candles)
        {
            decimal rawValue = Calculate(candles, candles.Count - 1);
            RawValues[Values.Count - 1] = rawValue;
            Values[Values.Count - 1] = Normalize(rawValue);
        }

        private decimal Calculate(List<Candle> candles, int barIdx)
        {
            DateTime barTime = candles[barIdx].TimeStart;
            List<Candle> slice = _storage.RetreiveByTime(barTime);

            if (slice.Count != 2 || slice.Any(x => x == null))
            {
                return 0;
            }

            decimal Mul = GetMultiplier();

            decimal value = slice[0].Close - slice[1].Close * Mul;

            return value;
        }

        private decimal GetMultiplier()
        {
            List<List<Candle>> storage = _storage.RetreiveAll();

            decimal max1 = storage[0].GetRange(Math.Max(storage[0].Count - 200, 0), Math.Min(storage[0].Count, 200)).Max(candle => candle.Close);
            decimal max2 = storage[1].GetRange(Math.Max(storage[1].Count - 200, 0), Math.Min(storage[1].Count, 200)).Max(candle => candle.Close);

            return max2 == 0 ? 0 : max1 / max2;
        }

        private decimal Normalize(decimal value)
        {
            List<decimal> range = RawValues.GetRange(Math.Max(RawValues.Count - 200, 0), Math.Min(RawValues.Count, 200));

            decimal min = range.Count == 0 ? 0 : range.Min();
            decimal max = range.Count == 0 ? 0 : range.Max();

            return max == min ? 0 : (value - min) / (max - min);
        }

        /// <summary>
        /// save settings to file
        /// сохранить настройки
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return;
            }
            try
            {
                using (StreamWriter writer = new StreamWriter(@"Engine\" + Name + @".txt", false))
                {
                    writer.WriteLine(ColorBase.ToArgb());
                    writer.WriteLine(PaintOn);
                    writer.Close();
                }
            }
            catch (Exception)
            {
                // send to log
                // отправить в лог
            }
        }

        public void Reload()
        {

        }

        /// <summary>
        /// display settings window
        /// показать окно настроек
        /// </summary>
        public void ShowDialog()
        {
            SpreadUi ui = new SpreadUi(this);
            ui.ShowDialog();

            if (ui.Changed)
            {
                Reload();
            }
        }

        public Color ColorBase
        { get; set; }

        public List<decimal> RawValues
        { get; set; }

        public List<decimal> Values
        { get; set; }

        /// <summary>
        /// candles used to build indicator
        /// свечи по которым строится индикатор
        /// </summary>
        private List<Candle> _myCandles;

        private CandleStorageProxy _storage;

        private readonly object processLock = new object();
    }
}
