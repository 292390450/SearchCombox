using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SearchComboBoxTest
{
    public class MainViewModel:INotifyPropertyChanged
    {

        public ObservableCollection<A> Sources { get; set; } = new ObservableCollection<A>()
            { new A { Age = 10, Name = "Tom" },new A(){Age = 15,Name = "jerrui"} };

        public ObservableCollection<A> Selected { get; set; } = new ObservableCollection<A>();

        public Expression<Func<object, string, bool>> SearchExpression { get; set; } =
            ((o, s) => ((A)o).Name.Contains(s));

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class A
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
