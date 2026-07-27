using BacLab.Dialogs;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Office.Interop.Word;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Windows;
using Word = Microsoft.Office.Interop.Word;

namespace BacLab.Models
{
    public static class CommonClass
    {
        public static int idGMPTest;

        public static void DownloadAnalizesTerra(BacLab_DBEntities context, d_Subdivisions subdivisions)
        {
            try
            {
                string ftpUrl = "ftp://operator.bak@srv-fab2.dia.zp.ua:21012/DIA-KDL/Registration";
                string username = "operator.bak";
                string password = "Jgthfnjh#b@k";
                FtpWebRequest request;

                List<string> listFilesFTP = new List<string>();


                request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                string line = string.Empty;
                //перевірка доступності
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        line = streamReader.ReadLine();
                        while (!string.IsNullOrEmpty(line))
                        {
                            if (line[0] != 'd')
                                listFilesFTP.Add(line.Split(new char[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries)[8]);

                            line = streamReader.ReadLine();
                        }
                    }
                }

                int countFiles = 0;

                //вигружаємо всі файли
                foreach (var fileNameFTP in listFilesFTP)
                {
                    countFiles++;
                    //вигруз на робочий стіл
                    string ftpUrl2 = "ftp://operator.bak@srv-fab2.dia.zp.ua:21012/DIA-KDL/Registration/" + fileNameFTP;
                    request = (FtpWebRequest)WebRequest.Create(ftpUrl2);
                    request.Method = WebRequestMethods.Ftp.DownloadFile;
                    request.Credentials = new NetworkCredential(username, password);
                    string folderName = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\Terra";
                    bool exists = System.IO.Directory.Exists(folderName);
                    if (!exists)
                        System.IO.Directory.CreateDirectory(folderName);
                    string fileName = folderName + "\\" + fileNameFTP;

                    using (Stream ftpStream = request.GetResponse().GetResponseStream())
                    using (Stream fileStream = System.IO.File.Create(fileName))
                    {
                        byte[] buffer = new byte[64];
                        int size = 0;
                        while ((size = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fileStream.Write(buffer, 0, size);
                        }
                    }

                    d_Institution dias = context.d_Institution.Where(c => c.id == 26).FirstOrDefault();
                    d_PatientStatus status = context.d_PatientStatus.Where(c => c.id == 1).FirstOrDefault();
                    d_Finance finance = context.d_Finance.Where(c => c.id == 2).FirstOrDefault();
                    DateTime dateDelivery = DateTime.Now;

                    //завантаження аналізів в BacLab
                    using (TextFieldParser parser = new TextFieldParser(fileName, System.Text.Encoding.Default))
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(";");

                        while (!parser.EndOfData)
                        {
                            string[] fields = parser.ReadFields();

                            int idTerra = Convert.ToInt32(fields[1]);
                            int year = Convert.ToInt32(fields[5]);
                            string name = fields[4];

                            d_Patients patient = context.d_Patients.Where(c => c.name.Equals(name) && c.year == year).FirstOrDefault()
                                ?? new d_Patients()
                                {
                                    name = fields[4],
                                    year = Convert.ToInt32(fields[5]),
                                    sex = fields[7]
                                };
                            d_Analyzes Analis = new d_Analyzes
                            {
                                idTerra = Convert.ToInt32(fields[0]),
                                d_Institution = dias,
                                d_Subdivisions = subdivisions,
                                labNum = Convert.ToInt32(fields[1]),
                                dateSampling = Convert.ToDateTime(fields[2]),
                                timeSampling = Convert.ToDateTime(fields[3]),
                                dateDelivery = Convert.ToDateTime(fields[11]),
                                timeDelivery = Convert.ToDateTime(fields[12]),
                                d_Patients = patient,
                                agePatient = Convert.ToInt32(fields[6]),
                                d_PatientStatus = status,
                                d_Finance = finance,
                                sendAnalis = false,
                                isIssued = false,
                                inRaxunok = false
                            };
                            dateDelivery = Convert.ToDateTime(fields[11]);
                            string str = fields[10];
                            Analis.d_Staff = context.d_Staff.Where(c => c.abbr.Contains(str)).FirstOrDefault();


                            // idGMPTerra . Якщо намає такого дослідження
                            Analis.idTerraGMP = Convert.ToInt32(fields[9]);
                            p_Group_Material_Purpose GMP = context.p_Group_Material_Purpose.Where(c => c.idTerraGMP == Analis.idTerraGMP).FirstOrDefault();
                            if (GMP == null)
                            {
                                switch (Analis.idTerraGMP)
                                {
                                    case 3044://вухо кандиди
                                        {
                                            GMP = context.p_Group_Material_Purpose.Where(c => c.id == 160).FirstOrDefault();
                                            Analis.comment = "ліве"; break;
                                        }
                                    case 3045:
                                        {
                                            GMP = context.p_Group_Material_Purpose.Where(c => c.id == 160).FirstOrDefault();
                                            Analis.comment = "праве"; break;
                                        }
                                    case 3112://вухо
                                        {
                                            GMP = context.p_Group_Material_Purpose.Where(c => c.id == 161).FirstOrDefault();
                                            Analis.comment = "ліве"; break;
                                        }
                                    case 3111:
                                        {
                                            GMP = context.p_Group_Material_Purpose.Where(c => c.id == 161).FirstOrDefault();
                                            Analis.comment = "праве"; break;
                                        }
                                    case 3127://грудне молоко
                                        {
                                            GMP = context.p_Group_Material_Purpose.Where(c => c.id == 70).FirstOrDefault();
                                            Analis.comment = "ліве"; break;
                                        }
                                    case 3126:
                                        {
                                            GMP = context.p_Group_Material_Purpose.Where(c => c.id == 70).FirstOrDefault();
                                            Analis.comment = "праве"; break;
                                        }
                                    case 3102://око
                                        {
                                            GMP = context.p_Group_Material_Purpose.Where(c => c.id == 164).FirstOrDefault();
                                            Analis.comment = "ліве"; break;
                                        }
                                    case 3101:
                                        {
                                            GMP = context.p_Group_Material_Purpose.Where(c => c.id == 164).FirstOrDefault();
                                            Analis.comment = "праве"; break;
                                        }
                                    case 3021://уретра
                                        {
                                            GMP = context.p_Group_Material_Purpose.Where(c => c.id == 76).FirstOrDefault();
                                            Analis.comment = "жіноча"; break;
                                        }
                                    case 3031:
                                        {
                                            GMP = context.p_Group_Material_Purpose.Where(c => c.id == 76).FirstOrDefault();
                                            Analis.comment = "чоловіча"; break;
                                        }
                                    case 3032:
                                        {
                                            GMP = context.p_Group_Material_Purpose.Where(c => c.id == 79).FirstOrDefault();
                                            break;
                                        }
                                    case 3025://вагіна
                                        {
                                            GMP = context.p_Group_Material_Purpose.Where(c => c.id == 73).FirstOrDefault();
                                            break;
                                        }
                                    case 3023://матка
                                        {
                                            GMP = context.p_Group_Material_Purpose.Where(c => c.id == 78).FirstOrDefault();
                                            Analis.comment = "спираль"; break;
                                        }
                                    case 3141://зів
                                        {
                                            GMP = context.p_Group_Material_Purpose.Where(c => c.id == 36).FirstOrDefault();
                                            Analis.comment = "мигдалики"; break;
                                        }
                                    case 3122://інший
                                        {
                                            GMP = context.p_Group_Material_Purpose.Where(c => c.id == 89).FirstOrDefault();
                                            Analis.comment = "альвеолярні відростки"; break;
                                        }
                                    case 3123://інший
                                        {
                                            GMP = context.p_Group_Material_Purpose.Where(c => c.id == 89).FirstOrDefault();
                                            Analis.comment = "зубоясенна кишеня"; break;
                                        }
                                    case 3124://пунктат
                                        {
                                            GMP = context.p_Group_Material_Purpose.Where(c => c.id == 85).FirstOrDefault();
                                            Analis.comment = "суглобна рідина"; break;
                                        }
                                    case 309://носоглотка
                                        {
                                            GMP = context.p_Group_Material_Purpose.Where(c => c.id == 44).FirstOrDefault();
                                            break;
                                        }
                                }
                            }
                            if (GMP == null)
                            {
                                DialogStackCheckBoxWindow dialog = new DialogStackCheckBoxWindow(context, subdivisions.id, null, "Не знайдено дослідження:\n" + Analis.idTerraGMP + " " + fields[8], "GMP");
                                if (dialog.ShowDialog() == true && dialog.IdGMP != null)
                                    GMP = context.p_Group_Material_Purpose.Where(c => c.id == dialog.IdGMP).FirstOrDefault();
                            }

                            if (GMP == null) continue;
                            Analis.p_Group_Material_Purpose = GMP;
                            var colMediums = context.p_Group_Material_Purpose_Medium.Where(c => c.id_GMP == Analis.idGMP);
                            context.d_Analyzes.Add(Analis);

                            foreach (var item in colMediums)
                            {
                                Analis.p_Analises_Mediums.Add(new p_Analises_Mediums()
                                {
                                    idAnalis = Analis.id,
                                    idMedium = item.id,
                                    d_Medium = item.d_Medium,
                                    idMethodInoculation = item.idMethodInoculation,
                                    d_MethodsInoculation = item.d_MethodsInoculation,
                                    timeIncubation = item.timeIncubation,
                                    timeInoculation = item.timeInoculation,
                                    timeObservation = item.timeObservation,
                                    isMain = item.is_main
                                });
                            }

                        }

                        context.SaveChanges();

                        //записуємо в рахунок
                        Random rnd = new Random();
                        var colAnalisDay = context.d_Analyzes.Where(c => c.idInstitution == 26 && c.dateDelivery == dateDelivery).ToList();
                        foreach (var col in colAnalisDay)
                            col.inRaxunok = false;

                        var colUrina = colAnalisDay.Where(c => c.idGMP == 35).ToList();
                        int countUrina = rnd.Next(4, 9);
                        for (int j = 0; j < countUrina; j++)
                        {
                            var item = colUrina.Where(c => c.inRaxunok != true).FirstOrDefault();
                            if (item != null)
                                item.inRaxunok = true;
                        }

                        var colOther = colAnalisDay.Where(c => c.inRaxunok != true).ToList();
                        int countOther = rnd.Next(2, 5);
                        for (int j = 0; j < countOther; j++)
                        {
                            var item = colOther.Where(c => c.inRaxunok != true).FirstOrDefault();
                            if (item != null)
                                item.inRaxunok = true;

                        }
                        context.SaveChanges();


                        // видаляємо дублікати пацієнтів
                        var colPatients = context.d_Patients
                            .GroupBy(p => new { p.name, p.year, p.adress })
                            .Where(g => g.Count() > 1)
                            .Select(g => new { g.Key.name, g.Key.year, g.Key.adress })
                            .OrderBy(c => c.name)
                            .ToList();

                        foreach (var patient in colPatients)
                        {
                            d_Patients pacientFirst = context.d_Patients.Where(c => c.name.Equals(patient.name, StringComparison.OrdinalIgnoreCase) &&
                            c.year == patient.year && c.adress == patient.adress).FirstOrDefault();
                            var colAnalises = context.d_Analyzes.Where(c => c.d_Patients.name.Equals(patient.name, StringComparison.OrdinalIgnoreCase) &&
                            c.d_Patients.year == patient.year && c.d_Patients.adress == patient.adress);

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
                        foreach (var item in col4)
                            context.d_Patients.Remove(item);

                        context.SaveChanges();


                        //загруз на FTP/Registration/Readed/
                        ftpUrl = "ftp://operator.bak@srv-fab2.dia.zp.ua:21012/DIA-KDL/Registration/Readed/" + fileNameFTP;
                        request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                        request.Method = WebRequestMethods.Ftp.UploadFile;
                        request.Credentials = new NetworkCredential(username, password);
                        using (Stream fileStream = System.IO.File.OpenRead(fileName))
                        {
                            using (Stream ftpStream = request.GetRequestStream())
                                fileStream.CopyTo(ftpStream);

                        }
                        //Delete на FTP/Registration
                        ftpUrl = "ftp://operator.bak@srv-fab2.dia.zp.ua:21012/DIA-KDL/Registration/" + fileNameFTP;
                        request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                        request.Method = WebRequestMethods.Ftp.DeleteFile;
                        request.Credentials = new NetworkCredential(username, password);
                        FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                        response.Close();
                        //Delete на комп'ютері
                        //System.IO.File.Delete(fileName);

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }
        }

        public static string UploadFileTerra(BacLab_DBEntities context)
        {
            string ftpUrl = ConfigurationManager.AppSettings["FtpUrl"];
            string username = ConfigurationManager.AppSettings["FtpUsername"];
            string password = ConfigurationManager.AppSettings["FtpPassword"];

            //string ftpUrl = "ftp://operator.bak@srv-fab2.dia.zp.ua:21012/DIA-KDL/Results";
            //string username = "operator.bak";  // Ваш логін
            //string password = "Jgthfnjh#b@k";  // Ваш пароль

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential(username, password);
            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    // перевірка з'єднання
                }
            }
            catch (WebException ex)
            {
                return $"З'єднання з сервером відсутне. {ex.Message}";
            }

            try
            {
                var listRecords = BuildTerraRecords(context);

                string folderName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Terra");
                if (!Directory.Exists(folderName))
                    Directory.CreateDirectory(folderName);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = Path.Combine(folderName, $"{timestamp}.csv");
                string uploadUrl = $"{ftpUrl}/{timestamp}.csv";

                WriteCsvFile(fileName, listRecords);

                UploadFileToFtp(uploadUrl, fileName, username, password);

                context.SaveChanges();

                return "Результати вивантажено в Terra";

            }
            catch (Exception ex)
            {
                return ex.Message + " " + ex.StackTrace;
            }

        }

