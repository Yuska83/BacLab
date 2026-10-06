using BacLab.Dialogs;
using BacLab.Models;
using BacLab.WorkSpace;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace BacLab.Administration
{
    /// <summary>
    /// Логика взаимодействия для SearchControl.xaml
    /// </summary>
    public partial class SearchControl : UserControl, INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_Staff staff;
        int idGroupreserch;
        IQueryable<d_Analyzes> query;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public SearchControl(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff, int idGroupreserch = -1)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivisions = subdivisions;
                this.staff = staff;
                this.idGroupreserch = idGroupreserch;
                
                if (staff.id != 4) //якщо не я
                    x_Subdivisions.Visibility = Visibility.Hidden;
                x_Subdivisions.ItemsSource = context.d_Subdivisions.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                x_institution.ItemsSource = context.d_Institution.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                x_department.ItemsSource = context.d_Department.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                x_patientStatus.ItemsSource = context.d_PatientStatus.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                x_diagnosis.ItemsSource = context.d_DiagnosisGroup.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                x_sentPerson.ItemsSource = context.d_SentPerson.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                x_material.ItemsSource = context.d_Material.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                x_purpose.ItemsSource = context.d_Purpose.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                x_finance.ItemsSource = context.d_Finance.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                x_jobDistrict.ItemsSource = context.d_District.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                x_jobPlace.ItemsSource = context.d_JobPlace.Where(c => c.show == true).OrderBy(c => c.index).ToList();
                x_job.ItemsSource = context.d_Job.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                x_jobStatus.ItemsSource = context.d_JobStatus.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                x_jobPlaceGroup.ItemsSource = context.d_JobPlaceGroup.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();
                x_whoPay.ItemsSource = context.d_WhoPay.Where(c => c.show == true).OrderBy(c => c.abbr).ToList();

                context.l_log.Add(new l_log() { date = DateTime.Now, datetime = DateTime.Now, idStaff = staff.id, idAction = 8 });

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "SearchDialog");
            }
        }
        public void x_searchBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string x = "";
                string str = "";

                query = context.d_Analyzes.Where(c => c.d_Patients.name.StartsWith(x_name.Text));
                if (!String.IsNullOrEmpty(x_name.Text))
                    str += "Паціент: " + x_name.Text + "; ";

                if (idGroupreserch != -1)
                    query = query.Where(c => c.p_Group_Material_Purpose.idGroup == idGroupreserch);

                if (x_Subdivisions.SelectedItem != null)
                {
                    int id = (x_Subdivisions.SelectedItem as d_Subdivisions).id;
                    query = query.Where(c => c.idSubdivisions == id);
                    str += "Підрозділ: " + (x_Subdivisions.SelectedItem as d_Subdivisions).name + "; ";
                }
                else
                    query = query.Where(c => c.idSubdivisions == subdivisions.id);


                //if (x_brakerage.IsChecked == true)
                //    query = query.Where(c => c.d_Brakerage != null);
                //else
                //    query = query.Where(c => c.d_Brakerage == null);


                if (x_dateDelivery.SelectedDate != null || x_dateDelivery2.SelectedDate != null)
                {
                    if (x_dateDelivery.SelectedDate != null && x_dateDelivery2.SelectedDate != null)
                    {
                        str += "Дата доставки з " + x_dateDelivery.SelectedDate.Value.ToShortDateString() + " по " + x_dateDelivery2.SelectedDate.Value.ToShortDateString() + "; ";
                        query = query.Where(c => c.dateDelivery >= x_dateDelivery.SelectedDate && c.dateDelivery <= x_dateDelivery2.SelectedDate);
                    }

                    else if (x_dateDelivery.SelectedDate != null && x_dateDelivery2.SelectedDate == null)
                    {
                        str += "Дата доставки з " + x_dateDelivery.SelectedDate.Value.ToShortDateString() + "; ";
                        query = query.Where(c => c.dateDelivery == x_dateDelivery.SelectedDate);
                    }
                    else if (x_dateDelivery.SelectedDate == null && x_dateDelivery2.SelectedDate != null)
                    {
                        str += "Дата доставки по " + x_dateDelivery2.SelectedDate.Value.ToShortDateString() + "; ";
                        query = query.Where(c => c.dateDelivery <= x_dateDelivery2.SelectedDate);
                    }


                }

                if (x_dateEnd.SelectedDate != null || x_dateEnd2.SelectedDate != null)
                {
                    if (x_dateEnd.SelectedDate != null && x_dateEnd2.SelectedDate != null)
                    {
                        str += "Дата завершення з " + x_dateEnd.SelectedDate.Value.ToShortDateString() + " по " + x_dateEnd2.SelectedDate.Value.ToShortDateString() + "; ";
                        query = query.Where(c => c.dateEnd >= x_dateEnd.SelectedDate && c.dateEnd <= x_dateEnd2.SelectedDate);
                    }
                    else if (x_dateEnd.SelectedDate != null && x_dateEnd2.SelectedDate == null)
                    {
                        str += "Дата завершення з " + x_dateEnd.SelectedDate.Value.ToShortDateString() + "; ";
                        query = query.Where(c => c.dateEnd == x_dateEnd.SelectedDate);
                    }
                    else if (x_dateEnd.SelectedDate == null && x_dateEnd2.SelectedDate != null)
                    {
                        str += "Дата завершення по " + x_dateEnd2.SelectedDate.Value.ToShortDateString() + "; ";
                        query = query.Where(c => c.dateEnd <= x_dateEnd2.SelectedDate);
                    }

                }

                if (x_purpose.SelectedItem != null)
                {
                    int id = (x_purpose.SelectedItem as d_Purpose).id;
                    query = query.Where(c => c.p_Group_Material_Purpose.idPurpose == id);
                    str += "Мета: " + (x_purpose.SelectedItem as d_Purpose).name + "; ";
                }

                if (x_material.SelectedItem != null)
                {
                    int id = (x_material.SelectedItem as d_Material).id;
                    query = query.Where(c => c.p_Group_Material_Purpose.idMaterial == id);
                    str += "Матеріал: " + (x_material.SelectedItem as d_Material).name + "; ";
                }

                if (x_finance.SelectedItem != null)
                {
                    int id = (x_finance.SelectedItem as d_Finance).id;
                    query = query.Where(c => c.idFinance == id);
                    str += "Фінансування: " + (x_finance.SelectedItem as d_Finance).name + "; ";
                }

                if (x_labNum.Text != "")
                {
                    int id = Convert.ToInt32(x_labNum.Text);
                    query = query.Where(c => c.labNum == id);
                    str += "Лаб№: " + x_labNum.Text + "; ";
                }
                if (x_institution.SelectedItem != null)
                {
                    int id = (x_institution.SelectedItem as d_Institution).id;
                    query = query.Where(c => c.idInstitution == id);
                    str += "Мед.заклад: " + (x_institution.SelectedItem as d_Institution).name + "; ";
                }
                if (x_department.SelectedItem != null)
                {
                    int id = (x_department.SelectedItem as d_Department).id;
                    query = query.Where(c => c.idDepartment == id);
                    str += "Відділення: " + (x_department.SelectedItem as d_Department).name + "; ";
                }
                if (x_diagnosis.SelectedItem != null)
                {
                    int id = (x_diagnosis.SelectedItem as d_DiagnosisGroup).id;
                    query = query.Where(c => c.idDiagnosisGroup == id);
                    str += "Діагноз: " + (x_diagnosis.SelectedItem as d_DiagnosisGroup).name + "; ";
                }
                if (x_patientStatus.SelectedItem != null)
                {
                    int id = (x_patientStatus.SelectedItem as d_PatientStatus).id;
                    query = query.Where(c => c.idPatientStatus == id);
                    str += "Статус пацієнта: " + (x_patientStatus.SelectedItem as d_PatientStatus).name + "; ";
                }
                if (x_numMedCards.Text != "")
                {
                    x = x_numMedCards.Text;
                    query = query.Where(c => c.numMedCard == x);
                    str += "Мед.карта: " + x_numMedCards.Text + "; ";
                }
                if (x_sentPerson.SelectedItem != null)
                {
                    int id = (x_sentPerson.SelectedItem as d_SentPerson).id;
                    query = query.Where(c => c.idSentPerson == id);
                    str += "Направляючий: " + (x_sentPerson.SelectedItem as d_SentPerson).name + "; ";
                }

                if (x_yearOfBirth.Text != "")
                {
                    int id = Convert.ToInt32(x_yearOfBirth.Text);
                    query = query.Where(c => c.d_Patients.year == id);
                    str += "Рік народження: " + x_yearOfBirth.Text + "; ";
                }
                if (x_adress.Text != "")
                {
                    x = x_adress.Text;
                    query = query.Where(c => c.d_Patients.adress.StartsWith(x));
                    str += "Адрес: " + x_adress.Text + "; ";
                }
                if (x_phone.Text != "")
                {
                    x = x_phone.Text;
                    query = query.Where(c => c.d_Patients.phone == x);
                    str += "Телефон: " + x_phone.Text + "; ";
                }

                if (x_comment.Text != "")
                {
                    x = x_comment.Text;
                    query = query.Where(c => c.comment.Contains(x_comment.Text));
                    str += "Коментар: " + x_comment.Text + "; ";
                }
                if (x_jobDistrict.SelectedItem != null)
                {
                    int id = (x_jobDistrict.SelectedItem as d_District).id;
                    query = query.Where(c => c.d_JobPlace.d_District.id == id);
                    str += "Район місця роботи: " + (x_jobDistrict.SelectedItem as d_District).name + "; ";
                }
                if (x_jobPlace.SelectedItem != null)
                {
                    x = x_jobPlace.Text;
                    query = query.Where(c => c.d_JobPlace.abbr.StartsWith(x));
                    str += "Місце роботи: " + x_jobPlace.Text + "; ";
                }
                if (x_job.SelectedItem != null)
                {
                    x = x_job.Text;
                    query = query.Where(c => c.d_Job.abbr.StartsWith(x));
                    str += "Посада: " + x_job.Text + "; ";
                }
                if (x_jobPlaceGroup.SelectedItem != null)
                {
                    int id = (x_jobPlaceGroup.SelectedItem as d_JobPlaceGroup).id;
                    query = query.Where(c => c.idJobPlaceGroup == id);
                    str += "Категорія: " + (x_jobPlaceGroup.SelectedItem as d_JobPlaceGroup).name + "; ";
                }
                if (x_jobStatus.SelectedItem != null)
                {
                    int id = (x_jobStatus.SelectedItem as d_JobStatus).id;
                    query = query.Where(c => c.idJobStatus == id);
                    str += "Статус: " + (x_jobStatus.SelectedItem as d_JobStatus).name + "; ";
                }
                if (x_whoPay.SelectedItem != null)
                {
                    int id = (x_whoPay.SelectedItem as d_WhoPay).id;
                    query = query.Where(c => c.idWhoPay == id);
                    str += "Сплачує: " + (x_whoPay.SelectedItem as d_WhoPay).name + "; ";
                }

                if (x_id.Text != "")
                {
                    int id = Convert.ToInt32(x_id.Text);
                    p_Analises_Mediums medims = context.p_Analises_Mediums.Where(c => c.id == id).FirstOrDefault();
                    query = query.Where(c => c.id == medims.d_Analyzes.id);
                    str += "Id: " + x_id.Text + "; ";
                }

                
                x_count.Text = query.Count().ToString();

                if ((this.Parent as Card)?.DataContext is SearchWindow)
                    ((this.Parent as Card).DataContext as SearchWindow).FillAnalizes(query);
                else if ((this.Parent as Expander).DataContext is WorkJournalWindow)
                    ((this.Parent as Expander).DataContext as WorkJournalWindow).FillAnalizes(query.ToList(), str.Substring(0, str.Length - 2));
                else if ((this.Parent as Expander).DataContext is ProfWindow)
                    ((this.Parent as Expander).DataContext as ProfWindow).FillAnalizesReady(query.ToList());

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "SearchDialog");
            }

        }

        private void x_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                    x_searchBTN_Click(this, new RoutedEventArgs());

                if (e.Key == Key.Delete && sender is ComboBox)
                    (sender as ComboBox).SelectedItem = null;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "SearchDialog");
            }
        }

        private void x_jobDistrict_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if ((sender as ComboBox).SelectedItem == null)
                    x_jobPlace.ItemsSource = null;
                else
                {
                    int idDistrict = (x_jobDistrict.SelectedItem as d_District).id;
                    x_jobPlace.ItemsSource = context.d_JobPlace.Where(c => c.idDistrict == idDistrict && c.show == true).OrderBy(c => c.abbr).ToList();
                }

            }
            catch (Exception)
            {

                throw;
            }

        }

        private void PrintJournal_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Excel.Application excel = new Excel.Application() { Visible = false };
                Excel.Workbook newDoc = excel.Workbooks.Add();
                try
                {
                    Excel.Worksheet sheet = (Excel.Worksheet)excel.Worksheets.get_Item(1);
                    Excel.Range xlRange = sheet.UsedRange;
                    int i = 1;
                    xlRange.Cells[1, i++] = "№";
                    xlRange.Cells[1, i++] = "Лаб.номер";
                    xlRange.Cells[1, i++] = "Дата взяття зразку";
                    xlRange.Cells[1, i++] = "Дата доставки";
                    xlRange.Cells[1, i++] = "Мед.заклад";
                    xlRange.Cells[1, i++] = "Відділення";
                    xlRange.Cells[1, i++] = "Пацієнт";
                    xlRange.Cells[1, i++] = "Рік народження";
                    xlRange.Cells[1, i++] = "Вік";
                    xlRange.Cells[1, i++] = "Діагноз";
                    xlRange.Cells[1, i++] = "Мед.карта";
                    xlRange.Cells[1, i++] = "Статус";
                    xlRange.Cells[1, i++] = "Біоматеріал";
                    xlRange.Cells[1, i++] = "Додатково";
                    xlRange.Cells[1, i++] = "Мета";
                    //xlRange.Cells[1, i++] = "Первинна госпіталізація";
                    //xlRange.Cells[1, i++] = "Чи було бак.обстеження";
                    //xlRange.Cells[1, i++] = "Перебування в стаціонарі < 48 год.";
                    //xlRange.Cells[1, i++] = "Після поранення пройшло <72 год.";
                    //xlRange.Cells[1, i++] = "АБ 1 на догоспітальному етапі";
                    //xlRange.Cells[1, i++] = "АБ 2 на догоспітальному етапі";
                    //xlRange.Cells[1, i++] = "АБ 1 в поточному закладі";
                    //xlRange.Cells[1, i++] = "АБ 2 в поточному закладі";
                    xlRange.Cells[1, i++] = "Видан";
                    xlRange.Cells[1, i++] = "Дата завершення";
                    xlRange.Cells[1, i++] = "Результат";
                    xlRange.Cells[1, i++] = "Культури";
                    xlRange.Cells[1, i++] = "АБ";
                    xlRange.Cells[1, i++] = "Чутливість";

                    int num = 1;
                    int column = 1;
                    int row = 1;
                    foreach (var item in query)
                    {
                        row++;
                        column = 1;
                        xlRange.Cells[row, column++] = num++;
                        xlRange.Cells[row, column++] = item.labNum;
                        xlRange.Cells[row, column++] = item.dateSampling;
                        xlRange.Cells[row, column++] = item.dateDelivery;
                        xlRange.Cells[row, column++] = item.d_Institution?.name;
                        xlRange.Cells[row, column++] = item.d_Department?.name;
                        xlRange.Cells[row, column++] = item.d_Patients?.name;
                        xlRange.Cells[row, column++] = item.d_Patients?.year;
                        xlRange.Cells[row, column++] = item.agePatient;
                        xlRange.Cells[row, column++] = item.diagnosis;
                        xlRange.Cells[row, column++] = item.numMedCard;
                        xlRange.Cells[row, column++] = item.d_PatientStatus.name;
                        xlRange.Cells[row, column++] = item.p_Group_Material_Purpose.d_Material?.name;
                        xlRange.Cells[row, column++] = item.comment;
                        xlRange.Cells[row, column++] = item.p_Group_Material_Purpose.d_Purpose?.name;
                        //xlRange.Cells[row, column++] = item.isFirstGospitelisation == true ? "так" : "ні";
                        //xlRange.Cells[row, column++] = item.wasPreviousBac == true ? "так" : "ні";
                        //xlRange.Cells[row, column++] = item.do48 == true ? "так" : "ні";
                        //xlRange.Cells[row, column++] = item.do72 == false ? "так" : "ні";
                        //xlRange.Cells[row, column++] = item.a_Antibiotic?.name;
                        //xlRange.Cells[row, column++] = item.a_Antibiotic1?.name;
                        //xlRange.Cells[row, column++] = item.a_Antibiotic2?.name;
                        //xlRange.Cells[row, column++] = item.a_Antibiotic3?.name;
                        xlRange.Cells[row, column++] = item.isEnd == true ? "так" : "ні";
                        xlRange.Cells[row, column++] = item.dateEnd;
                        xlRange.Cells[row, column++] = item.d_ResTemplate?.name;

                        foreach (var itemMO in item.p_Analises_Cultures)
                        {
                            int colMO = column;
                            xlRange.Cells[row, colMO] = itemMO.d_Microorganism.name;

                            foreach (var itemAB in itemMO.p_Analises_Cultures_ABTest.OrderBy(c => c.index))
                            {
                                xlRange.Cells[row, colMO + 1] = itemAB.d_TestAndAntibiotic?.name;
                                if (itemAB.ferment != true && itemAB.fag != true && itemAB.sinergizm != true && itemAB.comment != true)
                                    xlRange.Cells[row, colMO + 2] = itemAB.pm.Trim() == "+" ? "чутливий" : (itemAB.pm.Trim() == "-" ? "стійкий" : "проміжний");
                                else if (itemAB.ferment == true)
                                    xlRange.Cells[row, colMO + 2] = itemAB.pm.Trim().Equals("+") ? "продукує" : itemAB.pm.Trim().Equals("-") ? "не продукує" : "не визначалось";
                                else if (itemAB.sinergizm == true)
                                    xlRange.Cells[row, colMO + 2] = itemAB.pm.Trim().Equals("+") ? "спостерігається" : "не спостерігається";
                                else if (itemAB.fag == true)
                                    xlRange.Cells[row, colMO + 2] = itemAB.pm.Trim();
                                row++;
                            }
                            row--;
                        }
                    }

                    column--;

                    Excel.Range y1 = sheet.Cells[1, 1];
                    Excel.Range y2 = sheet.Cells[row, 21];
                    sheet.get_Range(y1, y2).Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                    sheet.get_Range(y1, y2).Columns.AutoFit();
                    sheet.Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    sheet.Cells.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                    excel.Visible = true;
                    excel.WindowState = Excel.XlWindowState.xlMinimized;
                    excel.WindowState = Excel.XlWindowState.xlMaximized;
                }
                catch (Exception ex)
                {
                    Message.Ok(ex.Message + "\n" + ex.StackTrace, "SearchDialog");
                    newDoc?.Close(SaveChanges: false);
                    excel?.Quit();
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "SearchDialog");
            }
        }

        private void x_UploadFileTerraBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            { 

                foreach (var item in query)
                    item.isSendToTerra = false;
                context.SaveChanges();
                string str = CommonClass.UploadFileTerra(context);
                Message.Ok(str, "SearchDialog");
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "SearchDialog");
            }
        }

        private void x_institution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if ((x_institution.SelectedItem as d_Institution)?.id == 26)
                    x_UploadFileTerraBTN.Visibility = Visibility.Visible;
                else x_UploadFileTerraBTN.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "SearchDialog");
            }
        }
    }
}
