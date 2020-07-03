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

        public int UpdateCount { get; set; } = 0;
        public int RenderCount { get; set; } = 0;

        public override void Render(DrawingContext context)
        {
            PlotModel model = PlotModel;

            if (model != null)
            {
                this.Background = model.Background.ToBrush();
            }

            base.Render(context);

            if (model == null)
            {
                return;
            }

            RenderCount++;
            var ft = new FormattedText();
            ft.Text = $"{RenderCount} / {UpdateCount}";
            ft.Typeface = new Typeface("{$Default}");
            ft.FontSize = 12;
            context.DrawText(OxyColors.Red.ToBrush(), new Point(0, 0), ft);
            var rc = new ImmediateModeRenderContext(context);
            ((IPlotModel)model).Render(rc, this.Bounds.Width, this.Bounds.Height);
        }
    }
}
