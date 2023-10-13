using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SlabOnGradient.Infrastructure;

namespace SlabOnGradient.ViewModels
{
    internal class MainWindowViewModel : Base.ViewModel
    {
        private RevitModelForfard _revitModel;

        internal RevitModelForfard RevitModel
        {
            get => _revitModel;
            set => _revitModel = value;
        }

        #region Заголовок
        private string _title = "Плита на уклоне";

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }
        #endregion


        #region Элементы оси трассы
        private string _roadAxisElemIds;

        public string RoadAxisElemIds
        {
            get => _roadAxisElemIds;
            set => Set(ref _roadAxisElemIds, value);
        }
        #endregion

        #region Элементы линии на поверхности 1
        private string _roadLineElemIds1;

        public string RoadLineElemIds1
        {
            get => _roadLineElemIds1;
            set => Set(ref _roadLineElemIds1, value);
        }
        #endregion

        #region Элементы линии на поверхности 2
        private string _roadLineElemIds2;

        public string RoadLineElemIds2
        {
            get => _roadLineElemIds2;
            set => Set(ref _roadLineElemIds2, value);
        }
        #endregion

        #region Элементы границы плиты
        private string _borderSlabElemIds;

        public string BorderSlabElemIds
        {
            get => _borderSlabElemIds;
            set => Set(ref _borderSlabElemIds, value);
        }
        #endregion

        #region Толщина покрытия
        private double _coatingThikness = Properties.Settings.Default.CoatingThikness;

        public double CoatingThikness
        {
            get => _coatingThikness;
            set => Set(ref _coatingThikness, value);
        }
        #endregion

        #region Шаг разбивки
        private double _step = Properties.Settings.Default.Step;

        public double Step
        {
            get => _step;
            set => Set(ref _step, value);
        }
        #endregion

        #region Команды

        #region Получение оси трассы
        public ICommand GetRoadAxis { get; }

        private void OnGetRoadAxisCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.GetPolyCurve();
            RoadAxisElemIds = RevitModel.RoadAxisElemIds;
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetRoadAxisCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Получение линии на поверхности дороги 1
        public ICommand GetRoadLines1 { get; }

        private void OnGetRoadLines1CommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.GetRoadLine1();
            RoadLineElemIds1 = RevitModel.RoadLineElemIds1;
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetRoadLines1CommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Получение линии на поверхности дороги 2
        public ICommand GetRoadLines2 { get; }

        private void OnGetRoadLines2CommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.GetRoadLine2();
            RoadLineElemIds2 = RevitModel.RoadLineElemIds2;
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetRoadLines2CommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Получение линий границ плиты
        public ICommand GetBorderSlabCommand { get; }

        private void OnGetBorderSlabCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.GetBorderSlabLines();
            BorderSlabElemIds = RevitModel.BorderSlabLinesElemIds;
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetBorderSlabCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Создать плиту на уклоне
        public ICommand CreateSlabCommand { get; }

        private void OnCreateSlabCommandExecuted(object parameter)
        {
            RevitModel.CreateSlabOnGradient(CoatingThikness, Step);
            SaveSettings();
            RevitCommand.mainView.Close();
        }

        private bool CanCreateSlabCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Закрыть окно
        public ICommand CloseWindowCommand { get; }

        private void OnCloseWindowCommandExecuted(object parameter)
        {
            SaveSettings();
            RevitCommand.mainView.Close();
        }

        private bool CanCloseWindowCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #endregion

        private void SaveSettings()
        {
            Properties.Settings.Default.RoadAxisElemIds = RoadAxisElemIds;
            Properties.Settings.Default.RoadLineElemIds1 = RoadLineElemIds1;
            Properties.Settings.Default.RoadLineElemIds2 = RoadLineElemIds2;
            Properties.Settings.Default.BorderSlabElemIds = BorderSlabElemIds;
            Properties.Settings.Default.CoatingThikness = CoatingThikness;
            Properties.Settings.Default.Step = Step;
            Properties.Settings.Default.Save();
        }


        #region Конструктор класса MainWindowViewModel
        public MainWindowViewModel(RevitModelForfard revitModel)
        {
            RevitModel = revitModel;

            #region Инициализация значений из Settings

            #region Инициализация значения элементам оси из Settings
            if (!(Properties.Settings.Default.RoadAxisElemIds is null))
            {
                string axisElementIdInSettings = Properties.Settings.Default.RoadAxisElemIds;
                if (RevitModel.IsLinesExistInModel(axisElementIdInSettings) && !string.IsNullOrEmpty(axisElementIdInSettings))
                {
                    RoadAxisElemIds = axisElementIdInSettings;
                    RevitModel.GetAxisBySettings(axisElementIdInSettings);
                }
            }
            #endregion

            #region Инициализация значения элементам линии на поверхности 1
            if (!(Properties.Settings.Default.RoadLineElemIds1 is null))
            {
                string line1ElementIdInSettings = Properties.Settings.Default.RoadLineElemIds1;
                if (RevitModel.IsLinesExistInModel(line1ElementIdInSettings) && !string.IsNullOrEmpty(line1ElementIdInSettings))
                {
                    RoadLineElemIds1 = line1ElementIdInSettings;
                    RevitModel.GetRoadLines1BySettings(line1ElementIdInSettings);
                }
            }
            #endregion

            #region Инициализация значения элементам линии на поверхности 2
            if (!(Properties.Settings.Default.RoadLineElemIds2 is null))
            {
                string line2ElementIdInSettings = Properties.Settings.Default.RoadLineElemIds2;
                if (RevitModel.IsLinesExistInModel(line2ElementIdInSettings) && !string.IsNullOrEmpty(line2ElementIdInSettings))
                {
                    RoadLineElemIds2 = line2ElementIdInSettings;
                    RevitModel.GetRoadLines2BySettings(line2ElementIdInSettings);
                }
            }
            #endregion

            #region Инициализация значения границ плиты
            if (!(Properties.Settings.Default.BorderSlabElemIds is null))
            {
                string borderSlabElemIdInSettings = Properties.Settings.Default.BorderSlabElemIds;
                if (RevitModel.IsBoundLinesExistInModel(borderSlabElemIdInSettings) && !string.IsNullOrEmpty(borderSlabElemIdInSettings))
                {
                    BorderSlabElemIds = borderSlabElemIdInSettings;
                    RevitModel.GetBorderSlabBySettings(borderSlabElemIdInSettings);
                }
            }
            #endregion

            #endregion

            #region Команды

            GetRoadAxis = new LambdaCommand(OnGetRoadAxisCommandExecuted, CanGetRoadAxisCommandExecute);

            GetRoadLines1 = new LambdaCommand(OnGetRoadLines1CommandExecuted, CanGetRoadLines1CommandExecute);

            GetRoadLines2 = new LambdaCommand(OnGetRoadLines2CommandExecuted, CanGetRoadLines2CommandExecute);

            GetBorderSlabCommand = new LambdaCommand(OnGetBorderSlabCommandExecuted, CanGetBorderSlabCommandExecute);

            CreateSlabCommand = new LambdaCommand(OnCreateSlabCommandExecuted, CanCreateSlabCommandExecute);

            CloseWindowCommand = new LambdaCommand(OnCloseWindowCommandExecuted, CanCloseWindowCommandExecute);

            #endregion
        }

        public MainWindowViewModel() { }
        #endregion
    }
}
