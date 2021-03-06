﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using FTN.Services.NetworkModelService.DataModel.Core;
using FTN.Services.NetworkModelService.DataModel.Wires;
using FTN.ESI.SIMES.CIM.CIMAdapter;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using DERMSApp.Model;
using System.Windows;
using System.Windows.Input;
using FTN.Common.WeatherForecast.Model;
using WeatherForecastProxyNS;
using FTN.Common;
using System.Threading;
using FTN.Common.Services;
using System.ServiceModel;
using DERMSApp.Views;
using Adapter;

namespace DERMSApp.ViewModels
{
    public class EntireNetworkViewModel: ViewModelBase, IDeltaNotifyCallback
    {
        //readonly ReadOnlyCollection<GeographicalRegionViewModel> _regions;
        private ObservableCollection<TableSMItem> _ders;
        private List<NetworkRootViewModel> _roots;
		CIMAdapter adapter = new CIMAdapter();
        private BindableBase historyDataChartVM;
        private BindableBase generationForecastVM;
        private WeatherInfo weather;
        private string weatherIcon;
        private ObservableCollection<TableSMItem> dersToSend = null;
        private TableSMItem derToSend = null;
        private string timeStamp;

        WeatherForecastProxy weatherProxy = new WeatherForecastProxy();

        /// <summary>
		/// Visibility of Tabular Data
		/// </summary>
        private Visibility showData;

        private Visibility showCharts;

        private Visibility showForecast;

        private Visibility weatherWidgetVisible;

        /// <summary>
        /// Simple property to hold the 'ShowTableCommand' - when executed
        /// it will change the current view to the 'Table Data'
        /// </summary>
        public ICommand ShowTableCommand { get; private set; }

        /// <summary>
        /// Simple property to hold the 'ShowChartCommand' - when executed
        /// it will change the current view to the 'History Chart'
        /// </summary>
        public ICommand ShowChartCommand { get; private set; }

        /// <summary>
        /// Simple property to hold the 'ShowForecastCommand' - when executed
        /// it will change the current view to the 'Forecast'
        /// </summary>
        public ICommand ShowForecastCommand { get; private set; }

        public ICommand FilterCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }
        public List<string> TypesForFilter { get; set; }

        private string filterButton;
        private string filterType;
        private bool isFilterEnabled;

        private string searchButton;
        private string searchName;
        private bool isSearchEnabled;

        private List<TableSMItem> tempList = new List<TableSMItem>();

        private double activeMinimum;
        private double activeMaximum;
        private double activeValue;

        private double reactiveMinimum;
        private double reactiveMaximum;
        private double reactiveValue;

        private double activeShareSun;

        private RDAdapter rdAdapter = new RDAdapter();

        private Visibility gaugesVisibility;

        private long selectedGid;

        public EntireNetworkViewModel() 
        {
            _ders = new ObservableCollection<TableSMItem>();
            _roots = new List<NetworkRootViewModel>();
            _roots.Add(new NetworkRootViewModel(_ders));

            ShowCharts = Visibility.Collapsed;
            ShowForecast = Visibility.Collapsed;
            ShowData = Visibility.Visible;
            ShowTableCommand = new RelayCommand(() => ExecuteShowTableCommand());
            ShowChartCommand = new RelayCommand(() => ExecuteShowChartCommand());
            FilterCommand = new RelayCommand(() => ExecuteFilterCommand());
            SearchCommand = new RelayCommand(() => ExecuteSearchCommand());
            //ShowForecastCommand = new RelayCommand(() => ExecuteShowForecastCommand());
            EventSystem.Subscribe<string>(ShowTable);
            EventSystem.Subscribe<long>(ObjectSelected);
            EventSystem.Subscribe<DateTime>(DisplayLastDateTime);
            EventSystem.Subscribe<ObservableCollection<TableSMItem>>(DisplayPowerAndFlexibility);
            EventSystem.Subscribe<TableSMItem>(DisplayPowerAndFlexibility);
            EventSystem.Subscribe<ForecastObjData>(ForecastForObject);
            WeatherWidgetVisible = Visibility.Hidden;
            dersToSend = null;
            derToSend = null;
            TypesForFilter = new List<string>();
            TypesForFilter.Add("Sun");
            TypesForFilter.Add("Wind");
            FilterButton = "Filter";
            SearchButton = "Search";
            IsSearchEnabled = true;
            IsFilterEnabled = true;
            selectedGid = -1;

            ConnectToCalculationEngine();
        }

