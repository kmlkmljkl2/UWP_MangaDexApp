using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.ComponentModel;

namespace UWP_MangaDexApp.Tests
{
    public class FakeClasses
    {
        public class FakeBindDictionary<Tkey, TValue> : Dictionary<Tkey, TValue>
        {
            private GridView View { get; }

            public FakeBindDictionary(GridView view)
            {
                View = view;
            }

            public new void Add(Tkey key, TValue value)
            {
                if (ContainsKey(key))
                {
                    base.Add(key, value);
                    View.Items.Add(key);
                }
                else
                {
                    base[key] = value;
                }
            }

            public new void Remove(Tkey key)
            {
                base.Remove(key);
                View.Items.Remove(key);
            }

            public new void Clear()
            {
                base.Clear();
                View.Items.Clear();
            }
        }

        public class FakeBindList<T> : List<T>
        {
            private GridView GridView { get; }
            private ListView ListView { get; }
            private FlipView FlipView { get; }

            public FakeBindList(GridView view)
            {
                GridView = view;
            }

            public FakeBindList(FlipView view)
            {
                FlipView = view;
            }

            public FakeBindList(ListView view)
            {
                ListView = view;
            }

            public new void Add(T item)
            {
                lock (this)
                {
                    base.Add(item);
                    GridView?.Items.Add(item);
                    ListView?.Items.Add(item);
                    FlipView?.Items.Add(item);
                }
            }

            public new void Remove(T item)
            {
                lock (this)
                {
                    base.Remove(item);
                    GridView?.Items.Remove(item);
                    ListView?.Items.Remove(item);
                    FlipView?.Items.Remove(item);
                }
            }

            public new void Clear()
            {
                base.Clear();
                GridView?.Items.Clear();
                ListView?.Items.Clear();
                FlipView?.Items.Clear();
            }
        }

        public class ObservableString : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public string _data = "";

            public string Data
            {
                get => _data;
                set
                {
                    if (_data != value)
                    {
                        _data = value;
                        OnPropertyChanged();
                    }
                }
            }

            public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
            {
                // Raise the PropertyChanged event, passing the name of the property whose value has changed.
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public override string ToString()
            {
                return Data;
            }
        }

        public class ObservableDouble : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public double _data = 1;

            public double Data
            {
                get => _data;
                set
                {
                    if (_data != value)
                    {
                        _data = value;
                        OnPropertyChanged();
                    }
                }
            }

            public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
            {
                // Raise the PropertyChanged event, passing the name of the property whose value has changed.
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public override string ToString()
            {
                return Data.ToString();
            }
        }
    }
}