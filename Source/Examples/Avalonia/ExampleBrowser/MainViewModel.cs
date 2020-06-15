// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Represents the view-model for the main window.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ExampleBrowser
{
    using ExampleLibrary;
    using OxyPlot;
    using OxyPlot.Series;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    public class Category
    {
        public Category(string key, List<ExampleInfo> examples)
        {
            Key = key;
            Examples = examples ?? throw new ArgumentNullException(nameof(examples));
        }

        public string Key { get; }
        public List<ExampleInfo> Examples { get; }
    }

    /// <summary>
    /// Represents the view-model for the main window.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel" /> class.
        /// </summary>
        public MainViewModel()
        {
            Model = new PlotModel() { Title = "Example Browser", Subtitle = "Select an example from the list" };
            Categories = Examples.GetList()
                .GroupBy(e => e.Category)
                .Select(g => new Category(g.Key, g.ToList()))
                .OrderBy(c => c.Key)
                .ToList();
        }

        public List<Category> Categories { get; }

        private ExampleInfo example;
        /// <summary>
        /// Gets the plot model.
        /// </summary>
        public ExampleInfo Example
        {
            get => example;
            set
            {
                if (example != value)
                {
                    example = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Example)));
                    Model = example?.PlotModel;
                    Model?.InvalidatePlot(true);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Code)));

                    // TODO: update Transpose/Reverse properties when we have access to them
                }
            }
        }

        private PlotModel model;
        /// <summary>
        /// Gets the plot model.
        /// </summary>
        public PlotModel Model
        {
            get => model;
            set
            {
                if (model != value)
                {
                    model = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Model)));
                }
            }
        }

        public string Code
        {
            get => Example?.Code;
        }

        private bool transpose;
        public bool Transpose
        {
            get => transpose;
            set
            {
                if (transpose != value)
                {
                    transpose = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Transpose)));
                }
            }
        }

        private bool canTranspose;
        public bool CanTranspose
        {
            get => canTranspose;
            set
            {
                if (canTranspose != value)
                {
                    canTranspose = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanTranspose)));
                }
            }
        }

        private bool reverse;
        public bool Reverse
        {
            get => reverse;
            set
            {
                if (reverse != value)
                {
                    reverse = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Reverse)));
                }
            }
        }

        private bool canReverse;
        public bool CanReverse
        {
            get => canReverse;
            set
            {
                if (canReverse != value)
                {
                    canReverse = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanReverse)));
                }
            }
        }

        public void ChangeExample(ExampleInfo example)
        {
            Example = example;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}