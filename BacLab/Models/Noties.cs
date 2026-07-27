using BacLab.Dialogs;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Drawing;
using System.Linq;
using System.Windows;
using ZXing;
using Excel = Microsoft.Office.Interop.Excel;

namespace BacLab.Models
{
    internal class Noties
    {
        BacLab_DBEntities context;
        d_Staff staff;
        d_Subdivisions subdivisions;
        string parol;

        private void fillWriteoff()
        {
            //try
            //{
            //    //var col = context.d_ConsumableWritingOff.Where(c => c.d_ConsumablesStock.idSubdivisions == subdivisions.id).ToList();
            //    //foreach (var itemDel in col)
            //    //{
            //    //    context.d_ConsumableWritingOff.Remove(itemDel);
            //    //}
            //    //context.SaveChanges();

            //    //var colAB = context.a_Antibiotic.Where(c => c.show == true
            //    //&& c.average != null && c.idPeriod == 1 && c.d_ConsumablesGroup.id == 1).OrderBy(c => c.abbr).ToList();

            //    DateTime dateControlNext;
            //    DateTime dateControl;
            //    DateTime dateControlfirst;
            //    DateTime dateControllast;
            //    d_Units units = context.d_Units.Where(c => c.id == 6).FirstOrDefault();
            //    d_Staff staff = context.d_Staff.Where(c => c.id == 4).FirstOrDefault();
            //    d_ConsumablesStock item;
            //    d_ConsumablesStock itemNext;

            //    string str = "";
            //    foreach (var ab in colAB)
            //    {

            //        Console.WriteLine(ab.abbr);

            //        var colConsumableStock = context.d_ConsumablesStock.Where(c => c.a_Antibiotic.id == ab.id && c.idSubdivisions == 1).OrderBy(c => c.dateDelivery).ToList();

            //        int avarage = ab.average != null ? Convert.ToInt32(ab.average) : 0;


            //        int months = 0;
            //        int quantyty = 0;
            //        if (avarage > 0 && ab.idPeriod == 1)
            //        {
            //            for (int i = 0; colConsumableStock.Count > i; i++)
            //            {
            //                item = colConsumableStock[i];
            //                itemNext = i + 1 < colConsumableStock.Count ? colConsumableStock[i + 1] : null;
            //                item.d_Units = units;

            //                if (item.a_AntibioticControl.Where(c => c.isEnterControl == true).FirstOrDefault() == null)
            //                {
            //                    str += item.a_Antibiotic.abbr + "\n";
            //                    continue;
            //                }

            //                months = 0;
            //                if (itemNext != null)
            //                {
            //                    dateControlNext = itemNext.a_AntibioticControl.Where(c => c.isEnterControl == true).FirstOrDefault().date;
            //                    dateControl = item.a_AntibioticControl.Where(c => c.isEnterControl == true).FirstOrDefault().date;
            //                    months = dateControlNext - dateControl != null ? ((dateControlNext.Year - dateControl.Year) * 12 + dateControlNext.Month - dateControl.Month) : 0;
            //                    if (months < 1) months = 1;
            //                }
            //                else
            //                    months = 3;

            //                quantyty = months * avarage;
            //                if (quantyty % 5 != 0)
            //                {
            //                    quantyty += 5 - (quantyty % 5);
            //                }
            //                item.quantityWas = quantyty;
            //                item.quantityBecame = quantyty;

            //                //+ надходження, яке відповідає кількості, яка була додана, а також даті надходження, яку вказали. Це потрібно для того, щоб потім при списанні правильно відображалась кількість і дата надходження
            //                item.d_ConsumableWritingOff.Add(new d_ConsumableWritingOff
            //                {
            //                    date = item.dateDelivery,
            //                    quantity = 0,
            //                    quantityWas = quantyty,
            //                    quantityBecame = quantyty,
            //                    isPlus = true,
            //                    d_Units = item.d_Units,
            //                    d_Staff = staff
            //                });

            //                if (item.conclusion.Equals("не придатно"))
            //                {
            //                    item.quantityBecame = 0;
            //                    item.isEnd = true;
            //                    item.dateEnd = item.a_AntibioticControl.Where(c => c.isEnterControl == true).FirstOrDefault().date;

            //                    item.d_ConsumableWritingOff.Add(new d_ConsumableWritingOff
            //                    {
            //                        date = item.a_AntibioticControl.Where(c => c.isEnterControl == true).FirstOrDefault().date,
            //                        quantity = item.quantityWas,
            //                        quantityWas = item.quantityWas,
            //                        quantityBecame = 0,
            //                        isPlus = false,
            //                        d_Units = item.d_Units,
            //                        d_Staff = staff
            //                    });
            //                    continue;
            //                }

            //                if (item.a_AntibioticControl.Where(c => c.isEnterControl != true).OrderBy(c => c.date).FirstOrDefault() == null)
            //                    continue;

            //                if (item.show == true)
            //                {
            //                    dateControlfirst = item.a_AntibioticControl.Where(c => c.isEnterControl != true).OrderBy(c => c.date).FirstOrDefault().date;
            //                    int periodNow = (int)(DateTime.Now - dateControlfirst).TotalDays;
            //                    if (periodNow > 0)
            //                    {
            //                        int periodRoznosaAverage = (int)(31 / ab.average);
            //                        for (int j = 1; j <= item.quantityWas; j++)
            //                        {
            //                            item.d_ConsumableWritingOff.Add(new d_ConsumableWritingOff
            //                            {
            //                                date = dateControlfirst,
            //                                quantity = 1,
            //                                quantityWas = item.quantityBecame,
            //                                quantityBecame = item.quantityBecame - 1,
            //                                isPlus = false,
            //                                d_Units = units,
            //                                d_Staff = staff
            //                            });
            //                            item.quantityBecame -= 1;
            //                            if (item.quantityBecame <= 0)
            //                            {
            //                                item.quantityBecame = 0;
            //                                item.isEnd = true;
            //                                item.dateEnd = dateControlfirst;
            //                                break;
            //                            }
            //                            dateControlfirst = dateControlfirst.AddDays(periodRoznosaAverage);
            //                            if (dateControlfirst > DateTime.Now)
            //                            {
            //                                break;
            //                            }
            //                        }

            //                    }
            //                    continue;
            //                }



            //                dateControlfirst = item.a_AntibioticControl.Where(c => c.isEnterControl != true).OrderBy(c => c.date).FirstOrDefault().date;
            //                dateControllast = item.a_AntibioticControl.Where(c => c.isEnterControl != true).OrderByDescending(c => c.date).FirstOrDefault().date;
            //                var period = (dateControllast - dateControlfirst).TotalDays;
            //                int periodRoznosa = (int)(period / item.quantityWas);

            //                for (int j = 1; j <= item.quantityWas; j++)
            //                {
            //                    item.d_ConsumableWritingOff.Add(new d_ConsumableWritingOff
            //                    {
            //                        date = dateControlfirst,
            //                        quantity = 1,
            //                        quantityWas = item.quantityBecame,
            //                        quantityBecame = item.quantityBecame - 1,
            //                        isPlus = false,
            //                        d_Units = units,
            //                        d_Staff = staff
            //                    });

            //                    item.quantityBecame -= 1;

            //                    if (item.quantityBecame <= 0)
            //                    {
            //                        item.quantityBecame = 0;
            //                        item.isEnd = true;
            //                        item.dateEnd = dateControlfirst;
            //                        break;
            //                    }

            //                    dateControlfirst = dateControlfirst.AddDays(periodRoznosa);
            //                }

            //            }
            //        }
            //    }

            //    context.SaveChanges();
            //    Console.WriteLine(str);

            //}
            //catch (Exception ex)
            //{
            //    Message.Ok(ex.Message + " " + ex.StackTrace, "MsgDialog");
            //}
        }