        #region Properties
        public List<NetworkRootViewModel> Roots
        {
            get { return _roots; }
            set
            {
                _roots = value;
                RaisePropertyChanged("Roots");
            }
        }

        public ObservableCollection<TableSMItem> DERS
        {

            get { return _ders; }
            set
            {
                _ders = value;
                RaisePropertyChanged("DERS");
            }
        }

        public Visibility ShowCharts
        {
            get { return showCharts; }
            set
            {
                showCharts = value;
                RaisePropertyChanged("ShowCharts");
            }
        }

        public Visibility ShowData
        {
            get { return showData; }
            set
            {
                showData = value;
                RaisePropertyChanged("ShowData");
            }
        }

        public BindableBase HistoryDataChartVM
        {
            get { return historyDataChartVM; }
            set
            {
                if (historyDataChartVM == value)
                    return;
                historyDataChartVM = value;
                RaisePropertyChanged("HistoryDataChartVM");
            }
        }

        public WeatherInfo Weather
        {
            get { return weather; }
            set
            {
                weather = value;
                RaisePropertyChanged("Weather");
            }
        }

        public string WeatherIcon
        {
            get
            {
                return weatherIcon;
            }

            set
            {
                weatherIcon = value;
                RaisePropertyChanged("WeatherIcon");
            }
        }

        public Visibility WeatherWidgetVisible
        {
            get
            {
                return weatherWidgetVisible;
            }

            set
            {
                weatherWidgetVisible = value;
                RaisePropertyChanged("WeatherWidgetVisible");
            }
        }

        public Visibility ShowForecast
        {
            get
            {
                return showForecast;
            }

            set
            {
                showForecast = value;
                RaisePropertyChanged("ShowForecast");
            }
        }

        public BindableBase GenerationForecastVM
        {
            get
            {
                return generationForecastVM;
            }

            set
            {
                if (generationForecastVM == value)
                    return;
                generationForecastVM = value;
                RaisePropertyChanged("GenerationForecastVM");
            }
        }

        public string TimeStamp
        {
            get
            {
                return timeStamp;
            }

            set
            {
                if (timeStamp == value)
                    return;
                timeStamp = value;
                RaisePropertyChanged("TimeStamp");
            }
        }

        public string FilterButton
        {
            get
            {
                return filterButton;
            }

            set
            {
                filterButton = value;
                RaisePropertyChanged("FilterButton");
            }
        }

        public string FilterType
        {
            get
            {
                return filterType;
            }

            set
            {
                filterType = value;
                RaisePropertyChanged("FilterType");
            }
        }

        public string SearchButton
        {
            get
            {
                return searchButton;
            }

            set
            {
                searchButton = value;
                RaisePropertyChanged("SearchButton");
            }
        }

        public string SearchName
        {
            get
            {
                return searchName;
            }

            set
            {
                searchName = value;
                RaisePropertyChanged("SearchName");
            }
        }

        public bool IsFilterEnabled
        {
            get
            {
                return isFilterEnabled;
            }

            set
            {
                isFilterEnabled = value;
                RaisePropertyChanged("IsFilterEnabled");
            }
        }

        public bool IsSearchEnabled
        {
            get
            {
                return isSearchEnabled;
            }

            set
            {
                isSearchEnabled = value;
                RaisePropertyChanged("IsSearchEnabled");
            }
        }

        public double ActiveMinimum
        {
            get
            {
                return activeMinimum;
            }

            set
            {
                activeMinimum = value;
                RaisePropertyChanged("ActiveMinimum");
            }
        }

        public double ActiveMaximum
        {
            get
            {
                return activeMaximum;
            }

            set
            {
                activeMaximum = value;
                RaisePropertyChanged("ActiveMaximum");
            }
        }

        public double ActiveValue
        {
            get
            {
                return activeValue;
            }

            set
            {
                activeValue = value;
                RaisePropertyChanged("ActiveValue");
            }
        }

        public double ReactiveMinimum
        {
            get
            {
                return reactiveMinimum;
            }

            set
            {
                reactiveMinimum = value;
                RaisePropertyChanged("ReactiveMinimum");
            }
        }

