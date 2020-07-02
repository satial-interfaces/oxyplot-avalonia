using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using OxyPlot;
using OxyPlot.Avalonia.Converters;

namespace OxyPlot.Avalonia
{
    public class ImmediateModePlotControl : UserControl
    {
        public PlotModel PlotModel { get; set; }

        public ImmediateModePlotControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void Render(DrawingContext context)
        {
            if (PlotModel != null)
            {
                this.Background = PlotModel.Background.ToBrush();
            }

            base.Render(context);

            if (PlotModel != null)
            {
                var rc = new ImmediateModeRenderContext(context);
                ((IPlotModel)PlotModel).Render(rc, this.Bounds.Width, this.Bounds.Height);
            }
        }
    }
}