        public void NewABControls()
        {
            Random rnd = new Random();
            DateTime dateStart;
            DateTime dateEnd;

            var colAB1 = context.d_ConsumablesStock.Where(c => c.idSubdivisions == 1).GroupBy(c => c.d_Consumables).OrderBy(c => c.Key.id)
                .ToList();


            foreach (var ab1 in colAB1)
            {

                Console.WriteLine("Антибіотик: " + ab1.Key.name);
                List<a_AntibioticControl> col = context.a_AntibioticControl.Where(c => c.idSubdivisions == 1 && c.d_ConsumablesStock.idConsumable == ab1.Key.id).ToList();
                context.a_AntibioticControl.RemoveRange(col);
                context.SaveChanges();
                Console.WriteLine("видалення завершено");

                int x = 0;
                var colAB = context.d_ConsumablesStock.Where(c => c.idSubdivisions == 1 && c.idConsumable == ab1.Key.id && c.conclusion.Equals("придатно") && c.show == true).GroupBy(c => c.d_Consumables).ToList();
                foreach (var ab in colAB)
                {
                    if (ab.Count() > 1)
                    {
                        Console.WriteLine("Антибіотик: " + ab.Key.name + ab1.Key.id
                                + " Кількість серій: " + ab.Count().ToString());
                        var colSeries = ab.OrderBy(c => c.dateDelivery).ToList();
                        var colNormsCulture = context.a_AntibioticNorms.Where(c => c.idConsumable == ab1.Key.id).ToList();

                        for (int i = 0; i < colSeries.Count; i++)
                        {

                            dateStart = colSeries[i].dateDelivery.Value.AddDays(30);
                            do
                            {
                                dateStart = dateStart.AddDays(1);
                            } while (dateStart.DayOfWeek != DayOfWeek.Wednesday);

                            if (i + 1 == colSeries.Count)
                            {
                                if (colSeries[i].show == false)
                                    dateEnd = (DateTime)colSeries[i].termin;
                                else dateEnd = DateTime.Now;
                            }

                            else
                                dateEnd = colSeries[i + 1].dateDelivery.Value.AddDays(30);
                            do
                            {
                                dateEnd = dateEnd.AddDays(1);
                            } while (dateEnd.DayOfWeek != DayOfWeek.Wednesday);

                            DateTime abControlDate = dateStart;

                            while (dateStart < dateEnd && dateStart < DateTime.Now)
                            {
                                if (dateStart.DayOfWeek == DayOfWeek.Wednesday)
                                {

                                    foreach (var itemNorms in colNormsCulture)
                                    {
                                        int value = rnd.Next((int)itemNorms.valueTargetMin - 1, (itemNorms.valueTargetMax == null ? (int)itemNorms.valueTargetMin + 1 : (int)itemNorms.valueTargetMax + 1));
                                        a_AntibioticControl a_AntibioticControl = context.a_AntibioticControl.Add(new a_AntibioticControl()
                                        {
                                            date = dateStart,
                                            d_ConsumablesStock = colSeries[i],
                                            d_Microorganism = itemNorms.d_Microorganism,
                                            valueCurrent = value,
                                            valuePermissiblemMax = itemNorms.valuePermissiblemMax,
                                            valuePermissiblemMin = itemNorms.valuePermissiblemMin,
                                            valueTargetMax = itemNorms.valueTargetMax,
                                            valueTargetMin = itemNorms.valueTargetMin,
                                            d_Subdivisions = subdivisions,

                                        });
                                        x++;

                                    }
                                    abControlDate = dateStart;

                                }

                                var Colonies_AB = context.p_Analises_Mediums_Date_Colonies_AB.Where(c => c.p_Analises_Mediums_Date_Colonies.p_Analises_Mediums_Date.date == dateStart && c.idConsumable == ab1.Key.id).ToList();

                                foreach (var item in Colonies_AB)
                                {
                                    item.d_ConsumablesStock = colSeries[i];
                                    item.commentABControl = "Контроль: " + abControlDate.ToShortDateString();
                                }

                                dateStart = dateStart.AddDays(1);

                            }
                            context.SaveChanges();
                            Console.WriteLine(" збережено: " + i.ToString() + " серія " + x.ToString() + " записів");

                        }
                    }

                }


            }
        }

