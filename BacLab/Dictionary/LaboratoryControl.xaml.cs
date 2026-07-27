using BacLab.Dialogs;
using BacLab.Models;
using Microsoft.Office.Interop.Word;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static BacLab.Models.CommonClass;
using Word = Microsoft.Office.Interop.Word;


namespace BacLab.Dictionary
{
    public partial class LaboratoryControl : UserControl, INotifyPropertyChanged
    {
        BacLab_DBEntities context;
        d_Subdivisions subdivisions;
        d_Staff staff;
        public d_Laboratoria Laboratory { get; set; }
        public ObservableCollection<DictionaryModel> ListItems { get; set; } = new ObservableCollection<DictionaryModel>();
        DictionaryModel oldItem = null;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public LaboratoryControl(BacLab_DBEntities context, d_Subdivisions subdivisions, d_Staff staff)
        {
            try
            {
                InitializeComponent();
                this.context = context;
                this.subdivisions = subdivisions;
                this.staff = staff;
                Laboratory = context.d_Laboratoria.Where(c => c.idSubdivisions == subdivisions.id).FirstOrDefault();
                if (Laboratory == null)
                {
                    Laboratory = new d_Laboratoria()
                    {
                        d_Subdivisions = subdivisions

                    };
                    context.d_Laboratoria.Add(Laboratory);
                    context.SaveChanges();
                }
                x_nameLabGrid.DataContext = Laboratory;

                x_emailTB.Text = Laboratory.email.Trim();
                x_emailParolTB.Text = Laboratory.parol.Trim();
                x_isA5_CB.IsChecked = Laboratory.A5;

                var col = context.d_Template;
                foreach (var item in col)
                {
                    ListItems.Add(new DictionaryModel()
                    {
                        Id = item.id,
                        Name = item.name,
                        Index = (int)item.index
                    });
                }

                x_templateGrid.DataContext = ListItems;

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_MainGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            try
            {
                e.NewItem = new DictionaryModel
                {
                    Id = 0,
                    Index = ListItems.Count() + 1,
                };

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        //Удаление строк. Если есть связи не удаляются
        private void CommandBinding_CanExecuteDelete(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                bool res = false;
                DictionaryModel item = x_templateGrid.SelectedItem as DictionaryModel;
                if (item.Id == 0)
                    oldItem = item;
                else
                {
                    res = DeleteRow(item.Id);
                    if (res == true)
                        oldItem = item;
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }
        }
        private bool DeleteRow(int id)
        {
            try
            {
                BacLab_DBEntities context2 = new BacLab_DBEntities();
                var delItem = context2.d_Template.Where(c => c.id == id).SingleOrDefault();
                context2.d_Template.Remove(delItem);
                context2.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                Message.Ok("Видалити неможливо. Є зв'язки" + "\n" + CommonClass.PrintReferencingEntities(context, typeof(d_Template).Name, id), "MsgDialog");
                return false;
            }
        }
        private void CommandBinding_ExecutedDelete(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (oldItem != null)
                { ListItems.Remove(oldItem); oldItem = null; }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            }

        }

        private void x_tempSaveBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string str = "";
                if (ListItems.Where(c => c.Name == "" || c.Name == null).Count() > 0)
                {
                    str += "Заповніть поле \"Назва\":\n";
                    foreach (var item in ListItems.Where(c => c.Name == "" || c.Name == null))
                        str += "№ " + item.Index + "\n";
                }
                if (str != "")
                { Message.Ok(str, "MsgDialog"); return; }

                bool isNew;
                foreach (var Item in ListItems)
                {
                    isNew = false;
                    d_Template d_Item = context.d_Template.Where(c => c.id == Item.Id).FirstOrDefault();
                    if (d_Item == null)
                    {
                        d_Item = new d_Template();
                        isNew = true;
                    }
                    d_Item.index = Item.Index;
                    d_Item.show = Item.IsShow;
                    d_Item.name = Item.Name;

                    if (isNew)
                        context.d_Template.Add(d_Item);
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_showBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DictionaryModel selectedItem = x_templateGrid.SelectedItem as DictionaryModel;
                if (selectedItem == null) return;
                if (selectedItem.Id == 0)
                {
                    Message.Ok("Бланк не знайдено", "MsgDialog");
                    return;
                }

                string folderMain = Environment.CurrentDirectory;

                byte[] data = context.d_Template.Where(c => c.id == selectedItem.Id).FirstOrDefault().temp;

                if (data != null)
                {
                    // Определяем тип файла
                    var fileType = CommonClass.GetOfficeFileType(data);

                    if (fileType == OfficeFileType.Unknown)
                    {
                        Message.Ok("Помилка: файл не є документом Word або Excel", "MsgDialog");
                        return;
                    }

                    // Определяем расширение на основе типа
                    string extension = fileType == OfficeFileType.Word || fileType == OfficeFileType.WordLegacy
                        ? ".docx"
                        : ".xlsx";

                    string rezultTemplate = Path.Combine(folderMain, selectedItem.Name + extension);
                    using (FileStream fs = new FileStream(rezultTemplate, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(data, 0, data.Length);
                    }

                    // Открываем в соответствующем приложении
                    if (fileType == OfficeFileType.Word || fileType == OfficeFileType.WordLegacy)
                    {
                        OpenWordDocument(rezultTemplate);
                    }
                    else if (fileType == OfficeFileType.Excel || fileType == OfficeFileType.ExcelLegacy)
                    {
                        OpenExcelDocument(rezultTemplate);
                    }
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
            }

        }

        private void x_printBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DictionaryModel selectedItem = x_templateGrid.SelectedItem as DictionaryModel;
                if (selectedItem == null) return;
                if (selectedItem.Id == 0)
                {
                    Message.Ok("Бланк не знайдено", "MsgDialog");
                    return;
                }

                string folderMain = Environment.CurrentDirectory;
                byte[] data = context.d_Template.Where(c => c.id == selectedItem.Id).FirstOrDefault().temp;

                if (data != null)
                {
                    string rezultTemplate = Path.Combine(folderMain, selectedItem.Name + ".docx");
                    using (FileStream fs = new FileStream(rezultTemplate, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(data, 0, data.Length);
                    }

                    Word.Application wordApp = null;
                    Document templateDoc = null;
                    try
                    {
                        wordApp = new Word.Application() { Visible = false };
                        templateDoc = wordApp.Documents.Open(rezultTemplate, ReadOnly: false);
                        if ((bool)Laboratory.A5)
                            templateDoc.PrintOut(true, false, WdPrintOutRange.wdPrintAllDocument,
                                Item: WdPrintOutItem.wdPrintDocumentContent, Copies: "1", Pages: "",
                                PageType: WdPrintOutPages.wdPrintAllPages, PrintToFile: false, Collate: true,
                                ManualDuplexPrint: false, PrintZoomPaperWidth: 8395.2, PrintZoomPaperHeight: 12556.8);
                        else
                            templateDoc.PrintOut(true, false, WdPrintOutRange.wdPrintAllDocument,
                               Item: WdPrintOutItem.wdPrintDocumentContent, Copies: "1", Pages: "",
                               PageType: WdPrintOutPages.wdPrintAllPages, PrintToFile: false, Collate: true,
                               ManualDuplexPrint: false);

                        templateDoc.Close();
                        wordApp.Quit();
                    }
                    catch (Exception ex)
                    {
                        Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
                        templateDoc?.Close();
                        wordApp?.Quit();
                    }
                }
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
            }
        }

        private void x_saveBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DictionaryModel selectedItem = x_templateGrid.SelectedItem as DictionaryModel;
                if (selectedItem == null) return;

                if (selectedItem.Id == 0)
                {
                    d_Template newIem = new d_Template()
                    {
                        index = selectedItem.Index,
                        name = selectedItem.Name
                    };
                    context.d_Template.Add(newIem);
                    context.SaveChanges();
                    selectedItem.Id = newIem.id;
                }

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == true)
                {
                    byte[] data; // сохраняем файл в базе
                    using (FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open))
                    {
                        data = new byte[fs.Length];
                        fs.Read(data, 0, data.Length);
                    }
                    context.d_Template.Where(c => c.id == selectedItem.Id).FirstOrDefault().temp = data;
                    context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
            }

        }

        private void x_emailSaveBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as Button).Name == "x_emailSaveBTN")
                {
                    Laboratory.email = x_emailTB.Text.Trim();
                    Laboratory.parol = x_emailParolTB.Text.Trim();
                }


                else if ((sender as Button).Name == "x_PrintSaveBTN")
                    Laboratory.A5 = (bool)x_isA5_CB.IsChecked;


                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
            }
        }


    }
}