        private static List<TerraCSVModel> BuildTerraRecords(BacLab_DBEntities context)
        {
            List<TerraCSVModel> listRecords = new List<TerraCSVModel>();
            List<TerraCSVModel> listAnalisRecords = new List<TerraCSVModel>();
            List<string> listErrors = new List<string>();
            try
            {
                TerraCSVModel headers = new TerraCSVModel()
                {
                    IdTerra = "ШК пробірки",
                    LabNum = "Дод.номер",
                    IdTerraGMP = "Гіс код",
                    MicroorganismName = "Патоген",
                    Opis = "Опис",
                    MicroorganismID = "Гіс код патогену",
                    Quantity = "Результат",
                    ABName = "Антибіотик",
                    ABID = "Гіс код антибіотика",
                    DoseStandart = "Стандартна доза",
                    DoseHight = "Висока доза",
                    Result = "Результат",
                    DateEnd = "Дата завершення",
                    TimeEnd = "Час завершення",
                    Doctor = "Виконавець"
                };

                listRecords.Add(headers);

                var colAnalis = context.d_Analyzes.Where(c => c.idInstitution == 26 && c.sendAnalis == true && c.isIssued != true).ToList();
                foreach (var analis in colAnalis)
                {
                    try
                    {
                        listAnalisRecords = new List<TerraCSVModel>();

                        //якщо не ріст
                        if (analis.idResTemplate != 2)
                        {
                            TerraCSVModel csvModel = new TerraCSVModel()
                            {
                                IdTerra = analis.idTerra.ToString(),
                                LabNum = analis.labNum.ToString(),
                                IdTerraGMP = analis.idTerraGMP.ToString(),
                                Opis = analis.d_ResTemplate.name,
                                MicroorganismID = "-1",
                                DateEnd = analis.dateEnd.Value.ToShortDateString(),
                                TimeEnd = analis.timeEnd.Value.ToLongTimeString(),
                                Doctor = "Аліменко Ю.Л."
                            };

                            listAnalisRecords.Add(csvModel);

                        }
                        else//якщо ріст
                        {
                            //якщо ДБ
                            if (analis.p_Analises_DB.FirstOrDefault() != null)
                            {
                                p_Analises_DB DB = analis.p_Analises_DB.FirstOrDefault();
                                TerraCSVModel csvModel = new TerraCSVModel()
                                {
                                    IdTerra = analis.idTerra.ToString(),
                                    LabNum = analis.labNum.ToString(),
                                    IdTerraGMP = analis.idTerraGMP.ToString(),
                                    MicroorganismName = "Біфідобактерії",
                                    MicroorganismID = "-2",
                                    Quantity = ChangeExponenta(DB.bif),
                                    DateEnd = analis.dateEnd.Value.ToShortDateString(),
                                    TimeEnd = analis.timeEnd.Value.ToLongTimeString(),
                                    Doctor = "Аліменко Ю.Л."
                                };
                                listAnalisRecords.Add(csvModel);
                                csvModel = new TerraCSVModel()
                                {
                                    IdTerra = analis.idTerra.ToString(),
                                    LabNum = analis.labNum.ToString(),
                                    IdTerraGMP = analis.idTerraGMP.ToString(),
                                    MicroorganismName = "Лактобактерії",
                                    MicroorganismID = "-3",
                                    Quantity = ChangeExponenta(DB.lac),
                                    DateEnd = analis.dateEnd.Value.ToShortDateString(),
                                    TimeEnd = analis.timeEnd.Value.ToLongTimeString(),
                                    Doctor = "Аліменко Ю.Л."
                                };
                                listAnalisRecords.Add(csvModel);
                                csvModel = new TerraCSVModel()
                                {
                                    IdTerra = analis.idTerra.ToString(),
                                    LabNum = analis.labNum.ToString(),
                                    IdTerraGMP = analis.idTerraGMP.ToString(),
                                    MicroorganismName = "Ентерококи",
                                    MicroorganismID = "-12",
                                    Quantity = ChangeExponenta(DB.ent),
                                    DateEnd = analis.dateEnd.Value.ToShortDateString(),
                                    TimeEnd = analis.timeEnd.Value.ToLongTimeString(),
                                    Doctor = "Аліменко Ю.Л."
                                };
                                listAnalisRecords.Add(csvModel);
                                csvModel = new TerraCSVModel()
                                {
                                    IdTerra = analis.idTerra.ToString(),
                                    LabNum = analis.labNum.ToString(),
                                    IdTerraGMP = analis.idTerraGMP.ToString(),
                                    MicroorganismName = "E.coli з нормальними фермантативними властивостями",
                                    MicroorganismID = "-4",
                                    Quantity = DB.coli1 != null ? ChangeExponenta(DB.coli1) : "не виявлено",
                                    DateEnd = analis.dateEnd.Value.ToShortDateString(),
                                    TimeEnd = analis.timeEnd.Value.ToLongTimeString(),
                                    Doctor = "Аліменко Ю.Л."
                                };
                                listAnalisRecords.Add(csvModel);
                                csvModel = new TerraCSVModel()
                                {
                                    IdTerra = analis.idTerra.ToString(),
                                    LabNum = analis.labNum.ToString(),
                                    IdTerraGMP = analis.idTerraGMP.ToString(),
                                    MicroorganismName = "E.colі з зміненими ферментативними властивостями",
                                    MicroorganismID = "-5",
                                    Quantity = DB.coli2 != null ? "ВИЯВЛЕНО" : "не виявлено",
                                    DateEnd = analis.dateEnd.Value.ToShortDateString(),
                                    TimeEnd = analis.timeEnd.Value.ToLongTimeString(),
                                    Doctor = "Аліменко Ю.Л."
                                };
                                listAnalisRecords.Add(csvModel);
                                csvModel = new TerraCSVModel()
                                {
                                    IdTerra = analis.idTerra.ToString(),
                                    LabNum = analis.labNum.ToString(),
                                    IdTerraGMP = analis.idTerraGMP.ToString(),
                                    MicroorganismName = "E.coli лактозонегативна",
                                    MicroorganismID = "-6",
                                    Quantity = DB.coli3 != null ? "ВИЯВЛЕНО" : "не виявлено",
                                    DateEnd = analis.dateEnd.Value.ToShortDateString(),
                                    TimeEnd = analis.timeEnd.Value.ToLongTimeString(),
                                    Doctor = "Аліменко Ю.Л."
                                };
                                listAnalisRecords.Add(csvModel);

                            }

                            bool st = false; bool Cand = false;
                            bool hemolysis = false; bool upf = false;
                            bool pat = false;

                            var cultures = analis.p_Analises_Cultures.ToList();

                            foreach (var itemCulture in cultures)
                            {
                                string culture = itemCulture.d_Microorganism.name;
                                string opys = "";
                                string quantity = "";
                                string idMOstr = itemCulture.d_Microorganism?.id.ToString();

                                if (itemCulture.d_Serotype != null)
                                { opys += "serovariant: " + itemCulture.d_Serotype.name; pat = true; }
                                if (itemCulture.d_Biovariant != null)
                                    opys += "biovariant: " + itemCulture.d_Biovariant.name;
                                if (itemCulture.mrsa == true)
                                    opys += "метицилінрезистентний (MRSA) ";
                                if (itemCulture.lacPlusMinus == true)
                                {
                                    if (analis.idGMP == 13)
                                    { idMOstr = "-13"; culture = "Escherichia coli зі зміненними ферментативними властивостями"; }
                                    else
                                        opys += "зі зміненними ферментативними властивостями ";
                                }
                                if (itemCulture.lacMinus == true)
                                {
                                    if (analis.idGMP == 13)
                                    { idMOstr = "-14"; culture = "Escherichia coli лактозонегативна"; }
                                    else
                                        opys += "лактозонегативна ";
                                }
                                if (itemCulture.hemolysis == true)
                                    opys += "з гемолітичними властивостями ";
                                if (itemCulture.proteolysis == true)
                                    opys += "з протелоітичними властивостями ";


                                //бо не дукується адекватно
                                if (itemCulture.quantity != null)
                                    quantity = ChangeExponenta(itemCulture.quantity);
                                else
                                    quantity = "ВИЯВЛЕНО";

                                if (analis.idGMP == 13) //ДБ
                                {
                                    int idMOGroup = itemCulture.d_Microorganism.g_MicroorganismGroup_Microorganism.FirstOrDefault().d_MicroorganismGroup.id;

                                    if ((idMOGroup == 1 && itemCulture.idCulture != 2 && itemCulture.pat != true) || idMOGroup == 2 || idMOGroup == 3) //упф
                                        upf = true;
                                    if (idMOGroup == 4)
                                        st = true;
                                    if (idMOGroup == 6)
                                        Cand = true;
                                    if (itemCulture.hemolysis == true)
                                        hemolysis = true;
                                }

                                var colAB = itemCulture.p_Analises_Cultures_ABTest.ToList();
                                foreach (var itemAB in colAB)
                                {

                                    //if (itemAB.forScrining == true) continue;

                                    string result = "";
                                    string doseStandart = "";
                                    string doseHigh = "";

                                    if (itemAB.ferment != true && itemAB.fag != true && itemAB.sinergizm != true && itemAB.comment != true)
                                    {
                                        if (itemAB.abResSen == true)
                                        {
                                            if (itemAB.pm.Trim() == "-")
                                                result = "природно стійкий";
                                            if (itemAB.pm.Trim() == "+")
                                                result = "природно чутливий";
                                        }
                                        else
                                            result = itemAB.pm.Trim() == "+" ? "чутливий" : (itemAB.pm.Trim() == "-" ? "стійкий" : "проміжний");

                                        // добавляем дозировки
                                        if (itemAB.d_TestAndAntibiotic.doseStandartPerOr != "" && itemAB.d_TestAndAntibiotic.doseStandartPerOr != null && itemAB.onlyVV != true)
                                            doseStandart = "п/о:" + itemAB.d_TestAndAntibiotic.doseStandartPerOr + "   ";
                                        if (itemAB.d_TestAndAntibiotic.doseStandart_Vv != "" && itemAB.d_TestAndAntibiotic.doseStandart_Vv != null && itemAB.onlyPerOr != true)
                                            doseStandart += "в/в:" + itemAB.d_TestAndAntibiotic.doseStandart_Vv;
                                        else doseStandart = doseStandart.TrimEnd();

                                        if (itemAB.d_TestAndAntibiotic.doseHighPerOr != "" && itemAB.d_TestAndAntibiotic.doseHighPerOr != null && itemAB.onlyVV != true)
                                            doseHigh = "п/о:" + itemAB.d_TestAndAntibiotic.doseHighPerOr + "  ";
                                        if (itemAB.d_TestAndAntibiotic.doseHigh_Vv != "" && itemAB.d_TestAndAntibiotic.doseHigh_Vv != null && itemAB.onlyPerOr != true)
                                            doseHigh += "в/в:" + itemAB.d_TestAndAntibiotic.doseHigh_Vv;
                                        else doseHigh = doseHigh.TrimEnd();

                                    }
                                    else if (itemAB.ferment == true)
                                        result = itemAB.pm.Trim().Equals("+") ? "продукує" : (itemAB.pm.Trim().Equals("-") ? "не продукує" : "не визначалось");
                                    else if (itemAB.fag == true)
                                        result = itemAB.pm;
                                    else if (itemAB.sinergizm == true)
                                        result = itemAB.pm.Trim().Equals("+") ? "спостерігається" : "не спостерігається";
                                    else if (itemAB.comment == true)
                                        result = itemAB.d_TestAndAntibiotic.note;



                                    TerraCSVModel csvModel = new TerraCSVModel()
                                    {
                                        IdTerra = analis.idTerra.ToString(),
                                        LabNum = analis.labNum.ToString(),
                                        IdTerraGMP = analis.idTerraGMP.ToString(),
                                        MicroorganismName = culture,
                                        Opis = opys,
                                        MicroorganismID = idMOstr,
                                        Quantity = quantity,
                                        ABName = itemAB.d_TestAndAntibiotic?.name,
                                        ABID = itemAB.d_TestAndAntibiotic?.id.ToString(),
                                        DoseStandart = doseStandart,
                                        DoseHight = doseHigh,
                                        Result = result,
                                        DateEnd = analis.dateEnd.Value.ToShortDateString(),
                                        TimeEnd = analis.timeEnd.Value.ToLongTimeString(),
                                        Doctor = "Аліменко Ю.Л."

                                    };
                                    listAnalisRecords.Add(csvModel);
                                }

                            }

                            if (analis.idGMP == 13) //ДБ
                            {
                                TerraCSVModel csvModel;

                                csvModel = new TerraCSVModel()
                                {
                                    IdTerra = analis.idTerra.ToString(),
                                    LabNum = analis.labNum.ToString(),
                                    IdTerraGMP = analis.idTerraGMP.ToString(),
                                    MicroorganismName = "Патогенні ентеробактерії",
                                    MicroorganismID = "-11",
                                    Quantity = pat == true ? "ВИЯВЛЕНО" : "не виявлено",
                                    DateEnd = analis.dateEnd.Value.ToShortDateString(),
                                    TimeEnd = analis.timeEnd.Value.ToLongTimeString(),
                                    Doctor = "Аліменко Ю.Л."
                                };
                                listAnalisRecords.Add(csvModel);

                                csvModel = new TerraCSVModel()
                                {
                                    IdTerra = analis.idTerra.ToString(),
                                    LabNum = analis.labNum.ToString(),
                                    IdTerraGMP = analis.idTerraGMP.ToString(),
                                    MicroorganismName = "Умовнопатогенні ентеробактерії",
                                    MicroorganismID = "-10",
                                    Quantity = upf == true ? "ВИЯВЛЕНО" : "не виявлено",
                                    DateEnd = analis.dateEnd.Value.ToShortDateString(),
                                    TimeEnd = analis.timeEnd.Value.ToLongTimeString(),
                                    Doctor = "Аліменко Ю.Л."
                                };
                                listAnalisRecords.Add(csvModel);

                                csvModel = new TerraCSVModel()
                                {
                                    IdTerra = analis.idTerra.ToString(),
                                    LabNum = analis.labNum.ToString(),
                                    IdTerraGMP = analis.idTerraGMP.ToString(),
                                    MicroorganismName = "Коагулазопозитивні стафілококи",
                                    MicroorganismID = "-7",
                                    Quantity = st == true ? "ВИЯВЛЕНО" : "не виявлено",
                                    DateEnd = analis.dateEnd.Value.ToShortDateString(),
                                    TimeEnd = analis.timeEnd.Value.ToLongTimeString(),
                                    Doctor = "Аліменко Ю.Л."
                                };
                                listAnalisRecords.Add(csvModel);

                                csvModel = new TerraCSVModel()
                                {
                                    IdTerra = analis.idTerra.ToString(),
                                    LabNum = analis.labNum.ToString(),
                                    IdTerraGMP = analis.idTerraGMP.ToString(),
                                    MicroorganismName = "Гриби роду Candida",
                                    MicroorganismID = "-8",
                                    Quantity = Cand == true ? "ВИЯВЛЕНО" : "не виявлено",
                                    DateEnd = analis.dateEnd.Value.ToShortDateString(),
                                    TimeEnd = analis.timeEnd.Value.ToLongTimeString(),
                                    Doctor = "Аліменко Ю.Л."
                                };
                                listAnalisRecords.Add(csvModel);

                                csvModel = new TerraCSVModel()
                                {
                                    IdTerra = analis.idTerra.ToString(),
                                    LabNum = analis.labNum.ToString(),
                                    IdTerraGMP = analis.idTerraGMP.ToString(),
                                    MicroorganismName = "Гемолітичні форми",
                                    MicroorganismID = "-9",
                                    Quantity = hemolysis == true ? "ВИЯВЛЕНО" : "не виявлено",
                                    DateEnd = analis.dateEnd.Value.ToShortDateString(),
                                    TimeEnd = analis.timeEnd.Value.ToLongTimeString(),
                                    Doctor = "Аліменко Ю.Л."
                                };
                                listAnalisRecords.Add(csvModel);

                            }

                        }
                        listRecords.AddRange(listAnalisRecords);
                        analis.isIssued = true;
                    }
                    catch (Exception)
                    {
                        listErrors.Add($"№: {analis.labNum} {analis.p_Group_Material_Purpose.d_Material}, Паціент: {analis.d_Patients.name}");
                    }

                }
                if (listErrors.Count > 0)
                {
                    string errorMessage = "Помилки при обробці наступних аналізів:\n" + string.Join("\n", listErrors);
                    MessageBox.Show(errorMessage, "Помилки", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                return listRecords;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message} {ex.StackTrace}");
                return listRecords;
            }

        }

        private static void WriteCsvFile(string fileName, List<TerraCSVModel> records)
        {
            using (var writer = new StreamWriter(fileName, false, Encoding.GetEncoding("windows-1251")))
            {
                var csvConfig = new CsvConfiguration(CultureInfo.GetCultureInfo("ru-RU"))
                {
                    HasHeaderRecord = false,
                    Delimiter = ";",
                };
                using (var csv = new CsvWriter(writer, csvConfig))
                {
                    csv.WriteRecords(records);
                }
            }
        }

        private static void UploadFileToFtp(string ftpUrl, string fileName, string username, string password)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(username, password);

            using (Stream fileStream = File.OpenRead(fileName))
            using (Stream ftpStream = request.GetRequestStream())
            {
                fileStream.CopyTo(ftpStream);
            }
            request.Abort();
        }

