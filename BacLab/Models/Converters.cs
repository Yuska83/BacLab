using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace BacLab.Models
{
    public class ControlDateHighlightConverter : IValueConverter
    {
        public HashSet<DateTime> ControlDates { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var btn = value as CalendarDayButton;
            if (btn?.DataContext is DateTime date && ControlDates != null)
                return ControlDates.Contains(date.Date);
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class DateTimeToDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            return ((DateTime)value).ToString("dd.MM.yyyy");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is String)
                {
                    if (String.IsNullOrEmpty((String)value)) return null;
                    return System.Convert.ToDateTime(value.ToString());
                }
                return ((DateTime)value).ToString("dd.MM.yyyy");
            }
            catch (Exception)
            {
                MessageBox.Show("Невірний формат дати");
                return null;

            }
        }
    }

    public class DateTimeToDateShotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            return ((DateTime)value).Date.ToShortDateString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is String @string)
                {
                    if (@string == String.Empty) return null;
                    return System.Convert.ToDateTime(value.ToString());
                }
                else return ((DateTime)value).Date.ToShortDateString();
            }
            catch (Exception)
            {
                MessageBox.Show("Невірний формат дати");
                return null;

            }

        }
    }

    public class DateTimeToDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            return ((DateTime)value).ToString("g");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is String)
                {
                    if (String.IsNullOrEmpty((String)value)) return null;
                    return System.Convert.ToDateTime(value.ToString());
                }
                return ((DateTime)value).ToString("g");
            }
            catch (Exception)
            {
                MessageBox.Show("Невірний формат дати");
                return null;

            }
        }
    }

    public class EmailConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "ні";
            return (bool)value == true ? "так" : "ні";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is String @string)
            {
                if (@string.Equals("так", StringComparison.OrdinalIgnoreCase))
                    return true;
                else if (@string.Equals("ні", StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            return false;
        }
    }

    public class BoolToPlusMinusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            return (bool)value == true ? "+" : "-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityBackConvector : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && ((bool)value) ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && ((bool)value) ? Visibility.Hidden : Visibility.Visible;

        }
    }

    public class BoolToVisibilityBackConvector2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && ((bool)value) ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && ((bool)value) ? Visibility.Visible : Visibility.Hidden;

        }
    }

    public class ObjectToVisibilityConvector : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CountToVisibilityConvector : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            d_Analyzes analis = value as d_Analyzes;
            int count = analis.d_Patients.d_Analyzes.Where(c => c.id != analis.id).ToList().Count;
            return count > 0 ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CountToEnabledConvector : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count = (int)value;
            return count > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class QuantityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null) return value.ToString().Trim();
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return value;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value.ToString() == "") return null;
                return value;
            }
            catch (Exception)
            {
                return null;
            }
        }


    }
    public class DiapazonConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string name;

            switch ((string)parameter)
            {
                case "Diapazontemperature":
                    name = values[0] + "-" + values[1];
                    break;
                default:
                    name = values[0] + "-" + values[1];
                    break;
            }

            return name;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            var splitValues = ((string)value).Split('-');
            return splitValues;
        }
    }
    public class AbSeriesStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ABControls abControls = value as ABControls;
            if (abControls == null) return null;
            return abControls.ABSeries.d_Producer.abbr + " c." + abControls.ABSeries.series + " до " + abControls.ABSeries.termin.Value.ToShortDateString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AbSeriesStringConverter2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            p_Analises_Mediums_Date_Colonies_AB ab = value as p_Analises_Mediums_Date_Colonies_AB;
            if (ab == null) return null;
            return ab.d_Consumables?.name + "\n" + ab.d_ConsumablesStock?.d_Producer?.abbr + "\nc."
                + ab.d_ConsumablesStock?.series + "  до " + ab.d_ConsumablesStock?.termin.Value.ToShortDateString()
                + "\n" + ab.commentABControl;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SerumSeriesStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            d_SerumStock serumStock = value as d_SerumStock;
            if (serumStock == null) return null;
            return serumStock.d_Serum?.abbr + "\n" + serumStock.d_Producer.abbr + " c." + serumStock.series + " до " + serumStock.termin.Value.ToShortDateString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TestSeriesStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            d_TestAndAntibiotic testStock = value as d_TestAndAntibiotic;
            if (testStock == null) return null;
            return testStock.name;
            //return testStock.d_Tests?.abbr + "\n" + testStock.d_Producer.abbr + " c." + testStock.series + " до " + testStock.termin.Value.ToShortDateString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AnalisStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            d_Analyzes analis = value as d_Analyzes;
            if (analis == null) return null;
            string str = "дата забору: " + analis.dateSampling.Value.ToShortDateString() + "   дата доставки: " + analis.dateDelivery.Value.ToShortDateString();
            string sex = analis.d_Patients?.sex == "ч" ? "чоловік" : analis.d_Patients?.sex == "ж" ? "жінка" : " ";
            str += "\nпаціент: " + analis.d_Patients?.name + "  " + analis.d_Patients?.year + " р.н.  " + analis.agePatient + "р.  " + sex + " " + analis.d_JobPlace?.abbr + "  " + analis.d_Job?.abbr;
            if (analis.p_Group_Material_Purpose.d_GroupResearch.id != 4)
                str += "\nмедзаклад: " + analis.d_Institution?.abbr + "   відділення: " + analis.d_Department?.abbr + "   діагноз: " + analis.d_Diagnosis?.abbr + "   статус: " + analis.d_PatientStatus?.abbr + "   направив: " + analis.d_SentPerson?.abbr;
            str += "\nреєстратор: " + analis.d_Staff1?.abbr;
            if (analis.d_Staff != null)
                str += "   видан: " + analis.d_Staff?.abbr;
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EquipmentStateStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            InnerControl innerControl = value as InnerControl;
            return innerControl.Equipment?.d_EquipmentState?.abbr;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DragMetalStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? "Є" : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TargetMMConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            a_AntibioticControl abControls = value as a_AntibioticControl;
            if (abControls == null) return null;
            string str = abControls.valueTargetMax != null ? "-" + abControls.valueTargetMax.ToString() : "";
            return abControls.valueTargetMin + str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PermissiblemMMConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            a_AntibioticControl abControls = value as a_AntibioticControl;
            if (abControls == null) return null;
            string str = abControls.valuePermissiblemMax != null ? "-" + abControls.valuePermissiblemMax.ToString() : "";
            return abControls.valuePermissiblemMin + str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return (bool)value == true ? "проводилось" : "не проводилось";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return (bool)value == true ? Brushes.White : Brushes.LightGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            return (bool)value ? "+" : "-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value as String)) return null;
            return ((string)value).Equals("+");
        }
    }

    public class CountToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            HashSet<p_Analises_Cultures> set = value as HashSet<p_Analises_Cultures>;
            if (set.Count > 0) return true;
            else return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CulturesToStringlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            HashSet<p_Analises_Cultures> set = value as HashSet<p_Analises_Cultures>;
            string str = "";
            if (set.Count > 0)
                foreach (var item in set)
                    str += item.d_Microorganism.abbr + " " + item.quantity + "\n";

            return str.Trim();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class SafeDictionaryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Dictionary<int, string> dict && parameter is int key)
            {
                return dict.TryGetValue(key, out var result) ? result : string.Empty;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
   
}