        public double ReactiveMaximum
        {
            get
            {
                return reactiveMaximum;
            }

            set
            {
                reactiveMaximum = value;
                RaisePropertyChanged("ReactiveMaximum");
            }
        }

        public double ReactiveValue
        {
            get
            {
                return reactiveValue;
            }

            set
            {
                reactiveValue = value;
                RaisePropertyChanged("ReactiveValue");
            }
        }

        public double ActiveShareSun
        {
            get
            {
                return activeShareSun;
            }

            set
            {
                activeShareSun = value;
                RaisePropertyChanged("ActiveShareSun");
            }
        }

        public Visibility GaugesVisibility
        {
            get
            {
                return gaugesVisibility;
            }

            set
            {
                gaugesVisibility = value;
                RaisePropertyChanged("GaugesVisibility");
            }
        }

        private void ShowTable(string command)
        {
            if (command.Equals("ShowTable"))
            {
                ExecuteShowTableCommand();
            }
        }
        #endregion

        private void ExecuteFilterCommand()
        {
            if(tempList.Count==0)
            {
                foreach(TableSMItem item in DERS)
                {
                    tempList.Add(item);
                }
                DERS.Clear();
                foreach(TableSMItem item in tempList)
                {
                    if(FilterType.Equals("Sun"))
                    {
                        if(item.Der.FuelType == FTN.Common.FuelType.Sun)
                        {
                            DERS.Add(item);
                        }
                    }
                    else if (FilterType.Equals("Wind"))
                    {
                        if (item.Der.FuelType == FTN.Common.FuelType.Wind)
                        {
                            DERS.Add(item);
                        }
                    }
                    else
                    {

                    }
                }
                FilterButton = "Cancel Filter";
                IsSearchEnabled = false;
            }
            else
            {
                DERS.Clear();
                foreach (TableSMItem item in tempList)
                {
                    DERS.Add(item);
                }
                tempList.Clear();
                FilterButton = "Filter";
                IsSearchEnabled = true;
            }

            ShowGauges();
        }

        private void ExecuteSearchCommand()
        {
            if (tempList.Count == 0)
            {
                foreach (TableSMItem item in DERS)
                {
                    tempList.Add(item);
                }
                DERS.Clear();
                foreach (TableSMItem item in tempList)
                {
					if (item.Der.Name.Contains(SearchName) || item.Der.Name.ToLower().Contains(SearchName))
					{
						DERS.Add(item);
					}
                }
                SearchButton = "Cancel Search";
                IsFilterEnabled = false;
            }
            else
            {
                DERS.Clear();
                foreach (TableSMItem item in tempList)
                {
                    DERS.Add(item);
                }
                tempList.Clear();
                SearchButton = "Search";
                IsFilterEnabled = true;
            }

            ShowGauges();
        }

        /// <summary>
        /// Set the visibility of DataGrid to 'Visible'
        /// </summary>
        private void ExecuteShowTableCommand()
        {
            ShowData = Visibility.Visible;
            ShowCharts = Visibility.Collapsed;
            ShowForecast = Visibility.Collapsed;
        }

        /// <summary>
        /// Set the visibility of ContentControl to 'Visible'
        /// </summary>
        private void ExecuteShowChartCommand()
        {
            HistoryDataChartVM = new HistoryDataChartViewModel(selectedGid);
            ShowData = Visibility.Collapsed;
            ShowCharts = Visibility.Visible;
            ShowForecast = Visibility.Collapsed;
        }

        private void DisplayLastDateTime(DateTime lastDateTime)
        {
            TimeStamp = lastDateTime.ToString();
        }

        private void DisplayPowerAndFlexibility(ObservableCollection<TableSMItem> _forecastders)
        {
            dersToSend = _forecastders;
            derToSend = null;
        }

        private void DisplayPowerAndFlexibility(TableSMItem _forecastder)
        {
            derToSend = _forecastder;
            dersToSend = null;
        }

        private void ForecastForObject(ForecastObjData d)
        {
            GenerationForecastVM = new GenerationForecastViewModel(d.Gid, d.Power, d.IsGroup, dersToSend, derToSend);
            EventSystem.Publish<bool>(true);
            ShowData = Visibility.Collapsed;
            ShowCharts = Visibility.Collapsed;
            ShowForecast = Visibility.Visible;
        }