        public void DublicatesPatients()
        {
            try
            {
                var colPatients = context.d_Patients.Where(c => c.adress == "" || c.adress == null)
                .GroupBy(p => new { p.name, p.year })
                .Where(g => g.Count() > 1)
                .Select(g => new { g.Key.name, g.Key.year })
                .OrderBy(c => c.name)
                .ToList();

                MessageBox.Show(colPatients.Count.ToString());

                foreach (var patient in colPatients)
                {
                    d_Patients pacientFirst = context.d_Patients.Where(c => c.name.Equals(patient.name, StringComparison.OrdinalIgnoreCase) &&
                    c.year == patient.year && (c.adress == "" || c.adress == null)).FirstOrDefault();
                    var colAnalises = context.d_Analyzes.Where(c => c.d_Patients.name.Equals(patient.name, StringComparison.OrdinalIgnoreCase) &&
                    c.d_Patients.year == patient.year && (c.d_Patients.adress == "" || c.d_Patients.adress == null));

                    foreach (var analis in colAnalises)
                    {
                        d_Patients oldPacient = analis.d_Patients;
                        int lab = analis.labNum;
                        analis.idPatient = pacientFirst.id;
                        analis.d_Patients = pacientFirst;
                    }
                }

                context.SaveChanges();

                //если у пациента нет анализов - удаляем пациента
                var col4 = context.d_Patients.Where(c => c.d_Analyzes.Count == 0);
                MessageBox.Show(col4.Count().ToString());

                foreach (var item in col4)
                {
                    context.d_Patients.Remove(item);
                }
                context.SaveChanges();

                var col2 = context.d_Patients.Where(c => c.adress == "" || c.adress == null)
               .GroupBy(p => new { p.name, p.year })
               .Where(g => g.Count() > 1)
               .Select(g => new { g.Key.name, g.Key.year })
               .ToList();

                MessageBox.Show(col2.Count.ToString());

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        public void generationCodes()
        {
            string barcodeText = "123456789012";

            // Створення об'єкта для генерації штрих-коду
            BarcodeWriter writer = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128, // Формат штрих-коду
                Options = new ZXing.Common.EncodingOptions
                {
                    Height = 200, // Висота зображення
                    Width = 300,  // Ширина зображення
                    Margin = 10   // Поля
                }
            };

            // Генерація зображення штрих-коду
            using (Bitmap bitmap = writer.Write(barcodeText))
            {
                // Збереження штрих-коду у файл
                string folderMain = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string filePath = folderMain + "\\barcode.png";
                bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                Console.WriteLine($"Штрих-код збережено у файл: {filePath}");
            }


            //считування штрих коду
            //private void textBoxBarcode_KeyDown_1(object sender, System.Windows.Input.KeyEventArgs e)
            //{
            //    // Якщо натиснута клавіша Enter (вказує на завершення сканування)
            //    if (e.Key == System.Windows.Input.Key.Enter)
            //    {
            //        string scannedCode = textBoxBarcode.Text.Trim(); // Отримуємо текст із поля
            //        x_stadies.Text = $"Зчитано: {scannedCode}";
            //        textBoxBarcode.Clear(); // Очищаємо поле для наступного сканування
            //    }
            //}

        }

        public void ExceptionDB()
        {
            try
            {

            }
            catch (DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
        }

        public void DeliteEmptyDate()
        {
            var col2 = context.p_Analises_Mediums_Date.Where(c => c.d_RezTemplateMedium == null).ToList();
            MessageBox.Show(col2.Count.ToString());
            foreach (var item in col2)
            {
                context.p_Analises_Mediums_Date.Remove(item);
            }
            context.SaveChanges();

        }

        public void VBD_48()
        {
            var col = context.d_Analyzes.Where(c => c.idPatientStatus == 10 && c.do48 == null).ToList();
            MessageBox.Show(col.Count.ToString());
            foreach (var item in col)
            {
                item.do48 = false;
            }
            context.SaveChanges();
        }

        public void DataBaseToDataBase()
        {
            //// Копіювання бази даних
            //BacLab_DB_OBLEntities contextObl = new BacLab_DB_OBLEntities();

            //var colOblAn = contextObl.d_Analyzes;
            //d_Analyzes Analis;
            //d_Patients Patient;
            //OblApp.d_Patients PacientObl;
            //int i = 0;

            //foreach (var analisObl in colOblAn)
            //{
            //    PacientObl = analisObl.d_Patients;
            //    Patient = context.d_Patients.Where(c => c.name.Equals(PacientObl.name) && c.year == PacientObl.year).FirstOrDefault();
            //    if (Patient == null)
            //    {
            //        Patient = new d_Patients()
            //        {
            //            name = PacientObl.name,
            //            year = PacientObl.year,
            //            adress = PacientObl.adress,
            //            phone = PacientObl.phone,
            //            email = PacientObl.email,
            //            sex = PacientObl.sex,
            //            idDictrict = PacientObl.idDictrict,
            //            idJob = PacientObl.idJob,
            //            idJobDictrict = PacientObl.idJobDictrict,
            //            idJobPlace = PacientObl.idJobPlace
            //        };

            //        context.d_Patients.Add(Patient);
            //        context.SaveChanges();

            //    }

            //    Analis = new d_Analyzes();

            //    Analis.labNum = analisObl.labNum;
            //    Analis.dateSampling = analisObl.dateSampling;
            //    Analis.dateDelivery = analisObl.dateDelivery;
            //    Analis.dateEnd = analisObl.dateEnd;
            //    Analis.idPatient = Patient.id;
            //    Analis.numMedCard = analisObl.numMedCard;
            //    Analis.sendAnalis = analisObl.sendAnalis;
            //    Analis.rezult = analisObl.rezult;
            //    Analis.idFinance = 2;
            //    if (analisObl.idInstitution == 70)
            //        Analis.idInstitution = 5;
            //    else
            //        Analis.idInstitution = context.d_Institution.Where(c => c.name.Equals(analisObl.d_Institution.name)).FirstOrDefault().id;

            //    if (analisObl.d_Department != null)
            //        Analis.idDepartment = context.d_Department.Where(c => c.name.Equals(analisObl.d_Department.name)).FirstOrDefault().id;

            //    if (analisObl.d_Diagnosis != null)
            //    {
            //        d_Diagnosis item = context.d_Diagnosis.Where(c => c.name.Equals(analisObl.d_Diagnosis.name)).FirstOrDefault();
            //        if (item == null)
            //        {
            //            item = new d_Diagnosis()
            //            {
            //                name = analisObl.d_Diagnosis.name,
            //                abbr = analisObl.d_Diagnosis.abbr,
            //                index = analisObl.d_Diagnosis.index,
            //                show = analisObl.d_Diagnosis.show
            //            };

            //        }
            //        Analis.d_Diagnosis = item;
            //    }

            //    if (analisObl.d_SentPerson != null)
            //    {
            //        d_SentPerson item = context.d_SentPerson.Where(c => c.name.Equals(analisObl.d_SentPerson.name)).FirstOrDefault();
            //        if (item == null)
            //        {
            //            item = new d_SentPerson()
            //            {
            //                name = analisObl.d_SentPerson.name,
            //                abbr = analisObl.d_SentPerson.abbr,
            //                index = analisObl.d_SentPerson.index,
            //                show = analisObl.d_SentPerson.show
            //            };

            //        }
            //        Analis.d_SentPerson = item;
            //    }

            //    Analis.idPatientStatus = analisObl.idPatientStatus;
            //    Analis.idResTemplate = analisObl.idResTemplate;
            //    if (analisObl.d_Staff != null)
            //        Analis.d_Staff = context.d_Staff.Where(c => c.name.Equals(analisObl.d_Staff.name)).FirstOrDefault();
            //    Analis.agePatient = analisObl.agePatient;

            //    if (analisObl.d_Institution1 != null)
            //        Analis.idInstitutionLaboratorii = context.d_Institution.Where(c => c.name.Equals(analisObl.d_Institution1.name)).FirstOrDefault().id;

            //    Analis.comment = analisObl.comment;

            //    Analis.idSubdivisions = 9;

            //    if (analisObl.idGMP == 1182)
            //        Analis.idGMP = 1185;
            //    else if (analisObl.idGMP == 1183)
            //        Analis.idGMP = 1186;
            //    else if (analisObl.idGMP == 1184)
            //        Analis.idGMP = 1187;
            //    else Analis.idGMP = analisObl.idGMP;

            //    var db = analisObl.p_Analises_DB.FirstOrDefault();
            //    if (db != null)
            //    {
            //        Analis.p_Analises_DB.Add(new p_Analises_DB()
            //        {
            //            bif = db.bif,
            //            lac = db.lac,
            //            ent = db.ent,
            //            coli1 = db.coli1,
            //            coli2 = db.coli2,
            //            coli3 = db.coli3
            //        });
            //    }

            //    var colMediums = analisObl.p_Analises_Mediums;
            //    foreach (var medium in colMediums)
            //    {
            //        p_Analises_Mediums newMedium = new p_Analises_Mediums();

            //        newMedium.idMedium = medium.idMedium;
            //        newMedium.isMain = medium.isMain;
            //        newMedium.idMethodInoculation = medium.idMethodInoculation;
            //        newMedium.timeIncubation = medium.timeIncubation;
            //        newMedium.timeInoculation = medium.timeInoculation;


            //        if (medium.p_Analises_Mediums_Cultures.Count > 0)
            //            foreach (var culture in medium.p_Analises_Mediums_Cultures)
            //            {
            //                p_Analises_Mediums_Cultures newCulture = new p_Analises_Mediums_Cultures();
            //                newCulture.idCulture = culture.idCulture;
            //                newCulture.idSerotype = culture.idSerotype;
            //                newCulture.idBiovariant = culture.idBiovariant;
            //                newCulture.quantity = culture.quantity;
            //                newCulture.hemolysis = culture.hemolysis;
            //                newCulture.proteolysis = culture.proteolysis;
            //                newCulture.lacPlusMinus = culture.lacPlusMinus;
            //                newCulture.lacMinus = culture.lacMinus;
            //                newCulture.pat = culture.pat;
            //                newCulture.betalactamase = culture.betalactamase;
            //                newCulture.blrs = culture.blrs;
            //                newCulture.carbopenemase = culture.carbopenemase;
            //                newCulture.mrsa = culture.mrsa;
            //                newCulture.pzb = culture.pzb;
            //                newCulture.vre = culture.vre;

            //                if (culture.p_Analises_Mediums_Cultures_AB.Count > 0)
            //                    foreach (var ab in culture.p_Analises_Mediums_Cultures_AB)
            //                    {
            //                        p_Analises_Mediums_Cultures_AB newAB = new p_Analises_Mediums_Cultures_AB();
            //                        newAB.idAB = ab.idAB;
            //                        newAB.pm = ab.pm;
            //                        newAB.mm = ab.mm;
            //                        newAB.index = ab.index;
            //                        newAB.disk = ab.disk;
            //                        newAB.forScrining = ab.forScrining;
            //                        newAB.ferment = ab.ferment;
            //                        newAB.fag = ab.fag;
            //                        newAB.sinergizm = ab.sinergizm;
            //                        newAB.comment = ab.comment;
            //                        newAB.onlyPerOr = ab.onlyPerOr;
            //                        newAB.onlyVV = ab.onlyVV;

            //                        newCulture.p_Analises_Mediums_Cultures_AB.Add(newAB);
            //                    }

            //                newMedium.p_Analises_Mediums_Cultures.Add(newCulture);
            //            }

            //        Analis.p_Analises_Mediums.Add(newMedium);

            //    }
            //    context.d_Analyzes.Add(Analis);
            //    context.SaveChanges();
            //    i++;
            //}

            //MessageBox.Show(i.ToString());
        }

        public void DeleteInnerControl()
        {
            //видалити всі контролі
            DateTime startDate = new DateTime(2025, 03, 25);
            DateTime endDate = new DateTime(2025, 03, 28);

            for (int i = 0; i < 300; i++)
            {

                DateTime date = startDate.AddDays(i);

                var colInner = context.p_Inner_Control.Where(c => c.date == date).ToList();

                foreach (var item in colInner)
                {
                    context.p_Inner_Control.Remove(item);
                }

                if (date.Date == endDate.Date) break;
            }
            context.SaveChanges();

            // зафіксувати  лампи
            DateTime date1 = new DateTime(2025, 03, 24);
            var colLampDate = context.p_Inner_Control.Where(c => c.date == date1 && c.isLampTime == true);
            var colLamp = context.d_Equipment.Where(c => c.idEquipmentGroup == 26 && c.d_EquipmentState == null);
            foreach (var lamp in colLamp)
            {

                lamp.time_workUFO = colLampDate.Where(c => c.d_Equipment.id == lamp.id).FirstOrDefault().timeCommon;
            }
            context.SaveChanges();

            //нові контролі
            for (int i = 0; i < 300; i++)
            {
                DateTime date = startDate.AddDays(i);

                //SaveRoomControls(date);

                if (date.Date == endDate.Date) break;
            }
            context.SaveChanges();

        }

        public void InnerControl()
        {


            try
            {
                var startDate = new DateTime(2023, 10, 01);
                var endDate = new DateTime(2025, 03, 11);


                var colRooms = context.d_Room;
                for (int i = 0; i < 500; i++)
                {
                    DateTime date1 = startDate.AddDays(i);
                    var item = context.p_Inner_Control.Where(c => c.idEquipment == 167 && c.date == date1).FirstOrDefault();
                    if (item != null)
                    {
                        item.idEquipmentState = 6;
                        item.value = "+37";
                    }
                    item = context.p_Inner_Control.Where(c => c.idEquipment == 170 && c.date == date1).FirstOrDefault();
                    if (item != null)
                    {
                        item.idEquipmentState = 6;
                        item.value = "+37";
                    }


                    if (date1.Date == endDate.Date) break;

                }

                context.SaveChanges();
            }

            catch (Exception ex)
            {

                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }


            //try
            //{
            //    var startDate = new DateTime(2023, 01, 04);
            //    var endDate = new DateTime(2023, 08, 14);

            //    var col = context.d_Room;
            //    foreach (var room in col)
            //    {
            //        if (room.id == 7)
            //        {
            //            for (int i = 0; i < 300; i++)
            //            {
            //                DateTime date1 = startDate.AddDays(i);
            //                var co = context.p_Inner_Control.Where(c => c.isLampTime == true && c.date == date1 && c.d_Room.id == 7).ToList();
            //                foreach (var item in co)
            //                {
            //                    context.p_Inner_Control.Remove(item);
            //                }

            //            }

            //            context.SaveChanges();

            //            startDate = new DateTime(2023, 01, 03);
            //            endDate = new DateTime(2023, 08, 14);
            //            var colLamp = room.d_Equipment.Where(c => c.idEquipmentGroup == 26);
            //            int min;
            //            DateTime date;
            //            foreach (var lamp in colLamp)
            //            {
            //                p_Inner_Control i = lamp.p_Inner_Control.Where(c => c.date == startDate).FirstOrDefault();
            //                lamp.time_workUFO = i?.timeCommon;
            //            }
            //            context.SaveChanges();

            //            foreach (var eq in colLamp)
            //            {
            //                for (int i = 1; i < 300; i++)
            //                {
            //                    date = startDate.AddDays(i);

            //                    if (date.DayOfWeek != DayOfWeek.Sunday)

            //                    {
            //                        p_Inner_Control newItem = new p_Inner_Control();
            //                        newItem.isLampTime = true;
            //                        newItem.date = date;
            //                        newItem.idEquipment = eq.id;
            //                        newItem.idEquipmentState = eq.idEquipmentState;
            //                        newItem.idStaff = room.idStaff;

            //                        string time = eq.time_workUFO;
            //                        if (time != null && time != "")
            //                        {
            //                            int hour = Convert.ToInt32(time.Substring(0, time.IndexOf('г')));
            //                            min = Convert.ToInt32(time.Substring(time.IndexOf('д') + 3, time.IndexOf('х') - time.IndexOf('д') - 3));
            //                            min = hour * 60 + min;
            //                            int min2 = 0;
            //                            if (hour < 2600)
            //                                min2 = (int)room.timeUFO_1;
            //                            else if (hour < 5300)
            //                                min2 = (int)room.timeUFO_2;
            //                            else
            //                                min2 = (int)room.timeUFO_3;

            //                            newItem.currentMode = min2.ToString();
            //                            newItem.value = min2.ToString();

            //                            min = min + min2;
            //                            hour = min / 60;
            //                            min = min % 60;
            //                            newItem.timeCommon = hour.ToString() + "год. " + min.ToString() + "хв.";
            //                            eq.time_workUFO = newItem.timeCommon;
            //                        }

            //                        room.p_Inner_Control.Add(newItem);

            //                    }

            //                    if (date.Date == endDate.Date) break;
            //                }
            //            }

            //            context.SaveChanges();
            //        }

            //    }




            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message + " " + ex.StackTrace);
            //}
        }
        public void InnerControl2()
        {
            //УФ лампи
            //var startDate = new DateTime(2023, 04, 01);
            //var endDate = new DateTime(2023, 05, 19);
            //for (int i = 0; i < 120; i++)
            //{
            //    DateTime date = startDate.AddDays(i);
            //    var co = context.p_Room_Control.Where(c => c.isLampTime == true && c.date == date).ToList();
            //    foreach (var item in co)
            //    {
            //        context.p_Room_Control.Remove(item);
            //    }

            //}

            //context.SaveChanges();

            //var startDate = new DateTime(2023, 04, 21, 0, 0, 0);
            //var endDate = new DateTime(2023, 06, 21, 0, 0, 0);
            //var colLamp = context.d_Equipment.Where(c => c.idEquipmentGroup == 26);
            //int min;
            //DateTime date;
            //foreach (var lamp in colLamp)
            //{
            //    p_Inner_Control i = lamp.p_Inner_Control.Where(c => c.date.Date == startDate.Date).FirstOrDefault();
            //    lamp.time_workUFO = i?.timeCommon;
            //}
            //context.SaveChanges();

            //foreach (var lamp in colLamp)
            //{
            //    for (int i = 1; i < 150; i++)
            //    {
            //        date = startDate.AddDays(i);
            //        p_Inner_Control item = lamp.p_Inner_Control.Where(c => c.date.Date == date.Date && c.isLampTime == true).FirstOrDefault();
            //        if (item != null)
            //        {
            //            string time = lamp.time_workUFO;
            //            if (time != null && time != "")
            //            {
            //                int hour = Convert.ToInt32(time.Substring(0, time.IndexOf('г')));
            //                min = Convert.ToInt32(time.Substring(time.IndexOf('д') + 3, time.IndexOf('х') - time.IndexOf('д') - 3));
            //                min = hour * 60 + min;
            //                int min2 = 0;
            //                if (hour < 2600)
            //                    min2 = (int)lamp.d_Room.timeUFO_1;
            //                else if (hour < 5300)
            //                    min2 = (int)lamp.d_Room.timeUFO_2;
            //                else
            //                    min2 = (int)lamp.d_Room.timeUFO_3;

            //                min = min + min2;
            //                hour = min / 60;
            //                min = min % 60;
            //                string str1 = hour.ToString() + "год. " + min.ToString() + "хв.";
            //                item.timeCommon = str1;
            //                lamp.time_workUFO = str1;

            //            }
            //        }

            //        if (date.Date == endDate.Date) break;
            //    }
            //}
            //context.SaveChanges();


            ////Дезінфекція
            //Random rnd = new Random();
            //var colRoom = context.d_Room.ToList();
            //var startDate = new DateTime(2023, 01, 02);
            //var endDate = new DateTime(2023, 05, 17);

            //for (int i = 0; i < 300; i++)
            //{
            //    DateTime date = startDate.AddDays(i);
            //    var colEqDate = context.p_Room_Control.Where(c => c.isEquipmentDisenf == true).Where(c => c.date == date).ToList();
            //    if (colEqDate.Count < 1)
            //    {
            //        if (date.DayOfWeek == DayOfWeek.Monday)
            //            foreach (var room in colRoom)
            //            {
            //                var colEq = context.d_Equipment.Where(c => c.idRoom == room.id && c.d_Disinfectants != null && c.d_EquipmentState == null);
            //                foreach (var eq in colEq)
            //                {
            //                    if (eq.idEquipmentGroup == 26)
            //                    {
            //                        p_Room_Control newItem = new p_Room_Control();
            //                        newItem.isEquipmentDisenf = true;
            //                        newItem.idRoom = room.id;
            //                        newItem.idEquipment = eq.id;
            //                        newItem.idDisinfectants = eq.idDisinfectants;
            //                        newItem.timeSpan = eq.time_Disinfectants;
            //                        newItem.idStaff = room.idStaff;
            //                        newItem.date = date;
            //                        context.p_Room_Control.Add(newItem);

            //                    }
            //                    else if (date.Day <= 7)
            //                    {
            //                        p_Room_Control newItem = new p_Room_Control();
            //                        newItem.isEquipmentDisenf = true;
            //                        newItem.idRoom = room.id;
            //                        newItem.idEquipment = eq.id;
            //                        newItem.idDisinfectants = eq.idDisinfectants;
            //                        newItem.timeSpan = eq.time_Disinfectants;
            //                        newItem.idStaff = room.idStaff;
            //                        newItem.date = date;
            //                        context.p_Room_Control.Add(newItem);
            //                    }
            //                }

            //            }
            //    }

            //    if (date == endDate) break;
            //}

            //context.SaveChanges();

            //температури кімнат
            //Random rnd = new Random();
            //var colRoom = context.d_Room.ToList();
            //var startDate = new DateTime(2023, 01, 02);
            //var endDate = new DateTime(2023, 05, 17);
            //int min;
            //int max;
            //string str = "";
            //for (int i = 0; i < 300; i++)
            //{
            //    DateTime date = startDate.AddDays(i);
            //    var colEqDate = context.p_Room_Control.Where(c => c.isRoomTemp == true).Where(c => c.date == date).ToList();
            //    if (colEqDate.Count < 1)
            //    {

            //        if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
            //            foreach (var room in colRoom)
            //            {
            //                //температура в кімнатах

            //                    p_Room_Control newItem = new p_Room_Control();
            //                    newItem.isRoomTemp = true;
            //                    newItem.d_Room = room;
            //                    newItem.d_Equipment1 = room.d_Equipment.Where(c => c.idEquipmentGroup == 22).FirstOrDefault();
            //                    newItem.temperature = rnd.Next(22, 25).ToString();
            //                    newItem.humidity = rnd.Next(55, 65).ToString();
            //                    newItem.idStaff = room.idStaff;
            //                    newItem.date = date;
            //                    newItem.time = new TimeSpan(8, 0, 0);
            //                    newItem.currentMode = room.temperature;
            //                    newItem.currentMode2 = room.humidity;
            //                    int temp = Convert.ToInt32(newItem.temperature);
            //                    int humidity = Convert.ToInt32(newItem.humidity);
            //                //str = newItem.temperature;
            //                //    if (str[0].Equals(' '))
            //                //    {
            //                //        if (str.IndexOf('±') > -1)
            //                //        {
            //                //            i = Convert.ToInt32(str.Substring(1, str.IndexOf('±') - 1).Trim());
            //                //            int j = Convert.ToInt32(str.Substring(str.IndexOf('±') + 1));
            //                //            min = i - j;
            //                //            max = i + j;
            //                //        }
            //                //        else
            //                //        {
            //                //            i = Convert.ToInt32(str.Substring(1).Trim());
            //                //            min = i;
            //                //            max = i;
            //                //        }

            //                //        if (temp < min) newItem.comment = "Темпереатура нижча за норму";
            //                //        if (temp > max) newItem.comment = "Темпереатура вища за норму";

            //                //    }
            //                //    else if (str[0].Equals('>'))
            //                //    {
            //                //        i = Convert.ToInt32(str.Substring(1).Trim());
            //                //        if (temp < i) newItem.comment = "Темпереатура нижча за норму";
            //                //    }
            //                //    else if (str[0].Equals('<'))
            //                //    {
            //                //        i = Convert.ToInt32(str.Substring(1).Trim());
            //                //        if (temp > i) newItem.comment = "Темпереатура вища за норму";
            //                //    }
            //                //    else
            //                //    {
            //                //        min = Convert.ToInt32(str.Substring(0, str.IndexOf(' ')).Trim());
            //                //        max = Convert.ToInt32(str.Substring(str.IndexOf(' ') + 3).Trim());

            //                //        if (temp < min) newItem.comment = "Темпереатура нижча за норму";
            //                //        if (temp > max) newItem.comment = "Темпереатура вища за норму";

            //                //    }

            //                context.p_Room_Control.Add(newItem);
            //                }
            //    }
            //    else
            //    {

            //        foreach (var item in colEqDate)
            //        {
            //            item.humidity = rnd.Next(55, 65).ToString();
            //        }
            //    }
            //    if (date == endDate) break;
            //}

            //context.SaveChanges();

            ////генеральне прибирання

            //Random rnd = new Random();
            //var colRoom = context.d_Room.ToList();
            //var startDate = new DateTime(2023, 01, 02);
            //var endDate = new DateTime(2023, 05, 17);

            //for (int i = 0; i < 300; i++)
            //{
            //    DateTime date = startDate.AddDays(i);


            //        if (date.DayOfWeek == DayOfWeek.Monday)
            //            foreach (var room in colRoom)
            //            {


            //                    if (date.Day <= 7)
            //                    {
            //                        p_Room_Control newItem = new p_Room_Control();
            //                        newItem.isRoomDisenf = true;
            //                        newItem.roomCleaning = true;
            //                        newItem.idRoom = room.id;
            //                        newItem.idStaff = room.idStaffCleaning;
            //                        newItem.date = date;
            //                        context.p_Room_Control.Add(newItem);
            //                    }
            //                }




            //    if (date == endDate) break;
            //}

            //context.SaveChanges();

            ////температури обладнання
            //var colRoom = context.d_Room.ToList();
            //var startDate = new DateTime(2023, 01, 02);
            //var endDate = new DateTime(2023, 05, 17);
            //for (int i = 0; i < 300; i++)
            //{
            //    DateTime date = startDate.AddDays(i);
            //    var colEqDate = context.p_Room_Control.Where(c=>c.isEquipmentTemp == true).Where(c => c.date == date).ToList();
            //    if (colEqDate.Count < 1)
            //    {

            //        if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
            //            foreach (var room in colRoom)
            //            {
            //                var colEq = context.d_Equipment.Where(c => c.idRoom == room.id && c.idThermometer != null && c.d_EquipmentState == null);
            //                foreach (var eq in colEq)
            //                {
            //                    p_Room_Control newItem = new p_Room_Control();
            //                    newItem.isEquipmentTemp = true;
            //                    newItem.idRoom = room.id;
            //                    newItem.idEquipment = eq.id;
            //                    newItem.idThermometer = eq.idThermometer;
            //                    newItem.currentMode = eq.currentMode;
            //                    newItem.temperature = eq.idEquipmentGroup == 17 ? "+6" : eq.currentMode.Substring(0, eq.currentMode.IndexOf('±')).Trim();
            //                    newItem.temperature2 = newItem.temperature;
            //                    newItem.idStaff = room.idStaff;
            //                    newItem.date = date;
            //                    context.p_Room_Control.Add(newItem);
            //                }
            //            }
            //    }
            //    else
            //    {
            //        colEqDate = colEqDate.Where(c => c.isEquipmentTemp == true).ToList();
            //        foreach (var item in colEqDate)
            //        {
            //            if (item.temperature == null)
            //            {
            //                item.temperature = "+6" ;
            //                item.temperature2 = item.temperature;
            //            }
            //            else if (item.temperature.Contains('±'))
            //            {
            //                item.temperature = item.temperature.Substring(0, item.currentMode.IndexOf('±')).Trim();
            //                item.temperature2 = item.temperature;
            //            }

            //        }
            //    }
            //    if (date == endDate) break;
            //}

            //context.SaveChanges();

        }

        public void Printer()
        {
            //Word.Application wordApp = new Word.Application { };
            //string printer = wordApp.Application.ActivePrinter;

            //var server = new LocalPrintServer();
            //PrintQueue queue = server.DefaultPrintQueue;

            ////various properties of printQueue
            //var isOutOfPaper = queue.IsOutOfPaper;
            //var isOffLine = queue.IsOffline;
            //var isPaperJam = queue.IsPaperJammed;
            //var requiresUser = queue.NeedUserIntervention;
            //var hasPaperProblem = queue.HasPaperProblem;
            //var isBusy = queue.IsBusy;

            //if (isOutOfPaper.Equals("true"))
            //{
            //    MessageBox.Show("isOutOfPaper");
            //}
            //if (isOffLine.Equals("true"))
            //{
            //    MessageBox.Show("isOffLine");
            //}
            //if (isPaperJam.Equals("true"))
            //{
            //    MessageBox.Show("isPaperJam");
            //}
            //if (requiresUser.Equals("true"))
            //{
            //    MessageBox.Show("requiresUser");
            //}
            //if (hasPaperProblem.Equals("true"))
            //{
            //    MessageBox.Show("hasPaperProblem");
            //}
            //if (isBusy.Equals("true"))
            //{
            //    MessageBox.Show("isBusy");
            //}


            //ManagementObjectCollection col;
            //ManagementObjectSearcher colSea;
            //colSea = new ManagementObjectSearcher("Select * from Win32_Printer");
            //col = colSea.Get();
            //foreach (ManagementObject objWMI in col)
            //{
            //    string name = objWMI["Name"].ToString().ToLower();
            //    if (name == printer.ToLower())
            //    {

            //        MessageBox.Show(objWMI["PrinterStatus"].ToString());

            //        MessageBox.Show(objWMI["ExtendedPrinterStatus"].ToString());
            //    }
            //}

        }

        public void FromExcelToDataBase()
        {
            Excel.Application excel = new Excel.Application() { Visible = false };
            Excel.Workbook workbook = excel.Workbooks.Open(@"C:\Users\User\Desktop\Облік обладнання.xls");
            Excel.Worksheet sheet = (Excel.Worksheet)excel.Worksheets.get_Item(3);
            Excel.Range xlRange = sheet.UsedRange;
            try
            {
                var colEquioment = context.d_Equipment;
                int i = 1;
                for (int row = 3; row < 16; row++)
                {
                    d_Equipment eq = new d_Equipment();

                    eq.isMain = true;
                    eq.isAccreditation = false;
                    eq.index = i++;
                    eq.name = Convert.ToString((sheet.Cells[row, 2] as Excel.Range).Value2);
                    eq.labNum = Convert.ToString((sheet.Cells[row, 3] as Excel.Range).Value2);
                    eq.zavNum = Convert.ToString((sheet.Cells[row, 4] as Excel.Range).Value2);
                    eq.invNum = Convert.ToString((sheet.Cells[row, 5] as Excel.Range).Value2);
                    eq.isPassport = (Convert.ToString((sheet.Cells[row, 6] as Excel.Range).Value2) != "" && (sheet.Cells[row, 6] as Excel.Range).Value2 != null) ? true : false;

                    string str = Convert.ToString((sheet.Cells[row, 7] as Excel.Range).Value2);


                    eq.yearManufacturer = Convert.ToInt32(str);
                    eq.manufacturer = Convert.ToString((sheet.Cells[row, 8] as Excel.Range).Value2);
                    eq.yearІnstallation = Convert.ToInt32(Convert.ToString((sheet.Cells[row, 9] as Excel.Range).Value2));
                    eq.comment = Convert.ToString((sheet.Cells[row, 10] as Excel.Range).Value2);

                    colEquioment.Add(eq);

                }

                context.SaveChanges();
                workbook?.Close(SaveChanges: false);
                excel?.Quit();
            }
            catch (Exception ex)
            {
                workbook?.Close(SaveChanges: false);
                excel?.Quit();
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

        }

        public void AnalisMadiumCultureAB()
        {
            //var colAnalisis = context.d_Analyzes.ToList();
            //MessageBox.Show(colAnalisis.Count().ToString());

            //var colMediums = context.p_Analises_Mediums.OrderBy(c => c.id).ToList();
            //MessageBox.Show(colMediums.Count().ToString());
            //var colMediumsDate = context.p_Analises_Mediums_Date.ToList();
            //MessageBox.Show(colMediumsDate.Count().ToString());
            //var colCulures = context.p_Analises_Mediums.Where(c=>c.p_Analises_Mediums_Date.Count()  <1).ToList();   
            //MessageBox.Show(colCulures.Count().ToString());
            //var colAB = context.p_Analises_Mediums_Cultures_AB.ToList();
            //MessageBox.Show(colAB.Count().ToString());

            //MessageBox.Show(colMediums.Where(c => c.p_Analises_Mediums_Cultures.Count() > 0).Count().ToString());





            //Thread potokRez = new Thread(SaveRezult);
            //potokRez.IsBackground = true;
            //potokRez.Start();

            //x_editBTN.IsEnabled = false;

            //foreach (var medium in colMediums)
            //{
            //    context.p_Analises_Mediums_Date.Add(new p_Analises_Mediums_Date
            //    {
            //        idAnalisesMediums = medium.id,
            //        idRezTemplateMedium =  2

            //    });

            //}
            //context.SaveChanges();





            //foreach (var culture in colCulures)
            //{
            //    p_Analises_Mediums medium = context.p_Analises_Mediums.Where(c => c.idAnalis == culture.idAnalysis).FirstOrDefault();
            //    if (medium != null)
            //    {
            //        p_Analises_Mediums_Cultures newCulture = new p_Analises_Mediums_Cultures()
            //        {
            //            idAnalisesMediums = medium.id,
            //            idCulture = (int)culture.idCulture,
            //            idSerotype = culture.id_Serotype,
            //            idBiovariant = culture.idBiovariant,
            //            hemolysis = culture.hemolysis,
            //            proteolysis = culture.proteolysis,
            //            lacPlusMinus = culture.lacPlusMinus,
            //            lacMinus = culture.lacMinus,
            //            pat = culture.pat,
            //            quantity = culture.quantity,
            //            betalactamase = culture.betalactamase,
            //            blrs = culture.blrs,
            //            carbopenemase = culture.carbopenemase,
            //            morphology = culture.morphology,
            //            mrsa = culture.mrsa,
            //            typeColony = culture.typeColony,
            //            pzb = culture.pzb,
            //            vre = culture.vre
            //        };
            //        context.p_Analises_Mediums_Cultures.Add(newCulture);

            //        context.SaveChanges();


            //        var colAB = culture.p_Analises_Cultures_AB.ToList();
            //        foreach (var ab in colAB)
            //        {
            //            context.p_Analises_Mediums_Cultures_AB.Add(new p_Analises_Mediums_Cultures_AB()
            //            {
            //                idAnalisesMediumsCultures = newCulture.id,
            //                idAB = ab.a_Antibiotic.id,
            //                pm = ab.pm,
            //                mm = ab.mm,
            //                disk = ab.disk,
            //                ferment = ab.ferment,
            //                fag = ab.fag,
            //                sinergizm = ab.sinergizm,
            //                comment = ab.comment,
            //                index = ab.index,
            //                forScrining = ab.forScrining,
            //                onlyPerOr = ab.onlyPerOr,
            //                onlyVV = ab.onlyVV
            //            });

            //        }

            //        context.SaveChanges();
            //    }

            //}


            //var col = context.p_Analises_Mediums_Cultures;
            //MessageBox.Show(col.Count().ToString());
            //var col2 = context.p_Analises_Mediums_Cultures_AB;
            //MessageBox.Show(col2.Count().ToString());


            ////var col = context.p_Analises_Mediums.ToList();
            ////MessageBox.Show(col.Count.ToString());
            ////foreach (var item in col)
            ////{
            ////    context.p_Analises_Mediums.Remove(item);
            ////}
            ////context.SaveChanges();

            //var colAnalises = context.d_Analyzes.Where(c=>c.)
            //foreach (var analis in colAnalises)
            //{
            //    var colMadiums = context.p_Group_Material_Purpose_Medium.Where(c => c.id_GMP == analis.idGMP);
            //    foreach (var medium in colMadiums)
            //    {
            //        analis.p_Analises_Mediums.Add(new p_Analises_Mediums()
            //        {
            //            idAnalis = analis.id,
            //            idMedium = medium.id,
            //            d_Medium = medium.d_Medium,
            //            idMethodInoculation = medium.idMethodInoculation,
            //            d_MethodsInoculation = medium.d_MethodsInoculation,
            //            timeIncubation = medium.timeIncubation,
            //            timeInoculation = medium.timeInoculation

            //        });
            //    }
            //}

            //context.SaveChanges();

            //MessageBox.Show(context.p_Analises_Mediums.ToList().Count.ToString());

            //context.p_Analises_Cultures.Remove(context.p_Analises_Cultures.Where(c => c.id == 5979).FirstOrDefault());
            //context.p_Analises_Cultures.Remove(context.p_Analises_Cultures.Where(c => c.id == 5980).FirstOrDefault());
            //context.p_Analises_Cultures.Remove(context.p_Analises_Cultures.Where(c => c.id == 5981).FirstOrDefault());
            //context.p_Analises_Cultures.Remove(context.p_Analises_Cultures.Where(c => c.id == 5982).FirstOrDefault());
            //context.p_Analises_Cultures.Remove(context.p_Analises_Cultures.Where(c => c.id == 5983).FirstOrDefault());
            //context.p_Analises_Cultures.Remove(context.p_Analises_Cultures.Where(c => c.id == 5984).FirstOrDefault());
            //context.SaveChanges();


            //var col2 = context.p_Analises_Cultures_AB.Where(c => c.idpAnalisesCultures == null);


            //MessageBox.Show(col2.Count().ToString());


            //int i = 0; int j = 0; int x = 0; int y = 0; int z = 0; int vre = 0;
            //var col = context.p_Analises_Cultures.Where(c => c.p_Analises_Cultures_AB.Count > 0);
            //foreach (var item in col)
            //{
            //    if (item.betalactamase == true)
            //        i++;
            //    if (item.blrs == true)
            //        j++;
            //    if (item.carbopenemase == true)
            //        x++;
            //    if (item.pzb == true)
            //        y++;
            //    if (item.mrsa == true)
            //        z++;
            //    if (item.vre == true)
            //        vre++; 

            //}

            //MessageBox.Show("Беталактамази " + i.ToString() + "\n" +
            //    "БЛРС " + j.ToString() + "\n" + 
            //    "Карбо " + x.ToString() + "\n" + 
            //    "ПЗБ " + y.ToString() + "\n" +
            //    "MRSA " + z.ToString() + "\n" +
            //    "VRE " + vre.ToString());


            //context.SaveChanges();


            //var col2 = context.p_Analises_Cultures.Where(c => c.d_Biovariant.id == 65);


            //MessageBox.Show(col2.Count().ToString());



            //int i = 0;
            //var col2 = context.d_Analyzes.Where(c => c.idInstitution == 20);
            //foreach (var item in col2)
            //{
            //    item.idEmail2 = null;
            //    i++;
            //}

            //MessageBox.Show(i.ToString());

            //context.SaveChanges();

        }

        public void FileToPDF()
        {
            //Сохранение в PDF
            //newDoc.SaveAs2(fileName, WdSaveFormat.wdFormatPDF);
            //newDoc.Close(WdSaveOptions.wdDoNotSaveChanges);
            //tempDoc.Close();
            //wordApp.Quit();
        }
    }
}