        public static void SaveBlank(BacLab_DBEntities context, d_Analyzes Analis, d_Laboratoria laboratoria, string rezultTemplate, string folderMain, d_Staff staff)
        {

            Word.Application wordApp = null;
            Document newDoc = null;
            Document templateDoc = null;

            try
            {
                wordApp = new Word.Application { Visible = false };

                int countTable = 0;
                Table tableDB = null;
                int countUPF = 0;
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileNameTemplate = Path.Combine(folderMain, $"Шаблон{timestamp}.docx");

                if (IsFileAvailable(rezultTemplate))
                    CopyWordDocument(rezultTemplate, fileNameTemplate);

                templateDoc = wordApp.Documents.Open(fileNameTemplate, ReadOnly: false);

                newDoc = wordApp.Documents.Add(DocumentType: WdNewDocumentType.wdNewBlankDocument);
                newDoc.PageSetup.TopMargin = 36;
                newDoc.PageSetup.BottomMargin = 36;
                newDoc.PageSetup.LeftMargin = 36;
                newDoc.PageSetup.RightMargin = 36;

                //для Обл
                if (Analis.d_Subdivisions.id == 9 || Analis.d_Subdivisions.id == 13)
                {
                    newDoc.ActiveWindow.ActivePane.View.SeekView = Word.WdSeekView.wdSeekCurrentPageHeader;
                    newDoc.ActiveWindow.Selection.Font.Name = "Times New Roman";
                    newDoc.ActiveWindow.Selection.Font.Size = 12;
                    Word.HeaderFooter header = newDoc.Sections[1].Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                    Word.Paragraph para = header.Range.Paragraphs.Add();
                    para.Range.Text = "Ф-ПР-7.4.1-БАК/О";
                    para.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
                }

                //різні таблиці шаблону
                if (Analis.d_Subdivisions.id == 9 || Analis.d_Subdivisions.id == 13)
                    AddTable(wordApp, newDoc, templateDoc, 1);
                else
                    AddTable(wordApp, newDoc, templateDoc, 7);
                countTable++;


                //заповнюємо шапку
                ReplaceWordStub(newDoc, "{laboratoria}", laboratoria.nameLab);
                ReplaceWordStub(newDoc, "{adressLaboratoria}", laboratoria.adress);
                ReplaceWordStub(newDoc, "{labNum}", Analis.labNum.ToString());
                ReplaceWordStub(newDoc, "{dateEnd1}", Analis.dateEnd.Value.ToShortDateString());
                ReplaceWordStub(newDoc, "{name}", Analis.d_Patients.name);
                ReplaceWordStub(newDoc, "{year}", Analis.d_Patients.year.ToString());
                ReplaceWordStub(newDoc, "{sex}", Analis.d_Patients.sex != null ? Analis.d_Patients.sex.Equals("ч") ? "чоловік" : "жінка" : "");
                ReplaceWordStub(newDoc, "{adress}", Analis.d_Patients.adress);
                ReplaceWordStub(newDoc, "{institution}", Analis.d_Institution?.name);
                ReplaceWordStub(newDoc, "{department}", Analis.d_Department?.name);
                ReplaceWordStub(newDoc, "{medCard}", Analis.numMedCard);
                ReplaceWordStub(newDoc, "{sentPerson}", Analis.d_SentPerson?.name);
                ReplaceWordStub(newDoc, "{diagnos}", Analis.d_Diagnosis?.name);
                string str1 = Analis.comment?.Length > 0 ? " (" + Analis.comment + ")" : "";
                ReplaceWordStub(newDoc, "{material}", Analis.p_Group_Material_Purpose.d_Material.name + str1);
                ReplaceWordStub(newDoc, "{purpose}", Analis.p_Group_Material_Purpose.d_Purpose.name);
                str1 = Analis.timeSampling != null ? Analis.timeSampling.Value.ToShortTimeString() : "";
                ReplaceWordStub(newDoc, "{dateSampling}", Analis.dateSampling.Value.ToShortDateString() + " " + str1);
                str1 = Analis.timeDelivery != null ? Analis.timeDelivery.Value.ToShortTimeString() : "";
                ReplaceWordStub(newDoc, "{dateDelivery}", Analis.dateDelivery.Value.ToShortDateString() + " " + str1);


                //если ДБ
                p_Analises_DB Analises_DB = Analis.p_Analises_DB.FirstOrDefault();
                if (Analises_DB != null)
                {
                    AddTable(wordApp, newDoc, templateDoc, 2);
                    countTable++;
                    tableDB = newDoc.Tables[countTable];

                    ReplaceWordStub(newDoc, "{bif}", Analises_DB?.bif);
                    ReplaceWordStub(newDoc, "{lac}", Analises_DB?.lac);
                    ReplaceWordStub(newDoc, "{ent}", Analises_DB?.ent);
                    ReplaceWordStub(newDoc, "{coli1}", Analises_DB?.coli1);
                }

                //если не рост
                if (Analis.d_ResTemplate.id != 2)
                {
                    AddTable(wordApp, newDoc, templateDoc, 3);
                    countTable++;
                    ReplaceWordStub(newDoc, "{purpose}", Analis.p_Group_Material_Purpose.d_Purpose.name);
                    ReplaceWordStub(newDoc, "{result}", Analis.d_ResTemplate.name);
                    newDoc.Bookmarks.get_Item("interval").Range.Text = Analis.p_Group_Material_Purpose.d_ReferenceInterval?.name;

                    //Підписи
                    if (Analis.d_Subdivisions.id == 9 || Analis.d_Subdivisions.id == 13)
                        AddTable(wordApp, newDoc, templateDoc, 6);
                    else
                        AddTable(wordApp, newDoc, templateDoc, 8);
                    ReplaceWordStub(newDoc, "{dateEnd2}", Analis.dateEnd.Value.ToShortDateString());
                    ReplaceWordStub(newDoc, "{dateEnd3}", Analis.dateEnd.Value.ToShortDateString());
                    if (Analis.d_Staff == null)
                        Analis.d_Staff = staff;
                    ReplaceWordStub(newDoc, "{nameDoctor}", Analis.d_Staff.abbr);
                    ReplaceWordStub(newDoc, "{verification}", context.d_Staff.Where(c => c.idSubdivisions == Analis.d_Staff.idSubdivisions && c.id_category == 1).FirstOrDefault()?.abbr);

                }
                else //если рост
                {
                    if (Analises_DB == null)
                    {
                        AddTable(wordApp, newDoc, templateDoc, 3);
                        countTable++;
                        ReplaceWordStub(newDoc, "{purpose}", Analis.p_Group_Material_Purpose.d_Purpose.name);
                        newDoc.Bookmarks.get_Item("interval").Range.Text = Analis.p_Group_Material_Purpose.d_ReferenceInterval?.name;
                    }

                    string strRezult = "";
                    bool isFirst = true;
                    string unit = Analis.p_Group_Material_Purpose.unit == true ? " КУО/см³" : " МК/см³";

                    foreach (var itemCulture in Analis.p_Analises_Cultures)
                    {
                        if (itemCulture.p_Analises_Cultures_ABTest.Count() < 1 && Analises_DB != null)
                            continue;

                        string culture = itemCulture.d_Microorganism.name;

                        if (itemCulture.d_Serotype != null)
                            culture = itemCulture.d_Serotype.name;
                        if (itemCulture.d_Biovariant != null)
                            culture += " biovariant " + itemCulture.d_Biovariant.name;
                        if (itemCulture.mrsa == true)
                            culture += " метицилінрезистентний (MRSA)";
                        if (itemCulture.hemolysis == true)
                            culture += " з гемолітичними властивостями";
                        if (itemCulture.proteolysis == true)
                            culture += " з протелоітичними властивостями";
                        if (itemCulture.lacPlusMinus == true)
                            culture += " зі зміненими ферментативними властивостями";
                        if (itemCulture.lacMinus == true)
                            culture += " лактозонегативна";

                        //бо не дукується адекватно
                        if (itemCulture.quantity != null && itemCulture.quantity.Contains("^"))
                        {
                            string str = itemCulture.quantity.Substring(itemCulture.quantity.IndexOf("^"), 2);
                            string str2 = AddExponenta(str);
                            itemCulture.quantity = itemCulture.quantity.Replace(str, str2);
                        }

                        culture += (String.IsNullOrEmpty(itemCulture.quantity)) ? "" : " " + itemCulture.quantity + (!itemCulture.quantity.Contains("ріст") ? unit : "");

                        strRezult += culture + ", ";

                        if (itemCulture.p_Analises_Cultures_ABTest.Count() < 1 && Analises_DB == null)
                            continue;

                        //добавляем основную таблицу
                        AddTable(wordApp, newDoc, templateDoc, 4);
                        countTable++;

                        int idMOGroup = itemCulture.d_Microorganism.g_MicroorganismGroup_Microorganism.FirstOrDefault().d_MicroorganismGroup.id;
                        if (tableDB != null) // ДБ
                        {
                            if (itemCulture.pat == true)//если пат.
                                ReplaceWordStub(newDoc, "{pat}", culture);
                            else if (itemCulture.lacPlusMinus == true)
                                ReplaceWordStub(newDoc, "{coli2}", itemCulture.quantity);
                            else if (itemCulture.lacMinus == true)
                                ReplaceWordStub(newDoc, "{coli3}", itemCulture.quantity);

                            else if ((idMOGroup == 1 && itemCulture.idCulture != 2) || idMOGroup == 2 || idMOGroup == 3) //упф
                            {
                                countUPF++;
                                switch (countUPF)
                                {
                                    case 1: ReplaceWordStub(newDoc, "{upf1}", culture); break;
                                    case 2: ReplaceWordStub(newDoc, "{upf2}", culture); break;
                                    case 3: ReplaceWordStub(newDoc, "{upf3}", culture); break;
                                }

                            }

                            if (idMOGroup == 4)
                                ReplaceWordStub(newDoc, "{st}", culture);
                            if (idMOGroup == 6)
                                ReplaceWordStub(newDoc, "{cand}", culture);
                            if (idMOGroup == 20)
                                ReplaceWordStub(newDoc, "{clostr}", culture);

                        }

                        ReplaceWordStub(newDoc, "{culture}", culture);

                        Table myTable = newDoc.Tables[countTable];
                        int rowCount = 4;
                        bool is4collum = true;
                        int idAntibioticGroup = 0;

                        if (itemCulture.p_Analises_Cultures_ABTest.Count < 1)
                        { myTable.Rows[3].Delete(); myTable.Rows[2].Delete(); }

                        foreach (var itemAB in itemCulture.p_Analises_Cultures_ABTest.OrderBy(c => c.d_TestAndAntibiotic.index))
                        {
                            if (isFirst)
                            {
                                Word.Row firstRow = myTable.Rows[1]; // Отримуємо перший рядок
                                Word.Row newRow = myTable.Rows.Add(firstRow);
                                newRow.Cells[1].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                                newRow.Cells[1].Range.Bold = 0;
                                newRow.Cells[1].Range.Text = "16.2 Результати чутливості виділених культур до хіміотерапевтичних препаратів";
                                rowCount++;
                                isFirst = false;
                            }


                            ////"тільки для скринінгу" друкуємо для онко і 3 пб
                            //if (itemAB.forScrining == true && (Analis.d_Institution?.id != 57 && Analis.d_Institution?.id != 56)) continue;

                            if (itemAB.ferment != true && itemAB.fag != true && itemAB.sinergizm != true && itemAB.comment != true)
                            {
                                myTable.Rows.Add();
                                myTable.Rows[rowCount].Range.Bold = 0;
                                if (idAntibioticGroup == 0) idAntibioticGroup = itemAB.d_TestAndAntibiotic.a_AntibioticGroup.id;
                                if (itemAB.d_TestAndAntibiotic.a_AntibioticGroup.id != idAntibioticGroup)
                                {
                                    myTable.Rows[rowCount].Range.Borders[WdBorderType.wdBorderTop].LineWidth = WdLineWidth.wdLineWidth100pt;
                                    idAntibioticGroup = itemAB.d_TestAndAntibiotic.a_AntibioticGroup.id;
                                }
                                else myTable.Rows[rowCount].Range.Borders[WdBorderType.wdBorderTop].LineWidth = WdLineWidth.wdLineWidth050pt;
                                myTable.Rows[rowCount].Cells[1].Range.Text = itemAB.d_TestAndAntibiotic.name; // результат
                                string str = itemAB.pm.Trim() == "+" ? "чутливий" : (itemAB.pm.Trim() == "-" ? "стійкий" : "проміжний");

                                // додаємо посилання, якщо це природна стійкість - резістентність
                                if (itemAB.abResSen == true)
                                {
                                    if (itemAB.pm.Trim() == "-")
                                        str += "\x00B9";
                                    if (itemAB.pm.Trim() == "+")
                                        str += "\x00B2";
                                }

                                if (itemAB.idTestAndAntibiotic == 79) // колістин
                                {
                                    var colistin = itemAB.p_Analises_Cultures.p_Analises_Cultures_ABDisk.Where(c => c.idConsumable == 279).FirstOrDefault();
                                    if (colistin != null)
                                        if (colistin.mm != "" && colistin.mm != null) //добавляем МІС
                                            str += "/" + colistin.mm.Trim() + "mg/ml";
                                }


                                myTable.Rows[rowCount].Cells[2].Range.Text = str;


                                str = ""; // добавляем дозировки
                                if (itemAB.d_TestAndAntibiotic.doseStandartPerOr != "" && itemAB.d_TestAndAntibiotic.doseStandartPerOr != null && itemAB.onlyVV != true)
                                    str = "п/о:" + itemAB.d_TestAndAntibiotic.doseStandartPerOr + " ";
                                if (itemAB.d_TestAndAntibiotic.doseStandart_Vv != "" && itemAB.d_TestAndAntibiotic.doseStandart_Vv != null && itemAB.onlyPerOr != true)
                                    str += "в/в:" + itemAB.d_TestAndAntibiotic.doseStandart_Vv;
                                else str = str.TrimEnd();
                                myTable.Rows[rowCount].Cells[3].Range.Text = str;
                                myTable.Rows[rowCount].Cells[3].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                                str = "";
                                if (itemAB.d_TestAndAntibiotic.doseHighPerOr != "" && itemAB.d_TestAndAntibiotic.doseHighPerOr != null && itemAB.onlyVV != true)
                                    str = "п/о:" + itemAB.d_TestAndAntibiotic.doseHighPerOr + " ";
                                if (itemAB.d_TestAndAntibiotic.doseHigh_Vv != "" && itemAB.d_TestAndAntibiotic.doseHigh_Vv != null && itemAB.onlyPerOr != true)
                                    str += "в/в:" + itemAB.d_TestAndAntibiotic.doseHigh_Vv;
                                else str = str.TrimEnd();
                                myTable.Rows[rowCount].Cells[4].Range.Text = str;
                                myTable.Rows[rowCount].Cells[4].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;


                                rowCount++;
                            }
                            else if (itemAB.ferment == true)
                            {
                                myTable.Rows.Add();
                                myTable.Rows[rowCount].Range.Bold = 1;
                                myTable.Rows[rowCount].Range.Borders[WdBorderType.wdBorderTop].LineWidth = WdLineWidth.wdLineWidth100pt;
                                myTable.Rows[rowCount].Cells[1].Range.Text = itemAB.d_TestAndAntibiotic.name;
                                myTable.Rows[rowCount].Cells[2].Range.Text = itemAB.pm.Trim().Equals("+") ? "продукує" : itemAB.pm.Trim().Equals("-") ? "не продукує" : "не визначалось";
                                myTable.Rows[rowCount].Cells[3].Range.Text = itemAB.d_TestAndAntibiotic.note;
                                myTable.Rows[rowCount].Cells[3].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                                if (is4collum)
                                { myTable.Rows[rowCount].Cells[3].Merge(myTable.Rows[rowCount].Cells[4]); is4collum = false; }
                                rowCount++;
                            }
                            else if (itemAB.fag == true)
                            {
                                myTable.Rows.Add();
                                myTable.Rows[rowCount].Range.Bold = 1;
                                myTable.Rows[rowCount].Range.Borders[WdBorderType.wdBorderTop].LineWidth = WdLineWidth.wdLineWidth100pt;
                                myTable.Rows[rowCount].Cells[1].Range.Text = itemAB.d_TestAndAntibiotic.name;
                                myTable.Rows[rowCount].Cells[2].Range.Text = itemAB.pm;
                                myTable.Rows[rowCount].Cells[3].Range.Text = itemAB.d_TestAndAntibiotic.note;
                                myTable.Rows[rowCount].Cells[3].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                                if (is4collum)
                                { myTable.Rows[rowCount].Cells[3].Merge(myTable.Rows[rowCount].Cells[4]); is4collum = false; }
                                rowCount++;
                            }
                            else if (itemAB.sinergizm == true)
                            {
                                myTable.Rows.Add();
                                myTable.Rows[rowCount].Range.Bold = 1;
                                myTable.Rows[rowCount].Range.Borders[WdBorderType.wdBorderTop].LineWidth = WdLineWidth.wdLineWidth100pt;
                                myTable.Rows[rowCount].Cells[1].Range.Text = itemAB.pm.Trim().Equals("+") ? "спостерігається" : "не спостерігається";
                                myTable.Rows[rowCount].Cells[2].Range.Text = itemAB.d_TestAndAntibiotic.note;
                                myTable.Rows[rowCount].Cells[2].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                                if (is4collum)
                                { myTable.Rows[rowCount].Cells[2].Merge(myTable.Rows[rowCount].Cells[4]); is4collum = false; }
                                rowCount++;
                            }
                            else if (itemAB.comment == true)
                            {
                                myTable.Rows.Add();
                                myTable.Rows[rowCount].Range.Bold = 0;
                                myTable.Rows[rowCount].Range.Borders[WdBorderType.wdBorderTop].LineWidth = WdLineWidth.wdLineWidth150pt;
                                myTable.Rows[rowCount].Cells[1].Range.Text = itemAB.d_TestAndAntibiotic.note;
                                myTable.Rows[rowCount].Cells[1].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                                if (is4collum)
                                {
                                    myTable.Rows[rowCount].Cells[1].Merge(myTable.Rows[rowCount].Cells[4]); is4collum = false;
                                }
                                rowCount++;
                            }
                        }

                    }

                    if (tableDB != null) // ДБ підчищаємо
                    {
                        ReplaceWordStub(newDoc, "{pat}", "-");
                        ReplaceWordStub(newDoc, "{coli1}", "-");
                        ReplaceWordStub(newDoc, "{coli2}", "-");
                        ReplaceWordStub(newDoc, "{coli3}", "-");
                        ReplaceWordStub(newDoc, "{upf1}", "-");
                        ReplaceWordStub(newDoc, "{upf2}", "-");
                        ReplaceWordStub(newDoc, "{upf3}", "-");
                        ReplaceWordStub(newDoc, "{st}", "-");
                        ReplaceWordStub(newDoc, "{cand}", "-");
                        ReplaceWordStub(newDoc, "{clostr}", "-");
                    }

                    if (Analises_DB == null)
                        ReplaceWordStub(newDoc, "{result}", "Виділено: " + strRezult.Substring(0, strRezult.Length - 2) + ".");

                    //добавляем разьяснения
                    AddTable(wordApp, newDoc, templateDoc, 5);

                    string riskfactors = "";
                    if (Analis.cvc != null) riskfactors += "Центральний катетер, ";
                    if (Analis.pc != null) riskfactors += "Периферичний катетер, ";
                    if (Analis.uc != null) riskfactors += "Сечовий катетер, ";
                    if (Analis.ssi != null) riskfactors += "Інфекції області хірургічного втручання, ";
                    if (Analis.uti != null) riskfactors += "Інфекції сечовивідних шляхів, ";
                    if (Analis.vap != null) riskfactors += "Вентилятор-ассоциированная пневмония, ";
                    if (Analis.bacteriemia != null) riskfactors += "Бактеріємія ";
                    riskfactors = riskfactors.EndsWith(", ") ? riskfactors.Substring(0, riskfactors.Length - 2) : riskfactors;
                    ReplaceWordStub(newDoc, "{riskfactors}", riskfactors);

                    //Підписи
                    if (Analis.d_Subdivisions.id == 9 || Analis.d_Subdivisions.id == 13)
                        AddTable(wordApp, newDoc, templateDoc, 6);
                    else
                        AddTable(wordApp, newDoc, templateDoc, 8);
                    ReplaceWordStub(newDoc, "{dateEnd2}", Analis.dateEnd.Value.ToShortDateString());
                    ReplaceWordStub(newDoc, "{dateEnd3}", Analis.dateEnd.Value.ToShortDateString());
                    ReplaceWordStub(newDoc, "{nameDoctor}", Analis.d_Staff.abbr);
                    ReplaceWordStub(newDoc, "{verification}", context.d_Staff.Where(c => c.idSubdivisions == Analis.d_Staff.idSubdivisions && c.id_category == 1).FirstOrDefault()?.abbr);

                }

                newDoc.ActiveWindow.ActivePane.View.SeekView = Word.WdSeekView.wdSeekCurrentPageFooter;
                newDoc.ActiveWindow.Selection.Font.Name = "Times New Roman";
                newDoc.ActiveWindow.Selection.Font.Size = 12;
                newDoc.ActiveWindow.Selection.TypeText("Сторінка ");
                Object TotalPages = Word.WdFieldType.wdFieldNumPages;
                Object CurrentPage = Word.WdFieldType.wdFieldPage;
                newDoc.ActiveWindow.Selection.HeaderFooter.LinkToPrevious = false;
                newDoc.ActiveWindow.Selection.Fields.Add(newDoc.ActiveWindow.Selection.Range, ref CurrentPage);
                newDoc.ActiveWindow.Selection.TypeText(" з ");
                newDoc.ActiveWindow.Selection.Fields.Add(newDoc.ActiveWindow.Selection.Range, ref TotalPages);
                newDoc.ActiveWindow.Selection.TypeText("\t\t                Результат аналізу № " + Analis.labNum.ToString() + " від " + Analis.dateEnd.Value.ToShortDateString());


                var docRange = newDoc.Range();
                docRange.Font.Size = 12;
                string fileName = folderMain + "\\" + Analis.labNum.ToString() + " " + Analis.dateDelivery.Value.ToShortDateString() + " " + Analis.d_Patients.name + ".pdf";

                newDoc.SaveAs2(fileName, WdSaveFormat.wdFormatPDF);

                templateDoc?.Close();
                newDoc?.Close(WdSaveOptions.wdDoNotSaveChanges);
                wordApp?.Quit();

                //сохраняем файл в базе
                byte[] data;
                using (FileStream fs = new FileStream(fileName, FileMode.Open))
                {
                    data = new byte[fs.Length];
                    fs.Read(data, 0, data.Length);
                }
                Analis.rezult = data;
                Analis.sendAnalis = true;

                context.SaveChanges();
                File.Delete(fileNameTemplate);
                File.Delete(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Бланк № " + Analis.labNum + " не збережено " + "\n" + ex.Message + " " + ex.StackTrace);
                newDoc?.Close(WdSaveOptions.wdDoNotSaveChanges);
                templateDoc?.Close();
                wordApp?.Quit();
            }
        }

        public static void ShowRezult(byte[] data, string folderMain)
        {
            try
            {
                bool isPdf = FileFormatHelper.IsPdf(data);
                bool isDocx = FileFormatHelper.IsDocx(data);
                string fileName = folderMain + "\\" + "Результат";
                if (isPdf)
                {
                    fileName += ".pdf";
                    using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(data, 0, data.Length);
                    }
                    try
                    {
                        Process acrobat = new Process();
                        acrobat.StartInfo.FileName = fileName;
                        acrobat.StartInfo.UseShellExecute = true; // Открыть через ассоциацию по умолчанию (обычно Acrobat)
                        acrobat.Start();
                    }
                    catch (Exception ex)
                    {
                        Message.Ok("Не вдалося відкрити PDF. Acrobat Reader не встановлено або виникла інша помилка.\n" + ex.Message, "MsgDialog");
                    }
                }
                else if (isDocx)
                {
                    fileName += ".docx";
                    using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(data, 0, data.Length);
                    }

                    var wordApp = new Word.Application();
                    var tempDoc = wordApp.Documents.Open(fileName, ReadOnly: true);
                    wordApp.Visible = true;
                    wordApp.Activate();
                    wordApp.WindowState = Word.WdWindowState.wdWindowStateMinimize;
                    wordApp.WindowState = Word.WdWindowState.wdWindowStateMaximize;
                }
                else
                {
                    Message.Ok("Невідомий формат файлу результату", "MsgDialog");
                    return;
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        public static string SendEmail(BacLab_DBEntities context, d_Analyzes Analis, d_Laboratoria laboratoria, string folderMain)
        {
            try
            {
                var colEmails = context.g_Institution_Email_Print.
                    Where(c => c.idInstitution == Analis.d_Institution.id && c.name != "" && c.name != null && c.isSend == true);
                if (colEmails.Count() > 0)
                {
                    List<string> listMailTo = new List<string>();
                    foreach (var item in colEmails)
                        if (item.idDepartment == null || item.idDepartment == Analis.d_Department?.id)
                            listMailTo.Add(item.name);

                    if (listMailTo.Count() > 0)
                    {
                        byte[] data = Analis.rezult;
                        bool isPdf = FileFormatHelper.IsPdf(data);
                        bool isDocx = FileFormatHelper.IsDocx(data);
                        string fileName = folderMain + "\\" + Analis.labNum.ToString() + " " + Analis.dateDelivery.Value.ToShortDateString() + " " + Analis.d_Patients.name;
                        if (!isPdf)
                            fileName += ".docx";
                        else
                            fileName += ".pdf";

                        using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                        {
                            fs.Write(data, 0, data.Length);
                        }

                        bool rez = SendMail("smtp.gmail.com", laboratoria.email, laboratoria.parol, listMailTo, Analis.dateDelivery.Value.ToShortDateString() + " " + Analis.d_Patients.name, fileName);

                        File.Delete(fileName);
                        if (rez) { Analis.isIssued = true; return "Відправлено"; }
                        else
                            return "Аналіз № " + Analis.labNum + " не відправлен на пошту\nПеревірте наявність інтернету, доступ до електроної скриньки лабораторії";

                    }
                    else
                    {
                        return "Одержувача не знайдено";
                    }
                }
                else
                {
                    return "Одержувача не знайдено";
                }

            }
            catch (Exception ex)
            {
                return "Аналіз № " + Analis.labNum + " не відправлен на пошту" + "\n" + ex.Message + " " + ex.StackTrace;
            }
        }

        public static bool PrintRezult(BacLab_DBEntities context, d_Analyzes Analis, d_Laboratoria laboratoria, string folderMain)
        {
            try
            {
                byte[] data = Analis.rezult;
                bool isPdf = FileFormatHelper.IsPdf(data);
                bool isDocx = FileFormatHelper.IsDocx(data);
                bool isA5 = (bool)laboratoria.A5;
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string pdfFileName = Path.Combine(folderMain, $"Результат{timestamp}.pdf");
                string docxfileName = Path.Combine(folderMain, $"Результат{timestamp}.docx");


                if (isA5)
                {
                    if (isPdf)
                    {
                        using (FileStream fs = new FileStream(pdfFileName, FileMode.Create, FileAccess.Write))
                        {
                            fs.Write(data, 0, data.Length);
                        }

                        try
                        {
                            // Використання Spire.PDF для конвертації PDF у Word
                            Spire.Pdf.PdfDocument pdf = new Spire.Pdf.PdfDocument();
                            pdf.LoadFromFile(pdfFileName);
                            pdf.SaveToFile(docxfileName, Spire.Pdf.FileFormat.DOCX);
                            pdf.Close();
                            File.Delete(pdfFileName);

                            RemoveRedTextFromWord(docxfileName);

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Не вдалося конвертувати PDF у Word для друку.\n" + ex.Message, "MsgDialog");
                            return false;
                        }

                    }
                    else if (isDocx)
                    {
                        using (FileStream fs = new FileStream(docxfileName, FileMode.Create, FileAccess.Write))
                        {
                            fs.Write(data, 0, data.Length);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Невідомий формат файлу результату", "MsgDialog");
                        return false;
                    }

                    var wordApp = new Word.Application() { };
                    var tempDoc = wordApp.Documents.Open(docxfileName, ReadOnly: false);

                    tempDoc.PrintOut(true, false, WdPrintOutRange.wdPrintAllDocument,
                                Item: WdPrintOutItem.wdPrintDocumentContent, Copies: "1", Pages: "",
                                PageType: WdPrintOutPages.wdPrintAllPages, PrintToFile: false, Collate: true,
                                ManualDuplexPrint: false, PrintZoomPaperWidth: 8395.2, PrintZoomPaperHeight: 12556.8);
                    Thread.Sleep(5000);
                    tempDoc.Close();
                    wordApp.Quit();
                    File.Delete(docxfileName);
                }
                else
                {
                    if (isPdf)
                    {
                        using (FileStream fs = new FileStream(pdfFileName, FileMode.Create, FileAccess.Write))
                        {
                            fs.Write(data, 0, data.Length);
                        }
                        try
                        {
                            Process printProcess = new Process();
                            printProcess.StartInfo.FileName = pdfFileName;
                            printProcess.StartInfo.UseShellExecute = true;
                            printProcess.StartInfo.Verb = "Print";
                            printProcess.Start();
                            printProcess.WaitForExit(10000); // Очікуємо до 10 секунд
                            printProcess.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Не вдалося надрукувати PDF. Acrobat Reader не встановлено або виникла інша помилка.\n" + ex.Message, "MsgDialog");
                            return false;
                        }
                    }
                    else if (isDocx)
                    {
                        using (FileStream fs = new FileStream(docxfileName, FileMode.Create, FileAccess.Write))
                        {
                            fs.Write(data, 0, data.Length);
                        }
                        var wordApp = new Word.Application() { Visible = false };
                        var tempDoc = wordApp.Documents.Open(docxfileName, ReadOnly: true);
                        tempDoc.PrintOut(true, false, WdPrintOutRange.wdPrintAllDocument,
                                Item: WdPrintOutItem.wdPrintDocumentContent, Copies: "1", Pages: "",
                                PageType: WdPrintOutPages.wdPrintAllPages, PrintToFile: false, Collate: true,
                                ManualDuplexPrint: false);
                        Thread.Sleep(5000);
                        tempDoc.Close();
                        wordApp.Quit();
                        File.Delete(docxfileName);
                    }
                    else
                    {
                        MessageBox.Show("Невідомий формат файлу результату", "MsgDialog");
                        return false;
                    }

                }
                //Analis.isPrint = true;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace, "MsgDialog");
                return false;
            }
        }

        public static bool SendMail(string smtpServer, string mailfrom, string password, List<string> mailto, string caption, string attachFile = null)
        {
            System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
            SmtpClient client = new SmtpClient();
            try
            {
                mail.From = new MailAddress(mailfrom);
                foreach (var item in mailto)
                    mail.To.Add(new MailAddress(item));
                mail.Subject = caption;
                if (!string.IsNullOrEmpty(attachFile))
                    mail.Attachments.Add(new Attachment(attachFile));
                client.Host = smtpServer;
                client.Port = 587;
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Credentials = new NetworkCredential(mailfrom, password);
                client.Send(mail);
                client.Dispose();
                mail.Dispose();
                return true;
            }
            catch (Exception)
            {
                client.Dispose();
                mail.Dispose();
                return false;
            }
        }
        public static bool IsFileAvailable(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    fs.Close();
                }
                return true;
            }
            catch (IOException)
            {
                // Файл зайнятий іншим процесом
                return false;
            }
        }
        private static void AddTable(Word.Application wordApp, Word.Document newDoc, Word.Document tempDoc, int numTab = -1)
        {
            try
            {

                object docStart = newDoc.Content.End - 1;
                object docEnd = newDoc.Content.End;
                tempDoc.Tables[numTab].Range.Copy();
                var rng = newDoc.Range(ref docStart, ref docEnd);
                rng.Paste();
                rng.Font.Name = "Times New Roman";
                newDoc.Paragraphs.Add();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
                wordApp.Quit();
            }

        }
        private static string AddExponenta(string str)
        {
            try
            {
                switch (str)
                {
                    case "^1": str = "\x00B9"; break;
                    case "^2": str = "\x00B2"; break;
                    case "^3": str = "\x00B3"; break;
                    case "^4": str = "\x2074"; break;
                    case "^5": str = "\x2075"; break;
                    case "^6": str = "\x2076"; break;
                    case "^7": str = "\x2077"; break;
                    case "^8": str = "\x2078"; break;
                    case "^9": str = "\x2079"; break;
                }
                return str;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
                return null;
            }

        }
        private static bool ReplaceWordStub(Document doc, string stub, string text)
        {
            try
            {
                var range = doc.Content;
                range.Find.ClearFormatting();
                if (text == null) text = "";
                if (text.Length > 255) text = text.Substring(0, 255);
                bool rez = range.Find.Execute(FindText: stub, ReplaceWith: text);
                return rez;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
                return false;
            }

        }
        private static string ChangeExponenta(string quantityCulture)
        {
            string quantity;
            if (quantityCulture.Contains("¹")) quantity = quantityCulture.Replace("¹", "^1");
            else if (quantityCulture.Contains("²")) quantity = quantityCulture.Replace("²", "^2");
            else if (quantityCulture.Contains("³")) quantity = quantityCulture.Replace("³", "^3");
            else if (quantityCulture.Contains("⁴")) quantity = quantityCulture.Replace("⁴", "^4");
            else if (quantityCulture.Contains("⁵")) quantity = quantityCulture.Replace("⁵", "^5");
            else if (quantityCulture.Contains("⁶")) quantity = quantityCulture.Replace("⁶", "^6");
            else if (quantityCulture.Contains("⁷")) quantity = quantityCulture.Replace("⁷", "^7");
            else if (quantityCulture.Contains("⁸")) quantity = quantityCulture.Replace("⁸", "^8");
            else if (quantityCulture.Contains("⁹")) quantity = quantityCulture.Replace("⁹", "^9");
            else quantity = quantityCulture;

            return quantity;
        }
        // Метод для копіювання Word-документа без відкриття його у Word
        public static void CopyWordDocument(string sourcePath, string destinationPath)
        {
            try
            {
                File.Copy(sourcePath, destinationPath, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не вдалося скопіювати документ: " + ex.Message, "Помилка");
            }
        }
        // Додайте цей метод для видалення всіх написів червоним кольором у Word-документі
        public static void RemoveRedTextFromWord(string filePath)
        {
            var wordApp = new Word.Application();
            Document doc = null;
            try
            {
                doc = wordApp.Documents.Open(filePath, ReadOnly: false);
                foreach (Word.Range range in doc.StoryRanges)
                {
                    Word.Range currentRange = range;
                    do
                    {
                        foreach (Word.Paragraph paragraph in currentRange.Paragraphs)
                        {
                            Word.Range paraRange = paragraph.Range;
                            if (paraRange.Font.Color == Word.WdColor.wdColorRed)
                            {
                                paraRange.Text = "";
                            }
                        }
                        currentRange = currentRange.NextStoryRange;
                    } while (currentRange != null);
                }
                doc.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при видаленні червоного тексту: " + ex.Message);
            }
            finally
            {
                doc?.Close();
                wordApp.Quit();
            }
        }

        // Метод для отримання інформації про таблиці, які посилаються на видаляємий запис DeleteRow
        public static string PrintReferencingEntities(BacLab_DBEntities context, string entitySetName, int id)
        {
            try
            {
                string result = "";
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                var container = objectContext.MetadataWorkspace.GetEntityContainer(objectContext.DefaultContainerName, DataSpace.CSpace);

                var referencing = container.BaseEntitySets
                    .OfType<EntitySet>()
                    .SelectMany(set => set.ElementType.NavigationProperties
                        .Where(np => np.ToEndMember.GetEntityType().Name == entitySetName)
                        .Select(np => new { From = set.Name, Navigation = np.Name, Set = set }))
                    .ToList();

                foreach (var item in referencing)
                {
                    // Знаходимо тип сутності для таблиці, що посилається
                    string rez = "";
                    try
                    {
                        var entityType = context.GetType().GetProperty(item.From)?.PropertyType.GetGenericArguments().FirstOrDefault();
                        if (entityType == null) continue;
                        var dbSet = context.Set(entityType);
                        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
                        var navProperty = System.Linq.Expressions.Expression.Property(parameter, item.Navigation);
                        if (navProperty == null) continue;

                        var idProperty = System.Linq.Expressions.Expression.Property(navProperty, "id");
                        if (idProperty == null) continue;
                        var idValue = System.Linq.Expressions.Expression.Constant(id);
                        var equal = System.Linq.Expressions.Expression.Equal(idProperty, idValue);
                        var lambda = System.Linq.Expressions.Expression.Lambda(equal, parameter);

                        var countMethod = typeof(Queryable).GetMethods()
                            .First(m => m.Name == "Count" && m.GetParameters().Length == 2)
                            .MakeGenericMethod(entityType);

                        int count = (int)countMethod.Invoke(null, new object[] { dbSet, lambda });

                        rez += "\n" + ($"Таблиця: {item.From}, Зв'язок: {item.Navigation}, Кількість: {count}");
                    }
                    catch (Exception)
                    {
                    }

                    result += rez;
                }
                return result;
            }
            catch (Exception ex)
            {
                return "Помилка при отриманні зв'язків." + " " + ex.Message + " " + ex.StackTrace;
            }

        }

        public static bool IsValidOfficeDocument(byte[] data)
        {
            if (data == null || data.Length < 4)
                return false;

            // Проверка на ZIP-архив (современные форматы .docx, .xlsx - это ZIP)
            bool isZip = data[0] == 0x50 && data[1] == 0x4B &&
                         (data[2] == 0x03 || data[2] == 0x05 || data[2] == 0x07) &&
                         (data[3] == 0x04 || data[3] == 0x06 || data[3] == 0x08);

            // Проверка на старый формат DOC (CFB - Compound File Binary)
            bool isOldDoc = data.Length >= 8 &&
                            data[0] == 0xD0 && data[1] == 0xCF &&
                            data[2] == 0x11 && data[3] == 0xE0 &&
                            data[4] == 0xA1 && data[5] == 0xB1 &&
                            data[6] == 0x1A && data[7] == 0xE1;

            return isZip || isOldDoc;
        }

        public static void OpenWordDocument(string filePath)
        {
            Word.Application wordApp = null;
            Document templateDoc = null;
            try
            {
                wordApp = new Word.Application();
                templateDoc = wordApp.Documents.Open(filePath, ReadOnly: false);
                wordApp.Visible = true;
                wordApp.Activate();
                wordApp.WindowState = WdWindowState.wdWindowStateMinimize;
                wordApp.WindowState = WdWindowState.wdWindowStateMaximize;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
                templateDoc?.Close();
                wordApp?.Quit();
            }
        }

        public static void OpenExcelDocument(string filePath)
        {
            Microsoft.Office.Interop.Excel.Application excelApp = null;
            Microsoft.Office.Interop.Excel.Workbook workbook = null;
            try
            {
                excelApp = new Microsoft.Office.Interop.Excel.Application();
                workbook = excelApp.Workbooks.Open(filePath, ReadOnly: false);
                excelApp.Visible = true;
                excelApp.WindowState = Microsoft.Office.Interop.Excel.XlWindowState.xlMaximized;
            }
            catch (Exception ex)
            {
                Message.Ok(ex.Message + "\n" + ex.StackTrace, "MsgDialog");
                workbook?.Close();
                excelApp?.Quit();
            }
        }

        public enum OfficeFileType
        {
            Unknown,
            Word,
            WordLegacy,
            Excel,
            ExcelLegacy
        }

        public static OfficeFileType GetOfficeFileType(byte[] data)
        {
            if (data == null || data.Length < 8)
                return OfficeFileType.Unknown;

            // Проверка на старый формат (CFB)
            bool isOldOffice = data[0] == 0xD0 && data[1] == 0xCF &&
                               data[2] == 0x11 && data[3] == 0xE0 &&
                               data[4] == 0xA1 && data[5] == 0xB1 &&
                               data[6] == 0x1A && data[7] == 0xE1;

            if (isOldOffice)
            {
                // Различаем Word и Excel по внутренним маркерам
                // Это упрощенная проверка, для точности нужен полный парсинг CFB
                return OfficeFileType.WordLegacy; // или ExcelLegacy
            }

            // Проверка на ZIP (современные форматы)
            bool isZip = data[0] == 0x50 && data[1] == 0x4B &&
                         (data[2] == 0x03 || data[2] == 0x05) &&
                         (data[3] == 0x04 || data[3] == 0x06);

            if (isZip)
            {
                // Определяем тип по содержимому ZIP
                return DetectModernOfficeType(data);
            }

            return OfficeFileType.Unknown;
        }

        private static OfficeFileType DetectModernOfficeType(byte[] data)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(data))
                using (System.IO.Compression.ZipArchive archive = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Read))
                {
                    // Word содержит word/document.xml
                    if (archive.Entries.Any(e => e.FullName.StartsWith("word/")))
                        return OfficeFileType.Word;

                    // Excel содержит xl/workbook.xml
                    if (archive.Entries.Any(e => e.FullName.StartsWith("xl/")))
                        return OfficeFileType.Excel;
                }
            }
            catch
            {
                return OfficeFileType.Unknown;
            }

            return OfficeFileType.Unknown;
        }


        public static void Log(BacLab_DBEntities context, d_Analyzes Analis, d_Staff staff, int idAction, bool saveOldRez)
        {
            try
            {
                context.l_log.Add(new l_log()
                {
                    date = DateTime.Now,
                    datetime = DateTime.Now,
                    d_Staff = staff,
                    idAction = idAction,
                    labNum = Analis.labNum,
                    namePacient = Analis.d_Patients.name,
                    dateDelivery = Analis.dateDelivery,
                    rezultOld = saveOldRez ? Analis.rezult : null
                });

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при записі в лог: " + ex.Message, "Помилка");
            }
        }
    }
}
