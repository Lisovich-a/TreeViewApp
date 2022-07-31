using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TreeViewApp.ViewModels.V2
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


    public abstract class HierarchicalObject : ObservableObject
    {
        private bool _isSearchResult;
        public bool IsSearchResult
        {
            get => _isSearchResult;
            set
            {
                SetProperty(ref _isSearchResult, value);
            }
        }


        private bool _isHostingSearchResult;
        public bool IsHostingSearchResult
        {
            get => _isHostingSearchResult;
            set
            {
                SetProperty(ref _isHostingSearchResult, value);
            }
        }


        public RelayCommand SelectCommand { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value == true)
                {
                    if (SelectedObject != null)
                    {
                        SelectedObject.IsSelected = false;
                    }
                    SelectedObject = this;
                }

                SetProperty(ref _isSelected, value);
            }
        }

        public static HierarchicalObject SelectedObject;

        public string Name { get; }

        protected HierarchicalObject(string name)
        {
            Name = name;
            SelectCommand = new RelayCommand(Select);
        }

        private void Select()
        {
            IsSelected = true;
        }
    }

    public class Category : HierarchicalObject
    {
        public RelayCommand OpenCloseCommand { get; }

        private bool _isOpen;
        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                SetProperty(ref _isOpen, value);
                foreach (var item in Items)
                {
                    item.IsVisible = _isOpen;
                }
            }
        }

        public ObservableCollection<Item> Items { get; } = new ObservableCollection<Item>();

        public Category(string name) : base(name)
        {
            OpenCloseCommand = new RelayCommand(OpenClose);
        }

        private void OpenClose()
        {
            IsOpen = !IsOpen;
        }
    }

    public class Item : HierarchicalObject
    {
        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                SetProperty(ref _isVisible, value);
            }
        }

        public Item(string name) : base(name)
        {

        }
    }



    public class MainViewModel : ObservableObject
    {
        private bool _inSearchMode;
        public bool InSearchMode
        {
            get => _inSearchMode;
            set
            {
                SetProperty(ref _inSearchMode, value);
            }
        }

        private DataProvider _dataProvider = new DataProvider();
        public ObservableCollection<Category> Categories { get; } = new ObservableCollection<Category>();

        public IAsyncRelayCommand LoadCommand { get; }



        private string _textToSearch;
        public string TextToSearch
        {
            get => _textToSearch;
            set
            {
                Search(value);
                SetProperty(ref _textToSearch, value);
            }
        }


        private void Search(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                InSearchMode = false;
            }
            else
            {
                InSearchMode = true;
                foreach (var category in Categories)
                {
                    category.IsSearchResult = category.Name.Contains(text, StringComparison.InvariantCultureIgnoreCase);
                    category.IsHostingSearchResult = false;

                    foreach (var item in category.Items)
                    {
                        item.IsHostingSearchResult = category.IsSearchResult;

                        if (item.Name.Contains(text, StringComparison.InvariantCultureIgnoreCase))
                        {
                            category.IsHostingSearchResult = true;
                            item.IsSearchResult = true;
                        }
                        else
                        {
                            item.IsSearchResult = false;
                        }
                    }
                }
            }
        }



        public MainViewModel()
        {
            LoadCommand = new AsyncRelayCommand(LoadAsync);
        }

        private async Task LoadAsync()
        {
            Item prevSelectedItem = null;
            Category prevSelectedCategory = null;

            if (HierarchicalObject.SelectedObject is Category category)
            {
                prevSelectedItem = null;
                prevSelectedCategory = category;
            }
            else if (HierarchicalObject.SelectedObject is Item item)
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
                var categoryOwner = Categories.FirstOrDefault(_ => _.Name == prevSelectedCategory.Name);
                var itemToSelect = categoryOwner?.Items.FirstOrDefault(_ => _.Name == prevSelectedItem.Name);

                if (itemToSelect != null)
                {
                    categoryOwner.IsOpen = true;
                    itemToSelect.IsSelected = true;
                }
            }
            else if (prevSelectedCategory != null)
            {
                var categoryToSelect = Categories.FirstOrDefault(_ => _.Name == prevSelectedCategory.Name);
                if (categoryToSelect != null)
                {
                    categoryToSelect.IsSelected = true;
                }
            }

            if (InSearchMode)
            {
                Search(TextToSearch);
            }
        }
    }
}