        public void ObjectSelected(long gid)
        {
            selectedGid = gid;

            //Ako se nalazimo na istorijskom dijagramo izmeni podatke... radi tree view
            if(ShowCharts == Visibility.Visible)
            {
                HistoryDataChartVM = new HistoryDataChartViewModel(selectedGid);
            }

            new Thread(() =>
            {
               if (gid != -1)
               {
                   WeatherInfo tempWeather = weatherProxy.Proxy.GetCurrentWeatherDataByGlobalId(gid);

                   tempWeather.Currently.Temperature = Math.Round(tempWeather.Currently.Temperature, 2);
                   tempWeather.Currently.WindSpeed = Math.Round(tempWeather.Currently.WindSpeed, 2);
                   tempWeather.Currently.Humidity = Math.Round(tempWeather.Currently.Humidity, 2);
                   tempWeather.Currently.Pressure = Math.Round(tempWeather.Currently.Pressure, 2);

                   if (tempWeather.Currently.Summary.ToUpper().Equals("CLEAR"))
                   {
                       WeatherIcon = @"../Images/WeatherConditionsSunny.png";
                   }
                   else if (tempWeather.Currently.Summary.ToUpper().Equals("OVERCAST") || tempWeather.Currently.Summary.ToUpper().Contains("CLOUD"))
                   {
                       WeatherIcon = @"../Images/WeatherConditionsOvercast.png";
                   }
                   else if (tempWeather.Currently.Summary.ToUpper().Contains("RAIN") || tempWeather.Currently.Summary.ToUpper().Contains("DRIZZLE"))
                   {
                       WeatherIcon = @"../Images/WeatherConditionsRain.png";
                   }
                   else
                   {
                       WeatherIcon = @"../Images/WindIcon.png";
                   }

                   Weather = tempWeather;

                   WeatherWidgetVisible = Visibility.Visible;

                   new Thread(() => ShowGauges()).Start();

               }
               else
               {
                   WeatherWidgetVisible = Visibility.Hidden;
                   new Thread(() => ShowGauges()).Start();
               }
           }).Start();
        }

        public void ShowGauges()
        {
            Thread.Sleep(1000);

            int counter = 0;
            while (true)
            {
                if(DERS.Count != 0)
                {
                    break;
                }
                else
                {
                    if(counter == 5)
                    {
                        break;
                    }

                    counter++;
                    Thread.Sleep(1000);
                }

   
            }
            
            ActiveMinimum = Math.Round(DERS.Sum(o => o.PDecrease), 2);
            ActiveMaximum = Math.Round(DERS.Sum(o => o.PIncrease), 2);
            ActiveValue = Math.Round(DERS.Sum(o => o.CurrentP), 2);

            ReactiveMinimum = Math.Round(DERS.Sum(o => o.QDecrease), 2);
            ReactiveMaximum = Math.Round(DERS.Sum(o => o.QIncrease), 2);
            ReactiveValue = Math.Round(DERS.Sum(o => o.CurrentQ), 2);

            double SunPower = DERS.Where(o => o.Der.FuelType.Equals(FuelType.Sun)).ToList().Sum(o => o.CurrentP);

            if (ActiveValue != 0)
            {
                ActiveShareSun = Math.Round((SunPower / ActiveValue) * 100, 2);
                if(ActiveShareSun > 100)
                {
                    ActiveShareSun = 100;
                }
            }
            else
            {
                ActiveShareSun = 0;
            }
           

            GaugesVisibility = Visibility.Visible;
        }

        public void Refresh()
        {
            Roots = new List<NetworkRootViewModel>() { new NetworkRootViewModel(_ders) };
            Roots[0].IsExpanded = false;

            var dialogBox = new DialogBox(new DialogBoxViewModel("Info!", true, "New delta applied.", 1));
            dialogBox.ShowDialog();
            //Roots.Add(new NetworkRootViewModel(_ders));

        }

        DuplexChannelFactory<IDeltaNotify> factory = null;
        IDeltaNotify proxy = null;

        public void ConnectToCalculationEngine()
        {
            //factory = new DuplexChannelFactory<IDeltaNotify>(
            //  new InstanceContext(this),
            //  new NetTcpBinding(),
            //  new EndpointAddress("net.tcp://localhost:10017/IDeltaNotify"));
            //proxy = factory.CreateChannel();
            //proxy.Register();
        }
    }
}
