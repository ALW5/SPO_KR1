using CSharpFunctionalExtensions;
using ProjectOne.Domain.Common;

namespace ProjectOne.Domain.VO;

public record SizeOfStorage
{
    private SizeOfStorage(int width, int height, int depth)
    {
        Width = width;
        Height = height;
        Depth = depth;
    }
    
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Depth { get; private set; }

    public static Result<SizeOfStorage, Error> Create(int width, int height, int depth)
    {
        if (width <= 0)
            return Errors.General.ValueIsInvalid("width");
        if (height <= 0)
            return Errors.General.ValueIsInvalid("height");
        if (depth <= 0)
            return Errors.General.ValueIsInvalid("depth");

        var sizeOfStorage = new SizeOfStorage(
            width,
            height,
            depth);
        
        return sizeOfStorage;
    }
}