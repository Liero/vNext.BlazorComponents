﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vNext.BlazorComponents.Data
{
    public class SelectionModel<T>
    {
        private IEqualityComparer<T> _equalityComparer;

        public SelectionModel(IEqualityComparer<T>? equalityComparer = null)
        {
            _equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
            SelectedItems = new HashSet<T>(_equalityComparer);
        }

        public HashSet<T> SelectedItems { get; protected set; }
        public T? SelectedItem { get; set; }
        public int SelectedIndex { get; set; } = -1;

        public IEqualityComparer<T> EqualityComparer
        {
            get => _equalityComparer;
            set
            {
                _equalityComparer = value;
                if (SelectedItems != null)
                {
                    SelectedItems = new HashSet<T>(SelectedItems, value);
                }
            }
        }

        public Func<RangeArgs, Task<IEnumerable<T>>>? GetRange { get; set; }

        public async Task Select(T? item, int? index, bool toggle = false, bool range = false)
        {
            SelectedItems = new HashSet<T>(SelectedItems, EqualityComparer);
            if (toggle && item != null)
            {
                if (SelectedItems.Remove(item))
                {
                    SelectedIndex = -1;
                    SelectedItem = default;
                }
                else
                {
                    SelectedIndex = index ?? -1;
                    SelectedItem = item;
                    SelectedItems.Add(item);
                }
            }
            else if (range && GetRange != null && SelectedItem != null && item != null)
            {
                var args = new RangeArgs(Math.Min(index ?? -1, SelectedIndex), Math.Max(index ?? -1, SelectedIndex), SelectedItem, item);
                var items = await GetRange(args);
                SelectedItems.Clear();
                foreach (var i in items)
                {
                    SelectedItems.Add(i);
                }
            }
            else
            {
                SelectedItems.Clear();
                if (item != null)
                {
                    SelectedItems.Add(item);
                }
                SelectedItem = item;
                SelectedIndex = index ?? -1;
            }
        }
    }
    public record RangeArgs(int From, int To, object Item1, object Item2)
    {
        public int Count => To - From + 1;
    }
}
