using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TreeViewApp.ViewModels
{
    public class DataProvider
    {
        private readonly int _everyLoadCountIncresing = 25;
        private int _lastLoadCount = 0;

        public async Task<List<Category>> LoadCategoriesAsync()
        {
            return await Task.Run(() =>
            {
                //Thread.Sleep(5000);

                var categories = new List<Category>();
                var loadCount = _lastLoadCount + _everyLoadCountIncresing;

                var rnd = new Random();
                for (var i = 1; i <= loadCount; i++)
                {
                    var category = new Category($"Category{i}");
                    var itemCount = rnd.Next(1, 10);
                    for (var j = 0; j < itemCount; j++)
                    {
                        var itemNumber = rnd.Next(1, 30);
                        var item = new Item($"Item{itemNumber}");

                        if (category.Items.All(_ => _.Name != item.Name))
                        {
                            category.Items.Add(item);
                        }
                    }

                    categories.Add(category);
                }

                _lastLoadCount = loadCount;

                return categories;
            });
        }
    }


    public class Category : ObservableObject
    {
        public string Name { get; }
        public ObservableCollection<Item> Items { get; } = new ObservableCollection<Item>();

        public Category(string name)
        {
            Name = name;
        }
    }

    public class Item : ObservableObject
    {
        public string Name { get; }

        public Item(string name)
        {
            Name = name;
        }
    }



    public class MainViewModel : ObservableObject
    {
        private DataProvider _dataProvider = new DataProvider();
        public ObservableCollection<Category> Categories { get; } = new ObservableCollection<Category>();


        private object _selectedItem;

        public object SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }


        public IAsyncRelayCommand LoadCommand { get; }


        public MainViewModel()
        {
            LoadCommand = new AsyncRelayCommand(LoadAsync);
            LoadAsync();
        }

        private async Task LoadAsync()
        {
            SelectedItem = Categories.FirstOrDefault()?.Items.FirstOrDefault();
            if(Categories.Count > 0)
                return;


            Item prevSelectedItem = null;
            Category prevSelectedCategory = null;

            if (SelectedItem is Category category)
            {
                prevSelectedItem = null;
                prevSelectedCategory = category;
            }
            else if (SelectedItem is Item item)
            {
                prevSelectedItem = item;
                prevSelectedCategory = Categories.First(_ => _.Items.Contains(item));
            }


            var reloadedCategories = await _dataProvider.LoadCategoriesAsync();

            Categories.Clear();
            foreach (var c in reloadedCategories)
            {
                Categories.Add(c);
            }


            if (prevSelectedItem != null)
            {
                SelectedItem = Categories.FirstOrDefault(_ => _.Name == prevSelectedCategory.Name)
                    ?.Items.FirstOrDefault(_ => _.Name == prevSelectedItem.Name);
            }
            else if (prevSelectedCategory != null)
            {
                SelectedItem = Categories.FirstOrDefault(_ => _.Name == prevSelectedCategory.Name);
            }
        }
    }
}
