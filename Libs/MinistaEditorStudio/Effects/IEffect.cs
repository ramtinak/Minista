using Microsoft.Graphics.Canvas;
namespace MinistaEditorStudio.Effects
{
    public interface IEffect
    {
        double Minimum { get; }
        double Maximum { get; }
        ICanvasImage ApplyEffect(ICanvasImage source, float value);
    }
}
