using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace BacLab.Models
{
    public class Spirt : INotifyPropertyChanged
    {
        
        int id;
        int? idConsumablesWritingOff;
        d_ConsumableWritingOff consumableWritingOff;
        DateTime? data;

        int? kysh_analis;
        int? kysh_search;
        int? klin_analis;
        int? klin_search;
        int? kap_analis;
        int? kap_search;
        int? prof_analis;
        int? prof_search;
        
        int? vsyogoSearch;
        double? spirtSearch;

        int? zmyvy_BGKP_analis;
        int? zmyvy_BGKP_search;
        int? zmyvy_St_analis;
        int? zmyvy_St_search;
        int? zmyvy_Pat_analis;
        int? zmyvy_Pat_search;
        int? sterylnist_analis;
        int? sterylnist_search;
        int? air_analis;
        int? air_search;
        int? water_analis;
        int? water_search;
        int? waterRiver_analis;
        int? waterRiver_search;
        int? water_stichna_analis;
        int? water_stichna_search;
        int? waterPool_analis;
        int? waterPool_search;
        int? waterBeach_analis;
        int? waterBeach_search;
        int? water_xol_analis;
        int? water_xol_search;
        int? grunt_analis;
        int? grunt_search;
        int? des_contam_analis;
        int? des_contam_search;
        int? des_chutlyvist_analis;
        int? des_chutlyvist_search;
        int? controlyAvtoclavov_analis;
        int? controlyAvtoclavov_search;
        int? liki_analis;
        int? liki_search;
        int? food_analis;
        int? food_search;
        int? botulism_analis;
        int? botulism_search;
        int? water_fagy_analis;
        int? water_fagy_search;
        int? water_paraz_analis;
        int? water_paraz_search;
        int? waterRiver_paraz_analis;
        int? waterRiver_paraz_search;
        int? grunt_paraz_analis;
        int? grunt_paraz_search;
        
        int? vsyogoSBD;
        double? spirtSBD;

        int? controly;
        double? spirtControly;
        int? mazky;
        double? spirtMazky;
        int? krany;
        double? spirtKrany;
        int? filtrApparat;
        double? spirtFiltrApparat;
        int? zadachi;
        double? spirtZadachi;
        double? spirtObrobkaHandTable;
        double? spirtObrobkaTermostat;
        double? spirtPrygotuvannyReactyviv;
        double? spirtEntomolog;
        double? spirtVnutryshnePeremishenya;
        double? spirtOther;
        string commentOther;

        double? spirtSumOther;

        double? spirtVsogo;

        double? zalyshok;
        string zalyshokStr;
        bool isEnabled;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public int Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
        public DateTime? Data { get { return data; } set { data = value; OnPropertyChanged("Data"); } }
        public int? IdConsumableWritingOff { get { return idConsumablesWritingOff; } set { idConsumablesWritingOff = value; OnPropertyChanged("IdConsumablesWritingOff"); } }
        public d_ConsumableWritingOff ConsumableWritingOff { get { return consumableWritingOff; } set { consumableWritingOff = value; OnPropertyChanged("ConsumableWritingOff"); } }
       
        List<d_Coefficient> listCoefficient;

        public int? Klin_analis { get { return klin_analis; } set { klin_analis = value; OnPropertyChanged("Klin_analis"); }}
        public int? Klin_search { get { return klin_search; } set { klin_search = value; OnPropertyChanged("Klin_search"); SumSearch(); }}
        
        public int? Kysh_analis { get { return kysh_analis; } set { kysh_analis = value; OnPropertyChanged("Kysh_analis");} }
        public int? Kysh_search { get { return kysh_search; } set { kysh_search = value; OnPropertyChanged("Kysh_search"); SumSearch(); }}
        
        public int? Prof_analis { get { return prof_analis; } set { prof_analis = value; OnPropertyChanged("Prof_analis"); } }
        public int? Prof_search { get { return prof_search; } set { prof_search = value; OnPropertyChanged("Prof_search"); SumSearch(); }}
       
        public int? Kap_analis { get { return kap_analis; } set { kap_analis = value; OnPropertyChanged("Kap_analis"); } }
        public int? Kap_search { get { return kap_search; } set { kap_search = value; OnPropertyChanged("Kap_search"); SumSearch(); }}
        
      

        public int? VsyogoSearch { get { return vsyogoSearch; } set { vsyogoSearch = value;
                SpirtSearch = vsyogoSearch*listCoefficient.FirstOrDefault(c => c.id == 21).koef / 1000.0;
                OnPropertyChanged("VsyogoSearch");}}
       
        public double? SpirtSearch { get { return spirtSearch; } set { spirtSearch = value;
                SumSpirt();
                OnPropertyChanged("SpirtSearch");} }

        public int? Zmyvy_BGKP_analis { get { return zmyvy_BGKP_analis; } set { zmyvy_BGKP_analis = value;
                Zmyvy_BGKP_search = zmyvy_BGKP_analis *(int)listCoefficient.FirstOrDefault(c => c.id == 3).koef;
                OnPropertyChanged("Zmyvy_BGKP_analis"); }}
        public int? Zmyvy_BGKP_search { get { return zmyvy_BGKP_search; } set { zmyvy_BGKP_search = value; OnPropertyChanged("Zmyvy_BGKP_search"); SumSBD_search();   }}
       
        public int? Zmyvy_St_analis { get { return zmyvy_St_analis; } set { zmyvy_St_analis = value; 
                Zmyvy_St_search = zmyvy_St_analis * (int)listCoefficient.FirstOrDefault(c => c.id == 5).koef;
                OnPropertyChanged("Zmyvy_St_analis"); } }
        public int? Zmyvy_St_search { get { return zmyvy_St_search; } set { zmyvy_St_search = value; OnPropertyChanged("Zmyvy_St_search"); SumSBD_search();  } }
        
       
        public int? Zmyvy_Pat_analis { get { return zmyvy_Pat_analis; } set { zmyvy_Pat_analis = value; 
                Zmyvy_Pat_search = zmyvy_Pat_analis * (int)listCoefficient.FirstOrDefault(c => c.id == 4).koef;
                OnPropertyChanged("Zmyvy_Pat_analis"); } }
        public int? Zmyvy_Pat_search { get { return zmyvy_Pat_search; } set { zmyvy_Pat_search = value; OnPropertyChanged("Zmyvy_Pat_search"); SumSBD_search();  } }
        
        public int? Sterylnist_analis { get { return sterylnist_analis; } set { sterylnist_analis = value;
                Sterylnist_search = sterylnist_analis * (int)listCoefficient.FirstOrDefault(c => c.id == 6).koef;
                OnPropertyChanged("Sterylnist_analis"); } }
        public int? Sterylnist_search { get { return sterylnist_search; } set { sterylnist_search = value; OnPropertyChanged("Sterylnist_search"); SumSBD_search(); } }
       
        public int? Air_analis { get { return air_analis; } set { air_analis = value;
                Air_search = air_analis * (int)listCoefficient.FirstOrDefault(c => c.id == 7).koef;
                OnPropertyChanged("Air_analis");}}
        public int? Air_search { get { return air_search; } set { air_search = value; OnPropertyChanged("Air_search"); SumSBD_search(); } }
       
        public int? Water_analis { get { return water_analis; } set { water_analis = value; 
                Water_search = water_analis * (int)listCoefficient.FirstOrDefault(c => c.id == 9).koef;
                Krany = water_analis;
                FiltrApparat = (water_analis ?? 0) + (waterRiver_analis ?? 0);
                OnPropertyChanged("Water_analis");  } }
        public int? Water_search { get { return water_search; } set { water_search = value; OnPropertyChanged("Water_search"); SumSBD_search(); } }
        public int? Water_fagy_analis { get { return water_fagy_analis; } set { water_fagy_analis = value; 
                Water_fagy_search = water_fagy_analis * (int)listCoefficient.FirstOrDefault(c => c.id == 26).koef;
                OnPropertyChanged("Water_fagy_analis"); }
        }
        public int? Water_fagy_search { get { return water_fagy_search; } set { water_fagy_search = value; OnPropertyChanged("Water_fagy_search"); SumSBD_search(); } }
        public int? WaterRiver_analis { get { return waterRiver_analis; } set { waterRiver_analis = value;
                WaterRiver_search = waterRiver_analis * (int)listCoefficient.FirstOrDefault(c => c.id == 10).koef;
                FiltrApparat = (water_analis ?? 0) + (waterRiver_analis ?? 0);
                OnPropertyChanged("WaterRiver_analis");  } }
        public int? WaterRiver_search { get { return waterRiver_search; } set { waterRiver_search = value; OnPropertyChanged("WaterRiver_search"); SumSBD_search(); } }
        
        public int? Water_stichna_analis { get { return water_stichna_analis; } set { water_stichna_analis = value;
                Water_stichna_search = water_stichna_analis * (int)listCoefficient.FirstOrDefault(c => c.id == 11).koef;
                OnPropertyChanged("Water_stichna_analis"); } }
        public int? Water_stichna_search { get { return water_stichna_search; } set { water_stichna_search = value; OnPropertyChanged("Water_stichna_search"); SumSBD_search(); } }
       
         public int? WaterPool_analis { get { return waterPool_analis; } set { waterPool_analis = value;
                WaterPool_search = waterPool_analis * (int)listCoefficient.FirstOrDefault(c => c.id == 29).koef;
                OnPropertyChanged("WaterPool_analis"); } }
        public int? WaterPool_search { get { return waterPool_search; } set { waterPool_search = value; OnPropertyChanged("WaterPool_search"); SumSBD_search(); } }
       
           public int? WaterBeach_analis { get { return waterBeach_analis; } set { waterBeach_analis = value;
                WaterBeach_search = waterBeach_analis * (int)listCoefficient.FirstOrDefault(c => c.id == 27).koef;
                OnPropertyChanged("WaterBeach_analis"); } }
        public int? WaterBeach_search { get { return waterBeach_search; } set { waterBeach_search = value; OnPropertyChanged("WaterBeach_search"); SumSBD_search(); } }
       
        public int? Water_xol_analis {  get { return water_xol_analis; } set { water_xol_analis = value;
                Water_xol_search = water_xol_analis * (int)listCoefficient.FirstOrDefault(c => c.id == 2).koef;
                OnPropertyChanged("Water_xol_analis");}}
        public int? Water_xol_search { get { return water_xol_search; } set { water_xol_search = value; OnPropertyChanged("Water_xol_search"); SumSBD_search(); } }
      
        public int? Grunt_analis { get { return grunt_analis; } set { grunt_analis = value;
                Grunt_search = grunt_analis * (int)listCoefficient.FirstOrDefault(c => c.id == 8).koef;
                OnPropertyChanged("Grunt_analis"); } }
        public int? Grunt_search { get { return grunt_search; } set { grunt_search = value; OnPropertyChanged("Grunt_search"); SumSBD_search(); } }
      
        public int? Des_contam_analis { get { return des_contam_analis; } set { des_contam_analis = value; 
                Des_contam_search = des_contam_analis * (int)listCoefficient.FirstOrDefault(c => c.id == 19).koef;
                OnPropertyChanged("Des_contam_analis");  } }
        public int? Des_contam_search { get { return des_contam_search; } set { des_contam_search = value; OnPropertyChanged("Des_contam_search"); SumSBD_search(); } }

        public int? Des_chutlyvist_analis { get { return des_chutlyvist_analis; } set{des_chutlyvist_analis = value;
                Des_chutlyvist_search = des_chutlyvist_analis * (int)listCoefficient.FirstOrDefault(c => c.id == 25).koef;
                OnPropertyChanged("Des_chutlyvist_analis");}}
        public int? Des_chutlyvist_search { get { return des_chutlyvist_search; } set { des_chutlyvist_search = value; OnPropertyChanged("Des_chutlyvist_search"); SumSBD_search(); } }

        public int? ControlyAvtoclavov_analis { get { return controlyAvtoclavov_analis; } set{controlyAvtoclavov_analis = value;
                ControlyAvtoclavov_search = controlyAvtoclavov_analis * (int)listCoefficient.FirstOrDefault(c => c.id == 25).koef;
                OnPropertyChanged("ControlyAvtoclavov_analis");}}
        public int? ControlyAvtoclavov_search { get { return controlyAvtoclavov_search; } set { controlyAvtoclavov_search = value; OnPropertyChanged("ControlyAvtoclavov_search"); SumSBD_search(); } }

        public int? Liki_analis { get { return liki_analis; } set { liki_analis = value;
                Liki_search = liki_analis * (int)listCoefficient.FirstOrDefault(c => c.id == 13).koef;
                OnPropertyChanged("Liki_analis");  } }
        public int? Liki_search { get { return liki_search; } set { liki_search = value; OnPropertyChanged("Liki_search"); SumSBD_search(); } }
      
        public int? Food_analis { get { return food_analis; } set { food_analis = value; 
                Food_search = food_analis * (int)listCoefficient.FirstOrDefault(c => c.id == 14).koef;
                OnPropertyChanged("Food_analis"); } }
        public int? Food_search { get { return food_search; } set { food_search = value; OnPropertyChanged("Food_search"); SumSBD_search(); } }
       
        public int? Botulism_analis { get { return botulism_analis; } set { botulism_analis = value; 
                Botulism_search = botulism_analis * (int)listCoefficient.FirstOrDefault(c => c.id == 15).koef;
                OnPropertyChanged("Botulism_analis"); } }
        public int? Botulism_search { get { return botulism_search; } set { botulism_search = value; OnPropertyChanged("Botulism_search"); SumSBD_search(); } }

       
        public int? Water_paraz_analis { get { return water_paraz_analis; } set { water_paraz_analis = value; 
                Water_paraz_search = water_paraz_analis * (int)listCoefficient.FirstOrDefault(c => c.id == 22).koef;
                OnPropertyChanged("Water_paraz_analis"); }}
        public int? Water_paraz_search { get { return water_paraz_search; } set { water_paraz_search = value; OnPropertyChanged("Water_paraz_search"); SumSBD_search(); } }

        public int? WaterRiver_paraz_analis { get { return waterRiver_paraz_analis; } set { waterRiver_paraz_analis = value; 
                WaterRiver_paraz_search = waterRiver_paraz_analis * (int)listCoefficient.FirstOrDefault(c => c.id == 23).koef;
                OnPropertyChanged("WaterRiver_paraz_analis"); }
        }
        public int? WaterRiver_paraz_search { get { return waterRiver_paraz_search; } set { waterRiver_paraz_search = value; OnPropertyChanged("WaterRiver_paraz_search"); SumSBD_search(); } }

        public int? Grunt_paraz_analis { get { return grunt_paraz_analis; } set { grunt_paraz_analis = value; 
                Grunt_paraz_search = grunt_paraz_analis * (int)listCoefficient.FirstOrDefault(c => c.id == 24).koef;
                OnPropertyChanged("Grunt_paraz_analis"); }
        }
        public int? Grunt_paraz_search { get { return grunt_paraz_search; } set { grunt_paraz_search = value; OnPropertyChanged("Grunt_paraz_search"); SumSBD_search(); } }

        public int? VsyogoSBD {get { return vsyogoSBD; } set { vsyogoSBD = value;
                SpirtSBD = vsyogoSBD * listCoefficient.FirstOrDefault(c => c.id == 21).koef / 1000.0;
                OnPropertyChanged("VsyogoSBD");} }
        public double? SpirtSBD { get { return spirtSBD; } set { spirtSBD = value; OnPropertyChanged("SpirtSBD"); SumSpirt(); } }
        

      
        public int? Controly { get { return controly; } set { controly = value;
                SpirtControly = controly / 1000.0;
                OnPropertyChanged("SpirtControly"); } }
        public double? SpirtControly { get { return spirtControly; } set { spirtControly = value; OnPropertyChanged("SpirtControly"); SumSpirtOther(); } }
     
        public int? Mazky { get { return mazky; } set { mazky = value; 
                SpirtMazky = mazky * listCoefficient.FirstOrDefault(c => c.id == 16).koef / 1000.0;
                OnPropertyChanged("Mazky"); } }
        public double? SpirtMazky { get { return spirtMazky; } set { spirtMazky = value; OnPropertyChanged("SpirtMazky"); SumSpirtOther(); }}
        
        public int? Krany { get { return krany; } set { krany = value; 
                    SpirtKrany = listCoefficient.FirstOrDefault(c => c.id == 20)?.koef * krany/1000.0;
                    OnPropertyChanged("Krany");} } 
        public double? SpirtKrany { get { return spirtKrany; } set { spirtKrany = value; OnPropertyChanged("SpirtKrany"); SumSpirtOther(); } }
        public int? FiltrApparat { get { return filtrApparat; } set { filtrApparat = value;
                SpirtFiltrApparat = filtrApparat * listCoefficient.FirstOrDefault(c => c.id == 17).koef / 1000.0;
                OnPropertyChanged("FiltrApparat"); } }
        public double? SpirtFiltrApparat { get { return spirtFiltrApparat; } set { spirtFiltrApparat = value; OnPropertyChanged("SpirtFiltrApparat"); SumSpirtOther(); } }
       
        public int? Zadachi { get { return zadachi; } set { zadachi = value; 
                SpirtZadachi = zadachi * listCoefficient.FirstOrDefault(c => c.id == 18).koef/1000.0;
                OnPropertyChanged("Zadachi"); } }
        public double? SpirtZadachi { get { return spirtZadachi; } set { spirtZadachi = value; OnPropertyChanged("SpirtZadachi"); SumSpirtOther(); } }
        
        public double? SpirtObrobkaHandTable { get { return spirtObrobkaHandTable; } set { spirtObrobkaHandTable = value; OnPropertyChanged("SpirtObrobkaHandTable"); SumSpirtOther(); } }
        public double? SpirtObrobkaTermostat { get { return spirtObrobkaTermostat; } set { spirtObrobkaTermostat = value; OnPropertyChanged("SpirtObrobkaTermostat"); SumSpirtOther(); } }
        public double? SpirtPrygotuvannyReactyviv { get { return spirtPrygotuvannyReactyviv; } set { spirtPrygotuvannyReactyviv = value; OnPropertyChanged("SpirtPrygotuvannyReactyviv"); SumSpirtOther(); } }
        public double? SpirtEntomolog { get { return spirtEntomolog; } set { spirtEntomolog = value; OnPropertyChanged("SpirtEntomolog"); SumSpirtOther(); } }
        public double? SpirtVnutryshnePeremishenya { get { return spirtVnutryshnePeremishenya; } set { spirtVnutryshnePeremishenya = value; OnPropertyChanged("SpirtVnutryshnePeremishenya"); SumSpirtOther(); } }
        public double? SpirtOther { get { return spirtOther; } set { spirtOther = value; OnPropertyChanged("SpirtOther"); SumSpirtOther(); } }
        public string CommentOther { get { return commentOther; } set { commentOther = value; OnPropertyChanged("CommentOther"); } }

        public double? SpirtSumOther { get { return spirtSumOther; } set { spirtSumOther = value; OnPropertyChanged("SpirtSumOther"); SumSpirt(); } }
        public double? SpirtVsyogo { get { return spirtVsogo; } set { spirtVsogo = value; 
                if(spirtVsogo > zalyshok)
                    IsEnabled = false;
                else
                    IsEnabled = true;
                OnPropertyChanged("SpirtVsyogo"); } }
       
        public double? Zalyshok { get { return zalyshok; } set { zalyshok = value; OnPropertyChanged("Zalyshok"); } }
        public string ZalyshokStr { get { return zalyshokStr; } set { zalyshokStr = value; OnPropertyChanged("ZalyshokStr"); } }
        public bool IsEnabled { get { return isEnabled; } set { isEnabled = value; OnPropertyChanged("IsEnabled"); } }
       
        private void SumSearch()
        {
            VsyogoSearch = (Klin_search ?? 0) + (Kysh_search ?? 0) + (Prof_search ?? 0) + (Kap_search ?? 0);
        }

        private void SumSBD_search()
        {
            VsyogoSBD = (Zmyvy_BGKP_search ?? 0) + (Zmyvy_Pat_search ?? 0) + (Zmyvy_St_search ?? 0) + (Sterylnist_search ?? 0) +
                (Air_search ?? 0)+ (Water_search ?? 0) + (Water_fagy_search ?? 0) + (WaterRiver_search ?? 0) + (Water_stichna_search ?? 0) 
                + (WaterPool_search ?? 0)+ (WaterBeach_search ?? 0) + (Water_xol_search ?? 0)
                + (Grunt_search ?? 0) + (Des_contam_search ?? 0) + (Des_chutlyvist_search ?? 0) + (ControlyAvtoclavov_search ?? 0) 
                + (Liki_search ?? 0) + (Food_search ?? 0) + (Botulism_search ?? 0) + (Water_paraz_search ?? 0) + (WaterRiver_paraz_search ?? 0) + (Grunt_paraz_search ?? 0);
        }

        private void SumSpirtOther()
        {
            SpirtSumOther = 
                (SpirtControly ?? 0) + (SpirtMazky ?? 0) + (SpirtKrany ?? 0) + (SpirtFiltrApparat ?? 0) + (SpirtZadachi ?? 0) +
                (SpirtObrobkaHandTable ?? 0) + (SpirtObrobkaTermostat ?? 0) + (SpirtPrygotuvannyReactyviv ?? 0) + (SpirtEntomolog ?? 0) 
                + (SpirtVnutryshnePeremishenya ?? 0) + (SpirtOther ?? 0);
        }

        private void SumSpirt()
        {
            double total = (SpirtSearch ?? 0) + (SpirtSBD ?? 0) + (SpirtSumOther ?? 0);
            SpirtVsyogo = Math.Round(total, 3);
        }

        public Spirt(List<d_Coefficient> listCoefficient, double? zalyshok)
        {
            this.listCoefficient = listCoefficient;
            this.zalyshok = zalyshok;
            Data = DateTime.Now.Date.AddDays(-1);
        }


        public Spirt(d_Spirt spirt, List<d_Coefficient> listCoefficient, double? zalyshok)
        {
            try
            {
                this.listCoefficient = listCoefficient;
                this.Zalyshok = zalyshok;
                ZalyshokStr = "Перевищує залишок "+ zalyshok.ToString() + " л.";
                Id = spirt.id;
                Data = spirt.data;
                IdConsumableWritingOff = spirt.idConsumableWritingOff;
                ConsumableWritingOff = spirt.d_ConsumableWritingOff;

                VsyogoSearch = spirt.vsyogoSearch;
                SpirtSearch = spirt.spirtSearch;

                Zmyvy_BGKP_analis = spirt.zmyvy_BGKP_analis;
                Zmyvy_BGKP_search = spirt.zmyvy_BGKP_search;
                Zmyvy_St_analis = spirt.zmyvy_St_analis;
                Zmyvy_St_search = spirt.zmyvy_St_search;
                Zmyvy_Pat_analis = spirt.zmyvy_Pat_analis;
                Zmyvy_Pat_search = spirt.zmyvy_Pat_search;
                Sterylnist_analis = spirt.sterylnist_analis;
                Sterylnist_search = spirt.sterylnist_search;
                Air_analis = spirt.air_analis;
                Air_search = spirt.air_search;
                Water_analis = spirt.water_analis;
                Water_search = spirt.water_search;
                Water_fagy_analis = spirt.water_fagy_analis;
                Water_fagy_search = spirt.water_fagy_search;
                WaterRiver_analis = spirt.waterRiver_analis;
                WaterRiver_search = spirt.waterRiver_search;
                Water_stichna_analis = spirt.water_stichna_analis;
                Water_stichna_search = spirt.water_stichna_search;
                Water_xol_analis = spirt.water_xol_analis;
                Water_xol_search = spirt.water_xol_search;
                Grunt_analis = spirt.grunt_analis;
                Grunt_search = spirt.grunt_search;
                Des_contam_analis = spirt.des_contam_analis;
                Des_contam_search = spirt.des_contam_search;
                Des_chutlyvist_analis = spirt.des_chutlyvist_analis;
                Des_chutlyvist_search = spirt.des_chutlyvist_search;
                Liki_analis = spirt.liki_analis;
                Liki_search = spirt.liki_search;
                Food_analis = spirt.food_analis;
                Food_search = spirt.food_search;
                Botulism_analis = spirt.botulism_analis;
                Botulism_search = spirt.botulism_search;
                Water_paraz_analis = spirt.water_paraz_analis;
                Water_paraz_search = spirt.water_paraz_search;
                WaterRiver_paraz_analis = spirt.waterRiver_paraz_analis;
                WaterRiver_paraz_search = spirt.waterRiver_paraz_search;
                Grunt_paraz_analis = spirt.grunt_paraz_analis;
                Grunt_paraz_search = spirt.grunt_paraz_search;

                VsyogoSBD = spirt.vsyogoSBD;
                SpirtSBD = spirt.spirtSBD;

                Controly = spirt.controly;
                SpirtControly = spirt.spirtControly;
                Mazky = spirt.mazky;
                SpirtMazky = spirt.spirtMazky;
                Krany = spirt.krany;
                SpirtKrany = spirt.spirtKrany;
                FiltrApparat = spirt.filtrApparat;
                SpirtFiltrApparat = spirt.spirtFiltrAppart;
                Zadachi = spirt.zadachi;
                SpirtZadachi = spirt.spirtZadachi;
                SpirtObrobkaHandTable = spirt.spirtObrobkaHandTable;
                SpirtObrobkaTermostat = spirt.spirtObrobkaTermostat;
                SpirtPrygotuvannyReactyviv = spirt.spirtPrygotuvannyReactyviv;
                SpirtVnutryshnePeremishenya = spirt.spirtVnutryshnePeremishenya;
                SpirtEntomolog = spirt.spirtEntomolog;
                SpirtOther = spirt.spirtOther;
                CommentOther = spirt.commentOther;


                SpirtSumOther = spirt.spirtSumOther;
                SpirtVsyogo = spirt.spirtVsyogo;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace, "Помилка");
            }
            
        }

        public d_Spirt SaveSpirt(d_ConsumableWritingOff consumableWritingOff, d_Spirt spirt)
        {
            try
            {
                spirt.data = this.Data;

                spirt.vsyogoSearch = this.VsyogoSearch;
                spirt.spirtSearch = this.SpirtSearch;

                spirt.zmyvy_BGKP_analis = this.Zmyvy_BGKP_analis;
                spirt.zmyvy_BGKP_search = this.Zmyvy_BGKP_search;
                spirt.zmyvy_St_analis = this.Zmyvy_St_analis;
                spirt.zmyvy_St_search = this.Zmyvy_St_search;
                spirt.zmyvy_Pat_analis = this.Zmyvy_Pat_analis;
                spirt.zmyvy_Pat_search = this.Zmyvy_Pat_search;
                spirt.sterylnist_analis = this.Sterylnist_analis;
                spirt.sterylnist_search = this.Sterylnist_search;
                spirt.air_analis = this.Air_analis;
                spirt.air_search = this.Air_search;
                spirt.water_analis = this.Water_analis;
                spirt.water_search = this.Water_search;
                spirt.water_fagy_analis = this.Water_fagy_analis;
                spirt.water_fagy_search = this.Water_fagy_search;
                spirt.waterRiver_analis = this.WaterRiver_analis;
                spirt.waterRiver_search = this.WaterRiver_search;
                spirt.water_stichna_analis = this.Water_stichna_analis;
                spirt.water_stichna_search = this.Water_stichna_search;
                spirt.water_xol_analis = this.Water_xol_analis;
                spirt.water_xol_search = this.Water_xol_search;
                spirt.grunt_analis = this.Grunt_analis;
                spirt.grunt_search = this.Grunt_search;
                spirt.des_contam_analis = this.Des_contam_analis;
                spirt.des_contam_search = this.Des_contam_search;
                spirt.des_chutlyvist_analis = this.Des_chutlyvist_analis;
                spirt.des_chutlyvist_search = this.Des_chutlyvist_search;
                spirt.liki_analis = this.Liki_analis;
                spirt.liki_search = this.Liki_search;
                spirt.food_analis = this.Food_analis;
                spirt.food_search = this.Food_search;
                spirt.botulism_analis = this.Botulism_analis;
                spirt.botulism_search = this.Botulism_search;
                spirt.water_paraz_analis = this.Water_paraz_analis;
                spirt.water_paraz_search = this.Water_paraz_search;
                spirt.waterRiver_paraz_analis = this.WaterRiver_paraz_analis;
                spirt.waterRiver_paraz_search = this.WaterRiver_paraz_search;
                spirt.grunt_paraz_analis = this.Grunt_paraz_analis;
                spirt.grunt_paraz_search = this.Grunt_paraz_search;

                spirt.vsyogoSBD = this.VsyogoSBD;
                spirt.spirtSBD = this.SpirtSBD;

                spirt.controly = this.Controly;
                spirt.spirtControly = this.SpirtControly;
                spirt.mazky = this.Mazky;
                spirt.spirtMazky = this.SpirtMazky;
                spirt.krany = this.Krany;
                spirt.spirtKrany = this.SpirtKrany;
                spirt.filtrApparat = this.FiltrApparat;
                spirt.spirtFiltrAppart = this.SpirtFiltrApparat;
                spirt.zadachi = this.Zadachi;
                spirt.spirtZadachi = this.SpirtZadachi;
                spirt.spirtObrobkaHandTable = this.SpirtObrobkaHandTable;
                spirt.spirtObrobkaTermostat = this.SpirtObrobkaTermostat;
                spirt.spirtPrygotuvannyReactyviv = this.SpirtPrygotuvannyReactyviv;
                spirt.spirtEntomolog = this.SpirtEntomolog;
                spirt.spirtVnutryshnePeremishenya = this.SpirtVnutryshnePeremishenya;   
                spirt.spirtOther = this.SpirtOther;
                spirt.commentOther = this.CommentOther;

                spirt.spirtSumOther = this.SpirtSumOther;
                spirt.spirtVsyogo = this.SpirtVsyogo;

                return spirt;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace, "Помилка");
                return null;
            }
        }
    }
}
