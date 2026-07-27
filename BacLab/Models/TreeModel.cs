using System;
using System.Collections.ObjectModel;

namespace BacLab.Models
{
    public class TreeModel : IDisposable
    {
        public object Content { get; set; }
        public ObservableCollection<TreeModel> Items { get; set; } = new ObservableCollection<TreeModel>();

        public string Name { get; set; }
        public TreeModel ParentModel { get; set; }
        public d_Analyzes Analyzes { get; set; }
        public p_Analises_Mediums AnalizesMediums { get; set; }
        public p_Analises_Mediums_Date AnalisesMediumsDate { get; set; }
        public p_Analises_Mediums_Date_Colonies AnalisesMediumsDateColonies { get; set; }

        public void Dispose()
        {
            // Рекурсивно очищаємо дочірні елементи
            foreach (var item in Items)
                item.Dispose();
            // Очищаємо Content, якщо це UserControl з IDisposable
            (Content as IDisposable)?.Dispose();
            Items.Clear();
            Content = null;
        }

    }
}
