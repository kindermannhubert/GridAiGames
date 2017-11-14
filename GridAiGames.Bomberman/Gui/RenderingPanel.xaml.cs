using System.Windows.Controls;
using System.Windows.Media;

namespace GridAiGames.Bomberman.Gui
{
    /// <summary>
    /// Interaction logic for RenderingPanel.xaml
    /// </summary>
    internal partial class RenderingPanel : UserControl
    {
        public WpfRenderer WpfRenderer { get; set; }
        public double GameIteration { get; set; }

        public RenderingPanel()
        {
            InitializeComponent();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            WpfRenderer?.Render(drawingContext, ActualWidth, ActualHeight, GameIteration);
        }
    }
}
