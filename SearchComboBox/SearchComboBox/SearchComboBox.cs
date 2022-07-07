using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SearchComboBox
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SearchComboBox"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SearchComboBox;assembly=SearchComboBox"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误:
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[浏览查找并选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:SearchComboBox/>
    ///
    /// </summary>
    public class SearchComboBox : Control
    {
        static SearchComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchComboBox), new FrameworkPropertyMetadata(typeof(SearchComboBox)));
        }

        public static readonly DependencyProperty ItemSourceProperty = DependencyProperty.Register(
            "ItemSource", typeof(IList), typeof(SearchComboBox), new PropertyMetadata(default(IList), ((o, args) =>
            {
                //如果传入的是onserb的集合要注册集合更改事件，

                if (o is SearchComboBox searchComboBox)
                {
                    if (args.OldValue is INotifyCollectionChanged oldObservable)
                    {
                        //取消事件的注册,避免事件导致的无法回收
                        oldObservable.CollectionChanged -= searchComboBox.ItemSourceChanged;
                    }
                    if (args.NewValue is INotifyCollectionChanged observable)
                    {
                        observable.CollectionChanged += searchComboBox.ItemSourceChanged;
                    }

                    searchComboBox.Search();
                }
            })));

        private void ItemSourceChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            //
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null)
                {
                    foreach (var eNewItem in e.OldItems)
                    {
                        SelectedItems.Remove(eNewItem);
                    }
                }
            }
            Search();
        }

        public IList ItemSource
        {
            get { return (IList)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
            "SelectedItems", typeof(IList), typeof(SearchComboBox), new PropertyMetadata(default(IList), ((o, args) =>
            {
                //如果传入的是onserb的集合要注册集合更改事件，

                if (o is SearchComboBox searchComboBox)
                {
                    if (args.OldValue is INotifyCollectionChanged oldObservable)
                    {
                        //取消事件的注册,避免事件导致的无法回收
                        oldObservable.CollectionChanged -= searchComboBox.SelectedItemChanged;
                    }
                    if (args.NewValue is INotifyCollectionChanged observable)
                    {
                        observable.CollectionChanged += searchComboBox.SelectedItemChanged;
                    }

                    searchComboBox.Search();
                }
            })));

        private void SelectedItemChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                //
                if (e.OldItems != null)
                {
                    foreach (var eOldItem in e.OldItems)
                    {
                        var remover = SearchList.FirstOrDefault(x => x.Item == eOldItem);
                        if (remover != null)
                        {
                            remover.IsSelected = false;
                        }
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null)
                {
                    foreach (var newItem in e.NewItems)
                    {
                        var newer = SearchList.FirstOrDefault(x => x.Item == newItem);
                        if (newer != null && !newer.IsSelected)
                        {
                            Search();
                        }
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                Search();
            }
        }

        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem", typeof(object), typeof(SearchComboBox), new PropertyMetadata(default(object), ((o, args) =>
            {
                if (o is SearchComboBox searchComboBox)
                {
                    if (searchComboBox.SelectedItems == null)
                    {

                        searchComboBox.SelectedItems = new ObservableCollection<object>();
                    }

                    if (searchComboBox.SelectedItems.Contains(args.OldValue))
                    {
                        searchComboBox.SelectedItems.Remove(args.OldValue);
                    }

                    searchComboBox.SelectedItems.Add(args.NewValue);

                }
            })));

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public static readonly DependencyProperty SelectModeProperty = DependencyProperty.Register(
            "SelectMode", typeof(SelectMode), typeof(SearchComboBox), new PropertyMetadata(default(SelectMode)));

        public SelectMode SelectMode
        {
            get { return (SelectMode)GetValue(SelectModeProperty); }
            set { SetValue(SelectModeProperty, value); }
        }

        public static readonly DependencyProperty SearchModeProperty = DependencyProperty.Register(
            "SearchMode", typeof(SearchMode), typeof(SearchComboBox), new PropertyMetadata(default(SearchMode)));

        public SearchMode SearchMode
        {
            get { return (SearchMode)GetValue(SearchModeProperty); }
            set { SetValue(SearchModeProperty, value); }
        }
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            "ItemTemplate", typeof(DataTemplate), typeof(SearchComboBox), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty SearchListProperty = DependencyProperty.Register(
            "SearchList", typeof(ObservableCollection<ItemWrap>), typeof(SearchComboBox), new PropertyMetadata(default(ObservableCollection<ItemWrap>)));

        public ObservableCollection<ItemWrap> SearchList
        {
            get { return (ObservableCollection<ItemWrap>)GetValue(SearchListProperty); }
            set { SetValue(SearchListProperty, value); }
        }

        public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register(
            "SearchText", typeof(string), typeof(SearchComboBox), new PropertyMetadata(default(string), ((o, args) =>
            {
                if (o is SearchComboBox searchComboBox && args.NewValue is string str)
                {
                    searchComboBox.Search();
                }
            })));

        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        public static readonly DependencyProperty SearchRuleProperty = DependencyProperty.Register(
            "SearchRule", typeof(Func<object, string, bool>), typeof(SearchComboBox), new PropertyMetadata(new Func<object, string, bool>(
                (o, s) =>
                {
                    return true;
                })));

        public Func<object, string, bool> SearchRule
        {
            get { return (Func<object, string, bool>)GetValue(SearchRuleProperty); }
            set { SetValue(SearchRuleProperty, value); }
        }

        private Func<object, string, bool> _searchFunc = new Func<object, string, bool>(((o, s) => true));
        public static readonly DependencyProperty SearchExpressionProperty = DependencyProperty.Register(
            "SearchExpression", typeof(Expression<Func<object, string, bool>>), typeof(SearchComboBox), new PropertyMetadata(
                (o, args) =>
                {
                    if (args.NewValue is Expression<Func<object, string, bool>> ex && o is SearchComboBox searchCombo)
                    {
                        searchCombo._searchFunc = ex.Compile();
                    }
                }));

        public Expression<Func<object, string, bool>> SearchExpression
        {
            get { return (Expression<Func<object, string, bool>>)GetValue(SearchExpressionProperty); }
            set { SetValue(SearchExpressionProperty, value); }
        }

        public static readonly DependencyProperty IsOpenSearchProperty = DependencyProperty.Register(
            "IsOpenSearch", typeof(Visibility), typeof(SearchComboBox), new PropertyMetadata(default(Visibility)));

        public Visibility IsOpenSearch
        {
            get { return (Visibility)GetValue(IsOpenSearchProperty); }
            set { SetValue(IsOpenSearchProperty, value); }
        }
        private Popup _searchPopup;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _searchPopup = this.Template.FindName("SerarchPanel", this) as Popup;
            var itemplate = this.Template.Resources["DefaultItemTemplate"];
            if (ItemTemplate == null)
            {
                ItemTemplate = itemplate as DataTemplate;
            }
        }

        public void Search()
        {
            if (ItemSource == null)
            {
                return;
            }
            SearchList?.Clear();
            if (string.IsNullOrEmpty(SearchText))
            {
                var search = new ObservableCollection<ItemWrap>();
                if (ItemSource != null)
                {
                    foreach (var VARIABLE in ItemSource)
                    {
                        search.Add(new ItemWrap() { Item = VARIABLE, IsSelected = SelectedItems?.Contains(VARIABLE) == true });
                    }
                }


                SearchList = search;
            }
            else
            {
                var search = new ObservableCollection<ItemWrap>();
                //走给的方法
                foreach (var VARIABLE in ItemSource)
                {
                    switch (SearchMode)
                    {
                        case SearchMode.Expression:
                            if (_searchFunc(VARIABLE, SearchText))
                            {
                                search.Add(new ItemWrap() { Item = VARIABLE, IsSelected = SelectedItems?.Contains(VARIABLE) == true });
                            }
                            break;
                        case SearchMode.Func:
                            if (SearchRule(VARIABLE, SearchText))
                            {
                                search.Add(new ItemWrap() { Item = VARIABLE, IsSelected = SelectedItems?.Contains(VARIABLE) == true });
                            }
                            break;
                    }


                }
                SearchList = search;
            }
        }
        public void RemoveItem(object sender, EventArgs para)
        {
            var select = (sender as FrameworkElement)?.DataContext;
            if (select != null)
            {

                SelectedItems.Remove(select);
                var search = SearchList.FirstOrDefault(x => x.Item == select);
                if (search != null)
                {
                    search.IsSelected = false;
                }
            }
        }
        public void SingleSelected(object sender, EventArgs para)
        {
            if (SelectMode != SelectMode.Single)
            {
                return;
            }
            if (SelectedItems == null)
            {
                SelectedItems = new ObservableCollection<object>();
            }

            var select = (sender as FrameworkElement)?.DataContext as ItemWrap;
            if (select != null)
            {
                SelectedItem = select.Item;
                if (!SelectedItems.Contains(select.Item))
                {
                    SelectedItems.Clear();
                    SelectedItems.Add(select.Item);
                }
                _searchPopup.IsOpen = false;
            }


        }

        public void MutilSelected(object sender, EventArgs args)
        {
            if (SelectMode != SelectMode.Multi)
            {
                return;
            }
            if (SelectedItems == null)
            {
                SelectedItems = new ObservableCollection<object>();
            }
            if (sender is CheckBox check)
            {
                //选中添加，取消移除
                var select = check.DataContext as ItemWrap;
                if (select != null)
                {
                    if (check.IsChecked == true)
                    {
                        SelectedItems?.Add(select.Item);
                    }
                    else
                    {
                        SelectedItems?.Remove(select.Item);
                    }
                }
            }
        }
    }
    public enum SelectMode
    {
        Single,
        Multi,
    }

    public enum SearchMode
    {
        Expression,
        Func,
    }

    public class ItemWrap : INotifyPropertyChanged
    {
        private object _item;
        public object Item
        {
            get { return _item; }
            set
            {
                _item = value;
                OnPropertyChanged();
            }
        }

        public bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
